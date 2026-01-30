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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Truncation style.
/// </summary>
public enum TruncStyle
{
    /// <summary>
    /// The end/right portion of the text is truncated at the specified width. This is suitable for left-to-right
    /// scripts.
    /// </summary>
    End = 0,

    /// <summary>
    /// Truncated text ends with ellipses. Suitable for left-to-right scripts.
    /// </summary>
    EndEllipses,

    /// <summary>
    /// The start/left portion of the text is truncated at the specified width. Suitable for right-to-left scripts.
    /// </summary>
    Start,

    /// <summary>
    /// Truncated text starts with ellipses. Suitable for right-to-left scripts.
    /// </summary>
    StartEllipses,

    /// <summary>
    /// Text which is truncated is shortened by replacing characters in the center of the text with
    /// ellipses.
    /// </summary>
    CenterEllipses
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns true for <see cref="TruncStyle.Start"/> or <see cref="TruncStyle.StartEllipses"/>.
    /// </summary>
    public static bool IsLeft(this TruncStyle src)
    {
        return src == TruncStyle.Start || src == TruncStyle.StartEllipses;
    }

    /// <summary>
    /// Returns true for <see cref="TruncStyle.End"/> or <see cref="TruncStyle.EndEllipses"/>.
    /// </summary>
    public static bool IsRight(this TruncStyle src)
    {
        return src == TruncStyle.End || src == TruncStyle.EndEllipses;
    }
}
