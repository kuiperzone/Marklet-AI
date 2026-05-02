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
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Stub implementation of <see cref="ICrossTrackable"/> for test purposes.
/// </summary>
public class CrossTextStub : ICrossTrackable, ICrossTrackOwner
{
    private static ulong s_keyCounter;

    // Mirror CrossTextBlock behaviour in implementation.
    private CrossTracker? _tracker;

    /// <inheritdoc cref="ICrossTrackOwner.Tracker"/>
    public CrossTracker? Tracker
    {
        get { return _tracker; }

        set
        {
            if (_tracker != value)
            {
                _tracker?.RemoveInternal(this);

                _tracker = value;
                value?.AddInternal(this);
            }
        }
    }

    /// <inheritdoc cref="ICrossTrackable.TrackKey"/>
    public CrossKey TrackKey { get; } = new(++s_keyCounter);

    /// <inheritdoc cref="ICrossTrackable.TrackPrefix"/>
    public string? TrackPrefix { get; set; }

    /// <inheritdoc cref="ICrossTrackable.TrackSeparator"/>
    public string? TrackSeparator { get; set; }

    /// <inheritdoc cref="ICrossTrackable.IsEmpty"/>
    public bool IsEmpty
    {
        get { return string.IsNullOrEmpty(Text); }
    }

    /// <inheritdoc cref="ICrossTrackable.TextLength"/>
    public int TextLength
    {
        get { return Text?.Length ?? 0; }
    }

    /// <inheritdoc cref="ICrossTrackable.SelectionStart"/>
    public int SelectionStart { get; private set; }

    /// <inheritdoc cref="ICrossTrackable.SelectionEnd"/>
    public int SelectionEnd { get; private set; }

    /// <inheritdoc cref="ICrossTrackable.HasSelection"/>
    public bool HasSelection
    {
        get { return SelectionStart != SelectionEnd && !IsEmpty; }
    }

    /// <inheritdoc cref="ICrossTrackable.IsPointerSelectEnabled"/>
    public bool IsPointerSelectEnabled { get; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string? Text { get; set; }

    /// <inheritdoc cref="ICrossTrackable.SelectNone"/>
    public bool SelectNone()
    {
        return Select(0, 0);
    }

    /// <inheritdoc cref="ICrossTrackable.Select(int, int)"/>
    public bool Select(int start, int end)
    {
        if (Tracker != null)
        {
            return Tracker.SelectSingle(this, start, end);
        }

        return SelectInternal(start, end);
    }

    /// <inheritdoc cref="ICrossTrackable.SelectAll"/>
    public bool SelectAll()
    {
        return Select(0, TextLength);
    }

    /// <inheritdoc cref="ICrossTrackable.GetEffectiveText(WhatText)"/>
    public string? GetEffectiveText(WhatText what)
    {
        const string NSpace = $"{nameof(ICrossTrackable)}.{nameof(GetEffectiveText)}";
        Diag.WriteLine(NSpace, what);

        int length = TextLength;

        if (!GetNormalizedSelectedRange(out int start, out int end))
        {
            Diag.WriteLine(NSpace, "No selection");

            if (what == WhatText.SelectedOrNull)
            {
                return null;
            }

            start = 0;
            end = length;
        }

        if (what == WhatText.All)
        {
            start = 0;
            end = length;
        }

        Diag.WriteLine(NSpace, $"Plain range: [{start}, {end})");
        return Text?.Substring(start, end - start);
    }

    /// <inheritdoc cref="ICrossTrackable.GetTextPosition"/>
    public int GetTextPosition(Point point)
    {
        return 0;
    }

    /// <summary>
    /// Gets the normalized range and returns true where "start" is less than "end".
    /// </summary>
    /// <remarks>
    /// Where <see cref="IsEmpty"/> is true, the result is always false with both "start" and "end" equals to 0.
    /// </remarks>
    public bool GetNormalizedSelectedRange(out int start, out int end)
    {
        const string NSpace = $"{nameof(ICrossTrackable)}.{nameof(GetNormalizedSelectedRange)}";
        start = Math.Min(Math.Min(SelectionStart, SelectionEnd), TextLength);
        end = Math.Min(Math.Max(SelectionStart, SelectionEnd), TextLength);

        Diag.WriteLine(NSpace, $"[{start}, {end}), length: {TextLength}");
        return start < TextLength && start < end;
    }

    bool ICrossTrackable.SelectInternal(int start, int end)
    {
        return SelectInternal(start, end);
    }

    private bool SelectInternal(int start, int end)
    {
        // Do not call tracker
        start = Math.Max(start, 0);
        end = Math.Max(end, 0);

        if (SelectionStart != start || SelectionEnd != end)
        {
            SelectionStart = start;
            SelectionEnd = end;
            return true;
        }

        return false;
    }
}