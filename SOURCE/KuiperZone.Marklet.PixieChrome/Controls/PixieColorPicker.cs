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

using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Metadata;
using Avalonia.Threading;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A subclass of <see cref="PixieControl"/> displaying a simple color picker.
/// </summary>
/// <remarks>
/// The <see cref="PixieControl.Title"/> for this control is not shown.
/// </remarks>
public class PixieColorPicker : PixieControl
{
    /// <summary>
    /// Maximum number of colors per <see cref="Palette"/> instance.
    /// </summary>
    public const int MaxPaletteColors = 9;

    private const string DefaultHintString = "Default";
    private const int PaletteRowCount = 3;
    private const int DefaultColorColumn = 5;
    private const int EditorRow = 4;
    private const int EditorColSpan = DefaultColorColumn;
    private readonly Grid _grid = new();
    private readonly List<Circle> _circles = new();
    private readonly Circle _defaultCircle;
    private readonly TextBlock _defaultHintBlock = new();
    private readonly TextEditor _editor = new();
    private readonly Ellipse _editorShape = new();
    private readonly DispatchCoalescer _paletteUpdater = new(DispatcherPriority.Render);
    private bool _isValueChanging;

    private Color _chosenColor;
    private bool _isDefaultColorVisible;
    private string? _defaultColorLabel = DefaultHintString;
    private bool _isEditorVisible = true;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieColorPicker()
    {
        IsTitleVisible = false;
        SetChildControl(_grid);

        _grid.VerticalAlignment = VerticalContentAlignment;
        _grid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _defaultCircle = new(this, default, default);
        _defaultCircle.SetGridPos(EditorRow, DefaultColorColumn);
        VerticalContentOffset = _defaultCircle.TotalSize.Height / 3.0;

        _defaultHintBlock.Text = _defaultColorLabel;
        _defaultHintBlock.Margin = new(6.0, 0.0, 0.0, 0.0);
        _defaultHintBlock.Foreground = ChromeStyling.ForegroundGray;
        _defaultHintBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        _defaultHintBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _defaultHintBlock.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _defaultHintBlock.PointerPressed += DefaultHintPointerPressedHandler;

        Grid.SetRow(_defaultHintBlock, EditorRow);
        Grid.SetColumn(_defaultHintBlock, DefaultColorColumn + 1);
        Grid.SetColumnSpan(_defaultHintBlock, int.MaxValue);
        _grid.Children.Add(_defaultHintBlock);

        // IMPORTANT. We have many grid cells and do not use GridLength.Auto for
        // performance reasons. Instead, as all cells as same size, we set programmatically.
        GridLength len = default;

        for (int n = 0; n < PaletteRowCount; ++n)
        {
            _grid.RowDefinitions.Add(new(len));
        }

        // Editor row
        _grid.RowDefinitions.Add(new(GridLength.Auto));

        // Columns
        var circleSize = _defaultCircle.TotalSize;
        len = new(circleSize.Width, GridUnitType.Pixel);

        for (int n = 0; n < MaxPaletteColors; ++n)
        {
            _grid.ColumnDefinitions.Add(new(len));
        }

        var fs = ChromeFonts.DefaultFontSize;
        _editorShape.Width = fs;
        _editorShape.Height = fs;
        _editorShape.StrokeThickness = 1.0;
        _editorShape.Margin = new(ChromeSizes.OneCh, 0, 0, 0);

        _editor.MaxLength = 9;
        _editor.Watermark = "#RRGGBB";
        _editor.InnerLeftContent = _editorShape;
        _editor.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _editor.TextChanging += EditorChangingHandler;

        fs = ChromeFonts.HugeFontSize;
        double h = fs / 4.5;
        double v = fs / 2.5;
        _editor.Margin = new(0, v, h, v);
        _editor.MinWidth = ChromeSizes.OneCh * 21;
        _editor.MaxWidth = Grid.GetColumnSpan(_editor) * circleSize.Width - h;

        SetEditorShape(null);

        Grid.SetRow(_editor, EditorRow);
        Grid.SetColumn(_editor, 0);
        Grid.SetColumnSpan(_editor, EditorColSpan);
        _grid.Children.Add(_editor);

        _paletteUpdater.Posted += PaletteUpdatePostedHandler;
        Palette.CollectionChanged += PaletteChangedHandler;
        SecondaryPalette.CollectionChanged += PaletteChangedHandler;
        TertiaryPalette.CollectionChanged += PaletteChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="ChosenColor"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieColorPicker, Color> ChosenColorProperty =
        AvaloniaProperty.RegisterDirect<PixieColorPicker, Color>(nameof(ChosenColor),
        o => o.ChosenColor, (o, v) => o.ChosenColor = v);

    /// <summary>
    /// Defines the <see cref="IsDefaultColorVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieColorPicker, bool> IsDefaultColorVisibleProperty =
        AvaloniaProperty.RegisterDirect<PixieColorPicker, bool>(nameof(IsDefaultColorVisible),
        o => o.IsDefaultColorVisible, (o, v) => o.IsDefaultColorVisible = v);

    /// <summary>
    /// Defines the <see cref="DefaultColorLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieColorPicker, string?> DefaultHintProperty =
        AvaloniaProperty.RegisterDirect<PixieColorPicker, string?>(nameof(DefaultColorLabel),
        o => o.DefaultColorLabel, (o, v) => o.DefaultColorLabel = v, DefaultHintString);

    /// <summary>
    /// Gets or sets the chosen color.
    /// </summary>
    /// <remarks>
    /// The initial value is the default color. The <see cref="PixieControl.ValueChanged"/> event is raised when this
    /// value changes.
    /// </remarks>
    public Color ChosenColor
    {
        get { return _chosenColor; }
        set { SetAndRaise(ChosenColorProperty, ref _chosenColor, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a "default color" option.
    /// </summary>
    /// <remarks>
    /// The default value is false.
    /// </remarks>
    public bool IsDefaultColorVisible
    {
        get { return _isDefaultColorVisible; }
        set { SetAndRaise(IsDefaultColorVisibleProperty, ref _isDefaultColorVisible, value); }
    }

    /// <summary>
    /// Gets or sets the text next to the "default color" option.
    /// </summary>
    /// <remarks>
    /// The default value is "Default".
    /// </remarks>
    public string? DefaultColorLabel
    {
        get { return _defaultColorLabel; }
        set { SetAndRaise(DefaultHintProperty, ref _defaultColorLabel, value); }
    }

    /// <summary>
    /// Gets a predefined color palette.
    /// </summary>
    /// <remarks>
    /// The maximum number of entries is limited to <see cref="MaxPaletteColors"/>, with items beyond this limit ignored.
    /// The initial value is empty.
    /// </remarks>
    [Content]
    public AvaloniaList<Color> Palette { get; } = new();

    /// <summary>
    /// Gets a secondary color palette which appears below <see cref="Palette"/>.
    /// </summary>
    public AvaloniaList<Color> SecondaryPalette { get; } = new();

    /// <summary>
    /// Gets a tertiary color palette which appears below <see cref="SecondaryPalette"/>.
    /// </summary>
    public AvaloniaList<Color> TertiaryPalette { get; } = new();

    /// <summary>
    /// Gets whether <see cref="ChosenColor"/> is the default color.
    /// </summary>
    public bool IsDefaultColor
    {
        get { return ChosenColor == default; }
    }

    /// <summary>
    /// Gets or sets whether the user can edit custom colors.
    /// </summary>
    protected bool IsEditorVisibleInternal
    {
        get { return _isEditorVisible; }

        set
        {
            if (_isEditorVisible != value)
            {
                _isEditorVisible = value;
                _editor.IsVisible = value;

                if (value)
                {
                    _defaultCircle.SetGridPos(EditorRow, DefaultColorColumn);
                    Grid.SetColumn(_defaultHintBlock, DefaultColorColumn + 1);
                }
                else
                {
                    _defaultCircle.SetGridPos(EditorRow, 0);
                    Grid.SetColumn(_defaultHintBlock, 1);
                }
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _defaultHintBlock.Cursor ??= ChromeCursors.HandCursor;

        if (IsDefaultColor)
        {
            SetEditorShape(null);
        }

        _defaultCircle.Refresh(IsDefaultColor);
        Styling.StylingChanged += StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChosenColorProperty)
        {
            if (!_isValueChanging)
            {
                SetAndRaiseChosen(change.GetNewValue<Color>(), true);
            }

            return;
        }

        if (p == IsDefaultColorVisibleProperty)
        {
            RefreshPalettes();
            return;
        }

        if (p == DefaultHintProperty)
        {
            _defaultHintBlock.Text = change.GetNewValue<string?>();
            return;
        }

        if (p == IsEffectivelyEnabledProperty)
        {
            _paletteUpdater.Post();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _editor.Focus();
    }

    private static string ToHexString(Color color)
    {
        uint rgb = color.ToUInt32() & 0x00FFFFFF;
        return "#" + rgb.ToString("X6", CultureInfo.InvariantCulture);
    }

    private void SetAndRaiseChosen(Color color, bool setEditor)
    {
        try
        {
            _isValueChanging = true;
            ChosenColor = color;

            bool def = color == default;
            _defaultCircle.Choose(def);
            _defaultHintBlock.Foreground = ChromeStyling.ForegroundGray;

            foreach (var item in _circles)
            {
                item.Choose(!def && item.Color == color);
            }

            SetEditorShape(def ? null : new ImmutableSolidColorBrush(color));

            if (setEditor)
            {
                if (def)
                {
                    _editor.Clear();
                }
                else
                {
                    _editor.Text = ToHexString(color);
                    _editor.SelectionStart = _editor.Text.Length;
                    _editor.SelectionEnd = _editor.Text.Length;
                }
            }
        }
        finally
        {
            _isValueChanging = false;
            OnValueChanged();
        }
    }

    private void SetEditorShape(IBrush? background)
    {
        _editorShape.Fill = background;
        _editorShape.Stroke = background == null ? Styling.Foreground : Brushes.Transparent;
    }

    private void RefreshPalettes()
    {
        _defaultCircle.IsVisible = IsDefaultColorVisible;
        _defaultHintBlock.IsVisible = IsDefaultColorVisible;

        bool empty = IsDefaultColor;
        _defaultCircle.Refresh(empty);

        var chosen = ChosenColor;

        for (int n = 0; n < _circles.Count; ++n)
        {
            var item = _circles[n];
            int col = n % MaxPaletteColors;
            int row = n / MaxPaletteColors;
            ConditionalDebug.ThrowIfGreaterThanOrEqual(row, PaletteRowCount);

            item.SetGridPos(row, col);
            item.Refresh(!empty && item.Color == chosen);
        }
    }

    private void RebuildPalette(AvaloniaList<Color> palette)
    {
        int max = Math.Min(palette.Count, MaxPaletteColors);
        Color disabled = IsEffectivelyEnabled ? default : ChromeStyling.ForegroundGray.Color;

        for (int n = 0; n < max; ++n)
        {
            var item = new Circle(this, palette[n], disabled);
            _circles.Add(item);
        }
    }

    private void PaletteUpdatePostedHandler(object? _, EventArgs __)
    {
        // Remove
        foreach (var item in _circles)
        {
            item.Remove();
        }

        _circles.Clear();
        RebuildPalette(Palette);
        RebuildPalette(SecondaryPalette);
        RebuildPalette(TertiaryPalette);

        var circleSize = _defaultCircle.TotalSize;
        _grid.RowDefinitions[0].Height = Palette.Count > 0 ? new GridLength(circleSize.Height, GridUnitType.Pixel) : default;
        _grid.RowDefinitions[1].Height = SecondaryPalette.Count > 0 ? new GridLength(circleSize.Height, GridUnitType.Pixel) : default;
        _grid.RowDefinitions[2].Height = TertiaryPalette.Count > 0 ? new GridLength(circleSize.Height, GridUnitType.Pixel) : default;

        var len = new GridLength(circleSize.Width, GridUnitType.Pixel);

        for (int n = 0; n < MaxPaletteColors; ++n)
        {
            _grid.ColumnDefinitions[n].Width = len;
        }

        RefreshPalettes();
    }

    private void PaletteChangedHandler(object? _, NotifyCollectionChangedEventArgs e)
    {
        _paletteUpdater.Post();
    }

    private void EditorChangingHandler(object? _, EventArgs __)
    {
        if (!_isValueChanging)
        {
            var text = _editor.Text?.Trim().TrimStart('#');

            if (Color.TryParse("#" + text, out Color color) || Color.TryParse(text, out color))
            {
                // Accept alpha but strip it out
                color = Color.FromRgb(color.R, color.G, color.B);
                SetAndRaiseChosen(color, false);
                return;
            }

            SetAndRaiseChosen(default, false);
        }
    }

    private void DefaultHintPointerPressedHandler(object? _, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            SetAndRaiseChosen(default, true);
        }
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        if (IsDefaultColor)
        {
            SetEditorShape(null);
        }

        _defaultCircle.Refresh(IsDefaultColor);
    }

    /// <summary>
    /// Internal class to render concentric circles.
    /// </summary>
    private sealed class Circle
    {
        private readonly Ellipse _outer = new();
        private readonly Ellipse _inner = new();
        private readonly PixieColorPicker _owner;

        public Circle(PixieColorPicker owner, Color color, Color disabled)
        {
            _owner = owner;
            Color = color.Blend(0.35, disabled);
            IsDefault = color == default;

            _inner.Cursor = ChromeCursors.HandCursor;
            _inner.Fill = new ImmutableSolidColorBrush(Color);
            _inner.PointerPressed += PointerPressedHandler;

            _outer.Cursor = ChromeCursors.HandCursor;
            _outer.PointerPressed += PointerPressedHandler;

            const double size0 = ChromeFonts.DefaultFontSize * 1.55;
            const double ring1 = 3.0;
            const double size1 = size0 + ring1 * 4;

            const double v0 = size0 / 2.5;
            const double v1 = v0 + ring1 * 2;
            const double h1 = ring1 * 2;

            const double px4 = 4.0;
            _inner.Width = size0;
            _inner.Height = size0;
            _inner.Margin = new(h1, v1, h1 + px4, v1);
            _inner.StrokeThickness = 1.0;

            _outer.Width = size1;
            _outer.Height = size1;
            _outer.StrokeThickness = ring1;

            _outer.Margin = new(0.0, v0, px4, v0);

            // Total size inc. margin
            TotalSize = new(size1 + px4, size1 + v0 * 2.0);
        }

        public Color Color { get; }
        public bool IsDefault { get; }
        public bool IsGridded { get; private set; }
        public Size TotalSize { get; }

        public bool IsVisible
        {
            get { return _inner.IsVisible; }

            set
            {
                _inner.IsVisible = value;
                _outer.IsVisible = value;
            }
        }

        public void SetGridPos(int row, int col)
        {
            Grid.SetRow(_inner, row);
            Grid.SetRow(_outer, row);
            Grid.SetColumn(_inner, col);
            Grid.SetColumn(_outer, col);

            if (!IsGridded)
            {
                IsGridded = true;
                var gc = _owner._grid.Children;
                gc.Add(_outer);
                gc.Add(_inner);
            }
        }

        public void Choose(bool chosen)
        {
            if (IsDefault)
            {
                _inner.Stroke = Styling.Foreground;
                _outer.Stroke = chosen ? Styling.Foreground : null;
                return;
            }

            _outer.Stroke = chosen ? _inner.Fill : null;
        }

        public void Refresh(bool chosen)
        {
            if (IsDefault && _outer.Stroke != null)
            {
                _inner.Stroke = Styling.Foreground;
                _outer.Stroke = Styling.Foreground;
            }

            Choose(chosen);
        }

        public void Remove()
        {
            if (IsGridded)
            {
                IsGridded = false;
                var gc = _owner._grid.Children;
                gc.Remove(_outer);
                gc.Remove(_inner);
            }
        }

        private void PointerPressedHandler(object? sender, PointerPressedEventArgs e)
        {
            var info = e.GetCurrentPoint(sender as Visual ?? _inner);

            if (info.Properties.IsLeftButtonPressed)
            {
                _owner.SetAndRaiseChosen(Color, true);
            }
        }

        public static explicit operator Control(Circle v)
        {
            throw new NotImplementedException();
        }
    }

}
