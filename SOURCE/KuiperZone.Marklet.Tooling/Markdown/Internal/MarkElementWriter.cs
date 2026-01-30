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

using System.Text;

namespace KuiperZone.Marklet.Tooling.Markdown.Internal;

internal sealed class MarkElementWriter
{
    private static readonly MarkElement Empty = new("");
    private static readonly InlineStyling[] StyleValues = Enum.GetValues<InlineStyling>();

    public MarkElementWriter()
    {
        Element = Empty;
    }

    public MarkElementWriter(MarkElement elem)
    {
        Element = elem;
    }

    public BlockKind Kind { get; set; }
    public MarkElement Element { get; set; }
    public string? QuotePrefix { get; set; }
    public string? ItemPrefix { get; set; }

    public override string ToString()
    {
        var buffer = new StringBuilder(Element.Text.Length);
        Write(buffer, true);
        return buffer.ToString();
    }

    public void Write(StringBuilder buffer, bool firstItem)
    {
        if (Kind.IsCode(false))
        {
            WritePlain(buffer, firstItem);
            return;
        }

        bool empty = Element.IsEmpty;

        if (firstItem)
        {
            WriteIndent(buffer, firstItem, empty);
        }

        if (empty)
        {
            return;
        }

        var link = Element.Link;
        var eStyling = Element.Styling;
        bool breakTag = Kind.IsCode(true);

        if (link?.Uri != null)
        {
            buffer.Append(link.IsImage ? "![" : "[");
        }

        if (eStyling != InlineStyling.Default)
        {
            var endTags = new List<string>(4);

            foreach (var style in StyleValues)
            {
                if (style != InlineStyling.Default && eStyling.HasFlag(style))
                {
                    var tag = style.ToMark();

                    if (tag != null)
                    {
                        buffer.Append(tag);
                        endTags.Add(tag);
                    }
                    else
                    {
                        tag = style.ToHtml();

                        if (tag != null)
                        {
                            // Fall to html
                            tag = style.ToHtml() + ">";
                            endTags.Add("</" + tag);

                            buffer.Append('<');
                            buffer.Append(tag);
                        }
                    }
                }
            }

            WriteSplit(buffer, breakTag);

            endTags.Reverse();

            foreach (var tag in endTags)
            {
                buffer.Append(tag);
            }
        }
        else
        {
            WriteSplit(buffer, breakTag);
        }

        if (link?.Uri != null)
        {
            buffer.Append("](");
            buffer.Append(link);

            if (link.Title != null)
            {
                buffer.Append(" \"");
                buffer.Append(link.Title);
                buffer.Append('"');
            }

            buffer.Append(')');
        }
    }

    public void WritePlain(StringBuilder buffer, bool firstItem)
    {
        var split = Element.Text.Split('\n');

        for (int n = 0; n < split.Length; ++n)
        {
            var frag = split[n];

            if (n != 0)
            {
                buffer.Append('\n');
                WriteIndent(buffer, false, frag.Length == 0);
            }
            else
            if (firstItem)
            {
                WriteIndent(buffer, true, frag.Length == 0);
            }

            buffer.Append(frag);
        }
    }

    private void WriteIndent(StringBuilder buffer, bool firstItem, bool empty)
    {
        var qp = QuotePrefix;
        var ip = ItemPrefix;

        if (qp == null && ip == null)
        {
            return;
        }

        if (ip != null)
        {
            if (!firstItem)
            {
                ip = empty ? null : new string(' ', ip.Length);
            }
            else
            if (empty)
            {
                ip = ip.TrimEnd();
            }
        }


        if (empty && qp != null && string.IsNullOrEmpty(ip))
        {
            qp = qp.TrimEnd();
        }

        buffer.Append(qp);
        buffer.Append(ip);
    }

    private void WriteSplit(StringBuilder buffer, bool breakTag)
    {
        if (breakTag)
        {
            buffer.Append(Element.Text.Replace("\n", HtmlElementWriter.BreakTag));
            return;
        }

        var split = Element.Text.Split('\n');

        for (int n = 0; n < split.Length; ++n)
        {
            var frag = split[n];

            if (n != 0)
            {
                buffer.Append("\\\n");
                WriteIndent(buffer, false, frag.Length == 0);
            }

            buffer.Append(frag);
        }
    }
}
