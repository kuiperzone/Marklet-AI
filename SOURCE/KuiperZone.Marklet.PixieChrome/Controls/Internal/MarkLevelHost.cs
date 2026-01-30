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
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Hosts controls displaying one or more <see cref="IReadOnlyMarkBlock"/> instances.
/// </summary>
/// <remarks>
/// Hosts may be cached and updated where <see cref="IsCompatible(IReadOnlyMarkBlock)"/> gives true.
/// </remarks>
internal sealed class MarkLevelHost : MarkVisualHost
{
    private readonly Grid _grid = new();
    private readonly List<MarkBlockHost> _childHosts = new(2);
    private readonly List<Rectangle>? _quoteBars;
    private readonly List<TextBlock>? _prefixes;

    /// <summary>
    /// Constructor with first compatible "source" block.
    /// </summary>
    public MarkLevelHost(MarkView owner, IReadOnlyMarkBlock source0)
        : base(owner)
    {
        Control = _grid;
        ChildHosts = _childHosts;

        QuoteLevel = source0.QuoteLevel;
        ListLevel = source0.ListLevel;

        if (!IsCompatible(source0))
        {
            throw new ArgumentException("Not a quote or list block", nameof(source0));
        }

        if (QuoteLevel > 0)
        {
            _quoteBars = new(2);

            for (int n = 0; n < QuoteLevel; ++n)
            {
                // Quote columns MUST come first
                _grid.ColumnDefinitions.Add(new(GridLength.Auto));
            }
        }

        if (ListLevel > 0)
        {
            _prefixes = new(2);

            // Indent spacer
            _grid.ColumnDefinitions.Add(new(GridLength.Auto));

            // Prefix
            _grid.ColumnDefinitions.Add(new(GridLength.Auto));
        }

        // Content
        _grid.ColumnDefinitions.Add(new(GridLength.Star));
    }

    /// <summary>
    /// Gets the children of this level. The Count of this gives the number of blocks this will nominally "consume".
    /// </summary>
    public IReadOnlyList<MarkVisualHost> ChildHosts { get; }

    /// <summary>
    /// Gets whether the instance is pending its first update (set false by <see cref="ConsumeUpdates"/>).
    /// </summary>
    public bool IsPending { get; private set; } = true;

    /// <summary>
    /// Gets whether "source" is compatible with this "cached" visual block.
    /// </summary>
    public bool IsCompatible(IReadOnlyMarkBlock source)
    {
        if (source.QuoteLevel > 0 || source.ListLevel > 0)
        {
            return QuoteLevel == source.QuoteLevel && ListLevel == source.ListLevel;
        }

        return false;
    }

    /// <summary>
    /// Refreshes colors and sizes, but not content.
    /// </summary>
    public override void RefreshLook(bool isFirst, bool isLast)
    {
        // Order important
        ConditionalDebug.ThrowIfTrue(IsPending);

        foreach (var item in _childHosts)
        {
            item.RefreshLook(isFirst, isLast);
        }

        RefreshInternal();
    }

    /// <summary>
    /// Consumes updates from "source" sequence and returns the number of blocks consumed.
    /// </summary>
    public override MarkConsumed ConsumeUpdates(IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        if (IsPending)
        {
            // New instance
            IsPending = false;
            ConditionalDebug.ThrowIfNotEqual(_childHosts.Count, 0);

            while(index < sequence.Count)
            {
                if (!IsCompatible(sequence[index]))
                {
                    break;
                }

                AddHost(MarkBlockHost.New(Owner, sequence, ref index));
            }

            if (_childHosts.Count == 0)
            {
                // At least one expected from construction
                throw new InvalidOperationException("Failed to consume initial blocks");
            }

            // Do last!
            RefreshInternal();
            return MarkConsumed.Changed;
        }

        // PRECHECK
        // Not strictly necessary, but the check should be efficient, and we
        // can return early without intensive updates on a compatibility failure.
        int rowN = 0;
        int hostCount = _childHosts.Count;

        if (sequence.Count - index < hostCount)
        {
            // Remaining count too short
            return MarkConsumed.Incompatible;
        }

        for (int n = index; n < sequence.Count; ++n)
        {
            var host = _childHosts[rowN];

            if (!host.IsCompatible(sequence[n]))
            {
                // IMPORTANT
                // If any fail compability, all fail!
                // We do NOT try to replace instances within the level.
                // This will break CrossTracker in any case.
                return MarkConsumed.Incompatible;
            }

            // If child compatible, the level itself must be compatible
            ConditionalDebug.ThrowIfFalse(IsCompatible(sequence[n]));

            if (++rowN == hostCount)
            {
                break;
            }
        }

        // CONSUME
        rowN = 0;
        bool changed = false;

        while(index < sequence.Count)
        {
            var rslt = _childHosts[rowN].ConsumeUpdates(sequence, ref index);

            if (rslt == MarkConsumed.Incompatible)
            {
                // The pre-check accepted this!
                ConditionalDebug.Fail($"Invalid {nameof(MarkVisualHost.ConsumeUpdates)} result");
                return MarkConsumed.Incompatible;
            }

            changed |= rslt == MarkConsumed.Changed;

            if (++rowN == hostCount)
            {
                break;
            }
        }

        if (changed)
        {
            // Do last!
            RefreshInternal();
            return MarkConsumed.Changed;
        }

        return MarkConsumed.NoChange;
    }

    /// <summary>
    /// Extends.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder(base.ToString());

        Append(buffer, $"{nameof(ChildHosts)}.Count", ChildHosts.Count);

        var list = new List<string?>();

        if (_prefixes != null)
        {
            foreach (var item in _prefixes)
            {
                list.Add(item.Text);
            }
        }

        Append(buffer, "Prefixes", string.Join(',', list));
        Append(buffer, "QuoteBars", _quoteBars?.Count);
        return buffer.ToString();
    }

    private double GetQuoteBarWidth()
    {
        return Math.Max(Owner.ScaledFontSize * 0.3, 1.0);
    }

    private void AddHost(MarkBlockHost host)
    {
        // Start
        int colN = QuoteLevel;
        int rowN = _childHosts.Count;
        ConditionalDebug.ThrowIfNotEqual(rowN, _grid.RowDefinitions.Count);

        _childHosts.Add(host);
        _grid.RowDefinitions.Add(new(GridLength.Auto));

        for (int n = 0; n < QuoteLevel; ++n)
        {
            // QUOTE BARS
            var bar = new Rectangle();
            bar.VerticalAlignment = VerticalAlignment.Stretch;
            bar.HorizontalAlignment = HorizontalAlignment.Left;

            // Not expected to be null
            _quoteBars!.Add(bar);

            Grid.SetRow(bar, rowN);
            Grid.SetColumn(bar, n);
            _grid.Children.Add(bar);
        }

        // Here, with no QuoteLevel, colN will be 0.
        // Otherwise it be at the expected "indent column" index.
        if (ListLevel > 0)
        {
            // LIST ITEMS
            ConditionalDebug.ThrowIfEqual(ListKind.None, host.Source.GetListKind());

            // INDENT COLUMN
            // Skip indent column as it is empty
            colN += 1;

            // PREFIX COLUMN
            var prefix = new TextBlock();
            prefix.VerticalAlignment = VerticalAlignment.Top;
            prefix.HorizontalAlignment = HorizontalAlignment.Right;

            // IMPORTANT
            // Link to associated host.
            // Need this in the later Refresh().
            prefix.Tag = host;

            // Not expected to be null
            _prefixes!.Add(prefix);

            Grid.SetRow(prefix, rowN);
            Grid.SetColumn(prefix, colN++);
            _grid.Children.Add(prefix);
        }

        // CONTENT
        var control = host.Control;
        Grid.SetRow(control, rowN);
        Grid.SetColumn(control, colN);
        _grid.Children.Add(control);

        SetTrackKey(host.TrackKey0);
        SetTrackKey(host.TrackKey1);
    }

    private void RefreshInternal()
    {
        if (_quoteBars != null)
        {
            ConditionalDebug.ThrowIfZero(QuoteLevel);

            for (int n = 0; n < QuoteLevel; ++n)
            {
                RefreshQuoteColumn(_grid.ColumnDefinitions[n]);
            }

            var fill = Owner.QuoteDecor;
            double width = GetQuoteBarWidth();

            foreach (var bar in _quoteBars)
            {
                bar.Width = width;
                bar.Fill = fill;
            }
        }

        if (_prefixes != null)
        {
            ConditionalDebug.ThrowIfZero(ListLevel);

            foreach (var item in _prefixes)
            {
                RefreshPrefix(item);
            }

            // QuoteLevel is also the "indent column".
            // It is valid for it to be 0.
            var col = _grid.ColumnDefinitions[QuoteLevel];
            ConditionalDebug.ThrowIfNotEqual(col.Width, GridLength.Auto);
            col.MinWidth = Owner.TabPx * ListLevel;
        }
    }

    private void RefreshQuoteColumn(ColumnDefinition col)
    {
        double width = GetQuoteBarWidth() * 4.0;
        col.MinWidth = width;
        col.MaxWidth = width;
        col.Width = new(width, GridUnitType.Pixel);
    }

    private void RefreshPrefix(TextBlock prefix)
    {
        var host = (MarkBlockHost)prefix.Tag!;
        var c = host.Control;
        prefix.Margin = new(0, c.Margin.Top, Owner.OneCh, 0);

        // This is where we update the content
        prefix.Text = host.Source.GetListPrefix();

        // Do not set Foreground or Family
        prefix.FontSize = Owner.ScaledFontSize;
        prefix.LineHeight = Owner.ScaledLineHeight;
    }

    private void SetTrackKey(ulong key)
    {
        if (key != 0U)
        {
            if (TrackKey0 == 0)
            {
                TrackKey0 = key;
            }

            TrackKey1 = key;
            ConditionalDebug.ThrowIfLessThan(TrackKey1, TrackKey0);
        }
    }
}