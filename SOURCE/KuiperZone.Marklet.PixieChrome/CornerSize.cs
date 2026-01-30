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
/// Window and control corners.
/// </summary>
/// <remarks>
/// Integer values may be written to disk and should not change.
/// </remarks>
public enum CornerSize
{
    /// <summary>
    /// Default.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Zero radius.
    /// </summary>
    None = 1,

    /// <summary>
    /// Small corner radius.
    /// </summary>
    Small = 2,

    /// <summary>
    /// Medium corner radius.
    /// </summary>
    Medium = 3,

    /// <summary>
    /// Large corner radius.
    /// </summary>
    Large = 4,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets the default corner pixel radius.
    /// </summary>
    public static double ToPixels(this CornerSize src)
    {
        switch (src)
        {
            case CornerSize.None: return 0.0;
            case CornerSize.Small: return 6.0;
            case CornerSize.Large: return 16.0;
            default: return 11.0; // medium
        }
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static CornerSize TrimLegal(this CornerSize src)
    {
        return Enum.IsDefined(src) ? src : CornerSize.Default;
    }
}
