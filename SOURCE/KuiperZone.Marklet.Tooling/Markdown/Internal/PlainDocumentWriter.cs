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

internal sealed class PlainDocumentWriter
{
    private readonly PlainBlockWriter _blocker = new();

    public PlainDocumentWriter(MarkDocument document)
    {
        Document = document;
    }

    public MarkDocument Document { get; }

    public override string ToString()
    {
        IReadOnlyMarkBlock? previous = null;

        foreach (var item in Document)
        {
            _blocker.Block = item;
            _blocker.Write(previous);
            previous = item;
        }

        return _blocker.Buffer.ToString();
    }

}
