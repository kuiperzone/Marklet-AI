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
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Provides a sequence of <see cref="GardenDeck"/> items pertaining to a "basket" grouping.
/// </summary>
public sealed class GardenBasket : IReadOnlyCollection<GardenDeck>
{
    private const int DefaultCapacity = 32;
    private const int DefaultFolderCapacity = 4;

    // Choice of sorted over hash container deliberate. The number of items are expected to be in 10s or 100s, not
    // 10,000s. Items are inserted from database in order (or reverse order) and opening should be faster than HashSet.
    // Also the sorted order avoids significant re-sorting with the Sort() method which is heavily used. Note we
    // actually store in reverse order to database, i.e. newest first for sorting reasons. We could keep a hashed
    // container if number becomes huge but this is not expected.
    private readonly IndexableSet<GardenDeck> _master = new(DefaultCapacity);

    private readonly IndexableSet<GardenDeck> _root = new(8);
    private readonly Dictionary<string, IndexableSet<GardenDeck>> _folders = new(DefaultFolderCapacity);

    internal GardenBasket(MemoryGarden garden, BasketKind kind)
    {
        ConditionalDebug.ThrowIfFalse(kind.IsLegal());
        Garden = garden;
        Kind = kind;
    }

    /// <summary>
    /// Occurs when one or more items in this basket are modified, added or removed.
    /// </summary>
    /// <remarks>
    /// The event does not identify which instance has changed.
    /// </remarks>
    public event EventHandler<EventArgs>? Changed;

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
        bool member = obj.Garden == Garden && obj.Basket == Kind;

        // Must have valid id
        ConditionalDebug.ThrowIfTrue(member && obj.Id.IsEmpty);

        // Must be in master list or not
        ConditionalDebug.ThrowIfTrue(member && !_master.Contains(obj));
        ConditionalDebug.ThrowIfTrue(!member && _master.Contains(obj));

        // Must have a folder entry, even if just empty
        ConditionalDebug.ThrowIfTrue(member && GetFolder(obj.Folder) == null);

        return member;
    }

    /// <summary>
    /// Finds the child in this basket or returns null.
    /// </summary>
    public GardenDeck? Find(Zuid id)
    {
        // Find with stub instance
        return FindInternal(new GardenDeck(id));
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
        var folder = GetFolder(MemoryGarden.Sanitize(name, MemoryGarden.MaxMetaLength));

        if (folder == null)
        {
            return null;
        }

        return Sort(new List<GardenDeck>(folder), sort);
    }

    /// <summary>
    /// Returns the sorted sequence of children with <see cref="GardenDeck.Folder"/> equal to "name" while
    /// returning only those items which match the given <see cref="SearchOptions"/>.
    /// </summary>
    /// <remarks>
    /// Folder names are compared case sensitively. A null or empty "name" will return root items. If the folder
    /// name does not exist, the result is null. If the folder exists but no items meet the "match" criteria, the result
    /// is an empty list. If "match" is null, behaves as <see cref="GetFolderContents(string?, GardenSort)"/>.
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

        List<GardenDeck> list = new(32);

        if (match.MaxResults <= 0 || match.Subtext.Length == 0)
        {
            return list;
        }

        foreach (var item in folder)
        {
            if (item.SearchContent(match))
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

        if (match.MaxResults <= 0 || match.Subtext.Length == 0)
        {
            return dict;
        }

        foreach (var item in _folders)
        {
            List<GardenDeck>? list = null;

            foreach (var deck in item.Value)
            {
                if (deck.SearchContent(match))
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
    public bool NewEmptyFolder(string? folderName)
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
    /// A null or empty folder name will match all items with no folder. The "oldName" must exist. If "allowMerge" is
    /// false, the "newFolder" must not exist". If "allowMerge" is true and the new name exists, contents will be
    /// merged. The result false if names are equal.
    /// </remarks>
    public bool RenameFolders(string? oldFolder, string? newFolder, bool allowMerge = false)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(RenameFolders)}";
        ConditionalDebug.WriteLine(NSpace, $"RENAME FOLDER: '{oldFolder}' to: '{newFolder}'");

        if (IsEmpty)
        {
            ConditionalDebug.WriteLine(NSpace, "Empty or closed");
            return false;
        }

        oldFolder = MemoryGarden.Sanitize(oldFolder, MemoryGarden.MaxMetaLength);
        newFolder = MemoryGarden.Sanitize(newFolder, MemoryGarden.MaxMetaLength);

        if (oldFolder == newFolder)
        {
            ConditionalDebug.WriteLine(NSpace, "Names are same");
            return false;
        }

        if (!allowMerge && (newFolder == null || _folders.ContainsKey(newFolder)))
        {
            ConditionalDebug.WriteLine(NSpace, "Destination already exists");
            return false;
        }

        var source = GetFolder(oldFolder);

        if (source == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Folder not exist");
            return false;
        }

        // Get copy before clear
        var array = source.ToArray();

        if (oldFolder == null)
        {
            _root.Clear();
            _root.TrimCapacity();
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
                con ??= Garden.Gardener?.Connect();
                item.SetFolderNoRaise(con, newFolder);
            }
        }
        finally
        {
            con?.Dispose();
        }

        OnChangedInternal(true);
        return true;
    }

    /// <summary>
    /// Moves items folder to another basket and returns true if changed.
    /// </summary>
    /// <remarks>
    /// Where and item folder pre-exists in the destination basket, items are merged. The result false if "destination"
    /// equals <see cref="Kind"/>.
    /// </remarks>
    public bool MoveBasket(string? folderName, BasketKind destination)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(MoveBasket)}";
        ConditionalDebug.WriteLine(NSpace, $"MOVE FOLDER: {Kind}, {folderName}, {destination}");

        if (IsEmpty || Kind == destination || Garden.Gardener == null)
        {
            return false;
        }

        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        var folder = GetFolder(folderName);

        if (folder == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Not found");
            return false;
        }

        if (folder.Count != 0)
        {
            using var con = Garden.Gardener.Connect();

            foreach (var item in folder.ToArray())
            {
                // This will call back on this class instance for the remove
                item.SetBasketNoRaise(con, destination);
            }

            // Ensure this now empty
            con.Dispose();
            ConditionalDebug.ThrowIfTrue(folder.Count != 0);

            if (folderName != null)
            {
                _folders.Remove(folderName);
            }

            OnChangedInternal(true);
            Garden.GetBasket(destination).OnChangedInternal(true);
            return true;
        }

        // Remove empty folder
        if (folderName != null && _folders.Remove(folderName))
        {
            // Nothing on disk to remove
            OnChangedInternal(true);
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
        ConditionalDebug.WriteLine(NSpace, $"MOVE FOLDER: {Kind}, {folderName}");

        if (IsEmpty)
        {
            return false;
        }

        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        var folder = GetFolder(folderName);

        if (folder == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Not found");
            return false;
        }

        if (folder.Count != 0)
        {
            var hash = new HashSet<BasketKind>();
            using var con = Garden.Gardener?.Connect();

            foreach (var item in folder.ToArray())
            {
                // This will call back on this class instance for the remove
                if (item.SetBasketNoRaise(con, item.Origin))
                {
                    hash.Add(item.Origin);
                }
            }

            foreach (var item in hash)
            {
                Garden.GetBasket(item).OnChangedInternal(true);
            }

            if (folder.Count == 0 && folderName != null)
            {
                _folders.Remove(folderName);
            }

            if (hash.Count != 0)
            {
                OnChangedInternal(true);
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
        ConditionalDebug.WriteLine(NSpace, $"DELETE FOLDER: {Kind}, {folderName}");

        if (IsEmpty)
        {
            ConditionalDebug.WriteLine(NSpace, "Empty");
            return false;
        }

        folderName = MemoryGarden.Sanitize(folderName, MemoryGarden.MaxMetaLength);
        var folder = GetFolder(folderName);

        if (folder == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Not found");
            return false;
        }

        if (folder.Count != 0)
        {
            using var con = Garden.Gardener?.Connect();

            try
            {
                // Safe to iterate on folder provided "remove" below is false
                foreach (var item in folder)
                {
                    _master.Remove(item);
                    item.DeleteDb(con);
                }

                folder.Clear();

                if (folderName != null)
                {
                    _folders.Remove(folderName);
                }

                con?.Dispose();
                OnChangedInternal(true);
                return true;
            }
            finally
            {
                con?.Dispose();
            }
        }

        // Remove empty folder
        if (folderName != null && _folders.Remove(folderName))
        {
            // Nothing on disk to remove
            OnChangedInternal(true);
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
        ConditionalDebug.WriteLine(NSpace, $"DELETE BASKET: {Kind}");

        if (!IsEmpty)
        {
            using var con = Garden.Gardener?.Connect();

            if (con != null)
            {
                foreach (var item in _master)
                {
                    item.DeleteDb(con);
                }
            }

            // This does the memory removal
            ClearCache(true);
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
        ConditionalDebug.WriteLine(NSpace, $"PRUNE BASKET: {Kind}, {options?.Period}");

        int count = 0;
        long used = 0;
        long alloc = Garden.BasketAlloc;

        DbConnection? con = null;
        ConditionalDebug.WriteLine(NSpace, "Deleting");

        try
        {
            // Sort order chosen for Unload() order
            foreach (var item in GetContents(GardenSort.UpdateNewestFirst))
            {
                if (options?.IsRemoveMatch(item) == true)
                {
                    ConditionalDebug.ThrowIfNotEqual(Kind, item.Basket);

                    count += 1;
                    con ??= Garden.Gardener?.Connect();

                    if (options.AlwaysDelete || Kind == BasketKind.Waste)
                    {
                        ConditionalDebug.WriteLine(NSpace, "Delete: " + item);
                        RemoveCache(item, false);
                        item.DeleteDb(con);
                        continue;
                    }

                    ConditionalDebug.WriteLine(NSpace, "Move to waste: " + item);
                    item.SetBasketNoRaise(con, BasketKind.Waste);
                }

                if (!item.IsCurrent && used >= alloc)
                {
                    // Free memory
                    item.TryUnload();
                }

                used += item.Footprint;
            }

            con?.Dispose();

            if (count != 0)
            {
                OnChangedInternal(true);
            }

            return count;
        }
        finally
        {
            con?.Dispose();
        }
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

    internal GardenDeck? FindInternal(GardenDeck stub)
    {
        // Binary search
        int index = _master.IndexOf(stub);

        if (index > -1)
        {
            return _master[index];
        }

        return null;
    }

    internal void OnChangedInternal(bool raise)
    {
        if (raise)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Insert in cache, but does not touch database.
    /// </summary>
    internal bool InsertCache(GardenDeck obj, bool raise)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(InsertCache)}";
        ConditionalDebug.WriteLine(NSpace, $"Insert on: {Kind}");
        ConditionalDebug.ThrowIfNotSame(obj.Garden, Garden);

        if (_master.Insert(obj) > -1)
        {
            AddToFolderCache(obj.Folder, obj);
            OnChangedInternal(raise);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes from cache, but does not touch database.
    /// </summary>
    internal bool RemoveCache(GardenDeck obj, bool raise)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(RemoveCache)}";
        ConditionalDebug.WriteLine(NSpace, obj);

        if (_master.Remove(obj))
        {
            ConditionalDebug.WriteLine(NSpace, "Removed from master");
            RemoveFromFolderCache(obj.Folder, obj, false);
            OnChangedInternal(raise);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clears cache, but does not touch database.
    /// </summary>
    internal bool ClearCache(bool raise)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(ClearCache)}";
        ConditionalDebug.WriteLine(NSpace, $"Clear on: {Kind}");

        if (!IsEmpty)
        {
            ConditionalDebug.WriteLine(NSpace, "Not empty");

            foreach (var item in _master)
            {
                item.DetachInternal();
            }

            _master.Clear();
            _master.TrimCapacity(DefaultCapacity);

            _folders.Clear();
            _root.Clear();
            _root.TrimCapacity(DefaultFolderCapacity);

            OnChangedInternal(raise);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves folder in cache, but does not touch database.
    /// </summary>
    internal void MoveFolderCache(GardenDeck obj, string? newFolder)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(MoveFolderCache)}";
        ConditionalDebug.WriteLine(NSpace, $"New folder: {newFolder}");
        ConditionalDebug.WriteLine(NSpace, obj);

        ConditionalDebug.ThrowIfEqual(obj.Folder, newFolder);
        RemoveFromFolderCache(obj.Folder, obj, false);
        AddToFolderCache(newFolder, obj);
    }

    private static List<GardenDeck> Sort(List<GardenDeck> clone, GardenSort sort)
    {
        switch (sort)
        {
            case GardenSort.Default:
            case GardenSort.CreationNewestFirst:
                // Already sorted on creation order
                return clone;
            case GardenSort.CreationOldestFirst:
                clone.Reverse();
                return clone;
            case GardenSort.UpdateNewestFirst:
                clone.Sort(UpdateNewestFirstCompare);
                return clone;
            case GardenSort.UpdateNewestPinnedFirst:
                clone.Sort(UpdateNewestPinnedFirstCompare);
                return clone;
            case GardenSort.UpdateOldestFirst:
                clone.Sort(UpdateOldestFirstCompare);
                return clone;
            case GardenSort.Title:
                clone.Sort(TitleCompare);
                return clone;
            default:
                throw new ArgumentException($"Invalid {nameof(GardenSort)} {sort}", nameof(sort));
        }
    }

    private static int UpdateNewestFirstCompare(GardenDeck x, GardenDeck y)
    {
        return y.Updated.CompareTo(x.Updated);
    }

    private static int UpdateNewestPinnedFirstCompare(GardenDeck x, GardenDeck y)
    {
        if (x.IsPinned != y.IsPinned)
        {
            return x.IsPinned ? -1 : 1;
        }

        return y.Updated.CompareTo(x.Updated);
    }

    private static int UpdateOldestFirstCompare(GardenDeck x, GardenDeck y)
    {
        return x.Updated.CompareTo(y.Updated);
    }

    private static int TitleCompare(GardenDeck x, GardenDeck y)
    {
        // Ordinal should be OK as NormC applied
        return StringComparer.OrdinalIgnoreCase.Compare(x.Title, y.Title);
    }

    private IndexableSet<GardenDeck>? GetFolder(string? key)
    {
        if (key == null)
        {
            return _root;
        }

        if (_folders.TryGetValue(key, out IndexableSet<GardenDeck>? folder))
        {
            return folder;
        }

        return null;
    }

    private void AddToFolderCache(string? key, GardenDeck obj)
    {
        IndexableSet<GardenDeck>? folder = _root;

        if (key != null && !_folders.TryGetValue(key, out folder))
        {
            folder = new(DefaultFolderCapacity);
            _folders.Add(key, folder);
        }

        folder.Insert(obj);
    }

    private void AddToFolderCache(string? key, IEnumerable<GardenDeck> array)
    {
        IndexableSet<GardenDeck>? folder = _root;

        if (key != null && !_folders.TryGetValue(key, out folder))
        {
            folder = new(DefaultFolderCapacity);
            _folders.Add(key, folder);
        }

        foreach (var item in array)
        {
            folder.Insert(item);
        }
    }

    private bool RemoveFromFolderCache(string? key, GardenDeck obj, bool removeIfEmpty)
    {
        IndexableSet<GardenDeck>? folder = _root;

        if (key != null && !_folders.TryGetValue(key, out folder))
        {
            // Folder not found
            return false;
        }

        if (folder.Remove(obj))
        {
            if (removeIfEmpty && key != null && folder.Count == 0)
            {
                _folders.Remove(key);
            }

            return true;
        }

        // Not found in folder
        return false;
    }

}