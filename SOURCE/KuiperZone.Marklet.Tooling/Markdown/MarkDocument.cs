// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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
    private static readonly MarkdownPipeline NoInlinePipeline;
    private static readonly MarkElement NewPara = new("\n\n");

    private readonly List<IReadOnlyMarkBlock> _blocks;

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

        NoInlinePipeline = builder.UseMathematics().UseAutoLinks().Build();
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
    public MarkDocument(string? markdown, MarkOptions opts = MarkOptions.Markdown | MarkOptions.Sanitize)
    {
        _blocks = new(DefaultCap);

        if (opts.HasFlag(MarkOptions.Sanitize))
        {
            const SanFlags Flags = SanFlags.SubControl | SanFlags.NormC | SanFlags.Trim;
            markdown = Sanitizer.Sanitize(markdown, Flags);
        }

        if (!string.IsNullOrEmpty(markdown))
        {
            if (!opts.HasFlag(MarkOptions.Blocks))
            {
                // Plain text
                var e = new MarkElement(markdown);
                var list = new List<MarkElement>();
                list.Add(e);
                _blocks.Add(new MarkBlock(list));
                return;
            }

            // Select pipeline
            var pipeline = opts.HasFlag(MarkOptions.Inlines) ? DefaultPipeline : NoInlinePipeline;

            // Offset store point we can reliably update from when chunking (hopefully)
            AppendMarkdig(Markdig.Markdown.Parse(markdown, pipeline), opts, 0, 0, '\0');

            if (opts.HasFlag(MarkOptions.Coalesce))
            {
                Coalesce();
            }
        }
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
    /// Overload of <see cref="Search(string?, SearchFlags, out int)"/>.
    /// </summary>
    public MarkDocument? Search(string? keyword, SearchFlags flags)
    {
        return Search(keyword, flags, out _);
    }

    /// <summary>
    /// Searches the content for "keyword" and returns a new <see cref="MarkDocument"/> instance if found.
    /// </summary>
    /// <remarks>
    /// If any occurrences of "keyword" are found, the call returns a new <see cref="MarkDocument"/> instance with
    /// occurrences highlighted using <see cref="InlineStyling.Keyword"/>. If no occurrences are found, the result is
    /// null. The source <see cref="MarkDocument"/> instance is not modified.
    /// </remarks>
    public MarkDocument? Search(string? keyword, SearchFlags flags, out int counter)
    {
        counter = 0;
        int sublen = keyword?.Length ?? 0;

        if (sublen == 0)
        {
            return null;
        }

        // Block count does not change, although blocks may.
        int blockCount = _blocks.Count;
        List<IReadOnlyMarkBlock>? clone = null;

        for (int bN = 0; bN < blockCount; ++bN)
        {
            var block0 = _blocks[bN];
            var elems0 = block0.Elements;

            MarkBlock? block1 = null;

            for (int eN = 0; eN < elems0.Count; ++eN)
            {
                MarkElement? elem = elems0[eN];
                int index = elem.Text.Search(keyword, flags);

                if (index < 0)
                {
                    continue;
                }

                clone ??= new(_blocks);

                if (block1 == null)
                {
                    block1 = new MarkBlock(block0, false);
                    clone[bN] = block1;
                }

                elems0 = block1.Elements;
                var elems1 = block1.Elements;

                while (index > -1)
                {
                    counter += 1;
                    var split = elem.Split(index, sublen, InlineStyling.Keyword);
                    Diag.ThrowIfZero(split.Length);

                    elems1[eN] = split[0];

                    for (int n = 1; n < split.Length; ++n)
                    {
                        elems1.Insert(++eN, split[n]);
                    }

                    elem = split[^1];

                    if (elem.Styling != InlineStyling.Keyword)
                    {

                        index = elem.Text.Search(keyword, flags);
                        continue;
                    }

                    break;
                }
            }
        }

        if (clone != null)
        {
            return new(clone);
        }

        return null;
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

    private void Coalesce()
    {
        var local = _blocks;

        for (int n = 0; n < local.Count; ++n)
        {
            var coal = local[n].Coalesce();

            if (coal != null)
            {
                local[n] = coal;
            }
        }

        int index = -1;
        MarkBlock? work = null;
        IReadOnlyMarkBlock? para = null;

        for (int n = 0; n <= local.Count; ++n)
        {
            if (n < local.Count)
            {
                var current = local[n];

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
                                local[index] = work;
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
                Diag.ThrowIfNegative(index);

                index += 1;
                int c = n - index;

                if (c > 0)
                {
                    n = index - 1;
                    local.RemoveRange(index, c);
                }

                index = -1;
                para = null;
                work = null;
            }
        }
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

    private void AppendMarkdig(ContainerBlock container, MarkOptions opts, int quoteLevel, int listLevel, char listBullet)
    {
        const string NSpace = nameof(MarkDocument) + "." + nameof(AppendMarkdig);
        Diag.WriteLine(NSpace, "ENTERED");

        foreach (var item in container)
        {
            Diag.WriteLine(NSpace, $"Source line {item.Line}");
            Diag.WriteLine(NSpace, $"quoteLevel: {quoteLevel}, listLevel: {listLevel}");

            MarkBlock? block = null;

            switch (item)
            {
                case ParagraphBlock leaf:
                    Diag.WriteLine(NSpace, nameof(ParagraphBlock));
                    block = new(leaf, opts);
                    break;
                case HeadingBlock leaf:
                    Diag.WriteLine(NSpace, $"HeadingBlock {leaf.Level}");
                    block = new(leaf, opts);
                    break;
                case ThematicBreakBlock leaf:
                    Diag.WriteLine(NSpace, nameof(ThematicBreakBlock));
                    block = new(leaf, opts);
                    break;
                case CodeBlock leaf:
                    // Handles fenced, indented and math
                    Diag.WriteLine(NSpace, nameof(CodeBlock));
                    block = new(leaf, opts);
                    break;
                case Table leaf:
                    Diag.WriteLine(NSpace, nameof(Table));
                    Diag.WriteLine(NSpace, $"IsValid: {leaf.IsValid()}");
                    block = new(leaf, opts);
                    break;
                case HtmlBlock leaf:
                    Diag.WriteLine(NSpace, nameof(HtmlBlock));
                    Diag.WriteLine(NSpace, $"{leaf.Type}");
                    block = new(leaf, opts);
                    break;
                case QuoteBlock quoteBlock:
                    Diag.WriteLine(nameof(MarkDocument), nameof(QuoteBlock));

                    // Don't increment quote level inside lists.
                    // Our data structure can't represent this properly.
                    int ql = listLevel == 0 ? quoteLevel + 1 : quoteLevel;
                    AppendMarkdig(quoteBlock, opts, ql, listLevel, listBullet);
                    continue;
                case ListBlock listBlock:
                    Diag.WriteLine(NSpace, nameof(ListBlock));
                    Diag.WriteLine(NSpace, $"Count: {listBlock.Count}");
                    Diag.WriteLine(NSpace, $"BulletType: {listBlock.BulletType}");
                    Diag.WriteLine(NSpace, $"IsOrdered: {listBlock.IsOrdered}");
                    Diag.WriteLine(NSpace, $"OrderedStart: {listBlock.OrderedStart}");
                    Diag.WriteLine(NSpace, $"DefaultOrderedStart: {listBlock.DefaultOrderedStart}");

                    AppendMarkdig(listBlock, opts, quoteLevel, listLevel + 1, listBlock.BulletType);
                    continue;
                case ListItemBlock listItem:
                    Diag.WriteLine(NSpace, nameof(ListItemBlock));
                    Diag.WriteLine(NSpace, $"Count: {listItem.Count}");
                    Diag.WriteLine(NSpace, $"Order: {listItem.Order}");
                    Diag.ThrowIfZero(listLevel);

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
                    Diag.WriteLine(NSpace, $"UNKNOWN: {item.GetType().Name}");
                    continue;
            }

            if (block != null)
            {
                Diag.WriteLine(NSpace, $"CONTENT: {block}");
                Diag.WriteLine(NSpace, $"IsOpen: {item.IsOpen}");
                Diag.WriteLine(NSpace, $"IsBreakable: {item.IsBreakable}");
                block.QuoteLevel = quoteLevel;
                _blocks.Add(block);
            }
        }
    }

}
