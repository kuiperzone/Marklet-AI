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

using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace KuiperZone.Marklet.PixieChrome.Internal;

/// <summary>
/// Populates Fluent <see cref="ColorPaletteResources"/> according to our own themes.
/// </summary>
internal sealed class ThemePalette : ColorPaletteResources
{
    private static readonly ThemePalette FluentDark;
    private static readonly ThemePalette RegularDark;
    private static readonly ThemePalette RegularLight;

    static ThemePalette()
    {
        FluentDark = new();
        FluentDark.PopulateFluentDark();

        RegularDark = new();
        RegularDark.PopulateRegularDark();

        RegularLight = new();
        RegularLight.PopulateRegularLight();
    }

    private ThemePalette()
    {
    }

    /// <summary>
    /// Gets the primary foreground used by the current theme.
    /// </summary>
    public ImmutableSolidColorBrush Foreground { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets the primary background used by the current theme (RegionColor).
    /// </summary>
    public ImmutableSolidColorBrush Background { get; private set; } = ChromeBrushes.White;

    /// <summary>
    /// Gets the "low" (darker) background used by the current theme (AltHigh).
    /// </summary>
    public ImmutableSolidColorBrush BackgroundLow { get; private set; } = ChromeBrushes.White;

    /// <summary>
    /// Gets the "high" (lighter) background used by the current theme.
    /// </summary>
    public ImmutableSolidColorBrush BackgroundHigh { get; private set; } = ChromeBrushes.White;

    /// <summary>
    /// Gets a brush for the left-hand buffer bar.
    /// </summary>
    public ImmutableSolidColorBrush BufferBarBrush { get; private set; } = ChromeBrushes.White;

    /// <summary>
    /// Gets a brush for borders, dividers and window border.
    /// </summary>
    /// <remarks>
    /// If color is to similar to background, use <see cref="WindowBorder"/> instead.
    /// </remarks>
    public ImmutableSolidColorBrush BorderBrush { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets a border brush for Flyout and ContextMenus pop-outs.
    /// </summary>
    public ImmutableSolidColorBrush WindowBorder { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets the group background brush.
    /// </summary>
    /// <remarks>
    /// Currently light/dark independent.
    /// </remarks>
    public ImmutableSolidColorBrush GroupBorder { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets the background for a focused TextBox.
    /// </summary>
    public ImmutableSolidColorBrush FocusedBox { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets or set the accent color on all palettes.
    /// </summary>
    public static void SetAccents(Color value)
    {
        RegularDark.Accent = value;
        FluentDark.Accent = value;
        RegularLight.Accent = value;
    }

    /// <summary>
    /// Gets palette variant.
    /// </summary>
    public static ThemePalette Get(ThemeVariant variant, bool preferFluent)
    {
        if (variant == ThemeVariant.Dark)
        {
            return preferFluent ? FluentDark : RegularDark;
        }

        return RegularLight;
    }

    private void PopulateFluentDark()
    {
        Background = new(0xFF09090A);
        Foreground = ChromeBrushes.White;
        BackgroundLow = ChromeBrushes.Black;
        BackgroundHigh = ChromeBrushes.VeryDarkGray;
        BorderBrush = ChromeBrushes.DarkGray;
        WindowBorder = ChromeBrushes.DarkGray;
        BufferBarBrush = ChromeBrushes.Black;
        GroupBorder = ChromeBrushes.Transparent;

        // PRIMARY FOREGROUND
        // BaseHigh ="White"
        BaseHigh = Foreground.Color;

        // PRIMARY BACKGROUND
        // RegionColor ="Black"
        RegionColor = Background.Color;

        // Same as expected AltHigh
        FocusedBox = ChromeBrushes.Black;
    }

    private void PopulateRegularDark()
    {
        Background = new(0xFF222226);
        Foreground = new(0xFFE0E0E0);
        BackgroundLow = ChromeBrushes.VeryDarkGray;
        BackgroundHigh = ChromeBrushes.DarkGray;
        BorderBrush = ChromeBrushes.DarkGray;
        WindowBorder = new(0xFF555557);
        BufferBarBrush = ChromeBrushes.Black;
        GroupBorder = ChromeBrushes.Transparent;

        // Same as expected AltHigh
        FocusedBox = ChromeBrushes.Black;

        // SET ONLY THOSE COLORS WE NEED (commented out for reference)
        // Even setting some to same as theme can break some controls
        // Accent="#ff0073cf"
        // Accent = ChromeBrushes.DefaultAccent.Color;

        // PRIMARY FOREGROUND
        // BaseHigh ="White"
        BaseHigh = Foreground.Color;

        // PRIMARY BACKGROUND
        // RegionColor ="Black"
        RegionColor = Background.Color;


        // Focused textbox background
        // AltHigh ="Black"
        // AltHigh = Colors.Black;

        // AltLow ="Black"
        // AltLow = Colors.Black;

        // AltMediumHigh ="Black"
        // AltMediumHigh = Colors.Black;

        // Hover TextBox background
        // AltMedium ="Black"
        // AltMedium = Colors.Black;

        // Unfocused TextBox background
        // AltMediumLow ="Black"
        AltMediumLow = Background.Color;


        // BaseMediumHigh="#ffb4b4b4"
        // BaseMediumHigh = Color.FromUInt32(0xffb4b4b4);

        // BaseMedium="#ff9a9a9a"
        // BaseMedium = Color.FromUInt32(0xff9a9a9a);

        // BaseMediumLow ="#ff676767"
        // BaseMediumLow = Color.FromUInt32(0xff676767);

        // CONFLICT: Thumb for scrollbar
        // Also: Disabled TextBox background
        // DO NOT SET (causes disabled toggle switches and scrollbar to be obscured)
        // BaseLow="#ff333333"
        // BaseLow = Color.FromUInt32(0xff333333);


        // ChromeAltLow ="#ffb4b4b4"
        // ChromeAltLow = Color.FromUInt32(0xffb4b4b4);

        // ChromeBlackHigh ="Black"
        // ChromeBlackHigh = Colors.Black;

        // ChromeBlackLow = "#ffb4b4b4"
        // ChromeBlackLow = Color.FromUInt32(0xffb4b4b4);

        // ChromeBlackMedium ="Black"
        // ChromeBlackMedium = Colors.Black;

        // ChromeBlackMediumLow ="Black"
        // ChromeBlackMediumLow = Colors.Black;

        // Disabled Slider foreground
        // ChromeDisabledHigh ="#ff333333"
        ChromeDisabledHigh = ChromeStyling.ForegroundGray.Color;

        // Disabled TextBox foreground
        // ChromeDisabledLow ="#ff9a9a9a"
        ChromeDisabledLow = Color.FromUInt32(0xFFB3B3B5);


        // ChromeGray = "Gray"
        // ChromeGray = Color.FromUInt32(0xFF808080);

        // ChromeHigh ="Gray"
        // ChromeHigh = Color.FromUInt32(0xFF808080);

        // ChromeLow ="#ff151515"
        // ChromeLow = Color.FromUInt32(0xff151515);

        // ChromeMedium ="#ff1d1d1d"
        // ChromeMedium = Color.FromUInt32(0xff1d1d1d);

        // ContextMenu background
        // ChromeMediumLow ="#ff2c2c2c"
        ChromeMediumLow = Color.FromUInt32(0xff36363A);

        // ChromeWhite = "White"
        // ChromeWhite = Colors.White;


        // ContextMenu hover background
        // ListLow = "#ff1d1d1d"
        // ListLow = Color.FromUInt32(0xff1d1d1d);

        // ComboBox pressed background
        // ContextMenu pressed background
        // ListMedium ="#ff333333"
        // ListMedium = Color.FromUInt32(0xff333333);
    }

    private void PopulateRegularLight()
    {
        Background = new(0xFFFAFAFB);
        Foreground = new(0xFF1C1C1C);
        BackgroundLow = ChromeBrushes.VeryLightGray;
        BackgroundHigh = ChromeBrushes.White;
        BorderBrush = new(0xFFC0C0C0);
        WindowBorder = new(0xFF909090);
        BufferBarBrush = new(0xFF505050);  //ChromeBrushes.DarkGray;
        GroupBorder = ChromeBrushes.LightGray;

        // Same as expected AltHigh
        FocusedBox = ChromeBrushes.White;

        // SET ONLY THOSE COLORS WE NEED (commented out for reference)
        // Even setting some to same as theme can break some controls
        // Accent="#ff0073cf"
        // Accent = ChromeBrushes.DefaultAccent.Color;

        // PRIMARY FOREGROUND
        // BaseHigh ="Black"
        BaseHigh = Foreground.Color;

        // PRIMARY BACKGROUND
        RegionColor = Background.Color;


        // Focused textbox background
        // AltHigh ="White"
        // AltHigh = Colors.White;

        // AltLow ="White"
        // AltLow = Colors.White;

        // AltMedium ="White"
        // AltMedium = Colors.White;

        // Hover TextBox background
        // AltMediumHigh ="White"
        // AltMediumHigh = Colors.White;

        // Unfocused TextBox background
        // AltMediumLow ="White"
        AltMediumLow = BackgroundLow.Color;


        // BaseMedium="#ff898989"
        // BaseMedium = Color.FromUInt32(0xff898989);

        // BaseMediumHigh="#ff5d5d5d"
        // BaseMediumHigh = Color.FromUInt32(0xff5d5d5d);

        // BaseMediumLow="#ff737373"
        // BaseMediumLow = Color.FromUInt32(0xff737373);

        // CONFLICT: Thumb for scrollbar
        // Also: Background for disabled region
        // DO NOT SET (causes disabled toggle switches and scrollbar to be obscured)
        // BaseLow="#ffcccccc"
        // BaseLow = Color.FromUInt32(0xffcccccc);


        // ChromeAltLow="#ff5d5d5d"
        // ChromeAltLow = Color.FromUInt32(0xff5d5d5d);

        // ChromeBlackHigh ="Black"
        // ChromeBlackHigh = Colors.Black;

        // ChromeBlackLow ="#ffcccccc"
        // ChromeBlackLow = Color.FromUInt32(0xffcccccc);

        // ChromeBlackMedium ="#ff5d5d5d"
        // ChromeBlackMedium = Color.FromUInt32(0xff5d5d5d);

        // ChromeBlackMediumLow ="#ff898989"
        // ChromeBlackMediumLow = Color.FromUInt32(0xff898989);

        // Disabled Slider foreground
        // ChromeDisabledHigh ="#ffcccccc"
        // ChromeDisabledHigh = Color.FromUInt32(0xffcccccc);

        // Disabled foreground
        // ChromeDisabledLow ="#ff898989"
        // ChromeDisabledLow = Color.FromUInt32(0xff898989);


        // ChromeGray="#ff737373"
        // ChromeGray = Color.FromUInt32(0xff737373);

        // ChromeHigh ="#ffcccccc"
        // ChromeHigh = Color.FromUInt32(0xffcccccc);

        // ChromeLow ="#ffececec"
        // ChromeLow = Color.FromUInt32(0xffececec);

        // ChromeMedium ="#ffe6e6e6"
        // ChromeMedium = Color.FromUInt32(0xffe6e6e6);

        // ChromeMediumLow ="#ffececec"
        // ChromeMediumLow = Color.FromUInt32(0xffececec);

        // ChromeWhite ="White"
        // ChromeWhite = Colors.White;


        // ContextMenu hover background
        // ListLow ="#ffe6e6e6"
        // ListLow = Color.FromUInt32(0xffe6e6e6);

        // ComboBox pressed background
        // ContextMenu pressed background
        // ListMedium ="#ffcccccc"
        // ListMedium = Color.FromUInt32(0xffcccccc);
    }
}
