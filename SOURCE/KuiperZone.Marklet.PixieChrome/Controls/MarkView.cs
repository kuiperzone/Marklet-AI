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
using Avalonia.Controls;
using Avalonia.Input;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Displays markdown content.
/// </summary>
/// <remarks>
/// The class is not aware <see cref="ChromeStyling"/> and properties are "direct".
/// </remarks>
public class MarkView : MarkControl, ICrossTrackOwner
{
    private readonly StackPanel _panel = new();
    private readonly List<MarkVisualHost> _cache = new();
    private readonly MarkDocument _source = new();
    private Control? _header;
    private Control? _footer;

    private string? _content;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkView()
    {
        Focusable = true;
        IsTabStop = false;
        FocusAdorner = null;

        ContextMenu = CrossContextMenu.Global;
        base.Child = _panel;
    }

    /// <summary>
    /// Constructor with shared selection host.
    /// </summary>
    public MarkView(CrossTracker tracker)
        : base(tracker)
    {
        Focusable = true;
        IsTabStop = false;
        FocusAdorner = null;

        ContextMenu = CrossContextMenu.Global;
        base.Child = _panel;
    }

    /// <summary>
    /// Constructor with owner, the properties of which are copied to this.
    /// </summary>
    /// <remarks>
    /// Intended where the instance is one of many children within a more sophisticated control.
    /// </remarks>
    protected MarkView(MarkControl owner)
        : base(owner)
    {
        // Ensure these are off
        ConditionalDebug.ThrowIfTrue(Focusable);
        ConditionalDebug.ThrowIfTrue(IsTabStop);
        base.Child = _panel;
    }

    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkView, string?> ContentProperty =
        AvaloniaProperty.RegisterDirect<MarkView, string?>(nameof(Content), o => o.Content, (o, v) => o.Content = v);

    /// <summary>
    /// Gets or sets the markdown content.
    /// </summary>
    /// <remarks>
    /// Note that after setting, actual rending may occur later. The default value is null.
    /// </remarks>
    public string? Content
    {
        get { return _content; }
        set { SetAndRaise(ContentProperty, ref _content, value); }
    }

    /// <summary>
    /// Gets or sets the markdown options used for processing <see cref="Content"/>.
    /// </summary>
    public MarkOptions Options { get; set; }

    /// <summary>
    /// Gets or sets a header Control instance.
    /// </summary>
    /// <remarks>
    /// The control instance will be inserted at the top of the view.
    /// </remarks>
    public Control? Header
    {
        get { return _header; }

        set
        {
            if (_header != value)
            {
                if (_header != null)
                {
                    _panel.Children.Remove(_header);
                }

                _header = value;

                if (value != null)
                {
                    _panel.Children.Insert(0, value);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a footer Control instance.
    /// </summary>
    /// <remarks>
    /// The footer Control instance will be inserted at the top of the view.
    /// </remarks>
    public Control? Footer
    {
        get { return _footer; }

        set
        {
            if (_footer != value)
            {
                if (_footer != null)
                {
                    _panel.Children.Remove(_footer);
                }

                _footer = value;

                if (value != null)
                {
                    _panel.Children.Insert(_panel.Children.Count, value);
                }
            }
        }
    }

    /// <summary>
    /// Gets the first <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    public ICrossTrackable? Track0 { get; protected set; }

    /// <summary>
    /// Gets the last <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    public ICrossTrackable? Track1 { get; protected set; }

    /// <summary>
    /// Do not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    protected new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Deselects any selected text.
    /// </summary>
    /// <remarks>
    /// Simply calls <see cref="CrossTracker.SelectNone"/>.
    /// </remarks>
    public bool SelectNone()
    {
        return Tracker.SelectNone();
    }

    /// <summary>
    /// Selects just this block.
    /// </summary>
    /// <remarks>
    /// Applicable only where <see cref="MarkControl.Tracker"/> is shared, otherwise behaviour of <see
    /// cref="SelectBlock"/> and <see cref="SelectAll"/> are the same.
    /// </remarks>
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
    /// Gets an array of internal blocks kinds primarily for test.
    /// </summary>
    /// <remarks>
    /// The result is subject to coalescing and blocks may be combined. Blocks contained within quote or list levels
    /// return a single block of <see cref="BlockKind.Para"/> irrespective of their contents. The result excludes <see
    /// cref="Header"/> and <see cref="Footer"/>.
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

    /// <inheritdoc cref="MarkControl.ImmediateRefresh"/>
    protected override void ImmediateRefresh()
    {
        int nm1 = _cache.Count - 1;

        for (int n = 0; n < _cache.Count; ++n)
        {
            _cache[n].Refresh(n == 0, n == nm1);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        const string NSpace = $"{nameof(MarkView)}.{nameof(OnPropertyChanged)}";

        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ContentProperty)
        {
            if (_source.Update(_content, Options))
            {
                ConditionalDebug.WriteLine(NSpace, "Update content");

                // Supply coalesced clone
                // This is intended to be efficient as it requires
                // fewer visual controls but does work in Coalesce().
                UpdateInternal(_source.Coalesce());

                // Alternatively...
                //UpdateInternal(_source);
            }

            return;
        }

        if (p == ContextMenuProperty)
        {
            var value = change.GetNewValue<ContextMenu?>();

            foreach (var item in _cache)
            {
                // Set null to clear as now set on parent
                item.Control.ContextMenu = null;
            }

            return;
        }
    }

    /// <summary>
    /// Overrides. The instance must be Focusable and Focused for this to work.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = Tracker.CopyText(WhatText.SelectedOrNull);
            return;
        }

        if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = Tracker.SelectAll();
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = Tracker.SelectNone();
            return;
        }
    }

    private void UpdateInternal(MarkDocument doc)
    {
        const string NSpace = $"{nameof(MarkView)}.{nameof(UpdateInternal)}";
        ConditionalDebug.WriteLine(NSpace, "UPDATE PROCESSSING");

        try
        {
            int index = 0;
            int cacheN = 0;
            var buffer = new List<Control>(doc.Count);

            Track0 = null;
            Track1 = null;

            if (_header != null)
            {
                buffer.Add(_header);
            }

            while (index < doc.Count)
            {
                MarkVisualHost host;

                if (cacheN < _cache.Count)
                {
                    host = _cache[cacheN];

                    if (host.ConsumeUpdates(doc, ref index) == MarkConsumed.Incompatible)
                    {
                        host = MarkVisualHost.New(this, doc, ref index);
                        _cache[cacheN] = host;
                    }

                    cacheN += 1;
                    AddTracks(host);
                    buffer.Add(host.Control);
                    continue;
                }

                host = MarkVisualHost.New(this, doc, ref index);

                _cache.Add(host);
                cacheN = _cache.Count;

                AddTracks(host);
                buffer.Add(host.Control);
            }

            _cache.RemoveRange(cacheN, _cache.Count - cacheN);
            _cache.TrimCapacity();

            if (_footer != null)
            {
                buffer.Add(_footer);
            }

            // WRITE NEW CONTENTS
            // Relies on extension method that avoids removing objects from visual tree.
            var children = _panel.Children;
            children.Replace(buffer);

            if (children.Capacity - children.Count > 32)
            {
                children.Capacity = children.Count;
            }

            ConditionalDebug.WriteLine(NSpace, "END OF UPDATE");
        }
        catch (Exception e)
        {
            ConditionalDebug.WriteLine(NSpace, e);
            throw;
        }
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