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
/// Main view of the <see cref="MemoryGarden.Current"/> item.
/// </summary>
/// <remarks>
/// The <see cref="DeckViewer"/> is the main view of the <see cref="MemoryGarden.Current"/> object, i.e. the session
/// history of the selected item. This class is event driven, with the visible <see cref="GardenDeck"/> item determined
/// by the current <see cref="MemoryGarden.Current"/> value.
/// </remarks>
public partial class DeckViewer : MarkControl, ICrossTrackOwner
{
    private static readonly ImmutableSolidColorBrush DefaultUserBackground = new(ChromeBrushes.BlueAccent.Color, 0.15);

    private readonly DispatchCoalescer _deckCoalescer = new(DispatcherPriority.Render);
    private readonly ScrollPanel _scroller = new();
    private readonly DispatcherTimer _scrollTimer = new();
    private ContentWidth _contentWidth;
    private IBrush? _userBackground = DefaultUserBackground;
    private CornerRadius _leafCornerRadius;
    private MemoryGarden? _garden;
    private double _pendingY = double.NaN;
    private GardenDeck? _currentDeck;
    private BusyIndicator? _busyIndicator;
    private bool _isScrollingUpward;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DeckViewer()
    {
        Focusable = true;
        IsTabStop = false;
        ActualContentWidth = _contentWidth.ToPixels() * Zoom.Fraction;

        _scroller.Focusable = false;
        _scroller.BringIntoViewOnFocusChange = false; // <- prevent jitter
        _scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        UpdateScrollerWidth(ContentWidth);

        // Difficult to fit to width without showing horizontal scroll if AutoHide is false
        ConditionalDebug.ThrowIfFalse(_scroller.AllowAutoHide);

        // Handles vertical scrolling while selecting text
        _scrollTimer.Tick += ScrollTimerTickHandler;
        _scrollTimer.Interval = TimeSpan.FromMilliseconds(50);
        _scroller.PointerMoved += ScrollerPointerMovedHandler;
        _scroller.PointerReleased += ScrollerPointerReleasedHandler;
        _scroller.PointerExited += ScrollerPointerExitedHandler;

        Zoom.Changed += ZoomChangedHandler;
        _deckCoalescer.Posted += DeckUpdatePostedHandler;

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
                    _garden.CurrentChanged -= CurrentChangedHandler;
                    _garden.CurrentUpdated -= CurrentUpdatedHandler;
                }

                _garden = value;
                UpdateDeck(value?.Current);

                if (value != null)
                {
                    value.CurrentChanged += CurrentChangedHandler;
                    value.CurrentUpdated += CurrentUpdatedHandler;
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
                if (value && _currentDeck != null)
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

    /// <inheritdoc cref="MarkControl.ImmediateRefresh"/>
    protected override void ImmediateRefresh()
    {
        UpdateScrollerWidth(ContentWidth);

        foreach (var item in _scroller.Children)
        {
            if (item is DeckLeaf leaf)
            {
                leaf.ImmediateRefresh();
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
            if (HasChild)
            {
                PropertyCoalescer?.Post();
            }

            return;
        }
    }

    private DeckLeaf NewBlock(GardenLeaf leaf)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(NewBlock)}";
        ConditionalDebug.WriteLine(NSpace, $"Leaf kind: {leaf.Kind}");

        var block = new DeckLeaf(this);
        block.Update(leaf);
        return block;
    }

    private void UpdateScrollerWidth(ContentWidth width)
    {
        ActualContentWidth = width.ToPixels() * Zoom.Fraction;
        _scroller.ContentMinWidth = ActualContentWidth;
        _scroller.ContentMaxWidth = ActualContentWidth;
    }

    private void UpdateDeck(GardenDeck? src)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(UpdateDeck)}";
        ConditionalDebug.WriteLine(NSpace, "Updating");

        var children = _scroller.Children;

        if (src == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Select none");
            children.Clear();
            return;
        }

        // Expect open
        // Setting GardenDeck.IsCurrent should open it
        ConditionalDebug.ThrowIfFalse(src.IsLoaded);

        int currentCount = children.Count;
        bool isScrollBottom = _scroller.IsBottom();
        var buffer = new List<Control>(src.Count + 1);

        for (int n = 0; n < src.Count; ++n)
        {
            var leaf = src[n];

            if (n < currentCount && children[n] is DeckLeaf view)
            {
                buffer.Add(view);
                view.Update(leaf);
                continue;
            }

            buffer.Add(NewBlock(leaf));
        }

        if (_busyIndicator != null)
        {
            buffer.Add(_busyIndicator);
            _busyIndicator = null;
        }

        children.Replace(buffer);

        if (isScrollBottom)
        {
            // Keep at end
            ScrollToEnd();
        }
    }

    private void CurrentChangedHandler(object? _, CurrentDeckChangedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(CurrentChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "SELECTED CHANGE RECEIVED");
        Tracker.SelectNone();

        _currentDeck = e.Current;

        if (_currentDeck != null)
        {
            _pendingY = 1.0;
            _deckCoalescer.Post();
            return;
        }

        // Clear
        UpdateDeck(null);
        _pendingY = double.NaN;
    }

    private void CurrentUpdatedHandler(object? _, CurrentDeckUpdatedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(CurrentChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "SELECTED UPDATE RECEIVED");

        _currentDeck = e.Current;
        _deckCoalescer.Post();
    }

    private void ScrollerPointerMovedHandler(object? _, PointerEventArgs e)
    {
        const string NSpace = $"{nameof(DeckViewer)}.{nameof(ScrollerPointerMovedHandler)}";

        if (_scroller.Extent.Height > _scroller.Viewport.Height && Tracker.SelectionCount != 0)
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
    }

    private void DeckUpdatePostedHandler(object? _, EventArgs __)
    {
        if (_currentDeck != null)
        {
            try
            {
                UpdateDeck(_currentDeck);
                _scroller.NormalizedY = _pendingY;
            }
            finally
            {
                _pendingY = double.NaN;
                _currentDeck = null;
            }
        }
    }

}