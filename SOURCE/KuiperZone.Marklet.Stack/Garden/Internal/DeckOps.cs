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
    /// Deck column. Do not change.
    /// </summary>
    public const string IdField = "id";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string FormatField = "format";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string OriginField = "origin_basket";

    /// <summary>
    /// Deck column. Do not change.
    /// </summary>
    public const string BasketField = "current_basket";

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
    public const string FlagsField = "flags";

    // VERSION 1
    // Keep this for all time.
    public const string CreateSql1 = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}       BIGINT NOT NULL PRIMARY KEY,
{FormatField}   INTEGER NOT NULL,
{OriginField}   INTEGER NOT NULL,
{BasketField}   INTEGER NOT NULL,
{UpdatedField}  BIGINT NOT NULL,
{TitleField}    VARCHAR(255),
{ModelField}    VARCHAR(255),
{FolderField}   VARCHAR(255),
{FlagsField}    INTEGER NOT NULL DEFAULT 0
);";

    // VERSION 2
    // When this is changed, add to Upgrade()
    private const string CreateSqlLatest = $@"CREATE TABLE IF NOT EXISTS {TableName} (
{IdField}       BIGINT NOT NULL PRIMARY KEY,
{FormatField}   INTEGER NOT NULL,
{OriginField}   INTEGER NOT NULL,
{BasketField}   INTEGER NOT NULL,
{UpdatedField}  BIGINT NOT NULL,
{TitleField}    VARCHAR(255),
{ModelField}    VARCHAR(255),
{FolderField}   VARCHAR(255),
{FlagsField}    INTEGER NOT NULL DEFAULT 0
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
        Diag.WriteLine(NSpace, Sql);

        var cmd = con.CreateCommand();
        cmd.CommandText = Sql;

        return cmd;
    }

    /// <summary>
    /// Inserts the <see cref="GardenDeck"/> header, but not its children. Throws on failure.
    /// </summary>
    /// <exception cref="ArgumentException">Id undefined</exception>
    /// <exception cref="InvalidOperationException">Primary key conflict</exception>
    /// <exception cref="DbException">Database exception</exception>
    public static void Insert(DbConnection con, GardenDeck obj, DbTransaction? tran = null)
    {
        const string Sql = $@"INSERT INTO {TableName}
        ({IdField}, {FormatField}, {OriginField}, {BasketField}, {UpdatedField}, {TitleField}, {ModelField}, {FolderField}, {FlagsField}) VALUES
        (@{IdField}, @{FormatField}, @{OriginField}, @{BasketField}, @{UpdatedField}, @{TitleField}, @{ModelField}, @{FolderField}, @{FlagsField});";

        if (obj.Id.IsEmpty)
        {
            throw new ArgumentException($"{nameof(GardenDeck)}.{nameof(obj.Id)} undefined", nameof(obj));
        }

        Diag.ThrowIfFalse(obj.Format.IsLegal());
        Diag.ThrowIfFalse(obj.CurrentBasket.IsLegal());

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;

        cmd.AddValue($"@{IdField}", obj.Id.Value);
        cmd.AddValue($"@{FormatField}", (byte)obj.Format); // <- enum
        cmd.AddValue($"@{OriginField}", (byte)obj.OriginBasket); // <- enum
        cmd.AddValue($"@{BasketField}", (byte)obj.CurrentBasket); // <- enum
        cmd.AddValue($"@{UpdatedField}", obj.Updated.Ticks);
        cmd.AddValue($"@{TitleField}", obj.Title);
        cmd.AddValue($"@{ModelField}", obj.Model);
        cmd.AddValue($"@{FolderField}", obj.Folder);
        cmd.AddValue($"@{FlagsField}", obj.Flags); // <- enum

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected at this point
            throw new InvalidOperationException($"Primary {nameof(GardenDeck)} key conflict");
        }
    }

    /// <summary>
    /// Updates the <see cref="GardenDeck"/> header, but not its children.
    /// </summary>
    /// <exception cref="DbException">Database exception</exception>
    public static bool Update(DbConnection con, GardenDeck obj, DeckMods mods, DbTransaction? tran = null)
    {
        const string Sql = $"UPDATE {TableName} SET";

        if (mods == DeckMods.None)
        {
            Diag.Fail($"{nameof(DeckMods)} none");
            return false;
        }

        bool i0 = false;
        var buffer = new StringBuilder(Sql);

        using var cmd = con.CreateCommand();
        cmd.Transaction = tran;
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
                    cmd.AddValue($"@{BasketField}", obj.CurrentBasket);
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
                case DeckMods.Flags:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append($" {FlagsField} = @{FlagsField}");
                    cmd.AddValue($"@{FlagsField}", obj.Flags);
                    i0 = true;
                    continue;
                default:
                    Diag.Fail($"Unknown {nameof(DeckMods)} {item}");
                    break;
            }
        }

        Diag.ThrowIfFalse(i0);
        buffer.Append($" WHERE {IdField} = @{IdField};");
        cmd.CommandText = buffer.ToString();

        if (cmd.ExecuteNonQuery() != 1)
        {
            // Not expected
            Diag.Fail($"Unexpected {nameof(GardenDeck)} UPDATE failure");
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
    /// <exception cref="DbException">Database exception</exception>
    public static bool Delete(DbConnection con, Zuid id)
    {
        const string NSpace = $"{nameof(DeckOps)}.{nameof(Delete)}";
        Diag.WriteLine(NSpace, "ZUID: " + id);

        const string Sql = $"DELETE FROM {TableName} WHERE {IdField} = @{IdField};";
        Diag.WriteLine(NSpace, Sql);

        // Employs delete cascade on leaf table
        // So no need to delete leaf data individually.
        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.AddValue("@id", id.Value);
        return cmd.ExecuteNonQuery() != 0;
    }

    /// <summary>
    /// Test routine only.
    /// </summary>
    /// <exception cref="DbException">Database exception</exception>
    public static void CreateSchema1(DbConnection con, DbTransaction tran)
    {
        ExecuteNonQuery(con, tran, CreateSql1);
    }

    /// <summary>
    /// Ensure table schema.
    /// </summary>
    /// <exception cref="DbException">Database exception</exception>
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
        const string NSpace = $"{nameof(DeckOps)}.{nameof(ExecuteNonQuery)}";

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
