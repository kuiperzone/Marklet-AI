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
/// Options flags used with <see cref="IServiceProvider"/> construction
/// </summary>
[Flags]
public enum ProviderFlags
{
    /// <summary>
    /// None default.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Force readonly access. The <see cref="SchemaInit1"/> and <see cref="WalNormal"/> are ignored.
    /// </summary>
    ReadOnly = 0x01,

    /// <summary>
    /// Set WAL (write-ahead) mode when initializing a new database and NORMAL synchronous mode per connection.
    /// </summary>
    /// <remarks>
    /// WAL applies only when initializing a new database. This should not be used against pre-existing non-WAL
    /// databases as there may be data loss in case of interruption.
    /// </remarks>
    WalNormal = 0x02,

    /// <summary>
    /// When creating new database, it forces initial schema version 1 and performs subsequent upgrade as a test.
    /// </summary>
    /// <remarks>
    /// This is for test only. It should not be used in production. It applies only when initializing a new database.
    /// </remarks>
    SchemaInit1 = 0x04,

    /// <summary>
    /// Use memory database.
    /// </summary>
    /// <remarks>
    /// This is for test only. This overrides and ignores <see cref="ReadOnly"/>. Any "path" or connection is also
    /// ignored.
    /// </remarks>
    Memory = 0x08,
}
