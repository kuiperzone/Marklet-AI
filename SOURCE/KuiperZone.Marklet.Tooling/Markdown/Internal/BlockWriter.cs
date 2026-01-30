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

internal abstract class BlockWriter
{
    /// <summary>
    /// Tab string.
    /// </summary>
    public static readonly string Tab = "    ";

    public static readonly IReadOnlyMarkBlock Empty = new MarkBlock();

    protected BlockWriter()
    {
        Block = Empty;
    }

    protected BlockWriter(IReadOnlyMarkBlock block)
    {
        Block = block;
        IsStandalone = true;
    }

    public bool IsStandalone { get; }
    public IReadOnlyMarkBlock Block { get; set; }
    public StringBuilder Buffer { get; } = new(128);

    public static string? GetQuotePrefix(int level)
    {
        string? str = null;

        for (int n = 0; n < level; ++n)
        {
            str += "> ";
        }

        return str;
    }

    public sealed override string ToString()
    {
        return Buffer.ToString();
    }

    protected void WriteLine()
    {
        if (Buffer.Length != 0)
        {
            Buffer.Append('\n');
        }
    }

    protected string? GetQuotePrefix()
    {
        return GetQuotePrefix(Block.QuoteLevel);
    }


}
