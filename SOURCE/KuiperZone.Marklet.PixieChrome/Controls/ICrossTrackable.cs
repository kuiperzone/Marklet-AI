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

using Avalonia.Controls.Documents;

namespace KuiperZone.Marklet.PixieChrome.Controls;
/// <summary>
/// Interface for objects possessing cross-text selection capability..
/// </summary>
public interface ICrossTrackable : ICrossTrackOwner
{
    /// <summary>
    /// Gets a logical ID value used by <see cref="CrossTracker"/>.
    /// </summary>
    /// <remarks>
    /// The value is not an index in a visual tree. The only requirement is that controls with higher <see
    /// cref="TrackKey"/> values appear below (on screen) than those with lower ones. The value is assigned the moment
    /// <see cref="ICrossTrackOwner.Tracker"/> is modified, and may be any incrementing value, but is always 0 when <see
    /// cref="ICrossTrackOwner.Tracker"/> is null.
    /// </remarks>
    ulong TrackKey { get; }

    /// <summary>
    /// Gets whether the selected text length is non-zero.
    /// </summary>
    bool HasSelection { get; }

    /// <summary>
    /// Gets whether the instance has styled inline content.
    /// </summary>
    bool HasComplexContent { get; }

    /// <summary>
    /// Gets (or sets) the character index for the selection start.
    /// </summary>
    /// <remarks>
    /// This value is not a XAML property with binding. Negative values are clamped.
    /// </remarks>
    public int SelectionStart { get; }

    /// <summary>
    /// Gets (or sets) the character index for the selection end.
    /// </summary>
    /// <remarks>
    /// This value is not a XAML property with binding. Negative values are clamped.
    /// </remarks>
    public int SelectionEnd { get; }

    /// <summary>
    /// Gets whether the content is empty.
    /// </summary>
    /// <remarks>
    /// Where <see cref="InlineUIContainer"/> is used, it is possible that <see cref="IsEmpty"/> gives false, while <see
    /// cref="TextLength"/> gives 0.
    /// </remarks>
    public bool IsEmpty { get; }

    /// <summary>
    /// Gets the text length.
    /// </summary>
    /// <remarks>
    /// Where <see cref="InlineUIContainer"/> is used, it is possible that <see cref="IsEmpty"/> gives false, while <see
    /// cref="TextLength"/> gives 0.
    /// </remarks>
    public int TextLength { get; }

    /// <summary>
    /// Gets (or sets) the text.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Clears the selection.
    /// </summary>
    /// <remarks>
    /// The result is true if the selection is changed. Calling this programmatically will always clear other selected
    /// areas from <see cref="ICrossTrackOwner.Tracker"/>.
    /// </remarks>
    bool SelectNone();

    /// <summary>
    /// Sets the selection start and end positions in a single operation.
    /// </summary>
    /// <remarks>
    /// The result is true if the selection is changed. Calling this programmatically will always clear other selected
    /// areas from <see cref="ICrossTrackOwner.Tracker"/>.
    /// </remarks>
    bool Select(int start, int end);

    /// <summary>
    /// Selects all text in the <see cref="ICrossTrackable"/> instance only.
    /// </summary>
    /// <remarks>
    /// The result is true if the selection is changed. Calling this programmatically will always clear other selected
    /// areas from <see cref="ICrossTrackOwner.Tracker"/>.
    /// </remarks>
    bool SelectAll();

    /// <summary>
    /// Gets text according to the given "what".
    /// </summary>
    string? GetEffectiveText(WhatText what);

    /// <summary>
    /// For use by <see cref="CrossTracker"/> only.
    /// </summary>
    /// <remarks>
    /// Calling this programmatically does not act on <see cref="ICrossTrackOwner.Tracker"/>.
    /// </remarks>
    internal bool SelectInternal(int start, int end);
}