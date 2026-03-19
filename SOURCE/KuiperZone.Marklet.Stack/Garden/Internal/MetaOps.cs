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
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden.Internal;

/// <summary>
/// Database operations.
/// </summary>
/// <remarks>
/// Stores a single <see cref="VersionField"/> value in the "primary row" using empty <see cref="PKeyField"/> name.
/// </remarks>
internal static class MetaOps
{
    /// <summary>
    /// Table schema version with first version starting at 1.
    /// </summary>
    public const int SchemaVersion = 1;

    /// <summary>
    /// Maximum version this implementation will use.
    /// </summary>
    /// <remarks>
    /// We can elect to jump table version to above this value if we decide old software should NOT read newer tables.
    /// </remarks>
    public const int MaxSchema = 99;

    /// <summary>
    /// Name of meta table. Do not change.
    /// </summary>
    public const string TableName = "meta_info";

    /// <summary>
    /// Primary row name. Do not change.
    /// </summary>
    /// <remarks>
    /// Row containing schema version has empty field name.
    /// </remarks>
    public const string PKeyField = "pkey";

    /// <summary>
    /// Schema version. Do not change.
    /// </summary>
    /// <remarks>
    /// Valid for primary row only with empty <see cref="PKeyField"/>
    /// </remarks>
    public const string VersionField = "version";

    private const string CreateSql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{PKeyField}  VARCHAR(255) NOT NULL PRIMARY KEY,
{VersionField}  INTEGER NOT NULL DEFAULT 0
);";

    /// <summary>
    /// Reads schema version. Returns 0 if not exist.
    /// </summary>
    public static int ReadSchema(DbConnection con)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(ReadSchema)}";
        ConditionalDebug.WriteLine(NSpace, $"Read VERSION from: {TableName}");

        const string Sql0 = $"SELECT name FROM sqlite_master WHERE type='table' AND name=@name LIMIT 1";
        ConditionalDebug.WriteLine(NSpace, Sql0);

        using var cmd0 = con.CreateCommand();
        cmd0.CommandText = Sql0;
        cmd0.AddValue($"@name", TableName);

        if (cmd0.ExecuteScalar() == null)
        {
            ConditionalDebug.WriteLine(NSpace, $"{TableName} not exist");
            return 0;
        }

        const string Sql1 = $"SELECT {VersionField} FROM {TableName} WHERE {PKeyField} = @{PKeyField};";
        ConditionalDebug.WriteLine(NSpace, Sql1);

        using var cmd1 = con.CreateCommand();
        cmd1.CommandText = Sql1;
        cmd1.AddValue($"@{PKeyField}", ""); // <- empty field

        using var reader = cmd1.ExecuteReader();

        if (reader.Read())
        {
            int version = reader.GetInt32(VersionField);
            ConditionalDebug.WriteLine(NSpace, $"VERSION: {version}");
            return version;
        }

        // Not found
        ConditionalDebug.WriteLine(NSpace, "Table row not found");
        return 0;
    }

    /// <summary>
    /// Ensure tables are created and/or upgraded.
    /// </summary>
    public static void Init(IMemoryGardener gardener)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Init)}";
        ConditionalDebug.WriteLine(NSpace, "INITIALIZE");

        using var con = gardener.Connect();
        var schema = ReadSchema(con);

        if (gardener.IsReadOnly)
        {
            if (schema == 0)
            {
                throw new InvalidOperationException("Cannot initialize read-only data. Read-write access required.");
            }

            if (schema < SchemaVersion)
            {
                throw new InvalidOperationException($"Cannot upgrade read-only data.\n\nData schema version {schema} requires upgrade to version {SchemaVersion}, but data is read-only. Read-write access required.");
            }
        }

        if (schema > MaxSchema)
        {
            throw new InvalidOperationException($"Data schema is too new to be read by this version of the software.\n\nApplication schema is {SchemaVersion} whereas data is version {schema}. Application update required.");
        }

        if (schema < 0)
        {
            // Not expected but no harm in checking
            throw new InvalidOperationException("Invalid version");
        }

        using var tran = con.BeginTransaction();

        try
        {
            // CREATE TABLES
            EnsureTable(con, tran);
            DeckOps.EnsureTable(con, tran);
            LeafOps.EnsureTable(con, tran);

            if (schema == 0)
            {
                ConditionalDebug.WriteLine(NSpace, "Write version");

                InsertSchemaVersion(con, tran, SchemaVersion);
                ConditionalDebug.ThrowIfNotEqual(1, Count(con));
            }
            else
            if (schema < SchemaVersion)
            {
                ConditionalDebug.WriteLine(NSpace, "BEGIN UPGRADE");

                UpdateSchemaVersion(con, tran, SchemaVersion);
                DeckOps.UpgradeTable(con, tran, schema);
                LeafOps.UpgradeTable(con, tran, schema);
            }

            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Empty data table.
    /// </summary>
    public static void Purge(DbConnection con)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(Purge)}";

        // Only deck table required as leaves cascade
        const string Sql = $"DELETE FROM {DeckOps.TableName};";

        ConditionalDebug.WriteLine(NSpace, Sql);
        using var tran = con.BeginTransaction();

        try
        {
            using var cmd0 = con.CreateCommand();
            cmd0.CommandText = Sql;
            cmd0.Transaction = tran;

            cmd0.ExecuteNonQuery();
            ConditionalDebug.ThrowIfNotEqual(0, DeckOps.Count(con));
            ConditionalDebug.ThrowIfNotEqual(0, LeafOps.Count(con));

            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Gets number of rows.
    /// </summary>
    public static int Count(DbConnection con)
    {
        const string Sql = $"SELECT COUNT(*) FROM {TableName};";

        var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static void EnsureTable(DbConnection con, DbTransaction? tran)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(EnsureTable)}";
        ConditionalDebug.WriteLine(NSpace, CreateSql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = CreateSql;
        cmd.Transaction = tran;

        // Always return 0
        cmd.ExecuteNonQuery();
    }

    private static void InsertSchemaVersion(DbConnection con, DbTransaction tran, int version)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(InsertSchemaVersion)}";
        ConditionalDebug.WriteLine(NSpace, $"INSERT SCHEMA: {version}");

        const string Sql = $@"INSERT INTO {TableName}
({PKeyField}, {VersionField}) VALUES
(@{PKeyField}, @{VersionField});";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;
        cmd.AddValue($"@{PKeyField}", ""); // <- primary row
        cmd.AddValue($"@{VersionField}", version);

        int result = cmd.ExecuteNonQuery();
        ConditionalDebug.ThrowIfNotEqual(1, result);
    }

    private static void UpdateSchemaVersion(DbConnection con, DbTransaction tran, int version)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(UpdateSchemaVersion)}";
        ConditionalDebug.WriteLine(NSpace, $"UPDATE SCHEMA: {version}");

        const string Sql = $@"UPDATE {TableName} SET {VersionField} = @{VersionField}
WHERE {PKeyField} = @{PKeyField};";

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;
        cmd.AddValue($"@{PKeyField}", ""); // <- primary row
        cmd.AddValue($"@{VersionField}", version);

        int result = cmd.ExecuteNonQuery();
        ConditionalDebug.ThrowIfNotEqual(1, result);
    }

}
