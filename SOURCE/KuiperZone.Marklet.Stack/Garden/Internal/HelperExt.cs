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

using System.Data.Common;

namespace KuiperZone.Marklet.Stack.Garden.Internal;

/// <summary>
/// Extension methods.
/// </summary>
internal static partial class HelperExt
{
    /// <summary>
    /// Returns field value of given "name".
    /// </summary>
    public static bool GetBoolean(this DbDataReader src, string name)
    {
        return src.GetBoolean(src.GetOrdinal(name));
    }

    /// <summary>
    /// Returns field value of given "name".
    /// </summary>
    public static int GetInt32(this DbDataReader src, string name)
    {
        return src.GetInt32(src.GetOrdinal(name));
    }

    /// <summary>
    /// Returns field value of given "name".
    /// </summary>
    public static long GetInt64(this DbDataReader src, string name)
    {
        return src.GetInt64(src.GetOrdinal(name));
    }

    /// <summary>
    /// Returns field string value given "name", or null.
    /// </summary>
    public static string? GetStringOrNull(this DbDataReader src, string name)
    {
        var ord = src.GetOrdinal(name);
        return src.IsDBNull(ord) ? null : src.GetString(ord);
    }

    /// <summary>
    /// Returns field value given "name".
    /// </summary>
    public static string GetStringOrEmpty(this DbDataReader src, string name)
    {
        var ord = src.GetOrdinal(name);
        return src.IsDBNull(ord) ? "" : src.GetString(ord);
    }

    /// <summary>
    /// Adds parameter value to <see cref="DbCommand"/> and returns the command.
    /// </summary>
    public static DbCommand AddParameter(this DbCommand src, string name, object? value)
    {
        var p = src.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        src.Parameters.Add(p);
        return src;
    }

}