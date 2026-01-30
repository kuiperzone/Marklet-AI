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
/// Result of <see cref="MemoryGarden.Open"/>.
/// </summary>
public enum OpenInit
{
    /// <summary>
    /// Database opened normally.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Indicates the database was created for the first time.
    /// </summary>
    Created,

    /// <summary>
    /// Database existed but has been upgraded to a later version.
    /// </summary>
    SchemaUpgraded,
}
