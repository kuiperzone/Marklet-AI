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

using Avalonia.Collections;
using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Shared;

/// <summary>
/// Helper class used with <see cref="MarkView"/> and related classes.
/// </summary>
public sealed class MarkEngine : ICrossTrackOwner
{
    private readonly List<MarkVisualHost> _cache = new(4);
    private readonly AvaloniaList<Control> _children;

    /// <summary>
    /// Constructor with owner and children instance.
    /// </summary>
    /// <remarks>
    /// The instance will managed "children".
    /// </remarks>
    public MarkEngine(MarkControl owner, AvaloniaList<Control> children)
    {
        Shim = new(owner);
        _children = children;
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public readonly MarkShim Shim;

    /// <inheritdoc cref="ICrossTrackOwner.Tracker"/>
    public CrossTracker Tracker
    {
        get { return Shim.Owner.Tracker; }
    }

    /// <summary>
    /// Gets the first <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    public ICrossTrackable? Track0 { get; private set; }

    /// <summary>
    /// Gets the last <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    public ICrossTrackable? Track1 { get; private set; }

    /// <summary>
    /// Deselects any selected text.
    /// </summary>
    public bool SelectNone()
    {
        return Tracker.SelectNone();
    }

    /// <summary>
    /// Selects just this block.
    /// </summary>
    public bool SelectBlock()
    {
        if (Track0 != null)
        {
            return Tracker.SelectRange(Track0, Track1) != 0;
        }

        return false;
    }

    /// <summary>
    /// Simply calls <see cref="CrossTracker.SelectAll"/>.
    /// </summary>
    public bool SelectAll()
    {
        return Tracker.SelectAll();
    }

    /// <summary>
    /// Refresh, but not "rebuild", when owner properties change.
    /// </summary>
    /// <remarks>
    /// This pertains to an update of colors and font styles.
    /// </remarks>
    public void OwnerRefresh()
    {
        int lc = _cache.Count - 1;

        for (int n = 0; n < _cache.Count; ++n)
        {
            _cache[n].Refresh(n == 0, n == lc);
        }
    }

    /// <summary>
    /// Rebuilds the children provided on construction from the given "document".
    /// </summary>
    /// <remarks>
    /// The "header" and "footer" are inserted at the start and end of children. There is no need to call <see
    /// cref="OwnerRefresh"/> before or after a rebuild.
    /// </remarks>
    public void Rebuild(MarkDocument document, Control? header = null, Control? footer = null)
    {
        const string NSpace = $"{nameof(MarkEngine)}.{nameof(Rebuild)}";
        Diag.WriteLine(NSpace, "UPDATE PROCESSSING");

        int index = 0;
        int cacheN = 0;
        var buffer = new List<Control>(document.Count + 2);

        Track0 = null;
        Track1 = null;

        if (header != null)
        {
            buffer.Add(header);
        }

        while (index < document.Count)
        {
            MarkVisualHost host;

            if (cacheN < _cache.Count)
            {
                host = _cache[cacheN];

                if (host.ConsumeUpdates(document, ref index) == MarkConsumed.Incompatible)
                {
                    host = MarkVisualHost.New(Shim, document, ref index);
                    _cache[cacheN] = host;
                }

                cacheN += 1;
                AddTracks(host);
                buffer.Add(host.Control);
                continue;
            }

            host = MarkVisualHost.New(Shim, document, ref index);

            _cache.Add(host);
            cacheN = _cache.Count;

            AddTracks(host);
            buffer.Add(host.Control);
        }

        _cache.RemoveRange(cacheN, _cache.Count - cacheN);

        if (footer != null)
        {
            buffer.Add(footer);
        }

        // WRITE NEW CONTENTS
        // Relies on extension method that avoids removing objects from visual tree.
        _children.Replace(buffer);

        _cache.TrimCapacity();
        _children.TrimCapacity();
        Diag.WriteLine(NSpace, "END OF UPDATE");
    }

    /// <summary>
    /// Gets an array of internal blocks kinds primarily for test.
    /// </summary>
    /// <remarks>
    /// Blocks contained within quote or list levels return a single block of <see cref="BlockKind.Para"/> irrespective
    /// of their contents. The result excludes headers or footers.
    /// </remarks>
    public BlockKind[] GetBlockKinds()
    {
        var array = new BlockKind[_cache.Count];

        for (int n = 0; n < _cache.Count; ++n)
        {
            if (_cache[n] is MarkBlockHost block)
            {
                array[n] = block.Kind;
            }
        }

        return array;
    }

    private void AddTracks(MarkVisualHost host)
    {
        if (host.Track0 != null)
        {
            Track0 ??= host.Track0;
            Track1 = host.Track1 ?? host.Track0;
        }
    }
}