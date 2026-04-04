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
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using Avalonia.Input;
using KuiperZone.Marklet.Shared;
using Avalonia.Threading;
using KuiperZone.Marklet.Windows;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Displays contents of a <see cref="GardenBasket"/> instance.
/// </summary>
internal sealed class BasketView : DockPanel
{
    public const int MaxDefaultFolders = 5;
    public const int MaxDefaultRootHistory = 10;
    public static readonly TimeSpan ScheduleSpan = TimeSpan.FromMilliseconds(200);

    private static readonly MemoryGarden Garden = GardenGrounds.Global;
    private static readonly IReadOnlyDictionary<string, FolderView> EmptyCache = new Dictionary<string, FolderView>();

    private readonly DispatcherTimer _timer = new();
    private readonly BasketToolbar _toolbar;
    private readonly TextEditor _editor = new();
    private readonly FolderView _rootFolder;
    private readonly ScrollPanel _scroller = new();
    private readonly TextBlock _noResults = new();

    private IReadOnlyDictionary<string, FolderView> _folderCache = EmptyCache;
    private bool _isSearching;
    private bool _isPinTop;
    private bool _searchChanged;

    /// <summary>
    /// Constructor.
    /// </summary>
    public BasketView(MainMission mission, BasketKind kind)
    {
        Kind = kind;
        Basket = Garden.GetBasket(kind);
        Mission = mission;

        FolderHeader = new("Folders", false);
        FolderHeader.NewFolder?.Click += (_, __) => ShowNewFolderAsync();

        RootHeader = new("History", true);

        FolderMore = new(false);
        FolderMore.ViewChangeRequired += (_, __) => ScheduleRebuild();

        HistoryMore = new(true);
        HistoryMore.ViewChangeRequired += (_, __) => ScheduleRebuild();
        HistoryMore.GotoTopClick += (_, __) => _scroller.NormalizedY = 0;

        // Allow to receive key input and click
        // on background to deselect renamer
        Focusable = true;
        IsTabStop = false;

        // Necessary to trigger first render when set true
        IsVisible = false;

        _toolbar = new(this);
        Children.Add(_toolbar);
        SetDock(_toolbar, Dock.Top);
        _toolbar.Margin = ToolMargin;

        Children.Add(_editor);
        SetDock(_editor, Dock.Top);
        _editor.Margin = ToolMargin;
        _editor.IsVisible = false;
        _editor.HasBackButton = false;
        _editor.HasMatchCaseButton = true;
        _editor.HasMatchWordButton = true;
        _editor.MaxLength = Math.Min(64, FindOptions.MaxLength);
        _editor.MaxLines = 1;
        _editor.Watermark = "Press enter to search\u2026";
        _editor.Submitted += SearchSubmittedHandler;
        _editor.TextChanging += SearchChangingHandler;

        _rootFolder = new(this, true);

        Children.Add(_scroller);
        SetDock(_scroller, Dock.Bottom);
        _scroller.ContentMargin = ScrollMargin;
        _scroller.IsAutoScrollWhenPressed = true;
        _scroller.VerticalSpacing = ChromeSizes.StandardPx;
        _scroller.ScrollChanged += ScrollerChangedHandler;

        _noResults.Text = "No Results";
        _noResults.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        _noResults.Foreground = ChromeStyling.GrayForeground;
        _noResults.Margin = FolderHeader.Margin;

        _timer.Interval = ScheduleSpan;
        _timer.Tick += (_, __) => Rebuild();
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
    /// Gets the <see cref="GardenBasket"/> data source.
    /// </summary>
    public GardenBasket Basket;

    /// <summary>
    /// Gets the mission.
    /// </summary>
    public MainMission Mission { get; }

    /// <summary>
    /// Gets the folder header.
    /// </summary>
    public FolderHeader FolderHeader { get; }

    /// <summary>
    /// Gets the root history header.
    /// </summary>
    public FolderHeader RootHeader { get; }

    /// <summary>
    /// Gets the folder more (see all) bar.
    /// </summary>
    public MoreBar FolderMore { get; }

    /// <summary>
    /// Gets the root history more (see all) bar.
    /// </summary>
    public MoreBar HistoryMore { get; }

    /// <summary>
    /// Gets the item sort order.
    /// </summary>
    public GardenSort SortOrder { get; private set; } = GardenSort.UpdateNewestFirst;

    /// <summary>
    /// Gets number of folders from last update.
    /// </summary>
    public int FolderCount
    {
        get { return _folderCache.Count; }
    }

    /// <summary>
    /// Gets or sets whether searching.
    /// </summary>
    public bool IsSearching
    {
        get { return _isSearching; }

        set
        {
            if (_isSearching != value)
            {
                _isSearching = value;
                _searchChanged = false;

                _editor.Clear();
                _editor.IsVisible = value;
                _toolbar.SearchButton.IsChecked = value;
                _toolbar.Menu.NewFolderItem?.IsEnabled = !value;
                FolderHeader.NewFolder?.IsEnabled = !value;

                if (value)
                {
                    _editor.Focus();
                    //Garden.Focused?.IsFocused = false;
                }

                ScheduleRebuild();
            }
        }
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

    public void ToggleSearch()
    {
        IsSearching = !IsSearching;
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
    /// Show new folder window.
    /// </summary>
    public async void ShowNewFolderAsync()
    {
        var window = new NewFolderWindow(Basket);
        await window.ShowDialog(Mission);

        if (window.IsPositiveResult)
        {
            Basket.NewEmptyFolder(window.FolderName);
            ScheduleRebuild();
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
    /// Handles button key gestures.
    /// </summary>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        return _toolbar.HandleKeyGesture(e);
    }

    /// <summary>
    /// Schedule <see cref="Rebuild"/> after short delay.
    /// </summary>
    public void ScheduleRebuild()
    {
        _timer.Restart();
    }

    /// <summary>
    /// Rebuilds the view.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not expected</exception>
    public void Rebuild()
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(Rebuild)}";
        ConditionalDebug.WriteLine(NSpace, "UPDATING: " + Kind);

        _timer.Stop();

        if (!IsVisible)
        {
            ConditionalDebug.WriteLine(NSpace, "NOT VISIBLE: " + Kind);
            return;
        }

        GardenDeck? recent = null;
        List<Control>? buffer = null;

        // Null unless mission search active
        var findOpts = GetFindOptions();
        bool isSearching = IsSearching && findOpts?.Subtext != null;

        // COLLATE COLLATE FOLDERS
        int count = 0;
        var collation = Basket.CollateByFolder(findOpts, SortOrder);
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
            folder.StartRebuild(item.Value, isSearching);
            folders.Add(folder);

            if (findOpts != null)
            {
                folder.IsOpen = true;
            }

            count += item.Value.Count;
        }

        if (findOpts == null || newCache.Count > _folderCache.Count)
        {
            // Store for next time
            _folderCache = newCache;
        }

        // We have to sort before we can trim.
        folders.Sort(FolderCompare);
        FolderMore.IsMoreVisible = folders.Count > MaxDefaultFolders;

        if (!FolderMore.IsMoreChecked && folders.Count > MaxDefaultFolders)
        {
            // Trim what we add to visual tree
            ConditionalDebug.WriteLine(NSpace, $"Trim end folders");
            folders.RemoveRange(MaxDefaultFolders, folders.Count - MaxDefaultFolders);
        }

        foreach (var item in folders)
        {
            recent ??= item.FinishRebuild();
        }

        // We need a copy because type difference
        buffer = new(folders.Count + 4);
        buffer.Add(FolderHeader);
        buffer.AddRange(folders);

        FolderHeader.Count = collation.Count;

        if (collation.Count > MaxDefaultFolders)
        {
            // Add "see all" folders
            buffer.Add(FolderMore);
        }


        // ROOT HISTORY
        var root = Basket.GetFolderContents(null, findOpts, SortOrder) ??
            throw new InvalidOperationException("Null not expected");

        ConditionalDebug.WriteLine(NSpace, "Updating history");
        buffer.Add(RootHeader);
        buffer.Add(_rootFolder);
        RootHeader.Count = root.Count;
        count += root.Count;

        HistoryMore.IsMoreVisible = root.Count > MaxDefaultRootHistory;
        buffer.Add(HistoryMore);

        if (root.Count > MaxDefaultRootHistory && !HistoryMore.IsMoreChecked)
        {
            ConditionalDebug.WriteLine(NSpace, $"Trim root end");
            root.RemoveRange(MaxDefaultRootHistory, root.Count - MaxDefaultRootHistory);
        }

        _rootFolder.StartRebuild(root, isSearching);
        recent ??= _rootFolder.FinishRebuild();

        if (isSearching && count == 0)
        {
            buffer.Clear();
            buffer.Add(_noResults);
        }

        // UPDATE
        _scroller.Children.Replace(buffer);

        if (recent != null)
        {
            recent.IsFocused = true;
            SetCardChecked(recent);
        }
        else
        {
            Garden.Focused?.IsFocused = false;
        }

        // Need to set Find here,
        // MainMission invokes further event.
        GardenGrounds.Find = findOpts;
        ViewChanged?.Invoke(this, EventArgs.Empty);
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

        if (e.Key == Key.Home)
        {
            _scroller.ScrollToHome();
            return;
        }

        if (e.Key == Key.End)
        {
            _scroller.ScrollToEnd();
            return;
        }

        if (_toolbar.HandleKeyGesture(e))
        {
            return;
        }

        if (Garden.Focused?.VisualComponent is DeckCard card)
        {
            // Call the static method, not PixieControl.HandleKeyGesture()
            CardMenu.HandleKeyGesture(e, card);
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
                ScheduleRebuild();
                return;
            }

            _timer.Stop();
            IsSearching = false;
            return;
        }

        if (p == IsPinTopProperty)
        {
            var value = change.GetNewValue<bool>();
            _toolbar.Menu.PinTopItem.IsChecked = value;
            SortOrder = value ? GardenSort.UpdateNewestPinnedFirst : GardenSort.UpdateNewestFirst;
            ScheduleRebuild();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Basket.Changed += BasketChangedHandler;
        Garden.FocusChanged += FocusChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Basket.Changed -= BasketChangedHandler;
        Garden.FocusChanged -= FocusChangedHandler;
    }

    private static void SetCardChecked(GardenDeck? deck)
    {
        if (deck?.VisualComponent is DeckCard card)
        {
            card.IsChecked = deck.IsFocused;

            if (deck.IsFocused)
            {
                card.Group?.IsOpen = true;
            }
        }
    }

    private int FolderCompare(FolderView x, FolderView y)
    {
        if (FolderMore.IsAlphaFolderSort)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(x.FolderName, y.FolderName);
        }

        return y.Updated.CompareTo(x.Updated);
    }

    private FindOptions? GetFindOptions()
    {
        if (IsSearching && !string.IsNullOrEmpty(_editor.Text))
        {
            var opts = new FindOptions(_editor.Text);

            if (!_editor.IsMatchCaseChecked)
            {
                opts.Flags |= FindFlags.IgnoreCase;
            }

            if (_editor.IsMatchWordChecked)
            {
                opts.Flags |= FindFlags.Word;
            }

            return opts;
        }

        return null;
    }

    private void FocusChangedHandler(object? _, FocusChangedEventArgs e)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(FocusChangedHandler)}";

        if (IsVisible)
        {
            SetCardChecked(Basket.Recent);

            if (Basket.Contains(e.Previous))
            {
                // Needed as last select may be in different folder
                SetCardChecked(e.Previous);
            }
        }
    }

    private void BasketChangedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(BasketChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "BASKET: " + Kind);
        ScheduleRebuild();
    }

    private void ScrollerChangedHandler(object? _, ScrollChangedEventArgs e)
    {
        const string NSpace = $"{nameof(BasketView)}.{nameof(ScrollerChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"NormY: {_scroller.NormalizedY:R}");
        HistoryMore.IsGotoTopVisible = _scroller.NormalizedY > 0.0;
    }

    private void SearchSubmittedHandler(object? _, EventArgs __)
    {
        if (IsSearching && _searchChanged)
        {
            _searchChanged = false;
            ScheduleRebuild();
        }
    }

    private void SearchChangingHandler(object? _, EventArgs __)
    {
        _searchChanged = true;

        // Clear not search
        if (IsSearching && string.IsNullOrEmpty(_editor.Text))
        {
            ScheduleRebuild();
        }
    }

}