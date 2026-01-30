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
/// Sort order identifiers.
/// </summary>
public enum GardenSort
{
    /// <summary>
    /// Sort by creation time with oldest first (ascending).
    /// </summary>
    CreationOldestFirst = 0,

    /// <summary>
    /// Sort by creation time with newest first (descending).
    /// </summary>
    CreationNewestFirst,

    /// <summary>
    /// Sort by update time with oldest first (ascending).
    /// </summary>
    UpdateOldestFirst,

    /// <summary>
    /// Sort by update time with newest first (descending).
    /// </summary>
    UpdateNewestFirst,

    /// <summary>
    /// Sort by access time with oldest first (ascending).
    /// </summary>
    AccessOldestFirst,

    /// <summary>
    /// Sort by access time with newest first (descending).
    /// </summary>
    AccessNewestFirst,

    /// <summary>
    /// Alpha sort by title.
    /// </summary>
    Title
}
