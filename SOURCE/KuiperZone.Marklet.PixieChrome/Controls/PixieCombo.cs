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
using Avalonia.Interactivity;
using Avalonia.Collections;
using Avalonia.Metadata;
using Avalonia.Controls.Templates;
using System.Collections.Specialized;
using Avalonia.Controls.Primitives;
using KuiperZone.Marklet.Tooling;
using Avalonia.Layout;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Composite control with internal ComboBox.
/// </summary>
/// <remarks>
/// The <see cref="PixieControl.ValueChanged"/> event occurs when <see cref="SelectedIndex"/> changes, not the <see
/// cref="Text"/> value.
/// </remarks>
public sealed class PixieCombo : PixieControl
{
    private const int DefaultSelectedIndex = -1;
    private const int DefaultMaxEditLength = 256;
    private const double DefaultMinContentWidth = 100;
    private const double DefaultMaxContentWidth = double.PositiveInfinity;
    private readonly ComboBox _comboBox = new();
    private TextBox? _innerBox;
    private StackPanel? _dropPanel;

    // Backing fields
    private int _selectedIndex = DefaultSelectedIndex;
    private string? _text;
    private bool _isEditable;
    private int _maxEditLength = DefaultMaxEditLength;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieCombo()
        : base(true, VerticalAlignment.Center)
    {
        SetChildControl(_comboBox);

        _comboBox.Classes.Add("corner-styling");
        SetComboBoxBorder(false);

        _comboBox.Background = Brushes.Transparent;
        _comboBox.MaxDropDownHeight = MaxDropHeight;
        _comboBox.VerticalAlignment = VerticalContentAlignment;
        _comboBox.MinWidth = DefaultMinContentWidth;
        _comboBox.MaxWidth = DefaultMaxContentWidth;

        var align = ControlAlignment; // <- follow init
        _comboBox.HorizontalAlignment = align;
        _comboBox.HorizontalContentAlignment = align;

        _comboBox.ItemsPanel = new FuncTemplate<Panel?>(CreateItemsPanel);
        _comboBox.GotFocus += InnerGotFocusHandler;
        _comboBox.LostFocus += InnerLostFocusHandler;
        _comboBox.DropDownClosed += InnerClosedHandler;
        _comboBox.SelectionChanged += InnerSelectionIndexChangedHandler;
        _comboBox.TemplateApplied += InnerTemplateAppliedHandler;

        var route = RoutingStrategies.Bubble;
        _comboBox.AddHandler(KeyDownEvent, InnerKeyDownHandler, route);
        _comboBox.AddHandler(TextInputEvent, InnerTextInputHandler, route);
        _comboBox.AddHandler(TextBox.TextChangedEvent, InnerTextChangedHandler, route);

        Items.CollectionChanged += ItemsChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="EditSubmitted"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> EditSubmittedEvent =
        RoutedEvent.Register<PixieControl, RoutedEventArgs>(nameof(EditSubmitted), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, AvaloniaList<string>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, AvaloniaList<string>>(
            nameof(Items), o => o.Items, (o, v) => o.Items = v ?? new AvaloniaList<string>());

    /// <summary>
    /// Defines the <see cref="SelectedIndex"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, int>(nameof(SelectedIndex),
        o => o.SelectedIndex, (o, v) => o.SelectedIndex = v, DefaultSelectedIndex);

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, string?> TextProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, string?>(nameof(Text),
        o => o.Text, (o, v) => o.Text = v);

    /// <summary>
    /// Defines the <see cref="IsEditable"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, bool> IsEditableProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, bool>(nameof(IsEditable),
        o => o.IsEditable, (o, v) => o.IsEditable = v);

    /// <summary>
    /// Defines the <see cref="MaxEditLength"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, int> MaxEditLengthProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, int>(nameof(MaxEditLength),
        o => o.MaxEditLength, (o, v) => o.MaxEditLength = v, DefaultMaxEditLength);

    /// <summary>
    /// Defines the <see cref="MaxDropHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxDropHeightProperty =
        AvaloniaProperty.Register<PixieCombo, double>(nameof(MaxDropHeight), 300.0);

    /// <summary>
    /// Defines the <see cref="MinControlWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinControlWidthProperty =
        AvaloniaProperty.Register<PixieCombo, double>(nameof(MinControlWidth), DefaultMinContentWidth);

    /// <summary>
    /// Defines the <see cref="MaxControlWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxControlWidthProperty =
        AvaloniaProperty.Register<PixieCombo, double>(nameof(MaxControlWidth), DefaultMaxContentWidth);

    /// <summary>
    /// Defines the <see cref="ControlAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> ControlAlignmentProperty =
        AvaloniaProperty.Register<PixieCombo, HorizontalAlignment>(nameof(ControlAlignment),
        HorizontalAlignment.Right);

    /// <summary>
    /// Occurs only when the user presses the ENTER key where <see cref="IsEditable"/> is true.
    /// </summary>
    public event EventHandler<RoutedEventArgs> EditSubmitted
    {
        add { AddHandler(EditSubmittedEvent, value); }
        remove { RemoveHandler(EditSubmittedEvent, value); }
    }

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <remarks>
    /// The default is empty.
    /// </remarks>
    [Content]
    public AvaloniaList<string> Items { get; private set; } = new();

    /// <summary>
    /// Gets or sets the selected index.
    /// </summary>
    /// <remarks>
    /// The <see cref="PixieControl.ValueChanged"/> event occurs when <see cref="SelectedIndex"/> is changed. The
    /// default is -1 until a selection is made or set.
    /// </remarks>
    public int SelectedIndex
    {
        get { return _selectedIndex; }
        set { SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value); }
    }

    /// <summary>
    /// Gets or sets the editable text.
    /// </summary>
    /// <remarks>
    /// The value may change when <see cref="SelectedIndex"/> changes or when the user inputs text where <see
    /// cref="IsEditable"/> is true. It may also be set where <see cref="IsEditable"/> is true. The <see
    /// cref="PixieControl.ValueChanged"/> does not occur when <see cref="Text"/> changes, but may occur if the change
    /// causes <see cref="SelectedIndex"/> to change. Setting does nothing where <see cref="IsEditable"/> is false.
    /// </remarks>
    public string? Text
    {
        get { return _text; }

        set
        {
            if (_isEditable)
            {
                SetAndRaise(TextProperty, ref _text, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the user can edit text.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool IsEditable
    {
        get { return _isEditable; }
        set { SetAndRaise(IsEditableProperty, ref _isEditable, value); }
    }

    /// <summary>
    /// Gets or sets the maximum user edit length.
    /// </summary>
    /// <remarks>
    /// The value does not restrict <see cref="Text"/> length set programmatically, nor does setting a lower value
    /// truncate existing text. The default is 256.
    /// </remarks>
    public int MaxEditLength
    {
        get { return _maxEditLength; }
        set { SetAndRaise(MaxEditLengthProperty, ref _maxEditLength, value); }
    }

    /// <summary>
    /// Gets or sets the maximum drop-down height.
    /// </summary>
    public double MaxDropHeight
    {
        get { return GetValue(MaxDropHeightProperty); }
        set { SetValue(MaxDropHeightProperty, value); }
    }

    /// <summary>
    /// Gets or sets the minimum control width.
    /// </summary>
    public double MinControlWidth
    {
        get { return GetValue(MinControlWidthProperty); }
        set { SetValue(MinControlWidthProperty, value); }
    }

    /// <summary>
    /// Gets or sets the maximum control width.
    /// </summary>
    /// <remarks>
    /// When inputting text into an editable control, its width may grow. This can be used to clamp it.
    /// </remarks>
    public double MaxControlWidth
    {
        get { return GetValue(MaxControlWidthProperty); }
        set { SetValue(MaxControlWidthProperty, value); }
    }

    /// <summary>
    /// Gets or sets the selectable drop-down alignment.
    /// </summary>
    /// <remarks>
    /// The default is Right.
    /// </remarks>
    public HorizontalAlignment ControlAlignment
    {
        get { return GetValue(ControlAlignmentProperty); }
        set { SetValue(ControlAlignmentProperty, value); }
    }

    /// <summary>
    /// Populates <see cref="Items"/> directly from values of enum type T.
    /// </summary>
    /// <remarks>
    /// See also <see cref="GetSelectedIndexAs{T}(T)"/>.
    /// </remarks>
    public string[] SetItemsAs<T>() where T : struct, Enum
    {
        var values = Enum.GetNames<T>();

        for (int n = 0; n < values.Length; ++n)
        {
            values[n] = values[n].GetFriendlyNameOf();
        }

        Items.Clear();
        Items.AddRange(values);
        return values;
    }

    /// <summary>
    /// Converts <see cref="SelectedIndex"/> numeric value to an enum of T, or returns "def" if index is not valid.
    /// </summary>
    public T GetSelectedIndexAs<T>(T def = default) where T : struct, Enum
    {
        var value = (T)(object)SelectedIndex;

        if (Enum.IsDefined<T>(value))
        {
            return value;
        }

        return def;
    }

    /// <summary>
    /// Overrides and also searches <see cref="Items"/>.
    /// </summary>
    public override bool Find(string? keyword, List<PixieFinding>? findings)
    {
        if (base.Find(keyword, findings))
        {
            return true;
        }

        if (keyword == null || keyword.Length < 3)
        {
            // Ignore short strings as results will be meaningless
            return false;
        }

        foreach (var item in Items)
        {
            if (item.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                findings?.Add(new(this));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == SelectedIndexProperty)
        {
            _comboBox.SelectedIndex = change.GetNewValue<int>();
            return;
        }

        if (p == TextProperty)
        {
            // Already linked up to fire event when changes
            _comboBox.Text = change.GetNewValue<string?>();
            return;
        }

        if (p == IsEditableProperty)
        {
            // See below as to why we combine with enabled
            _comboBox.IsEditable = change.GetNewValue<bool>() && IsEffectivelyEnabled;
            _comboBox.HorizontalContentAlignment = GetInternalAlignment(ControlAlignment);
            return;
        }

        if (p == MaxEditLengthProperty)
        {
            // Nothing needed to be done?
            return;
        }

        if (p == MinControlWidthProperty)
        {
            _comboBox.MinWidth = change.GetNewValue<double>();
            return;
        }

        if (p == MaxControlWidthProperty)
        {
            _comboBox.MaxWidth = change.GetNewValue<double>();
            return;
        }

        if (p == MaxDropHeightProperty)
        {
            _comboBox.MaxDropDownHeight = change.GetNewValue<double>();
            return;
        }

        if (p == ControlAlignmentProperty)
        {
            var value = change.GetNewValue<HorizontalAlignment>();
            _comboBox.HorizontalAlignment = value;
            _comboBox.HorizontalContentAlignment = GetInternalAlignment(value);
            return;
        }

        if (p == IsEffectivelyEnabledProperty)
        {
            // Fighting fluent
            // Editable ComboBox shows jankeyness which disabled.
            // Therefore we temporarily turn editable off when disabled.
            _comboBox.IsEditable = IsEditable && change.GetNewValue<bool>();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        const string NSpace = $"{nameof(PixieCombo)}.{nameof(OnPointerPressed)}";
        base.OnPointerPressed(e);

        var info = e.GetCurrentPoint(this);

        if (info.Properties.IsLeftButtonPressed && !_comboBox.IsPointerOver && MaxEditLength <= 0)
        {
            ConditionalDebug.WriteLine(NSpace, "Left pressed");
            e.Handled = true;
            _comboBox.IsDropDownOpen = true;
            return;
        }
    }

    private static void UpdateDropPanelItem(Control? item)
    {
        if (item != null)
        {
            var marg = ChromeSizes.RegularPadding;
            item.Margin = new(marg.Left, marg.Top / 2.0, marg.Right * 2.0, marg.Bottom / 2.0);

            // Optional?
            // item.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;

            if (item is TemplatedControl t)
            {
                t.CornerRadius = Styling.SmallCornerRadius;
            }
        }
    }

    private void SetComboBoxBorder(bool focused)
    {
        // Fighting Fluent here
        _comboBox.BorderBrush = focused ? Styling.AccentBrush : Brushes.Transparent;
    }

    private HorizontalAlignment GetInternalAlignment(HorizontalAlignment dropAlign)
    {
        return IsEditable ? HorizontalAlignment.Left : dropAlign;
    }

    private StackPanel CreateItemsPanel()
    {
        _dropPanel = new StackPanel();
        _dropPanel.Orientation = Orientation.Vertical;
        _dropPanel.Children.CollectionChanged += InnerPanelChildrenChangedHandler;
        return _dropPanel;
    }

    private void ItemsChangedHandler(object? _, EventArgs __)
    {
        var items = Items;
        var comboItems = _comboBox.Items;

        while (comboItems.Count > items.Count)
        {
            comboItems.RemoveAt(comboItems.Count - 1);
        }

        for (int n = 0; n < items.Count; ++n)
        {
            if (n == comboItems.Count)
            {
                // New item
                comboItems.Add(items[n]);
                continue;
            }

            comboItems[n] = items[n];
        }
    }

    private void InnerTextInputHandler(object? _, TextInputEventArgs e)
    {
        // This only fires (synchronously) in response to user key press only (not setting).
        const string NSpace = $"{nameof(PixieCombo)}.{nameof(InnerTextInputHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Input: {e.Text}");

        int length = _text?.Length ?? 0;

        if (e.Text != null && length + e.Text.Length > MaxEditLength)
        {
            e.Handled = true;
        }
    }

    private void InnerTextChangedHandler(object? _, EventArgs __)
    {
        // Occurs asynchronously after text changes and the new text is rendered.
        const string NSpace = $"{nameof(PixieCombo)}.{nameof(InnerTextChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Text: {_comboBox.Text}");
        _text = _comboBox.Text;
    }

    private void InnerSelectionIndexChangedHandler(object? _, EventArgs __)
    {
        _selectedIndex = _comboBox.SelectedIndex;

        if (_selectedIndex > -1 && _selectedIndex < Items.Count)
        {
            _text = Items[_selectedIndex];
        }

        OnValueChanged();
    }

    private void InnerKeyDownHandler(object? _, KeyEventArgs e)
    {
        const string NSpace = $"{nameof(PixieCombo)}.{nameof(InnerKeyDownHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Key: {e.PhysicalKey}");

        if (e.PhysicalKey == PhysicalKey.Enter && IsEditable && !_comboBox.IsDropDownOpen)
        {
            e.Handled = true;
            RaiseEvent(new RoutedEventArgs(EditSubmittedEvent, this));
        }
    }

    private void InnerPanelChildrenChangedHandler(object? _, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is Control c)
                {
                    c.FocusAdorner = ChromeStyling.NewAdorner();
                    UpdateDropPanelItem(c);
                }
            }
        }
    }

    private void InnerTemplateAppliedHandler(object? _, TemplateAppliedEventArgs e)
    {
        // Fighting fluent
        // Locates the internal TextBox and disables its separate jankey FocusAdorner.
        _innerBox = e.NameScope.Find<TextBox>("PART_EditableTextBox");
        ConditionalDebug.ThrowIfNull(_innerBox);

        _innerBox?.SetValue(FocusAdornerProperty, null);
        _innerBox?.SetValue(BackgroundProperty, Brushes.Transparent);
        _innerBox?.SetValue(TextBox.MaxLinesProperty, 1);

        // Only need it once
        _comboBox.TemplateApplied -= InnerTemplateAppliedHandler;
    }

    private void InnerGotFocusHandler(object? _, EventArgs __)
    {
        SetComboBoxBorder(true);
        _comboBox.ClearValue(BackgroundProperty);
    }

    private void InnerLostFocusHandler(object? _, EventArgs __)
    {
        SetComboBoxBorder(false);
        _comboBox.Background = Brushes.Transparent;
    }

    private void InnerClosedHandler(object? _, EventArgs __)
    {
        SelectedIndex = _comboBox.SelectedIndex;
    }
}
