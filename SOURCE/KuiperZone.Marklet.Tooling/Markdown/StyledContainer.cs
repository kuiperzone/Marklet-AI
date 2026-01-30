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

using KuiperZone.Marklet.Tooling.Markdown.Internal;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <inheritdoc cref="IReadOnlyStyledContainer"/>
public class StyledContainer : IReadOnlyStyledContainer, IEquatable<IReadOnlyStyledContainer>
{
    private const int DefaultCap = 4;
    private static readonly Dictionary<string, InlineStyling> HtmlDictionary = new(16);

    static StyledContainer()
    {
        HtmlDictionary.Add("em", InlineStyling.Emphasis);
        HtmlDictionary.Add("i", InlineStyling.Emphasis);

        HtmlDictionary.Add("strong", InlineStyling.Strong);
        HtmlDictionary.Add("b", InlineStyling.Strong);

        HtmlDictionary.Add("code", InlineStyling.Code);

        HtmlDictionary.Add("samp", InlineStyling.Mono);
        HtmlDictionary.Add("kbd", InlineStyling.Mono);
        HtmlDictionary.Add("var", InlineStyling.Mono);
        HtmlDictionary.Add("tt", InlineStyling.Mono);

        HtmlDictionary.Add("ins", InlineStyling.Underline);
        HtmlDictionary.Add("u", InlineStyling.Underline);

        HtmlDictionary.Add("del", InlineStyling.Strike);
        HtmlDictionary.Add("s", InlineStyling.Strike);
        HtmlDictionary.Add("strike", InlineStyling.Strike);

        HtmlDictionary.Add("sub", InlineStyling.Sub);
        HtmlDictionary.Add("sup", InlineStyling.Sup);

        HtmlDictionary.Add("mark", InlineStyling.Mark);

        // Do not add Math or Keyword
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public StyledContainer()
    {
        Elements = new(DefaultCap);
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    public StyledContainer(IReadOnlyStyledContainer other)
    {
        Elements = new(other.Elements);
    }

    /// <summary>
    /// Constructor with "elements" list which the instance will own.
    /// </summary>
    /// <remarks>
    /// A default list is assigned if "elements" is null.
    /// </remarks>
    public StyledContainer(List<MarkElement>? elements)
    {
        Elements = elements ?? new();
    }

    /// <summary>
    /// Parse using Markdig.
    /// </summary>
    internal StyledContainer(LeafBlock leaf, MarkOptions opts)
    {
        ConditionalDebug.WriteLine(nameof(StyledContainer), $"Leaf constructor");
        Elements = new(DefaultCap);
        AppendMarkdig(leaf, opts);
    }

    /// <summary>
    /// Parse using Markdig.
    /// </summary>
    internal StyledContainer(TableCell? cell, MarkOptions opts)
    {
        ConditionalDebug.WriteLine(nameof(StyledContainer), $"TableCell constructor");
        Elements = new(cell?.Count ?? 0);

        if (cell != null)
        {
            // No images in tables
            opts |= MarkOptions.ImageAsLink;

            foreach (var item in cell)
            {
                ConditionalDebug.WriteLine(nameof(StyledContainer), $"Cell item: {item.GetType().Name}");

                if (item is LeafBlock leaf)
                {
                    AppendMarkdig(leaf, opts);
                }
            }
        }
    }

    /// <summary>
    /// A readonly empty instance.
    /// </summary>
    public static readonly IReadOnlyStyledContainer Empty = new StyledContainer();

    /// <inheritdoc cref="IReadOnlyStyledContainer.Elements"/>
    public List<MarkElement> Elements { get; }

    IReadOnlyList<MarkElement> IReadOnlyStyledContainer.Elements
    {
        get { return Elements; }
    }

    /// <inheritdoc cref="IReadOnlyStyledContainer.IsEmpty"/>
    public bool IsEmpty()
    {
        return IsEmpty(Elements);
    }

    /// <inheritdoc cref="IReadOnlyStyledContainer.GetWidth(bool)"/>
    public int GetWidth(bool graphemes)
    {
        int maxWidth = 0;
        int lineWidth = 0;

        foreach (var item in Elements)
        {
            var text = item.Text;

            int n0 = 0;
            int n1 = text.IndexOf('\n');

            while (n1 > -1)
            {
                lineWidth += graphemes ? text.GetVisualLength(n0, n1 - n0) : text.Length;
                maxWidth = Math.Max(lineWidth, maxWidth);
                lineWidth = 0;

                n0 = n1 + 1;
                n1 = text.IndexOf('\n', n0);
            }

            lineWidth += graphemes ? text.GetVisualLength(n0, n1 - n0) : text.Length;
            maxWidth = Math.Max(lineWidth, maxWidth);
        }

        return maxWidth;
    }

    /// <inheritdoc cref="IReadOnlyStyledContainer.GetWidth()"/>
    public int GetWidth()
    {
        return GetWidth(true);
    }

    /// <inheritdoc cref="IReadOnlyStyledContainer.GetHeight()"/>
    public int GetHeight()
    {
        int height = 1;

        foreach (var item in Elements)
        {
            var s = item.Text;

            int index = s.IndexOf('\n');

            while (index != -1)
            {
                height += 1;
                index = s.IndexOf('\n', index + 1);
            }
        }

        return height;
    }

    /// <inheritdoc cref="IReadOnlyStyledContainer.ToString(TextFormat)"/>
    public virtual string ToString(TextFormat format)
    {
        switch (format)
        {
            case TextFormat.Unicode:
                return new PlainBlockWriter(new MarkBlock(Elements)).ToString();
            case TextFormat.Html:
                return new HtmlBlockWriter(new MarkBlock(Elements)).ToString();
            default:
                return new MarkBlockWriter(new MarkBlock(Elements)).ToString();
        }
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(IReadOnlyStyledContainer? other)
    {
        if (other == this)
        {
            return true;
        }

        if (other == null || Elements.Count != other.Elements.Count)
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
    /// Overrides.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as IReadOnlyStyledContainer);
    }

    /// <inheritdoc cref="IReadOnlyStyledContainer.ToString()"/>
    public sealed override string ToString()
    {
        return ToString(TextFormat.Markdown);
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
    /// Clears <see cref="Elements"/> and resets all properties to default values.
    /// </summary>
    public virtual void Reset()
    {
        Elements.Clear();
        Elements.TrimCapacity();
    }

    /// <summary>
    /// Overridden in subclass.
    /// </summary>
    protected virtual bool IsEmpty(List<MarkElement>? elems)
    {
        if (elems != null && elems.Count != 0)
        {
            foreach (var item in elems)
            {
                if (!item.IsEmpty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    internal void AppendHashCode(HashCode hash)
    {
        foreach (var item in Elements)
        {
            hash.Add(item);
        }
    }

    private void AppendMarkdig(LeafBlock leaf, MarkOptions opts)
    {
        ConditionalDebug.WriteLine(nameof(StyledContainer), $"ProcessInLines: {leaf.ProcessInlines}");

        if (!leaf.ProcessInlines)
        {
            Elements.Add(new(string.Join('\n', leaf.Lines)));
            return;
        }

        AppendMarkdig(leaf.Inline, opts, InlineStyling.Default, null);

        // Clean up
        int n = 0;

        while (n < Elements.Count - 1)
        {
            var e0 = Elements[n];
            var e1 = Elements[n + 1];

            if (n == Elements.Count - 2 && e1.Text != "\n" && string.IsNullOrWhiteSpace(e1.Text))
            {
                // Remove empty
                Elements.RemoveAt(n + 1);
                break;
            }

            if (e0.Styling == e1.Styling && LinkInfo.Equals(e0.Link, e1.Link) && e0.Text != "\n" && e1.Text != "\n")
            {
                // Merge
                Elements[n] = new(string.Concat(e0.Text, e1.Text), e0.Styling, e0.Link);
                Elements.RemoveAt(n + 1);
                continue;
            }

            n += 1;
        }
    }

    private void AppendMarkdig(ContainerInline? container, MarkOptions opts, InlineStyling styling, LinkInfo? info)
    {
        ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(AppendMarkdig)} Entered");

        if (container == null)
        {
            ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(container)} null");
            return;
        }

        bool wantImages = !opts.HasFlag(MarkOptions.ImageAsLink);
        bool ignoreInline = opts.HasFlag(MarkOptions.IgnoreInline);
        bool ignorePlainLinks = opts.HasFlag(MarkOptions.IgnorePlainLinks);
        bool htmlAsCode = opts.HasFlag(MarkOptions.InlineHtmlAsCode) && !ignoreInline;

        bool isHtmlLinkOpen = false;
        List<string>? htmlTags = null;

        foreach (Inline item in container)
        {
            switch (item)
            {
                case LiteralInline literal:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " + nameof(LiteralInline));
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Literal content: {literal.Content}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Literal styling: {styling}");
                    string content = literal.Content.ToString();

                    if (content.Length == 0)
                    {
                        // Not expected
                        ConditionalDebug.WriteLine(nameof(StyledContainer), "Empty");
                        continue;
                    }

                    if (ignoreInline)
                    {
                        ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(ignoreInline)} is true");
                        Elements.Add(new(content, styling, info));
                        continue;
                    }

                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(ignoreInline)} is false");
                    Elements.Add(new(content.Replace("\n", " "), styling, info));
                    continue;
                case EmphasisInline em:
                    var es = em.DelimiterCount == 2 ? InlineStyling.Strong : InlineStyling.Emphasis;
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " + $"{nameof(EmphasisInline)}: {es}");
                    AppendMarkdig(em, opts, styling | es, info);
                    continue;
                case CodeInline code:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " + nameof(CodeInline));
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Code content: {code.Content}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Code styling: {styling}");
                    Elements.Add(new(code.Content, styling | InlineStyling.Code));
                    continue;
                case MathInline math:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " + nameof(MathInline));
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Math content: {math.Content}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Math styling: {styling}");
                    Elements.Add(new(math.Content.ToString(), styling | InlineStyling.Math));
                    continue;
                case LinkInline link:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " + nameof(LinkInline));
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Url: {link.Url}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"IsImage: {link.IsImage}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Title: {link.Title}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Pointy: {link.UrlHasPointyBrackets}");
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"AutoLink: {link.IsAutoLink}");

                    if (link.IsAutoLink)
                    {
                        ConditionalDebug.ThrowIfNull(link.Url);

                        if (!ignorePlainLinks)
                        {
                            ConditionalDebug.WriteLine(nameof(StyledContainer), "Plain text link");
                            Elements.Add(new(link.Url ?? "", styling, new(link.Url)));
                            continue;
                        }

                        ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(ignorePlainLinks)} is true");
                        Elements.Add(new(link.Url ?? "", styling));
                        continue;
                    }

                    AppendMarkdig(link, opts, styling, new LinkInfo(link.Url, link.IsImage && wantImages, link.Title));
                    continue;
                case AutolinkInline autoLink:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " + nameof(AutolinkInline));
                    info = null;
                    Elements.Add(new(autoLink.Url, styling, new(autoLink.Url)));
                    continue;
                case LineBreakInline brk:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), "+ " +nameof(LineBreakInline));

                    if ((ignoreInline || brk.IsHard) && !styling.HasFlag(InlineStyling.Code))
                    {
                        ConditionalDebug.WriteLine(nameof(StyledContainer), "Hard link");
                        Elements.Add(MarkElement.Newline);
                        continue;
                    }

                    ConditionalDebug.WriteLine(nameof(StyledContainer), "Soft link");
                    Elements.Add(new(" ", styling, info));
                    continue;
                case PipeTableDelimiterInline pipe:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"+ {nameof(PipeTableDelimiterInline)} styling: {styling}");
                    Elements.Add(new("|", styling, info));
                    AppendMarkdig(pipe, opts, styling, info);
                    continue;
                case HtmlInline html:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"+ {nameof(HtmlInline)} tag: {html.Tag}");

                    if (ignoreInline)
                    {
                        // The HtmlInline seems to still be firing even if pipe is built with inline parser?
                        ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(ignoreInline)} is true");
                        Elements.Add(new(html.Tag, styling, info));
                        continue;
                    }

                    var tag = MarkTag.TryTag(html.Tag);

                    if (styling.HasFlag(InlineStyling.Code))
                    {
                        // Ensure we skip "</code>" when opened by HTML so it is picked up below
                        if (tag == null || !tag.IsClose || tag.Name != "code" || htmlTags?.Contains("code") != true)
                        {
                            ConditionalDebug.WriteLine(nameof(StyledContainer), $"Verbatim in code: {html.Tag}, {html.IsClosed}");
                            Elements.Add(new(html.Tag, styling, info));
                            continue;
                        }
                    }

                    if (tag == null)
                    {
                        ConditionalDebug.WriteLine(nameof(StyledContainer), "Invalid tag ignored (not expected)");
                        continue;
                    }

                    if (htmlAsCode)
                    {
                        ConditionalDebug.WriteLine(nameof(StyledContainer), $"{nameof(htmlAsCode)} is true");
                        Elements.Add(new(html.Tag, styling | InlineStyling.Code, info));
                        continue;
                    }

                    if (tag.Name == "br" && tag.IsOpen)
                    {
                        ConditionalDebug.WriteLine(nameof(StyledContainer), "Linebreak");
                        Elements.Add(new("\n", styling, info));
                        continue;
                    }

                    if (TryStyleTag(tag, out InlineStyling hs))
                    {
                        if (tag.IsOpen)
                        {
                            if (htmlTags?.Contains(tag.Name) != true)
                            {
                                ConditionalDebug.WriteLine(nameof(StyledContainer), $"Open style: {hs}");
                                styling |= hs;

                                htmlTags ??= new(4);
                                htmlTags.Add(tag.Name);
                                continue;
                            }
                        }
                        else
                        if (tag.IsClose)
                        {
                            if (htmlTags?.Remove(tag.Name) == true)
                            {
                                ConditionalDebug.WriteLine(nameof(StyledContainer), $"Close style: {hs}");
                                styling &= ~hs;
                                continue;
                            }
                        }

                        ConditionalDebug.WriteLine(nameof(StyledContainer), "Style tag ignored");
                        continue;
                    }

                    if (TryLinkTag(tag, wantImages, out LinkInfo? lk))
                    {
                        if (lk != null)
                        {
                            if (info == null && lk.IsImage)
                            {
                                ConditionalDebug.WriteLine(nameof(StyledContainer), "Image tag");
                                Elements.Add(new(tag.GetAttrib("alt") ?? "", styling, lk));
                                continue;
                            }

                            if (info == null && !lk.IsImage && !isHtmlLinkOpen)
                            {
                                ConditionalDebug.WriteLine(nameof(StyledContainer), "Open link tag");
                                info = lk;
                                isHtmlLinkOpen = true;
                                continue;
                            }
                        }
                        else
                        if (isHtmlLinkOpen)
                        {
                            ConditionalDebug.WriteLine(nameof(StyledContainer), "Close link tag");
                            info = null;
                            isHtmlLinkOpen = false;
                            continue;
                        }

                        ConditionalDebug.WriteLine(nameof(StyledContainer), "Link tag ignored");
                        continue;
                    }

                    ConditionalDebug.WriteLine(nameof(StyledContainer), "Unknown tag ignored");
                    continue;
                case HtmlEntityInline entity:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"Entity: {entity.Original}");
                    Elements.Add(new(entity.Transcoded.ToString(), styling, info));
                    continue;
                default:
                    ConditionalDebug.WriteLine(nameof(StyledContainer), $"UNKNOWN: {item.GetType().Name}");
                    continue;

            }
        }
    }

    private static bool TryStyleTag(MarkTag tag, out InlineStyling style)
    {
        style = InlineStyling.Default;

        if (tag == null || (tag.IsOpen && tag.IsClose))
        {
            return false;
        }

        return HtmlDictionary.TryGetValue(tag.Name, out style);
    }

    private static bool TryLinkTag(MarkTag tag, bool wantImages, out LinkInfo? link)
    {
        link = null;

        if (tag.Name == "a")
        {
            if (tag.IsOpen)
            {
                if (tag.IsClose)
                {
                    return false;
                }

                link = new(tag.GetAttrib("href"), tag.GetAttrib("title"));
            }

            return true;
        }

        if (tag.Name == "img")
        {
            if (tag.IsOpen && tag.IsClose)
            {

                link = new(tag.GetAttrib("src"), wantImages, tag.GetAttrib("title"));
                return true;
            }
        }

        return false;
    }
}
