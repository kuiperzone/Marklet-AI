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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.PixieChrome.Windows.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Base class for application windows with custom CSD chrome.
/// </summary>
/// <remarks>
/// The <see cref="ChromeWindow"/> optionally implements its own chrome, as defined on construction. Otherwise, the
/// class behaves as regular <see cref="Window"/>.
/// </remarks>
public class ChromeWindow : Window
{
    /// <summary>
    /// Shortcut to <see cref="ChromeStyling.Global"/>.
    /// </summary>
    protected static readonly ChromeStyling Styling = ChromeStyling.Global;

    private const int GrabPixels = 8;
    private const int TitleRow = 0;
    private const int ContentRow = 1;
    private const int DeactiveZ = int.MaxValue;

    private readonly bool _initialized;
    private readonly Grid _contentGrid = new();
    private readonly Border _outerWrap = new();
    private readonly Border _innerWrap = new();
    private readonly ChromeBar _chromeBar;

    private ChromeContextMenu? _chromeContext;
    private bool _chromeWindowLast;
    private Control? _contentSource;
    private PixelPoint _closePosition;
    private Point _dragResizePoint = new(double.NegativeInfinity, double.NegativeInfinity);
    private bool _isSettingBase;
    private Cursor? _cursor;

    private Border? _deactiveOverlay;

    // Direct properties
    private const bool DefaultIsDimmable = true;
    private bool _isChromeAlwaysVisible;
    private bool _isEscapeToClose;
    private bool _isDimmable = DefaultIsDimmable;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// The <see cref="Settings"/> use <see cref="WindowSettings.Global"/> instance. The "isDialog" flag indicates that
    /// this is to be shown using <see cref="Window.ShowDialog(Window)"/>, but does not enforce it. Passing "isDialog"
    /// true initializes StartupLocation, CanMinimize, CanMaximize, <see cref="IsEscapeToClose"/> etc. These may be
    /// changed after construction.
    /// </remarks>
    public ChromeWindow(bool isDialog = false)
        : this(WindowSettings.Global, isDialog)
    {
    }

    /// <summary>
    /// Constructor with custom <see cref="WindowSettings"/> instance.
    /// </summary>
    public ChromeWindow(WindowSettings settings, bool isDialog)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.Constructor";
        ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");
        FontSize = ChromeFonts.DefaultFontSize;
        FontFamily = ChromeFonts.DefaultFamily;

        if (isDialog)
        {
            CanMinimize = false;
            CanMaximize = false;
            IsEscapeToClose = true;
            IsChromeAlwaysVisible = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        // Set early
        Settings = settings;
        IsChromeWindow = Settings.GetChromeWindow(isDialog);
        ShowInTaskbar = Settings.GetShowInTaskbar(isDialog);

        _contentGrid.RowDefinitions.Add(new(GridLength.Auto));
        _contentGrid.RowDefinitions.Add(new(GridLength.Star));

        _chromeBar = new ChromeBar(this);
        ChromeBar = _chromeBar;

        Grid.SetRow(_chromeBar, TitleRow);
        _contentGrid.Children.Add(_chromeBar);

        _innerWrap.Child = _contentGrid;
        _innerWrap.ClipToBounds = true;

        _outerWrap.Child = _innerWrap;
        _outerWrap.Background = Styling.Background;

        // FIRST TIME SET
        // Set on base intentional.
        base.Content = _outerWrap;
        base.Background = Brushes.Transparent;

        // Expect this (this is a "unit test")
        ConditionalDebug.ThrowIfNotNull(_contentSource);

        Activated += ActivatedHandler;
        Deactivated += DeactivatedHandler;
        Closing += ClosingHandler;

        OnWindowSettingsChanged(isDialog);
        _initialized = true;
    }

    /// <summary>
    /// Defines the <see cref="IsEscapeToClose"/> property.
    /// </summary>
    public static readonly DirectProperty<ChromeWindow, bool> IsChromeAlwaysVisibleProperty =
        AvaloniaProperty.RegisterDirect<ChromeWindow, bool>(nameof(IsChromeAlwaysVisible),
        o => o.IsChromeAlwaysVisible, (o, v) => o.IsChromeAlwaysVisible = v);

    /// <summary>
    /// Defines the <see cref="IsEscapeToClose"/> property.
    /// </summary>
    public static readonly DirectProperty<ChromeWindow, bool> IsEscapeToCloseProperty =
        AvaloniaProperty.RegisterDirect<ChromeWindow, bool>(nameof(IsEscapeToClose),
        o => o.IsEscapeToClose, (o, v) => o.IsEscapeToClose = v);

    /// <summary>
    /// Defines the <see cref="IsDimmable"/> property.
    /// </summary>
    public static readonly DirectProperty<ChromeWindow, bool> IsDimmableProperty =
        AvaloniaProperty.RegisterDirect<ChromeWindow, bool>(nameof(IsDimmable),
        o => o.IsDimmable, (o, v) => o.IsDimmable = v, DefaultIsDimmable);

    /// <summary>
    /// Gets the <see cref="WindowSettings"/> instance supplied on construction.
    /// </summary>
    /// <remarks>
    /// Typically the instance will be that of <see cref="WindowSettings.Global"/> and calling <see
    /// cref="SettingsBase.OnChanged(bool)"/> will update not only this <see cref="ChromeWindow"/> instance, but all
    /// windows which share the same <see cref="WindowSettings.Global"/> reference.
    /// </remarks>
    public readonly WindowSettings Settings;

    /// <summary>
    /// Gets or sets whether <see cref="ChromeBar"/> is visible even if <see cref="IsChromeWindow"/> is false.
    /// </summary>
    /// <remarks>
    /// When <see cref="IsChromeWindow"/> is false, the <see cref="ChromeBar"/> is not normally shown. However, if <see
    /// cref="IsChromeAlwaysVisible"/> is true the <see cref="ChromeBar"/> is shown under the system titlebar provided
    /// it has one or more buttons. The window control buttons, i.e. minimize, maximize and close, are not shown in this
    /// scenario. The default is false.
    /// </remarks>
    public bool IsChromeAlwaysVisible
    {
        get { return _isChromeAlwaysVisible; }
        set { SetAndRaise(IsChromeAlwaysVisibleProperty, ref _isChromeAlwaysVisible, value); }
    }

    /// <summary>
    /// Gets or sets whether an unhandled ESCAPE key press will close the window.
    /// </summary>
    public bool IsEscapeToClose
    {
        get { return _isEscapeToClose; }
        set { SetAndRaise(IsEscapeToCloseProperty, ref _isEscapeToClose, value); }
    }

    /// <summary>
    /// Gets or sets whether the window is dimmed when inactive.
    /// </summary>
    public bool IsDimmable
    {
        get { return _isDimmable; }
        set { SetAndRaise(IsDimmableProperty, ref _isDimmable, value); }
    }

    /// <summary>
    /// Gets the client width available to the application.
    /// </summary>
    /// <remarks>
    /// This may not be same as as Width, as adjusts for chrome decoration. Where <see cref="IsChromeWindow"/> is false,
    /// it returns the same as Width. This is not an Avalonia property.
    /// </remarks>
    public double ClientWidth
    {
        get { return ClientSize.Width; }

        set
        {
            if (ClientSize.Width != value)
            {
                // Left the Width property raise the changes
                Width = value + _outerWrap.BorderThickness.Width();
            }
        }
    }

    /// <summary>
    /// Gets the client height available to the application.
    /// </summary>
    /// <remarks>
    /// This may not be same as as Height, as adjusts for chrome decoration. Where <see cref="IsChromeWindow"/> is false,
    /// it returns the same as Height. This is not an Avalonia property.
    /// </remarks>
    public double ClientHeight
    {
        get { return ClientSize.Height; }

        set
        {
            if (ClientSize.Height != value)
            {
                // Left the Height property raise the changes
                Height = value + _outerWrap.BorderThickness.Height() + _chromeBar.BarHeight;
            }
        }
    }

    /// <summary>
    /// Gets whether the Window draws its own chrome-bar.
    /// </summary>
    public bool IsChromeWindow { get; private set; }

    /// <summary>
    /// Gets whether Window.OnOpened() has been called and remains true.
    /// </summary>
    public bool HasOpened { get; private set; }

    /// <summary>
    /// Returns an interface for adding controls to the window.
    /// </summary>
    public IChromeBar ChromeBar { get; }

    /// <summary>
    /// Gets or sets the content as a Control.
    /// </summary>
    /// <remarks>
    /// This replaces the base class property and may potentially give a different value to that of GetValue(). Do not
    /// use GetValue() for this.
    /// </remarks>
    [Content]
    public new Control? Content
    {
        get { return _contentSource; }
        set { base.Content = value; }
    }

    /// <summary>
    /// Gets or sets the background brush.
    /// </summary>
    /// <remarks>
    /// This replaces the base class property and may potentially give a different value to that of GetValue(). Do not
    /// use GetValue() for this.
    /// </remarks>
    public new IBrush? Background
    {
        get { return _innerWrap.Background; }
        set { base.Background = value; }
    }

    /// <summary>
    /// Gets the Window.SystemDecorations while hiding the base class setter.
    /// </summary>
    /// <remarks>
    /// This property should not be set directly. The value is controlled by <see cref="IsChromeWindow"/>.
    /// </remarks>
    public new SystemDecorations SystemDecorations
    {
        get { return base.SystemDecorations; }
    }

    /// <summary>
    /// Gets the Window.CornerRadius while hiding the base class setter.
    /// </summary>
    /// <remarks>
    /// This property should not be set directly. The value is controlled by <see cref="IsChromeWindow"/>.
    /// </remarks>
    public new CornerRadius CornerRadius
    {
        get { return _outerWrap.CornerRadius; }
    }

    /// <summary>
    /// Gets the client size.
    /// </summary>
    /// <remarks>
    /// This replaces the base class property and may potentially give a different value to that of GetValue(). Do not
    /// use GetValue() for this.
    /// </remarks>
    public new Size ClientSize
    {
        get
        {
            var width = _contentGrid.ColumnDefinitions[0].ActualWidth;
            var height = _contentGrid.RowDefinitions[ContentRow].ActualHeight;
            return new(width, height);
        }
    }

    /// <summary>
    /// Gets access to the underlay control.
    /// </summary>
    protected IBarUnderlay Underlay
    {
        get { return _chromeBar.Underlay; }
    }

    /// <summary>
    /// Gets the current screen in DIPs.
    /// </summary>
    public Rect GetScreenRect()
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(GetScreenRect)}";

        var screen = Screens.ScreenFromWindow(this);
        ConditionalDebug.WriteLine(NSpace, "Window screen: " + screen?.DisplayName ?? "NULL");

        if (screen == null)
        {
            screen = Screens.Primary;
            ConditionalDebug.WriteLine(NSpace, "Primary screen: " + screen?.DisplayName ?? "NULL");
        }

        if (screen != null && screen.Scaling > 0.0)
        {
            var scale = screen.Scaling;
            var work = screen.WorkingArea;

            var pos = new Point(work.X / scale, work.Y / scale);
            ConditionalDebug.WriteLine(NSpace, "DIP x,y: " + pos);

            var size = new Size(work.Width / scale, work.Height / scale);
            ConditionalDebug.WriteLine(NSpace, "DIP size: " + size);
            return new(pos, size);
        }

        ConditionalDebug.WriteLine(NSpace, "NO SCREEN");
        return new(default, Size.Infinity);
    }

    /// <summary>
    /// Gets a Rect containing physical X, Y relative to "parent", and Width and Height in DPI.
    /// </summary>
    public Rect GetRelativeRect(Window parent)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(GetRelativeRect)}";

        var p0 = IsVisible ? Position : _closePosition;
        ConditionalDebug.WriteLine(NSpace, $"p0: {p0}");

        var p1 = parent.Position;
        ConditionalDebug.WriteLine(NSpace, $"p1: {p1}");

        var pos = new Point(p0.X - p1.X, p0.Y - p1.Y);

        ConditionalDebug.WriteLine(NSpace, $"Pos: {pos}");
        ConditionalDebug.WriteLine(NSpace, $"Size: {new Size(Width, Height)}");

        return new(pos, new Size(Width, Height));
    }

    /// <summary>
    /// Restores the "bounds" as given by <see cref="GetRelativeRect"/>.
    /// </summary>
    /// <remarks>
    /// The window is clamped to the "parent" screen area.
    /// </remarks>
    public void RestoreRelativeRect(Window parent, Rect bounds)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(RestoreRelativeRect)}";

        if (!double.IsFinite(bounds.Width) || !double.IsFinite(bounds.Height) || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        Width = bounds.Width;
        Height = bounds.Height;
        ConditionalDebug.WriteLine(NSpace, $"Size: {bounds.Size}");

        if (!double.IsFinite(bounds.X) || !double.IsFinite(bounds.Y))
        {
            return;
        }

        var p1 = parent.Position;
        var pos = new PixelPoint(p1.X + (int)bounds.X, p1.Y + (int)bounds.Y);

        var screen = parent.Screens.ScreenFromWindow(parent);

        if (screen != null)
        {
            var scale = screen.Scaling;
            var work = screen.WorkingArea;

            int x = Math.Clamp(pos.X, work.X, work.X + work.Width - (int)(Width * scale));
            int y = Math.Clamp(pos.Y, work.Y, work.Y + work.Height - (int)(Height * scale));

            Position = new PixelPoint(x, y);
            ConditionalDebug.WriteLine(NSpace, $"Position: {Position}");

            WindowStartupLocation = WindowStartupLocation.Manual;
        }
    }

    /// <summary>
    /// Invoked by <see cref="ChromeStyling"/> on change.
    /// </summary>
    /// <remarks>
    /// The method is called by default when the window is being readied for opening with "init" equal to true. It is
    /// not called once the window has closed.
    /// </remarks>
    protected virtual void OnStylingChanged(bool init)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(OnStylingChanged)}";
        ConditionalDebug.WriteLine(NSpace, $"Init: {init}");
        ConditionalDebug.ThrowIfNotEqual(Brushes.Transparent, base.Background);

        UpdateBorder(WindowState);
        _chromeBar.RefreshStyling();
        _outerWrap.Background = Styling.Background;
        _outerWrap.BorderBrush = Styling.WindowBorder;
    }

    /// <summary>
    /// This method is called after the Window has opened once the application is idle.
    /// </summary>
    protected virtual void OnOpenedIdle()
    {
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (!HasOpened)
        {
            // Initialize
            OnWindowSettingsChanged(IsDialog);
            OnStylingChanged(true);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(OnOpened)}";
        ConditionalDebug.WriteLine(NSpace, $"IsDialog: {IsDialog}");
        ConditionalDebug.WriteLine(NSpace, $"Title: {Title}");

        base.OnOpened(e);

        var limit = GetScreenRect();
        Width = Math.Min(Width, limit.Width);
        Height = Math.Min(Height, limit.Height);

        HasOpened = true;
        Styling.StylingChanged += StylingChangedHandler;
        Settings.Changed += SettingsChangedHandler;

        // WAYLAND WORK AROUND
        // https://github.com/AvaloniaUI/Avalonia/issues/19231
        var focusable = this.FirstFocusableChild();

        if (focusable != null)
        {
            focusable.Focus();
        }
        else
        {
            Focusable = true;
            Focus();
        }

        Dispatcher.UIThread.Post(OnOpenedIdle, DispatcherPriority.ApplicationIdle);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Styling.StylingChanged -= StylingChangedHandler;
        Settings.Changed -= SettingsChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(OnKeyDown)}";
        base.OnKeyDown(e);

        // https://github.com/AvaloniaUI/Avalonia/issues/19231
        // Log the receiver class type. Had an issue with parent window receiving keys.
        ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");
        ConditionalDebug.WriteLine(NSpace, $"Key: {e.Key}, {e.PhysicalKey}");
        ConditionalDebug.WriteLine(NSpace, $"Handled: {e.Handled}, {IsActive}");

        if (!IsActive)
        {
            // Shouldn't really need to check, but had
            // situation where window behind is receiving keys.
            return;
        }

        if (ChromeBar.LeftGroup.HandleKeyGesture(e))
        {
            return;
        }

        if (ChromeBar.RightGroup.HandleKeyGesture(e))
        {
            return;
        }

        if (e.PhysicalKey == PhysicalKey.Escape && IsEscapeToClose && IsActive && !e.Handled)
        {
            ConditionalDebug.WriteLine(NSpace, "Escape accepted");
            e.Handled = true;
            Close();
            return;
        }
    }


    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;

        // REDIRECTED
        if (p == BackgroundProperty)
        {
            if (!_isSettingBase && _initialized)
            {
                // Keep transparent
                SetBaseValue(BackgroundProperty, Brushes.Transparent);

                // Set on INNER wrapper
                _innerWrap.Background = change.GetNewValue<IBrush?>();
                base.OnPropertyChanged(change);
            }

            return;
        }

        // REDIRECTED
        if (p == ContentProperty)
        {
            if (!_isSettingBase && _initialized)
            {
                // Order VERY important
                var value = change.NewValue as Control;

                if (value == null && change.NewValue != null)
                {
                    throw new InvalidOperationException("Window Content must be Control");
                }

                // Order important
                var orig = _contentSource;

                // Keep outer wrapper
                _contentSource = value;
                SetBaseValue(ContentProperty, _outerWrap);

                if (orig != null)
                {
                    _contentGrid.Children.Remove(orig);
                }

                if (value != null)
                {
                    Grid.SetColumn(value, 0);
                    Grid.SetRow(value, ContentRow);
                    _contentGrid.Children.Add(value);
                }

                base.OnPropertyChanged(change);
            }

            return;
        }

        if (p == CursorProperty)
        {
            if (!_isSettingBase)
            {
                // Just need to hold this
                _cursor = change.GetNewValue<Cursor?>();
                base.OnPropertyChanged(change);
            }

            return;
        }

        // Other properties
        base.OnPropertyChanged(change);

        if (p == WindowStateProperty)
        {
            UpdateBorder(change.GetNewValue<WindowState>());
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(OnPointerMoved)}";

        base.OnPointerMoved(e);

        if (IsChromeWindow && CanResize)
        {
            var p = e.GetCurrentPoint(this).Properties;

            if (!p.IsLeftButtonPressed && TryEdge(e, out WindowEdge edge))
            {
                ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");
                ConditionalDebug.WriteLine(NSpace, $"Edge detect at: {e.GetPosition(this)}");

                SetBaseValue(CursorProperty, EdgeToCursor(edge));
                return;
            }
        }

        // Reset
        SetBaseValue(CursorProperty, _cursor);
    }

    private static Cursor? EdgeToCursor(WindowEdge edge)
    {
        switch (edge)
        {
            case WindowEdge.North:
            case WindowEdge.South: return ChromeCursors.SizeNorthSouthCursor;
            case WindowEdge.West:
            case WindowEdge.East: return ChromeCursors.SizeWestEastCursor;
            case WindowEdge.NorthWest: return ChromeCursors.TopLeftCornerCursor;
            case WindowEdge.NorthEast: return ChromeCursors.TopRightCornerCursor;
            case WindowEdge.SouthWest: return ChromeCursors.BottomLeftCornerCursor;
            case WindowEdge.SouthEast: return ChromeCursors.BottomRightCornerCursor;
            default: throw new ArgumentException("Invalid edge", nameof(edge));
        }
    }

    private void OnWindowSettingsChanged(bool isDialog)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(OnWindowSettingsChanged)}";
        ConditionalDebug.WriteLine(NSpace, $"IsDialog: {isDialog}");

        IsChromeWindow = Settings.GetChromeWindow(isDialog);
        ConditionalDebug.WriteLine(NSpace, $"IsChromeWindow: {IsChromeWindow}");

        if (IsChromeWindow != _chromeWindowLast || !_initialized)
        {
            _chromeWindowLast = IsChromeWindow;

            if (IsChromeWindow)
            {
                // Kill system chrome
                SetBaseValue(SystemDecorationsProperty, SystemDecorations.None);
                SetBaseValue(TransparencyLevelHintProperty, new WindowTransparencyLevel[] { WindowTransparencyLevel.Transparent });
                SetBaseValue(ExtendClientAreaTitleBarHeightHintProperty, -1);
                SetBaseValue(ExtendClientAreaToDecorationsHintProperty, true);
                SetBaseValue(ExtendClientAreaChromeHintsProperty, Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome);

                _chromeBar.DoubleTapped += ChromeDoubleTappedHandler;
                _chromeBar.PointerPressed += ChromePointerPressedHandler;

                // Need to intercept event over child controls for resizing.
                // We may get multiple firings on the handler method.
                AddHandler(PointerPressedEvent, PointerPressedHandler,
                    RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
                    handledEventsToo: true);
            }
            else
            {
                // Regular window behaviour
                SetBaseValue(SystemDecorationsProperty, SystemDecorations.Full);
                ClearValue(TransparencyLevelHintProperty);
                ClearValue(ExtendClientAreaTitleBarHeightHintProperty);
                ClearValue(ExtendClientAreaToDecorationsHintProperty);
                ClearValue(ExtendClientAreaChromeHintsProperty);

                _chromeBar.DoubleTapped -= ChromeDoubleTappedHandler;
                _chromeBar.PointerPressed -= ChromePointerPressedHandler;

                RemoveHandler(PointerPressedEvent, PointerPressedHandler);
            }
        }

        ShowInTaskbar = Settings.GetShowInTaskbar(isDialog);

        UpdateBorder(WindowState);
        _chromeBar.UpdateWindowSettings(isDialog);
    }

    private bool TryEdge(PointerEventArgs e, out WindowEdge edge)
    {
        var p = e.GetPosition(this);

        if (IsOverInteractiveElement(p))
        {
            edge = default;
            return false;
        }

        if (p.X < GrabPixels)
        {
            if (p.Y < GrabPixels)
            {
                edge = WindowEdge.NorthWest;
                return true;
            }

            if (p.Y > Bounds.Height - GrabPixels)
            {
                edge = WindowEdge.SouthWest;
                return true;
            }

            edge = WindowEdge.West;
            return true;
        }

        if (p.X > Bounds.Width - GrabPixels)
        {
            if (p.Y < GrabPixels)
            {
                edge = WindowEdge.NorthEast;
                return true;
            }

            if (p.Y > Bounds.Height - GrabPixels)
            {
                edge = WindowEdge.SouthEast;
                return true;
            }

            edge = WindowEdge.East;
            return true;
        }

        if (p.Y < GrabPixels)
        {
            edge = WindowEdge.North;
            return true;
        }

        if (p.Y > Bounds.Height - GrabPixels)
        {
            edge = WindowEdge.South;
            return true;
        }

        edge = default;
        return false;
    }

    private void SetBaseValue(AvaloniaProperty property, object? value)
    {
        try
        {
            _isSettingBase = true;
            SetValue(property, value);
        }
        finally
        {
            _isSettingBase = false;
        }
    }

    private void UpdateBorder(WindowState state)
    {
        if (IsChromeWindow && state == WindowState.Normal)
        {
            _outerWrap.BorderThickness = new(1.0);
            _outerWrap.CornerRadius = Styling.LargeCornerRadius;
            _innerWrap.CornerRadius = Styling.LargeCornerRadius;
            return;
        }

        _outerWrap.BorderThickness = default;
        _outerWrap.CornerRadius = default;
        _innerWrap.CornerRadius = default;
    }

    private void DeactivateContent()
    {
        if (IsDimmable && _deactiveOverlay == null)
        {
            var olay = new Border();
            olay.Opacity = 0.25;
            olay.IsHitTestVisible = true;
            olay.Background = ChromeBrushes.Black;
            olay.ZIndex = DeactiveZ;

            Grid.SetRow(olay, TitleRow);
            Grid.SetRowSpan(olay, int.MaxValue);

            _contentGrid.Children.Add(olay);
            _deactiveOverlay = olay;
        }
    }

    private bool ReactivateContent()
    {
        if (_deactiveOverlay != null)
        {
            // Restore the darkened owner
            var olay = _deactiveOverlay;
            _deactiveOverlay = null;

            _contentGrid!.Children.Remove(olay);
            return true;
        }

        return false;
    }

    private bool IsOverInteractiveElement(Point p)
    {
        var v = this.GetVisualAt(p);

        if (v != null)
        {
            return v is LightDismissOverlayLayer or Button or TextBox or ScrollBar or Slider;
        }

        return v != this;
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        OnStylingChanged(false);
    }

    private void SettingsChangedHandler(object? _, EventArgs __)
    {
        OnWindowSettingsChanged(IsDialog);
    }

    private void ActivatedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(ActivatedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");
        ReactivateContent();
    }

    private void DeactivatedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(DeactivatedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");
        DeactivateContent();
    }

    private void ChromePointerPressedHandler(object? _, PointerPressedEventArgs e)
    {
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(ChromePointerPressedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");

        // Need to handle as right-click to fix quirk/bug on windows
        // Handle this here because otherwise conflicts with drag move below.
        var props = e.GetCurrentPoint(_chromeBar).Properties;

        if (props.IsRightButtonPressed)
        {
            e.Handled = true;
            _chromeContext ??= new(this);
            _chromeContext.Open(_chromeBar);
            return;
        }

        // Handle drag move as we don't have system bar if this is called
        if (props.IsLeftButtonPressed && !TryEdge(e, out WindowEdge _))
        {
            ConditionalDebug.WriteLine(NSpace, $"Begin drag move");
            BeginMoveDrag(e);
            e.Handled = true;
        }
    }

    private void ChromeDoubleTappedHandler(object? _, TappedEventArgs e)
    {
        // Modern drag move
        if (IsChromeWindow && CanMaximize)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }

    private void PointerPressedHandler(object? _, PointerPressedEventArgs e)
    {
        // Drag resize
        const string NSpace = $"{nameof(ChromeWindow)}.{nameof(PointerPressedHandler)}";

        if (IsChromeWindow && CanResize)
        {
            var pos = e.GetPosition(this);

            try
            {
                // We may get multiple calls from same event.
                // The dragResizePoint prevents repeats.
                if (_dragResizePoint != pos &&
                    e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                    TryEdge(e, out WindowEdge edge))
                {
                    // Accept even if event was handled
                    ConditionalDebug.WriteLine(NSpace, $"Class type: {GetType().Name}");
                    ConditionalDebug.WriteLine(NSpace, $"BeginResizeDrag: {pos}");

                    BeginResizeDrag(edge, e);
                    e.Handled = true;
                    return;
                }
            }
            finally
            {
                // Prevent repeats
                _dragResizePoint = pos;
            }
        }

    }

    private void ClosingHandler(object? _, WindowClosingEventArgs e)
    {
        _closePosition = Position;
    }

}