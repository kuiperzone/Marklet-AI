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
using Avalonia.Media.Fonts;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Application specific font families.
/// </summary>
public static class AppFonts
{
    static AppFonts()
    {
        const string NSpace = $"{nameof(AppFonts)}.Static";
        const string AssetPrefix = "fonts:AppFonts";

        // Link to for convenience only
        DefaultFamily = ChromeFonts.DefaultFamily;
        MonospaceFamily = ChromeFonts.MonospaceFamily;
        ConditionalDebug.WriteLine(NSpace, "LOADING FONTS");

        try
        {
            var fc = new EmbeddedFontCollection(
                new Uri(AssetPrefix, UriKind.Absolute),
                new Uri("avares://KuiperZone.Marklet/Assets/Fonts", UriKind.Absolute));

            FontManager.Current.AddFontCollection(fc);
            SlabFamily = new($"{AssetPrefix}#Roboto Slab");
            VintageFamily = new($"{AssetPrefix}#Comfortaa");
            ArtisticFamily = new($"{AssetPrefix}#Josefin Slab");
        }
        catch (Exception e)
        {
            // Expected to throw in unit test
            ConditionalDebug.WriteLine(NSpace, "WARNING: Font load failed");
            ConditionalDebug.WriteLine(NSpace, e.Message);
            SlabFamily = DefaultFamily;
            VintageFamily = DefaultFamily;
            ArtisticFamily = DefaultFamily;
        }
    }

    /// <summary>
    /// Gets the default application font family.
    /// </summary>
    public static readonly FontFamily DefaultFamily;

    /// <summary>
    /// Gets the  monospace font family.
    /// </summary>
    public static readonly FontFamily MonospaceFamily;

    /// <summary>
    /// Gets a size correction value for <see cref="MonospaceFamily"/>.
    /// </summary>
    public const double MonospaceCorrection = 1.0;

    /// <summary>
    /// Gets the slab font family.
    /// </summary>
    public static readonly FontFamily SlabFamily;

    /// <summary>
    /// Gets a size correction value for <see cref="SlabFamily"/>.
    /// </summary>
    public const double SlabCorrection = 1.1;

    /// <summary>
    /// Gets the "vintage" font family.
    /// </summary>
    public static readonly FontFamily VintageFamily;

    /// <summary>
    /// Gets a size correction value for <see cref="VintageFamily"/>.
    /// </summary>
    public const double VintageCorrection = 1.0;

    /// <summary>
    /// Gets the "artistic" font family.
    /// </summary>
    /// <remarks>
    /// This font is to be found in "Assets".
    /// </remarks>
    public static readonly FontFamily ArtisticFamily;

    /// <summary>
    /// Gets a size correction value for <see cref="ArtisticFamily"/>.
    /// </summary>
    public const double ArtisticCorrection = 1.26;

}
