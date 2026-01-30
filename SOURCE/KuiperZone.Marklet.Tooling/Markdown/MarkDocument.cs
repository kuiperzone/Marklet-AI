// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// AUTHOR    : Andrew Thomas
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// LICENSE   : AGPL-3.0-only
// -----------------------------------------------------------------------------

// Marklet is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free Software
// Foundation, version 3 of the License only.
//
// Marklet is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along
// with Marklet. If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using KuiperZone.Marklet.Tooling.Markdown.Internal;
using Markdig;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <summary>
/// Markdown content document.
/// </summary>
/// <remarks>
/// This, and associated classes, began life as a standalone markdown parser. After months of unremitting effort,
/// the author finally gave up and adopted "Markdig" behind the scenes. This now serves as an abstraction of Markdig.
/// In effect, it is a "parser-parser" which parses the output of the Markdig parser in order to get things in a
/// state where they can be shunted into visual controls. Who could have guessed Markdown would be so complex!
/// Hats off to the authors of Markdig.
/// </remarks>
public sealed class MarkDocument : IReadOnlyList<IReadOnlyMarkBlock>, IEquatable<MarkDocument>
{
    private const int DefaultCap = 8;
    private static readonly MarkdownPipeline DefaultPipeline;
    private static readonly MarkdownPipeline IgnoreInlinePipeline;
    private static readonly MarkElement NewPara = new("\n\n");

    private readonly List<IReadOnlyMarkBlock> _blocks;
    private string? _markdown;
    private int _asyncVersion;

    static MarkDocument()
    {
        // NB. ConfigureNewLine("\n") - not needed. We do our own writing.
        DefaultPipeline = new MarkdownPipelineBuilder().UseMathematics().UsePipeTables().UseAutoLinks().Build();

        var builder = new MarkdownPipelineBuilder();

        builder.BlockParsers.Clear();
        builder.InlineParsers.Clear();
        builder.Extensions.Clear();

        // Order seems important
        builder.BlockParsers.Add(new FencedCodeBlockParser());
        builder.BlockParsers.Add(new IndentedCodeBlockParser());
        builder.BlockParsers.Add(new ParagraphBlockParser());
        builder.BlockParsers.Add(new MathBlockParser());

        builder.InlineParsers.Add(new AutolinkInlineParser());

        IgnoreInlinePipeline = builder.UseMathematics().UseAutoLinks().Build();
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkDocument()
    {
        _blocks = new(DefaultCap);
    }

    /// <summary>
    /// Parsing constructor.
    /// </summary>
    public MarkDocument(string content, MarkOptions opts = default)
        : this()
    {
        Update(content, opts);
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <remarks>
    /// If "other" is null, constructor behaves as default constructor.
    /// </remarks>
    public MarkDocument(MarkDocument? other)
    {
        if (other != null)
        {
            _blocks = new(other);
            CopyState(other);
            return;
        }

        _blocks = new(DefaultCap);
    }

    /// <summary>
    /// Copy constructor with content sequence.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkDocument"/> instance will own "block" which should not be modified externally. It is assumed
    /// "blocks" are well formed.
    /// </remarks>
    public MarkDocument(List<IReadOnlyMarkBlock> blocks)
    {
        _blocks = blocks;
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/> indexer.
    /// </summary>
    public IReadOnlyMarkBlock this[int index]
    {
        get { return _blocks[index]; }
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public int Count
    {
        get { return _blocks.Count; }
    }

    /// <summary>
    /// Gets whether this instance was created by <see cref="Coalesce"/>.
    /// </summary>
    /// <remarks>
    /// The value is set false when the document is modified.
    /// </remarks>
    public bool IsCoalesced { get; private set; }

    /// <summary>
    /// Sanitizes (NORM-C) the string according the given options.
    /// </summary>
    /// <remarks>
    /// It does nothing except return "str" unchanged if <see cref="MarkOptions.Presan"/> is given.
    /// </remarks>
    [return: NotNullIfNotNull(nameof(str))]
    public static string? Sanitize(string? str, MarkOptions opts, int maxLength = int.MaxValue)
    {
        if (opts.HasFlag(MarkOptions.Presan))
        {
            return str;
        }

        const SanFlags Flags = SanFlags.SubControl | SanFlags.NormC;
        return Sanitizer.Sanitize(str, Flags, maxLength);
    }

    /// <summary>
    /// Overload with default options.
    /// </summary>
    [return: NotNullIfNotNull(nameof(str))]
    public static string? Sanitize(string? str, int maxLength = int.MaxValue)
    {
        return Sanitize(str, default, maxLength);
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public IEnumerator<IReadOnlyMarkBlock> GetEnumerator()
    {
        return _blocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _blocks.GetEnumerator();
    }

    /// <summary>
    /// Clears the document and returns true if the contents are modified.
    /// </summary>
    public bool Clear()
    {
        if (_blocks.Count != 0)
        {
            _blocks.Clear();
            _blocks.TrimCapacity();
            ResetState();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Appends the "document" contents and returns true if modified.
    /// </summary>
    public bool Append(MarkDocument document)
    {
        if (document._blocks.Count != 0)
        {
            ResetState();

            // Shallow copy
            _blocks.AddRange(document);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Appends a deep copy of "blocks" to the document and returns true if modified.
    /// </summary>
    /// <remarks>
    /// Blocks are deep cloned so that post changes to "blocks" will not affect this instance. It is assumed "blocks"
    /// are well formed.
    /// </remarks>
    public bool Append(IEnumerable<IReadOnlyMarkBlock> blocks)
    {
        int count = _blocks.Count;

        foreach (var item in blocks)
        {
            _blocks.Add(new MarkBlock(item, true));
        }

        if (_blocks.Count != count)
        {
            ResetState();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses "markdown" and updates (replaces) the contents and returns true if the contents are modified.
    /// </summary>
    /// <remarks>
    /// The "markdown" input is normalized for C-form. If "markdown" is null or empty, the document will be empty when
    /// this method returns. This method employs an algorithm to optimize performance where "markdown" is a string which
    /// is continuously appended to.
    /// </remarks>
    [return: NotNullIfNotNull(nameof(markdown))]
    public bool Update(string? markdown, MarkOptions opts = default)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return Clear();
        }

        // This should hopefully be fast where we are not normalizing for C-form
        markdown = Sanitize(markdown, opts);

        if (_markdown != markdown)
        {
            Clear();

            // Select pipeline
            var pipeline = opts.HasFlag(MarkOptions.IgnoreInline) ? IgnoreInlinePipeline : DefaultPipeline;

            // Offset store point we can reliably update from when chunking (hopefully)
            AppendMarkdig(Markdig.Markdown.Parse(markdown, pipeline), opts, 0, 0, '\0');
            _markdown = markdown;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Asynchronous variant of <see cref="Update"/>.
    /// </summary>
    public async Task<bool> UpdateAsync(string? markdown, MarkOptions opts = default)
    {
        var version = Interlocked.Increment(ref _asyncVersion);

        if (markdown == null || markdown.Length < 256)
        {
            // Immediate
            return Update(markdown, opts);
        }

        var clone = new MarkDocument(this);
        var task = await Task.Run(() => clone.Update(markdown, opts));

        // Only apply if this is still the latest request.
        if (task && version == Volatile.Read(ref _asyncVersion))
        {
            _blocks.Clear();
            _blocks.AddRange(clone);
            CopyState(clone);
        }

        return task;
    }

    /// <summary>
    /// Combines blocks and elements of matching kinds and styles, returning a new document instance.
    /// </summary>
    /// <remarks>
    /// This method always leaves the source instance unchanged and returns a clone. If <see cref="IsCoalesced"/> is
    /// true on the called instance, however, the result is simply "this" instance. Otherwise, if "accoutrements" is
    /// false, additional content, such as <see cref="MarkBlock.Table"/>, are not copied to the clone. The use case is
    /// to minimise visual controls needed in user interface generation.
    /// </remarks>
    public MarkDocument Coalesce()
    {
        var clone = new List<IReadOnlyMarkBlock>(_blocks);

        if (IsCoalesced)
        {
            return new(clone) { IsCoalesced = true };
        }

        for (int n = 0; n < clone.Count; ++n)
        {
            var coal = clone[n].Coalesce();

            if (coal != null)
            {
                clone[n] = coal;
            }
        }

        int index = -1;
        MarkBlock? work = null;
        IReadOnlyMarkBlock? para = null;

        for (int n = 0; n <= clone.Count; ++n)
        {
            if (n < clone.Count)
            {
                var current = clone[n];

                if (current.Kind == BlockKind.Para && current.GetListKind() == ListKind.None)
                {
                    // Suitable
                    if (para == null)
                    {
                        index = n;
                        work = null;
                        para = current;
                        continue;
                    }

                    if (para.QuoteLevel == current.QuoteLevel)
                    {
                        if (!current.IsEmpty())
                        {
                            if (work == null)
                            {
                                work = new(para, false);
                                para = work;
                                clone[index] = work;
                            }

                            if (!work.IsEmpty())
                            {
                                work.Elements.Add(NewPara);
                            }

                            work.Elements.AddRange(current.Elements);
                        }

                        continue;
                    }
                }
            }

            if (para != null)
            {
                ConditionalDebug.ThrowIfNegative(index);

                index += 1;
                int count = n - index;

                if (count > 0)
                {
                    n = index - 1;
                    clone.RemoveRange(index, count);
                }

                index = -1;
                para = null;
                work = null;
            }
        }

        var doc = new MarkDocument(clone);
        doc.IsCoalesced = true;
        return doc;
    }

    /// <summary>
    /// Writes the document to a string in the given "format".
    /// </summary>
    public string ToString(TextFormat format)
    {
        switch (format)
        {
            case TextFormat.Unicode:
                return new PlainDocumentWriter(this).ToString();
            case TextFormat.Html:
                return new HtmlDocumentWriter(this).ToString();
            default:
                return new MarkDocumentWriter(this).ToString();
        }
    }

    /// <summary>
    /// Equivalent to <see cref="ToString(TextFormat)"/> with <see cref="TextFormat.Markdown"/>.
    /// </summary>
    public override string ToString()
    {
        return ToString(TextFormat.Markdown);
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(MarkDocument? other)
    {
        if (other == this)
        {
            return true;
        }

        if (other == null || _blocks.Count != other._blocks.Count)
        {
            return false;
        }

        for (int n = 0; n < Count; ++n)
        {
            if (!_blocks[n].Equals(other._blocks[n]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MarkDocument);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        // Seems expensive for a large document.
        // This is really only to stop the IDE from nagging.
        var hash = new HashCode();

        foreach (var item in _blocks)
        {
            ((MarkBlock)item).AppendHashCode(hash);
        }

        return hash.ToHashCode();
    }

    private void CopyState(MarkDocument? other)
    {
        if (other != null)
        {
            _markdown = other._markdown;
            IsCoalesced = other.IsCoalesced;
        }
    }

    private void ResetState()
    {
        _markdown = null;
        IsCoalesced = false;
    }

    private void AppendMarkdig(ContainerBlock container, MarkOptions opts, int quoteLevel, int listLevel, char listBullet)
    {
        const string NSpace = nameof(MarkDocument) + "." + nameof(AppendMarkdig);
        ConditionalDebug.WriteLine(NSpace, "ENTERED");

        foreach (var item in container)
        {
            ConditionalDebug.WriteLine(NSpace, $"Source line {item.Line}");
            ConditionalDebug.WriteLine(NSpace, $"quoteLevel: {quoteLevel}, listLevel: {listLevel}");

            MarkBlock? block = null;

            switch (item)
            {
                case ParagraphBlock leaf:
                    ConditionalDebug.WriteLine(NSpace, nameof(ParagraphBlock));
                    block = new(leaf, opts);
                    break;
                case HeadingBlock leaf:
                    ConditionalDebug.WriteLine(NSpace, $"HeadingBlock {leaf.Level}");
                    block = new(leaf, opts);
                    break;
                case ThematicBreakBlock leaf:
                    ConditionalDebug.WriteLine(NSpace, nameof(ThematicBreakBlock));
                    block = new(leaf, opts);
                    break;
                case CodeBlock leaf:
                    // Handles fenced, indented and math
                    ConditionalDebug.WriteLine(NSpace, nameof(CodeBlock));
                    block = new(leaf, opts);
                    break;
                case Table leaf:
                    ConditionalDebug.WriteLine(NSpace, nameof(Table));
                    ConditionalDebug.WriteLine(NSpace, $"IsValid: {leaf.IsValid()}");
                    block = new(leaf, opts);
                    break;
                case HtmlBlock leaf:
                    ConditionalDebug.WriteLine(NSpace, nameof(HtmlBlock));
                    ConditionalDebug.WriteLine(NSpace, $"{leaf.Type}");
                    block = new(leaf, opts);
                    break;
                case QuoteBlock quoteBlock:
                    ConditionalDebug.WriteLine(nameof(MarkDocument), nameof(QuoteBlock));

                    // Don't increment quote level inside lists.
                    // Our data structure can't represent this properly.
                    int ql = listLevel == 0 ? quoteLevel + 1 : quoteLevel;
                    AppendMarkdig(quoteBlock, opts, ql, listLevel, listBullet);
                    continue;
                case ListBlock listBlock:
                    ConditionalDebug.WriteLine(NSpace, nameof(ListBlock));
                    ConditionalDebug.WriteLine(NSpace, $"Count: {listBlock.Count}");
                    ConditionalDebug.WriteLine(NSpace, $"BulletType: {listBlock.BulletType}");
                    ConditionalDebug.WriteLine(NSpace, $"IsOrdered: {listBlock.IsOrdered}");
                    ConditionalDebug.WriteLine(NSpace, $"OrderedStart: {listBlock.OrderedStart}");
                    ConditionalDebug.WriteLine(NSpace, $"DefaultOrderedStart: {listBlock.DefaultOrderedStart}");

                    AppendMarkdig(listBlock, opts, quoteLevel, listLevel + 1, listBlock.BulletType);
                    continue;
                case ListItemBlock listItem:
                    ConditionalDebug.WriteLine(NSpace, nameof(ListItemBlock));
                    ConditionalDebug.WriteLine(NSpace, $"Count: {listItem.Count}");
                    ConditionalDebug.WriteLine(NSpace, $"Order: {listItem.Order}");
                    ConditionalDebug.ThrowIfZero(listLevel);

                    var block0 = _blocks.Count;

                    AppendMarkdig(listItem, opts, quoteLevel, listLevel, listBullet);

                    for (int n = block0; n < _blocks.Count; ++n)
                    {
                        var b = (MarkBlock)_blocks[n];

                        if (b.ListLevel == 0)
                        {
                            b.ListLevel = listLevel;

                            if (n == block0)
                            {
                                b.ListOrder = listItem.Order;
                                b.ListBullet = listBullet;
                            }
                        }
                    }

                    continue;
                default:
                    ConditionalDebug.WriteLine(NSpace, $"UNKNOWN: {item.GetType().Name}");
                    continue;
            }

            if (block != null)
            {
                ConditionalDebug.WriteLine(NSpace, $"CONTENT: {block}");
                ConditionalDebug.WriteLine(NSpace, $"IsOpen: {item.IsOpen}");
                ConditionalDebug.WriteLine(NSpace, $"IsBreakable: {item.IsBreakable}");
                block.QuoteLevel = quoteLevel;
                _blocks.Add(block);
            }
        }
    }

}
