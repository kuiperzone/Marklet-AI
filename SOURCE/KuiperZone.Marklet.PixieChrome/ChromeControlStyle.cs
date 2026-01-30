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
/// Chrome window button style options affecting minimize, maximize and close buttons.
/// </summary>
/// <remarks>
/// Integer values may be written to disk and should not be changed.
/// </remarks>
public enum ChromeControlStyle
{
    /// <summary>
    /// Default traditional.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Only close button is shown.
    /// </summary>
    /// <remarks>
    /// Minimize and maximize buttons are hidden, even if the window supports these functions. This is similar to a
    /// default Gnome window without tweaks.
    /// </remarks>
    CloseOnly = 1,

    /// <summary>
    /// Only large close button is shown.
    /// </summary>
    /// <remarks>
    /// Minimize and maximize buttons are hidden, even if the window supports these functions. This option is intended
    /// for use with dialog windows.
    /// </remarks>
    LargeCloseOnly = 2,

    /// <summary>
    /// Up-down arrows and diamond restore (similar to some desktop themes).
    /// </summary>
    DiamondArrows = 3,

    /// <summary>
    /// Diagonal arrows and box restore (similar to some desktop themes).
    /// </summary>
    DiagonalArrows = 4,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns true if larger font size.
    /// </summary>
    public static bool IsLarger(this ChromeControlStyle src)
    {
        return src == ChromeControlStyle.LargeCloseOnly;
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static ChromeControlStyle TrimLegal(this ChromeControlStyle src)
    {
        return Enum.IsDefined(src) ? src : ChromeControlStyle.Default;
    }

}
