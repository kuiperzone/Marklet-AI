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

using Avalonia.Media;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using Avalonia.Controls;
using KuiperZone.Marklet.Tooling.Markdown;
using KuiperZone.Marklet.PixieChrome.Shared;

namespace KuiperZone.Marklet.Controls.Internal;

/// <summary>
/// For use by <see cref="DeckViewer"/>.
/// </summary>
internal sealed class LeafView : Border, ICrossTrackOwner
{
    private readonly MarkEngine _engine;
    private readonly StackPanel _panel = new();
    private readonly CrossTextBlock _header0 = new();
    private CrossTextBlock? _header1;
    private MarkDocument? _source;
    private long _changeCounter;
    private readonly double _topMargin;

    /// <summary>
    /// Constructor with shared selection host.
    /// </summary>
    /// <remarks>
    /// The <see cref="Rebuild"/> method must be called after construction.
    /// </remarks>
    public LeafView(DeckViewer owner, double topMargin)
    {
        Owner = owner;
        ContextMenu = DeckMenu.Global;

        Child = _panel;
        _engine = new(owner, _panel.Children);
        _topMargin = topMargin;

        _header0.Tracker = owner.Tracker;
        _header0.TextWrapping = TextWrapping.NoWrap;
        _header0.TextTrimming = TextTrimming.CharacterEllipsis;
        _header0.Foreground = ChromeStyling.GrayForeground;
        _header0.FontSize = ChromeFonts.SmallFontSize * owner.Zoom.Fraction;
    }

    /// <inheritdoc cref="ICrossTrackOwner.Tracker"/>
    public CrossTracker Tracker
    {
        get { return Owner.Tracker; }
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public readonly DeckViewer Owner;

    /// <summary>
    /// Gets the leaf format.
    /// </summary>
    public LeafFormat Format { get; private set; }

    /// <summary>
    /// Gets the number instances of <see cref="GlobalGarden.Search"/> determined at last <see cref="Rebuild"/> call.
    /// </summary>
    public int SearchCount { get; private set; }

    public void OwnerRefresh()
    {
        MorphKind();
        _engine.OwnerRefresh();
    }

    /// <summary>
    /// Update on "leaf" content.
    /// </summary>
    public void Rebuild(GardenLeaf leaf, bool focused, bool finding)
    {
        const string NSpace = $"{nameof(LeafView)}.{nameof(Rebuild)}";
        Diag.WriteLine(NSpace, $"Change counters: {_changeCounter}, leaf: {leaf.VisualCounter}");

        bool change = false;

        // Rely on 64-bit ChangeCounter being unique and 0 on initialization.
        if (Format != leaf.Format || _changeCounter == 0)
        {
            // Must do this before Rebuild below
            change = true;
            Format = leaf.Format;
            MorphKind();
        }

        Diag.WriteLine(NSpace, $"Leaf kind: {leaf.Format}");

        if (focused || change || _changeCounter != leaf.VisualCounter)
        {
            change = true;
            _changeCounter = leaf.VisualCounter;

            _header1 = null;
            var name = leaf.Format.ToName();

            if (leaf.Format == LeafFormat.AssistantMessage && leaf.Assistant != null)
            {
                name = leaf.Assistant;
            }

            if (name != null)
            {
                _header0.Text = name.ToUpperInvariant();
                _header1 = _header0;
            }

            Diag.WriteLine(NSpace, $"Parse options: {leaf.ParsingsOptions}");

            // PROCESS MARKDOWN
            _source = new(leaf.Content, leaf.ParsingsOptions);
        }

        if (_source == null)
        {
            SearchCount = 0;
            return;
        }

        if (change || finding)
        {
            SearchCount = 0;

            var document = _source;
             var f = GlobalGarden.Search;

            if (f?.Keyword != null)
            {
                // Find / search
                document = _source.Search(f.Keyword, f.Flags, out int counter) ?? _source;
                SearchCount = counter;
            }

            // REBUILD VISUAL
            _engine.Rebuild(document, _header1);
        }
    }

    /// <summary>
    /// Deselects any selected text.
    /// </summary>
    public bool SelectNone()
    {
        return _engine.SelectNone();
    }

    /// <summary>
    /// Selects just this block.
    /// </summary>
    public bool SelectBlock()
    {
        return _engine.SelectBlock();
    }

    /// <summary>
    /// Simply calls <see cref="CrossTracker.SelectAll"/>.
    /// </summary>
    public bool SelectAll()
    {
        return _engine.SelectAll();
    }

    private void MorphKind()
    {
        const string NSpace = $"{nameof(LeafView)}.{nameof(MorphKind)}";
        Diag.WriteLine(NSpace, $"New kind: {Format}");

        const double LimitWidthF = 0.75;
        var ch = _engine.Shim.OneCh;
        var lh = _engine.Shim.LineHeight;
        var shim = _engine.Shim;

        // Don't set side margin
        Margin = new(0.0, lh + _topMargin, 0.0, ch);

        Padding = new(ch * 2.0);
        _header0?.FontSize = ChromeFonts.SmallFontSize * Owner.Zoom.Fraction;

        CornerRadius = Owner.LeafCornerRadius;
        MinWidth = Owner.ActualContentWidth * 0.05;

        switch (Format)
        {
            case LeafFormat.UserMessage:
                shim.ForegroundOverride = null;
                Background = Owner.UserBackground;
                Width = double.NaN;
                MaxWidth = Owner.ActualContentWidth * LimitWidthF;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                return;
            case LeafFormat.AssistantMessage:

                shim.ForegroundOverride = null;

                if (Owner.FocusedKind == DeckFormat.Note)
                {
                    shim.ForegroundOverride = ChromeStyling.GrayForeground;
                }

                ClearValue(BackgroundProperty);
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                return;
            case LeafFormat.UserNote:
                shim.ForegroundOverride = null;
                ClearValue(BackgroundProperty);
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                return;
            case LeafFormat.UserAttachment:
            case LeafFormat.AssistantAttachment:
            case LeafFormat.Notification:
                shim.ForegroundOverride = ChromeStyling.GrayForeground;
                ClearValue(BackgroundProperty);
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                return;
            default:
                Diag.Fail($"Invalid {nameof(LeafFormat)}");
                return;
        }
    }

}