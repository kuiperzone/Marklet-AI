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

using KuiperZone.Marklet.Controls;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// The internal <see cref="ContentViewer"/> width.
/// </summary>
/// <remarks>
/// Integer values may be written to disk and should not change.
/// </remarks>
public enum ContentWidth
{
    /// <summary>
    /// Default.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Narrow width.
    /// </summary>
    Narrow = 1,

    /// <summary>
    /// Medium width.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Wide width.
    /// </summary>
    Wide = 3,

    /// <summary>
    /// Full view width.
    /// </summary>
    FullWidth = 4,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets the width in DIPs.
    /// </summary>
    public static double ToPixels(this ContentWidth src)
    {
        switch (src)
        {
            case ContentWidth.Narrow: return 600.0;
            case ContentWidth.Wide: return 1200.0;
            case ContentWidth.FullWidth: return double.PositiveInfinity;
            default: return 900.0; // medium
        }
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static ContentWidth TrimLegal(this ContentWidth src)
    {
        return Enum.IsDefined(src) ? src : ContentWidth.Default;
    }

}
