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
using System.Collections.ObjectModel;

namespace KuiperZone.Marklet.Stack;

/// <summary>
/// Maintains an indexable sorted set of "T".
/// </summary>
/// <remarks>
/// Items are sorted according to <see cref="IComparable{T}"/>, which is used also for equality.
/// I.e. multiple items for which <see cref="IComparable{T}"/> returns 0 cannot exist.
/// </remarks>
public class IndexableSet<T> : IReadOnlyList<T>,
    IReadOnlyCollection<T>, IEnumerable<T>
    where T : IComparable<T>
{
    private const int DefaultCapacity = 8;
    private readonly List<T> _list;

    /// <summary>
    /// Constructor.
    /// </summary>
    public IndexableSet()
    {
        _list = new(DefaultCapacity);
    }

    /// <summary>
    /// Constructor with capacity.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IndexableSet(int capacity)
    {
        _list = new(capacity);
    }

    /// <summary>
    /// Constructor with initial values.
    /// </summary>
    /// <remarks>
    /// The <see cref="InsertMany"/> method is used to load values so that repeated items are excluded.
    /// </remarks>
    public IndexableSet(IEnumerable<T> values)
    {
        _list = new(DefaultCapacity);
        InsertMany(values);
    }

    /// <summary>
    /// Constructor with initial values.
    /// </summary>
    /// <remarks>
    /// The <see cref="InsertMany"/> method is used to load values so that repeated items are excluded.
    /// </remarks>
    public IndexableSet(ICollection<T> values)
    {
        _list = new(values.Count);
        InsertMany(values);
    }

    /// <summary>
    /// Constructor with initial values.
    /// </summary>
    public IndexableSet(IndexableSet<T> values)
    {
        _list = new(values);
    }

    /// <summary>
    /// Indexer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">n</exception>
    public T this[int index]
    {
        get { return _list[index]; }
    }

    /// <summary>
    /// Gets the number items contained.
    /// </summary>
    public int Count
    {
        get { return _list.Count; }
    }

    /// <summary>
    /// Returns true if empty.
    /// </summary>
    public bool IsEmpty
    {
        get { return _list.Count == 0; }
    }

    /// <summary>
    /// Gets or sets the container's capacity.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int Capacity
    {
        get { return _list.Capacity; }
        set { _list.Capacity = value; }
    }

    /// <summary>
    /// Clears the container.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Trims excess capacity.
    /// </summary>
    public void TrimExcess()
    {
        _list.TrimExcess();
    }

    /// <summary>
    /// Calls <see cref="TrimExcess"/> only if <see cref="Capacity"/> exceed <see cref="Count"/> by "delta".
    /// </summary>
    public void TrimCapacity(int delta = 32)
    {
        if (_list.Capacity - _list.Count > delta)
        {
            _list.TrimExcess();
        }
    }

    /// <summary>
    /// Returns true if the container contains "item".
    /// </summary>
    public bool Contains(T item)
    {
        return IndexOf(item) > -1;
    }

    /// <summary>
    /// Returns a read-only wrapper for the current collection.
    /// </summary>
    public ReadOnlyCollection<T> AsReadOnly()
    {
        return _list.AsReadOnly();
    }

    /// <summary>
    /// Returns the first item, or throws if empty.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">empty</exception>
    public T First()
    {
        return _list[0];
    }

    /// <summary>
    /// Returns the last item, or throws if empty.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">empty</exception>
    public T Last()
    {
        return _list[^1];
    }

    /// <summary>
    /// Returns either the last where "last" is true, or the first otherwise.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">empty</exception>
    public T End(bool last)
    {
        return last ? _list[^1] : _list[0];
    }

    /// <summary>
    /// Returns the index position of "item", or a negative value (not necessarily -1) if the item does not exist.
    /// </summary>
    /// <remarks>
    /// If not found, the result is a negative number that is the bitwise complement of the index of the next element
    /// that is larger than item or, if there is no larger element, the bitwise complement of <see cref="Count"/>.
    /// </remarks>
    public int IndexOf(T item)
    {
        int count = _list.Count;

        if (count == 0)
        {
            return -1;
        }

        int comp = item.CompareTo(_list[^1]);

        if (comp >= 0)
        {
            return comp > 0 ? ~count : count - 1;
        }

        if (count > 1)
        {
            comp = item.CompareTo(_list[0]);

            if (comp <= 0)
            {
                return comp < 0 ? -1 : 0;
            }
        }

        return _list.BinarySearch(item);
    }

    /// <summary>
    /// Inserts "item" into the container and returns the insert position, or -1 if item already exists.
    /// </summary>
    public int Insert(T item)
    {
        int index = IndexOf(item);

        if (index < 0)
        {
            index = ~index;
            _list.Insert(index, item);
            return index;
        }

        return -1;
    }

    /// <summary>
    /// Inserts the "items" into the container and returns the number of items inserted.
    /// </summary>
    /// <remarks>
    /// Where one or more items already exist, the return count will be less than the count of
    /// "items".
    /// </remarks>
    public int InsertMany(IEnumerable<T> items)
    {
        int count = 0;

        foreach (var item in items)
        {
            int index = IndexOf(item);

            if (index < 0)
            {
                _list.Insert(~index, item);
                count += 1;
            }
        }

        return count;
    }

    /// <summary>
    /// Upserts "item" into the container and returns the insert position.
    /// </summary>
    /// <remarks>
    /// Any existing item is replaced.
    /// </remarks>
    public int Upsert(T item)
    {
        int index = IndexOf(item);

        if (index < 0)
        {
            index = ~index;
            _list.Insert(index, item);
            return index;
        }

        if (_list.Count == 0)
        {
            _list.Add(item);
            return 0;
        }

        _list[index] = item;
        return index;
    }

    /// <summary>
    /// Upserts the "items" into the container and returns the count of items.
    /// </summary>
    /// <remarks>
    /// Existing items are replaced.
    /// </remarks>
    public int UpsertMany(IEnumerable<T> items)
    {
        int count = 0;

        foreach (var item in items)
        {
            count += 1;
            int index = IndexOf(item);

            if (index < 0)
            {
                _list.Insert(~index, item);
                continue;
            }

            if (_list.Count == 0)
            {
                _list.Add(item);
                continue;
            }

            _list[index] = item;
        }

        return count;
    }

    /// <summary>
    /// Removes the given "item" and returns true on success.
    /// </summary>
    public bool Remove(T item)
    {
        int index = IndexOf(item);

        if (index > -1)
        {
            _list.RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the element at the given index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">index</exception>
    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    /// <summary>
    /// Remove either last or first item and returns the item removed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Empty</exception>
    public T RemoveEnd(bool last)
    {
        var rem = End(last);
        _list.RemoveAt(last ? _list.Count - 1 : 0);
        return rem;
    }

    /// <summary>
    /// Removes a range of elements from this list.
    /// </summary>
    /// <exception cref="ArgumentException">index and count do not denote a valid range of element</exception>
    /// <exception cref="ArgumentOutOfRangeException">index or count less than 0</exception>
    public void RemoveRange(int index, int count)
    {
        _list.RemoveRange(index, count);
    }

    /// <summary>
    /// Removes elements from this list starting from "index".
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">index less than 0 or greater than count</exception>
    public void RemoveFrom(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _list.Count);
        _list.RemoveRange(index, _list.Count - index);
    }

    /// <summary>
    /// Removes many elements, and returns the number removed.
    /// </summary>
    /// <remarks>
    /// Items not located are ignored, therefore the return value may be less than the input
    /// sequence count.
    /// </remarks>
    public int RemoveMany(IEnumerable<T> items)
    {
        int count = 0;

        foreach (var item in items)
        {
            int index = IndexOf(item);

            if (index > -1)
            {
                count += 1;
                _list.RemoveAt(index);
            }
        }

        return count;
    }

    /// <summary>
    /// Copies elements and returns them as a new List.
    /// </summary>
    public List<T> ToList()
    {
        return new(_list);
    }

    /// <summary>
    /// Copies elements and returns them as a new array.
    /// </summary>
    public T[] ToArray()
    {
        return _list.ToArray();
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}
