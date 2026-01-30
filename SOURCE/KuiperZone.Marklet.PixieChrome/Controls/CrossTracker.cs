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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Handles selection across multiple blocks for <see cref="CrossTextBlock"/>.
/// </summary>
public sealed class CrossTracker
{
    private const int DefaultCapacity = 64;
    private readonly SortedList<ulong, ICrossTrackable> _children = new(DefaultCapacity);
    private readonly SortedList<ulong, ICrossTrackable> _selecting = new(DefaultCapacity);
    private static ulong s_nextKey;
    private ulong _dragKey;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CrossTracker()
    {
        Children = _children.Values;
        Selecting = _selecting.Values;
    }

    /// <summary>
    /// Gets the children in the tracker.
    /// </summary>
    public IEnumerable<ICrossTrackable> Children { get; }

    /// <summary>
    /// Gets the count of <see cref="Children"/>.
    /// </summary>
    public int Count
    {
        get { return _children.Count; }
    }

    /// <summary>
    /// Gets the current subset of <see cref="Children"/> that are undergoing selection.
    /// </summary>
    public IEnumerable<ICrossTrackable> Selecting { get; }

    /// <summary>
    /// Gets the count of <see cref="Selecting"/>.
    /// </summary>
    /// <remarks>
    /// This may report non-zero where <see cref="HasValidSelection"/> is false, as a child may be in the process of
    /// selecting.
    /// </remarks>
    public int SelectingCount
    {
        get { return _selecting.Count; }
    }

    /// <summary>
    /// Gets whether one or more child instances have selected text.
    /// </summary>
    public bool HasValidSelection
    {
        get
        {
            foreach (var item in _selecting.Values)
            {
                if (item.HasSelection)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Generates a new unique incrementing key value.
    /// </summary>
    /// <remarks>
    /// Assume single thread application use only.
    /// </remarks>
    public static ulong NextKey()
    {
        // Starts at 1 and never decrements
        // Assume not concurrent thread otherwise need Interlocked
        return ++s_nextKey;
    }

    /// <summary>
    /// Returns true if the child is managed by this tracker.
    /// </summary>
    public bool Contains([NotNullWhen(true)] ICrossTrackable? obj)
    {
        // This is fast as it avoids lookup
        if (obj != null)
        {
            // BUT CHECK KEY NON-ZERO FIRST!
            // When adding new, the state is in flux and tracker may already be set.
            if (obj.TrackKey != 0 && obj.Tracker == this)
            {
                // We expect it to be a member
                ConditionalDebug.ThrowIfFalse(_children.ContainsKey(obj.TrackKey));
                return true;
            }

            // We do not expect it to be a member
            ConditionalDebug.ThrowIfTrue(_children.ContainsKey(obj.TrackKey));
            return false;
        }

        return false;
    }

    /// <summary>
    /// Clears selected text for all children.
    /// </summary>
    public bool SelectNone()
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectNone)}";
        ConditionalDebug.WriteLine(NSpace, $"Clear selection count: {_selecting.Count}");

        _dragKey = 0;

        if (_selecting.Count != 0)
        {
            foreach (var item in _selecting.Values)
            {
                item.SelectInternal(0, 0);
            }

            _selecting.Clear();
            TrimCapacity(_selecting);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Selects a contiguous sequence of <see cref="Children"/>, from the given "firstKey" instance up to, and
    /// including, "lastKey". All text in each child within range is selected.
    /// </summary>
    /// <remarks>
    /// The result is the number of child blocks selected. Both "firstKey" and "lastKey" must be members of <see
    /// cref="Children"/>, otherwise the call does nothing and returns 0. The "firstKey" must be equal or less than the
    /// that of "lastKey", otherwise the result is 0. If "lastKey" is 0, however, all text in the first block only is
    /// selected. Any selection prior to the call is cleared by this call where the result not 0.
    /// </remarks>
    public int Select(ulong firstKey, ulong lastKey = 0)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(Select)}";
        ConditionalDebug.WriteLine(NSpace, $"Select from children: {_children.Count}");
        ConditionalDebug.WriteLine(NSpace, $"Keys: {firstKey}, {lastKey}");

        if (lastKey == 0U)
        {
            lastKey = firstKey;
        }

        if (firstKey == 0U || _children.Count == 0 || firstKey > lastKey ||
            !_children.ContainsKey(firstKey) || !_children.ContainsKey(lastKey))
        {
            ConditionalDebug.WriteLine(NSpace, $"Failed");
            return 0;
        }

        SelectNone();
        var view = GetViewBetween(firstKey, lastKey);

        foreach (var item in view.Values)
        {
            _selecting.Add(item.TrackKey, item);
            item.SelectInternal(0, item.TextLength);
        }

        ConditionalDebug.WriteLine(NSpace, $"Done {view.Count}");
        return view.Count;
    }

    /// <summary>
    /// Fully selects all children.
    /// </summary>
    public bool SelectAll()
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectAll)}";
        ConditionalDebug.WriteLine(NSpace, $"Select all children: {_children.Count}");

        _dragKey = 0;

        if (_children.Count != 0)
        {
            foreach (var item in _children.Values)
            {
                _selecting.TryAdd(item.TrackKey, item);
                item.SelectInternal(0, item.TextLength);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the text combined from all instances according to "what".
    /// </summary>
    public string? GetEffectiveText(WhatText what)
    {
        StringBuilder? buffer = null;
        var values = _selecting.Values;

        if (what == WhatText.All)
        {
            values = _children.Values;
        }

        foreach (var item in values)
        {
            buffer ??= new(64);

            if (buffer.Length != 0)
            {
                buffer.Append("\n\n");
            }

            buffer.Append(item.GetEffectiveText(what));
        }

        var text = buffer?.ToString();

        if (string.IsNullOrEmpty(text) && what == WhatText.SelectedOrAll)
        {
            return GetEffectiveText(WhatText.All);
        }

        return text;
    }

    /// <summary>
    /// Copies all or selected text to the clipboard according to "what" and returns true on success, or false if
    /// nothing was copied.
    /// </summary>
    public bool CopyText(WhatText what)
    {
        if (_children.Count != 0)
        {
            var text = GetEffectiveText(what);

            if (!string.IsNullOrEmpty(text) && _children.Values[0] is Control control)
            {
                return control.CopyToClipboard(text);
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a string summarizing contents for debug purposes only.
    /// </summary>
    internal string? GetDebugString()
    {
        if (_children.Count == 0)
        {
            return null;
        }

        var sb = new StringBuilder(128);

        foreach (var item in _children.Values)
        {
            if (sb.Length != 0)
            {
                sb.Append('\n');
            }

            if (_selecting.ContainsKey(item.TrackKey))
            {
                sb.Append("X ");
            }
            else
            {
                sb.Append("  ");
            }

            sb.Append(item.TrackKey);
            sb.Append(" : ");

            if (item.HasComplexContent && item is CrossTextBlock xblock)
            {
                sb.Append(Sanitizer.ToDebugSafe(xblock.Inlines!.Text, true, 32));
            }
            else
            {
                sb.Append(Sanitizer.ToDebugSafe(item.Text, true, 32));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Adds "obj" to <see cref="Children"/>.
    /// </summary>
    /// <exception cref="ArgumentException">TrackKey is 0, or duplicate</exception>
    internal void AddInternal(ICrossTrackable obj)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(AddInternal)}";
        ConditionalDebug.WriteLine(NSpace, $"ADDING OBJECT {nameof(obj.TrackKey)} = {obj.TrackKey}");

        if (obj.TrackKey == 0)
        {
            throw new ArgumentException($"{nameof(ICrossTrackable.TrackKey)} is 0");
        }

        // Will throw no duplicate
        _children.Add(obj.TrackKey, obj);

        // An unselected block could potentially go
        // into the middle of a selected region, so we always clear.
        SelectNone();
        obj.SelectInternal(0, 0);
    }

    internal bool RemoveInternal([NotNullWhen(true)] ICrossTrackable? obj)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(RemoveInternal)}";

        if (obj != null)
        {
            var key = obj.TrackKey;
            ConditionalDebug.WriteLine(NSpace, $"Removing {nameof(obj.TrackKey)} {key}");

            if (_children.Remove(key))
            {
                if (_dragKey != 0)
                {
                    // Clear all selections
                    SelectNone();
                }
                else
                {
                    _selecting.Remove(key);
                }

                TrimCapacity(_children);
                ConditionalDebug.WriteLine(NSpace, "Success OK");
                return true;
            }
        }

        // The Contains() should prevent this
        ConditionalDebug.Fail("Unexpected remove failure");
        return false;
    }

    internal bool SelectInternal(ICrossTrackable obj, int start, int end)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectInternal)}";
        ConditionalDebug.WriteLine(NSpace, $"SELECT START KEY: {obj.TrackKey}");
        ConditionalDebug.ThrowIfFalse(_children.ContainsKey(obj.TrackKey));

        if (_selecting.Count == 1 && _selecting.ContainsKey(obj.TrackKey))
        {
            return obj.SelectInternal(start, end);
        }

        bool rslt = SelectNone();

        if (obj.SelectInternal(start, end))
        {
            _selecting.Add(obj.TrackKey, obj);
            return true;
        }

        return rslt;
    }

    internal void StartSelect(ICrossTrackable obj, bool dragging)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(StartSelect)}";
        ConditionalDebug.WriteLine(NSpace, $"SELECT START KEY: {obj.TrackKey}");
        ConditionalDebug.ThrowIfFalse(_children.ContainsKey(obj.TrackKey));

        SelectNone();
        _selecting.Add(obj.TrackKey, obj);
        _dragKey = dragging ? obj.TrackKey : 0;
    }

    internal DragDirection DragSelect(ICrossTrackable captured)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(DragSelect)}";
        ConditionalDebug.WriteLine(NSpace, $"DRAG START KEY: {captured.TrackKey}");
        ConditionalDebug.WriteLine(NSpace, $"Current count: {_selecting.Count}");

        ConditionalDebug.ThrowIfFalse(_children.ContainsKey(captured.TrackKey));

        // Must already have at least one selection
        if (_selecting.Count != 0)
        {
            ConditionalDebug.WriteLine(NSpace, $"Capture selecting: {captured.HasSelection}");
            bool topDown = captured.TrackKey > _dragKey;

            if (captured.HasSelection)
            {
                topDown = !topDown;
            }

            _dragKey = captured.TrackKey;
            var top = _selecting.Values[0];
            var bottom = _selecting.Values[^1];
            ConditionalDebug.WriteLine(NSpace, $"Initial range: [{top.TrackKey}, {bottom.TrackKey}]");

            if (captured.TrackKey > bottom.TrackKey)
            {
                bottom = captured;
            }
            else
            if (topDown)
            {
                bottom = captured;
            }
            else
            {
                top = captured;
            }

            var view = GetViewBetween(top.TrackKey, bottom.TrackKey);
            ConditionalDebug.WriteLine(NSpace, $"Range now: [{view.Values.First().TrackKey}, {view.Values.Last().TrackKey}]");

            foreach (var item in _selecting.Values)
            {
                if (!view.ContainsKey(item.TrackKey))
                {
                    ConditionalDebug.WriteLine(NSpace, $"Deselect item: {item.TrackKey}");
                    item.SelectInternal(0, 0);
                }
            }

            _selecting.Clear();
            top = view.Values[0];
            bottom = view.Values[^1];

            foreach (var item in view.Values)
            {
                _selecting.Add(item.TrackKey, item);

                if (item == captured)
                {
                    // Skip - has capture and will set its own
                    continue;
                }

                if (item == top)
                {
                    int start = Math.Min(item.SelectionStart, item.SelectionEnd);
                    item.SelectInternal(start, item.TextLength);
                    continue;
                }

                if (item == bottom)
                {
                    int end = Math.Max(item.SelectionStart, item.SelectionEnd);
                    item.SelectInternal(0, end);
                    continue;
                }

                item.SelectInternal(0, item.TextLength);
            }

            // This was mental!
            ConditionalDebug.WriteLine(NSpace, $"topDown: {topDown}");
            ConditionalDebug.WriteLine(NSpace, $"New count: {_selecting.Count}");

            if (_selecting.Count > 1)
            {
                return topDown ? DragDirection.LeftToRight : DragDirection.RightToLeft;
            }

            return topDown ? DragDirection.FromStart : DragDirection.FromEnd;
        }

        ConditionalDebug.Fail("Unexpected lack of selection");
        return DragDirection.FromStart;
    }

    internal bool RemoveSelection(ICrossTrackable obj)
    {
        ConditionalDebug.ThrowIfFalse(_children.ContainsKey(obj.TrackKey));

        if (obj.TrackKey == _dragKey)
        {
            _dragKey = 0;
        }

        return _selecting.Remove(obj.TrackKey);
    }

    private static void TrimCapacity(SortedList<ulong, ICrossTrackable> list)
    {
        if (list.Count < DefaultCapacity && list.Capacity > DefaultCapacity)
        {
            list.TrimExcess();
        }
    }

    private SortedList<ulong, ICrossTrackable> GetViewBetween(ulong firstKey, ulong lastKey)
    {
        if (firstKey > lastKey)
        {
            (firstKey, lastKey) = (lastKey, firstKey);
        }

        var view = new SortedList<ulong, ICrossTrackable>();

        foreach (var item in _children.Values)
        {
            if (item.TrackKey >= firstKey)
            {
                if (item.TrackKey > lastKey)
                {
                    return view;
                }

                view.Add(item.TrackKey, item);
            }
        }

        return view;
    }

}