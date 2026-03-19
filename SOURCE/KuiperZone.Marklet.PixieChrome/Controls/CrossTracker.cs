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
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Handles selection across multiple blocks for <see cref="CrossTextBlock"/>.
/// </summary>
/// <remarks>
/// Intended to be efficient as will need to handle 100s, if not 1000s, of items.
/// </remarks>
public sealed class CrossTracker
{
    private const int DefaultCapacity = 128;
    private readonly HashSet<ICrossTrackable> _children = new(DefaultCapacity);
    private int _anchorPos;
    private ICrossTrackable? _anchorObj;
    private HashSet<ICrossTrackable>? _selection;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// The "container" should be the control under which all children will live.
    /// </remarks>
    public CrossTracker(Control container)
    {
        Children = _children;
        Container = container;
    }

    /// <summary>
    /// Occurs when the user clicks on an URI within the text.
    /// </summary>
    /// <remarks>
    /// When a link is clicked, the default behaviour is to invoke this event. When it returns, if <see
    /// cref="LinkClickEventArgs.Handled"/> is false the invoker will attempt to open the link using <see
    /// cref="ChromeApplication.SafeLaunchUri(Uri)"/>.
    /// </remarks>
    public event EventHandler<LinkClickEventArgs>? LinkClick;

    /// <summary>
    /// Gets the container parent which all <see cref="Children"/> are to share.
    /// </summary>
    public Control Container { get; }

    /// <summary>
    /// Gets the children in the tracker.
    /// </summary>
    public IReadOnlySet<ICrossTrackable> Children { get; }

    /// <summary>
    /// Gets the first selected instance.
    /// </summary>
    public ICrossTrackable? FirstSelected { get; private set; }

    /// <summary>
    /// Gets the last selected instance.
    /// </summary>
    public ICrossTrackable? LastSelected { get; private set; }

    /// <summary>
    /// Gets the current number of selected children.
    /// </summary>
    public int SelectionCount
    {
        get { return _selection?.Count ?? 0; }
    }

    /// <summary>
    /// Returns true if the child is managed by this tracker.
    /// </summary>
    public bool Contains([NotNullWhen(true)] ICrossTrackable? obj)
    {
        // This is fast as it avoids lookup
        ConditionalDebug.ThrowIfTrue(obj?.Tracker == this && !_children.Contains(obj));
        return obj?.Tracker == this && _children.Count != 0;
    }

    /// <summary>
    /// Returns true if the child one of the selected instances.
    /// </summary>
    public bool IsSelected([NotNullWhen(true)] ICrossTrackable? obj)
    {
        if (_selection == null || obj == null || obj.Tracker != this)
        {
            return false;
        }

        // There's normally no need for this, as ICrossTrackable.HasSelection is more efficient.
        return _selection.Contains(obj);
    }

    /// <summary>
    /// Clears selected text for all children.
    /// </summary>
    /// <remarks>
    /// The result is true on change.
    /// </remarks>
    public bool SelectNone()
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectNone)}";
        ConditionalDebug.WriteLine(NSpace, $"Clear selection count: {SelectionCount}");

        _anchorObj = null;

        if (_selection != null)
        {
            foreach (var item in _selection)
            {
                item.SelectInternal(0, 0);
            }

            _selection = null;
            FirstSelected = null;
            LastSelected = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Selects text from "start" to "end" in single instance, while deselecting other children.
    /// </summary>
    /// <remarks>
    /// If "start" equals "end", the operation is equivalent to <see cref="SelectNone"/>. The result is true on change.
    /// </remarks>
    public bool SelectSingle(ICrossTrackable obj, int start, int end)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectSingle)}";
        ConditionalDebug.WriteLine(NSpace, $"SELECT: {obj.TrackKey}");
        ConditionalDebug.ThrowIfFalse(_children.Contains(obj));

        if (Contains(obj) && obj.SelectInternal(start, end))
        {
            _anchorObj = null;

            ConditionalDebug.WriteLine(NSpace, "Rebuilding");
            RebuildSelection(obj, obj.SelectionStart, obj, obj.SelectionEnd);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Selects a contiguous sequence of <see cref="Children"/>, from the given "obj0" instance up to, and including,
    /// "obj1". All text in each child within range is selected.
    /// </summary>
    /// <remarks>
    /// The return is the number of child blocks selected as a result of this operation. Both "obj0" and "obj1" must be
    /// members of <see cref="Children"/>, otherwise the call does nothing and returns 0. If "obj1" is null, the
    /// operation is equivalent to <see cref="SelectSingle"/>. Any selection prior to the call is cleared by this call
    /// where the result not 0.
    /// </remarks>
    public int SelectRange(ICrossTrackable obj0, ICrossTrackable? obj1 = null)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectRange)}";
        ConditionalDebug.WriteLine(NSpace, $"Select from children: {_children.Count}");
        ConditionalDebug.WriteLine(NSpace, $"Keys: {obj0.TrackKey}, {obj1?.TrackKey}");

        obj1 ??= obj0;

        if (_children.Count == 0 || !Contains(obj0) || !Contains(obj1))
        {
            ConditionalDebug.WriteLine(NSpace, $"Failed");
            return 0;
        }

        _anchorObj = null;
        return RebuildSelection(obj0, 0, obj1, obj1.TextLength);
    }

    /// <summary>
    /// Fully selects all children and returns true if changed.
    /// </summary>
    public bool SelectAll()
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(SelectAll)}";
        ConditionalDebug.WriteLine(NSpace, $"Select all children: {_children.Count}");

        if (_children.Count != 0)
        {
            var first = FirstSelected;
            var last = LastSelected;
            int origCount = SelectionCount;

            _anchorObj = null;
            _selection = null;
            FirstSelected = null;
            LastSelected = null;

            var view = new HashSet<ICrossTrackable>(_children.Count);

            foreach (var item in _children)
            {
                if (item.TrackKey.IsValid)
                {
                    view.Add(item);
                    item.SelectInternal(0, item.TextLength);

                    if (FirstSelected == null)
                    {
                        FirstSelected = item;
                        LastSelected = item;
                        continue;
                    }

                    if (SortCompare(item, FirstSelected) < 0)
                    {
                        FirstSelected = item;
                        continue;
                    }

                    if (SortCompare(item, LastSelected!) > 0)
                    {
                        LastSelected = item;
                    }
                }
            }

            if (view.Count != 0)
            {
                _selection = view;
            }

            return origCount != view.Count || !ReferenceEquals(FirstSelected, first) || !ReferenceEquals(LastSelected, last);
        }

        return false;
    }

    /// <summary>
    /// Gets the text combined from all instances according to "what".
    /// </summary>
    /// <remarks>
    /// The call may be expensive with very large texts.
    /// </remarks>
    public string? GetEffectiveText(WhatText what)
    {
        string? text = null;
        IEnumerable<ICrossTrackable>? values = what == WhatText.All ? _children : _selection;

        if (values != null)
        {
            StringBuilder? buffer = null;
            ICrossTrackable? previous = null;

            // Painful if huge
            var sorted = new List<ICrossTrackable>(values);
            sorted.Sort(SortCompare);

            foreach (var item in sorted)
            {
                buffer ??= new(256);

                if (previous != null)
                {
                    if (previous.TrackSeparator != null)
                    {
                        buffer.Append(previous.TrackSeparator);
                    }
                    else
                    if (item.TrackKey.IsHorizontalWith(previous.TrackKey))
                    {
                        buffer.Append(' ');
                    }
                    else
                    {
                        buffer.Append("\n\n");
                    }
                }

                previous = item;
                buffer.Append(item.TrackPrefix);
                buffer.Append(item.GetEffectiveText(what));
            }

            text = buffer?.ToString();
        }

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

            if (!string.IsNullOrEmpty(text))
            {
                // We just need any control
                return Container.CopyToClipboard(text);
            }
        }

        return false;
    }

    /// <summary>
    /// Invokes <see cref="LinkClick"/> and returns true if the link was handled.
    /// </summary>
    /// <remarks>
    /// Called by <see cref="CrossTextBlock"/> in response to <see cref="CrossRun"/> content.
    /// </remarks>
    public bool OnLinkClick(Uri uri)
    {
        var e = new LinkClickEventArgs(uri);
        LinkClick?.Invoke(this, e);
        return e.Handled;
    }

    /// <summary>
    /// Adds "obj" to <see cref="Children"/>.
    /// </summary>
    /// <remarks>
    /// Adding does not affect existing selected text and new items may potentionally be added within an existing
    /// selection range. The caller must clear the selection where this is likely.
    /// </remarks>
    /// <exception cref="ArgumentException">Already exists, or belongs to other tracker</exception>
    internal void AddInternal(ICrossTrackable obj)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(AddInternal)}";
        ConditionalDebug.WriteLine(NSpace, "ADDING OBJECT");

        if (obj.Tracker != this && obj.Tracker != null)
        {
            // Can't be a member of more than one (allow null at time of call)
            throw new ArgumentException("Already belongs to other tracker");
        }

        if (_children.Add(obj))
        {
            if (_children.Count == 1)
            {
                Container.PointerMoved += ParentPointerMovedHandler;
            }

            // This should efficient
            obj.SelectInternal(0, 0);

            ConditionalDebug.WriteLine(NSpace, "Done ok");
            return;
        }

        // Not expected in normal operation
        throw new ArgumentException("Already exists");
    }

    /// <summary>
    /// Removes "obj" from <see cref="Children"/> and returns true on success.
    /// </summary>
    internal bool RemoveInternal([NotNullWhen(true)] ICrossTrackable? obj)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(RemoveInternal)}";

        if (obj == null)
        {
            return false;
        }

        if (_children.Remove(obj))
        {
            ConditionalDebug.WriteLine(NSpace, "Removed");

            if (obj == _anchorObj)
            {
                _anchorObj = null;
            }

            if (_selection?.Remove(obj) == true && _selection.Count == 0)
            {
                _selection = null;
            }

            if (_children.Count == 0)
            {
                Container.PointerMoved -= ParentPointerMovedHandler;
            }

            _children.TrimCapacity(DefaultCapacity);
            ConditionalDebug.WriteLine(NSpace, "Success OK");
            return true;
        }

        // Not expected in normal operation
        return false;
    }

    internal void SetAnchor(ICrossTrackable obj, int textPos)
    {
        // Set anchor only
        // Leave to other calls to do resetting
        _anchorObj = obj;
        _anchorPos = textPos;
    }

    private static int SortCompare(ICrossTrackable obj0, ICrossTrackable obj1)
    {
        return obj0.TrackKey.CompareTo(obj1.TrackKey);
    }

    private int RebuildSelection(ICrossTrackable obj0, int pos0, ICrossTrackable obj1, int pos1)
    {
        var cap = Math.Min(_children.Count, DefaultCapacity);
        var view = new HashSet<ICrossTrackable>(cap);

        if (ReferenceEquals(obj0, obj1))
        {
            // Single
            if (obj0.TrackKey.IsValid)
            {
                view.Add(obj0);
                FirstSelected = obj0;
                LastSelected = obj0;
            }
        }
        else
        {
            var comp = SortCompare(obj0, obj1);

            if (comp > 0)
            {
                (obj0, obj1) = (obj1, obj0);
                (pos0, pos1) = (pos1, pos0);
            }

            FirstSelected = obj0;
            var key0 = obj0.TrackKey;

            LastSelected = obj1;
            var key1 = obj1.TrackKey;

            foreach (var item in _children)
            {
                var key = item.TrackKey;

                if (key.IsValid && key.CompareTo(key0) >= 0 && key.CompareTo(key1) <= 0)
                {
                    view.Add(item);
                }
            }
        }

        if (_selection != null)
        {
            // Ensure those not selected are unselected
            foreach (var item in _selection)
            {
                if (!view.Contains(item))
                {
                    item.SelectInternal(0, 0);
                }
            }
        }

        if (view.Count == 0)
        {
            // Invalid
            _anchorObj = null;
            _selection = null;
            FirstSelected = null;
            LastSelected = null;
            return 0;
        }

        _selection = view;

        if (view.Count == 1)
        {
            // Single
            obj0.SelectInternal(pos0, pos1);
            return 1;
        }

        foreach (var item in view)
        {
            if (ReferenceEquals(obj0, item))
            {
                // Top-left
                item.SelectInternal(item.TextLength, pos0);
                continue;
            }

            if (ReferenceEquals(obj1, item))
            {
                // Bottom-right
                item.SelectInternal(0, pos1);
                continue;
            }

            item.SelectInternal(0, item.TextLength);
        }

        return _selection.Count;
    }

    private void ParentPointerMovedHandler(object? _, PointerEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTracker)}.{nameof(ParentPointerMovedHandler)}";

        if (_anchorObj?.TrackKey.IsValid != true)
        {
            _anchorObj = null;
            return;
        }

        var info = e.GetCurrentPoint(Container);
        var props = info.Properties;

        if (!props.IsLeftButtonPressed || props.IsRightButtonPressed)
        {
            _anchorObj = null;
            return;
        }

        if (Container.GetVisualAt(info.Position) is ICrossTrackable target)
        {
            ConditionalDebug.WriteLine(NSpace, "Has trackable");

            if (target.Tracker == this && target.IsPointerSelectEnabled &&
                target is Control control && e.Pointer.Captured != control)
            {
                int pos = target.GetTextPosition(e.GetPosition(control));
                ConditionalDebug.WriteLine(NSpace, $"Target pos: {pos}");

                RebuildSelection(_anchorObj, _anchorPos, target, pos);
                e.Pointer.Capture(control);
            }

            return;
        }
    }

}
