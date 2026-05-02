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
/// Provides information when opening <see cref="MemoryGarden"/>.
/// </summary>
public enum GardenStatus
{
    /// <summary>
    /// Service not connected.
    /// </summary>
    /// <remarks>
    /// In this mode, all content is not persistant.
    /// </remarks>
    Disconnected = 0,

    /// <summary>
    /// Open OK.
    /// </summary>
    /// <remarks>
    /// Open database with matching schema version. This is nominal behaviour.
    /// </remarks>
    OpenOk,

    /// <summary>
    /// Database created and open.
    /// </summary>
    /// <remarks>
    /// This is nominal behaviour.
    /// </remarks>
    CreatedOk,

    /// <summary>
    /// Database schema upgraded and open.
    /// </summary>
    /// <remarks>
    /// This is nominal behaviour.
    /// </remarks>
    UpgradedOk,

    /// <summary>
    /// Database open as readonly.
    /// </summary>
    /// <remarks>
    /// This is nominal behaviour only if the source is intentionally readonly.
    /// </remarks>
    Readonly,

    /// <summary>
    /// Database requires creation or upgrade, but source is read-only.
    /// </summary>
    /// <remarks>
    /// Database can't be opened. Functionally equivalent to <see cref="Disconnected"/>.
    /// </remarks>
    WriteRequired,

    /// <summary>
    /// Database schema version not supported.
    /// </summary>
    /// <remarks>
    /// Database can't be opened. Schema version is tool old or too new. Functionally equivalent to <see cref="Disconnected"/>.
    /// </remarks>
    NotSupported,

    /// <summary>
    /// Unknown error on opening, not found or not a SQL database.
    /// </summary>
    /// <remarks>
    /// Database can't be opened. Functionally equivalent to <see cref="Disconnected"/>.
    /// </remarks>
    OpenError,

    /// <summary>
    /// Connection loss.
    /// </summary>
    Lost,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns whether the status is nominal read/write.
    /// </summary>
    /// <remarks>
    /// It returns false for <see cref="GardenStatus.Readonly"/>.
    /// </remarks>
    public static bool IsOpen(this GardenStatus src)
    {
        return src == GardenStatus.OpenOk || src == GardenStatus.CreatedOk || src == GardenStatus.UpgradedOk || src == GardenStatus.Readonly;
    }

    /// <summary>
    /// Returns a short message string.
    /// </summary>
    public static string ToMessage(this GardenStatus src)
    {
        switch (src)
        {
            case GardenStatus.Disconnected:
                return "Disconnected";
            case GardenStatus.OpenOk:
                return "Open OK";
            case GardenStatus.CreatedOk:
                return "Created OK";
            case GardenStatus.UpgradedOk:
                return "Upgraded OK";
            case GardenStatus.Readonly:
                return "Read-only";
            case GardenStatus.WriteRequired:
                return "Write access required";
            case GardenStatus.NotSupported:
                return "Not supported";
            case GardenStatus.Lost:
                return "Connection lost";
            default:
                return "Unknown error or not a SQL database";
        }
    }
}