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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Base class for a container which hosts a Control with surrounding accoutrements and left-right buttons.
/// </summary>
/// <remarks>
/// A base class <see cref="PixieControl"/> instance is instantiable and may be used to display a simple <see
/// cref="Title"/> with surrounding buttons and symbols. Multiple <see cref="PixieControl"/> instances may be housed in
/// a <see cref="PixieGroup"/>. Pixie controls are intended to be efficient as it was intended that there may be a 100
/// or more of visible.
/// </remarks>
public class PixieControl : Border
{
    /// <summary>
    /// Default value for <see cref="VerticalContentOffset"/>.
    /// </summary>
    protected readonly static double DefaultVerticalOffset = MeasureDefaultOffset();

    /// <summary>
    /// Global styling instance for convenience only.
    /// </summary>
    protected readonly static ChromeStyling Styling = ChromeStyling.Global;

    private const int LeftColumn = 0;
    private const int TitleColumn = 1;
    private const int ItemControlColumn = 2;
    private const int RightColumn = 3;
    private const int ColumnCount = 4;

    private const int HeaderRow = 0;
    private const int ContentRow = 1;
    private const int FooterRow = 2;

    // Intention is to keep row content appearing
    // vertically centered even if aligned to top.
    private const double TitleSpacer = 12.0;
    private static readonly Thickness HeaderMargin = new(0.0, 0.0, 0.0, 4.0);
    private static readonly Thickness FooterMargin = new(0.0, 4.0, 0.0, 0.0);

    private readonly Border _adorner = new();
    private readonly Grid _grid = new();

    private readonly TextBlock _titleBlock = new();
    private FontWeight _titleWeight = FontWeight.Normal;
    private string? _title;

    private Control? _childControl;

    private string? _header;
    private TextBlock? _headerBlock;

    private string? _footer;
    private TextAlignment _footerAlignment;
    private TextBlock? _footerBlock;

    private string? _leftSymbol;
    private string? _rightSymbol;
    private Accoutrement? _leftStack;
    private Accoutrement? _rightStack;

    private Control? _focusControl;
    private ITemplate<Control>? _origAdorner;
    private double _leftIndent;
    private bool _isTitleVisible = true;
    private double _verticalContentOffset = DefaultVerticalOffset;
    private VerticalAlignment _verticalContentAlignment;

    private bool _isSettingBase;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieControl()
        : this(true, VerticalAlignment.Center)
    {
    }

    /// <summary>
    /// Subclass constructor with initial values for <see cref="IsTitleVisible"/> and <see cref="VerticalContentAlignment"/>.
    /// </summary>
    protected PixieControl(bool isTitleVisible, VerticalAlignment verticalAlignment)
    {
        _isTitleVisible = isTitleVisible;
        _verticalContentAlignment = verticalAlignment;

        base.Child = _adorner;
        _adorner.Child = _grid;
        _adorner.BorderThickness = new(1.0);
        _adorner.MinHeight = ChromeFonts.DefaultLineHeight;

        _grid.VerticalAlignment = VerticalAlignment.Center;

        for (int n = 0; n < ColumnCount; ++n)
        {
            _grid.ColumnDefinitions.Add(new(n == ItemControlColumn ? GridLength.Star : GridLength.Auto));
        }

        _grid.RowDefinitions.Add(new(GridLength.Auto));
        _grid.RowDefinitions.Add(new(GridLength.Auto));
        _grid.RowDefinitions.Add(new(GridLength.Auto));

        _titleBlock.FocusAdorner = null;
        _titleBlock.IsVisible = GetTitleVisibility();
        _titleBlock.HorizontalAlignment = HorizontalAlignment.Left;
        _titleBlock.VerticalAlignment = verticalAlignment;
        _titleBlock.TextWrapping = TextWrapping.Wrap; // <- initial but overridden later
        _titleBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        _titleBlock.PointerExited += PointerExitedHandler;
        SetTitlePadding(_titleBlock, _verticalContentOffset);

        Grid.SetColumn(_titleBlock, TitleColumn);
        Grid.SetRow(_titleBlock, ContentRow);
        _grid.Children.Add(_titleBlock);

        _grid.SizeChanged += GridSizeChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="ValueChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<PixieControl, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, string?>(nameof(Header),
        o => o.Header, (o, v) => o.Header = v);

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, string?> TitleProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, string?>(nameof(Title),
        o => o.Title, (o, v) => o.Title = v);

    /// <summary>
    /// Defines the <see cref="TitleWeight"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, FontWeight> TitleWeightProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, FontWeight>(nameof(TitleWeight),
        o => o.TitleWeight, (o, v) => o.TitleWeight = v, FontWeight.Normal);

    /// <summary>
    /// Defines the <see cref="Footer"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, string?> FooterProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, string?>(nameof(Footer),
        o => o.Footer, (o, v) => o.Footer = v);

    /// <summary>
    /// Defines the <see cref="Footer"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, TextAlignment> FooterAlignmentProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, TextAlignment>(nameof(FooterAlignment),
        o => o.FooterAlignment, (o, v) => o.FooterAlignment = v);

    /// <summary>
    /// Defines the <see cref="LeftSymbol"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, string?> LeftSymbolProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, string?>(nameof(LeftSymbol),
        o => o.LeftSymbol, (o, v) => o.LeftSymbol = v);

    /// <summary>
    /// Defines the <see cref="RightSymbol"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieControl, string?> RightSymbolProperty =
        AvaloniaProperty.RegisterDirect<PixieControl, string?>(nameof(RightSymbol),
        o => o.RightSymbol, (o, v) => o.RightSymbol = v);

    /// <summary>
    /// Occurs when the subclass control value changes.
    /// </summary>
    /// <remarks>
    /// For example, it occurs when the slider on the <see cref="PixieSlider"/> is moved. It occurs when <see
    /// cref="PixieButton.IsBackgroundCheckedProperty"/> changes etc. It does not occur when <see cref="PixieButton"/> is clicked. It
    /// does not occur for <see cref="PixieSelectable"/>.
    /// </remarks>
    public event EventHandler<RoutedEventArgs> ValueChanged
    {
        add { AddHandler(ValueChangedEvent, value); }
        remove { RemoveHandler(ValueChangedEvent, value); }
    }

    /// <summary>
    /// Gets or sets the header text shown above <see cref="Title"/> .
    /// </summary>
    public string? Header
    {
        get { return _header; }
        set { SetAndRaise(HeaderProperty, ref _header, value); }
    }

    /// <summary>
    /// Gets or sets the primary text.
    /// </summary>
    public string? Title
    {
        get { return _title; }
        set { SetAndRaise(TitleProperty, ref _title, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="Title"/> font weight.
    /// </summary>
    public FontWeight TitleWeight
    {
        get { return _titleWeight; }
        set { SetAndRaise(TitleWeightProperty, ref _titleWeight, value); }
    }

    /// <summary>
    /// Gets or sets the footer text shown below the control.
    /// </summary>
    public virtual string? Footer
    {
        get { return _footer; }
        set { SetAndRaise(FooterProperty, ref _footer, value); }
    }

    /// <summary>
    /// Gets or sets the alignment of <see cref="Footer"/>.
    /// </summary>
    public TextAlignment FooterAlignment
    {
        get { return _footerAlignment; }
        set { SetAndRaise(FooterAlignmentProperty, ref _footerAlignment, value); }
    }

    /// <summary>
    /// Gets or sets the left symbol string.
    /// </summary>
    public string? LeftSymbol
    {
        get { return _leftSymbol; }
        set { SetAndRaise(LeftSymbolProperty, ref _leftSymbol, value); }
    }

    /// <summary>
    /// Gets or sets the right symbol string.
    /// </summary>
    public string? RightSymbol
    {
        get { return _rightSymbol; }
        set { SetAndRaise(RightSymbolProperty, ref _rightSymbol, value); }
    }

    /// <summary>
    /// Gets the left button.
    /// </summary>
    /// <remarks>
    /// By default, the <see cref="LightButton"/>.IsVisible value is false and MUST be set true to be seen. The button is
    /// initialized to show a default symbol. Button properties can only be set programmatically.
    /// </remarks>
    public LightButton LeftButton
    {
        get
        {
            _leftStack ??= new(true, this);
            return _leftStack.Button;
        }
    }

    /// <summary>
    /// Gets the right button.
    /// </summary>
    /// <remarks>
    /// By default, the <see cref="LightButton"/>.IsVisible value is false and MUST be set true to be seen. The button
    /// is initialized to show a default symbol. Button properties can only be set programmatically.
    /// </remarks>
    public LightButton RightButton
    {
        get
        {
            _rightStack ??= new(false, this);
            return _rightStack.Button;
        }
    }

    /// <summary>
    /// Gets the group manager.
    /// </summary>
    public PixieGroup? Group { get; internal set; }

    /// <summary>
    /// Gets or sets the internal padding.
    /// </summary>
    /// <remarks>
    /// Note that <see cref="PixieControl"/> has internal padding which cannot be removed. This shadows "Padding" but
    /// adds to that existing. It is not a StyledProperty.
    /// </remarks>
    public Thickness ContentPadding
    {
        get { return _adorner.Padding; }
        set { _adorner.Padding = value; }
    }

    /// <summary>
    /// Gets or sets whether to show grid lines for debug purposes.
    /// </summary>
    public bool ShowGridLines
    {
        get { return _grid.ShowGridLines; }
        set { _grid.ShowGridLines = value; }
    }

    /// <summary>
    /// For use by <see cref="PixieGroup"/>.
    /// </summary>
    internal double LeftIndent
    {
        get { return _leftIndent; }

        set
        {
            if (_leftIndent != value)
            {
                _leftIndent = value;
                SetGridMargin(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the internal background without altering the Control itself.
    /// </summary>
    protected IBrush? AdornerBackground
    {
        get { return _adorner.Background; }
        set { _adorner.Background = value; }
    }

    /// <summary>
    /// Gets whether to draw a pseudo focus box, as if the container itself is focused.
    /// </summary>
    protected bool CanPseudoFocus
    {
        get { return _focusControl != null; }
    }

    /// <summary>
    /// Gets or sets whether the <see cref="Title"/> is visible.
    /// </summary>
    /// <remarks>
    /// Subclass may hide the title where superfluous or problematic.
    /// </remarks>
    protected bool IsTitleVisible
    {
        get { return _isTitleVisible; }

        set
        {
            if (_isTitleVisible != value)
            {
                _isTitleVisible = value;
                UpdateTitleMaxWidth(_grid.Bounds.Width);

                // Not needed with Update() call above
                // _titleBlock.IsVisible = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a vertical offset of all content items except the subclass control item.
    /// </summary>
    /// <remarks>
    /// Intended to allow vertical positioning by the subclass.
    /// </remarks>
    protected double VerticalContentOffset
    {
        get { return _verticalContentOffset; }

        set
        {
            if (_verticalContentOffset != value)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(VerticalContentOffset));
                _verticalContentOffset = value;
                _leftStack?.Offset = value;
                _rightStack?.Offset = value;
                SetTitlePadding(_titleBlock, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets vertical alignment of all content items except the subclass control item.
    /// </summary>
    /// <remarks>
    /// Intended to allow vertical positioning by the subclass.
    /// </remarks>
    protected VerticalAlignment VerticalContentAlignment
    {
        get { return _verticalContentAlignment; }

        set
        {
            if (_verticalContentAlignment != value)
            {
                _verticalContentAlignment = value;
                _leftStack?.SetValue(VerticalAlignmentProperty, value);
                _rightStack?.SetValue(VerticalAlignmentProperty, value);
                _titleBlock.SetValue(VerticalAlignmentProperty, value);
            }
        }
    }

    /// <summary>
    /// Gets the hover brush.
    /// </summary>
    protected virtual ImmutableSolidColorBrush HoverBrush
    {
        get { return ChromeStyling.PixieHover; }
    }

    /// <summary>
        /// Do not use.
        /// </summary>
        [Obsolete($"Do not use.", true)]
    protected new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Do not use.
    /// </summary>
    /// <remarks>
    /// Focus pertains to a child control, not the panel. A public setter would not have the expected effect.
    /// </remarks>
    [Obsolete($"Do not use.", true)]
    protected new bool Focusable
    {
        get { return base.Focusable; }
        set { base.Focusable = value; }
    }

    /// <summary>
    /// Do not use. Use <see cref="ContentPadding"/> instead.
    /// </summary>
    [Obsolete($"Do not use. Use {nameof(ContentPadding)} instead.", true)]
    protected new Thickness Padding
    {
        get { return base.Padding; }
        set { base.Padding = value; }
    }

    /// <summary>
    /// Focuses.
    /// </summary>
    public void Focus()
    {
        _childControl?.Focus();
    }

    /// <summary>
    /// Replaces base class method. Base class method will do nothing.
    /// </summary>
    public new void Focus(NavigationMethod _, KeyModifiers __ = KeyModifiers.None)
    {
        _childControl?.Focus();
    }

    /// <summary>
    /// Returns true if <see cref="Header"/>, <see cref="Title"/> or <see cref="Footer"/> contains "keyword" case
    /// insensitively.
    /// </summary>
    /// <remarks>
    /// The instance is appended to "results" if true. The return value is always false for null or empty strings.
    /// </remarks>
    public virtual bool Find(string? keyword, List<PixieFinding>? findings)
    {
        const StringComparison Comp = StringComparison.OrdinalIgnoreCase;

        if (string.IsNullOrEmpty(keyword))
        {
            return false;
        }

        if (Header?.Contains(keyword, Comp) == true ||
            Title?.Contains(keyword, Comp) == true ||
            Footer?.Contains(keyword, Comp) == true)
        {
            findings?.Add(new(this));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Brings into view and calls attention.
    /// </summary>
    public void Attention()
    {
        AdornerBackground = Styling.WeakAccent;
        DispatcherTimer.RunOnce(() => ClearFocusBackground(), TimeSpan.FromMilliseconds(1000));
        Dispatcher.UIThread.Post(() => this.BringIntoView(), DispatcherPriority.ContextIdle);
    }

    /// <summary>
    /// Clears pseudo focus background.
    /// </summary>
    internal virtual void ClearFocusBackground()
    {
        AdornerBackground = null;
    }

    /// <summary>
    /// Sets and initializes the "child" control instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can be called once only</exception>
    /// <exception cref="ArgumentException">Invalid fraction</exception>
    protected void SetChildControl(Control child)
    {
        ConditionalDebug.WriteLine($"{GetType().Name}.{nameof(SetChildControl)}", $"Control: {child.GetType().Name}");

        if (_childControl != null)
        {
            throw new InvalidOperationException($"{nameof(SetChildControl)} can be called once only");
        }

        // Not wrapped with has a control
        _titleBlock.TextWrapping = TextWrapping.NoWrap;

        _childControl = child;
        child.VerticalAlignment = _verticalContentAlignment;

        Grid.SetColumn(_childControl, ItemControlColumn);
        Grid.SetRow(_childControl, ContentRow);
        _grid.Children.Add(_childControl);

        child.PointerExited += PointerExitedHandler;
        child.SizeChanged += SubControlSizeChangedHandler;
    }

    /// <summary>
    /// Sets the control with which to trigger pseudo focus.
    /// </summary>
    protected void SetPseudoFocusControl(Control? control)
    {
        if (control != _focusControl)
        {
            _adorner.BorderBrush = null;

            if (_focusControl != null)
            {
                _focusControl.FocusAdorner = _origAdorner;
                _focusControl.GotFocus -= PseudoGotFocusHandler;
                _focusControl.LostFocus -= PseudoLostFocusHandler;
            }

            _focusControl = control;

            if (control != null)
            {
                control.IsTabStop = true;
                SetBaseProperty(FocusableProperty, control == this);

                _origAdorner = control.FocusAdorner;
                control.FocusAdorner = null;

                control.GotFocus += PseudoGotFocusHandler;
                control.LostFocus += PseudoLostFocusHandler;
            }
            else
            {
                SetBaseProperty(FocusableProperty, false);
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetGridMargin(_leftIndent);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var info = e.GetCurrentPoint(this);

        if (info.Properties.IsLeftButtonPressed)
        {
            _childControl?.Focus(NavigationMethod.Pointer);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_focusControl != null)
        {
            AdornerBackground = HoverBrush;
            return;
        }

        // Determine if there is something inside to
        // focus and, if so, show hover background.
        if (_childControl != null)
        {
            bool focusable = _childControl.Focusable && _childControl.IsTabStop;

            if (!focusable)
            {
                var child = _childControl.FirstFocusableChild();
                focusable = child?.Focusable == true && child.IsTabStop;
            }

            if (focusable)
            {
                AdornerBackground = HoverBrush;
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        PointerExitedHandler(this, e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var isSetingBase = _isSettingBase;
        _isSettingBase = false;

        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChildProperty && change.OldValue != null)
        {
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }

        if (p == FocusableProperty)
        {
            if (!isSetingBase)
            {
                throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
            }

            return;
        }

        if (p == HeaderProperty)
        {
            NewOrUpdateBanner(ref _headerBlock, true, change.GetNewValue<string?>());
            return;
        }

        if (p == FooterProperty)
        {
            NewOrUpdateBanner(ref _footerBlock, false, change.GetNewValue<string?>());
            return;
        }

        if (p == FooterAlignmentProperty)
        {
            _footerBlock?.SetValue(TextBlock.TextAlignmentProperty, change.GetNewValue<TextAlignment>());
            return;
        }

        if (p == TitleProperty)
        {
            _titleBlock.Text = change.GetNewValue<string?>();
            _titleBlock.IsVisible = GetTitleVisibility();
            return;
        }

        if (p == TitleWeightProperty)
        {
            _titleBlock.FontWeight = change.GetNewValue<FontWeight>();
            return;
        }

        if (p == LeftSymbolProperty)
        {
            var value = change.GetNewValue<string?>();

            if (_leftStack != null)
            {
                _leftStack.SetSymbol(value);
                return;
            }

            if (!string.IsNullOrEmpty(value))
            {
                _leftStack ??= new(true, this);
                _leftStack.SetSymbol(value);
            }

            return;
        }

        if (p == RightSymbolProperty)
        {
            var value = change.GetNewValue<string?>();

            if (_rightStack != null)
            {
                _rightStack.SetSymbol(value);
                return;
            }

            if (!string.IsNullOrEmpty(value))
            {
                _rightStack ??= new(false, this);
                _rightStack.SetSymbol(value);
            }

            return;
        }

        if (p == CornerRadiusProperty)
        {
            _adorner.CornerRadius = change.GetNewValue<CornerRadius>();
            return;
        }

        if (p == IsEffectivelyEnabledProperty)
        {
            SetTextEnabled(_titleBlock, change.GetNewValue<bool>());
            return;
        }
    }

    /// <summary>
    /// Raises <see cref="ValueChangedEvent"/>.
    /// </summary>
    protected void OnValueChanged()
    {
        RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
    }

    /// <summary>
    /// Also raises <see cref="ValueChangedEvent"/>.
    /// </summary>
    protected void ValueChangedHandler(object? _, EventArgs __)
    {
        RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
    }

    /// <summary>
    /// Returns true if point is inside control bounds.
    /// </summary>
    protected bool IsInsideBounds(Point point)
    {
        // Point is relative to Control (not its parent)
        var b = Bounds;
        return point.X >= 0 && point.X < b.Width && point.Y >= 0 && point.Y < b.Height;
    }

    /// <summary>
    /// Greys out the block is "enabled" is false.
    /// </summary>
    protected static void SetTextEnabled(TextBlock? block, bool enabled)
    {
        if (block != null)
        {
            if (enabled)
            {
                block.ClearValue(TextBlock.ForegroundProperty);
            }
            else
            {
                block.Foreground = ChromeStyling.ForegroundGray;
            }
        }
    }

    private static void SetTitlePadding(TextBlock block, double vOffset)
    {
        // Padding not Margin
        block.Padding = new(0.0, vOffset, TitleSpacer, vOffset);
    }

    private static double MeasureDefaultOffset()
    {
        try
        {
            var b = new TextBlock();
            b.Text = "Ag";
            b.FontFamily = ChromeFonts.DefaultFamily;
            b.FontSize = ChromeFonts.DefaultFontSize;
            b.Measure(Size.Infinity);
            return Math.Max(Accoutrement.DefaultMinHeight - b.DesiredSize.Height, 0.0) / 2.0;
        }
        catch
        {
            // Will throw in unit tests
            return 5.0;
        }
    }

    private bool GetTitleVisibility()
    {
        return _isTitleVisible && !string.IsNullOrEmpty(_title);
    }

    private void UpdateTitleMaxWidth(double gridWidth)
    {
        if (_isTitleVisible)
        {
            double sum = 0.0;

            if (_childControl != null && _childControl.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                sum = _childControl.Bounds.Width;
            }

            for (int n = 0; n < ColumnCount; ++n)
            {
                if (n != TitleColumn && n != ItemControlColumn)
                {
                    sum += _grid.ColumnDefinitions[n].ActualWidth;
                }
            }

            double width = gridWidth - sum;

            if (width > 0)
            {
                _titleBlock.IsVisible = GetTitleVisibility();
                _titleBlock.MaxWidth = width;
                return;
            }

            _titleBlock.IsVisible = false;
            return;
        }

        _titleBlock.IsVisible = GetTitleVisibility();
        _titleBlock.MaxWidth = double.PositiveInfinity;
    }

    private void NewOrUpdateBanner(ref TextBlock? current, bool top, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            if (current != null)
            {
                _grid.Children.Remove(current);
            }

            current = null;
            return;
        }

        if (current == null)
        {
            current = new TextBlock();
            current.FontSize = ChromeFonts.SmallFontSize;
            current.Foreground = ChromeStyling.ForegroundGray;
            current.TextWrapping = top ? TextWrapping.NoWrap : TextWrapping.Wrap;
            current.Margin = top ? HeaderMargin : FooterMargin;
            current.VerticalAlignment = _verticalContentAlignment;
            current.TextTrimming = TextTrimming.CharacterEllipsis;
            current.PointerExited += PointerExitedHandler;

            if (!top)
            {
                current.TextAlignment = FooterAlignment;
            }

            Grid.SetColumn(current, TitleColumn);
            Grid.SetColumnSpan(current, int.MaxValue);
            Grid.SetRow(current, top ? HeaderRow : FooterRow);

            // Very occasionally this throws in unit test (not in any visual tree)
            _grid.Children.Add(current);
        }

        // Changeable properties
        current.Text = text;
    }

    private void SetGridMargin(double indent)
    {
        var m = ChromeSizes.RegularPadding;
        _grid.Margin = new(m.Left + indent, m.Top, m.Right, m.Bottom);
    }

    private void SetBaseProperty(AvaloniaProperty property, bool value)
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

    private void PointerExitedHandler(object? _, PointerEventArgs e)
    {
        if (!IsInsideBounds(e.GetPosition(this)))
        {
            ClearFocusBackground();
        }
    }

    private void PseudoGotFocusHandler(object? _, GotFocusEventArgs e)
    {
        if (e.NavigationMethod == NavigationMethod.Tab || e.NavigationMethod == NavigationMethod.Directional)
        {
            _adorner.BorderBrush = Styling.AccentBrush;
        }
    }

    private void PseudoLostFocusHandler(object? _, EventArgs __)
    {
        _adorner.BorderBrush = null;
    }

    private void SubControlSizeChangedHandler(object? _, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width != e.PreviousSize.Width)
        {
            UpdateTitleMaxWidth(_grid.Bounds.Width);
        }
    }

    private void GridSizeChangedHandler(object? _, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width != e.PreviousSize.Width)
        {
            UpdateTitleMaxWidth(e.NewSize.Width);
        }
    }

    private sealed class Accoutrement : StackPanel
    {
        public const double SymbolSpacer = 8.0;
        public const double ButtonSpacer = 2.0;
        public const double DefaultMinHeight = LightButton.MinBoxSize;
        private TextBlock? _block;
        private LightButton? _button;

        public Accoutrement(bool isLeft, PixieControl owner)
        {
            IsLeft = isLeft;
            Offset = owner._verticalContentOffset;
            Orientation = Orientation.Horizontal;
            VerticalAlignment = owner._verticalContentAlignment;
            MinHeight = DefaultMinHeight;
            Grid.SetRow(this, ContentRow);
            Grid.SetColumn(this, isLeft ? LeftColumn : RightColumn);
            owner._grid.Children.Add(this);
        }

        public readonly bool IsLeft;

        public double Offset
        {
            get { return Margin.Top; }
            set { Margin = new(0.0, Math.Max(value - DefaultVerticalOffset, 0.0), 0.0, 0.0); }
        }

        public LightButton Button
        {
            get
            {
                if (_button != null)
                {
                    return _button;
                }

                _button = new LightButton();
                _button.IsVisible = false;
                _button.Content = Symbols.MoreVert;

                if (IsLeft)
                {
                    _button.Margin = new(0.0, 0.0, ButtonSpacer, 0.0);
                    Children.Add(_button);
                    return _button;
                }

                _button.Margin = new(ButtonSpacer, 0.0, 0.0, 0.0);
                Children.Insert(0, _button);
                return _button;
            }
        }

        public void SetSymbol(string? symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                if (_block != null)
                {
                    Children.Remove(_block);
                    _block = null;
                }

                return;
            }

            if (_block == null)
            {
                _block = new TextBlock();
                _block.VerticalAlignment = VerticalAlignment.Center;
                _block.MaxLines = 1;
                _block.FontSize = ChromeFonts.SymbolFontSize;
                SetTextEnabled(_block, IsEffectivelyEnabled);

                if (IsLeft)
                {
                    _block.Margin = new(0.0, 0.0, SymbolSpacer, 0.0);
                    Children.Insert(0, _block);
                }
                else
                {
                    _block.Margin = new(SymbolSpacer, 0.0, 0.0, 0.0);
                    Children.Add(_block);
                }
            }

            _block.Inlines = ChromeFonts.NewTextRun(symbol);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsEffectivelyEnabledProperty)
            {
                SetTextEnabled(_block, change.GetNewValue<bool>());
            }
        }
    }

}
