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

namespace KuiperZone.Marklet.Stack.Garden.Internal;

/// <summary>
/// Provides change information for <see cref="GardenSession"/>.
/// </summary>
[Flags]
internal enum ModFlags
{
    /// <summary>
    /// No change.
    /// </summary>
    None = 0x00000000,

    /// <summary>
    /// The <see cref="GardenSession.UpdateTime"/> has changed.
    /// </summary>
    Time = 0x00000001,

    /// <summary>
    /// The <see cref="GardenSession.HomeBin"/> has changed.
    /// </summary>
    HomeBin = 0x00000002,

    /// <summary>
    /// The <see cref="GardenSession.IsWaste"/> state has changed.
    /// </summary>
    IsWaste = 0x00000004,

    /// <summary>
    /// The <see cref="GardenSession.Title"/> has changed.
    /// </summary>
    Title = 0x00000008,

    /// <summary>
    /// The <see cref="GardenSession.Model"/> has changed.
    /// </summary>
    Model = 0x00000010,

    /// <summary>
    /// The <see cref="GardenSession.Topic"/> has changed.
    /// </summary>
    Topic = 0x00000020,

    /// <summary>
    /// A <see cref="GardenLeaf"/> child has changed.
    /// </summary>
    /// <remarks>
    /// This typically implies <see cref="GardenLeaf.Content"/> has changed but may include changes to other <see
    /// cref="GardenLeaf"/> properties.
    /// </remarks>
    Leaf = 0x00000040,
}

/// <summary>
/// Extension methods.
/// </summary>
internal static partial class HelperExt
{
    /// <summary>
    /// Returns whether value contains a "visual" flag.
    /// </summary>
    public static bool HasVisual(this ModFlags src)
    {
        // All visual for now.
        return src != ModFlags.None;
    }
}