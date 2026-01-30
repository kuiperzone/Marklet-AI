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

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <summary>
/// A table comprising instances of <see cref="StyledContainer"/> arranged by columns and rows.
/// </summary>
public interface IReadOnlyMarkTable : IEquatable<IReadOnlyMarkTable>
{
    /// <summary>
    /// Gets the row count.
    /// </summary>
    int RowCount { get; }

    /// <summary>
    /// Gets the column count.
    /// </summary>
    /// <remarks>
    /// The number of columns is fixed on construction and cannot be changed. The value cannot be less than 1.
    /// </remarks>
    int ColCount { get; }

    /// <summary>
    /// Gets the column alignments.
    /// </summary>
    /// <remarks>
    /// The sequence always has <see cref="ColCount"/> items.
    /// </remarks>
    IReadOnlyList<ColAlign> Aligns { get; }

    /// <summary>
    /// Gets cell elements.
    /// </summary>
    /// <remarks>
    /// Cells are arranged across (left to right) then down. The count is always equal <see cref="RowCount"/> * <see
    /// cref="ColCount"/>. The first <see cref="ColCount"/> items represent cells in the "header".
    /// </remarks>
    IReadOnlyList<IReadOnlyStyledContainer> Cells { get; }

    /// <summary>
    /// Gets the cell at the given row and column.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">rowN, colN</exception>
    IReadOnlyStyledContainer Cell(int rowN, int colN);

    /// <summary>
    /// Returns the display width of "colN" in either characters or "graphemes" (user perceived characters).
    /// </summary>
    /// <remarks>
    /// The result does not include any padding or construction lines between cells, and may be 0 if all cells in the
    /// column are empty. The result is intended to be used in padding for fixed font output.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">colN</exception>
    int GetColWidth(int colN, bool graphemes);

    /// <summary>
    /// Equivalent to <see cref="GetColWidth(int, bool)"/> with "graphemes" equal to true.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">colN</exception>
    int GetColWidth(int colN);

    /// <summary>
    /// Returns the display height of "rowN" in characters.
    /// </summary>
    /// <remarks>
    /// The result is never less than 1, even for an empty row.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">rowN</exception>
    int GetRowHeight(int rowN);

    /// <summary>
    /// Returns a string in the given "format".
    /// </summary>
    string ToString(TextFormat format);

    /// <summary>
    /// Equivalent to <see cref="ToString(TextFormat)"/> with <see cref="TextFormat.Markdown"/>.
    /// </summary>
    string ToString();
}
