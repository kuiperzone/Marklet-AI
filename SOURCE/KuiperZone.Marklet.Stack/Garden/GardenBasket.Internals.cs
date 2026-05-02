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

using System.Diagnostics.CodeAnalysis;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

// INTERNALS (partial)
public sealed partial class GardenBasket : IReadOnlyCollection<GardenDeck>
{
    internal void SetRecentInternal(GardenDeck? obj)
    {
        Diag.ThrowIfTrue(obj != null && obj.CurrentBasket != Kind);
        RecentFocused = obj;
    }

    /// <summary>
    /// Insert "obj" into cache, but does not touch database.
    /// </summary>
    internal bool InsertCache(GardenDeck obj)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(InsertCache)}";
        Diag.WriteLine(NSpace, $"Insert to {Kind}");

        if (_master.Insert(obj) > -1)
        {
            AddToFolderCache(obj.Folder, obj);

            if (obj.IsFocused)
            {
                RecentFocused = obj;
            }

            Diag.WriteLine(NSpace, $"Inserted OK");
            return true;
        }

        Diag.WriteLine(NSpace, $"Failed");
        return false;
    }

    /// <summary>
    /// Removes from cache, but does not touch database.
    /// </summary>
    internal bool RemoveCache(GardenDeck obj)
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(RemoveCache)}";
        Diag.WriteLine(NSpace, obj);

        if (_master.Remove(obj))
        {
            Diag.WriteLine(NSpace, "Removed from master");

            if (obj == RecentFocused)
            {
                RecentFocused = null;
            }

            RemoveFromFolderCache(obj.Folder, obj, false);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clears cache, but does not touch database.
    /// </summary>
    internal bool ClearCache()
    {
        const string NSpace = $"{nameof(GardenBasket)}.{nameof(ClearCache)}";
        Diag.WriteLine(NSpace, $"Clear on: {Kind}");

        RecentFocused = null;

        if (!IsEmpty)
        {
            Diag.WriteLine(NSpace, "Not empty");

            foreach (var item in _master)
            {
                item.DetachInternal();
            }

            _master.Clear();
            _master.TrimCapacity(DefaultCapacity);

            _root.Clear();
            _folders.Clear();
            _root.TrimCapacity(DefaultFolderCapacity);

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
        Diag.WriteLine(NSpace, $"New folder: {newFolder}");
        Diag.WriteLine(NSpace, obj);

        Diag.ThrowIfEqual(obj.Folder, newFolder);
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
        var px = x.IsPinned;

        if (px != y.IsPinned)
        {
            return px ? -1 : 1;
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

    private bool EqualsLite([NotNullWhen(true)] GardenBasket? other)
    {
        return other != null &&
            Kind == other.Kind &&
            _master.Count == other._master.Count &&
            _root.Count == other._root.Count;
    }
}