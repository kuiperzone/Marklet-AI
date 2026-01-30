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

using System.Globalization;
using KuiperZone.Marklet.Tooling.Markdown.Internal;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Syntax;

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <inheritdoc cref="IReadOnlyMarkBlock"/>
public sealed class MarkBlock : StyledContainer,
    IReadOnlyMarkBlock, IReadOnlyStyledContainer,
    IEquatable<IReadOnlyStyledContainer>, IEquatable<IReadOnlyMarkBlock>
{
    /// <summary>
    /// Maximum value of <see cref="QuoteLevel"/>.
    /// </summary>
    public const int MaxQuoteLevel = 9;

    /// <summary>
    /// Maximum value of <see cref="ListLevel"/>.
    /// </summary>
    public const int MaxListLevel = 6;

    /// <summary>
    /// Maximum <see cref="Lang"/> length.
    /// </summary>
    public const int MaxLang = 48;

    private int _quoteLevel;
    private int _listLevel;
    private int _listOrder;
    private char _listBullet;
    private string? _lang;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkBlock()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public MarkBlock(BlockKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// Copy constructor (deep copy).
    /// </summary>
    /// <remarks>
    /// If "accoutrements" is true, additional content, such as <see cref="Table"/>, are copied.
    /// </remarks>
    public MarkBlock(IReadOnlyMarkBlock other, bool accoutrements)
        : base(other)
    {
        Kind = other.Kind;
        _lang = other.Lang;
        _quoteLevel = other.QuoteLevel;
        _listLevel = other.ListLevel;
        _listOrder = other.ListOrder;
        _listBullet = other.ListBullet;
        _lang = other.Lang;

        if (accoutrements && other.Table != null)
        {
            Table = new(other.Table);
        }
    }

    /// <summary>
    /// Constructor with "elements" list which the instance will own.
    /// </summary>
    /// <remarks>
    /// A default list is assigned if "elements" is null.
    /// </remarks>
    public MarkBlock(List<MarkElement>? elements)
        : base(elements)
    {
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(ParagraphBlock para, MarkOptions opts)
        : base(para, opts)
    {
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(HeadingBlock heading, MarkOptions opts)
        : base(heading, opts)
    {
        switch (heading.Level)
        {
            case 1:
                Kind = BlockKind.H1;
                break;
            case 2:
                Kind = BlockKind.H2;
                break;
            case 3:
                Kind = BlockKind.H3;
                break;
            case 4:
                Kind = BlockKind.H4;
                break;
            case 5:
                Kind = BlockKind.H5;
                break;
            case 6:
                Kind = BlockKind.H6;
                break;
        }
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(ThematicBreakBlock rule, MarkOptions opts)
        : base(rule, opts)
    {
        Kind = BlockKind.Rule;
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(CodeBlock code, MarkOptions opts)
        : base(code, opts)
    {
        if (code is MathBlock)
        {
            // MathBlock before FencedCodeBlock
            Kind = BlockKind.MathCode;
            return;
        }

        if (code is FencedCodeBlock fb)
        {
            Kind = BlockKind.FencedCode;
            Lang = fb.Info;
            return;
        }

        Kind = BlockKind.IndentedCode;
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(HtmlBlock code, MarkOptions opts)
        : base(code, opts)
    {
        Kind = BlockKind.IndentedCode;
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(MathBlock math, MarkOptions opts)
        : base(math, opts)
    {
        Kind = BlockKind.MathCode;
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkBlock(Table table, MarkOptions opts)
    {
        Kind = BlockKind.TableCode;
        Table = new MarkTable(table, opts);
        Elements.AddRange(Table.ToPaddedBlock().Elements);
    }

    /// <summary>
    /// A readonly empty instance.
    /// </summary>
    public new static readonly IReadOnlyMarkBlock Empty = new MarkBlock();

    /// <inheritdoc cref="IReadOnlyMarkBlock.Kind"/>
    public BlockKind Kind { get; set; }

    /// <inheritdoc cref="IReadOnlyMarkBlock.QuoteLevel"/>
    public int QuoteLevel
    {
        get { return _quoteLevel; }
        set { _quoteLevel = Math.Clamp(value, 0, MaxQuoteLevel); }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.ListLevel"/>
    public int ListLevel
    {
        get { return _listLevel; }
        set { _listLevel = Math.Clamp(value, 0, MaxListLevel); }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.ListOrder"/>
    public int ListOrder
    {
        get { return _listOrder; }
        set { _listOrder = Math.Clamp(value, 0, 999999999); }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.ListBullet"/>
    public char ListBullet
    {
        get { return _listBullet; }

        set
        {
            if (value == '*' || value == '+' || value == '-')
            {
                _listBullet = value;
            }
            else
            {
                _listBullet = '\0';
            }
        }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.Lang"/>
    public string? Lang
    {
        get { return _lang; }

        set
        {
            value = value?.Trim();

            if (string.IsNullOrEmpty(value) || value.Contains("```") || value.Contains("~~~"))
            {
                _lang = null;
                return;
            }

            if (value.Length > MaxLang)
            {
                _lang = value.Substring(0, MaxLang);
                return;
            }

            _lang = value;
        }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.Table"/>
    public MarkTable? Table { get; set; }

    /// <inheritdoc cref="StyledContainer.Reset"/>
    public override void Reset()
    {
        base.Reset();
        Kind = BlockKind.Para;
        _lang = null;
        _quoteLevel = 0;
        _listLevel = 0;
        _listOrder = 0;
        _listBullet = '\0';
        Table = null;
    }

    /// <inheritdoc cref="StyledContainer.ToString(TextFormat)"/>
    public override string ToString(TextFormat format)
    {
        switch (format)
        {
            case TextFormat.Unicode:
                return new PlainBlockWriter(this).ToString();
            case TextFormat.Html:
                return new HtmlBlockWriter(this).ToString();
            default:
                return new MarkBlockWriter(this).ToString();
        }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.GetQuotePrefix"/>
    public string? GetQuotePrefix()
    {
        return BlockWriter.GetQuotePrefix(_quoteLevel);
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.GetListKind"/>
    public ListKind GetListKind()
    {
        if (_listLevel == 0)
        {
            return ListKind.None;
        }

        if (_listOrder != 0)
        {
            return ListKind.Ordered;
        }

        if (_listBullet != '\0')
        {
            return ListKind.Bulleted;
        }

        return ListKind.Continuation;
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.GetListPrefix"/>
    public string? GetListPrefix(TextFormat format = TextFormat.Unicode)
    {
        var listKind = GetListKind();

        if (listKind == ListKind.None || listKind == ListKind.Continuation || format == TextFormat.Html)
        {
            return null;
        }

        if (listKind == ListKind.Ordered)
        {
            ConditionalDebug.ThrowIfZero(_listOrder);
            return _listOrder.ToString(CultureInfo.InvariantCulture) + ".";
        }

        ConditionalDebug.ThrowIfZero(_listBullet);

        if (format == TextFormat.Markdown)
        {
            return _listBullet.ToString();
        }

        switch (_listLevel)
        {
            case 1: return "\u2022"; // <-  filled bullet
            case 2: return "\u25E6"; // <-  unfilled bullet
            default: return "\u25AA"; // <- square bullet
        }
    }

    /// <inheritdoc cref="IReadOnlyMarkBlock.Coalesce"/>
    public MarkBlock? Coalesce()
    {
        if (Elements.Count != 0)
        {
            MarkBlock? change = null;

            // Need to trim last due to "<br>"
            string text;
            int x = Elements.Count - 1;
            MarkElement? elem = Elements[x];

            if (Kind.IsCode())
            {
                text = elem.Text.TrimSpaceEnd();
            }
            else
            {
                text = elem.Text.TrimEnd();
            }

            if (text.Length != elem.Text.Length)
            {
                change = new(this, false);
                change.Elements[x] = new(text, elem);
            }

            int n = 0;
            List<MarkElement> working = change?.Elements ?? Elements;

            elem = null;

            while (n < working.Count)
            {
                var item = working[n];

                if (item.IsEmpty)
                {
                    change ??= new(this, false);
                    working = change.Elements;

                    working.RemoveAt(n);
                    continue;
                }

                if (elem != null && elem.Styling == item.Styling && LinkInfo.Equals(elem.Link, item.Link))
                {
                    change ??= new(this, false);
                    working = change.Elements;

                    elem = new(string.Concat(elem.Text, item.Text), item);
                    working[n - 1] = elem;

                    working.RemoveAt(n);
                    continue;
                }

                n += 1;
                elem = item;
            }

            return change;
        }

        if (Kind == BlockKind.TableCode && Table != null)
        {
            // Populate
            return Table.ToPaddedBlock();
        }

        return null;
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(IReadOnlyMarkBlock? other)
    {
        if (other == this)
        {
            return true;
        }

        if (other == null || Kind != other.Kind ||
            Elements.Count != other.Elements.Count ||
            _quoteLevel != other.QuoteLevel || _listLevel != other.ListLevel ||
            _listOrder != other.ListOrder || _listBullet != other.ListBullet ||
            _lang != other.Lang)
        {
            return false;
        }

        for (int n = 0; n < Elements.Count; ++n)
        {
            if (!Elements[n].Equals(other.Elements[n]))
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
        return Equals(obj as IReadOnlyMarkBlock);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        AppendHashCode(hash);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override bool IsEmpty(List<MarkElement>? elements)
    {
        if (Table != null || Kind != BlockKind.Para ||
            (_listLevel > 0 && (_listOrder != 0 || _listBullet != '\0')))
        {
            return false;
        }

        return base.IsEmpty(elements);
    }

    internal new void AppendHashCode(HashCode hash)
    {
        hash.Add(Kind);
        hash.Add(_quoteLevel);
        hash.Add(_listLevel);
        hash.Add(_listOrder);
        hash.Add(_listBullet);
        hash.Add(_lang);

        foreach (var item in Elements)
        {
            hash.Add(item.GetHashCode());
        }
    }

}
