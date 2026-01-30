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

using System.Collections;
using System.Data.Common;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// A sequence container for <see cref="GardenSession"/> items backed by a database implementation provided by the <see
/// cref="IMemoryGardener"/> implementation.
/// </summary>
/// <remarks>
/// A single instance is to be used by the application which shall be long-lived. Thanks to Molly Rocket for the helpful
/// metaphors.
/// </remarks>
public sealed class MemoryGarden : IReadOnlyCollection<GardenSession>
{
    /// <summary>
    /// Gets the maximum length of "name" strings.
    /// </summary>
    public const int MaxNameLength = 64;

    private const string TableName = "garden_info"; // Do not change
    private readonly IReadOnlyList<GardenSession> Empty = new List<GardenSession>();
    private readonly GardenBin[] _allBins;

    /// <summary>
    /// Constructor with "gardener" instance.
    /// </summary>
    public MemoryGarden(IMemoryGardener gardener)
    {
        Gardener = gardener;

        HomeBin = new(this, false);
        ArchiveBin = new(this, false);
        WasteBin = new(this, true);

        _allBins = [HomeBin, ArchiveBin, WasteBin];
    }


    /// <summary>
    /// Occurs when the <see cref="Selected"/> reference instance changes.
    /// </summary>
    public event EventHandler<SelectedChangedEventArgs>? SelectedChanged;

    /// <summary>
    /// Occurs when the properties on the <see cref="Selected"/> instance are updated.
    /// </summary>
    /// <remarks>
    /// The event is invoked only for the instance given by <see cref="Selected"/>. It is not invoked when items are
    /// added or removed.
    /// </remarks>
    public event EventHandler<SelectedUpdatedEventArgs>? SelectedUpdated;

    /// <summary>
    /// Provides the gardener instance.
    /// </summary>
    public IMemoryGardener Gardener { get; }

    /// <summary>
    /// Implements <see cref="IReadOnlyCollection{T}.Count"/> and gets the total open count.
    /// </summary>
    /// <remarks>
    /// Always returns 0 if <see cref="IsOpen"/> is false.
    /// </remarks>
    public int Count
    {
        get
        {
            int count = 0;

            for(int n = 0; n < _allBins.Length; ++n)
            {
                count += _allBins[n].Count;
            }

            return count;
        }
    }

    /// <summary>
    /// Gets whether <see cref="GardenSession"/> contents are loaded into memory.
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets the "selected" <see cref="GardenSession"/> child.
    /// </summary>
    /// <remarks>
    /// The term "selected" means that the item is selected for display. Initial value is null (none).
    /// </remarks>
    public GardenSession? Selected { get; private set; }

    /// <summary>
    /// Gets the "home" bin.
    /// </summary>
    public GardenBin HomeBin { get; }

    /// <summary>
    /// Gets the "archive" bin.
    /// </summary>
    public GardenBin ArchiveBin { get; }

    /// <summary>
    /// Gets the "waste" bin.
    /// </summary>
    /// <remarks>
    /// Items in this bin will eventually go to landfill if not recycled.
    /// </remarks>
    public GardenBin WasteBin { get; }

    /// <summary>
    /// Implements <see cref="IEnumerable{T}.GetEnumerator()"/>.
    /// </summary>
    /// <remarks>
    /// The order of items should be assumed to be arbitrary. Always returns an empty sequence if <see cref="IsOpen"/>
    /// is false.
    /// </remarks>
    public IEnumerator<GardenSession> GetEnumerator()
    {
        int count = Count;

        if (count != 0)
        {
            var list = new List<GardenSession>(count);

            for (int n = 0; n < _allBins.Length; ++n)
            {
                list.AddRange(_allBins[n].AsDirectEnumerable());
            }

            return list.GetEnumerator();
        }

        return Empty.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Opens the database, loads session headers, and sets <see cref="IsOpen"/> to true.
    /// </summary>
    /// <remarks>
    /// It does nothing if <see cref="IsOpen"/> is already true. It may be called immediately after construction.
    /// </remarks>
    public OpenInit Open()
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Open)}";
        ConditionalDebug.WriteLine(NSpace, "OPEN GARDEN");

        if (!IsOpen)
        {
            using var con = Gardener.Connect();

            var init = Init(con);
            ConditionalDebug.WriteLine(NSpace, $"GARDEN INIT: {init}");

            var list = new List<GardenSession>(32);
            GardenSession.Read(con, this, list);

            IsOpen = true;
            var binSet = new HashSet<GardenBin>();

            foreach (var item in list)
            {
                var b = GetBin(item);
                binSet.Add(b);

                // Do not invoke event for every item
                b.Add(item, false);
            }

            foreach (var item in binSet)
            {
                // Single event for multiple insertions
                item.OnChanged(true);
            }

            return init;
        }

        ConditionalDebug.WriteLine(NSpace, "Already open");
        return OpenInit.Open;
    }

    /// <summary>
    /// Frees memory and sets <see cref="IsOpen"/> to false, returning true on success.
    /// </summary>
    public bool Close()
    {
        if (IsOpen)
        {
            try
            {
                foreach (var bin in _allBins)
                {
                    bin.Close();
                }

                // Discard should clear this
                ConditionalDebug.ThrowIfNotNull(Selected);
                return true;
            }
            finally
            {
                IsOpen = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Simply calls <see cref="Close"/> followed by <see cref="Open"/>.
    /// </summary>
    public OpenInit Reload()
    {
        Close();
        return Open();
    }

    /// <summary>
    /// Returns true if the garden contains the given item.
    /// </summary>
    public bool Contains(GardenSession item)
    {
        // We should have to call container for this.
        return IsOpen && item.Owner == this;
    }

    /// <summary>
    /// Inserts a new <see cref="GardenSession"/> instance and returns the same instance.
    /// </summary>
    /// <remarks>
    /// The instance is not committed (persistant) until a message is appended or properties are changed. Throws if <see
    /// cref="IsOpen"/> is false.
    /// </remarks>
    /// <exception cref="ArgumentException">Already inserted</exception>
    /// <exception cref="InvalidOperationException">Garden closed</exception>
    public GardenSession Insert(GardenSession item)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Insert)}";
        ConditionalDebug.WriteLine(NSpace, "NEW INSTANCE");
        ConditionalDebug.WriteLine(NSpace, $"Bin Kind: {item.HomeBin}");

        ThrowIfClosed();
        item.InsertDbNoRaise(this);
        GetBin(item).Add(item, true);

        if (item.IsSelected)
        {
            OnSelected(item);
        }

        return item;
    }

    /// <summary>
    /// Overload.
    /// </summary>
    public GardenSession Insert(BinKind bin = BinKind.Home)
    {
        return Insert(new GardenSession(bin));
    }

    /// <summary>
    /// Deletes all data from the database, including all messages, and sets <see cref="Count"/> to 0, but does not
    /// delete the database itself.
    /// </summary>
    /// <remarks>
    /// It returns true if data was removed, or false if the database was already empty. The database is not closed
    /// after the call, but will be empty. Throws if <see cref="IsOpen"/> is false when called.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Garden closed</exception>
    public bool Purge()
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Purge)}";
        ConditionalDebug.WriteLine(NSpace, "PURGE");

        ThrowIfClosed();

        if (Count != 0)
        {
            ConditionalDebug.WriteLine(NSpace, "Clear memory");

            Close();
            IsOpen = true;

            const string Sql = $"DELETE FROM {GardenSession.TableName};";
            ConditionalDebug.WriteLine(NSpace, Sql);

            using var con = Gardener.Connect();
            using var cmd = con.CreateCommand();
            cmd.CommandText = Sql;
            cmd.ExecuteNonQuery();
        }

        ConditionalDebug.ThrowIfNotNull(Selected);
        return false;
    }

    /// <summary>
    /// Moves items which expire from <see cref="HomeBin"/> to <see cref="WasteBin"/>, and sends items that have
    /// expired in <see cref="WasteBin"/> to landfill, and closes unused children to free up memory.
    /// </summary>
    /// <remarks>
    /// This house cleaning method is to be polled periodically with "emptyCompose" equal to false (approx. once a
    /// minute). It returns true on any change. Idle children are moved to <see cref="WasteBin"/> according to <see
    /// cref="GardenBin.Timeout"/>, and children already in <see cref="WasteBin"/> are deleted. Moreover, large
    /// children may be closed to free memory. Throws if <see cref="IsOpen"/> is false.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Garden closed</exception>
    public bool Prune()
    {
        ThrowIfClosed();

        bool deleted = false;
        long openLength = 0;
        var waste = new List<GardenSession>(8);

        foreach (var item in _allBins)
        {
            deleted |= item.Prune(waste, ref openLength);
        }

        if (waste.Count != 0)
        {
            var con = Gardener.Connect();
            var oldBins = new HashSet<GardenBin>();

            foreach (var item in waste)
            {
                ConditionalDebug.ThrowIfTrue(item.IsWaste);

                // Hold the bin before change
                oldBins.Add(GetBin(item));

                // Write change
                item.SetIsWasteNoRaise(con, true);
            }

            // Raise single change on each bin
            // rather than invoking for every item.
            foreach (var item in oldBins)
            {
                item.OnChanged(true);
            }

            WasteBin.OnChanged(true);
            return true;
        }

        if (deleted)
        {
            WasteBin.OnChanged(true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds first child instance with matching "title", or null.
    /// </summary>
    /// <remarks>
    /// Matching is simple and case sensitive. The <see cref="GardenSession.Title"/> value does not have to be unique and
    /// only first matching instance is returned, or null if not found. The call always returns null if <see
    /// cref="IsOpen"/> is false.
    /// </remarks>
    public GardenSession? FindFirstOnTitle(string? title)
    {
        title = SanitizeName(title);

        if (title != null && IsOpen)
        {
            foreach (var bin in _allBins)
            {
                foreach (var item in bin)
                {
                    if (item.Title == title)
                    {
                        return item;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the <see cref="GardenBin"/> instance.
    /// </summary>
    /// <remarks>
    /// The result is always <see cref="WasteBin"/> where "isWaste" is true.
    /// </remarks>
    public GardenBin GetBin(BinKind kind, bool isWaste = false)
    {
        if (isWaste)
        {
            return WasteBin;
        }

        switch (kind)
        {
            case BinKind.Archive:
                return ArchiveBin;
            default:
                return HomeBin;
        }
    }

    internal static string? SanitizeName(string? text)
    {
        const SanFlags SanNameFlags = SanFlags.Trim | SanFlags.NormC | SanFlags.SubControl;
        text = Sanitizer.Sanitize(text, SanNameFlags, MaxNameLength);
        return !string.IsNullOrEmpty(text) ? text : null;
    }

    /// <summary>
    /// Sets <see cref="Selected"/> on the given child and returns true if selection changed.
    /// </summary>
    /// <remarks>
    /// Does nothing if <see cref="IsOpen"/> is false.
    /// </remarks>
    internal void OnSelected(GardenSession? item)
    {
        if (Selected != item)
        {
            ConditionalDebug.ThrowIfTrue(item?.IsSelected == false);

            // Silent
            var old = Selected;
            old?.DeselectNoRaise();

            if (IsOpen && item != null)
            {
                Selected = item;
                SelectedChanged?.Invoke(this, new(item, old));
                return;
            }

            Selected = null;
            SelectedChanged?.Invoke(this, new(null, old));
        }
    }


    /// <summary>
    /// Called when child properties changes.
    /// </summary>
    internal void OnChildChanged(GardenSession item, ModFlags flags, bool raiseBin)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(OnChildChanged)}";
        ConditionalDebug.WriteLine(NSpace, "CHANGED EVENT: " + item.ToString());
        ConditionalDebug.WriteLine(NSpace, "Flags: " + flags);

        if (flags != ModFlags.None)
        {
            try
            {
                if (flags.HasFlag(ModFlags.IsWaste))
                {
                    RemoveFromBins(item, raiseBin);
                    GetBin(item).Add(item, raiseBin);
                    return;
                }

                if (flags.HasFlag(ModFlags.HomeBin) && !item.IsWaste)
                {
                    RemoveFromBins(item, raiseBin);
                    GetBin(item).Add(item, raiseBin);
                    return;
                }

                GetBin(item).OnChanged(raiseBin);
            }
            finally
            {
                if (Selected == item)
                {
                    SelectedUpdated?.Invoke(this, new(item));
                }
            }
        }
    }

    internal void OnChildDeleted(GardenSession item, bool raiseBin)
    {
        GetBin(item).Remove(item, raiseBin);
    }

    private static void CreateTableIfNotExist(DbConnection con, DbTransaction? tran)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(CreateTableIfNotExist)}";
        ConditionalDebug.WriteLine(NSpace, $"CREATE TABLE: {TableName}");

        const string Sql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
    table_name  VARCHAR(255) PRIMARY KEY,
    version     INTEGER NOT NULL DEFAULT 0,
    meta        TEXT
);";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    private static int SelectVersion(DbConnection con, DbTransaction tran, string table)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(SelectVersion)}";
        ConditionalDebug.WriteLine(NSpace, $"SELECT VERSION from: {TableName}");

        const string Sql = $"SELECT version FROM {TableName} WHERE table_name = @table_name;";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = CreateCommand(con, tran, Sql, table);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            int version = reader.GetInt32("version");
            ConditionalDebug.WriteLine(NSpace, $"Table {table} = {version}");
            return version;
        }

        // Not found
        return 0;
    }

    private static void InsertVersion(DbConnection con, DbTransaction tran, string table, int version)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(InsertVersion)}";
        ConditionalDebug.WriteLine(NSpace, $"INSERT VERSION: {table} = {version}");

        const string Sql = $"INSERT INTO {TableName} (table_name, version) VALUES (@table_name, @version);";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = CreateCommand(con, tran, Sql, table, version);
        cmd.ExecuteNonQuery();
    }

    private static void UpdateVersion(DbConnection con, DbTransaction tran, string table, int version)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(UpdateVersion)}";
        ConditionalDebug.WriteLine(NSpace, $"UPDATE VERSION: {table} = {version}");

        const string Sql = $"UPDATE {TableName} SET version = @version WHERE table_name = @table_name;";
        using var cmd = CreateCommand(con, tran, Sql, table, version);

        cmd.ExecuteNonQuery();
    }

    private static OpenInit Init(DbConnection con)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Init)}";
        ConditionalDebug.WriteLine(NSpace, "INITIALIZE GARDEN");

        const string SessionTable = GardenSession.TableName;
        const string LeafTable = GardenLeaf.TableName;
        using var tran = con.BeginTransaction();

        try
        {
            // CREATE TABLES
            var init = OpenInit.Open;
            CreateTableIfNotExist(con, tran);
            GardenSession.CreateTable(con, tran);
            GardenLeaf.CreateTableInternal(con, tran);

            // PARENT
            int version = SelectVersion(con, tran, SessionTable);
            ConditionalDebug.WriteLine(NSpace, $"'{SessionTable}' current version  = {version}");

            if (version <= 0)
            {
                init = OpenInit.Created;
                ConditionalDebug.WriteLine(NSpace, $"Register '{SessionTable}' current version = {version}");
                InsertVersion(con, tran, SessionTable, GardenSession.Version);
            }
            else
            if (GardenSession.UpgradeTable(con, tran, version))
            {
                init = OpenInit.SchemaUpgraded;
                ConditionalDebug.WriteLine(NSpace, $"Upgrade '{SessionTable}' current version = {version}");
                UpdateVersion(con, tran, SessionTable, version);
            }

            // LEAF
            version = SelectVersion(con, tran, LeafTable);
            ConditionalDebug.WriteLine(NSpace, $"'{LeafTable}' current version = {version}");

            if (version <= 0)
            {
                // Don't set FirstRun in subsequence calls
                ConditionalDebug.WriteLine(NSpace, $"Register '{LeafTable}' current version = {version}");
                InsertVersion(con, tran, LeafTable, GardenLeaf.Version);
            }
            else
            if (GardenLeaf.UpgradeTableInternal(con, tran, version))
            {
                init = OpenInit.SchemaUpgraded;
                ConditionalDebug.WriteLine(NSpace, $"Upgrade '{LeafTable}' current version = {version}");
                UpdateVersion(con, tran, LeafTable, version);
            }

            tran.Commit();
            return init;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    private static DbCommand CreateCommand(DbConnection con, DbTransaction? tran, string sql, string table, int version = -1)
    {
        var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tran;

        cmd.AddParameter("@table_name", table);

        if (version > -1)
        {
            cmd.AddParameter("@version", version);
        }

        return cmd;
    }

    private GardenBin GetBin(GardenSession obj)
    {
        return GetBin(obj.HomeBin, obj.IsWaste);
    }

    private bool RemoveFromBins(GardenSession obj, bool raiseBin)
    {
        // Assumed container has changed, but we don't know old container.
        foreach (var item in _allBins)
        {
            if (item.Remove(obj, raiseBin))
            {
                return true;
            }
        }

        return false;
    }

    private void ThrowIfClosed()
    {
        if (!IsOpen)
        {
            throw new InvalidOperationException("Garden closed");
        }
    }
}