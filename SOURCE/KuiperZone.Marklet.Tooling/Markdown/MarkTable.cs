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

using Markdig.Extensions.Tables;

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <inheritdoc cref="IReadOnlyMarkTable"/>
public sealed class MarkTable : IReadOnlyMarkTable
{
    private static readonly MarkElement ColSep = new("   ");
    private static readonly MarkElement NewLine = new("\n");
    private readonly List<ColAlign> _aligns;
    private readonly List<StyledContainer> _cells;

    /// <summary>
    /// Constructor which creates a table with a single row.
    /// </summary>
    /// <remarks>
    /// The column count remains fixed. The "rowCount" may be 0, however, "colCount" must be 1 or greater.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">rowCount, colCount</exception>
    public MarkTable(int rowCount, int colCount)
        : this(rowCount, colCount, new(colCount), new(rowCount * colCount))
    {
        for (int x = 0; x < colCount; ++x)
        {
            _aligns.Add(ColAlign.Left);

            for (int y = 0; y < rowCount; ++y)
            {
                _cells.Add(new StyledContainer());
            }
        }
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    public MarkTable(IReadOnlyMarkTable other)
        : this(other.RowCount, other.ColCount, new(other.Aligns), new(other.Cells.Count))
    {
        foreach (var item in other.Cells)
        {
            _cells.Add(new StyledContainer(item));
        }
    }

    /// <summary>
    /// Markdig constructor.
    /// </summary>
    internal MarkTable(Table table, MarkOptions opts)
    {
        _cells = new(8);
        Cells = _cells;

        for (int y = 0; y < table.Count; ++y)
        {
            if (table[y] is TableRow row)
            {
                ConditionalDebug.WriteLine(nameof(MarkTable), $"Row {RowCount} columns: {row.Count}");
                RowCount += 1;

                for (int x = 0; x < row.Count; ++x)
                {
                    if (y == 0)
                    {
                        ColCount += 1;
                    }

                    if (x < ColCount)
                    {
                        _cells.Add(new StyledContainer(row[x] as TableCell, opts));
                    }
                    else
                    {
                        _cells.Add(new StyledContainer());
                    }
                }
            }
        }

        ConditionalDebug.WriteLine(nameof(MarkTable), $"RowCount: {table.Count}");
        ConditionalDebug.WriteLine(nameof(MarkTable), $"ColCount: {ColCount}");

        _aligns = new(ColCount);
        Aligns = _aligns;

        var cols = table.ColumnDefinitions;

        for (int x = 0; x < ColCount; ++x)
        {
            if (x < cols.Count)
            {
                var a = cols[x].Alignment;

                if (a == TableColumnAlign.Center)
                {
                    _aligns.Add(ColAlign.Center);
                    continue;
                }

                if (a == TableColumnAlign.Right)
                {
                    _aligns.Add(ColAlign.Right);
                    continue;
                }
            }

            _aligns.Add(ColAlign.Left);
        }
    }

    private MarkTable(int rowCount, int colCount, List<ColAlign> aligns, List<StyledContainer> cells)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(colCount);
        RowCount = rowCount;
        ColCount = colCount;

        _aligns = aligns;
        Aligns = _aligns;

        _cells = cells;
        Cells = _cells;
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.RowCount"/>
    public int RowCount { get; private set; }

    /// <inheritdoc cref="IReadOnlyMarkTable.ColCount"/>
    public int ColCount { get; }

    /// <inheritdoc cref="IReadOnlyMarkTable.Aligns"/>
    public IReadOnlyList<ColAlign> Aligns { get; }

    /// <inheritdoc cref="IReadOnlyMarkTable.Cells"/>
    public IReadOnlyList<StyledContainer> Cells { get; }

    IReadOnlyList<IReadOnlyStyledContainer> IReadOnlyMarkTable.Cells
    {
        get { return _cells; }
    }

    /// <summary>
    /// Static equals.
    /// </summary>
    public static bool Equals(MarkTable? table0, MarkTable? table1)
    {
        if (table0 == table1)
        {
            return true;
        }

        if (table0 != null)
        {
            return table0.Equals(table1);
        }

        // Cannot be null
        return table1!.Equals(table0);
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.Cell(int, int)"/>
    public StyledContainer Cell(int rowN, int colN)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rowN);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rowN, RowCount);

        ArgumentOutOfRangeException.ThrowIfNegative(colN);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(colN, ColCount);

        return _cells[rowN * ColCount + colN];
    }

    IReadOnlyStyledContainer IReadOnlyMarkTable.Cell(int rowN, int colN)
    {
        return Cell(rowN, colN);
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.GetColWidth(int)"/>
    public int GetColWidth(int colN, bool graphemes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(colN);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(colN, ColCount);
        int width = 0;

        for (int y = 0; y < RowCount; ++y)
        {
            width = Math.Max(width, Cell(y, colN).GetWidth(graphemes));
        }

        return width;
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.GetColWidth(int)"/>
    public int GetColWidth(int colN)
    {
        return GetColWidth(colN, true);
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.GetRowHeight(int)"/>
    public int GetRowHeight(int rowN)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rowN);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rowN, RowCount);
        int height = 1;
        int start = ColCount * rowN;
        int end = start + ColCount;

        for (int n = start; n < end; ++n)
        {
            height = Math.Max(height, Cells[n].GetHeight());
        }

        return height;
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.ToString(TextFormat)"/>
    public string ToString(TextFormat format)
    {
        var block = new MarkBlock(BlockKind.TableCode);
        block.Table = this;
        return block.ToString(format);
    }

    /// <inheritdoc cref="IReadOnlyMarkTable.ToString()"/>
    public override string ToString()
    {
        return ToString(TextFormat.Unicode);
    }

    /// <summary>
    /// Sets the alignment for column "align".
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">colN</exception>
    public void SetAlign(int colN, ColAlign align)
    {
        _aligns[colN] = align;
    }

    /// <summary>
    /// Adds a new row and returns the index of the row.
    /// </summary>
    public int AddRow()
    {
        int row = RowCount;
        _cells.Capacity += ColCount;

        for (int n = 0; n < ColCount; ++n)
        {
            _cells.Add(new StyledContainer());
        }

        RowCount += 1;
        return row;
    }

    /// <summary>
    /// Converts the table to padded block.
    /// </summary>
    /// <remarks>
    /// The resulting block will be suitable for rendering in a monospace font. If "tight" is false, the block appears
    /// as a regular table with row separators and <see cref="InlineStyling.Strong"/> is used in the header row. If
    /// "tight" true, the output is a title grid, with no row line separators or spacing between cells.
    /// </remarks>
    public MarkBlock ToPaddedBlock(bool tight = false)
    {
        const char LcN = '\u2500';

        // Thicker line (not used)
        // const char Lc0 = '\u2501';

        var splix = new List<List<StyledContainer>>(ColCount);
        var buffer = new List<MarkElement>(RowCount + RowCount * ColCount * 2);

        int totWidth = 0;
        var colWidths = new int[ColCount];

        for (int x = 0; x < ColCount; ++x)
        {
            colWidths[x] = GetColWidth(x);
            totWidth += colWidths[x];

            if (x != 0 && !tight)
            {
                totWidth += ColSep.Text.Length;
            }
        }

        MarkElement? rowSep0 = null;
        MarkElement? rowSepN = null;

        if (!tight)
        {
            rowSep0 = new MarkElement(new string(LcN, totWidth));
            rowSepN = new MarkElement(new string(LcN, totWidth), InlineStyling.Grayed);
        }

        for (int y = 0; y < RowCount; ++y)
        {
            splix.Clear();
            int rowHeight = GetRowHeight(y);
            var headStyle = (y == 0 && !tight) ? InlineStyling.Strong : InlineStyling.Default;

            for (int x = 0; x < ColCount; ++x)
            {
                var cell = Cell(y, x);
                var colX = new List<StyledContainer>(rowHeight);
                splix.Add(colX);

                for (int n = 0; n < rowHeight; ++n)
                {
                    colX.Add(new StyledContainer());
                }

                int lineN = 0;

                foreach (var e in cell.Elements)
                {
                    var esl = e.SplitLines(e.Styling | headStyle);

                    for (int n = 0; n < esl.Length; ++n)
                    {
                        if (n != 0)
                        {
                            lineN += 1;
                        }

                        colX[lineN].Elements.Add(esl[n]);
                    }
                }
            }


            for (int n = 0; n < rowHeight; ++n)
            {
                if (y != 0 || n != 0)
                {
                    buffer.Add(NewLine);
                }

                for (int x = 0; x < ColCount; ++x)
                {
                    if (x != 0 && !tight)
                    {
                        buffer.Add(ColSep);
                    }

                    var container = splix[x][n];
                    int width = container.GetWidth();
                    var pad = GetPadLeft(Aligns[x], width, colWidths[x]);

                    if (pad != null)
                    {
                        buffer.Add(pad);
                    }

                    buffer.AddRange(container.Elements);

                    pad = GetPadRight(Aligns[x], width, colWidths[x]);

                    if (pad != null)
                    {
                        buffer.Add(pad);
                    }
                }
            }

            if (y < RowCount - 1)
            {
                buffer.Add(NewLine);

                if (!tight)
                {
                    buffer.Add(y == 0 ? rowSep0! : rowSepN!);
                }
            }
        }

        // Don't set block.Table itself.
        var block = new MarkBlock(buffer);
        block.Kind = BlockKind.TableCode;
        block.Coalesce();
        return block;
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(IReadOnlyMarkTable? other)
    {
        if (other == this)
        {
            return true;
        }

        if (other == null || ColCount != other.ColCount || RowCount != other.RowCount)
        {
            return false;
        }

        for (int n = 0; n < Cells.Count; ++n)
        {
            if (!Cells[n].Equals(other.Cells[n]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as IReadOnlyMarkTable);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        AppendHashCode(hash);
        return hash.ToHashCode();
    }

    internal void AppendHashCode(HashCode hash)
    {
        hash.Add(ColCount);
        hash.Add(RowCount);

        foreach (var item in Cells)
        {
            item.AppendHashCode(hash);
        }
    }

    private static MarkElement? GetPadLeft(ColAlign align, int width, int colWidth)
    {
        int delta = colWidth - width;
        ConditionalDebug.ThrowIfNegative(delta);

        if (align == ColAlign.Right)
        {
            if (delta > 0)
            {
                return new MarkElement(new string(' ', delta));
            }

            return null;
        }

        if (align == ColAlign.Center)
        {
            delta /= 2;

            if (delta > 0)
            {
                return new MarkElement(new string(' ', delta));
            }
        }

        return null;
    }

    private static MarkElement? GetPadRight(ColAlign align, int width, int colWidth)
    {
        int delta = colWidth - width;
        ConditionalDebug.ThrowIfNegative(delta);

        if (align == ColAlign.Left)
        {
            if (delta > 0)
            {
                return new MarkElement(new string(' ', delta));
            }

            return null;
        }

        if (align == ColAlign.Center)
        {
            delta = colWidth - width - delta / 2;

            if (delta > 0)
            {
                return new MarkElement(new string(' ', delta));
            }
        }

        return null;
    }

}
