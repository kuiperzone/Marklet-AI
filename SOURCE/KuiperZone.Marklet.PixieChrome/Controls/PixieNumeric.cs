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
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A subclass of <see cref="PixieControl"/> displaying an up-down numeric control.
/// </summary>
public sealed partial class PixieNumeric : PixieControl
{
    /// <summary>
    /// Maximum text input length.
    /// </summary>
    public const int MaxInputLength = 33;

    private const double DefaultControlWidth = 100;
    private const double DefaultMaxWidth = 200;
    private const decimal DefaultMaxValue = 10m;
    private const decimal DefaultIncrement = 1m;
    private const bool DefaultAcceptFractionInput = true;
    private const bool DefaultCanEdit = true;
    private readonly StackPanel _itemPanel = new();
    private readonly TextEditor _editor = new();
    private readonly LightButton _resetButton = new();
    private readonly LightButton _upButton = new();
    private readonly LightButton _downButton = new();

    private bool _isSettingEditor;
    private TextBlock? _unitsBlock;

    // Backing fields
    private decimal _value;
    private decimal _default = decimal.MinValue;
    private decimal _minValue;
    private decimal _maxValue = DefaultMaxValue;
    private decimal _increment = DefaultIncrement;
    private bool _acceptFractionInput = DefaultAcceptFractionInput;
    private bool _alwaysShowBorder;
    private bool _canEdit = DefaultCanEdit;
    private string? _units;
    private string? _footer;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieNumeric()
        : base(true, Avalonia.Layout.VerticalAlignment.Center)
    {
        SetChildControl(_itemPanel);

        _itemPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _itemPanel.HorizontalAlignment = ControlAlignment;
        _itemPanel.VerticalAlignment = VerticalContentAlignment;

        _editor.MinWidth = DefaultControlWidth;
        _editor.MaxWidth = Math.Max(DefaultControlWidth, DefaultMaxWidth);
        _editor.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Right;

        _editor.Margin = new(0.0, 0.0, 4.0, 0.0);
        _editor.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _editor.HasBackButton = false;
        _editor.MaxLines = 1;
        _editor.MaxLength = MaxInputLength;
        _editor.Text = _value.ToString(Format);

        _editor.Background = Brushes.Transparent;
        _editor.BorderBrush = Brushes.Transparent;
        SetBackgroundClass(false);

        SetCanEdit(_canEdit);

        _editor.GotFocus += EditorGotFocusHandler;
        _editor.LostFocus += EditorLostFocusHandler;
        _editor.TextChanging += EditorTextChangingHandler;

        var bm = new Thickness(0.0, 0.0, 2.0, 0.0);

        _resetButton.Margin = bm;
        _resetButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _resetButton.Content = Symbols.SettingsBackupRestore;
        _resetButton.IsVisible = false;
        _resetButton.Background = ChromeStyling.ButtonBrush;
        _resetButton.Click += ResetButtonClickHandler;

        _upButton.Margin = bm;
        _upButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _upButton.Content = Symbols.Add;
        _upButton.IsRepeatable = true;
        _upButton.Background = ChromeStyling.ButtonBrush;
        _upButton.Click += UpButtonClickHandler;

        _downButton.Margin = bm;
        _downButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _downButton.Content = Symbols.Remove;
        _downButton.IsRepeatable = true;
        _downButton.Background = ChromeStyling.ButtonBrush;
        _downButton.Click += DownButtonClickHandler;

        _itemPanel.Children.Add(_editor);
        _itemPanel.Children.Add(_upButton);
        _itemPanel.Children.Add(_downButton);
        _itemPanel.Children.Add(_resetButton);
    }

    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, decimal> ValueProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, decimal>(nameof(Value),
        o => o.Value, (o, v) => o.Value = v);

    /// <summary>
    /// Defines the <see cref="Default"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, decimal> DefaultProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, decimal>(nameof(Default),
        o => o.Default, (o, v) => o.Default = v);

    /// <summary>
    /// Defines the <see cref="MinValue"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, decimal> MinValueProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, decimal>(nameof(MinValue),
        o => o.MinValue, (o, v) => o.MinValue = v);

    /// <summary>
    /// Defines the <see cref="MaxValue"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, decimal> MaxValueProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, decimal>(nameof(MaxValue),
        o => o.MaxValue, (o, v) => o.MaxValue = v, DefaultMaxValue);

    /// <summary>
    /// Defines the <see cref="Increment"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, decimal> IncrementProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, decimal>(nameof(Increment),
        o => o.Increment, (o, v) => o.Increment = v, DefaultIncrement);

    /// <summary>
    /// Defines the <see cref="AcceptFractionInput"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, bool> AcceptFractionInputProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, bool>(nameof(AcceptFractionInput),
        o => o.AcceptFractionInput, (o, v) => o.AcceptFractionInput = v, DefaultAcceptFractionInput);

    /// <summary>
    /// Defines the <see cref="AlwaysShowBorder"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, bool> AlwaysShowBorderProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, bool>(nameof(AlwaysShowBorder),
        o => o.AlwaysShowBorder, (o, v) => o.AlwaysShowBorder = v);

    /// <summary>
    /// Defines the <see cref="CanEdit"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, bool> CanEditProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, bool>(nameof(CanEdit),
        o => o.CanEdit, (o, v) => o.CanEdit = v, DefaultCanEdit);

    /// <summary>
    /// Defines the <see cref="Units"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieNumeric, string?> UnitsProperty =
        AvaloniaProperty.RegisterDirect<PixieNumeric, string?>(nameof(Units),
        o => o.Units, (o, v) => o.Units = v);

    /// <summary>
    /// Defines the <see cref="Format"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<PixieSlider, string?>(nameof(FormatProperty));

    /// <summary>
    /// Defines the <see cref="ControlWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ControlWidthProperty =
        AvaloniaProperty.Register<PixieNumeric, double>(nameof(ControlWidth), 100.0);

    /// <summary>
    /// Defines the <see cref="ControlAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Layout.HorizontalAlignment> ControlAlignmentProperty =
        AvaloniaProperty.Register<PixieNumeric, Avalonia.Layout.HorizontalAlignment>(nameof(ControlAlignment),
        Avalonia.Layout.HorizontalAlignment.Right);

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <remarks>
    /// The value is clamp between <see cref="MinValue"/> and <see cref="MaxValue"/>.
    /// </remarks>
    public decimal Value
    {
        get { return _value; }

        set
        {
            var min = GetMiniMax(out decimal max);
            SetAndRaise(ValueProperty, ref _value, Math.Clamp(value, min, max));
        }
    }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    /// <remarks>
    /// The <see cref="Default"/> value allows the user to reset <see cref="Value"/> easily. When <see cref="Default"/>
    /// is within range of <see cref="MinValue"/> and <see cref="MaxValue"/>, an additional "reset" button is shown. The
    /// initial value of <see cref="Default"/> is <see cref="decimal.MinValue"/> which is intended to ensure that the
    /// reset button is NOT shown by default.
    /// </remarks>
    public decimal Default
    {
        get { return _default; }
        set { SetAndRaise(DefaultProperty, ref _default, value); }
    }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    /// <remarks>
    /// Setting will clamp <see cref="Value"/> to the new range. The initial value 10.
    /// </remarks>
    public decimal MinValue
    {
        get { return _minValue; }
        set { SetAndRaise(MinValueProperty, ref _minValue, value); }
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    /// <remarks>
    /// Setting will clamp <see cref="Value"/> to the new range. The initial value is 0.
    /// </remarks>
    public decimal MaxValue
    {
        get { return _maxValue; }
        set { SetAndRaise(MaxValueProperty, ref _maxValue, value); }
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    /// <remarks>
    /// The value must be positive value greater than 0. Setting will clamp <see cref="Value"/> to the new range.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Negative or zero</exception>
    public decimal Increment
    {
        get { return _increment; }

        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(Increment));
            SetAndRaise(IncrementProperty, ref _increment, value);
        }
    }

    /// <summary>
    /// Gets or sets whether the user can type numbers with a fractional part.
    /// </summary>
    /// <remarks>
    /// Setting to false does not prevent programatically setting <see cref="Value"/> or <see cref="Increment"/> to
    /// fractional values, but will prevent the user from typing "1.232" etc. The default is true.
    /// </remarks>
    public bool AcceptFractionInput
    {
        get { return _acceptFractionInput; }
        set { SetAndRaise(AcceptFractionInputProperty, ref _acceptFractionInput, value); }
    }

    /// <summary>
    /// Gets or sets whether to show the editor border.
    /// </summary>
    /// <remarks>
    /// By default the editor border is not shown unless focused. It may be set to true where <see
    /// cref="ControlAlignment"/> is Left. The default is false.
    /// </remarks>
    public bool AlwaysShowBorder
    {
        get { return _alwaysShowBorder; }
        set { SetAndRaise(AlwaysShowBorderProperty, ref _alwaysShowBorder, value); }
    }

    /// <summary>
    /// Gets or sets whether the user can edit the value directly.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool CanEdit
    {
        get { return _canEdit; }
        set { SetAndRaise(CanEditProperty, ref _canEdit, value); }
    }

    /// <summary>
    /// Gets or sets a short "units" string.
    /// </summary>
    /// <remarks>
    /// The default is null.
    /// </remarks>
    public string? Units
    {
        get { return _units; }
        set { SetAndRaise(UnitsProperty, ref _units, value); }
    }

    /// <summary>
    /// Gets or sets the numeric format string.
    /// </summary>
    /// <remarks>
    /// Default is null.
    /// </remarks>
    public string? Format
    {
        get { return GetValue(FormatProperty); }
        set { SetValue(FormatProperty, value); }
    }

    /// <summary>
    /// Gets or sets the editor width excluding the side buttons.
    /// </summary>
    public double ControlWidth
    {
        get { return GetValue(ControlWidthProperty); }
        set { SetValue(ControlWidthProperty, value); }
    }

    /// <summary>
    /// Gets or sets the selectable drop-down alignment.
    /// </summary>
    /// <remarks>
    /// The default is Right.
    /// </remarks>
    public Avalonia.Layout.HorizontalAlignment ControlAlignment
    {
        get { return GetValue(ControlAlignmentProperty); }
        set { SetValue(ControlAlignmentProperty, value); }
    }

    /// <summary>
    /// Gets whether <see cref="Default"/> is within the range [<see cref="MinValue"/>, <see cref="MaxValue"/>].
    /// </summary>
    public bool HasDefault
    {
        get
        {
            var min = GetMiniMax(out decimal max);
            return _default >= min && _default <= max;
        }
    }

    /// <summary>
    /// Overrides and supports variables "{VALUE}", "{MIN}" and "{MAX}".
    /// </summary>
    public override string? Footer
    {
        get { return _footer; }

        set
        {
            if (_footer != value)
            {
                _footer = value;
                SetFooterVariables(Format);
            }
        }
    }

    /// <summary>
    /// Programmatically increments <see cref="Value"/> by <see cref="Increment"/> and returns true if changed.
    /// </summary>
    public bool IncrementValue()
    {
        var value = Value;
        IncrementValue(Increment);
        return value != Value;
    }

    /// <summary>
    /// Programmatically decrements <see cref="Value"/> by <see cref="Increment"/> and returns true if changed.
    /// </summary>
    public bool DecrementValue()
    {
        var value = Value;
        IncrementValue(-Increment);
        return value != Value;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ValueProperty)
        {
            var format = Format;

            try
            {
                _isSettingEditor = true;
                _editor.Text = change.GetNewValue<decimal>().ToString(format);
                return;
            }
            finally
            {
                SetFooterVariables(format);
                OnValueChanged();
                _isSettingEditor = false;
            }
        }

        if (p == DefaultProperty)
        {
            _resetButton.IsVisible = HasDefault;
            return;
        }

        if (p == MinValueProperty || p == MaxValueProperty)
        {
            var min = GetMiniMax(out decimal max);
            Value = Math.Clamp(Value, min, max);
            _resetButton.IsVisible = HasDefault;
            return;
        }

        if (p == FormatProperty)
        {
            try
            {
                _isSettingEditor = true;
                _editor.Text = Value.ToString(change.GetNewValue<string?>());
                return;
            }
            finally
            {
                _isSettingEditor = false;
            }
        }

        if (p == AlwaysShowBorderProperty)
        {
            if (change.GetNewValue<bool>())
            {
                _editor.ClearValue(BorderBrushProperty);
            }
            else
            {
                _editor.BorderBrush = Brushes.Transparent;
            }

            return;
        }

        if (p == ControlWidthProperty)
        {
            var value = change.GetNewValue<double>();
            _editor.MaxWidth = Math.Max(value, DefaultMaxWidth);
            _editor.MinWidth = value;
            return;
        }

        if (p == CanEditProperty)
        {
            SetCanEdit(change.GetNewValue<bool>());
            return;
        }

        if (p == UnitsProperty)
        {
            SetUnits(change.GetNewValue<string?>());
            return;
        }

        if (p == ControlAlignmentProperty)
        {
            var value = change.GetNewValue<Avalonia.Layout.HorizontalAlignment>();
            _itemPanel.HorizontalAlignment = value;
            _editor.HorizontalContentAlignment = value;
            return;
        }
    }

    private decimal GetMiniMax(out decimal max)
    {
        if (_minValue <= _maxValue)
        {
            max = _maxValue;
            return _minValue;
        }

        // Switch
        max = _minValue;
        return _maxValue;
    }

    private void IncrementValue(decimal inc)
    {
        ConditionalDebug.ThrowIfZero(inc);
        var src = Value;
        var value = inc * decimal.Truncate(src / inc);

        if ((value > src && inc > 0m) || value < src && inc < 0m)
        {
            Value = value;
            return;
        }

        Value = value + inc;
    }

    private void SetUnits(string? units)
    {
        units = units?.TrimTitle(8);

        if (!string.IsNullOrEmpty(units))
        {
            if (_unitsBlock == null)
            {
                _unitsBlock = new();
                _unitsBlock.FontSize = ChromeFonts.SmallFontSize;
                _unitsBlock.Foreground = ChromeStyling.ForegroundGray;
                _unitsBlock.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

                const double delta = (ChromeFonts.DefaultFontSize - ChromeFonts.SmallFontSize) / 2.0;
                _unitsBlock.Margin = new(1.0, delta, 4.0, 0.0);

                _editor.InnerRightContent = _unitsBlock;
            }

            _unitsBlock.Text = units;
            return;
        }

        _unitsBlock = null;
        _editor.InnerRightContent = null;
    }

    private void SetCanEdit(bool canEdit)
    {
        _editor.IsReadOnly = !canEdit;
        _editor.Focusable = canEdit;
        _editor.IsHitTestVisible = canEdit;
    }

    private void SetBackgroundClass(bool focused)
    {
        if (focused)
        {
            // Leave it to the control
            _editor.Classes.Remove("fixed-background");
            return;
        }

        _editor.Classes.Add("fixed-background");
    }

    private void SetFooterVariables(string? format)
    {
        if (_itemPanel != null && !string.IsNullOrEmpty(_footer))
        {
            var v0 = _value.ToString(format);
            var min = _minValue.ToString(format);
            var max = _maxValue.ToString(format);
            base.Footer = _footer?.Replace("{VALUE}", v0).Replace("{MIN}", min).Replace("{MAX}", max);
            return;
        }

        base.Footer = _footer;
    }

    private void EditorTextChangingHandler(object? _, EventArgs __)
    {
        if (!_isSettingEditor)
        {
            var text = _editor.Text?.Trim();

            if (!string.IsNullOrEmpty(text))
            {
                var min = GetMiniMax(out decimal max);

                if (!decimal.TryParse(text, out decimal value) || value < min || value > max)
                {
                    _editor.Foreground = ChromeBrushes.RedLightAccent;
                    return;
                }

                if (!AcceptFractionInput && decimal.Truncate(value) != value)
                {
                    _editor.Foreground = ChromeBrushes.RedLightAccent;
                    return;
                }

                // This raises event
                Value = value;
            }

            _editor.ClearValue(TemplatedControl.ForegroundProperty);
        }
    }

    private void EditorGotFocusHandler(object? _, GotFocusEventArgs e)
    {
        SetBackgroundClass(true);
    }

    private void EditorLostFocusHandler(object? _, EventArgs __)
    {
        SetBackgroundClass(false);
        _editor.Text = Value.ToString(Format);
    }

    private void ResetButtonClickHandler(object? _, EventArgs __)
    {
        Value = Default;
    }

    private void UpButtonClickHandler(object? _, EventArgs __)
    {
        IncrementValue(Increment);
    }

    private void DownButtonClickHandler(object? _, EventArgs __)
    {
        IncrementValue(-Increment);
    }

}
