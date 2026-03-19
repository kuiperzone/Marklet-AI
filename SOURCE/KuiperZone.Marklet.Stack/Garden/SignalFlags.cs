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
/// Visual signal flags carried by <see cref="GardenDeck"/>.
/// </summary>
[Flags]
public enum SignalFlags : byte
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Show attention on the item.
    /// </summary>
    ItemAttention = 0x01,

    /// <summary>
    /// Show attention on the item's folder (ignored on root).
    /// </summary>
    FolderAttention = 0x02,

    /// <summary>
    /// Ensure that the deck's folder is open (ignored on root).
    /// </summary>
    OpenFolder = 0x04,
}
