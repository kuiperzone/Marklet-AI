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

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Chrome window button background display options affecting minimize, maximize and close buttons.
/// </summary>
/// <remarks>
/// Integer values may be written to disk and should not be changed.
/// </remarks>
public enum ChromeControlBackground
{
    /// <summary>
    /// Default transparent.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Control buttons are shown with circle background.
    /// </summary>
    Square = 1,

    /// <summary>
    /// Control buttons are shown as square with the close button accented.
    /// </summary>
    AccentedSquare = 2,

    /// <summary>
    /// Control buttons are shown with circle background.
    /// </summary>
    Circle = 3,

    /// <summary>
    /// Control buttons are shown as circle with the close button accented.
    /// </summary>
    AccentedCircle = 4,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns true if the button has a background, i.e. not transparent.
    /// </summary>
    public static bool HasBackground(this ChromeControlBackground src)
    {
        return src != ChromeControlBackground.Default;
    }

    /// <summary>
    /// Returns true if a circle.
    /// </summary>
    public static bool IsCircle(this ChromeControlBackground src)
    {
        return src == ChromeControlBackground.Circle || src == ChromeControlBackground.AccentedCircle;
    }

    /// <summary>
    /// Returns true if "close" is accented.
    /// </summary>
    public static bool IsCloseAccented(this ChromeControlBackground src)
    {
        return src == ChromeControlBackground.AccentedSquare || src == ChromeControlBackground.AccentedCircle;
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static ChromeControlBackground TrimLegal(this ChromeControlBackground src)
    {
        return Enum.IsDefined(src) ? src : ChromeControlBackground.Default;
    }

}
