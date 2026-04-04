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
using System.Diagnostics;

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
    private static readonly MemoryGarden Garden = GardenGrounds.Global;
    private static readonly ImmutableSolidColorBrush DefaultUserBackground = new(ChromeBrushes.BlueAccent.Color, 0.15);

    private readonly DispatchCoalescer _coalescer = new(DispatcherPriority.Render);
    private readonly ScrollPanel _scroller = new();
    private ContentWidth _contentWidth;
    private IBrush? _userBackground = DefaultUserBackground;
    private CornerRadius _leafCornerRadius;
    private GardenDeck? _focusedDeck;
    private bool _focusChanged;
    private bool _findChanged;
    private BusyIndicator? _busyIndicator;

    private int _findN = -1;
    private DeckLeaf? _findView;

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
        ConditionalDebug.ThrowIfFalse(_scroller.AllowAutoHide);

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
    /// Gets or sets the background brush used for <see cref="LeafKind.User"/> messages.
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
    /// Gets whether the <see cref="DeckViewer"/> is highlighting the <see cref="GardenGrounds.Find"/> text.
    /// </summary>
    public bool IsFinding { get; private set; }

    /// <summary>
    /// Gets the total number of occurrences of <see cref="GardenGrounds.Find"/> text.
    /// </summary>
    /// <summary>
    /// The value is always 0 where <see cref="IsFinding"/> is false.
    /// </summary>
    public int FindCount { get; private set; }

    /// <summary>
    /// Gets the current "find" position where <see cref="FindCount"/> is greater than 0.
    /// </summary>
    /// <remarks>
    /// The range is [-1, <see cref="FindCount"/>], where -1 implies page top, and <see cref="FindCount"/> the page
    /// bottom. The value is invalid where <see cref="FindCount"/> is 0. Note that <see cref="FindCount"/> is not
    /// contiguous. It is not possible to navigate on level of underlying textual "Run" instances, but only message
    /// leaves themselves, hence it will jump over values when <see cref="PreviousOrHome"/> or <see
    /// cref="NextOrEnd"/> are called.
    /// </remarks>
    public int FindPos { get; private set; }

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
                    //_chatCoalescer.Post();
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
    /// Unwanted child.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    private new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Brings the previous item containing find text into view or, scrolls to top if no more find results available.
    /// </summary>
    /// <remarks>
    /// The result is true if the scroll area is at the top on return, or false if more "previous results" are
    /// available. The <see cref="Changed"/> event is always invoked.
    /// </remarks>
    public bool PreviousOrHome()
    {
        if (FindCount > 0)
        {
            var children = _scroller.Children;

            for (int n = _findN - 1; n > -1; --n)
            {
                if (children[n] is DeckLeaf leaf && leaf.FindCount != 0)
                {
                    _findN = n;
                    _findView = leaf;
                    FindPos -= leaf.FindCount;
                    leaf.BringIntoView();
                    Changed?.Invoke(this, EventArgs.Empty);
                    return false;
                }
            }
        }

        // Beyond range
        _findView = null;
        _findN = -1;
        FindPos = 0;

        // Scroll start
        _scroller.NormalizedY = 0.0;

        Changed?.Invoke(this, EventArgs.Empty);
        return false;
    }

    /// <summary>
    /// Brings the next item containing find text into view or, scrolls to bottom if no more find results available.
    /// </summary>
    /// <remarks>
    /// The result is true if the scroll area is at the bottom on return, or false if more "next results" are available.
    /// The <see cref="Changed"/> event is always invoked.
    /// </remarks>
    public bool NextOrEnd()
    {
        var children = _scroller.Children;

        if (FindCount > 0)
        {
            if (_findView != null)
            {
                FindPos += _findView.FindCount;
            }
            else
            {
                FindPos = 1;
            }

            for (int n = _findN + 1; n < children.Count; ++n)
            {
                if (children[n] is DeckLeaf leaf && leaf.FindCount != 0)
                {
                    _findN = n;
                    _findView = leaf;
                    leaf.BringIntoView();
                    Changed?.Invoke(this, EventArgs.Empty);
                    return false;
                }
            }
        }

        // Beyond range
        _findView = null;
        _findN = children.Count;
        FindPos = FindCount + 1;

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
            if (item is DeckLeaf leaf)
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
        GardenGrounds.FindChanged += FindHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Garden.FocusChanged -= FocusChangedHandler;
        Garden.FocusedUpdated -= FocusedUpdatedHandler;
        GardenGrounds.FindChanged -= FindHandler;
    }

    private void UpdateScrollerWidth(ContentWidth width)
    {
        ActualContentWidth = width.ToPixels() * Zoom.Fraction;
        _scroller.ContentMinWidth = ActualContentWidth;
        _scroller.ContentMaxWidth = ActualContentWidth;
    }

    private DeckLeaf? ResetFind()
    {
        var oldLeaf = _findView;
        _findN = -1;
        _findView = null;

        FindPos = 1;
        FindCount = 0;
        IsFinding = _focusedDeck?.Count > 0 && GardenGrounds.Find?.Subtext != null;

        return oldLeaf;
    }

    private void Rebuild(bool focused, bool finding)
    {
        // We limit it 1,000 currently.
        // Tested this with 10,000 messages. Using Task.Yield()
        // showed no demonstrable benefit. Searching adds overhead.
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(Rebuild)}";
        ConditionalDebug.WriteLine(NSpace, "REBUILD");

        var oldView = ResetFind();

        if (_focusedDeck == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Select none");
            _scroller.Children.Clear();
            return;
        }

        // Expect open
        // Setting GardenDeck.IsFocused should open it
        ConditionalDebug.ThrowIfFalse(_focusedDeck.IsLoaded);

        var src = _focusedDeck;
        var children = _scroller.Children;
        bool isScrollBottom = _scroller.IsBottom();
        var buffer = new List<Control>(src.Count + 2);

        for (int n = 0; n < src.Count; ++n)
        {
            var leaf = src[n];

            if (n < children.Count && children[n] is DeckLeaf view)
            {
                buffer.Add(view);
                view.Rebuild(leaf, focused, finding);
            }
            else
            {
                // Keep top clear of navigator
                view = new DeckLeaf(this, n == 0 ? DeckNavigator.MinimumHeight : 0.0);
                buffer.Add(view);
                view.Rebuild(leaf, focused, finding);
            }

            if (view.FindCount > 0)
            {
                if (_findView == null)
                {
                    _findN = n;
                    _findView = view;
                    FindPos = 1;
                    FindCount += view.FindCount;
                    continue;
                }

                if (!focused && view == oldView)
                {
                    _findN = n;
                    _findView = view;
                    FindPos = FindCount + 1;
                }

                FindCount += view.FindCount;
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
        ConditionalDebug.WriteLine(NSpace, $"Tracker count: {Tracker.Children.Count}");
    }

    private void ZoomChangedHandler(object? _, EventArgs __)
    {
        _scroller.ContentMaxWidth = ContentWidth.ToPixels() * Zoom.Fraction;
    }

    private void FindHandler(object? _, EventArgs __)
    {
        _findChanged = true;
        _coalescer.Post();
    }

    private void FocusChangedHandler(object? sender, FocusChangedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(FocusChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "SELECTED CHANGE RECEIVED");

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
        ConditionalDebug.WriteLine(NSpace, "SELECTED UPDATE RECEIVED");

        _focusedDeck = e.Current;
        _coalescer.Post();
    }

    private void PostedHandler(object? _, EventArgs __)
    {
        bool focused = _focusChanged;
        _focusChanged = false;

        bool finding = _findChanged;
        _findChanged = false;

        bool isBottom = _scroller.IsBottom();
        Rebuild(focused, finding);

        if ((focused || finding) && _findView != null)
        {
            Dispatcher.UIThread.Post(() => _findView.BringIntoView());
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