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
using Microsoft.Data.Sqlite;

namespace KuiperZone.Marklet.Stack.Garden.Internal;

/// <summary>
/// Database operations.
/// </summary>
/// <remarks>
/// Stores a single <see cref="SchemaVersionField"/> value in the "primary row" using empty <see cref="IdField"/> name.
/// </remarks>
internal static class MetaOps
{
    /// <summary>
    /// Implementation schema version with first version starting at 1.
    /// </summary>
    /// <remarks>
    /// Version 1 is pre-release development version. Version 2 is intended as first production release. A value of 0
    /// implies schema does not exist.
    /// </remarks>
    public const int SchemaVersion = 1;

    /// <summary>
    /// Minimum version this implementation will read in readonly mode without upgrading to <see cref="SchemaVersion"/>.
    /// </summary>
    public const int MinReadSchema = 1;

    /// <summary>
    /// Minimum version this implementation will upgrade to <see cref="SchemaVersion"/>.
    /// </summary>
    /// <remarks>
    /// This should be lower or equal to <see cref="MinReadSchema"/>. If this is increased, we break backward
    /// compatibility.
    /// </remarks>
    public const int MinUpgradeSchema = 1;

    /// <summary>
    /// Maximum version above <see cref="SchemaVersion"/> this implementation will open.
    /// </summary>
    /// <remarks>
    /// If a database gives a version higher than this, we will not attempt to open it. We can, therefore,
    /// elect to jump table version to above this value if we decide old software can NOT read a newer schema.
    /// </remarks>
    public const int MaxSchema = 99;

    /// <summary>
    /// Name of meta table. Do not change.
    /// </summary>
    public const string TableName = $"meta_info";

    /// <summary>
    /// Primary key. Do not change.
    /// </summary>
    /// <remarks>
    /// A row with id of 0 must exist and contains <see cref="SchemaVersionField"/> value. Do not change.
    /// </remarks>
    public const string IdField = "id";

    /// <summary>
    /// Schema version. Do not change.
    /// </summary>
    /// <remarks>
    /// Valid for primary row. Do not change.
    /// </remarks>
    public const string SchemaVersionField = "schema_version";

    /// <summary>
    /// Name. Do not change.
    /// </summary>
    /// <remarks>
    /// For primary row only. This may contain a "database name". Do not change.
    /// </remarks>
    public const string NameField = "name";

    /// <summary>
    /// Json config. Do not change.
    /// </summary>
    /// <remarks>
    /// For primary row only. This may contain json config. Do not change.
    /// </remarks>
    public const string ConfigField = "config";

#if DEBUG
    public static readonly bool IsDebug = true;
#else
    public static readonly bool IsDebug = false;
#endif

    // Make explicit
    private const int PrimaryRowId = 0;

    // We are not intending to upgrade this in production. It is fixed.
    private const string CreateSql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}               INTEGER NOT NULL PRIMARY KEY,
{SchemaVersionField}    INTEGER NOT NULL DEFAULT 0,
{NameField}             VARCHAR(255),
{ConfigField}           TEXT
);";

    /// <summary>
    /// Reads schema version and open init action.
    /// </summary>
    public static GardenStatus TryOpen(IServiceProvider provider, out int version)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(TryOpen)}";
        Diag.WriteLine(NSpace, "Source: " + provider.Source);
        Diag.WriteLine(NSpace, "Readonly: " + provider.IsReadOnly);

        version = 0;

        try
        {
            using var con = provider.Connect();

            version = ReadSchemaVersion(con);
            var init = ToStatus(version, provider.IsReadOnly);
            Diag.WriteLine(NSpace, "SCHEMA VERSION: " + version);
            return init;
        }
        catch (SqliteException e)
        {
            Diag.WriteLine(NSpace, e);

            if (e.SqliteErrorCode == 6 || e.SqliteErrorCode == 8 || e.SqliteErrorCode == 14)
            {
                // SQLITE_LOCKED (6)
                // SQLITE_READONLY (8)
                // SQLITE_CANTOPEN (14)
                // Otherwise fall-thru to try read-only
                return GardenStatus.WriteRequired;
            }

            return GardenStatus.OpenError;
        }
        catch (Exception e)
        {
            Diag.WriteLine(NSpace, e);

            if (provider.IsReadOnly)
            {
                return GardenStatus.WriteRequired;
            }

            return GardenStatus.OpenError;
        }
    }

    /// <summary>
    /// Ensure tables are created and/or upgraded, returning a connection on success.
    /// </summary>
    /// <remarks>
    /// The call does not throw, but returns null and sets "status" on failure.
    /// </remarks>
    public static DbConnection? Init(IServiceProvider provider, out int version, out GardenStatus status)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(Init)}";
        Diag.WriteLine(NSpace, "INITIALIZE");
        Diag.WriteLine(NSpace, "Source: " + provider.Source);
        Diag.WriteLine(NSpace, "Flags: " + provider.Flags + ", Ro: " + provider.IsReadOnly);

        // Sanity checks on constants intended to prevent mistakes in future changes.
        Diag.ThrowIfGreaterThan(MinReadSchema, SchemaVersion);
        Diag.ThrowIfGreaterThan(MinUpgradeSchema, SchemaVersion);
        Diag.ThrowIfGreaterThan(MinUpgradeSchema, MinReadSchema);
        Diag.ThrowIfGreaterThan(SchemaVersion, MaxSchema);

        version = 0;

        DbConnection? con = null;
        DbTransaction? tran = null;

        try
        {
            con = provider.Connect();
            version = ReadSchemaVersion(con);
            Diag.WriteLine(NSpace, $"VERSION = {version}");

            status = ToStatus(version, provider.IsReadOnly);
            Diag.WriteLine(NSpace, $"{nameof(GardenStatus)} = {status}");


            if (status == GardenStatus.OpenOk)
            {
                Diag.ThrowIfLessThan(version, MinReadSchema);

                if (TableExists(con, DeckOps.TableName) && TableExists(con, LeafOps.TableName))
                {
                    return con;
                }

                status = GardenStatus.OpenError;
                return null;
            }

            if (status == GardenStatus.CreatedOk || status == GardenStatus.UpgradedOk)
            {
                Diag.ThrowIfTrue(provider.IsReadOnly);
                tran = con.BeginTransaction();

                // ENSURE META TABLE
                UpsertSchema(con, tran, version);

                if (status == GardenStatus.CreatedOk && provider.Flags.HasFlag(ProviderFlags.SchemaInit1))
                {
                    Diag.WriteLine(NSpace, "INIT TEST SCHEMA 1");
                    Diag.ThrowIfNotEqual(0, version);

                    DeckOps.CreateSchema1(con, tran);
                    LeafOps.CreateSchema1(con, tran);

                    // Now walk the upgrade process
                    version = 1;
                }

                DeckOps.CreateOrAlter(con, tran, version);
                LeafOps.CreateOrAlter(con, tran, version);

                tran.Commit();
                Diag.WriteLine(NSpace, "COMMIT OK");

                if (status == GardenStatus.CreatedOk && provider.Flags.HasFlag(ProviderFlags.WalNormal))
                {
                    // SQLite specific
                    // Persistent setting. We set only on first create.
                    // Changing it on large database may involve lengthy blocking.
                    ExecuteNonQuery(con, null, "PRAGMA journal_mode=WAL;", false);
                }
            }

            Diag.ThrowIfEqual(0, CountRows(con));
            Diag.ThrowIfNotEqual(SchemaVersion, ReadSchemaVersion(con));
            Diag.ThrowIfFalse(TableExists(con, DeckOps.TableName));
            Diag.ThrowIfFalse(TableExists(con, LeafOps.TableName));
            return con;
        }
        catch (Exception e)
        {
            Diag.WriteLine(NSpace, e);
            tran?.Rollback();
            status = provider.IsReadOnly ? GardenStatus.WriteRequired : GardenStatus.OpenError;
            return null;
        }
        finally
        {
            tran?.Dispose();
        }
    }

    /// <summary>
    /// Drops ALL tables and calls <see cref="Init"/> to rebuild.
    /// </summary>
    public static GardenStatus Purge(IServiceProvider provider)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(Purge)}";
        Diag.WriteLine(NSpace, $"Purge " + provider.Connection);

        if (provider.IsReadOnly)
        {
            return GardenStatus.Readonly;
        }

        DbConnection? con = null;
        DbTransaction? tran = null;

        try
        {
            // We can alternatively do this, where FOREIGN keys ensure leaves are emptied.
            // But this not accommodate newer tables, and does not rebuild meta table.

            // ExecuteNonQuery(con, tran, $"DELETE FROM {DeckOps.TableName};");

            // But above will only drop known tables. Below drops
            // everything and then rebuilds, even if unknown schema.
            con = provider.Connect();
            ExecuteNonQuery(con, null, "PRAGMA foreign_keys = OFF;", IsDebug);

            tran = con.BeginTransaction();

            foreach (var item in GetTableNames(con))
            {
                ExecuteNonQuery(con, tran, $"DROP TABLE IF EXISTS {item};", true);
            }

            tran.Commit();
        }
        catch
        {
            tran?.Rollback();
            return GardenStatus.Lost;
        }
        finally
        {
            if (con != null)
            {
                ExecuteNonQuery(con, null, "PRAGMA foreign_keys = ON;", IsDebug);
            }

            tran?.Dispose();
            con?.Dispose();
        }

        // Rebuild
        Init(provider, out _, out GardenStatus status)?.Dispose();
        return status;
    }

    /// <summary>
    /// Returns whether the table exists.
    /// </summary>
    /// <exception cref="ArgumentException">Empty name</exception>
    public static bool TableExists(DbConnection con, string tableName)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(TableExists)}";

        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Empty name", nameof(tableName));
        }

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name LIMIT 1";
        Diag.WriteLine(NSpace, cmd.CommandText);

        cmd.AddValue($"@name", tableName);

        if (cmd.ExecuteScalar() == null)
        {
            Diag.WriteLine(NSpace, $"Table {tableName} NOT exist");
            return false;
        }

        Diag.WriteLine(NSpace, $"Table {tableName} exist OK");
        return true;
    }

    /// <summary>
    /// Gets number of rows.
    /// </summary>
    public static int CountRows(DbConnection con)
    {
        const string Sql = $"SELECT COUNT(*) FROM {TableName};";

        var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static int ReadSchemaVersion(DbConnection con)
    {
        using var cmd = GetPrimaryReader(con, SchemaVersionField);
        using var reader = cmd?.ExecuteReader();

        if (reader?.Read() == true)
        {
            return reader.GetInt32OrThrow(SchemaVersionField);
        }

        return 0;
    }

    public static string? GetName(DbConnection con)
    {
        using var cmd = GetPrimaryReader(con, NameField);
        using var reader = cmd?.ExecuteReader();

        if (reader?.Read() == true)
        {
            return reader.GetStringOrDefault(NameField);
        }

        return null;
    }

    public static void UpdateName(DbConnection con, string? value)
    {
        UpdateField(con, null, NameField, value);
    }

    public static string? GetConfig(DbConnection con)
    {
        using var cmd = GetPrimaryReader(con, ConfigField);
        using var reader = cmd?.ExecuteReader();

        if (reader?.Read() == true)
        {
            return reader.GetStringOrDefault(ConfigField);
        }

        return null;
    }

    public static void UpdateConfig(DbConnection con, string? value)
    {
        UpdateField(con, null, ConfigField, value);
    }

    private static void UpsertSchema(DbConnection con, DbTransaction tran, int currentSchema)
    {
        Diag.ThrowIfNegative(currentSchema);

        if (currentSchema == 0)
        {
            ExecuteNonQuery(con, tran, CreateSql, true);
            InsertSchemaVersion(con, tran, SchemaVersion);
        }
        else
        if (currentSchema < SchemaVersion)
        {
            UpdateField(con, tran, SchemaVersionField, SchemaVersion);
        }

        Diag.ThrowIfFalse(TableExists(con, TableName));
    }

    private static GardenStatus ToStatus(int schema, bool readOnly)
    {
        if (schema < 0)
        {
            return GardenStatus.OpenError;
        }

        if (readOnly)
        {
            if (schema == 0)
            {
                // We must create the database
                return GardenStatus.WriteRequired;
            }

            if (schema < MinUpgradeSchema || schema > MaxSchema)
            {
                return GardenStatus.NotSupported;
            }

            if (schema < MinReadSchema)
            {
                // We must upgrade to read
                return GardenStatus.WriteRequired;
            }

            // We think we can open this as readonly
            return GardenStatus.Readonly;
        }

        if (schema == 0)
        {
            // We need to create
            return GardenStatus.CreatedOk;
        }

        if (schema < MinUpgradeSchema || schema > MaxSchema)
        {
            // Cannot support
            return GardenStatus.NotSupported;
        }

        if (schema < SchemaVersion)
        {
            // We need to upgrade
            return GardenStatus.UpgradedOk;
        }

        // We think we can open this
        return GardenStatus.OpenOk;
    }

    private static DbCommand? GetPrimaryReader(DbConnection con, string field)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(GetPrimaryReader)}";
        Diag.WriteLine(NSpace, $"Read from: {TableName}");

        if (TableExists(con, TableName))
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT {field} FROM {TableName} WHERE {IdField} = @{IdField};";

            cmd.AddValue($"@{IdField}", PrimaryRowId);
            Diag.WriteLine(NSpace, cmd.CommandText);
            return cmd;
        }

        Diag.WriteLine(NSpace, $"Table not found");
        return null;
    }

    private static void InsertSchemaVersion(DbConnection con, DbTransaction tran, int version)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(InsertSchemaVersion)}";

        Diag.WriteLine(NSpace, $"INSERT SCHEMA: {version}");
        const string Sql = $@"INSERT INTO {TableName} ({IdField}, {SchemaVersionField}) VALUES (@{IdField}, @{SchemaVersionField});";

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;
        Diag.WriteLine(NSpace, cmd.CommandText);

        cmd.AddValue($"@{IdField}", PrimaryRowId);
        cmd.AddValue($"@{SchemaVersionField}", version);

        int result = cmd.ExecuteNonQuery();
        Diag.ThrowIfNotEqual(1, result);
    }

    private static void UpdateField(DbConnection con, DbTransaction? tran, string fieldName, object? fieldValue)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(UpdateField)}";
        Diag.WriteLine(NSpace, $"UPDATE FIELD: {fieldName} = {fieldValue}");

        using var cmd = con.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = $@"UPDATE {TableName} SET {fieldName} = @{fieldName} WHERE {IdField} = @{IdField};";

        cmd.AddValue($"@{IdField}", PrimaryRowId);
        cmd.AddValue(fieldName, fieldValue);
        Diag.WriteLine(NSpace, cmd.CommandText);

        int result = cmd.ExecuteNonQuery();
        Diag.ThrowIfNotEqual(1, result);
    }

    private static List<string> GetTableNames(DbConnection con)
    {
        var tables = new List<string>();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private static void ExecuteNonQuery(DbConnection con, DbTransaction? tran, string? sql, bool throwOnFail)
    {
        const string NSpace = $"{nameof(MetaOps)}.{nameof(ExecuteNonQuery)}";

        if (!string.IsNullOrEmpty(sql))
        {
            try
            {
                using var cmd = con.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = sql;

                Diag.WriteLine(NSpace, cmd.CommandText);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);

                if (throwOnFail)
                {
                    throw;
                }
            }
        }
    }
}
