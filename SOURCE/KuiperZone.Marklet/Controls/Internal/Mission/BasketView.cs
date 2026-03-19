// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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
using Avalonia.Input;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Displays contents of a <see cref="GardenBasket"/> instance.
/// </summary>
internal sealed class BasketView : DockPanel
{
#if DEBUG
    // Gotta see this in action
    public const int MaxDefaultFolders = 2;
#else
    public const int MaxDefaultFolders = 5;
#endif

#if DEBUG
    public const int MaxDefaultRootHistory = 2;
#else
    public const int MaxDefaultRootHistory = 10;
#endif

    private static readonly IReadOnlyDictionary<string, FolderView> EmptyCache = new Dictionary<string, FolderView>();

    private readonly BasketCase _bcase;
    private readonly ScrollPanel _scroller = new();
    private readonly FolderView _rootFolder;

    private IReadOnlyDictionary<string, FolderView> _folderCache = EmptyCache;
    private bool _isPinTop;

    /// <summary>
    /// Constructor.
    /// </summary>
    public BasketView(MainMission mission, BasketKind kind)
    {
        Garden = mission.Garden;
        Kind = kind;
        Basket = Garden.GetBasket(kind);

        _bcase = new(this, mission);
        _rootFolder = new(this, true);

        // Allow to receive key input and click
        // on background to deselect renamer
        Focusable = true;
        IsTabStop = false;

        // Necessary to trigger first render when set true
        IsVisible = false;
        _bcase.Toolbar.Margin = ToolMargin;
        _bcase.SearchEditor.Margin = ToolMargin;
        _scroller.ContentMargin = ScrollMargin;
        _scroller.VerticalSpacing = ChromeSizes.StandardPx;

        Children.Add(_bcase.Toolbar);
        SetDock(_bcase.Toolbar, Dock.Top);

        Children.Add(_bcase.SearchEditor);
        SetDock(_bcase.SearchEditor, Dock.Top);

        Children.Add(_scroller);
        SetDock(_scroller, Dock.Bottom);

        var more = _bcase.HistoryMore;
        more.GotoTopClick += (_, __) => _scroller.NormalizedY = 0;
        _scroller.ScrollChanged += ScrollerChangedHandler;

        Basket.Changed += BasketChangedHandler;
        Garden.CurrentChanged += CurrentChangedHandler;
        _bcase.ScheduleTick += ScheduleRebuildHandler;
    }

    /// <summary>
    /// Fixed margin size.
    /// </summary>
    public static readonly Thickness ToolMargin = new(ChromeSizes.TwoCh, 0.0, ChromeSizes.TwoCh, ChromeSizes.TwoCh);

    /// <summary>
    /// Fixed margin size.
    /// </summary>
    public static readonly Thickness ScrollMargin = new(ChromeSizes.TwoCh, 0.0, 3.0 * ChromeSizes.OneCh, 0.0);

    /// <summary>
    /// Defines the <see cref="IsPinTop"/> property.
    /// </summary>
    public static readonly DirectProperty<BasketView, bool> IsPinTopProperty =
        AvaloniaProperty.RegisterDirect<BasketView, bool>(nameof(IsPinTop),
        o => o.IsPinTop, (o, v) => o.IsPinTop = v);

    /// <summary>
    /// Occurs after the view is updated.
    /// </summary>
    /// <remarks>
    /// This occurs after any <see cref="GardenBasket.Changed"/> event while the basket is visible.
    /// </remarks>
    public event EventHandler<EventArgs>? ViewChanged;

    /// <summary>
    /// Gets the basket kind. For search, the value is <see cref="BasketKind.None"/>.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Gets the garden.
    /// </summary>
    public MemoryGarden Garden { get; }

    /// <summary>
    /// Gets the <see cref="GardenBasket"/> data source.
    /// </summary>
    public GardenBasket Basket;

    /// <summary>
    /// Gets the item sort order.
    /// </summary>
    public GardenSort SortOrder { get; private set; } = GardenSort.UpdateNewestFirst;

    /// <summary>
    /// Gets currently selected item in this basket.
    /// </summary>
    /// <remarks>
    /// The result is null if no items or multiple items are selected.
    /// </remarks>
    public GardenDeck? Current { get; private set; }

    /// <summary>
    /// Gets number of folders from last update.
    /// </summary>
    public int FolderCount
    {
        get { return _folderCache.Count; }
    }

    /// <summary>
    /// Gets whether the search editor is visible.
    /// </summary>
    public bool IsSearching
    {
        get { return _bcase.IsSearching; }
        set { _bcase.IsSearching = false; }
    }

    /// <summary>
    /// Gets or sets whether pinned items are sorted first.
    /// </summary>
    /// <remarks>
    /// Want this as a property so we can potentially respond to events.
    /// </remarks>
    public bool IsPinTop
    {
        get { return _isPinTop; }
        set { SetAndRaise(IsPinTopProperty, ref _isPinTop, value); }
    }

    /// <summary>
    /// Open all open folders.
    /// </summary>
    public void ExpandFolders()
    {
        foreach (var item in _folderCache.Values)
        {
            item.IsOpen = true;
        }
    }

    /// <summary>
    /// Close all open folders.
    /// </summary>
    public void CloseFolders()
    {
        foreach (var item in _folderCache.Values)
        {
            item.IsOpen = false;
        }
    }

    /// <summary>
    /// Refresh existing content with rebuild.
    /// </summary>
    public void RefreshVisual()
    {
        foreach (var item in _folderCache.Values)
        {
            item.RefreshVisual(IsSearching);
        }

        _rootFolder.RefreshVisual(IsSearching);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(OnKeyDown)}";

        base.OnKeyDown(e);
        ConditionalDebug.WriteLine(NSpace, $"Key: {e.Key}, {e.KeyModifiers}");

        if (e.Handled)
        {
            return;
        }

        if (IsSearching && e.Key == Key.Escape)
        {
            e.Handled = true;
            IsSearching = false;
            return;
        }

        if (_bcase.Menu.HandleKeyGesture(e))
        {
            return;
        }

        if (Garden.Current?.VisualComponent is DeckCard card)
        {
            // Call the static method, not PixieControl.HandleKeyGesture()
            if (CardMenu.HandleKeyGesture(e, card))
            {
                return;
            }
        }
    }

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
                return;
            }

            _bcase.IsEnabled = false;
            return;
        }

        if (p == IsPinTopProperty)
        {
            var value = change.GetNewValue<bool>();
            _bcase.Menu.PinTopItem.IsChecked = value;
            SortOrder = value ? GardenSort.UpdateNewestPinnedFirst : GardenSort.UpdateNewestFirst;
            _bcase.ScheduleRebuild();
            return;
        }
    }

    private static void SetCardChecked(GardenDeck? deck)
    {
        if (deck?.VisualComponent is DeckCard card)
        {
            card.IsChecked = deck.IsCurrent;

            if (deck.IsCurrent)
            {
                card.Group?.IsOpen = true;
            }
        }
    }

    private void OnShow()
    {
        // We have become visible
        const string NSpace = $"{nameof(BasketView)}.{nameof(OnShow)}";
        ConditionalDebug.WriteLine(NSpace, Kind);

        // Setting to true should rebuild immediately
        _bcase.IsEnabled = true;
        SetCardChecked(Current);
    }

    private void ScheduleRebuildHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(ScheduleRebuildHandler)}";
        ConditionalDebug.WriteLine(NSpace, "UPDATING: " + Kind);

        if (!IsVisible)
        {
            ConditionalDebug.WriteLine(NSpace, "NOT VISIBLE: " + Kind);
            return;
        }

        List<Control>? buffer = null;
        List<GardenDeck>? root = null;
        var folderMore = _bcase.FolderMore;
        var historyMore = _bcase.HistoryMore;

        // COLLATE COLLATE FOLDERS
        var searchOpts = _bcase.GetSearchOptions();
        var collation = Basket.CollateByFolder(searchOpts, SortOrder);

        var folders = new List<FolderView>(collation.Count + 8);
        var newCache = new Dictionary<string, FolderView>(folders.Capacity);

        // CHILD FOLDERS
        // Collation contains items with group name only
        foreach (var item in collation)
        {
            // Cannot have empty name
            ConditionalDebug.ThrowIfNullOrEmpty(item.Key);

            if (!_folderCache.TryGetValue(item.Key, out FolderView? folder))
            {
                ConditionalDebug.WriteLine(NSpace, "New folder instance");
                folder = new FolderView(this, item.Key);
            }

            ConditionalDebug.WriteLine(NSpace, "Updating folder");
            ConditionalDebug.ThrowIfNotEqual(item.Key, folder.FolderName);
            newCache.Add(item.Key, folder);

            // Start to assign timestamp
            folder.StartRebuild(item.Value, _bcase.IsSearching);
            folders.Add(folder);

            if (searchOpts != null)
            {
                folder.IsOpen = true;
            }
        }

        if (searchOpts == null || newCache.Count > _folderCache.Count)
        {
            // Store for next time
            _folderCache = newCache;
        }

        // We have to sort before we can trim.
        folders.Sort(_bcase.FolderCompare);
        folderMore.IsMoreVisible = folders.Count > MaxDefaultFolders;

        if (!folderMore.IsMoreChecked && folders.Count > MaxDefaultFolders)
        {
            // Trim what we add to visual tree
            ConditionalDebug.WriteLine(NSpace, $"Trim end folders");
            folders.RemoveRange(MaxDefaultFolders, folders.Count - MaxDefaultFolders);
        }

        foreach (var item in folders)
        {
            item.FinishRebuild();
        }

        // We need a copy because type difference
        buffer = new(folders.Count + 4);
        buffer.Add(_bcase.FolderHeader);
        buffer.AddRange(folders);

        _bcase.FolderHeader.Count = collation.Count;

        if (collation.Count > MaxDefaultFolders)
        {
            // Add "see all" folders
            buffer.Add(folderMore);
        }


        // ROOT HISTORY
        root = Basket.GetFolderContents(null, searchOpts, SortOrder) ??
            throw new InvalidOperationException("Null not expected");

        ConditionalDebug.WriteLine(NSpace, "Updating history");
        buffer.Add(_bcase.RootHeader);
        buffer.Add(_rootFolder);
        _bcase.RootHeader.Count = root.Count;

        historyMore.IsMoreVisible = root.Count > MaxDefaultRootHistory;
        buffer.Add(historyMore);

        if (root.Count > MaxDefaultRootHistory && !historyMore.IsMoreChecked)
        {
            ConditionalDebug.WriteLine(NSpace, $"Trim root end");
            root.RemoveRange(MaxDefaultRootHistory, root.Count - MaxDefaultRootHistory);
        }

        _rootFolder.StartRebuild(root, _bcase.IsSearching);
        _rootFolder.FinishRebuild();

        if (root.Count == 0)
        {
            buffer.Add(_bcase.None);
        }

        // UPDATE
        _scroller.Children.Replace(buffer);

        if (Basket.Contains(Garden.Current))
        {
            if (Current != Garden.Current)
            {
                Current = Garden.Current;
            }
        }
        else
        if (Basket.Contains(Current))
        {
            Current.IsCurrent = true;
        }
        else
        {
            Current = null;
            Garden.Current?.IsCurrent = false;
        }

        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CurrentChangedHandler(object? _, CurrentDeckChangedEventArgs e)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(CurrentChangedHandler)}";

        if (Basket.Contains(e.Current))
        {
            ConditionalDebug.WriteLine(NSpace, $"SELECTED: {e.Current}");
            ConditionalDebug.ThrowIfFalse(e.Current.IsCurrent);

            Current = e.Current;

            if (IsVisible)
            {
                SetCardChecked(Current);
            }
        }
        else
        if (e.Current == null && IsVisible)
        {
            Current = null;
        }

        if (Basket.Contains(e.Previous))
        {
            // Needed as last select may be in different folder
            SetCardChecked(e.Previous);
        }
    }

    private void BasketChangedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(BasketChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "BASKET: " + Kind);
        ConditionalDebug.WriteLine(NSpace, "Current updated: " + Garden?.Current?.Updated);

        if (Basket.Contains(Garden?.Current))
        {
            ConditionalDebug.WriteLine(NSpace, "Contains this");
            Current = Garden.Current;
        }
        else
        if (!Basket.Contains(Current))
        {
            ConditionalDebug.WriteLine(NSpace, "Not contains this");
            Current = null;
        }

        _bcase.ScheduleRebuild();
    }

    private void ScrollerChangedHandler(object? _, ScrollChangedEventArgs e)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(ScrollerChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"NormY: {_scroller.NormalizedY:R}");
        _bcase.HistoryMore.IsGotoTopVisible = _scroller.NormalizedY > 0.0;
    }

}