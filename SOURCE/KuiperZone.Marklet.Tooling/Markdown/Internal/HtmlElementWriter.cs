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

using System.Net;
using System.Text;

namespace KuiperZone.Marklet.Tooling.Markdown.Internal;

internal sealed class HtmlElementWriter
{
    public const string BreakTag = "<br />";

    private static readonly MarkElement Empty = new("");
    private static readonly InlineStyling[] StyleValues = Enum.GetValues<InlineStyling>();

    public HtmlElementWriter()
    {
        Element = Empty;
    }

    public HtmlElementWriter(MarkElement elem)
    {
        Element = elem;
    }

    public BlockKind Kind { get; set; }
    public MarkElement Element { get; set; }

    public override string ToString()
    {
        var buffer = new StringBuilder(Element.Text.Length);
        Write(buffer);
        return buffer.ToString();
    }

    public void Write(StringBuilder buffer)
    {
        var link = Element.Link;
        var eStyling = Element.Styling;

        if (eStyling == InlineStyling.Default || Kind.IsCode())
        {
            if (link != null)
            {
                WriteLink(buffer, link, Element.Text);
                return;
            }

            buffer.Append(EncodeText(Kind, Element.Text));
            return;
        }

        // We have to split out styling
        var endTags = new List<string>(4);

        foreach (var style in StyleValues)
        {
            if (style != InlineStyling.Default && eStyling.HasFlag(style))
            {
                var tag = style.ToHtml();

                if (tag != null)
                {
                    tag += ">";
                    endTags.Add("</" + tag);

                    buffer.Append('<');
                    buffer.Append(tag);
                }
            }
        }

        if (link != null)
        {
            WriteLink(buffer, link, Element.Text);
        }
        else
        {
            buffer.Append(EncodeText(Kind, Element.Text));
        }

        if (endTags != null)
        {
            endTags.Reverse();
            foreach (var tag in endTags)
            {
                buffer.Append(tag);
            }
        }
    }

    private static void WriteLink(StringBuilder buffer, LinkInfo link, string? text)
    {
        if (link.IsImage)
        {
            WriteLinkImage(buffer, link, text);
            return;
        }

        buffer.Append("<a href=\"");
        buffer.Append(WebUtility.HtmlEncode(link.ToString())?.Replace("\\", "%5C"));

        if (link.Title != null)
        {
            buffer.Append("\" title=\"");
            buffer.Append(EncodeText(BlockKind.Para, link.Title, false));
        }

        buffer.Append("\">");

        // Assume we are not using links in pre or code
        buffer.Append(EncodeText(BlockKind.Para, text ?? "", true));

        buffer.Append("</a>");
    }

    private static void WriteLinkImage(StringBuilder buffer, LinkInfo link, string? text)
    {
        text ??= link.Title;

        buffer.Append("<img src=\"");
        buffer.Append(WebUtility.HtmlEncode(link.ToString())?.Replace("\\", "%5C"));

        if (!string.IsNullOrEmpty(text))
        {
            buffer.Append("\" alt=\"");
            buffer.Append(EncodeText(BlockKind.Para, text, false));
        }

        if (link.Title != null)
        {
            buffer.Append("\" title=\"");
            buffer.Append(EncodeText(BlockKind.Para, link.Title, false));
        }

        buffer.Append("\" />");
    }

    public static string EncodeText(BlockKind kind, string text, bool preserveBreak = true)
    {
        const int CapOffset = 16;

        int n0 = 0;
        StringBuilder? buffer = null;

        for (int n = 0; n < text.Length; ++n)
        {
            char c = text[n];

            switch (c)
            {
                case '\n':
                    if (!kind.IsCode())
                    {
                        buffer ??= new(text.Length + CapOffset);
                        buffer.Append(text.AsSpan(n0, n - n0));
                        buffer.Append(preserveBreak ? BreakTag : " ");
                        n0 = n + 1;
                    }
                    continue;
                case '&':
                case '<':
                case '>':
                case '"':
                    buffer ??= new(text.Length + CapOffset);
                    buffer.Append(text.AsSpan(n0, n - n0));
                    buffer.Append(WebUtility.HtmlEncode(c.ToString()));
                    n0 = n + 1;
                    continue;
                default:
                    if (c < ' ' && c != '\t')
                    {
                        // Omit it.
                        // Assume <pre> for tab.
                        buffer ??= new(text.Length + CapOffset);
                        buffer.Append(text.AsSpan(n0, n - n0));
                        n0 = n + 1;
                    }
                    continue;
            }
        }

        if (buffer != null)
        {
            if (n0 < text.Length)
            {
                buffer.Append(text.AsSpan(n0));
            }

            return buffer.ToString();
        }

        return text;
    }

}
