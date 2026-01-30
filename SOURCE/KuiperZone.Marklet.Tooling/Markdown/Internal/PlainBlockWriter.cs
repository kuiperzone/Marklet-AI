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

internal sealed class PlainBlockWriter : BlockWriter
{
    private readonly PlainElementWriter _writer = new();
    private readonly List<int> _stack = new();

    public PlainBlockWriter()
    {
    }

    public PlainBlockWriter(IReadOnlyMarkBlock block)
        : base(block)
    {
        Write(null);
    }

    public void Write(IReadOnlyMarkBlock? previous)
    {
        var kind = Block.Kind;

        _writer.Kind = kind;
        WriteSpacingLine(previous);

        if (kind == BlockKind.TableCode)
        {
            if (Block.Elements.Count != 0)
            {
                // Use pre-generated first
                WriteCode(false);
                return;
            }

            if (Block.Table != null)
            {
                WriteTable(Block.Table);
                return;
            }

            // Fall thru
        }

        if (kind.IsCode())
        {
            WriteCode(kind.IsIndented() || kind == BlockKind.FencedCode);
            return;
        }

        if (kind == BlockKind.Rule)
        {
            Buffer.Append(GetQuotePrefix());
            Buffer.Append("* * *");
            return;
        }

        WriteDefault();
    }

    private void WriteCode(bool indent)
    {
        var items = Block.Elements;
        _writer.QuotePrefix = GetQuotePrefix();
        _writer.ItemPrefix = GetItemPrefix();
        _writer.Indent = indent ? Tab : null;

        for (int n = 0; n < items.Count; ++n)
        {
            _writer.Element = items[n];
            _writer.Write(Buffer, n == 0);
        }
    }

    private void WriteDefault()
    {
        WriteCode(false);
    }

    private void WriteTable(MarkTable table)
    {
        Buffer.Append(table.ToPaddedBlock().ToString(TextFormat.Unicode));
    }

    private void WriteSpacingLine(IReadOnlyMarkBlock? previous)
    {
        if (previous != null)
        {
            if (previous.QuoteLevel > 0)
            {
                Buffer.Append('\n');
                Buffer.Append(GetQuotePrefix(Math.Min(Block.QuoteLevel, previous.QuoteLevel)));
                Buffer.Append('\n');
                return;
            }

            Buffer.Append("\n\n");
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

            var prefix = Block.GetListPrefix(TextFormat.Unicode);
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

}
