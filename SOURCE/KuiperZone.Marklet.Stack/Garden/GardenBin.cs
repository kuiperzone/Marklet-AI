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
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Provides a sequence of <see cref="GardenSession"/> items pertaining to a "bin" grouping.
/// </summary>
public sealed class GardenBin : IReadOnlyCollection<GardenSession>
{
    // Choice of SortedSet over HashSet deliberate.
    // The number of items are expected to be in 10s or 100s, not 10,000s.
    // Items are inserted from database in order and opening should be faster than HashSet.
    // Also the sorted order will minimise re-sorting with the Sort() method.
    private readonly SortedSet<GardenSession> _set = new();

    private IReadOnlyList<string?>? _topics;

    internal GardenBin(MemoryGarden owner, bool isWaste)
    {
        Owner = owner;
        IsWaste = isWaste;
    }

    /// <summary>
    /// Occurs when one or more items in this bin are modified, added or removed.
    /// </summary>
    /// <remarks>
    /// The event does not identify which instance has changed.
    /// </remarks>
    public event EventHandler<EventArgs>? Updated;

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public MemoryGarden Owner { get; }

    /// <summary>
    /// Gets whether this is the waste bin.
    /// </summary>
    public bool IsWaste { get; }

    /// <summary>
    /// Gets or sets the timeout period for an item to remain in this bin before being moved to <see
    /// cref="MemoryGarden.WasteBin"/> or, where <see cref="IsWaste"/> is true, to be deleted.
    /// </summary>
    /// <remarks>
    /// A zero or negative value disables this period. The default value is 0.
    /// </remarks>
    public TimeSpan Timeout { get; set; }

    /// <summary>
    /// Gets the item count.
    /// </summary>
    public int Count
    {
        get { return _set.Count; }
    }

    /// <summary>
    /// Gets whether "item" is conntained.
    /// </summary>
    public bool Contains(GardenSession item)
    {
        return _set.Contains(item);
    }

    /// <summary>
    /// Gets a collated sequence of unique <see cref="GardenSession.Topic"/> values.
    /// </summary>
    /// <remarks>
    /// Tags are collated and sorted case insensitively. Where two strings differ only in case, only one will be
    /// returned. A null string item is valid and indicates that one or more items exist in the bin with no <see
    /// cref="GardenSession.Topic"/> value.
    /// </remarks>
    public IReadOnlyCollection<string?> GetTopics()
    {
        if (_topics == null)
        {
            var temp = new HashSet<string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in _set)
            {
                temp.Add(item.Topic);
            }

            var list = temp.ToList();
            list.Sort(StringComparer.OrdinalIgnoreCase);
            _topics = list;
        }

        return _topics;
    }

    /// <summary>
    /// Returns a new sorted sequence of children.
    /// </summary>
    public List<GardenSession> GetSortedSessions(GardenSort sort)
    {
        return Sort(new(_set), sort);
    }

    /// <summary>
    /// Returns a new sorted sequence of children with filtered according "topic".
    /// </summary>
    /// <remarks>
    /// Topic names are compared case insensitively. A null or empty "topic" will return only untagged sessions.
    /// </remarks>
    public List<GardenSession> GetSortedSessions(string? topic, GardenSort sort)
    {
        topic = MemoryGarden.SanitizeName(topic);
        var clone = new List<GardenSession>(Math.Min(_set.Count, 8));

        foreach (var item in _set)
        {
            if (string.Equals(item.Topic, topic, StringComparison.OrdinalIgnoreCase))
            {
                clone.Add(item);
            }
        }

        return Sort(clone, sort);
    }

    /// <summary>
    /// Renames all children with a matching <see cref="GardenSession.Topic"/> and return true if one or more items
    /// were renamed.
    /// </summary>
    public bool RenameTopic(string? oldName, string? newName)
    {
        const string NSpace = $"{nameof(GardenBin)}.{nameof(RenameTopic)}";
        ConditionalDebug.WriteLine(NSpace, $"RENAME: '{oldName}' to: '{newName}'");

        if (_set.Count != 0)
        {
            ConditionalDebug.ThrowIfFalse(Owner.IsOpen);
            oldName = MemoryGarden.SanitizeName(oldName);
            newName = MemoryGarden.SanitizeName(newName);

            if (oldName != newName)
            {
                DbConnection? con = null;

                try
                {
                    foreach (var item in _set.ToArray())
                    {
                        ConditionalDebug.ThrowIfNotSame(Owner, item.Owner);

                        if (string.Equals(item.Topic, oldName, StringComparison.OrdinalIgnoreCase))
                        {
                            con ??= Owner.Gardener.Connect();
                            item.SetTopicNoRaise(con, newName);
                        }
                    }

                    OnChanged(con != null);
                    return con != null;
                }
                finally
                {
                    con?.Dispose();
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Deletes all children with matching <see cref="GardenSession.Topic"/> and returns true if one or more items were
    /// removed.
    /// </summary>
    /// <remarks>
    /// This may completely delete multiple sessions.
    /// </remarks>
    public bool DeleteTopic(string? name)
    {
        const string NSpace = $"{nameof(GardenBin)}.{nameof(DeleteTopic)}";
        ConditionalDebug.WriteLine(NSpace, $"DELETE TOPIC: {name}");

        if (_set.Count != 0)
        {
            ConditionalDebug.ThrowIfFalse(Owner.IsOpen);
            name = MemoryGarden.SanitizeName(name);

            DbConnection? con = null;

            try
            {
                foreach (var item in _set.ToArray())
                {
                    ConditionalDebug.ThrowIfNotSame(Owner, item.Owner);

                    if (string.Equals(item.Topic, name, StringComparison.OrdinalIgnoreCase))
                    {
                        con ??= Owner.Gardener.Connect();
                        item.DeleteDb(con, false);
                    }
                }

                OnChanged(con != null);
                return con != null;
            }
            finally
            {
                con?.Dispose();
            }
        }

        return false;
    }

    /// <summary>
    /// Implements <see cref="IEnumerable.GetEnumerator"/> and returns items in creation order according to <see
    /// cref="Zuid.Timestamp"/>.
    /// </summary>
    public IEnumerator<GardenSession> GetEnumerator()
    {
        // A common use of this may be to loop on items while changing
        // their properties. This may potentally change the bin contents
        // itself. Therefore, will we make a clone for iteration.
        return new List<GardenSession>(_set).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal bool Add(GardenSession item, bool raise)
    {
        ConditionalDebug.ThrowIfNull(item.Owner);
        ConditionalDebug.ThrowIfFalse(Owner.IsOpen);

        if (_set.Add(item))
        {
            OnChanged(raise);
            return true;
        }

        return false;
    }

    internal bool Remove(GardenSession item, bool raise)
    {
        if (_set.Remove(item))
        {
            OnChanged(raise);
            return true;
        }

        return false;
    }

    internal void OnChanged(bool raise)
    {
        _topics = null;

        if (raise)
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }

    internal void Close()
    {
        var temp = _set.ToArray();

        _set.Clear();

        foreach (var item in temp)
        {
            item.Discard();
        }

        OnChanged(temp.Length != 0);
    }

    /// <summary>
    /// Internal prune on bin. Append items to be moved to waste.
    /// Returns true only if items in waste have been deleted.
    /// </summary>
    internal bool Prune(List<GardenSession>? waste, ref long openLength)
    {
        const int MaxLength = 1024 * 1024;

        var now = DateTime.UtcNow;
        List<GardenSession>? deletes = null;

        foreach (var item in GetSortedSessions(GardenSort.AccessNewestFirst))
        {
            if (item.IsSelected)
            {
                // Ignore selected
                continue;
            }

            if (Timeout > TimeSpan.Zero && now > item.AccessTime + Timeout)
            {
                item.Close();

                if (IsWaste)
                {
                    deletes = new(8);
                    deletes.Add(item);
                    continue;
                }

                waste?.Add(item);
                continue;
            }

            if (item.IsOpen)
            {
                if (openLength >= MaxLength)
                {
                    // Free memory
                    item.Close();
                    continue;
                }

                openLength += item.TotalOpenLength;
            }
        }

        // Expected to be open here
        ConditionalDebug.ThrowIfFalse(Owner.IsOpen);

        if (deletes != null && Owner.IsOpen)
        {
            var con = Owner.Gardener.Connect();

            foreach (var item in deletes)
            {
                // No raise on child items
                item.DeleteDb(con, false);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Skips copy.
    /// </summary>
    internal IEnumerable<GardenSession> AsDirectEnumerable()
    {
        return _set;
    }

    private static List<GardenSession> Sort(List<GardenSession> clone, GardenSort sort)
    {
        switch (sort)
        {
            case GardenSort.CreationOldestFirst:
                // Already sorted on creation order
                return clone;
            case GardenSort.CreationNewestFirst:
                clone.Reverse();
                return clone;
            case GardenSort.UpdateOldestFirst:
                clone.Sort(UpdateOldestFirstCompare);
                return clone;
            case GardenSort.UpdateNewestFirst:
                clone.Sort(UpdateNewestFirstCompare);
                return clone;
            case GardenSort.AccessOldestFirst:
                clone.Sort(AccessOldestFirstCompare);
                return clone;
            case GardenSort.AccessNewestFirst:
                clone.Sort(AccessNewestFirstCompare);
                return clone;
            case GardenSort.Title:
                clone.Sort(TitleCompare);
                return clone;
            default:
                throw new ArgumentException($"Invalid {nameof(GardenSort)} {sort}", nameof(sort));
        }
    }

    private static int UpdateOldestFirstCompare(GardenSession x, GardenSession y)
    {
        return x.UpdateTime.CompareTo(y.UpdateTime);
    }

    private static int UpdateNewestFirstCompare(GardenSession x, GardenSession y)
    {
        return y.UpdateTime.CompareTo(x.UpdateTime);
    }

    private static int AccessOldestFirstCompare(GardenSession x, GardenSession y)
    {
        return x.AccessTime.CompareTo(y.AccessTime);
    }

    private static int AccessNewestFirstCompare(GardenSession x, GardenSession y)
    {
        return y.AccessTime.CompareTo(x.AccessTime);
    }

    private static int TitleCompare(GardenSession x, GardenSession y)
    {
        // Ordinal should be OK as NormC applied
        return StringComparer.OrdinalIgnoreCase.Compare(x.Title, y.Title);
    }

}