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

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Fixed categories for <see cref="GardenSession"/> instances, excluding "waste".
/// </summary>
/// <remarks>
/// Note that the "waste" or "trash" bin is considered separately, as moving an item to the "waste" category would
/// mean losing the original designation. Integer values are written to storage and must not change.
/// </remarks>
public enum BinKind
{
    /// <summary>
    /// Instance is categorized as "home", "recent" or "working".
    /// </summary>
    Home = 0,

    /// <summary>
    /// Instance is categorized as "archived" and not subject to housekeeping.
    /// </summary>
    Archive = 1,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static BinKind TrimLegal(this BinKind src)
    {
        return Enum.IsDefined(src) ? src : BinKind.Home;
    }
}