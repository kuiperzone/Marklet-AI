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

using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Helper class which calls <see cref="GardenDeck.Close"/> on disposal if instance was not already open.
/// </summary>
public sealed class DeckTransientOpener : IDisposable
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public DeckTransientOpener(GardenDeck inst)
    {
        Closable = !inst.IsOpen;
        Instance = inst;
        inst.Open();

        Diag.ThrowIfFalse(inst.IsOpen);
    }

    /// <summary>
    /// Gets whether to close on dispose.
    /// </summary>
    public bool Closable { get; }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    public GardenDeck Instance { get; }

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        if (Closable)
        {
            Instance.Close();
        }
    }
}