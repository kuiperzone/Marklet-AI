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

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Provides a simple "button" without Avalonia theming or internal button templates.
/// </summary>
/// <remarks>
/// The primary use of <see cref="LightButton"/> is to act as symbolic buttons with its initial background as
/// transparent and borderless. It is intended to be light-weight, theme independent but easy to configure using global
/// style selectors.
/// </remarks>
public class LightButton : Border
{
    /// <summary>
    /// Default MinWidth and MinHeight dimension.
    /// </summary>
    public const double MinBoxSize = ChromeFonts.SymbolFontSize * 1.65;

    /// <summary>
    /// Minimum <see cref="FontScale"/> value.
    /// </summary>
    public const double MinFontScale = 0.1;

    /// <summary>
    /// Maximum <see cref="FontScale"/> value.
    /// </summary>
    public const double MaxFontScale = 10.0;

    private const TextAlignment DefaultAlignment = TextAlignment.Center;
    private static readonly Thickness DefaultContentPadding = new(double.NaN);
    private static readonly TimeSpan RepeatInitialInterval = TimeSpan.FromMilliseconds(600);
    private static readonly IBrush DefaultCheckedPressedBackground = ChromeStyling.ButtonCheckedPressed;
    private static readonly IBrush DefaultHoverBackground = ChromeStyling.ButtonHover;

    private sealed class InnerBorder : Border;
    private sealed class InnerText : TextBlock;

    private readonly InnerText _block = new();
    private readonly InnerBorder _underlay = new();
    private bool _isHovering;
    private bool _isPressing;
    private DispatcherTimer? _repeatTimer;

    // Backing fields
    private ICommand? _command;
    private object? _commandParameter;
    private KeyGesture? _displayGesture;
    private string? _content;
    private TextAlignment _contentAlignment = DefaultAlignment;
    private FontWeight _fontWeight = FontWeight.Normal;
    private bool _isRepeatable;
    private bool _isChecked;
    private bool _canToggle;
    private ContextMenu? _dropMenu;
    private string? _tip;

    private double _fontSize = double.NaN;
    private double _fontScale = 1.0;
    private Thickness _contentPadding = DefaultContentPadding;

    // Styled cached
    private IBrush? _foreground;
    private IBrush _hoverBackground = DefaultHoverBackground;
    private IBrush _checkedPressedBackground = DefaultCheckedPressedBackground;
    private IBrush? _hoverForeground;
    private IBrush? _checkedPressedForeground;

    /// <summary>
    /// Constructor.
    /// </summary>
    public LightButton()
    {
        base.Child = _underlay;
        _underlay.Child = _block;

        Focusable = true;
        base.FocusAdorner = null;

        SetCurrentValue(MinWidthProperty, MinBoxSize);
        SetCurrentValue(MinHeightProperty, MinBoxSize);

        // Don't need to set FontSize as we use Inlines exclusively
        _block.TextWrapping = TextWrapping.NoWrap;
        _block.TextAlignment = _contentAlignment;
        _block.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

        _underlay.ClipToBounds = true;
        _underlay.Background = Brushes.Transparent;
        _underlay.PointerPressed += PointerPressedHandler;
        _underlay.PointerReleased += PointerReleasedHandler;
        _underlay.PointerEntered += PointerEnteredHandler;
        _underlay.PointerExited += PointerExitedHandler;
    }

    /// <summary>
    /// Defines the <see cref="Click"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<LightButton, RoutedEventArgs>(nameof(Click), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="CheckedChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CheckedChangedEvent =
        RoutedEvent.Register<LightButton, RoutedEventArgs>(nameof(CheckedChanged), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="Command"/> event.
    /// </summary>
    public static readonly DirectProperty<LightButton, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<LightButton, ICommand?>(nameof(Command),
        o => o.Command, (o, v) => o.Command = v);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, object?> CommandParameterProperty =
        AvaloniaProperty.RegisterDirect<LightButton, object?>(nameof(CommandParameter),
        o => o.CommandParameter, (o, v) => o.CommandParameter = v);

    /// <summary>
    /// Defines the <see cref="Gesture"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, KeyGesture?> GestureProperty =
        AvaloniaProperty.RegisterDirect<LightButton, KeyGesture?>(nameof(Gesture),
        o => o.Gesture, (o, v) => o.Gesture = v);

    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, string?> ContentProperty =
        AvaloniaProperty.RegisterDirect<LightButton, string?>(nameof(Content),
        o => o.Content, (o, v) => o.Content = v);

    /// <summary>
    /// Defines the <see cref="ContentAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, TextAlignment> ContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<LightButton, TextAlignment>(nameof(ContentAlignment),
        o => o.ContentAlignment, (o, v) => o.ContentAlignment = v, DefaultAlignment);

    /// <summary>
    /// Defines the <see cref="FontWeight"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, FontWeight> FontWeightProperty =
        AvaloniaProperty.RegisterDirect<LightButton, FontWeight>(nameof(FontWeight),
        o => o.FontWeight, (o, v) => o.FontWeight = v, FontWeight.Normal);

    /// <summary>
    /// Defines the <see cref="IsRepeatable"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, bool> IsRepeatableProperty =
        AvaloniaProperty.RegisterDirect<LightButton, bool>(nameof(IsRepeatable),
        o => o.IsRepeatable, (o, v) => o.IsRepeatable = v);

    /// <summary>
    /// Defines the <see cref="IsChecked"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, bool> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<LightButton, bool>(nameof(IsChecked),
        o => o.IsChecked, (o, v) => o.IsChecked = v);

    /// <summary>
    /// Defines the <see cref="CanToggle"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, bool> CanToggleProperty =
        AvaloniaProperty.RegisterDirect<LightButton, bool>(nameof(CanToggle),
        o => o.CanToggle, (o, v) => o.CanToggle = v);

    /// <summary>
    /// Defines the <see cref="DropMenu"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, ContextMenu?> DropMenuProperty =
        AvaloniaProperty.RegisterDirect<LightButton, ContextMenu?>(nameof(DropMenu),
        o => o.DropMenu, (o, v) => o.DropMenu = v);

    /// <summary>
    /// Defines the <see cref="Tip"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, string?> TipProperty =
        AvaloniaProperty.RegisterDirect<LightButton, string?>(nameof(Tip),
        o => o.Tip, (o, v) => o.Tip = v);

    /// <summary>
    /// Defines the <see cref="FontSize"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, double> FontSizeProperty =
        AvaloniaProperty.RegisterDirect<LightButton, double>(nameof(FontSize),
        o => o.FontSize, (o, v) => o.FontSize = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="FontScale"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, double> FontScaleProperty =
        AvaloniaProperty.RegisterDirect<LightButton, double>(nameof(FontScale),
        o => o.FontScale, (o, v) => o.FontScale = v, 1.0);

    /// <summary>
    /// Defines the <see cref="ContentPadding"/> property.
    /// </summary>
    public static readonly DirectProperty<LightButton, Thickness> ContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<LightButton, Thickness>(nameof(ContentPadding),
        o => o.ContentPadding, (o, v) => o.ContentPadding = v, DefaultContentPadding);

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<LightButton, IBrush?>(nameof(Foreground));

    /// <summary>
    /// Defines the <see cref="HoverForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HoverForegroundProperty =
        AvaloniaProperty.Register<LightButton, IBrush?>(nameof(HoverForeground));

    /// <summary>
    /// Defines the <see cref="HoverBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> HoverBackgroundProperty =
        AvaloniaProperty.Register<LightButton, IBrush>(nameof(HoverBackground), DefaultHoverBackground);

    /// <summary>
    /// Defines the <see cref="CheckedPressedForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> CheckedPressedForegroundProperty =
        AvaloniaProperty.Register<LightButton, IBrush?>(nameof(CheckedPressedForeground));

    /// <summary>
    /// Defines the <see cref="CheckedPressedBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> CheckedPressedBackgroundProperty =
        AvaloniaProperty.Register<LightButton, IBrush>(nameof(CheckedPressedBackground), DefaultCheckedPressedBackground);

    /// <summary>
    /// Defines the <see cref="DisabledForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> DisabledForegroundProperty =
        AvaloniaProperty.Register<LightButton, IBrush>(nameof(DisabledForeground), ChromeStyling.ForegroundGray);

    /// <summary>
    /// Defines the <see cref="FocusBorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FocusBorderBrushProperty =
        AvaloniaProperty.Register<LightButton, IBrush?>(nameof(FocusBorderBrush));

    /// <summary>
    /// Occurs when the user left-clicks.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Click
    {
        add { AddHandler(ClickEvent, value); }
        remove { RemoveHandler(ClickEvent, value); }
    }

    /// <summary>
    /// Occurs when <see cref="IsChecked"/> changes.
    /// </summary>
    public event EventHandler<RoutedEventArgs> CheckedChanged
    {
        add { AddHandler(CheckedChangedEvent, value); }
        remove { RemoveHandler(CheckedChangedEvent, value); }
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the button is clicked.
    /// </summary>
    public ICommand? Command
    {
        get { return _command; }
        set { SetAndRaise(CommandProperty, ref _command, value); }
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command"/>.
    /// </summary>
    public object? CommandParameter
    {
        get { return _commandParameter; }
        set { SetAndRaise(CommandParameterProperty, ref _commandParameter, value); }
    }

    /// <summary>
    /// Gets or sets a key gesture.
    /// </summary>
    /// <remarks>
    /// The value is appended to <see cref="Tip"/> provided the tip is not empty. The button does not respond directly
    /// to the given key input, however, <see cref="HandleKeyGesture"/> can be used by a caller.
    /// </remarks>
    public KeyGesture? Gesture
    {
        get { return _displayGesture; }
        set { SetAndRaise(GestureProperty, ref _displayGesture, value); }
    }

    /// <summary>
    /// Gets or sets the content string.
    /// </summary>
    /// <remarks>
    /// Content characters in the Unicode "private use" range are shown using the "material-symbol" font family.
    /// </remarks>
    public string? Content
    {
        get { return _content; }
        set { SetAndRaise(ContentProperty, ref _content, value); }
    }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    /// <remarks>
    /// Default is Center.
    /// </remarks>
    public TextAlignment ContentAlignment
    {
        get { return _contentAlignment; }
        set { SetAndRaise(ContentAlignmentProperty, ref _contentAlignment, value); }
    }

    /// <summary>
    /// Gets or sets the font weight.
    /// </summary>
    public FontWeight FontWeight
    {
        get { return _fontWeight; }
        set { SetAndRaise(FontWeightProperty, ref _fontWeight, value); }
    }

    /// <summary>
    /// Gets or sets whether the button is repeatable.
    /// </summary>
    public bool IsRepeatable
    {
        get { return _isRepeatable; }
        set { SetAndRaise(IsRepeatableProperty, ref _isRepeatable, value); }
    }

    /// <summary>
    /// Gets or sets whether the button is in "checked" state.
    /// </summary>
    /// <remarks>
    /// The initial value is false.
    /// </remarks>
    public bool IsChecked
    {
        get { return _isChecked; }
        set { SetAndRaise(IsCheckedProperty, ref _isChecked, value); }
    }

    /// <summary>
    /// Gets or sets whether the button toggles <see cref="IsChecked"/> automatically when clicked.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsChecked"/> state can still be set programmatically when <see cref="CanToggle"/> is false. The
    /// initial value is false.
    /// </remarks>
    public bool CanToggle
    {
        get { return _canToggle; }
        set { SetAndRaise(CanToggleProperty, ref _canToggle, value); }
    }

    /// <summary>
    /// Gets or sets a <see cref="ContextMenu"/> shown under the button when clicked.
    /// </summary>
    /// <remarks>
    /// The menu is shown after the <see cref="Click"/> handler is invoked. No menu is shown if the handler sets <see
    /// cref="RoutedEventArgs.Handled"/> to true.
    /// </remarks>
    public ContextMenu? DropMenu
    {
        get { return _dropMenu; }
        set { SetAndRaise(DropMenuProperty, ref _dropMenu, value); }
    }

    /// <summary>
    /// Gets or sets the tool-tip.
    /// </summary>
    public string? Tip
    {
        get { return _tip; }
        set { SetAndRaise(TipProperty, ref _tip, value); }
    }

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    /// <remarks>
    /// Where the value is NaN, the actual font size is determined automatically for individual characters in
    /// conjunction with <see cref="FontScale"/>. Where a positive non-zero value, the font size is fixed. The default
    /// is NaN.
    /// </remarks>
    public double FontSize
    {
        get { return _fontSize; }
        set { SetAndRaise(FontSizeProperty, ref _fontSize, value); }
    }

    /// <summary>
    /// Gets or sets the "font scale".
    /// </summary>
    /// <remarks>
    /// This is used only where <see cref="FontSize"/> is NaN, otherwise it is ignored. The value is clamped between
    /// <see cref="MinFontScale"/> and <see cref="MaxFontScale"/>. Setting NaN throws. The default is 1.0.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid NaN</exception>
    public double FontScale
    {
        get { return _fontScale; }
        set { SetAndRaise(FontScaleProperty, ref _fontScale, ClampFontScale(value)); }
    }

    /// <summary>
    /// Gets or sets the text padding.
    /// </summary>
    /// <remarks>
    /// Use this to set padding rather than Border.Padding. Where individual side values are NaN, the respective side
    /// padding is calculated automatically based on <see cref="FontSize"/>. The default is Thickness(NaN).
    /// </remarks>
    public Thickness ContentPadding
    {
        get { return _contentPadding; }
        set { SetAndRaise(ContentPaddingProperty, ref _contentPadding, value); }
    }

    /// <summary>
    /// Gets or sets the foreground brush.
    /// </summary>
    public IBrush? Foreground
    {
        get { return _foreground; }
        set { SetValue(ForegroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the foreground when <see cref="HoverBackground"/> is operative.
    /// </summary>
    /// <remarks>
    /// The <see cref="Foreground"/> is used where <see cref="HoverForeground"/> is null. The default is null.
    /// </remarks>
    public IBrush? HoverForeground
    {
        get { return _hoverForeground; }
        set { SetValue(HoverForegroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the background when hovering.
    /// </summary>
    /// <remarks>
    /// The default is a gray which works on both dark and light themes. The property is not nullable, but can be set to
    /// transparent.
    /// </remarks>
    public IBrush HoverBackground
    {
        get { return _hoverBackground; }
        set { SetValue(HoverBackgroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the foreground when <see cref="CheckedPressedBackground"/> is used.
    /// </summary>
    /// <remarks>
    /// The <see cref="Foreground"/> is used where <see cref="CheckedPressedForeground"/> is null. The default is null.
    /// </remarks>
    public IBrush? CheckedPressedForeground
    {
        get { return _checkedPressedForeground; }
        set { SetValue(CheckedPressedForegroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the background both when <see cref="IsChecked"/> is true and when pressed.
    /// </summary>
    /// <remarks>
    /// The default is a gray which works on both dark and light themes. The property is not nullable, but can be set to
    /// transparent.
    /// </remarks>
    public IBrush CheckedPressedBackground
    {
        get { return _checkedPressedBackground; }
        set { SetValue(CheckedPressedBackgroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the foreground disabled color.
    /// </summary>
    /// <remarks>
    /// The default is a gray which works on both dark and light themes. The property is not nullable.
    /// </remarks>
    public IBrush DisabledForeground
    {
        get { return GetValue(DisabledForegroundProperty); }
        set { SetValue(DisabledForegroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the focus border brush with which to create a focus rectangle.
    /// </summary>
    /// <remarks>
    /// If null, the <see cref="HoverBackground"/> is used to indicate focus using a background brush instead. The
    /// default is null (use background).
    /// </remarks>
    public IBrush? FocusBorderBrush
    {
        get { return GetValue(FocusBorderBrushProperty); }
        set { SetValue(FocusBorderBrushProperty, value); }
    }

    /// <summary>
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Do not use. Use <see cref="ContentPadding"/> instead.
    /// </summary>
    [Obsolete($"Do not use. Use {nameof(ContentPadding)} instead.", true)]
    public new Thickness Padding
    {
        get { return base.Padding; }
        set { base.Padding = value; }
    }

    /// <summary>
    /// Do not use. Use <see cref="ContentPadding"/> instead.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new ITemplate<Control>? FocusAdorner
    {
        get { return base.FocusAdorner; }
        set { base.FocusAdorner = value; }
    }

    /// <summary>
    /// Programmatically clicks button.
    /// </summary>
    /// <remarks>
    /// The event occurs even if the button is disabled or invisible.
    /// </remarks>
    public void OnClick()
    {
        var command = Command;
        var parameter = CommandParameter;

        if (CanToggle)
        {
            IsChecked = !IsChecked;
        }

        if (command != null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }

        var e1 = new RoutedEventArgs(ClickEvent, this);

        RaiseEvent(e1);

        if (!e1.Handled)
        {
            var menu = DropMenu;

            if (menu?.IsOpen == false && menu.Normalize())
            {
                menu.HorizontalOffset = 0;
                menu.VerticalOffset = 0;
                menu.Placement = PlacementMode.Bottom;
                menu.Open(this);
            }
        }
    }

    /// <summary>
    /// Calls <see cref="OnClick"/> if the given key input matches <see cref="Gesture"/> and returns true on success.
    /// </summary>
    /// <remarks>
    /// The <see cref="OnClick"/> is NOT called where <see cref="RoutedEventArgs.Handled"/> is already true, nor if the
    /// button is disabled or invisible. If "dropMenu" is true, any <see cref="MenuItem.InputGesture"/> keys in <see
    /// cref="DropMenu"/> will also be handled. On success, the <see cref="RoutedEventArgs.Handled"/> value is set to
    /// true on return.
    /// </remarks>
    public bool HandleKeyGesture(KeyEventArgs e, bool dropMenu = true)
    {
        if (!e.Handled && IsEffectivelyEnabled && IsEffectivelyVisible)
        {
            if (Gesture?.Matches(e) == true)
            {
                e.Handled = true;
                OnClick();
                return true;
            }

            if (dropMenu && HandleMenuKeyGesture(DropMenu?.Items, e))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (IsFocused && e.KeyModifiers == KeyModifiers.None && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            e.Handled = true;
            UpdateBlockColors(true);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (IsFocused && e.KeyModifiers == KeyModifiers.None && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            e.Handled = true;
            UpdateBlockColors(_isPressing);
            OnClick();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        // Only allow focus if it comes from keyboard navigation
        if (e.NavigationMethod != NavigationMethod.Tab && e.NavigationMethod != NavigationMethod.Directional)
        {
            e.Handled = true;
            return;
        }

        base.OnGotFocus(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChildProperty && change.OldValue != null)
        {
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }

        if (p == PaddingProperty)
        {
            // Use AdornerPadding
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }

        if (p == CornerRadiusProperty)
        {
            // Copy to
            _underlay.CornerRadius = change.GetNewValue<CornerRadius>();
            return;
        }

        // BUTTON PROPERTIES
        if (p == ContentProperty || p == FontSizeProperty || p == FontScaleProperty)
        {
            UpdateBlockContent();
            UpdateContentPadding();
            return;
        }

        if (p == ContentPaddingProperty)
        {
            UpdateContentPadding();
            return;
        }

        if (p == ContentAlignmentProperty)
        {
            _block.TextAlignment = change.GetNewValue<TextAlignment>();
            return;
        }

        if (p == GestureProperty || p == TipProperty)
        {
            var tip = Tip;

            if (!string.IsNullOrEmpty(tip))
            {
                if (Gesture != null)
                {
                    var block = new TextBlock();
                    var inlines = new InlineCollection();

                    inlines.Add(new Run(tip + '\u2002'));

                    var run = new Run(Gesture.ToString());
                    run.Foreground = ChromeStyling.ForegroundGray;
                    inlines.Add(run);

                    block.Inlines = inlines;
                    ToolTip.SetTip(this, block);
                    return;
                }

                ToolTip.SetTip(this, tip);
                return;
            }

            ToolTip.SetTip(this, null);
            return;
        }

        if (p == FontWeightProperty)
        {
            _block.FontWeight = change.GetNewValue<FontWeight>();
            return;
        }

        if (p == ForegroundProperty)
        {
            // Cached
            _foreground = change.GetNewValue<IBrush?>();
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == HoverBackgroundProperty)
        {
            // Cached
            _hoverBackground = change.GetNewValue<IBrush>();
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == HoverForegroundProperty)
        {
            // Cached
            _hoverForeground = change.GetNewValue<IBrush?>();
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == CheckedPressedBackgroundProperty)
        {
            // Cached
            _checkedPressedBackground = change.GetNewValue<IBrush>();
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == CheckedPressedForegroundProperty)
        {
            // Cached
            _checkedPressedForeground = change.GetNewValue<IBrush?>();
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == IsCheckedProperty)
        {
            UpdateBlockColors(_isPressing);
            RaiseEvent(new RoutedEventArgs(CheckedChangedEvent, this));
            return;
        }

        if (p == IsFocusedProperty)
        {
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == FocusBorderBrushProperty)
        {
            var value = change.GetNewValue<IBrush?>();
            base.FocusAdorner = value != null ? ChromeStyling.NewAdorner() : null;
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == IsRepeatableProperty)
        {
            if (change.GetNewValue<bool>())
            {
                if (_repeatTimer == null)
                {
                    _repeatTimer = new();
                    _repeatTimer.Interval = RepeatInitialInterval;
                    _repeatTimer.Tick += RepeatTimerTickHandler;
                }

                return;
            }

            _repeatTimer?.Stop();
            _repeatTimer = null;
            return;
        }

        if (p == IsEffectivelyEnabledProperty)
        {
            UpdateBlockColors(_isPressing);
            return;
        }

        if (p == DisabledForegroundProperty)
        {
            if (!IsEffectivelyEnabled)
            {
                _block.Foreground = change.GetNewValue<IBrush?>();
            }

            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Want arrow cursor even if parent uses different
        Cursor ??= ChromeCursors.ArrowCursor;
    }

    private static bool HandleMenuKeyGesture(IEnumerable<object?>? menu, KeyEventArgs e)
    {
        if (menu != null && !e.Handled)
        {
            foreach (var item in menu)
            {
                if (item is MenuItem m && m.IsEffectivelyEnabled && m.IsEffectivelyVisible)
                {
                    if (m.InputGesture?.Matches(e) == true)
                    {
                        e.Handled = true;
                        m.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, m));
                        return true;
                    }

                    if (HandleMenuKeyGesture(m.Items, e))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static double ClampFontScale(double scale)
    {
        if (double.IsNaN(scale))
        {
            throw new ArgumentException("Invalid NaN", nameof(FontScale));
        }

        return Math.Clamp(scale, MinFontScale, MaxFontScale);
    }

    // Needs to be called if ContentPadding, FontSize, FontScale, or Content change
    private void UpdateContentPadding()
    {
        // Chosen not to use FontScale on AppFontSize as we would have to also deal with MaxWidth and MaxHeight
        var fs = _fontSize > 0.0 ? _fontSize : ChromeFonts.DefaultFontSize;

        double left, right;
        var pad = _contentPadding;

        if (double.IsNaN(pad.Left))
        {
            if (_content?.Length > 1)
            {
                // More than one char - this is not expected to be a square
                left = fs * 0.75;
            }
            else
            {
                // Padding intended to maintain an approx. square
                left = fs * 0.35;
            }
        }
        else
        {
            left = Math.Max(pad.Left, 0.0);
        }

        if (double.IsNaN(pad.Right))
        {
            if (_content?.Length > 1)
            {
                right = fs * 0.75;
            }
            else
            {
                right = fs * 0.35;
            }
        }
        else
        {
            right = Math.Max(pad.Right, 0.0);
        }

        var top = double.IsNaN(pad.Top) ? fs * 0.20 : Math.Max(pad.Top, 0.0);
        var bottom = double.IsNaN(pad.Bottom) ? fs * 0.20 : Math.Max(pad.Bottom, 0.0);

        // We actually set it on Margin
        _block.Margin = new(left, top, right, bottom);
    }

    private void UpdateBlockContent()
    {
        _block.Inlines = ChromeFonts.NewTextRun(_content, _fontSize, _fontScale);
    }

    private void UpdateBlockColors(bool pressing)
    {
        // BACKGROUND
        bool hover = false;

        if (pressing)
        {
            _underlay.Background = _checkedPressedBackground;
        }
        else
        if (_isHovering || (IsFocused && base.FocusAdorner == null))
        {
            hover = true;
            _underlay.Background = _hoverBackground;
        }
        else
        if (IsChecked)
        {
            pressing = true;
            _underlay.Background = _checkedPressedBackground;
        }
        else
        {
            _underlay.Background = Brushes.Transparent;
        }


        // ENABLED FOREGROUND
        if (IsEffectivelyEnabled)
        {
            if (hover && _hoverForeground != null)
            {
                _block.Foreground = _hoverForeground;
                return;
            }

            if (pressing && _checkedPressedForeground != null)
            {
                _block.Foreground = _checkedPressedForeground;
                return;
            }

            if (_foreground != null)
            {
                _block.Foreground = _foreground;
                return;
            }

            _block.ClearValue(TextBlock.ForegroundProperty);
            return;
        }


        // DISABLED
        _block.Foreground = DisabledForeground;
    }

    private void PointerPressedHandler(object? _, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            _isPressing = true;
            UpdateBlockColors(true);

            if (_repeatTimer != null)
            {
                _repeatTimer.Interval = RepeatInitialInterval;
                _repeatTimer.Restart();
            }
        }
    }

    private void PointerReleasedHandler(object? _, PointerReleasedEventArgs e)
    {
        if (_isPressing)
        {
            e.Handled = true;
            _isPressing = false;
            _repeatTimer?.Stop();
            UpdateBlockColors(false);
            OnClick();
        }
    }

    private void PointerEnteredHandler(object? _, RoutedEventArgs __)
    {
        _isHovering = true;
        _isPressing = false;
        UpdateBlockColors(false);
    }

    private void PointerExitedHandler(object? _, RoutedEventArgs __)
    {
        _isHovering = false;
        _isPressing = false;
        UpdateBlockColors(false);
    }

    private void RepeatTimerTickHandler(object? _, EventArgs __)
    {
        if (_repeatTimer != null)
        {
            if (_repeatTimer.Interval == RepeatInitialInterval)
            {
                _repeatTimer.Interval = TimeSpan.FromMilliseconds(100);
                _repeatTimer.Restart();
            }

            OnClick();
        }
    }
}