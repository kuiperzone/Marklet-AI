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

using System.Data.Common;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Garden storage provider.
/// </summary>
/// <remarks>
/// Just a stupid name for a <see cref="DbConnection"/> factory. Factory? Gardens are nicer than factories.
/// </remarks>
public interface IMemoryGardener
{
    /// <summary>
    /// Gets location string.
    /// </summary>
    /// <remarks>
    /// Intended to be shown to the user. It is not necessarily the full connection string.
    /// </remarks>
    string? Location { get; }

    /// <summary>
    /// Gets whether the connection is readonly.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Returns a new (or pooled) connection.
    /// </summary>
    DbConnection Connect();
}
