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
/// Flag values stored with <see cref="GardenDeck"/>.
/// </summary>
/// <remarks>
/// Existing values are written to storage and must not change. Currently a byte, but we could extend this to long in
/// the future without affecting database schema.
/// </remarks>
[Flags]
public enum DeckFlags : int // <- we could extend to long
{
    /// <summary>
    /// None default.
    /// </summary>
    None = 0x00000000,

    /// <summary>
    /// Is pinned.
    /// </summary>
    Pinned = 0x00000001,

    /// <summary>
    /// The deck was created as a branch.
    /// </summary>
    Branch = 0x00000002,
}
