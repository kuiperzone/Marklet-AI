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
using System.Text;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden.Internal;

/// <summary>
/// Database operations for <see cref="GardenLeaf"/>.
/// </summary>
internal static class LeafOps
{
    /// <summary>
    /// Name of leaf table. Do not change.
    /// </summary>
    public const string TableName = "leaf";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string IdField = "id";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string OwnerField = "owner_id";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string FormatField = "format";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string AssistantField = "assistant";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string ContentField = "content";

    // VERSION 1
    private const string CreateSql1 = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}           BIGINT NOT NULL PRIMARY KEY,
{OwnerField}        BIGINT NOT NULL,
{FormatField}       INTEGER NOT NULL,
{AssistantField}    VARCHAR(255),
{ContentField}      TEXT,
CONSTRAINT  fk_owner
    FOREIGN KEY ({OwnerField})
    REFERENCES {DeckOps.TableName}({DeckOps.IdField})
    ON DELETE CASCADE
);";

    // VERSION 2
    // When this is changed, add to Upgrade()
    private const string CreateSqlLatest = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}           BIGINT NOT NULL PRIMARY KEY,
{OwnerField}        BIGINT NOT NULL,
{FormatField}       INTEGER NOT NULL,
{AssistantField}    VARCHAR(255),
{ContentField}      TEXT,
CONSTRAINT  fk_owner
    FOREIGN KEY ({OwnerField})
    REFERENCES {DeckOps.TableName}({DeckOps.IdField})
    ON DELETE CASCADE
);";

    private static readonly LeafMods[] Fields;

    static LeafOps()
    {
        var leaf = new List<LeafMods>(4);

        foreach (var item in Enum.GetValues<LeafMods>())
        {
            if (item != LeafMods.None)
            {
                leaf.Add(item);
            }
        }

        Fields = leaf.ToArray();
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

    /// <summary>
    /// Gets a <see cref="GardenLeaf"/> reader command given a parent <see cref="GardenDeck"/>.
    /// </summary>
    public static DbCommand GetReader(DbConnection con, GardenDeck owner)
    {
        const string NSpace = $"{nameof(LeafOps)}.{nameof(GetReader)}";

        const string Sql = $"SELECT * FROM {TableName} WHERE {OwnerField} = @{OwnerField} ORDER BY {IdField}";
        Diag.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.AddValue($"@{OwnerField}", owner.Id.Value);

        return cmd;
    }

    /// <summary>
    /// Inserts <see cref="GardenLeaf"/> into the database and assigns <see cref="Garden"/> and return true on success.
    /// </summary>
    /// <remark>
    /// The result is false on unexpected <see cref="GardenLeaf.Id"/> conflict, otherwise it throws on failure.
    /// </remark>
    /// <exception cref="ArgumentException">Id undefined</exception>
    /// <exception cref="DbException">Database exception</exception>
    public static bool Insert(DbConnection con, GardenLeaf obj, DbTransaction? tran = null)
    {
        const string NSpace = $"{nameof(LeafOps)}.{nameof(GetReader)}";

        const string Sql = $@"INSERT INTO {TableName}
        ({IdField}, {OwnerField}, {FormatField}, {AssistantField}, {ContentField}) VALUES
        (@{IdField}, @{OwnerField}, @{FormatField}, @{AssistantField}, @{ContentField});";

        if (obj.Id.IsEmpty)
        {
            throw new ArgumentException($"{nameof(GardenLeaf)}.{nameof(obj.Id)} undefined", nameof(obj));
        }

        if (obj.Owner?.Id.IsEmpty != false)
        {
            throw new ArgumentException($"{nameof(obj.Owner)}.{nameof(GardenDeck.Id)} undefined", nameof(obj));
        }

        Diag.ThrowIfFalse(obj.Format.IsLegal());

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;

        cmd.AddValue($"@{IdField}", obj.Id.Value);
        cmd.AddValue($"@{OwnerField}", obj.Owner.Id.Value);
        cmd.AddValue($"@{FormatField}", (byte)obj.Format);  // <- enum as byte
        cmd.AddValue($"@{AssistantField}", obj.Assistant);
        cmd.AddValue($"@{ContentField}", obj.Content);

        Diag.WriteLine(NSpace, cmd.CommandText);

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected here
            Diag.Fail($"Possible {nameof(GardenLeaf)} primary key conflict");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the <see cref="GardenLeaf"/>.
    /// </summary>
    public static bool Update(DbConnection con, GardenLeaf obj, LeafMods mods, DbTransaction? tran = null)
    {
        const string Sql = $"UPDATE {TableName} SET";

        if (mods == LeafMods.None)
        {
            Diag.Fail($"{nameof(LeafMods)} none");
            return false;
        }

        bool i0 = false;
        var buffer = new StringBuilder(Sql);

        using var cmd = con.CreateCommand();
        cmd.AddValue("@id", obj.Id.Value);

        foreach (var item in Fields)
        {
            if (!mods.HasFlag(item))
            {
                continue;
            }

            switch (item)
            {
                case LeafMods.Assistant:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {AssistantField} = @{AssistantField}");
                    cmd.AddValue($"@{AssistantField}", obj.Assistant);
                    i0 = true;
                    continue;
                case LeafMods.Content:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {ContentField} = @{ContentField}");
                    cmd.AddValue($"@{ContentField}", obj.Content);
                    i0 = true;
                    continue;
            }
        }

        Diag.ThrowIfFalse(i0);
        buffer.Append($" WHERE {IdField} = @{IdField};");
        cmd.CommandText = buffer.ToString();
        cmd.Transaction = tran;

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected
            Diag.Fail($"Unexpected {nameof(GardenLeaf)} UPDATE failure");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Delete the <see cref="GardenLeaf"/> item with the given "id".
    /// </summary>
    public static bool Delete(DbConnection con, Zuid id)
    {
        const string NSpace = $"{nameof(LeafOps)}.{nameof(Delete)}";
        Diag.WriteLine(NSpace, "ZUID: " + id);

        const string Sql = $"DELETE FROM {TableName} WHERE {IdField} = @{IdField};";
        Diag.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.AddValue("@id", id.Value);
        return cmd.ExecuteNonQuery() != 0;
    }

    /// <summary>
    /// Test routine only.
    /// </summary>
    public static void CreateSchema1(DbConnection con, DbTransaction tran)
    {
        ExecuteNonQuery(con, tran, CreateSql1);
    }

    /// <summary>
    /// Ensure table schema.
    /// </summary>
    public static void CreateOrAlter(DbConnection con, DbTransaction tran, int currentVersion)
    {
        const string NSpace = $"{nameof(DeckOps)}.{nameof(CreateOrAlter)}";
        Diag.ThrowIfNegative(currentVersion);

        if (currentVersion == 0)
        {
            ExecuteNonQuery(con, tran, CreateSqlLatest);
            return;
        }

        if (currentVersion < MetaOps.SchemaVersion)
        {
            Diag.WriteLine(NSpace, $"UPGRADE TABLE: {TableName} from {currentVersion} to {MetaOps.SchemaVersion}");
            while (UpgradeIteration(con, tran, ++currentVersion)) ;
        }
    }

    private static bool UpgradeIteration(DbConnection con, DbTransaction tran, int currentVersion)
    {
        if (currentVersion > MetaOps.SchemaVersion)
        {
            return false;
        }

        // I.e. ALTER TABLE message ADD COLUMN metadata TEXT DEFAULT '';
        Diag.ThrowIfZero(currentVersion);
        Diag.ThrowIfGreaterThan(currentVersion, MetaOps.MaxSchema);

        switch (currentVersion)
        {
            case 1:
                // First;
                return true;

            case 2:
                // Alter from 1 to 2
                // ExecuteNonQuery(con, tran, "ALTER TABLE...");
                return true;
            default:
                // Not expected
                // We must account for all versions even if no change
                throw new InvalidOperationException($"Unknown schema version {currentVersion}");
        }
    }

    private static void ExecuteNonQuery(DbConnection con, DbTransaction? tran, string? sql)
    {
        const string NSpace = $"{nameof(LeafOps)}.{nameof(ExecuteNonQuery)}";

        if (!string.IsNullOrEmpty(sql))
        {
            using var cmd = con.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = sql;

            Diag.WriteLine(NSpace, cmd.CommandText);
            cmd.ExecuteNonQuery();
        }
    }

}
