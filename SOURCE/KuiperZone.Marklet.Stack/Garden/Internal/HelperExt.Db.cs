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
using System.Diagnostics.CodeAnalysis;

namespace KuiperZone.Marklet.Stack.Garden.Internal;

/// <summary>
/// Extension methods.
/// </summary>
internal static partial class HelperExt
{
    /// <summary>
    /// Returns field value of given "name".
    /// </summary>
    public static bool GetBooleanOrThrow(this DbDataReader src, string name)
    {
        // Throws if value is NULL (not expected)
        return src.GetBoolean(src.GetOrdinal(name));
    }

    /// <summary>
    /// Returns field value of given "name" or a default value if the column does not exist or is null.
    /// </summary>
    public static bool GetBooleanOrDefault(this DbDataReader src, string name, bool def = false)
    {
        try
        {
            int ordinal = src.GetOrdinal(name);
            return src.IsDBNull(ordinal) ? def : src.GetBoolean(ordinal);
        }
        catch (IndexOutOfRangeException)
        {
            return def;
        }
    }

    /// <summary>
    /// Returns field value of given "name".
    /// </summary>
    public static int GetInt32OrThrow(this DbDataReader src, string name)
    {
        // Could do?
        // int ordinal = reader.GetOrdinal(columnName);
        // return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);

        // Throws if value is NULL (not expected)
        return src.GetInt32(src.GetOrdinal(name));
    }

    /// <summary>
    /// Returns field value of given "name" or a default value if the column does not exist or is null.
    /// </summary>
    public static int GetInt32OrDefault(this DbDataReader src, string name, int def = 0)
    {
        try
        {
            int ordinal = src.GetOrdinal(name);
            return src.IsDBNull(ordinal) ? def : src.GetInt32(ordinal);
        }
        catch (IndexOutOfRangeException)
        {
            return def;
        }
    }

    /// <summary>
    /// Returns field value of given "name".
    /// </summary>
    public static long GetInt64OrThrow(this DbDataReader src, string name)
    {
        // Throws if value is NULL (not expected)
        return src.GetInt64(src.GetOrdinal(name));
    }

    /// <summary>
    /// Returns field value of given "name" or a default value if the column does not exist or is null.
    /// </summary>
    public static long GetInt64OrDefault(this DbDataReader src, string name, long def = 0)
    {
        try
        {
            int ordinal = src.GetOrdinal(name);
            return src.IsDBNull(ordinal) ? def : src.GetInt64(ordinal);
        }
        catch (IndexOutOfRangeException)
        {
            return def;
        }
    }

    /// <summary>
    /// Returns field value of given "name" or a default value if the column does not exist or is null.
    /// </summary>
    [return: NotNullIfNotNull(nameof(def))]
    public static string? GetStringOrDefault(this DbDataReader src, string name, string? def = null)
    {
        try
        {
            var ord = src.GetOrdinal(name);
            return src.IsDBNull(ord) ? def : src.GetString(ord);
        }
        catch (IndexOutOfRangeException)
        {
            return def;
        }
    }

    /// <summary>
    /// Adds parameter value to <see cref="DbCommand"/> and returns the command.
    /// </summary>
    public static DbCommand AddValue(this DbCommand src, string name, object? value)
    {
        var p = src.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        src.Parameters.Add(p);
        return src;
    }

}