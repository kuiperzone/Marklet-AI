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

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Identifies the <see cref="GardenBasket"/> instance.
/// </summary>
/// <remarks>
/// Integer values are written to storage and must not change.
/// </remarks>
public class PruneOptions
{
    /// <summary>
    /// Gets or sets the retention period.
    /// </summary>
    public TimeSpan Period { get; set; } = TimeSpan.MaxValue;

    /// <summary>
    /// Gets or sets whether the item is deleted rather than being moved to <see cref="BasketKind.Waste"/>.
    /// </summary>
    /// <remarks>
    /// Items are always deleted when pruning the <see cref="BasketKind.Waste"/> basket itself.
    /// </remarks>
    public bool AlwaysDelete { get; set; }

    /// <summary>
    /// Gets or sets whether to remove expired pinned items.
    /// </summary>
    public bool RemovePinned { get; set; }

    /// <summary>
    /// Gets or sets whether to remove members in folders.
    /// </summary>
    /// <remarks>
    /// A folder is naturally removed when it becomes empty unless it has project information.
    /// </remarks>
    public bool RemoveFolderItems { get; set; }

    /// <summary>
    /// Gets or sets whether to remove empty folders with expired project information.
    /// </summary>
    public bool RemoveEmptyProjects { get; set; }

    /// <summary>
    /// Gets or sets whether to include the currently selected item.
    /// </summary>
    public bool IgnoreCurrent { get; set; }

    /// <summary>
    /// Returns true if the given object matches criteria for removal.
    /// </summary>
    public bool IsRemoveMatch(GardenDeck obj)
    {
        return DateTime.UtcNow - obj.Updated > Period &&
            (RemovePinned || !obj.IsPinned) &&
            (RemoveFolderItems || obj.Folder == null) && // TBD project info
            (!IgnoreCurrent || !obj.IsCurrent);
    }
}
