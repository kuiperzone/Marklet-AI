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

using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Provides a sequence of <see cref="GardenDeck"/> items pertaining to a "basket" grouping.
/// </summary>
public sealed partial class GardenBasket : IReadOnlyCollection<GardenDeck>
{
    private const int DefaultCapacity = 32;
    private const int DefaultFolderCapacity = 4;

    // Throttles memory cache per basket
    private const double CacheF = 0.025; // <- 2.5% of 16GB, = 410MB
    private const long MinCache = 100 * 1024 * 1024; // 100MB
    private static readonly long SystemMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    private static readonly double CacheBytes = Math.Max((long)(CacheF * SystemMemory), MinCache);

    // Choice of sorted over hash container deliberate. The number of items are expected to be in 100s, not
    // 10,000s. Items are inserted from database in order (or reverse order) and opening should be faster than HashSet.
    // Also the sort order avoids significant re-sorting with the Sort() method which is heavily used. Note we
    // actually store in reverse order to database, i.e. newest first for sorting reasons. We could keep a hashed
    // container if number becomes huge but this is not expected.
    private readonly IndexableSet<GardenDeck> _master = new(DefaultCapacity);

    private readonly IndexableSet<GardenDeck> _root = new(DefaultFolderCapacity);
    private readonly Dictionary<string, IndexableSet<GardenDeck>> _folders = new(DefaultFolderCapacity);

    /// <summary>
    /// Constructor.
    /// </summary>
    internal GardenBasket(MemoryGarden garden, BasketKind kind)
    {
        Diag.ThrowIfFalse(kind.IsLegal());
        Garden = garden;
        Kind = kind;
    }

    /// <summary>
    /// Clone constructor.
    /// </summary>
    internal GardenBasket(MemoryGarden garden, GardenBasket other)
    {
        Garden = garden;
        Kind = other.Kind;

        foreach(var item in other._master)
        {
            InsertCache(new GardenDeck(garden, item));
        }
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public MemoryGarden Garden { get; }

    /// <summary>
    /// Gets the basket identifier.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Gets the total child count including items which may not be visible to the UI.
    /// </summary>
    public int Count
    {
        get { return _master.Count; }
    }

    /// <summary>
    /// Gets the number of folders.
    /// </summary>
    public int FolderCount
    {
        get { return _folders.Count; }
    }

    /// <summary>
    /// Gets true if both <see cref="Count"/> and <see cref="FolderCount"/> are 0.
    /// </summary>
    public bool IsEmpty
    {
        get { return _master.Count == 0 && _folders.Count == 0; }
    }

    /// <summary>
    /// Gets the most recent focused instance in this basket.
    /// </summary>
    public GardenDeck? RecentFocused { get; private set; }

    /// <summary>
    /// Gets of value intended to approximate memory consumed by child items in bytes.
    /// </summary>
    /// <remarks>
    /// The figure is not necessarily intended to be accurate but provide a means to detect high usage only. The result
    /// is 0 when <see cref="Count"/> is 0. This is not something to be reported in UI as it would give inaccurate
    /// value.
    /// </remarks>
    public long GetFootprint()
    {
        long count = 0;

        foreach (var item in _master)
        {
            count += item.Footprint;
        }

        return count;
    }

    /// <summary>
    /// Gets whether this basket contains the given instance.
    /// </summary>
    public bool Contains([NotNullWhen(true)] GardenDeck? obj)
    {
        if (obj == null)
        {
            return false;
        }

        // Fast
        bool member = obj.Garden == Garden && obj.CurrentBasket == Kind;

        // Must have valid id
        Diag.ThrowIfTrue(member && obj.Id.IsEmpty);

        // Must be in master list or not
        Diag.ThrowIfTrue(member && !_master.Contains(obj));
        Diag.ThrowIfTrue(!member && _master.Contains(obj));

        // Must have a folder entry, even if just empty
        Diag.ThrowIfTrue(member && GetFolder(obj.Folder) == null);

        return member;
    }

    /// <summary>
    /// Finds based on the <see cref="GardenDeck.Id"/> or returns null.
    /// </summary>
    public GardenDeck? FindOnId(Zuid id)
    {
        // Find with stub instance
        if (!id.IsEmpty)
        {
            int index = _master.IndexOf(GardenDeck.CreateStub(id));

            if (index > -1)
            {
                return _master[index];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns whether the folder name exists.
    /// </summary>
    /// <remarks>
    /// An emptry string refers to "root" and always returns true.
    /// </remarks>
    public bool ContainsFolder(string? folderName)
    {
        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        return folderName == null || _folders.ContainsKey(folderName);
    }

    /// <summary>
    /// Gets a collated sequence of unique <see cref="GardenDeck.Folder"/> names.
    /// </summary>
    /// <remarks>
    /// Folders are collated and sorted case sensitively. The result does not include children with no folder.
    /// </remarks>
    public List<string> GetFolderNames()
    {
        var list = new List<string>(_folders.Keys);
        list.Sort(StringComparer.OrdinalIgnoreCase);
        return list;
    }

    /// <summary>
    /// Returns a new sorted sequence of all children.
    /// </summary>
    public List<GardenDeck> GetContents(GardenSort sort = GardenSort.Default)
    {
        return Sort(new(_master), sort);
    }

    /// <summary>
    /// Returns the count of instances with <see cref="GardenDeck.Folder"/> equal to "name".
    /// </summary>
    /// <remarks>
    /// Folder names are compared case sensitively. A null or empty "folderName" will return a count of root children.
    /// A non existant folder name returns 0.
    /// </remarks>
    public int CountInFolder(string? name)
    {
        return GetFolder(MemoryGarden.Sanitize(name, MemoryGarden.MaxMetaLength))?.Count ?? 0;
    }

    /// <summary>
    /// Returns the sorted sequence of children with <see cref="GardenDeck.Folder"/> equal to "name".
    /// </summary>
    /// <remarks>
    /// Folder names are compared case sensitively. A null or empty "name" will return root items. If the folder
    /// name does not exist, the result is null.
    /// </remarks>
    public List<GardenDeck>? GetFolderContents(string? name, GardenSort sort = GardenSort.Default)
    {
        return GetFolderContents(name, null, sort);
    }

    /// <summary>
    /// Returns the sorted sequence of children with <see cref="GardenDeck.Folder"/> equal to "name" while also
    /// restricting results to only those items which match the given <see cref="SearchOptions"/>.
    /// </summary>
    /// <remarks>
    /// Folder names are compared case sensitively. A null or empty "name" will return root items. If the folder name
    /// does not exist, the result is null. If the folder exists but no items meet the "match" criteria, the result is
    /// an empty list. If "match" is null, behaves as <see cref="GetFolderContents(string?, GardenSort)"/>.
    /// </remarks>
    public List<GardenDeck>? GetFolderContents(string? name, SearchOptions? match, GardenSort sort = GardenSort.Default)
    {
        var folder = GetFolder(MemoryGarden.Sanitize(name, MemoryGarden.MaxMetaLength));

        if (folder == null)
        {
            return null;
        }

        if (match == null)
        {
            return Sort(new List<GardenDeck>(folder), sort);
        }

        List<GardenDeck> list = new(16);

        if (match.MaxResults <= 0 || match.Keyword == null)
        {
            return list;
        }

        foreach (var item in folder)
        {
            if (item.SearchInContent(match))
            {
                list.Add(item);
            }
        }

        return Sort(list, sort);
    }

    /// <summary>
    /// Collates <see cref="GardenDeck"/> children on <see cref="GardenDeck.Folder"/>.
    /// </summary>
    /// <remarks>
    /// A new instance is returned on each call. The result does not include children with no folder (root). In each
    /// entry, items are sorted according to "sort".
    /// </remarks>
    public Dictionary<string, List<GardenDeck>> CollateByFolder(GardenSort sort = GardenSort.Default)
    {
        var dict = new Dictionary<string, List<GardenDeck>>(_folders.Count);

        foreach (var item in _folders)
        {
            dict.Add(item.Key, Sort(item.Value.ToList(), sort));
        }

        return dict;
    }

    /// <summary>
    /// Collates <see cref="GardenDeck"/> children on <see cref="GardenDeck.Folder"/>, while returning only those items
    /// which match the given <see cref="SearchOptions"/>.
    /// </summary>
    /// <remarks>
    /// A new instance is returned on each call. The result does not include children with no folder (root). In each
    /// entry, items are sorted according to "sort". If "match" is null, behaves as <see
    /// cref="CollateByFolder(GardenSort)"/>.
    /// </remarks>
    public Dictionary<string, List<GardenDeck>> CollateByFolder(SearchOptions? match, GardenSort sort = GardenSort.Default)
    {
        int count = 0;
        var dict = new Dictionary<string, List<GardenDeck>>(_folders.Count);

        if (match == null)
        {
            foreach (var item in _folders)
            {
                dict.Add(item.Key, Sort(item.Value.ToList(), sort));
            }

            return dict;
        }

        if (match.MaxResults <= 0 || match.Keyword == null)
        {
            return dict;
        }

        foreach (var item in _folders)
        {
            List<GardenDeck>? list = null;

            foreach (var deck in item.Value)
            {
                if (deck.SearchInContent(match))
                {
                    list ??= new(8);
                    list.Add(deck);

                    if (++count == match.MaxResults)
                    {
                        dict.Add(item.Key, Sort(list, sort));
                        return dict;
                    }
                }
            }

            if (list != null)
            {
                dict.Add(item.Key, Sort(list, sort));
            }
        }

        return dict;
    }

    /// <summary>
    /// Create an empty folder and returns true on success.
    /// </summary>
    /// <remarks>
    /// The folder is not written to storage until one or more items are added with <see cref="GardenDeck.Folder"/>
    /// equal to the name. Folders are naturally created by assigning <see cref="GardenDeck.Folder"/>. This method
    /// provides only a visual means of creating an initial folder in memory for the UI workflow.
    /// </remarks>
    public bool CreateFolder(string? folderName)
    {
        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength + 1);

        if (folderName == null || folderName.Length > MemoryGarden.MaxMetaLength)
        {
            return false;
        }

        return _folders.TryAdd(folderName, new(DefaultFolderCapacity));
    }

    /// <summary>
    /// Bulk renames <see cref="GardenDeck.Folder"/> on matching items and returns true if one or more items were
    /// renamed.
    /// </summary>
    /// <remarks>
    /// A null or empty "oldFolder" will match all items with no folder, otherwise "oldFolder" must exist. If "merge" is
    /// false, "newFolder" must not exist. If "allowMerge" is true and the new name exists, contents will be
    /// merged. The result false if names are equal.
    /// </remarks>
    public bool RenameFolder(string? oldFolder, string? newFolder, bool merge = false)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(RenameFolder)}";
        Diag.WriteLine(NSpace, $"RENAME FOLDER: '{oldFolder}' to: '{newFolder}'");

        if (IsEmpty)
        {
            Diag.WriteLine(NSpace, "Empty or closed");
            return false;
        }

        oldFolder = MemoryGarden.Sanitize(oldFolder, MemoryGarden.MaxMetaLength);
        newFolder = MemoryGarden.Sanitize(newFolder, MemoryGarden.MaxMetaLength);

        if (oldFolder == newFolder)
        {
            Diag.WriteLine(NSpace, "Names are same");
            return false;
        }

        if (!merge && (newFolder == null || _folders.ContainsKey(newFolder)))
        {
            Diag.WriteLine(NSpace, "Destination already exists");
            return false;
        }

        var source = GetFolder(oldFolder);

        if (source == null)
        {
            Diag.WriteLine(NSpace, "Folder not exist");
            return false;
        }

        // Get copy before clear
        var array = source.ToArray();

        if (oldFolder == null)
        {
            _root.Clear();
            _root.TrimCapacity(DefaultFolderCapacity);
        }
        else
        {
            _folders.Remove(oldFolder);
        }

        AddToFolderCache(newFolder, array);
        DbConnection? con = null;

        try
        {
            foreach (var item in array)
            {
                con ??= Garden.Provider?.Connect();
                item.SetFolderNoRaise(con, newFolder);
            }
        }
        catch (Exception e)
        {
            Diag.WriteLine(NSpace, e);
            Garden.SetStatus(GardenStatus.Lost);
            return false;
        }
        finally
        {
            con?.Dispose();
        }

        Garden.OnChanged(Kind);
        return true;
    }

    /// <summary>
    /// Moves items folder to another basket and returns true if changed.
    /// </summary>
    /// <remarks>
    /// Where and item folder pre-exists in the destination basket, items are merged. The result false if "dest" equals
    /// <see cref="Kind"/>.
    /// </remarks>
    public bool MoveBasket(string? folderName, BasketKind dest)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(MoveBasket)}";
        Diag.WriteLine(NSpace, $"MOVE FOLDER: {Kind}, {folderName}, {dest}");

        if (IsEmpty || Kind == dest)
        {
            return false;
        }

        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        var folder = GetFolder(folderName);

        if (folder == null)
        {
            Diag.WriteLine(NSpace, "Not found");
            return false;
        }

        if (folder.Count != 0)
        {
            try
            {
                using var con = Garden.Provider?.Connect();

                foreach (var item in folder.ToArray())
                {
                    // IMPORTANT
                    // This will call back on this class instance for the remove
                    item.SetBasketNoRaise(con, dest);
                }

                // Ensure this now empty
                Diag.ThrowIfTrue(folder.Count != 0);
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                Garden.Close();
                return false;
            }

            if (folderName != null)
            {
                _folders.Remove(folderName);
            }

            Garden.OnChanged(Kind);
            Garden.OnChanged(dest);
            return true;
        }

        // Remove empty folder
        if (folderName != null && _folders.Remove(folderName))
        {
            // Nothing on disk to remove
            Garden.OnChanged(Kind);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Restore items to their home folders and returns true if changed.
    /// </summary>
    public bool RestoreFolder(string? folderName)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(RestoreFolder)}";
        Diag.WriteLine(NSpace, $"MOVE FOLDER: {Kind}, {folderName}");

        if (IsEmpty)
        {
            return false;
        }

        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        var folder = GetFolder(folderName);

        if (folder == null)
        {
            Diag.WriteLine(NSpace, "Not found");
            return false;
        }

        if (folder.Count != 0)
        {
            var hash = new HashSet<BasketKind>();

            try
            {
                using var con = Garden.Provider?.Connect();

                foreach (var item in folder.ToArray())
                {
                    // IMPORTANT
                    // This will call back on this class instance for the remove
                    if (item.SetBasketNoRaise(con, item.OriginBasket))
                    {
                        hash.Add(item.OriginBasket);
                    }
                }
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                Garden.SetStatus(GardenStatus.Lost);
                return false;
            }

            foreach (var item in hash)
            {
                Garden.OnChanged(item);
            }

            if (folder.Count == 0 && folderName != null)
            {
                _folders.Remove(folderName);
            }

            if (hash.Count != 0)
            {
                Garden.OnChanged(Kind);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Remove children with matching <see cref="GardenDeck.Folder"/> and returns true if one or more items were removed.
    /// </summary>
    /// <remarks>
    /// This will nominally delete multiple items from the database. The folder entry itself is removed.
    /// </remarks>
    public bool DeleteFolder(string? folderName)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(DeleteFolder)}";
        Diag.WriteLine(NSpace, $"DELETE FOLDER: {Kind}, {folderName}");

        if (IsEmpty)
        {
            Diag.WriteLine(NSpace, "Empty");
            return false;
        }

        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        var folder = GetFolder(folderName);

        if (folder == null)
        {
            Diag.WriteLine(NSpace, "Not found");
            return false;
        }

        if (folder.Count != 0)
        {
            try
            {
                using var con = Garden.Provider?.Connect();

                // Safe to iterate on folder
                foreach (var item in folder)
                {
                    if (item == RecentFocused)
                    {
                        // IMPORTANT
                        // Needed here as we are not calling RemoveCache()
                        RecentFocused = null;
                    }

                    _master.Remove(item);
                    item.DeleteDb(con);
                }
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                Garden.SetStatus(GardenStatus.Lost);
                return false;
            }

            // Clear folder directly
            folder.Clear();

            if (folderName != null)
            {
                _folders.Remove(folderName);
            }

            Garden.OnChanged(Kind);
            return true;
        }

        // Remove empty folder
        if (folderName != null && _folders.Remove(folderName))
        {
            // Nothing on disk to remove
            Garden.OnChanged(Kind);
            return true;
        }


        return false;
    }

    /// <summary>
    /// Removes all children in the basket and returns true if one or more items were removed.
    /// </summary>
    /// <remarks>
    /// This will nominally delete multiple items from the database.
    /// </remarks>
    public bool DeleteAll()
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(DeleteAll)}";
        Diag.WriteLine(NSpace, $"DELETE BASKET: {Kind}");

        if (!IsEmpty)
        {
            try
            {
                using var con = Garden.Provider?.Connect();

                if (con != null)
                {
                    foreach (var item in _master)
                    {
                        item.DeleteDb(con);
                    }
                }
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                Garden.SetStatus(GardenStatus.Lost);
                return false;
            }

            // This does the memory removal
            ClearCache();
            Garden.OnChanged(Kind);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a count of the number of items to be removed.
    /// </summary>
    public int PruneCount(PruneOptions options)
    {
        int count = 0;

        foreach (var item in _master)
        {
            if (options.IsRemoveMatch(item))
            {
                count += 1;
            }
        }

        return count;
    }

    /// <summary>
    /// Prunes items and returns removal count.
    /// </summary>
    /// <remarks>
    /// Where "options" is null, the method may be used to reduce excessive memory use.
    /// </remarks>
    public int Prune(PruneOptions? options)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(Prune)}";
        Diag.WriteLine(NSpace, $"PRUNE BASKET: {Kind}, {options?.Period}");

        int count = 0;
        long used = 0;

        DbConnection? con = null;
        Diag.WriteLine(NSpace, "Deleting");

        try
        {
            // Sort order chosen for Unload() order
            foreach (var item in GetContents(GardenSort.UpdateNewestFirst))
            {
                if (options?.IsRemoveMatch(item) == true)
                {
                    Diag.ThrowIfNotEqual(Kind, item.CurrentBasket);

                    count += 1;
                    con ??= Garden.Provider?.Connect();

                    if (options.AlwaysDelete || Kind == BasketKind.Waste)
                    {
                        Diag.WriteLine(NSpace, "Delete: " + item);
                        RemoveCache(item);
                        item.DeleteDb(con);
                        continue;
                    }

                    Diag.WriteLine(NSpace, "Move to waste: " + item);
                    item.SetBasketNoRaise(con, BasketKind.Waste);
                }

                if (!item.IsFocused && used >= CacheBytes)
                {
                    // Free memory
                    item.Close();
                }

                used += item.Footprint;
            }

            con?.Dispose();

            if (count != 0)
            {
                Garden.OnChanged(Kind);
            }

            return count;
        }
        catch (Exception e)
        {
            Diag.WriteLine(NSpace, e);
            Garden.SetStatus(GardenStatus.Lost);
            return 0;
        }
        finally
        {
            con?.Dispose();
        }
    }

    /// <summary>
    /// Returns whether both instances in storage contain same data.
    /// </summary>
    /// <remarks>
    /// Unlike Equals(), this method calls <see cref="GardenDeck.IsConcordant(GardenDeck?)"/> on all child instances. It may be
    /// expensive in comparison to Equals().
    /// </remarks>
    public bool IsConcordant([NotNullWhen(true)] GardenBasket? other)
    {
        if (EqualsLite(other))
        {
            // We only need to check master here
            for (int n = 0; n < _master.Count; ++n)
            {
                if (!_master[n].IsConcordant(other._master[n]))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Implements <see cref="IEnumerable.GetEnumerator"/> and returns items in creation order according to <see
    /// cref="Zuid.Timestamp"/>.
    /// </summary>
    public IEnumerator<GardenDeck> GetEnumerator()
    {
        return _master.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _master.GetEnumerator();
    }
}