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
using Avalonia.Threading;
using Avalonia.LogicalTree;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Composite control with internal ComboBox.
/// </summary>
/// <remarks>
/// The <see cref="PixieControl.ValueChanged"/> event occurs when <see cref="SelectedIndex"/> changes, not the <see
/// cref="Text"/> value.
/// </remarks>
public class PixieCombo : PixieControl
{
    private const int DefaultSelectedIndex = -1;
    private const int DefaultMaxEditLength = 256;
    private const double DefaultMinContentWidth = 100;
    private const double DefaultMaxContentWidth = double.PositiveInfinity;
    private readonly SpecialBox _box = new();

    // Backing fields
    private int _selectedIndex = DefaultSelectedIndex;
    private string? _text;
    private string? _placeholderText;
    private bool _isEditable;
    private int _maxEditLength = DefaultMaxEditLength;
    private bool _isTranslateFriendly;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieCombo()
        : base(true, VerticalAlignment.Center)
    {
        SetSubject(_box);

        _box.Background = Brushes.Transparent;
        _box.MaxDropDownHeight = MaxDropHeight;
        _box.VerticalAlignment = VerticalContentAlignment;
        _box.MinWidth = DefaultMinContentWidth;
        _box.MaxWidth = DefaultMaxContentWidth;

        var align = SubjectAlignment; // <- follow init
        _box.HorizontalAlignment = align;
        _box.HorizontalContentAlignment = align;
        _box.Margin = new(0.0, ChromeSizes.SmallPx);
        _box.PlaceholderText = "None";

        _box.SelectionChanged += BoxSelectionIndexChangedHandler;
        _box.DropDownClosed += DropClosedHandler;

        var route = RoutingStrategies.Bubble;
        _box.AddHandler(KeyDownEvent, BoxKeyDownHandler, route);
        _box.AddHandler(TextInputEvent, BoxTextInputHandler, route);
        _box.AddHandler(TextBox.TextChangedEvent, BoxTextChangedHandler, route);

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
    public static readonly DirectProperty<PixieCombo, AvaloniaList<object>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, AvaloniaList<object>>(
            nameof(Items), o => o.Items, (o, v) => o.Items = v ?? new AvaloniaList<object>());

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
    /// Defines the <see cref="PlaceholderText"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, string?> PlaceholderTextProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, string?>(nameof(PlaceholderText),
        o => o.PlaceholderText, (o, v) => o.PlaceholderText = v);

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
    /// Defines the <see cref="IsTranslateFriendly"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCombo, bool> IsTranslateFriendlyProperty =
        AvaloniaProperty.RegisterDirect<PixieCombo, bool>(nameof(IsTranslateFriendly),
        o => o.IsTranslateFriendly, (o, v) => o.IsTranslateFriendly = v);

    /// <summary>
    /// Defines the <see cref="MaxDropHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxDropHeightProperty =
        AvaloniaProperty.Register<PixieCombo, double>(nameof(MaxDropHeight), 300.0);

    /// <summary>
    /// Defines the <see cref="MinSubjectWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinSubjectWidthProperty =
        AvaloniaProperty.Register<PixieCombo, double>(nameof(MinSubjectWidth), DefaultMinContentWidth);

    /// <summary>
    /// Defines the <see cref="MaxSubjectWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxSubjectWidthProperty =
        AvaloniaProperty.Register<PixieCombo, double>(nameof(MaxSubjectWidth), DefaultMaxContentWidth);

    /// <summary>
    /// Defines the <see cref="SubjectAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> SubjectAlignmentProperty =
        AvaloniaProperty.Register<PixieCombo, HorizontalAlignment>(nameof(SubjectAlignment),
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
    public AvaloniaList<object> Items { get; private set; } = new();

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
    /// cref="IsEditable"/> is true. It may also be set programmatically only where <see cref="IsEditable"/> is true.
    /// The <see cref="PixieControl.ValueChanged"/> does not occur when <see cref="Text"/> changes, but may occur if the
    /// change causes <see cref="SelectedIndex"/> to change. Setting does nothing where <see cref="IsEditable"/> is
    /// false.
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
    /// Gets or sets the placeholder text.
    /// </summary>
    public string? PlaceholderText
    {
        get { return _placeholderText; }
        set { SetAndRaise(PlaceholderTextProperty, ref _placeholderText, value); }
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
    /// Gets or sets whether to translate display content of <see cref="Items"/>.
    /// </summary>
    /// <remarks>
    /// Where <see cref="IsTranslateFriendly"/> is true, the result of <see cref="Textual.GetFriendlyNameOf(string,
    /// int)"/> is shown in place of item Tostring().
    /// </remarks>
    public bool IsTranslateFriendly
    {
        get { return _isTranslateFriendly; }
        set { SetAndRaise(IsTranslateFriendlyProperty, ref _isTranslateFriendly, value); }
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
    /// Gets or sets the minimum subject control width.
    /// </summary>
    public double MinSubjectWidth
    {
        get { return GetValue(MinSubjectWidthProperty); }
        set { SetValue(MinSubjectWidthProperty, value); }
    }

    /// <summary>
    /// Gets or sets the maximum subject control width.
    /// </summary>
    /// <remarks>
    /// When inputting text into an editable control, its width may grow. This can be used to clamp it.
    /// </remarks>
    public double MaxSubjectWidth
    {
        get { return GetValue(MaxSubjectWidthProperty); }
        set { SetValue(MaxSubjectWidthProperty, value); }
    }

    /// <summary>
    /// Gets or sets the subject control alignment.
    /// </summary>
    /// <remarks>
    /// The default is Right.
    /// </remarks>
    public HorizontalAlignment SubjectAlignment
    {
        get { return GetValue(SubjectAlignmentProperty); }
        set { SetValue(SubjectAlignmentProperty, value); }
    }

    /// <summary>
    /// Populates <see cref="Items"/> directly from values of enum type T.
    /// </summary>
    /// <remarks>
    /// Selected values may be excluded.
    /// </remarks>
    public void SetItemsAs<T>(params T[] exclusions) where T : struct, Enum
    {
        Items.Clear();

        foreach (var item in Enum.GetValues<T>())
        {
            if (!exclusions.Contains(item))
            {
                Items.Add(item);
            }
        }
    }

    /// <summary>
    /// Helper which casts <see cref="SelectedIndex"/> value to T.
    /// </summary>
    /// <exception cref="InvalidCastException">Invalid cast</exception>
    /// <exception cref="ArgumentOutOfRangeException">SelectedIndex</exception>
    public T GetSelectedIndexAs<T>()
    {
        return (T)Items[SelectedIndex];
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

        return false;
    }

    /// <summary>
    /// Given an element of <see cref="Items"/>, returns the instance to be displayed in the drop panel.
    /// </summary>
    /// <remarks>
    /// Where <see cref="IsTranslateFriendly"/> is true, the base method returns <see cref="Textual.GetFriendlyNameOf(string,
    /// int)"/> for the given "item". Otherwise, it simply returns "item" itself. May be overridden for translation.
    /// </remarks>
    protected virtual object GetDropItem(object item)
    {
        if (_isTranslateFriendly)
        {
            return item.ToString()?.GetFriendlyNameOf() ?? "";
        }

        return item;
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
            _box.SelectedIndex = change.GetNewValue<int>();
            return;
        }

        if (p == TextProperty)
        {
            // Already linked up to fire event when changes
            _box.Text = change.GetNewValue<string?>();
            return;
        }

        if (p == PlaceholderTextProperty)
        {
            _box.PlaceholderText = change.GetNewValue<string?>();
            return;
        }

        if (p == IsEditableProperty)
        {
            // See below as to why we combine with enabled
            _box.IsEditable = change.GetNewValue<bool>() && IsEffectivelyEnabled;
            _box.HorizontalContentAlignment = GetInternalAlignment(SubjectAlignment);
            return;
        }

        if (p == IsTranslateFriendlyProperty)
        {
            if (Items.Count != 0)
            {
                // Force rebuild
                var hold = Items.ToArray();
                Items.Clear();
                Items.AddRange(hold);
            }

            return;
        }


        if (p == MaxEditLengthProperty)
        {
            _box.MaxLength = _maxEditLength;
            return;
        }

        if (p == MinSubjectWidthProperty)
        {
            _box.MinWidth = change.GetNewValue<double>();
            return;
        }

        if (p == MaxSubjectWidthProperty)
        {
            _box.MaxWidth = change.GetNewValue<double>();
            return;
        }

        if (p == MaxDropHeightProperty)
        {
            _box.MaxDropDownHeight = change.GetNewValue<double>();
            return;
        }

        if (p == SubjectAlignmentProperty)
        {
            var value = change.GetNewValue<HorizontalAlignment>();
            _box.HorizontalAlignment = value;
            _box.HorizontalContentAlignment = GetInternalAlignment(value);
            return;
        }

        if (p == IsEffectivelyEnabledProperty)
        {
            // Fighting fluent
            // Editable ComboBox shows jankeyness which disabled.
            // Therefore we temporarily turn editable off when disabled.
            _box.IsEditable = IsEditable && change.GetNewValue<bool>();
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

        if (_isEditable)
        {
            return;
        }

        var info = e.GetCurrentPoint(this);
        ConditionalDebug.WriteLine(NSpace, "Left pressed A");
        ConditionalDebug.WriteLine(NSpace, "Pointer over Combo: " + _box.IsPointerOver);

        if (info.Properties.IsLeftButtonPressed && !_isEditable && !_box.IsPointerOver)
        {
            ConditionalDebug.WriteLine(NSpace, "Left pressed B");
            e.Handled = true;
            _box.IsDropDownOpen = true;
            return;
        }
    }

    private HorizontalAlignment GetInternalAlignment(HorizontalAlignment dropAlign)
    {
        return IsEditable ? HorizontalAlignment.Left : dropAlign;
    }

    private void ItemsChangedHandler(object? _, EventArgs __)
    {
        var src = Items;
        var dest = _box.Items;

        while (dest.Count > src.Count)
        {
            dest.RemoveAt(dest.Count - 1);
        }

        for (int n = 0; n < src.Count; ++n)
        {
            if (n == dest.Count)
            {
                // New item
                dest.Add(GetDropItem(src[n]));
                continue;
            }

            dest[n] = GetDropItem(src[n]);
        }
    }

    private void BoxTextChangedHandler(object? _, EventArgs __)
    {
        // Occurs asynchronously after text changes and the new text is rendered.
        const string NSpace = $"{nameof(PixieCombo)}.{nameof(BoxTextChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Text: {_box.Text}");
        _text = _box.Text;
    }

    private void BoxTextInputHandler(object? _, TextInputEventArgs e)
    {
        // This only fires (synchronously) in response to user key press only (not setting).
        int length = _text?.Length ?? 0;

        if (e.Text != null && length + e.Text.Length > MaxEditLength)
        {
            e.Handled = true;
        }
    }

    private void BoxSelectionIndexChangedHandler(object? _, EventArgs __)
    {
        _selectedIndex = _box.SelectedIndex;

        if (_selectedIndex > -1 && _selectedIndex < Items.Count)
        {
            _text = Items[_selectedIndex]?.ToString();
        }

        OnValueChanged();
    }

    private void BoxKeyDownHandler(object? _, KeyEventArgs e)
    {
        const string NSpace = $"{nameof(PixieCombo)}.{nameof(BoxKeyDownHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Key: {e.Key}");

        if (e.Key == Key.Enter && IsEditable && !_box.IsDropDownOpen)
        {
            e.Handled = true;
            RaiseEvent(new RoutedEventArgs(EditSubmittedEvent, this));
            return;
        }
    }

    private void DropClosedHandler(object? _, EventArgs __)
    {
        SelectedIndex = _box.SelectedIndex;
    }

    /// <summary>
    /// Attempts to curtail Fluent Combo jankiness
    /// </summary>
    private class SpecialBox : ComboBox
    {
        private TextBox? _edit;
        private StackPanel? _dropPanel;
        private int _maxLength = DefaultMaxEditLength;

        public SpecialBox()
        {
            Classes.Add("corner-small");
            SetSpecialBorder(false);
            ItemsPanel = new FuncTemplate<Panel?>(CreateItemsPanel);

            DropDownOpened += DropOpenedHandler;
        }

        public int MaxLength
        {
            get { return _maxLength; }

            set
            {
                if (_maxLength != value)
                {
                    _maxLength = value;
                    _edit?.MaxLength = value;
                }
            }
        }

        protected override Type StyleKeyOverride { get; } = typeof(ComboBox);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            var p = change.Property;
            base.OnPropertyChanged(change);

            if (p == IsEditableProperty)
            {
                SetSpecialBorder(IsFocused);
                return;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var info = e.GetCurrentPoint(this);

            if (info.Properties.IsLeftButtonPressed && _edit?.IsPointerOver == false)
            {
                e.Handled = true;
                IsDropDownOpen = true;
                _dropPanel?.Focus();
                return;
            }

            base.OnPointerPressed(e);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // Fighting fluent
            // Locates the internal TextBox and disables its separate jankey FocusAdorner.
            _edit = e.NameScope.Find<TextBox>("PART_EditableTextBox");
            ConditionalDebug.ThrowIfNull(_edit);

            if (_edit != null)
            {
                _edit.FocusAdorner = null;
                _edit.Background = Brushes.Transparent;
                _edit.MaxLines = 1;
                _edit.MaxLength = _maxLength;
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            SetSpecialBorder(true);
            ClearValue(BackgroundProperty);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            SetSpecialBorder(false);
            Background = Brushes.Transparent;
        }

        private static void UpdateDropPanelItem(Control? item)
        {
            if (item != null)
            {
                var marg = ChromeSizes.StandardPadding;
                item.Margin = new(marg.Left, marg.Top / 2.0, marg.Right * 2.0, marg.Bottom / 2.0);

                if (item is TemplatedControl t)
                {
                    t.CornerRadius = Styling.SmallCornerRadius;
                }
            }
        }

        private StackPanel CreateItemsPanel()
        {
            _dropPanel = new StackPanel();
            _dropPanel.Orientation = Orientation.Vertical;
            _dropPanel.Children.CollectionChanged += DropChildrenChangedHandler;
            return _dropPanel;
        }

        private void SetSpecialBorder(bool focused)
        {
            // Fighting Fluent here
            if (focused)
            {
                BorderBrush = Styling.AccentBrush;
                return;
            }

            if (IsEditable)
            {
                ClearValue(BorderBrushProperty);
                return;
            }

            BorderBrush = Brushes.Transparent;
        }

        private void DropChildrenChangedHandler(object? _, NotifyCollectionChangedEventArgs e)
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

        private void DropOpenedHandler(object? _, EventArgs __)
        {
            _dropPanel?.FindLogicalAncestorOfType<Border>()?.Background = ChromeStyling.Global.TintBackground;

            if (Items.Count == 0)
            {
                Dispatcher.UIThread.Post(() => IsDropDownOpen = false, DispatcherPriority.ContextIdle);
            }
        }
    }

}
