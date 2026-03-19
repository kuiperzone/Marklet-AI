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
/// Database operations for <see cref="GardenDeck"/>.
/// </summary>
internal static class DeckOps
{
    /// <summary>
    /// Name of deck table. Do not change.
    /// </summary>
    public const string TableName = "deck";

    /// <summary>
    /// Gets the current table version of deck data, where 1 is the first release.
    /// </summary>
    public const int TableVersion = 1;

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string IdField = "id";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string KindField = "deck_kind";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string OriginField = "origin_kind";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string BasketField = "basket_kind";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string UpdatedField = "updated";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string TitleField = "title";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string ModelField = "model";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string FolderField = "folder";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string PinnedField = "pinned";

    // VERSION 1
    private const string CreateSql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}       BIGINT NOT NULL PRIMARY KEY,
{KindField}     INTEGER NOT NULL,
{OriginField}   INTEGER NOT NULL,
{BasketField}   INTEGER NOT NULL,
{UpdatedField}  BIGINT NOT NULL,
{TitleField}    VARCHAR(255),
{ModelField}    VARCHAR(255),
{FolderField}    VARCHAR(255),
{PinnedField}   BOOL NOT NULL DEFAULT FALSE
);";

    private static readonly DeckMods[] Fields;

    static DeckOps()
    {
        var list = new List<DeckMods>(8);

        foreach (var item in Enum.GetValues<DeckMods>())
        {
            if (item != DeckMods.None && item != DeckMods.Leaf)
            {
                list.Add(item);
            }
        }

        Fields = list.ToArray();
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
    /// Gets a <see cref="GardenDeck"/> reader command which should be disposed.
    /// </summary>
    public static DbCommand GetReader(DbConnection con)
    {
        const string NSpace = $"{nameof(DeckOps)}.{nameof(GetReader)}";

        const string Sql = $"SELECT * FROM {TableName} ORDER BY {IdField};";
        ConditionalDebug.WriteLine(NSpace, Sql);

        var cmd = con.CreateCommand();
        cmd.CommandText = Sql;

        return cmd;
    }

    /// <summary>
    /// Inserts the <see cref="GardenDeck"/> header, but not its children.
    /// </summary>
    /// <exception cref="ArgumentException">Id undefined</exception>
    public static bool Insert(DbConnection con, GardenDeck obj, DbTransaction? tran = null)
    {
        const string Sql = $@"INSERT INTO {TableName}
        ({IdField}, {KindField}, {OriginField}, {BasketField}, {UpdatedField}, {TitleField}, {ModelField}, {FolderField}, {PinnedField}) VALUES
        (@{IdField}, @{KindField}, @{OriginField}, @{BasketField}, @{UpdatedField}, @{TitleField}, @{ModelField}, @{FolderField}, @{PinnedField});";

        if (obj.Id.IsEmpty)
        {
            throw new ArgumentException($"{nameof(GardenDeck)}.{nameof(obj.Id)} undefined", nameof(obj));
        }

        ConditionalDebug.ThrowIfFalse(obj.Kind.IsLegal());
        ConditionalDebug.ThrowIfFalse(obj.Basket.IsLegal());

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;

        cmd.AddValue($"@{IdField}", obj.Id.Value);
        cmd.AddValue($"@{KindField}", (byte)obj.Kind); // <- enum as byte
        cmd.AddValue($"@{OriginField}", (byte)obj.Origin); // <- enum as byte
        cmd.AddValue($"@{BasketField}", (byte)obj.Basket); // <- enum as byte
        cmd.AddValue($"@{UpdatedField}", obj.Updated.Ticks);
        cmd.AddValue($"@{TitleField}", obj.Title);
        cmd.AddValue($"@{ModelField}", obj.Model);
        cmd.AddValue($"@{FolderField}", obj.Folder);
        cmd.AddValue($"@{PinnedField}", obj.IsPinned);

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected
            ConditionalDebug.Fail($"Unexpected {nameof(GardenDeck)} INSERT failure");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the <see cref="GardenDeck"/> header, but not its children.
    /// </summary>
    public static bool Update(DbConnection con, GardenDeck obj, DeckMods mods, DbTransaction? tran = null)
    {
        const string Sql = $"UPDATE {TableName} SET";

        if (mods == DeckMods.None)
        {
            ConditionalDebug.Fail($"{nameof(DeckMods)} none");
            return false;
        }

        bool i0 = false;
        var buffer = new StringBuilder(Sql);

        using var cmd = con.CreateCommand();
        cmd.AddValue($"@{IdField}", obj.Id.Value);

        foreach (var item in Fields)
        {
            if (!mods.HasFlag(item))
            {
                continue;
            }

            switch (item)
            {
                case DeckMods.Updated:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {UpdatedField} = @{UpdatedField}");
                    cmd.AddValue($"@{UpdatedField}", obj.Updated.Ticks);
                    i0 = true;
                    continue;
                case DeckMods.Basket:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {BasketField} = @{BasketField}");
                    cmd.AddValue($"@{BasketField}", obj.Basket);
                    i0 = true;
                    continue;
                case DeckMods.Title:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {TitleField} = @{TitleField}");
                    cmd.AddValue($"@{TitleField}", obj.Title);
                    i0 = true;
                    continue;
                case DeckMods.Model:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {ModelField} = @{ModelField}");
                    cmd.AddValue($"@{ModelField}", obj.Model);
                    i0 = true;
                    continue;
                case DeckMods.Folder:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {FolderField} = @{FolderField}");
                    cmd.AddValue($"@{FolderField}", obj.Folder);
                    i0 = true;
                    continue;
                case DeckMods.Pinned:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {PinnedField} = @{PinnedField}");
                    cmd.AddValue($"@{PinnedField}", obj.IsPinned);
                    i0 = true;
                    continue;
                default:
                    ConditionalDebug.Fail($"Unknown {nameof(DeckMods)} {item}");
                    break;
            }
        }

        ConditionalDebug.ThrowIfFalse(i0);
        buffer.Append($" WHERE {IdField} = @{IdField};");
        cmd.CommandText = buffer.ToString();
        cmd.Transaction = tran;

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected
            ConditionalDebug.Fail($"Unexpected {nameof(GardenDeck)} UPDATE failure");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Delete the <see cref="GardenDeck"/> item with the given "id", including its leaf data.
    /// </summary>
    /// <remarks>
    /// The deletion will cascade the <see cref="GardenLeaf"/> children which will also be removed.
    /// </remarks>
    public static bool Delete(DbConnection con, Zuid id)
    {
        const string NSpace = $"{nameof(DeckOps)}.{nameof(Delete)}";
        ConditionalDebug.WriteLine(NSpace, "ZUID: " + id);

        const string Sql = $"DELETE FROM {TableName} WHERE {IdField} = @{IdField};";
        ConditionalDebug.WriteLine(NSpace, Sql);

        // Employs delete cascade on leaf table
        // So no need to delete leaf data individually.
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
        const string NSpace = $"{nameof(DeckOps)}.{nameof(EnsureTable)}";
        ConditionalDebug.WriteLine(NSpace, CreateSql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = CreateSql;
        cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    public static void UpgradeTable(DbConnection _, DbTransaction __, int currentSchema)
    {
        // Future expansion.
        const string NSpace = $"{nameof(DeckOps)}.{nameof(UpgradeTable)}";

        // FOR FUTURE
        // I.e. ALTER TABLE message ADD COLUMN metadata TEXT DEFAULT '';
        ConditionalDebug.WriteLine(NSpace, $"UPGRADE TABLE: {TableName} from {currentSchema} to {TableVersion}");

        // Not expected (would currently be an error)
        throw new NotImplementedException($"{TableName} upgrade not implemented");
    }

}
