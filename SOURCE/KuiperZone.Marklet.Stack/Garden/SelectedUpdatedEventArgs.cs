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
/// Event arguments for <see cref="MemoryGarden.SelectedUpdated"/>.
/// </summary>
public sealed class SelectedUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public SelectedUpdatedEventArgs(GardenSession selected)
    {
        Selected = selected;
    }

    /// <summary>
    /// Gets the selected instance which has been updated.
    /// </summary>
    public GardenSession Selected { get; }
}
