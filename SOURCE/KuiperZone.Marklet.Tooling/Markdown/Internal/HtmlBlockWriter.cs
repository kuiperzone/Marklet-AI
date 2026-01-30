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

namespace KuiperZone.Marklet.Tooling.Markdown.Internal;

internal sealed class HtmlBlockWriter : BlockWriter
{
    private readonly HtmlElementWriter _writer = new();

    public HtmlBlockWriter()
    {
    }

    public HtmlBlockWriter(MarkBlock block)
        : base(block)
    {
        Write();
    }

    public void Write()
    {
        var kind = Block.Kind;
        _writer.Kind = kind;

        WriteLine();

        if (kind == BlockKind.TableCode && Block.Table != null)
        {
            WriteTable(Block.Table, false);
            return;
        }

        if (kind.IsCode())
        {
            WriteCode();
            return;
        }

        if (kind == BlockKind.Rule)
        {
            Buffer.Append("<hr />");
            return;
        }

        WriteDefault();
    }

    private string? GetTag(bool open)
    {
        switch (Block.Kind)
        {
            case BlockKind.Para: return open ? "<p>" : "</p>";
            case BlockKind.H1: return open ? "<h1>" : "</h1>";
            case BlockKind.H2: return open ? "<h2>" : "</h2>";
            case BlockKind.H3: return open ? "<h3>" : "</h3>";
            case BlockKind.H4: return open ? "<h4>" : "</h4>";
            case BlockKind.H5: return open ? "<h5>" : "</h5>";
            case BlockKind.H6: return open ? "<h6>" : "</h6>";
            case BlockKind.FencedCode:
            case BlockKind.IndentedCode: return open ? "<pre><code>" : "</code></pre>";
            case BlockKind.Rule: return open ? "<hr/>" : null;

            // Not valid for this call
            default: throw new ArgumentException($"Invalid {nameof(BlockKind)} = {Block.Kind}");
        }
    }

    private void WriteCode()
    {
        if (Block.Lang != null && Block.Kind == BlockKind.FencedCode)
        {
            Buffer.Append("<pre><code class=\"language-");
            Buffer.Append(Block.Lang);
            Buffer.Append("\">");
        }
        else
        {
            Buffer.Append("<pre><code>");
        }

        foreach (var item in Block.Elements)
        {
            _writer.Element = item;
            _writer.Write(Buffer);
        }

        Buffer.Append("\n</code></pre>");
    }

    private void WriteDefault()
    {
        Buffer.Append(GetTag(true));

        foreach (var item in Block.Elements)
        {
            _writer.Element = item;
            _writer.Write(Buffer);
        }

        Buffer.Append(GetTag(false));
    }

    private void WriteTable(MarkTable table, bool tight)
    {
        Buffer.Append("<table style=\"border-collapse:collapse;\">");

        for (int y = 0; y < table.RowCount; ++y)
        {
            if (!tight && y == 0)
            {
                Buffer.Append("\n<tr style=\"border-style:solid; border-width:0 0 2px 0;\">");
            }
            else
            if (!tight && y < table.RowCount - 1)
            {
                Buffer.Append("\n<tr style=\"border-style:solid; border-width:0 0 1px 0;\">");
            }
            else
            {
                Buffer.Append("\n<tr>");
            }

            for (int x = 0; x < table.ColCount; ++x)
            {
                if (y == 0)
                {
                    Buffer.Append("\n<th style=\"text-align:");
                }
                else
                {
                    Buffer.Append("\n<td style=\"text-align:");
                }

                Buffer.Append(table.Aligns[x].ToString().ToLowerInvariant());
                Buffer.Append(";\">");

                foreach (var item in table.Cell(y, x).Elements)
                {
                    _writer.Element = item;
                    _writer.Write(Buffer);
                }

                if (y == 0)
                {
                    Buffer.Append("</th>");
                }
                else
                {
                    Buffer.Append("</td>");
                }

            }

            Buffer.Append("\n</tr>");
        }

        Buffer.Append("\n</table>");
    }

}
