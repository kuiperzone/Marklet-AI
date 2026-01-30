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

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Interface for objects possesing a <see cref="Tracker"/> property.
/// </summary>
public interface ICrossTrackOwner
{
    /// <summary>
    /// Gets (or sets) the logical parent selection tracker.
    /// </summary>
    /// <remarks>
    /// The assignment of <see cref="Tracker"/> defines the insertion order into <see cref="CrossTracker"/>. The only
    /// way to change the order is to assign null, and then re-assign the tracker instance as this will move the
    /// instance to the end.
    /// </remarks>
    CrossTracker? Tracker { get; }

}