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
    /// Gets the table version of message leaf data, where 1 is the first release.
    /// </summary>
    public const int TableVersion = 1;

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string IdField = "id";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string ParentField = "parent_id";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string KindField = "leaf_kind";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string AssistantField = "assistant";

    /// <summary>
    /// Leaf column. Do not change.
    /// </summary>
    public const string ContentField = "content";

    // VERSION 1
    private const string CreateSql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}           BIGINT NOT NULL PRIMARY KEY,
{ParentField}       BIGINT NOT NULL,
{KindField}         INTEGER NOT NULL,
{AssistantField}    VARCHAR(255),
{ContentField}      TEXT NOT NULL,
CONSTRAINT  fk_parent
    FOREIGN KEY ({ParentField})
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
    public static DbCommand GetReader(DbConnection con, GardenDeck parent)
    {
        const string NSpace = $"{nameof(LeafOps)}.{nameof(GetReader)}";

        const string Sql = $"SELECT * FROM {TableName} WHERE {ParentField} = @{ParentField} ORDER BY {IdField}";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.AddValue($"@{ParentField}", parent.Id.Value);

        return cmd;
    }

    /// <summary>
    /// Inserts the <see cref="GardenLeaf"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Id undefined</exception>
    public static bool Insert(DbConnection con, GardenLeaf obj, DbTransaction? tran = null)
    {
        const string Sql = $@"INSERT INTO {TableName}
        ({IdField}, {ParentField}, {KindField}, {AssistantField}, {ContentField}) VALUES
        (@{IdField}, @{ParentField}, @{KindField}, @{AssistantField}, @{ContentField});";

        if (obj.Id.IsEmpty)
        {
            throw new ArgumentException($"{nameof(GardenLeaf)}.{nameof(obj.Id)} undefined", nameof(obj));
        }

        if (obj.DeckOwner?.Id.IsEmpty != false)
        {
            throw new ArgumentException($"{nameof(obj.DeckOwner)}.{nameof(GardenDeck.Id)} undefined", nameof(obj));
        }

        ConditionalDebug.ThrowIfFalse(obj.Kind.IsLegal());

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;

        cmd.AddValue($"@{IdField}", obj.Id.Value);
        cmd.AddValue($"@{ParentField}", obj.DeckOwner.Id.Value);
        cmd.AddValue($"@{KindField}", (byte)obj.Kind);  // <- enum as byte
        cmd.AddValue($"@{AssistantField}", obj.Assistant);
        cmd.AddValue($"@{ContentField}", obj.Content);

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected
            ConditionalDebug.Fail($"Unexpected {nameof(GardenLeaf)} INSERT failure");
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
            ConditionalDebug.Fail($"{nameof(LeafMods)} none");
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

        ConditionalDebug.ThrowIfFalse(i0);
        buffer.Append($" WHERE {IdField} = @{IdField};");
        cmd.CommandText = buffer.ToString();
        cmd.Transaction = tran;

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected
            ConditionalDebug.Fail($"Unexpected {nameof(GardenLeaf)} UPDATE failure");
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
        ConditionalDebug.WriteLine(NSpace, "ZUID: " + id);

        const string Sql = $"DELETE FROM {TableName} WHERE {IdField} = @{IdField};";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.AddValue("@id", id.Value);
        return cmd.ExecuteNonQuery() != 0;
    }

    /// <summary>
    /// Ensure table exists.
    /// </summary>
    public static void EnsureTable(DbConnection con, DbTransaction tran)
    {
        const string NSpace = $"{nameof(LeafOps)}.{nameof(EnsureTable)}";
        ConditionalDebug.WriteLine(NSpace, CreateSql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = CreateSql;
        cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    public static void UpgradeTable(DbConnection _, DbTransaction __, int currentSchema)
    {
        // Future expansion.
        const string NSpace = $"{nameof(LeafOps)}.{nameof(UpgradeTable)}";

        // FOR FUTURE
        // I.e. ALTER TABLE message ADD COLUMN metadata TEXT DEFAULT '';
        ConditionalDebug.WriteLine(NSpace, $"UPGRADE TABLE: {TableName} from {currentSchema} to {TableVersion}");

        // Not expected (would currently be an error)
        throw new NotImplementedException($"{TableName} upgrade not implemented");
    }

}
