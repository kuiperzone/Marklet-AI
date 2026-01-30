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

internal sealed class MarkBlockWriter : BlockWriter
{
    private readonly MarkElementWriter _writer = new();
    private readonly List<int> _stack = new();

    public MarkBlockWriter()
    {
    }

    public MarkBlockWriter(IReadOnlyMarkBlock block)
        : base(block)
    {
        Write(null);
    }

    public void Write(IReadOnlyMarkBlock? previous)
    {
        var kind = Block.Kind;

        _writer.Kind = kind;
        WriteSpacingLine(previous);

        if (kind == BlockKind.TableCode && Block.Table != null)
        {
            WriteTable(Block.Table);
            return;
        }

        if (kind.IsCode())
        {
            WriteCode();
            return;
        }

        if (kind == BlockKind.Rule)
        {
            Buffer.Append(GetQuotePrefix());
            Buffer.Append("---");
            return;
        }

        WriteDefault();
    }

    private static string? ToAlign(ColAlign align)
    {
        switch (align)
        {
            case ColAlign.Center: return ":---:";
            case ColAlign.Right: return "---:";
            default: return "---";
        }
    }

    private void WriteCode()
    {
        string? ticks = null;
        var quotePrefix = GetQuotePrefix();
        var itemPrefix = GetItemPrefix();

        if (Block.Kind == BlockKind.FencedCode)
        {
            ticks = GetFenceTicks();
            Buffer.Append(quotePrefix);
            Buffer.Append(itemPrefix);
            Buffer.Append(ticks);
            Buffer.Append(Block.Lang);
            Buffer.Append('\n');

            if (itemPrefix != null)
            {
                itemPrefix = new string(' ', itemPrefix.Length);
            }
        }
        else
        if (Block.Kind == BlockKind.MathCode)
        {
            Buffer.Append(quotePrefix);
            Buffer.Append(itemPrefix);
            Buffer.Append("$$\n");
        }
        else
        if (!IsStandalone)
        {
            itemPrefix += Tab;
        }

        var items = Block.Elements;
        _writer.QuotePrefix = quotePrefix;
        _writer.ItemPrefix = itemPrefix;

        for (int n = 0; n < items.Count; ++n)
        {
            _writer.Element = items[n];
            _writer.WritePlain(Buffer, n == 0);
        }


        if (Block.Kind == BlockKind.FencedCode)
        {
            Buffer.Append('\n');
            Buffer.Append(quotePrefix);
            Buffer.Append(itemPrefix);
            Buffer.Append(ticks);
        }
        else
        if (Block.Kind == BlockKind.MathCode)
        {
            Buffer.Append('\n');
            Buffer.Append(quotePrefix);
            Buffer.Append(itemPrefix);
            Buffer.Append("$$");
        }
    }

    private void WriteDefault()
    {
        var items = Block.Elements;
        _writer.QuotePrefix = GetQuotePrefix();
        _writer.ItemPrefix = GetItemPrefix();

        for (int n = 0; n < items.Count; ++n)
        {
            _writer.Element = items[n];
            _writer.Write(Buffer, n == 0);
        }
    }

    private void WriteTable(MarkTable table)
    {
        // Prefixes were intended for a block of text that may contain newlines
        // and must be split. Here with many blocks in the form of table cells.
        string? quotePrefix0 = GetQuotePrefix();

        // Item prefix typical contains list prefix "1." etc., but
        // we will use to include the vertical bar "|" for each cell.
        string? itemPrefix0 = GetItemPrefix();

        // Only first cell (0,0) has original. Others are spacers.
        var spacePrefix = itemPrefix0 != null ? new string(' ', itemPrefix0.Length) + "|" : "|";

        itemPrefix0 += "|";

        for (int y = 0; y < table.RowCount; ++y)
        {
            var qp = quotePrefix0;
            var ip = y == 0 ? itemPrefix0 : spacePrefix;

            if (y != 0)
            {
                Buffer.Append('\n');
            }

            if (y == 1)
            {
                // Alignment
                for (int n = 0; n < table.Aligns.Count; ++n)
                {
                    if (n == 0)
                    {
                        Buffer.Append(qp);
                        Buffer.Append(ip);
                    }
                    else
                    {
                        Buffer.Append('|');
                    }

                    Buffer.Append(ToAlign(table.Aligns[n]));
                }

                Buffer.Append("|\n");
            }

            // Data
            for (int x = 0; x < table.ColCount; ++x)
            {
                var items = table.Cell(y, x).Elements;

                for (int n = 0; n < items.Count; ++n)
                {
                    _writer.QuotePrefix = qp;
                    _writer.ItemPrefix = ip;
                    _writer.Element = items[n];
                    _writer.Write(Buffer, n == 0);
                }

                qp = null;
                ip = "|";
            }

            Buffer.Append('|');
        }
    }

    private void WriteSpacingLine(IReadOnlyMarkBlock? previous)
    {
        if (previous != null)
        {
            if (previous.QuoteLevel > 0)
            {
                WriteLine();
                Buffer.Append(GetQuotePrefix(Math.Min(Block.QuoteLevel, previous.QuoteLevel)));
                WriteLine();
                return;
            }

            WriteLine();
            WriteLine();
        }
    }

    private string? GetItemPrefix()
    {
        string? prefix = GetPaddedListPrefix();

        switch (Block.Kind)
        {
            case BlockKind.H1:
                return prefix + "# ";
            case BlockKind.H2:
                return prefix + "## ";
            case BlockKind.H3:
                return prefix + "### ";
            case BlockKind.H4:
                return prefix + "#### ";
            case BlockKind.H5:
                return prefix + "##### ";
            case BlockKind.H6:
                return prefix + "###### ";
            default:
                return prefix;
        }
    }

    private string? GetPaddedListPrefix()
    {
        int listLevel = Block.ListLevel;

        if (listLevel > 0)
        {
            int last = 0;
            int delta = 0;

            var prefix = Block.GetListPrefix(TextFormat.Markdown);
            int count = Math.Min(listLevel - 1, _stack.Count);

            if (prefix == null)
            {
                count = Math.Min(listLevel, _stack.Count);
            }

            for (int n = 0; n < count; ++n)
            {
                last = _stack[n];
                delta += last;
            }

            while (_stack.Count > count)
            {
                _stack.RemoveAt(_stack.Count - 1);
            }


            int length = prefix?.Length ?? 0;
            _stack.Add(length > 0 ? length + 1 : last); // <- plus 1 for space

            var indent = new string(' ', delta);

            if (prefix != null)
            {
                return string.Concat(indent, prefix, " ");
            }

            return indent;
        }

        _stack.Clear();
        return null;
    }

    private string GetFenceTicks(int count = 3)
    {
        // Ensure ticks not found in code
        var ticks = new string('`', count);

        foreach (var item in Block.Elements)
        {
            if (item.Text.Contains(ticks))
            {
                if (count > 256)
                {
                    // Not expected, but don't hang.
                    return ticks;
                }

                return GetFenceTicks(count + 3);
            }
        }

        return ticks;
    }

}
