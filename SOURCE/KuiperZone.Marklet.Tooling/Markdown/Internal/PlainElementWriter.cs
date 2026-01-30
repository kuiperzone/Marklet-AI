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

internal sealed class PlainElementWriter
{
    private static readonly MarkElement Empty = new("");

    public PlainElementWriter()
    {
        Element = Empty;
    }

    public PlainElementWriter(MarkElement elem)
    {
        Element = elem;
    }

    public BlockKind Kind { get; set; }
    public MarkElement Element { get; set; }
    public string? QuotePrefix { get; set; }
    public string? ItemPrefix { get; set; }
    public string? Indent { get; set; }

    public override string ToString()
    {
        return Element.Text;
    }

    public void Write(StringBuilder buffer, bool firstItem)
    {
        var split = Element.Text.Split('\n');

        for (int n = 0; n < split.Length; ++n)
        {
            var frag = GetProcessedText(split[n]);

            if (n != 0)
            {
                buffer.Append('\n');
                WritePrefixing(buffer, false, frag.Length == 0, frag);
            }
            else
            if (firstItem)
            {
                WritePrefixing(buffer, true, frag.Length == 0, frag);
            }

            buffer.Append(frag);
        }
    }

    private string GetProcessedText(string frag)
    {
        var s = Element.Styling;

        if (s.HasFlag(InlineStyling.Sup) && !s.HasFlag(InlineStyling.Code))
        {
            return frag.ToSuperscript();
        }

        if (s.HasFlag(InlineStyling.Sub) && !s.HasFlag(InlineStyling.Code))
        {
            return frag.ToSubscript();
        }

        return frag;
    }

    private void WritePrefixing(StringBuilder buffer, bool firstItem, bool empty, string frag)
    {
        var qp = QuotePrefix;
        var ip = ItemPrefix;
        var id = empty ? null : Indent;

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

        buffer.Append(qp);
        buffer.Append(ip);
        buffer.Append(id);
    }

}
