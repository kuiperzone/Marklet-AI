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

using Avalonia;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Interface for objects possessing cross-text selection capability..
/// </summary>
public interface ICrossTrackable : ICrossTrackOwner
{
    /// <summary>
    /// Gets a sort key used by <see cref="CrossTracker"/>.
    /// </summary>
    /// <remarks>
    /// If the control is not in the visual true or not rendered, the value will be <see cref="CrossKey.Empty"/>.
    /// </remarks>
    CrossKey TrackKey { get; }

    /// <summary>
    /// Gets (or sets) a prefix when text is copied.
    /// </summary>
    /// <remarks>
    /// If null or empty, nothing is inserted as a prefix. The value is typically left at null default.
    /// </remarks>
    string? TrackPrefix { get; }

    /// <summary>
    /// Gets (or sets) a separator suffix when text is copied.
    /// </summary>
    /// <remarks>
    /// If null, a value is determined automatically (i.e. two line feeds for vertical separation and a space for
    /// horizontal). The value is typically left at null default.
    /// </remarks>
    string? TrackSeparator { get; }

    /// <summary>
    /// Gets whether the content is empty.
    /// </summary>
    public bool IsEmpty { get; }

    /// <summary>
    /// Gets the text length.
    /// </summary>
    public int TextLength { get; }

    /// <summary>
    /// Gets (or sets) the character index for the selection start.
    /// </summary>
    public int SelectionStart { get; }

    /// <summary>
    /// Gets (or sets) the character index for the selection end.
    /// </summary>
    public int SelectionEnd { get; }

    /// <summary>
    /// Gets whether the selected text length is non-zero.
    /// </summary>
    bool HasSelection { get; }

    /// <summary>
    /// Gets whether the user can select text with the mouse pointer.
    /// </summary>
    bool IsPointerSelectEnabled { get; }

    /// <summary>
    /// Clears the selection.
    /// </summary>
    /// <remarks>
    /// The result is true if the selection is changed. On success, other selected children of <see
    /// cref="ICrossTrackOwner.Tracker"/> are de-selected.
    /// </remarks>
    bool SelectNone();

    /// <summary>
    /// Sets the selection start and end positions in a single operation.
    /// </summary>
    /// <remarks>
    /// The result is true if the selection is changed. On success, other selected children of <see
    /// cref="ICrossTrackOwner.Tracker"/> are de-selected.
    /// </remarks>
    bool Select(int start, int end);

    /// <summary>
    /// Selects all text in this <see cref="ICrossTrackable"/> instance only.
    /// </summary>
    /// <remarks>
    /// The result is true if the selection is changed. On success, other selected children of <see
    /// cref="ICrossTrackOwner.Tracker"/> are de-selected.
    /// </remarks>
    bool SelectAll();

    /// <summary>
    /// Gets text according to the given "what".
    /// </summary>
    string? GetEffectiveText(WhatText what);

    /// <summary>
    /// Returns the text position at the given pixel point in the instances reference frame.
    /// </summary>
    int GetTextPosition(Point point);

    /// <summary>
    /// For use by <see cref="CrossTracker"/> only.
    /// </summary>
    /// <remarks>
    /// It does not act on <see cref="ICrossTrackOwner.Tracker"/>. The result is true on change.
    /// </remarks>
    internal bool SelectInternal(int start, int end);
}