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

using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Provides common interface for item accepting drag-drop of <see cref="GardenDeck"/> items.
/// </summary>
internal interface IDeckDrop
{
    /// <summary>
    /// Returns whether target can accept the given instance.
    /// </summary>
    bool CanDrop(object obj);

    /// <summary>
    /// Resets the visual state after a positive call to <see cref="StartDrop"/>.
    /// </summary>
    void CancelDrop();

    /// <summary>
    /// Updates visual state for a possible drag-drop.
    /// </summary>
    /// <remarks>
    /// The <see cref="AcceptDrop"/> or <see cref="CancelDrop"/> should be called after this.
    /// </remarks>
    void StartDrop(object obj);

    /// <summary>
    /// Accepts or rejects the <see cref="GardenDeck"/> instance.
    /// </summary>
    /// <remarks>
    /// There is no need to call <see cref="CancelDrop"/> if after this call.
    /// </remarks>
    bool AcceptDrop(object obj);


}