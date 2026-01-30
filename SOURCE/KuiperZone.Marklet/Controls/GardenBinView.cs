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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Shared;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Displays contents of a <see cref="GardenBin"/> instance.
/// </summary>
/// <remarks>
/// The class is event driven and changes when <see cref="GardenBin.Updated"/> is raised. The class derives from <see
/// cref="PixieGroup"/> and shares its StyleKey. Important.
/// </remarks>
public sealed class GardenBinView : TopicControl
{
    private readonly DispatcherTimer _visibilityTimer = new();
    private readonly DispatchCoalescer _updater = new(DispatcherPriority.Render);

    private GardenBin? _sourceBin;
    private GardenSort _sorting = GardenSort.UpdateNewestFirst;
    private Dictionary<string, TopicControl> _topicCache = new();
    private DateTime _refreshTime;
    private TextBlock? _tagged;
    private TextBlock? _untagged;
    private SessionControl? _selectedControl;
    private bool _initialUpdate = true;

    // Backing fields
    private bool _isTopicGrouped;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public GardenBinView()
    {
        _updater.Posted += (_, __) => Update();
        _visibilityTimer.Interval = TimeSpan.FromMilliseconds(250);
        _visibilityTimer.Tick += TimerTickHandler;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public GardenBinView(bool isTopicGrouped)
        : this()
    {
        _isTopicGrouped = isTopicGrouped;
    }

    /// <summary>
    /// Defines the <see cref="IsTopicGrouped"/> property.
    /// </summary>
    public static readonly DirectProperty<GardenBinView, bool> IsTopicGroupedProperty =
        AvaloniaProperty.RegisterDirect<GardenBinView, bool>(nameof(IsTopicGrouped),
            o => o.IsTopicGrouped, (o, v) => o.IsTopicGrouped = v);

    /// <summary>
    /// Defines the <see cref="IsTopicGrouped"/> property.
    /// </summary>
    public static readonly DirectProperty<GardenBinView, GardenSort> SortingProperty =
        AvaloniaProperty.RegisterDirect<GardenBinView, GardenSort>(nameof(Sorting),
            o => o.Sorting, (o, v) => o.Sorting = v);

    /// <summary>
    /// Gets whether items are grouped according to <see cref="GardenSession.Topic"/>.
    /// </summary>
    public bool IsTopicGrouped
    {
        get { return _isTopicGrouped; }
        set { SetAndRaise(IsTopicGroupedProperty, ref _isTopicGrouped, value); }
    }

    /// <summary>
    /// Gets or sets how child items are sorted.
    /// </summary>
    public GardenSort Sorting
    {
        get { return _sorting; }
        set { SetAndRaise(SortingProperty, ref _sorting, value); }
    }

    /// <summary>
    /// Gets or sets the associated source data bin.
    /// </summary>
    public GardenBin? SourceBin
    {
        get { return _sourceBin; }

        set
        {
            if (_sourceBin != value)
            {
                if (_sourceBin != null)
                {
                    _sourceBin.Updated -= BinUpdatedHandler;
                    _sourceBin.Owner.SelectedChanged -= GardenSelectChangedHandler;
                }

                _sourceBin = value;
                IsWaste = false;
                Garden = value?.Owner;
                _initialUpdate = true;

                if (value != null)
                {
                    IsWaste = value.IsWaste;
                    value.Updated += BinUpdatedHandler;
                    value.Owner.SelectedChanged += GardenSelectChangedHandler;
                }

                PostUpdate();
            }
        }
    }

    /// <summary>
    /// Gets whether this is the waste bin.
    /// </summary>
    public bool IsWaste { get; private set; }

    /// <summary>
    /// Gets the <see cref="MemoryGarden"/> owner of the <see cref="SourceBin"/> instance for convenience.
    /// </summary>
    public MemoryGarden? Garden { get; private set; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(GardenBinView);

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == IsVisibleProperty)
        {
            if (change.GetNewValue<bool>())
            {
                OnShow();
            }

            return;
        }

        if (p == IsTopicGroupedProperty || p == SortingProperty)
        {
            PostUpdate();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Refresh();
        Styling.StylingChanged += StylingChangedHandler;
        _visibilityTimer.IsEnabled = true;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _visibilityTimer.IsEnabled = false;
        Styling.StylingChanged -= StylingChangedHandler;
    }

    private static int SessionCompare(PixieGroup x, PixieGroup y)
    {
        return StringComparer.OrdinalIgnoreCase.Compare(x.CollapseTitle, y.CollapseTitle);
    }

    private static TextBlock NewTagBlock(string title)
    {
        const double fs = ChromeFonts.SmallFontSize;
        var block = new TextBlock();
        block.Text = title;

        block.FontSize = fs;
        block.Foreground = ChromeStyling.ForegroundGray;
        block.Margin = new(0.0, fs, 0.0, fs);
        return block;
    }

    private void Update()
    {
        const string NSpace = $"{nameof(GardenBinView)}.{nameof(Update)}";
        ConditionalDebug.WriteLine(NSpace, "UPDATING: " + TopTitle);

        // Refresh not needed for a while
        _refreshTime = default;

        // True on first update
        bool initial = _initialUpdate;
        _initialUpdate = false;

        // Get list of bin session in custom sorted order
        List<Control>? buffer = null;
        List<GardenSession> sortedSessions = _sourceBin?.GetSortedSessions(_sorting) ?? new();

        SessionControl? newInstance = null;

        if (IsTopicGrouped)
        {
            // Re-assemble child topics. We re-use existing
            // topic controls from Children where we can as we
            // assume this is more efficient than building new
            // controls from scratch on each update. It is also
            // important in holding open state of a named topic.
            var collation = CollationOnTopic(sortedSessions);
            var named = new List<TopicControl>(collation.Count + 8);
            var newCache = new Dictionary<string, TopicControl>(named.Count);

            // Collation contains items with group name only
            foreach (var item in collation)
            {
                // Cannot have empty name
                ConditionalDebug.ThrowIfNullOrEmpty(item.Key);
                var gx = _topicCache.GetValueOrDefault(item.Key) ?? new(item.Key);
                var recent = gx.UpdateChildren(item.Value);

                named.Add(gx);
                newCache.Add(item.Key, gx);
                newInstance = SessionControl.MoreRecent(newInstance, recent);
            }

            _topicCache = newCache;

            // Named groups as always alpha sorted
            named.Sort(SessionCompare);

            // We need a copy because type difference
            buffer = new(named.Count + 2);
            _tagged ??= NewTagBlock("Topics");
            buffer.Add(_tagged);

            buffer.AddRange(named);

            _untagged ??= NewTagBlock("Untagged");
            buffer.Add(_untagged);
        }

        // 1. Where IsTopicGrouped is true, we append items with
        //    no topic to the bottom of "working", or:
        // 2. Where IsTopicGrouped is false, we will append all items
        //    as ungrouped, and assign to "this" top-level group.
        newInstance = SessionControl.MoreRecent(newInstance, UpdateChildren(sortedSessions, buffer));

        if (!initial && newInstance != null)
        {
          //  _selectedControl = newInstance;
            newInstance.Source.IsSelected = true;
        }
    }

    private Dictionary<string, List<GardenSession>> CollationOnTopic(List<GardenSession> sorted)
    {
        var capacity = Math.Max(Children.Count, 4);
        var collation = new Dictionary<string, List<GardenSession>>(capacity, StringComparer.OrdinalIgnoreCase);

        foreach (var item in sorted)
        {
            // Do not include those without topic
            if (item.Topic != null)
            {
                if (collation.TryGetValue(item.Topic, out List<GardenSession>? list))
                {
                    list.Add(item);
                    continue;
                }

                list = new(4);
                list.Add(item);
                collation.Add(item.Topic, list);
            }
        }

        return collation;
    }

    private void OnShow()
    {
        // We have become visible
        const string NSpace = $"{nameof(GardenBinView)}.{nameof(OnShow)}";
        ConditionalDebug.WriteLine(NSpace, TopTitle);

        PostUpdate();

        // Reselect on show
        if (_selectedControl?.IsMemberOf(SourceBin) == true)
        {
            _selectedControl.Source.IsSelected = true;
            return;
        }

        _selectedControl = null;
        Garden?.Selected?.IsSelected = false;
    }

    private void PostUpdate()
    {
        if (IsVisible)
        {
            _updater.Post();
        }
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        Refresh();
    }

    private void BinUpdatedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(GardenBinView)}.{nameof(BinUpdatedHandler)}";
        ConditionalDebug.WriteLine(NSpace, TopTitle);
        PostUpdate();
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(GardenBinView)}.{nameof(TimerTickHandler)}";

        try
        {
            if (_selectedControl?.Group?.IsOpen == false)
            {
                // If user closes a child group we automatically deselect.
                ConditionalDebug.WriteLine(NSpace, $"Group has closed for {TopTitle}");

                _selectedControl = null;
                Garden?.Selected?.IsSelected = false;
            }

            // Periodic refresh
            var now = DateTime.UtcNow;

            if (now > _refreshTime)
            {
                // This should not be expensive.
                // Primary purpose is to refresh "friendly time"
                ConditionalDebug.WriteLine(NSpace, $"Refresh children for {TopTitle}");

                if (_refreshTime != default)
                {
                    Refresh();
                }

                _refreshTime = now.AddSeconds(55 + Random.Shared.Next(0, 10)); // <- allow to drift between bins
            }
        }
        catch (Exception ex)
        {
            ConditionalDebug.WriteLine(NSpace, ex, false);
            throw;
        }
    }

    private void GardenSelectChangedHandler(object? _, SelectedChangedEventArgs e)
    {
        const string NSpace = $"{nameof(GardenBinView)}.{nameof(GardenSelectChangedHandler)}";

        if (e.Selected?.VisualTag is SessionControl selected && selected.IsMemberOf(SourceBin))
        {
            ConditionalDebug.WriteLine(NSpace, $"SELECTED: {e.Selected}");
            ConditionalDebug.ThrowIfFalse(e.Selected.IsSelected);
            _selectedControl = selected;

            if (!selected.IsBackgroundChecked)
            {
                selected.Group?.IsOpen = true;
                selected.IsBackgroundChecked = true;
                selected.BringIntoView();
            }
        }

        if (e.Previous?.VisualTag is SessionControl unBtn && unBtn.IsBackgroundChecked)
        {
            // The selected instance may not belong to this bin (that is OK).
            ConditionalDebug.WriteLine(NSpace, $"UNSELECTED: {e.Previous}");
            ConditionalDebug.ThrowIfTrue(e.Previous.IsSelected);
            unBtn.IsBackgroundChecked = false;
        }
    }

}