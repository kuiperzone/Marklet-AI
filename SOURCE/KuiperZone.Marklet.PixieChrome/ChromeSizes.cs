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

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Static class providing consistent sizing values which key off <see cref="ChromeFonts.DefaultFontSize"/>.
/// </summary>
/// <remarks>
/// Increasing <see cref="ChromeFonts.DefaultFontSize"/> will increase these values.
/// </remarks>
public static class ChromeSizes
{
    // Base margin/padding. Scales if we increase font size.
    // Chosen as minimum to safely keep controls away
    // from side with Large rounded corners
    private const double StdX = ChromeFonts.DefaultFontSize * 0.6250;
    private const double StdY = ChromeFonts.DefaultFontSize * 0.4286;

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
    /// Gets a small pixel spacer (~3px at font-size 14).
    /// </summary>
    public const double SmallPx = StdY * 0.5;

    /// <summary>
    /// Gets medium pixel spacer (~6px at font-size 14).
    /// </summary>
    public const double StandardPx = StdY;

    /// <summary>
    /// Gets large pixel spacer (~12px at font-size 14).
    /// </summary>
    public const double LargePx = StdY * 2.0;

    /// <summary>
    /// Gets large pixel spacer (~24px at font-size 14).
    /// </summary>
    public const double HugePx = StdY * 4.0;

    /// <summary>
    /// Gets large pixel spacer (~48px at font-size 14).
    /// </summary>
    public const double GiantPx = StdY * 8.0;

    /// <summary>
    /// Gets minimum dialog button width (~80px).
    /// </summary>
    public const double MinDialogButtonWidth = ChromeFonts.DefaultFontSize * 5.7143;

    /// <summary>
    /// Gets minimum dialog button height (~32px).
    /// </summary>
    public const double MinDialogButtonHeight = ChromeFonts.DefaultFontSize * 2.2857;


    /// <summary>
    /// Gets padding 0.25 x size of <see cref="StandardPadding"/>.
    /// </summary>
    /// <remarks>
    /// Intended to be approx 2 pixels. The padding is not uniform.
    /// </remarks>
    public static readonly Thickness TinyPadding = new(0.25 * StdX, 0.25 * StdY);

    /// <summary>
    /// Gets left-right of <see cref="TinyPadding"/> only.
    /// </summary>
    public static readonly Thickness TinyLeftRight = new(0.25 * StdX, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="TinyPadding"/> only.
    /// </summary>
    public static readonly Thickness TinyTopBottom = new(0.0, 0.25 * StdY);


    /// <summary>
    /// Gets padding 0.5 x size of <see cref="StandardPadding"/>.
    /// </summary>
    /// <remarks>
    /// Intended to be approx 4 pixels. The padding is not uniform.
    /// </remarks>
    public static readonly Thickness SmallPadding = new(0.5 * StdX, 0.5 * StdY);

    /// <summary>
    /// Gets left-right of <see cref="SmallPadding"/> only.
    /// </summary>
    public static readonly Thickness SmallLeftRight = new(0.5 * StdX, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="SmallPadding"/> only.
    /// </summary>
    public static readonly Thickness SmallTopBottom = new(0.0, 0.5 * StdY);


    /// <summary>
    /// Gets a padding sufficient to ensure contents are clear of rounded corners at <see cref="CornerSize.Large"/>.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness StandardPadding = new(StdX, StdY);

    /// <summary>
    /// Gets left-right of <see cref="StandardPadding"/> only.
    /// </summary>
    public static readonly Thickness StandardLeftRight = new(StdX, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="StandardPadding"/> only.
    /// </summary>
    public static readonly Thickness StandardTopBottom = new(0.0, StdY);


    /// <summary>
    /// Gets padding 2 x size of <see cref="StandardPadding"/>.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness LargePadding = new(2.0 * StdX, 2.0 * StdY);

    /// <summary>
    /// Gets left-right of <see cref="LargePadding"/> only.
    /// </summary>
    public static readonly Thickness LargeLeftRight = new(2.0 * StdX, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="LargePadding"/> only.
    /// </summary>
    public static readonly Thickness LargeTopBottom = new(0.0, 2.0 * StdY);


    /// <summary>
    /// Gets padding 4 x size of <see cref="StandardPadding"/>.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness HugePadding = new(4.0 * StdX, 4.0 * StdY);

    /// <summary>
    /// Gets left-right of <see cref="HugePadding"/> only.
    /// </summary>
    public static readonly Thickness HugeLeftRight = new(4.0 * StdX, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="HugePadding"/> only.
    /// </summary>
    public static readonly Thickness HugeTopBottom = new(0.0, 4.0 * StdY);


    /// <summary>
    /// Gets padding 8 x size of <see cref="StandardPadding"/>.
    /// </summary>
    /// <remarks>
    /// The padding is not uniform.
    /// </remarks>
    public static readonly Thickness GiantPadding = new(8.0 * StdX, 8.0 * StdY);

    /// <summary>
    /// Gets left-right of <see cref="GiantPadding"/> only.
    /// </summary>
    public static readonly Thickness GiantLeftRight = new(8.0 * StdX, 0.0);

    /// <summary>
    /// Gets top-bottom of <see cref="GiantPadding"/> only.
    /// </summary>
    public static readonly Thickness GiantTopBottom = new(0.0, 8.0 * StdY);

}
