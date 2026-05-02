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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using KuiperZone.Marklet.Stack.Garden;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using Avalonia.Threading;
using Avalonia.Input;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Controls.Internal;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Main view of the <see cref="MemoryGarden.Focused"/> item.
/// </summary>
/// <remarks>
/// The <see cref="DeckViewer"/> is the main view of the <see cref="MemoryGarden.Focused"/> object, i.e. the session
/// history of the selected item. This class is event driven, with the visible <see cref="GardenDeck"/> item determined
/// by the current <see cref="MemoryGarden.Focused"/> value.
/// </remarks>
public sealed class DeckViewer : MarkControl, ICrossTrackOwner
{
    private static readonly MemoryGarden Garden = GlobalGarden.Global;
    private static readonly ImmutableSolidColorBrush DefaultUserBackground = new(ChromeBrushes.BlueAccent.Color, 0.15);

    private readonly DispatchCoalescer _coalescer = new(DispatcherPriority.Render);
    private readonly ScrollPanel _scroller = new();
    private ContentWidth _contentWidth;
    private IBrush? _userBackground = DefaultUserBackground;
    private CornerRadius _leafCornerRadius;
    private GardenDeck? _focusedDeck;
    private bool _focusChanged;
    private bool _searchChanged;
    private BusyIndicator? _busyIndicator;

    private int _keywordN = -1;
    private LeafView? _keywordLeaf;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DeckViewer()
    {
        Focusable = true;
        IsTabStop = false;
        ActualContentWidth = _contentWidth.ToPixels() * Zoom.Fraction;

        _scroller.Focusable = false;
        _scroller.IsAutoScrollWhenPressed = true;
        _scroller.BringIntoViewOnFocusChange = false; // <- prevent jitter
        _scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        UpdateScrollerWidth(ContentWidth);

        // Difficult to fit to width without showing horizontal scroll if AutoHide is false
        Diag.ThrowIfFalse(_scroller.AllowAutoHide);

        Zoom.Changed += ZoomChangedHandler;
        _coalescer.Posted += PostedHandler;

        base.Child = _scroller;
    }

    /// <summary>
    /// Defines the <see cref="ContentWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<DeckViewer, ContentWidth> ContentWidthProperty =
        AvaloniaProperty.RegisterDirect<DeckViewer, ContentWidth>(nameof(ContentWidth),
        o => o.ContentWidth, (o, v) => o.ContentWidth = v);

    /// <summary>
    /// Defines the <see cref="UserBackground"/> property.
    /// </summary>
    public static readonly DirectProperty<DeckViewer, IBrush?> UserBackgroundProperty =
        AvaloniaProperty.RegisterDirect<DeckViewer, IBrush?>(nameof(UserBackground),
        o => o.UserBackground, (o, v) => o.UserBackground = v, DefaultUserBackground);

    /// <summary>
    /// Defines the <see cref="LeafCornerRadius"/> property.
    /// </summary>
    public static readonly DirectProperty<DeckViewer, CornerRadius> LeafCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<DeckViewer, CornerRadius>(nameof(LeafCornerRadius),
        o => o.LeafCornerRadius, (o, v) => o.LeafCornerRadius = v);

    /// <summary>
    /// Occurs when content changes include search results.
    /// </summary>
    public event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Gets or sets the "inner width" as a <see cref="Shared.ContentWidth"/> value.
    /// </summary>
    /// <remarks>
    /// This the base class Width value is expected to be NaN and stretched.
    /// </remarks>
    public ContentWidth ContentWidth
    {
        get { return _contentWidth; }
        set { SetAndRaise(ContentWidthProperty, ref _contentWidth, value); }
    }

    /// <summary>
    /// Gets or sets the background brush used for <see cref="LeafFormat.UserMessage"/> messages.
    /// </summary>
    public IBrush? UserBackground
    {
        get { return _userBackground; }
        set { SetAndRaise(UserBackgroundProperty, ref _userBackground, value); }
    }

    /// <summary>
    /// Gets or sets border radius in pixels of message leaf borders.
    /// </summary>
    public CornerRadius LeafCornerRadius
    {
        get { return _leafCornerRadius; }
        set { SetAndRaise(LeafCornerRadiusProperty, ref _leafCornerRadius, value); }
    }

    /// <summary>
    /// Gets whether the <see cref="DeckViewer"/> is highlighting the <see cref="GlobalGarden.Search"/> text.
    /// </summary>
    public bool IsSearching { get; private set; }

    /// <summary>
    /// Gets the total number of occurrences of <see cref="GlobalGarden.Search"/> text.
    /// </summary>
    /// <summary>
    /// The value is always 0 where <see cref="IsSearching"/> is false.
    /// </summary>
    public int KeywordCount { get; private set; }

    /// <summary>
    /// Gets the current "keyword" position where <see cref="KeywordCount"/> is greater than 0.
    /// </summary>
    /// <remarks>
    /// The range is [-1, <see cref="KeywordCount"/>], where -1 implies page top, and <see cref="KeywordCount"/> the page
    /// bottom. The value is invalid where <see cref="KeywordCount"/> is 0. Note that <see cref="KeywordCount"/> is not
    /// contiguous. It is not possible to navigate on level of underlying textual "Run" instances, but only message
    /// leaves themselves, hence it will jump over values when <see cref="PreviousKeywordOrHome"/> or <see
    /// cref="NextKeywordOrEnd"/> are called.
    /// </remarks>
    public int KeywordPos { get; private set; }

    /// <summary>
    /// TBD
    /// </summary>
    public bool IsBusy
    {
        get { return _busyIndicator != null; }

        set
        {
            if (IsBusy != value)
            {
                if (value && _focusedDeck != null)
                {
                    _busyIndicator = new();
                    return;
                }

                if (_busyIndicator != null)
                {
                    _scroller.Children.Remove(_busyIndicator);
                    _busyIndicator = null;
                }
            }
        }
    }

    /// <summary>
    /// Gets the actual internal content width in pixels.
    /// </summary>
    public double ActualContentWidth { get; private set; }

    /// <summary>
    /// Gets the currently focused kind.
    /// </summary>
    public DeckFormat FocusedKind
    {
        get { return _focusedDeck?.Format ?? DeckFormat.None; }
    }

    /// <summary>
    /// Unwanted child.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    private new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Brings the previous item containing keyword text into view or, scrolls to top if no more search results available.
    /// </summary>
    /// <remarks>
    /// The result is true if the scroll area is at the top on return, or false if more "previous results" are
    /// available. The <see cref="Changed"/> event is always invoked.
    /// </remarks>
    public bool PreviousKeywordOrHome()
    {
        if (KeywordCount > 0)
        {
            var children = _scroller.Children;

            for (int n = _keywordN - 1; n > -1; --n)
            {
                if (children[n] is LeafView leaf && leaf.SearchCount != 0)
                {
                    _keywordN = n;
                    _keywordLeaf = leaf;
                    KeywordPos -= leaf.SearchCount;
                    leaf.BringIntoView();
                    Changed?.Invoke(this, EventArgs.Empty);
                    return false;
                }
            }
        }

        // Beyond range
        _keywordLeaf = null;
        _keywordN = -1;
        KeywordPos = 0;

        // Scroll start
        _scroller.NormalizedY = 0.0;

        Changed?.Invoke(this, EventArgs.Empty);
        return false;
    }

    /// <summary>
    /// Brings the next item containing keyword text into view or, scrolls to bottom if no more keyword results available.
    /// </summary>
    /// <remarks>
    /// The result is true if the scroll area is at the bottom on return, or false if more "next results" are available.
    /// The <see cref="Changed"/> event is always invoked.
    /// </remarks>
    public bool NextKeywordOrEnd()
    {
        var children = _scroller.Children;

        if (KeywordCount > 0)
        {
            if (_keywordLeaf != null)
            {
                KeywordPos += _keywordLeaf.SearchCount;
            }
            else
            {
                KeywordPos = 1;
            }

            for (int n = _keywordN + 1; n < children.Count; ++n)
            {
                if (children[n] is LeafView leaf && leaf.SearchCount != 0)
                {
                    _keywordN = n;
                    _keywordLeaf = leaf;
                    leaf.BringIntoView();
                    Changed?.Invoke(this, EventArgs.Empty);
                    return false;
                }
            }
        }

        // Beyond range
        _keywordLeaf = null;
        _keywordN = children.Count;
        KeywordPos = KeywordCount + 1;

        // Scroll end
        _scroller.NormalizedY = 1.0;

        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Scroll to home.
    /// </summary>
    public void ScrollToHome()
    {
        _scroller.NormalizedY = 0.0;
    }

    /// <summary>
    /// Scroll to end.
    /// </summary>
    public void ScrollToEnd()
    {
        _scroller.NormalizedY = 1.0;
    }

    /// <see cref="MarkControl.HandleKeyGesture(KeyEventArgs)"/>
    public override bool HandleKeyGesture(KeyEventArgs e)
    {
        if (base.HandleKeyGesture(e))
        {
            return true;
        }

        if (e.Handled)
        {
            return false;
        }

        if (e.KeyModifiers != KeyModifiers.None)
        {
            return false;
        }

        switch (e.Key)
        {
            case Key.Up:
                e.Handled = true;
                _scroller.LineUp();
                return true;
            case Key.Down:
                e.Handled = true;
                _scroller.LineDown();
                return true;
            case Key.PageUp:
                e.Handled = true;
                _scroller.PageUp();
                return true;
            case Key.PageDown:
                e.Handled = true;
                _scroller.PageDown();
                return true;
            case Key.Home:
                e.Handled = true;
                _scroller.ScrollToHome();
                return true;
            case Key.End:
                e.Handled = true;
                _scroller.ScrollToEnd();
                return true;
            default:
                return false;
        }
    }

    /// <inheritdoc cref="MarkControl.PropertyRefresh"/>
    protected override void PropertyRefresh()
    {
        UpdateScrollerWidth(ContentWidth);

        foreach (var item in _scroller.Children)
        {
            if (item is LeafView leaf)
            {
                leaf.OwnerRefresh();
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

        if (p == ContentWidthProperty || p == UserBackgroundProperty || p == LeafCornerRadiusProperty)
        {
            PropertyCoalescer?.Post();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Garden.FocusChanged += FocusChangedHandler;
        Garden.FocusedUpdated += FocusedUpdatedHandler;
        GlobalGarden.SearchChanged += SearchHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Garden.FocusChanged -= FocusChangedHandler;
        Garden.FocusedUpdated -= FocusedUpdatedHandler;
        GlobalGarden.SearchChanged -= SearchHandler;
    }

    private void UpdateScrollerWidth(ContentWidth width)
    {
        ActualContentWidth = width.ToPixels() * Zoom.Fraction;
        _scroller.ContentMinWidth = ActualContentWidth;
        _scroller.ContentMaxWidth = ActualContentWidth;
    }

    private LeafView? ResetSearch()
    {
        var oldLeaf = _keywordLeaf;
        _keywordN = -1;
        _keywordLeaf = null;

        KeywordPos = 1;
        KeywordCount = 0;
        IsSearching = _focusedDeck?.Count > 0 && GlobalGarden.Search?.Keyword != null;

        return oldLeaf;
    }

    private void Rebuild(bool focused, bool searching)
    {
        // We limit it 1,000 currently.
        // Tested this with 10,000 messages. Using Task.Yield()
        // showed no demonstrable benefit. Searching adds overhead.
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(Rebuild)}";
        Diag.WriteLine(NSpace, "REBUILD");

        var oldView = ResetSearch();

        if (_focusedDeck == null)
        {
            Diag.WriteLine(NSpace, "Select none");
            _scroller.Children.Clear();
            return;
        }

        // Expect open
        // Setting GardenDeck.IsFocused should open it
        Diag.ThrowIfFalse(_focusedDeck.IsOpen);

        var src = _focusedDeck;
        var children = _scroller.Children;
        bool isScrollBottom = _scroller.IsBottom();
        var buffer = new List<Control>(src.Count + 2);

        for (int n = 0; n < src.Count; ++n)
        {
            var leaf = src[n];

            if (n < children.Count && children[n] is LeafView view)
            {
                buffer.Add(view);
                view.Rebuild(leaf, focused, searching);
            }
            else
            {
                // Keep top clear of navigator
                view = new LeafView(this, n == 0 ? DeckNavigator.MinimumHeight : 0.0);
                buffer.Add(view);
                view.Rebuild(leaf, focused, searching);
            }

            if (view.SearchCount > 0)
            {
                if (_keywordLeaf == null)
                {
                    _keywordN = n;
                    _keywordLeaf = view;
                    KeywordPos = 1;
                    KeywordCount += view.SearchCount;
                    continue;
                }

                if (!focused && view == oldView)
                {
                    _keywordN = n;
                    _keywordLeaf = view;
                    KeywordPos = KeywordCount + 1;
                }

                KeywordCount += view.SearchCount;
            }
        }

        if (_busyIndicator != null)
        {
            buffer.Add(_busyIndicator);
            _busyIndicator = null;
        }

        children.Replace(buffer);
        children.TrimCapacity();

        // Ensure children are removed from Tracker, otherwise will have memory leak.
        Diag.WriteLine(NSpace, $"Tracker count: {Tracker.Children.Count}");
    }

    private void ZoomChangedHandler(object? _, EventArgs __)
    {
        _scroller.ContentMaxWidth = ContentWidth.ToPixels() * Zoom.Fraction;
    }

    private void SearchHandler(object? _, EventArgs __)
    {
        _searchChanged = true;
        _coalescer.Post();
    }

    private void FocusChangedHandler(object? sender, FocusChangedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(FocusChangedHandler)}";
        Diag.WriteLine(NSpace, "SELECTED CHANGE RECEIVED");

        Tracker.SelectNone();

        if (e.Current != null)
        {
            _focusedDeck = e.Current;
            _focusChanged = true;
            _coalescer.Post();
            return;
        }

        // Clear
        _focusedDeck = null;
        _focusChanged = false;
        _coalescer.Cancel();
        Rebuild(false, false);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void FocusedUpdatedHandler(object? sender, FocusedUpdatedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(FocusChangedHandler)}";
        Diag.WriteLine(NSpace, "SELECTED UPDATE RECEIVED");

        _focusedDeck = e.Current;
        _coalescer.Post();
    }

    private void PostedHandler(object? _, EventArgs __)
    {
        bool focused = _focusChanged;
        _focusChanged = false;

        bool searching = _searchChanged;
        _searchChanged = false;

        bool isBottom = _scroller.IsBottom();
        Rebuild(focused, searching);

        if ((focused || searching) && _keywordLeaf != null)
        {
            Dispatcher.UIThread.Post(() => _keywordLeaf.BringIntoView());
            Changed?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (isBottom || focused)
        {
            _scroller.NormalizedY = 1.0;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }
}