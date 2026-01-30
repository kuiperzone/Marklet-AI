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
/// Event arguments for <see cref="MemoryGarden.SelectedChanged"/>.
/// </summary>
public sealed class SelectedChangedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public SelectedChangedEventArgs(GardenSession? selected, GardenSession? previous)
    {
        Selected = selected;
        Previous = previous;
    }

    /// <summary>
    /// Gets the newly selected instance which may be null (none).
    /// </summary>
    public GardenSession? Selected { get; }

    /// <summary>
    /// Gets the previously selected instance which may be null (none), or an instance that has been deleted and should
    /// be discarded.
    /// </summary>
    public GardenSession? Previous { get; }
}
