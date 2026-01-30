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

using Avalonia;
using Avalonia.Controls;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Subclass of <see cref="PixieGroup"/> intended to hold instances of <see cref="SessionControl"/> for use with <see
/// cref="GardenBinView"/>.
/// </summary>
public class TopicControl : PixieGroup
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public TopicControl(string title)
        : this()
    {
        IsSubFolder = true;
        CollapseTitle = title;
        IsCollapsable = true;
        IsOpen = false;

        ChildIndent = ChromeSizes.OneCh * 3.0;
        ChildCorner = Styling.SmallCornerRadius;
    }

    /// <summary>
    /// Protected constructor.
    /// </summary>
    protected TopicControl()
    {
        Spacing = 1.0;
    }

    /// <summary>
    /// Gets whether created with public title constructor.
    /// </summary>
    protected bool IsSubFolder { get; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(PixieGroup);

    internal void Refresh()
    {
        ChildCorner = Styling.SmallCornerRadius;

        foreach (var item in Children)
        {
            if (item is SessionControl b)
            {
                b.Refresh();
                continue;
            }

            if (item is TopicControl g)
            {
                g.Refresh();
                continue;
            }
        }
    }

    /// <summary>
    /// Rebuilds child items re-using existing controls where possible.
    /// </summary>
    /// <remarks>
    /// Returns the most recent "new instance" session control created, or null if no new controls where created. A
    /// non-null result is an indication that one or more items have been moved into the owner bin.
    /// </remarks>
    internal SessionControl? UpdateChildren(List<GardenSession> source, List<Control>? buffer = null)
    {
        const string NSpace = $"{nameof(TopicControl)}.{nameof(Refresh)}";

        bool isBuffered = buffer != null;
        SessionControl? newInstance = null;

        buffer ??= new(source.Count);

        foreach (var item in source)
        {
            // If Owner is null, it means we have stale copy that
            // has been removed from the database. It shouldn't happen.
            ConditionalDebug.ThrowIfNull(item.Owner);

            if (item.Owner == null || (isBuffered && item.Topic != null))
            {
                // Ignore
                // If buffered, we only want items not belonging to a group
                // because groups are already supplied within initial "buffer".
                ConditionalDebug.WriteLine(NSpace, $"Ignore: {item.Title}");
                continue;
            }

            // See if it holding a visual object which is our child.
            // This saves us looking it up. If not, we create a new one.
            var b = item.VisualTag as SessionControl;

            if (b?.Group == this)
            {
                ConditionalDebug.WriteLine(NSpace, $"Refresh: {item.Title}");
                b.Refresh();
                buffer.Add(b);
                continue;
            }

            ConditionalDebug.WriteLine(NSpace, $"NEW OBJ: {item.Title}");
            var obj = new SessionControl(item);
            newInstance = SessionControl.MoreRecent(newInstance, obj);
            buffer.Add(obj);
        }

        // This should be efficient where references are unchanged.
        // It will avoid pulling things in or out of the tree where possible.
        Children.Replace(buffer);

        return newInstance;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == IsOpenProperty && IsSubFolder)
        {
            if (IsOpen)
            {
                CollapseSymbol = IsOpen ? Symbols.FolderOpen : Symbols.Folder;
                return;
            }

            CollapseSymbol = Symbols.Folder;
            return;
        }
    }
}
