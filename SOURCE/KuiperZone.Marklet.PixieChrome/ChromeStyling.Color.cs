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

using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using KuiperZone.Marklet.PixieChrome.Internal;
using KuiperZone.Marklet.Tooling;
using ReactiveUI;

namespace KuiperZone.Marklet.PixieChrome;

// Implementation of color and brush related properties for partial class.
public sealed partial class ChromeStyling
{
    private const double TintOpacity = 0.15;
    private Color _accentColor = ChromeBrushes.DefaultAccent.Color;
    private ImmutableSolidColorBrush? _accentBrush;
    private ImmutableSolidColorBrush? _accent50;
    private ImmutableSolidColorBrush? _accent35;
    private ImmutableSolidColorBrush? _accent25;

    private Color _tintColor;

    /// <summary>
    /// Gets the button background.
    /// </summary>
    /// <remarks>
    /// The color is semi-transparent and intended to be light/dark independent.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush ButtonBrush = new(0x40787878);

    /// <summary>
    /// Gets the button hover background.
    /// </summary>
    /// <remarks>
    /// The color is semi-transparent and intended to be light/dark independent.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush ButtonHover = new(0x60B0B0C0);

    /// <summary>
    /// Gets the button checked and/or pressed background.
    /// </summary>
    /// <remarks>
    /// The color is semi-transparent and intended to be light/dark independent.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush ButtonCheckedPressed = new(0x608080A0);

    /// <summary>
    /// Gets the group hover background.
    /// </summary>
    /// <remarks>
    /// The color is semi-transparent and intended to be light/dark independent.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush PixieHover = ChromeBrushes.Highlight;

    /// <summary>
    /// Gets the group shade background.
    /// </summary>
    /// <remarks>
    /// The color is semi-transparent and intended to be light/dark independent.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush GroupShade = ChromeBrushes.Highlight;

    /// <summary>
    /// Gets the (disabled) foreground gray.
    /// </summary>
    /// <remarks>
    /// The color is semi-transparent and intended to be light/dark independent.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush GrayForeground = new(0xFF909090);

    /// <summary>
    /// Gets whether the actual displayed theme is dark.
    /// </summary>
    public bool IsActualThemeDark { get; private set; }

    /// <summary>
    /// Gets the primary foreground used by the current theme.
    /// </summary>
    public ImmutableSolidColorBrush Foreground { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets the primary background used by the current theme (RegionColor).
    /// </summary>
    public ImmutableSolidColorBrush Background { get; private set; } = ChromeBrushes.MidGray;

    /// <summary>
    /// Gets the "low" (darker) background used by the current theme (AltHigh).
    /// </summary>
    public ImmutableSolidColorBrush BackgroundLow { get; private set; } = ChromeBrushes.MidGray;

    /// <summary>
    /// Gets the "high" (lighter) background used by the current theme.
    /// </summary>
    public ImmutableSolidColorBrush BackgroundHigh { get; private set; } = ChromeBrushes.MidGray;

    /// <summary>
    /// Gets a brush for the left-hand buffer bar.
    /// </summary>
    public ImmutableSolidColorBrush BufferBarBrush { get; private set; } = ChromeBrushes.DarkGray;

    /// <summary>
    /// Gets a brush for borders, dividers and window border.
    /// </summary>
    /// <remarks>
    /// If color is to similar to background, use <see cref="WindowBorder"/> instead.
    /// </remarks>
    public ImmutableSolidColorBrush BorderBrush { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets a border brush for ContextMenus and other pop-outs.
    /// </summary>
    public ImmutableSolidColorBrush WindowBorder { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets the background for a focused TextBox.
    /// </summary>
    public ImmutableSolidColorBrush FocusedBox { get; private set; } = ChromeBrushes.Black;

    /// <summary>
    /// Gets or sets the theme accent color.
    /// </summary>
    /// <remarks>
    /// The color is not sensitive to light or dark theme. The value is same for all themes. Chaning invokes <see
    /// cref="StylingChanged"/>.
    /// </remarks>
    public Color AccentColor
    {
        get { return _accentColor; }

        set
        {
            if (_accentColor != value)
            {
                _accentColor = value.A != 0 ? value : ChromeBrushes.DefaultAccent.Color;
                ThemePalette.SetAccents(_accentColor);

                _accentBrush = null;
                _accent50 = null;
                _accent35 = null;
                _accent25 = null;

                this.RaisePropertyChanged(nameof(AccentColor));
                this.RaisePropertyChanged(nameof(AccentBrush));
                this.RaisePropertyChanged(nameof(Accent50));
                this.RaisePropertyChanged(nameof(Accent35));
                this.RaisePropertyChanged(nameof(Accent25));

                // Now fixed
                // this.RaisePropertyChanged(nameof(LinkForeground));
                // this.RaisePropertyChanged(nameof(LinkHover));

                OnStylingChanged();
            }
        }
    }

    /// <summary>
    /// Gets the accent brush as solid color with 1.0 opacity.
    /// </summary>
    /// <remarks>
    /// The color is not sensitive to light or dark theme.
    /// </remarks>
    public ImmutableSolidColorBrush AccentBrush
    {
        get { return _accentBrush ??= new ImmutableSolidColorBrush(_accentColor); }
    }

    /// <summary>
    /// Gets the "text selection" accent background brush with 50% opacity.
    /// </summary>
    /// <remarks>
    /// The color is derived from <see cref="AccentColor"/>.
    /// </remarks>
    public ImmutableSolidColorBrush Accent50
    {
        get { return _accent50 ??= new ImmutableSolidColorBrush(_accentColor, 0.50); }
    }

    /// <summary>
    /// Gets the "text selection" accent background brush with 35% opacity.
    /// </summary>
    /// <remarks>
    /// The color is derived from <see cref="AccentColor"/>.
    /// </remarks>
    public ImmutableSolidColorBrush Accent35
    {
        get { return _accent35 ??= new ImmutableSolidColorBrush(_accentColor, 0.35); }
    }

    /// <summary>
    /// Gets the "text selection" accent background brush with 20% opacity.
    /// </summary>
    /// <remarks>
    /// The color is derived from <see cref="AccentColor"/>.
    /// </remarks>
    public ImmutableSolidColorBrush Accent25
    {
        get { return _accent25 ??= new ImmutableSolidColorBrush(_accentColor, 0.25); }
    }

    /// <summary>
    /// Gets the link foreground brush.
    /// </summary>
    /// <remarks>
    /// Currently a fixed value.
    /// </remarks>
    public ImmutableSolidColorBrush LinkForeground { get; private set; } = ChromeBrushes.BlueAccent;

    /// <summary>
    /// Gets the group background brush.
    /// </summary>
    /// <remarks>
    /// Currently a fixed value.
    /// </remarks>
    public ImmutableSolidColorBrush LinkHover { get; private set; } = ChromeBrushes.BlueLightAccent;

    /// <summary>
    /// Gets or sets the tint color
    /// </summary>
    public Color TintColor
    {
        get { return _tintColor; }

        set
        {
            if (_tintColor != value)
            {
                _tintColor = value;

                this.RaisePropertyChanged(nameof(TintColor));

                RaiseTint(value);
                OnStylingChanged();
            }
        }
    }

    /// <summary>
    /// Gets an overlay tint brush.
    /// </summary>
    public IBrush? TintBrush { get; private set; }

    /// <summary>
    /// Gets an overlay tint brush.
    /// </summary>
    public IBrush? TintBackground { get; private set; }

    /// <summary>
    /// Gets a contrasting foreground color to given background.
    /// </summary>
    public IBrush? GetContrastForeground(Color background)
    {
        var variant = background.IsDark() ? ThemeVariant.Dark : ThemeVariant.Light;
        return ThemePalette.Get(variant, _preferFluentDark).Foreground;
    }

    private void RaiseColorProperties(ThemePalette palette)
    {
        const string NSpace = $"{nameof(ChromeStyling)}.{nameof(RaiseColorProperties)}";
        Diag.WriteLine(NSpace, "Raising");

        Foreground = palette.Foreground;
        Background = palette.Background;
        BackgroundLow = palette.BackgroundLow;
        BackgroundHigh = palette.BackgroundHigh;
        BufferBarBrush = palette.BufferBarBrush;
        BorderBrush = palette.BorderBrush;
        WindowBorder = palette.WindowBorder;
        FocusedBox = palette.FocusedBox;

        // Don't raise Accent related properties
        this.RaisePropertyChanged(nameof(IsActualThemeDark));
        this.RaisePropertyChanged(nameof(Foreground));
        this.RaisePropertyChanged(nameof(Background));
        this.RaisePropertyChanged(nameof(BackgroundLow));
        this.RaisePropertyChanged(nameof(BackgroundHigh));
        this.RaisePropertyChanged(nameof(BufferBarBrush));
        this.RaisePropertyChanged(nameof(BorderBrush));
        this.RaisePropertyChanged(nameof(WindowBorder));
        this.RaisePropertyChanged(nameof(FocusedBox));

        RaiseTint(_tintColor);
    }

    private void RaiseTint(Color value)
    {
        if (value != default)
        {
            TintBrush = new ImmutableSolidColorBrush(value, TintOpacity);
            TintBackground = new ImmutableSolidColorBrush(value.Blend(TintOpacity, BackgroundLow.Color));
        }
        else
        {
            TintBrush = null;
            TintBackground = BackgroundLow;
        }

        this.RaisePropertyChanged(nameof(TintBrush));
        this.RaisePropertyChanged(nameof(TintBackground));
    }
}
