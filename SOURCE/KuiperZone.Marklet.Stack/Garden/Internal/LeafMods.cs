// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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
/// Provides change information for <see cref="GardenLeaf"/>.
/// </summary>
[Flags]
internal enum LeafMods
{
    /// <summary>
    /// No change.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// The <see cref="GardenLeaf.Assistant"/> has changed.
    /// </summary>
    Assistant = 0x0001,

    /// <summary>
    /// The <see cref="GardenLeaf.Content"/> has changed.
    /// </summary>
    Content = 0x0002,
}

/// <summary>
/// Extension methods.
/// </summary>
internal static partial class HelperExt
{
    /// <summary>
    /// Returns whether value contains a "visual" flag.
    /// </summary>
    public static bool IsVisual(this LeafMods src)
    {
        // All visual for now.
        return src != LeafMods.None;
    }

}