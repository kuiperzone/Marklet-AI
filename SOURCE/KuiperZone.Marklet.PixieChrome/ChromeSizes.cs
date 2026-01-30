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

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Static class providing consistent sizing values which key off <see cref="ChromeFonts.DefaultFontSize"/>.
/// </summary>
/// <remarks>
/// Increasing <see cref="ChromeFonts.DefaultFontSize"/> will increase these values.
/// </remarks>
public static class ChromeSizes
{
    /// <summary>
    /// Gets the <see cref="ChromeFonts.DefaultFontSize"/> multipled by 0.55 as a rough char width approximation.
    /// </summary>
    public const double OneCh = ChromeFonts.DefaultFontSize * 0.55;

    /// <summary>
    /// Gets <see cref="OneCh"/> multiplied by 2.0 as a rough approximation of 2 space characters.
    /// </summary>
    public const double TwoCh = OneCh * 2.0;

    /// <summary>
    /// Gets <see cref="OneCh"/> multiplied by 4.0 as a rough approximation of 4 space characters.
    /// </summary>
    public const double TabPx = OneCh * 4.0;

    /// <summary>
    /// Gets a small pixels spacer (6px at font-size 14).
    /// </summary>
    public const double SmallSpacerPx = ChromeFonts.DefaultFontSize * 0.4285;

    /// <summary>
    /// Gets medium pixels spacer (12px at font-size 14).
    /// </summary>
    public const double MediumSpacerPx = SmallSpacerPx * 2.0;

    /// <summary>
    /// Gets large pixels spacer (24px at font-size 14).
    /// </summary>
    public const double LargeSpacerPx = SmallSpacerPx * 4.0;

    /// <summary>
    /// Gets large pixels spacer (32px at font-size 14).
    /// </summary>
    public const double HugeSpacerPx = SmallSpacerPx * 5.3333;

    /// <summary>
    /// Gets the GridSplitter Height or Width (4px at font-size 14).
    /// </summary>
    public const double SplitterSize = ChromeFonts.DefaultFontSize / 4.5;

    /// <summary>
    /// Gets minimum dialog button width.
    /// </summary>
    public const double MinDialogButtonWidth = ChromeFonts.DefaultFontSize * 5.7143; // <- ~80px

    /// <summary>
    /// Gets minimum dialog button height.
    /// </summary>
    public const double MinDialogButtonHeight = ChromeFonts.DefaultFontSize * 2.2857; // <- ~32px

    /// <summary>
    /// Gets a padding sufficient to ensure contents are clear of rounded corners.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness RegularPadding = new(ChromeFonts.LargeFontSize / 2.0, ChromeFonts.LargeFontSize / 3.0);

    /// <summary>
    /// Gets left-right of <see cref="RegularPadding"/> only.
    /// </summary>
    public static readonly Thickness RegularLeftRight = new(RegularPadding.Left, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="RegularPadding"/> only.
    /// </summary>
    public static readonly Thickness RegularTopBottom = new(0.0, RegularPadding.Top);

    /// <summary>
    /// Gets padding twice the size of <see cref="RegularPadding"/>.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness LargePadding = new(ChromeFonts.LargeFontSize, 2.0 * ChromeFonts.LargeFontSize / 3.0);

    /// <summary>
    /// Gets left-right of <see cref="LargePadding"/> only.
    /// </summary>
    public static readonly Thickness LargeLeftRight = new(LargePadding.Left, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="LargePadding"/> only.
    /// </summary>
    public static readonly Thickness LargeTopBottom = new(0.0, LargePadding.Top);

    /// <summary>
    /// Gets padding twice the size of <see cref="LargePadding"/>.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness HugePadding = new(2.0 * ChromeFonts.LargeFontSize, 4.0 * ChromeFonts.LargeFontSize / 3.0);

    /// <summary>
    /// Gets left-right of <see cref="HugePadding"/> only.
    /// </summary>
    public static readonly Thickness HugeLeftRight = new(HugePadding.Left, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="HugePadding"/> only.
    /// </summary>
    public static readonly Thickness HugeTopBottom = new(0.0, HugePadding.Top);

}
