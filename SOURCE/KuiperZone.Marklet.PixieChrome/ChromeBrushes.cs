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

using Avalonia.Media.Immutable;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Application specific color brushes.
/// </summary>
public static class ChromeBrushes
{
    /// <summary>
    /// Blue accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush BlueAccent = new(0xFF3584E4);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush BlueLightAccent = new(0xFF5A81B1);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush BlueDarkAccent = new(0xFF1A4172);

    /// <summary>
    /// Teal accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush TealAccent = new(0xFF2190A4);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush TealLightAccent = new(0xFF508791);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush TealDarkAccent = new(0xFF104752);

    /// <summary>
    /// Green accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush GreenAccent = new(0xFF3A944A);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush GreenLightAccent = new(0xFF5C8964);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush GreenDarkAccent = new(0xFF1C4A24);

    /// <summary>
    /// Yellow accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush YellowAccent = new(0xFFC88800);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush YellowLightAccent = new(0xFFA3833F);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush YellowDarkAccent = new(0xFF644400);

    /// <summary>
    /// Orange accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush OrangeAccent = new(0xFFED5B00);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush OrangeLightAccent = new(0xFFB66D3F);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush OrangeDarkAccent = new(0xFF762D00);

    /// <summary>
    /// Red accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush RedAccent = new(0xFFE62D42);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush RedLightAccent = new(0xFFB25660);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush RedDarkAccent = new(0xFF731620);

    /// <summary>
    /// Pink accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush PinkAccent = new(0xFFD56199);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush PinkLightAccent = new(0xFFAA708C);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush PinkDarkAccent = new(0xFF6A304C);

    /// <summary>
    /// Purple accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush PurpleAccent = new(0xFF9141AC);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush PurpleLightAccent = new(0xFF886095);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush PurpleDarkAccent = new(0xFF482056);

    /// <summary>
    /// Slate accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush SlateAccent = new(0xFF6F8396);

    /// <summary>
    /// Light accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush SlateLightAccent = new(0xFF77818A);

    /// <summary>
    /// Dark accent variation.
    /// </summary>
    public static readonly ImmutableSolidColorBrush SlateDarkAccent = new(0xFF37414B);

    /// <summary>
    /// Default accent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush DefaultAccent = BlueAccent;

    /// <summary>
    /// Gets a static very dark gray.
    /// </summary>
    public static readonly ImmutableSolidColorBrush VeryDarkGray = new(0xFF18181B);

    /// <summary>
    /// Gets a static dark gray.
    /// </summary>
    public static readonly ImmutableSolidColorBrush DarkGray = new(0xFF36363A);

    /// <summary>
    /// Gets a static mid-gray brush. 0xFFA0A0A0?
    /// </summary>
    public static readonly ImmutableSolidColorBrush MidGray = new(0x80808080);

    /// <summary>
    /// Gets a static light gray.
    /// </summary>
    public static readonly ImmutableSolidColorBrush LightGray = new(0xFFE0E0E0);

    /// <summary>
    /// Gets a static very light gray.
    /// </summary>
    public static readonly ImmutableSolidColorBrush VeryLightGray = new(0xFFF2F2F4);

    /// <summary>
    /// Gets a static black brush.
    /// </summary>
    public static readonly ImmutableSolidColorBrush Black = new(0xFF000000);

    /// <summary>
    /// Gets a static white brush.
    /// </summary>
    public static readonly ImmutableSolidColorBrush White = new(0xFFFFFFFF);

    /// <summary>
    /// Gets a static transparent.
    /// </summary>
    public static readonly ImmutableSolidColorBrush Transparent = new(0x00000000);

}
