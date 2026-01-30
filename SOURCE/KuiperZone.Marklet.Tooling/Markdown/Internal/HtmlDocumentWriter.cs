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
using System.Text;

namespace KuiperZone.Marklet.Tooling.Markdown.Internal;

internal sealed class HtmlDocumentWriter
{
    private static readonly IReadOnlyMarkBlock EmptyBlock = BlockWriter.Empty;
    private readonly HtmlBlockWriter _blocker = new();
    private readonly StringBuilder _buffer;

    private List<string> _listStack = new();

    public HtmlDocumentWriter(MarkDocument document)
    {
        Document = document;
        _buffer = _blocker.Buffer;
    }

    public MarkDocument Document { get; }

    public override string ToString()
    {
        var previous = EmptyBlock;

        foreach (var item in Document)
        {
            _blocker.Block = item;

            HandleStack(item, previous);
            previous = item;

            _blocker.Write();
        }

        HandleStack(EmptyBlock, previous);
        return _blocker.Buffer.ToString();
    }

    private void HandleStack(IReadOnlyMarkBlock block0, IReadOnlyMarkBlock block1)
    {
        // block0 is new block, block1 is previous
        int delta = block0.QuoteLevel - block1.QuoteLevel;

        if (delta != 0)
        {
            block1 = EmptyBlock;
            UnwindListStack(_listStack!.Count);
            WindQuoteBlock(delta);
            UnwindQuoteBlock(-delta);
        }

        int level0 = block0.ListLevel;
        var kind0 = block0.GetListKind();
        bool ord0 = kind0 == ListKind.Ordered;
        char ufx0 = !ord0 ? block0.ListBullet : '\0';

        int level1 = block1.ListLevel;
        bool ord1 = block1.GetListKind() == ListKind.Ordered;
        char ufx1 = !ord1 ? block1.ListBullet : '\0';

        // Forced order change
        bool forceOrder = ord0 && ord1 && block0.ListOrder != block1.ListOrder + 1;

        if (level0 > 0 && level1 > 0)
        {
            string? ofx0 = ord0 ? block0.ListOrder.ToString(CultureInfo.InvariantCulture) : null;

            if (level0 > level1)
            {
                WindListStack(ofx0, level0 - level1);
            }
            else
            if (level0 < level1 || forceOrder)
            {
                UnwindListStack(level1 - level0 + 1);
                WindListStack(ofx0, 1);
            }
            else
            if (kind0.IsHead())
            {
                if (ord0 != ord1 || ufx0 != ufx1 || forceOrder)
                {
                    UnwindListStack(1);
                    WindListStack(ofx0, 1);
                }
                else
                {
                    _buffer.Append("\n</li>");
                    WriteLine();
                    _buffer.Append("<li>");
                }
            }
        }
        else
        if (level0 > 0 || forceOrder)
        {
            string? ofx0 = ord0 ? block0.ListOrder.ToString(CultureInfo.InvariantCulture) : null;
            WindListStack(ofx0, level0);
        }
        else
        if (level0 == 0)
        {
            UnwindListStack(_listStack!.Count);
        }
    }

    private void WindListStack(string? ordered, int count)
    {
        string tag0 = "<ul>";
        string tag1 = "<ul>";
        string tagX = "\n</ul>";

        if (ordered != null)
        {
            tag0 = "<ol>";
            tag1 = $"<ol start=\"{ordered}\">";
            tagX = "\n</ol>";
        }

        for (int n = 0; n < count; ++n)
        {
            WriteLine();
            _buffer.Append(n == count - 1 ? tag1 : tag0);
            _listStack!.Add(tagX);

            _buffer.Append("\n<li>");
        }
    }

    private void UnwindListStack(int count)
    {
        count = Math.Min(count, _listStack!.Count);

        for (int n = 0; n < count; ++n)
        {
            _buffer.Append("\n</li>");

            int index = _listStack.Count - 1;
            _buffer.Append(_listStack[index]);
            _listStack.RemoveAt(index);
        }
    }

    private void WindQuoteBlock(int count)
    {
        for (int n = 0; n < count; ++n)
        {
            WriteLine();
            _buffer.Append("<blockquote>");
        }
    }

    private void UnwindQuoteBlock(int count)
    {
        for (int n = 0; n < count; ++n)
        {
            _buffer.Append("\n</blockquote>");
        }
    }

    private void WriteLine()
    {
        if (_buffer.Length != 0)
        {
            _buffer.Append('\n');
        }
    }
}
