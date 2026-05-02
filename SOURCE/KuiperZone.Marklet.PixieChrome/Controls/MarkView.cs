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
using KuiperZone.Marklet.PixieChrome.Shared;
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
    private readonly MarkEngine _engine;
    private readonly StackPanel _panel = new();
    private MarkDocument? _source;
    private MarkDocument? _found;
    private Control? _header;
    private Control? _footer;
    private string? _content;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkView()
        : this(null)
    {
    }

    /// <summary>
    /// Constructor with "shared" tracker.
    /// </summary>
    /// <remarks>
    /// If "shared" is null, same as default construction.
    /// </remarks>
    public MarkView(CrossTracker? shared)
        : base(shared)
    {
        Child = _panel;
        _engine = new(this, _panel.Children);

        ContextMenu = CrossContextMenu.Global;
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
    /// The default value is null.
    /// </remarks>
    public string? Content
    {
        get { return _content; }
        set { SetAndRaise(ContentProperty, ref _content, value); }
    }

    /// <summary>
    /// Gets or sets the markdown options used for processing <see cref="Content"/> and <see cref="SetContent"/>.
    /// </summary>
    /// <remarks>
    /// Setting this value does not cause a visible update. Rather options are used on next update.
    /// </remarks>
    public MarkOptions Options { get; set; } = MarkOptions.Markdown | MarkOptions.Coalesce | MarkOptions.Sanitize;

    /// <summary>
    /// Gets the keyword substring text.
    /// </summary>
    public string? KeywordText { get; private set; }

    /// <summary>
    /// Gets the number of occurrences of <see cref="KeywordText"/>.
    /// </summary>
    public int KeywordCount { get; private set; }

    /// <summary>
    /// Gets the "search in text" flag options.
    /// </summary>
    public SearchFlags SearchFlags { get; private set; }

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
    public ICrossTrackable? Track0
    {
        get { return _engine.Track0; }
    }

    /// <summary>
    /// Gets the last <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    public ICrossTrackable? Track1
    {
        get { return _engine.Track1; }
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
    /// Sets <see cref="Content"/>, <see cref="KeywordText"/> and <see cref="SearchFlags"/> in single operation,
    /// returning true if changed and visible update applied.
    /// </summary>
    public bool SetContent(string? content, string? keyword = null, SearchFlags flags = SearchFlags.None)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            content = null;
        }

        bool update = false;

        if (_content != content)
        {
            update = true;
            _content = content;
            _source = new(content, Options);
            _found = null;
        }

        if (SetSearchInternal(keyword, flags) || update)
        {
            Rebuild();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets <see cref="KeywordText"/> and <see cref="SearchFlags"/>, returning true if changed and visible update
    /// applied.
    /// </summary>
    public bool SetSearch(string? keyword, SearchFlags flags)
    {
        if (SetSearchInternal(keyword, flags))
        {
            Rebuild();
            return true;
        }

        return false;
    }

    /// <inheritdoc cref="MarkControl.PropertyRefresh"/>
    protected override void PropertyRefresh()
    {
        _engine.OwnerRefresh();
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
            Diag.WriteLine(NSpace, "Update content");
            _found = null;
            _source = new(_content, Options);
            SetSearchInternal(KeywordText, SearchFlags);
            Rebuild();
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

    private void Rebuild()
    {
        var src = _found ?? _source;

        if (src != null)
        {
            _engine.Rebuild(src, _header, _footer);
        }
    }

    private bool SetSearchInternal(string? keyword, SearchFlags flags)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            // Will not trim the sides, but not going to searching for a single space etc.
            keyword = null;
        }

        if (Options.HasFlag(MarkOptions.Sanitize))
        {
            keyword = Sanitizer.Sanitize(keyword, SanFlags.SubControl | SanFlags.NormC);
        }

        if (_found == null || KeywordText != keyword || SearchFlags != flags)
        {
            KeywordText = keyword;
            SearchFlags = flags;

            int counter = 0;
            var old = _found;

            _found = _source?.Search(KeywordText, SearchFlags, out counter);
            KeywordCount = counter;

            return old != null || _found != null;
        }

        return false;
    }
}