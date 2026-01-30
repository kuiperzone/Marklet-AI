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
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Windows.Internal;

internal sealed class ButtonGrid : Grid
{
    public const int LeftColumn = 0;
    public const int StarColumn = 1;
    public const int RightColumn = 2;
    public const int MinimizeColumn = 3;
    public const int MaximizeColumn = 4;
    public const int CloseColumn = 5;
    public const int ColumnCount = 6;

    private const int ButtonSpacing = 7;
    private const double SymbolFontSize = ChromeFonts.SymbolFontSize;
    private const double LargeFontSize = ChromeFonts.LargeSymbolFontSize;
    private const double LargeSizeF = LargeFontSize / SymbolFontSize;

    private static readonly ChromeStyling Styling = ChromeStyling.Global;
    private readonly ChromeWindow _window;
    private readonly LightBar _leftGroup;
    private readonly LightBar _rightGroup;

    public ButtonGrid(ChromeBar owner)
    {
        _window = owner.Owner;
        var settings = _window.Settings;

        var cols = ColumnDefinitions;

        for (int n = 0; n < ColumnCount; ++n)
        {
            cols.Add(new(n != StarColumn ? GridLength.Auto : GridLength.Star));
        }

        _leftGroup = new();
        SetColumn(_leftGroup, LeftColumn);
        Children.Add(_leftGroup);
        LeftGroup = _leftGroup;

        _rightGroup = new();
        SetColumn(_rightGroup, RightColumn);
        Children.Add(_rightGroup);
        RightGroup = _rightGroup;

        ControlStyle = settings.ControlStyle;
        ControlBackground = settings.ControlBackground;

        var margin = new Thickness(ButtonSpacing, 0.0, 0.0, 0.0);
        MinimizeButton.ContentPadding = default; // <- important
        MinimizeButton.Focusable = false;
        MinimizeButton.Margin = margin;
        MinimizeButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        SetColumn(MinimizeButton, MinimizeColumn);
        Children.Add(MinimizeButton);


        MaximizeButton.ContentPadding = default;
        MaximizeButton.Focusable = false;
        MaximizeButton.Margin = margin;
        MaximizeButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        SetColumn(MaximizeButton, MaximizeColumn);
        Children.Add(MaximizeButton);

        CloseButton.ContentPadding = default;
        CloseButton.Focusable = false;
        CloseButton.Margin = margin;
        CloseButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        SetColumn(CloseButton, CloseColumn);
        Children.Add(CloseButton);

        _window.PropertyChanged += WindowPropertyChangedHandler;
    }

    public LightBar LeftGroup { get; }
    public LightBar RightGroup { get; }
    public LightButton MinimizeButton = new();
    public LightButton MaximizeButton = new();
    public LightButton CloseButton = new();

    public double BarHeight { get; private set; }
    public bool IsCompact { get; private set; }
    public ChromeControlStyle ControlStyle { get; private set; }
    public ChromeControlBackground ControlBackground { get; private set; }

    public void RefreshStyling()
    {
        UpdateControlButtons();
    }

    public void UpdateWindowSettings(bool isDialog)
    {
        var settings = _window.Settings;
        IsCompact = settings.GetCompact(isDialog);
        ControlStyle = settings.GetControlStyle(isDialog);
        ControlBackground = settings.GetControlBackground(isDialog);

        _leftGroup.FontScale = IsCompact ? 1.0 : LargeSizeF;
        _rightGroup.FontScale = IsCompact ? 1.0 : LargeSizeF;
        BarHeight = _leftGroup.ButtonHeight;

        UpdateControlButtons();
    }

    private void UpdateControlButtons()
    {
        // The design intention is to emulate standard desktop appearances while
        // offering customization. The logic is therefore somewhat complex and nuanced.

        // Collate all state so we can keep track
        var state = _window.WindowState;
        var canMinimize = _window.CanMinimize;
        var canMaximize = _window.CanMaximize;
        bool defVisible = _window.IsChromeWindow;

        // MINIMIZE
        UpdateControlButton(MinimizeButton, false);

        switch (ControlStyle)
        {
            case ChromeControlStyle.CloseOnly:
            case ChromeControlStyle.LargeCloseOnly:
                MinimizeButton.IsVisible = false;
                break;
            case ChromeControlStyle.DiamondArrows:
                MinimizeButton.IsVisible = canMinimize && defVisible;
                MinimizeButton.Content = Symbols.KeyboardArrowDown;
                break;
            case ChromeControlStyle.DiagonalArrows:
                MinimizeButton.IsVisible = canMinimize && defVisible;
                MinimizeButton.Content = Symbols.Minimize;
                break;
            default:
                MinimizeButton.IsVisible = canMinimize && defVisible;
                MinimizeButton.Content = Symbols.Minimize;
                break;
        }

        // MAXIMIZE
        UpdateControlButton(MaximizeButton, false);

        switch (ControlStyle)
        {
            case ChromeControlStyle.CloseOnly:
            case ChromeControlStyle.LargeCloseOnly:
                MaximizeButton.IsVisible = false;
                break;
            case ChromeControlStyle.DiamondArrows:
                MaximizeButton.IsVisible = canMaximize && defVisible;
                MaximizeButton.Content = state == WindowState.Maximized ? Symbols.Stat0 : Symbols.KeyboardArrowUp;
                break;
            case ChromeControlStyle.DiagonalArrows:
                MaximizeButton.IsVisible = canMaximize && defVisible;
                MaximizeButton.Content = state == WindowState.Maximized ? Symbols.CheckBoxOutlineBlank : Symbols.ExpandContent;
                break;
            default:
                MaximizeButton.IsVisible = canMaximize && defVisible;
                MaximizeButton.Content = state == WindowState.Maximized ? Symbols.SelectWindow2 : Symbols.CheckBoxOutlineBlank;
                break;
        }

        // CLOSE
        UpdateControlButton(CloseButton, ControlBackground.IsCloseAccented());
        CloseButton.Content = Symbols.Close; // <- always same here
        CloseButton.IsVisible = defVisible;
    }

    private void UpdateControlButton(LightButton button, bool accented)
    {
        double fs = ControlStyle.IsLarger() ? ChromeFonts.LargeSymbolFontSize : ChromeFonts.SymbolFontSize;

        var max = fs * (IsCompact ? 1.4 : 1.6);
        button.FontSize = fs;
        button.MinWidth = max;
        button.MinHeight = max;
        button.MaxWidth = max;
        button.MaxHeight = max;
        button.CornerRadius = ControlBackground.IsCircle() ? new(max / 2.0) : Styling.SmallCornerRadius;

        UpdateControlColor(button, accented);
    }

    private void UpdateControlColor(LightButton button, bool accented)
    {
        if (ControlBackground.HasBackground())
        {
            if (accented)
            {
                button.Foreground = ChromeBrushes.White;
                button.Background = Styling.AccentBrush;
                return;
            }

            button.Background = ChromeStyling.ButtonBrush;
            button.ClearValue(LightButton.ForegroundProperty);
            return;
        }

        button.ClearValue(LightButton.ForegroundProperty);
        button.ClearValue(BackgroundProperty);
    }

    private void WindowPropertyChangedHandler(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        var p = e.Property;

        if (p == Window.WindowStateProperty ||
            p == Window.CanMinimizeProperty ||
            p == Window.CanMaximizeProperty)
        {
            UpdateControlButtons();
            return;
        }
    }
}
