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
using KuiperZone.Marklet.PixieChrome;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Font category (family style).
/// </summary>
/// <remarks>
/// Integer values may be written to disk and should not be changed.
/// </remarks>
public enum FontCategory
{
    /// <summary>
    /// Default sans-serif.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Monospace.
    /// </summary>
    Mono = 1,

    /// <summary>
    /// Slab font.
    /// </summary>
    Slab = 2,

    /// <summary>
    /// A artistic font.
    /// </summary>
    Vintage = 3,

    /// <summary>
    /// A artistic font.
    /// </summary>
    Artistic = 4,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets whether the font is monospace.
    /// </summary>
    public static bool IsMonospace(this FontCategory src)
    {
        return src == FontCategory.Mono;
    }

    /// <summary>
    /// Gets the font family.
    /// </summary>
    public static FontFamily ToFamily(this FontCategory src)
    {
        switch (src)
        {
            case FontCategory.Mono:
                return AppFonts.MonospaceFamily;
            case FontCategory.Slab:
                return AppFonts.SlabFamily;
            case FontCategory.Vintage:
                return AppFonts.VintageFamily;
            case FontCategory.Artistic:
                return AppFonts.ArtisticFamily;
            default:
                return ChromeFonts.DefaultFamily;
        }
    }

    /// <summary>
    /// Gets a font size correction factor.
    /// </summary>
    public static double ToCorrection(this FontCategory src)
    {
        switch (src)
        {
            case FontCategory.Mono:
                return AppFonts.MonospaceCorrection;
            case FontCategory.Slab:
                return AppFonts.SlabCorrection;
            case FontCategory.Vintage:
                return AppFonts.VintageCorrection;
            case FontCategory.Artistic:
                return AppFonts.ArtisticCorrection;
            default:
                return 1.0;
        }
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static FontCategory TrimLegal(this FontCategory src)
    {
        return Enum.IsDefined(src) ? src : FontCategory.Default;
    }
}
