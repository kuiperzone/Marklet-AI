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
/// Provides change information for <see cref="GardenDeck"/>.
/// </summary>
[Flags]
internal enum DeckMods
{
    /// <summary>
    /// No change.
    /// </summary>
    None = 0x00000000,

    /// <summary>
    /// The <see cref="GardenDeck.Basket"/> has changed.
    /// </summary>
    Basket = 0x00000002,

    /// <summary>
    /// The <see cref="GardenDeck.Updated"/> has changed.
    /// </summary>
    Updated = 0x00000001,

    /// <summary>
    /// The <see cref="GardenDeck.Title"/> has changed.
    /// </summary>
    Title = 0x00000004,

    /// <summary>
    /// The <see cref="GardenDeck.Model"/> has changed.
    /// </summary>
    Model = 0x00000008,

    /// <summary>
    /// The <see cref="GardenDeck.Folder"/> has changed.
    /// </summary>
    Folder = 0x00000010,

    /// <summary>
    /// The <see cref="GardenDeck.IsPinned"/> has changed.
    /// </summary>
    Pinned = 0x00000020,

    /// <summary>
    /// A <see cref="GardenLeaf"/> child has changed.
    /// </summary>
    /// <remarks>
    /// This typically implies <see cref="GardenLeaf.Content"/> has changed but may include changes to other <see
    /// cref="GardenLeaf"/> properties.
    /// </remarks>
    Leaf = 0x10000000,
}

/// <summary>
/// Extension methods.
/// </summary>
internal static partial class HelperExt
{
    /// <summary>
    /// Returns whether value contains a "visual" flag.
    /// </summary>
    public static bool IsVisual(this DeckMods src)
    {
        // All visual for now.
        return src != DeckMods.None;
    }
}