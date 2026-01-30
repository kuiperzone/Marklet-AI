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

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Main view of the <see cref="MemoryGarden.Selected"/> item.
/// </summary>
/// <remarks>
/// The <see cref="ContentViewer"/> is the main view of the <see cref="MemoryGarden.Selected"/> object, i.e. the chat
/// history of the selected item. This class is event driven, with the visible <see cref="GardenSession"/> item determined
/// by the current <see cref="MemoryGarden.Selected"/> value.
/// </remarks>
public partial class ContentViewer : MarkControl, ICrossTrackOwner
{
    private static readonly ImmutableSolidColorBrush DefaultUserBackground = new(ChromeBrushes.BlueAccent.Color, 0.15);

    private readonly DispatchCoalescer _sessionUpdater = new(DispatcherPriority.Render);
    private readonly DispatchCoalescer _propertyChanger = new(DispatcherPriority.Render);
    private readonly ScrollPanel _scroller = new();
    private readonly DispatcherTimer _scrollTimer = new();
    private ContentWidth _innerWidth;
    private IBrush? _userBackground = DefaultUserBackground;
    private CornerRadius _leafCornerRadius;
    private MemoryGarden? _garden;
    private double _pendingY = double.NaN;
    private GardenSession? _pendingSession;
    private BusyIndicator? _busyIndicator;
    private bool _isScrollingUpward;
    private bool _morphSession;

    /// <summary>
    /// Constructor.
    /// </summary>
    public ContentViewer()
    {
        Focusable = true;
        IsTabStop = false;

        QuoteItalic = true;
        MonoFamily = AppFonts.MonospaceFamily;

        _scroller.Focusable = false;
        _scroller.BringIntoViewOnFocusChange = false; // <- prevent jitter
        _scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

        // Difficult to fit to width without showing horizontal scroll if AutoHide is false
        ConditionalDebug.ThrowIfFalse(_scroller.AllowAutoHide);

        // Need to keep inner panel clear of right scrollbar.
        _scroller.ContentMargin = HorizontalOffset;
        _scroller.ContentMaxWidth = ContentWidth.ToPixels();
        _scroller.ContentMinWidth = ContentWidth.Narrow.ToPixels();

        // Handles vertical scrolling while selecting text
        _scrollTimer.Tick += ScrollTimerTickHandler;
        _scrollTimer.Interval = TimeSpan.FromMilliseconds(50);
        _scroller.PointerMoved += ScrollerPointerMovedHandler;
        _scroller.PointerReleased += ScrollerPointerReleasedHandler;
        _scroller.PointerExited += ScrollerPointerExitedHandler;
        _scroller.SizeChanged += ScrollerSizeChangedHandler;

        base.Child = _scroller;
        _propertyChanger.Posted += PropertyPostedHandler;
        _sessionUpdater.Posted += SessionUpdaterPostedHandler;
        Zoom.Changed += ZoomChangedHandler;
    }

    /// <summary>
    /// Offset used to align content.
    /// </summary>
    public static readonly Thickness HorizontalOffset = new(16.0, 0.0, 48.0, 0.0);

    /// <summary>
    /// Defines the <see cref="ContentWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<ContentViewer, ContentWidth> ContentWidthProperty =
        AvaloniaProperty.RegisterDirect<ContentViewer, ContentWidth>(nameof(ContentWidth),
        o => o.ContentWidth, (o, v) => o.ContentWidth = v);

    /// <summary>
    /// Defines the <see cref="UserBackground"/> property.
    /// </summary>
    public static readonly DirectProperty<ContentViewer, IBrush?> UserBackgroundProperty =
        AvaloniaProperty.RegisterDirect<ContentViewer, IBrush?>(nameof(UserBackground),
        o => o.UserBackground, (o, v) => o.UserBackground = v, DefaultUserBackground);

    /// <summary>
    /// Defines the <see cref="LeafCornerRadius"/> property.
    /// </summary>
    public static readonly DirectProperty<ContentViewer, CornerRadius> LeafCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<ContentViewer, CornerRadius>(nameof(LeafCornerRadius),
        o => o.LeafCornerRadius, (o, v) => o.LeafCornerRadius = v);

    /// <summary>
    /// Gets or sets the "inner width" as a <see cref="Shared.ContentWidth"/> value.
    /// </summary>
    /// <remarks>
    /// This the base class Width value is expected to be NaN and stretched.
    /// </remarks>
    public ContentWidth ContentWidth
    {
        get { return _innerWidth; }
        set { SetAndRaise(ContentWidthProperty, ref _innerWidth, value); }
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
    /// Gets the shared <see cref="CrossTracker"/> instance.
    /// </summary>
    public CrossTracker Tracker { get; } = new();

    /// <summary>
    /// Gets or sets the <see cref="MemoryGarden"/> instance.
    /// </summary>
    /// <remarks>
    /// The <see cref="Garden"/> property is initially null and must be created, opened and assigned externally.
    /// </remarks>
    public MemoryGarden? Garden
    {
        get { return _garden; }

        set
        {
            if (_garden != value)
            {
                if (_garden != null)
                {
                    _garden.SelectedChanged -= SelectedChangedHandler;
                    _garden.SelectedUpdated -= SelectedUpdatedHandler;
                }

                _garden = value;
                UpdateSession(value?.Selected);

                if (value != null)
                {
                    value.SelectedChanged += SelectedChangedHandler;
                    value.SelectedUpdated += SelectedUpdatedHandler;
                }
            }
        }
    }

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
                if (_busyIndicator == null)
                {
                    bool bottom = _scroller.IsBottom();

                    _busyIndicator = new();
                    _scroller.Children.Add(_busyIndicator);

                    if (bottom)
                    {
                        _scroller.ScrollToEnd();
                    }

                    return;
                }

                _scroller.Children.Remove(_busyIndicator);
                _busyIndicator = null;
            }
        }
    }

    /// <summary>
    /// Gets the actual internal content width in pixels.
    /// </summary>
    public double ActualContentWidth
    {
        get { return _scroller.ActualContentWidth; }
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

    /// <summary>
    /// Handles key input from parent window.
    /// </summary>
    public bool HandleKey(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(ContentViewer)}.{nameof(HandleKey)}";
        ConditionalDebug.WriteLine(NSpace, $"KEY: {e.Key}, {e.KeyModifiers}");

        if (e.Handled)
        {
            ConditionalDebug.WriteLine(NSpace, "Already handled");
            return false;
        }

        if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control && Tracker.HasValidSelection)
        {
            e.Handled = true;
            ConditionalDebug.WriteLine(NSpace, $"CTRL-C accepted");

            Tracker.CopyText(WhatText.SelectedOrNull);
            return true;
        }

        // Only one instance to act on this key press
        if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            ConditionalDebug.WriteLine(NSpace, $"CTRL-A accepted");

            Tracker.SelectAll();
            return true;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None && Tracker.HasValidSelection)
        {
            e.Handled = true;
            ConditionalDebug.WriteLine(NSpace, $"ESCAPE accepted");

            Tracker.SelectNone();
            return true;
        }

        if (Zoom.HandleKeyGesture(e))
        {
            ConditionalDebug.ThrowIfFalse(e.Handled);
            return true;
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

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // IMPORTANT.
        // No need for properties which affect children.
        // Children will take care of themselves provided
        // we call the base class.
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChildProperty && change.OldValue != null)
        {
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }

        if (p == ContentWidthProperty && base.Child != null)
        {
            _scroller.ContentMaxWidth = ContentWidth.ToPixels() * Zoom.Fraction;
            return;
        }

        if (p == ContentWidthProperty || p == UserBackgroundProperty || p == LeafCornerRadiusProperty)
        {
            _morphSession = true;
            _propertyChanger.Post();
            return;
        }

        if (IsMarkControlProperty(p))
        {
            _propertyChanger.Post();
            return;
        }
    }

    private ContentLeaf NewBlock(GardenLeaf leaf)
    {
        const string NSpace = $"{nameof(ContentViewer)}.{nameof(NewBlock)}";
        ConditionalDebug.WriteLine(NSpace, $"Leaf kind: {leaf.Kind}");

        var block = new ContentLeaf(this);
        block.Update(leaf);
        return block;
    }

    private void UpdateSession(GardenSession? instance)
    {
        const string NSpace = $"{nameof(ContentViewer)}.{nameof(UpdateSession)}";
        ConditionalDebug.WriteLine(NSpace, "Updating");

        var children = _scroller.Children;

        if (instance == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Select none");
            children.Clear();
            return;
        }

        // Expect open
        // Setting Session.IsSelected should open it
        ConditionalDebug.ThrowIfFalse(instance.IsOpen);

        int childCount = children.Count;
        bool isScrollBottom = _scroller.IsBottom();
        var buffer = new List<Control>(instance.Count + 1);

        for (int n = 0; n < instance.Count; ++n)
        {
            var leaf = instance[n];

            if (n < childCount)
            {
                if (children[n] is ContentLeaf view)
                {
                    buffer.Add(view);
                    view.Update(leaf);
                }

                continue;
            }

            buffer.Add(NewBlock(leaf));
        }

        if (IsBusy)
        {
            _busyIndicator ??= new();
            buffer.Add(_busyIndicator);
        }

        children.Replace(buffer);

        if (isScrollBottom)
        {
            // Keep at end
            ScrollToEnd();
        }
    }

    private void SelectedChangedHandler(object? _, SelectedChangedEventArgs e)
    {
        const string NSpace = $"{nameof(ContentViewer)}.{nameof(SelectedChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "SELECTED CHANGE RECEIVED");

        if (e.Previous != null)
        {
            e.Previous.VisualScrollPos = _scroller.NormalizedY;
            ConditionalDebug.WriteLine(NSpace, $"Previous scroll: {e.Previous.VisualScrollPos}");
        }

        _pendingSession = e.Selected;

        if (_pendingSession != null)
        {
            _pendingY = _pendingSession.VisualScrollPos;
            _sessionUpdater.Post();
            return;
        }

        // Clear
        UpdateSession(null);
        _pendingY = double.NaN;
    }

    private void SelectedUpdatedHandler(object? _, SelectedUpdatedEventArgs e)
    {
        const string NSpace = $"{nameof(ContentViewer)}.{nameof(SelectedChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "SELECTED UPDATE RECEIVED");

        _pendingSession = e.Selected;
        _sessionUpdater.Post();
    }

    private void ScrollerPointerMovedHandler(object? _, PointerEventArgs e)
    {
        const string NSpace = $"{nameof(ContentViewer)}.{nameof(ScrollerPointerMovedHandler)}";

        if (_scroller.Extent.Height > _scroller.Viewport.Height && Tracker.HasValidSelection)
        {
            var info = e.GetCurrentPoint(_scroller);
            var props = info.Properties;

            if (props.IsLeftButtonPressed)
            {
                var point = info.Position;

                if (point.Y < 0 || point.Y >= _scroller.Bounds.Height)
                {
                    _isScrollingUpward = point.Y < 0;

                    _scrollTimer.Restart();
                    ScrollTimerTickHandler(null, EventArgs.Empty);
                    return;
                }
            }
        }

        _scrollTimer.Stop();
    }

    private void ScrollerPointerReleasedHandler(object? _, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            _scrollTimer.Stop();
        }
    }

    private void ScrollerPointerExitedHandler(object? _, PointerEventArgs e)
    {
        _scrollTimer.Stop();
    }

    private void ScrollerSizeChangedHandler(object? _, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width != e.PreviousSize.Width)
        {
            var width = _scroller.ActualContentWidth;

            foreach (var item in _scroller.Children)
            {
                if (item is ContentLeaf leaf)
                {
                    leaf.UpdateWidthLimit(width);
                }
            }
        }
    }

    private void ScrollTimerTickHandler(object? _, EventArgs __)
    {
        var offY = _scroller.Offset.Y;

        if (_isScrollingUpward)
        {
            _scroller.LineUp();
        }
        else
        {
            _scroller.LineDown();
        }

        if (offY == _scroller.Offset.Y || _scroller.Bounds.Height < 3.0)
        {
            _scrollTimer.Stop();
            return;
        }
    }

    private void ZoomChangedHandler(object? _, EventArgs __)
    {
        _scroller.ContentMaxWidth = ContentWidth.ToPixels() * Zoom.Fraction;
        _propertyChanger.Post();
    }

    private void SessionUpdaterPostedHandler(object? _, EventArgs __)
    {
        if (_pendingSession != null)
        {
            try
            {
                UpdateSession(_pendingSession);
                _scroller.NormalizedY = _pendingY;
            }
            finally
            {
                _pendingY = double.NaN;
                _pendingSession = null;
            }
        }
    }

    private void PropertyPostedHandler(object? _, EventArgs __)
    {
        bool morph = _morphSession;
        _morphSession = false;

        foreach (var item in _scroller.Children)
        {
            if (item is ContentLeaf leaf)
            {
                leaf.Refresh(morph);
            }
        }
    }
}