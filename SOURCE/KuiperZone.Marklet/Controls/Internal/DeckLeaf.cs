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
internal sealed class DeckLeaf : Border, ICrossTrackOwner
{
    private readonly MarkEngine _engine;
    private readonly StackPanel _panel = new();
    private readonly CrossTextBlock _header0 = new();
    private CrossTextBlock? _header1;
    private MarkDocument? _source;
    private long _changeCounter;
    private double _topMargin;

    /// <summary>
    /// Constructor with shared selection host.
    /// </summary>
    /// <remarks>
    /// The <see cref="Rebuild"/> method must be called after construction.
    /// </remarks>
    public DeckLeaf(DeckViewer owner, double topMargin)
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
    /// Gets the leaf kind.
    /// </summary>
    public LeafKind Kind { get; private set; }

    /// <summary>
    /// Gets the number instances of <see cref="GardenGrounds.Find"/> determined at last <see cref="Rebuild"/> call.
    /// </summary>
    public int FindCount { get; private set; }

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
        const string NSpace = $"{nameof(DeckLeaf)}.{nameof(Rebuild)}";
        ConditionalDebug.WriteLine(NSpace, $"Change counters: {_changeCounter}, leaf: {leaf.VisualCounter}");

        bool change = false;

        // Rely on 64-bit ChangeCounter being unique and 0 on initialization.
        if (Kind != leaf.Kind || _changeCounter == 0)
        {
            // Must do this before Rebuild below
            change = true;
            Kind = leaf.Kind;
            MorphKind();
        }

        ConditionalDebug.WriteLine(NSpace, $"Leaf kind: {leaf.Kind}");

        if (focused || change || _changeCounter != leaf.VisualCounter)
        {
            change = true;
            _changeCounter = leaf.VisualCounter;

            _header1 = null;
            var entity = leaf.Kind.DefaultEntity();

            if (leaf.Assistant != null && leaf.Kind == LeafKind.Assistant)
            {
                entity = leaf.Assistant;
            }

            if (entity != null)
            {
                _header0.Text = entity.ToUpperInvariant();
                _header1 = _header0;
            }

            ConditionalDebug.WriteLine(NSpace, $"Parse options: {leaf.ParseOptions}");

            // PROCESS MARKDOWN
            _source = new(leaf.Content, leaf.ParseOptions);
        }

        if (_source == null)
        {
            FindCount = 0;
            return;
        }

        if (change || finding)
        {
            FindCount = 0;

            var document = _source;
             var f = GardenGrounds.Find;

            if (f?.Subtext != null)
            {
                // Find / search
                document = _source.Find(f.Subtext, f.Flags, out int counter) ?? _source;
                FindCount = counter;
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
        const string NSpace = $"{nameof(DeckLeaf)}.{nameof(MorphKind)}";
        ConditionalDebug.WriteLine(NSpace, $"New kind: {Kind}");

        var ch = _engine.Shim.OneCh;
        var lh = _engine.Shim.LineHeight;

        // Don't set side margin
        Margin = new(0.0, lh + _topMargin, 0.0, ch);

        Padding = new(ch * 2.0);
        _header0?.FontSize = ChromeFonts.SmallFontSize * Owner.Zoom.Fraction;

        CornerRadius = Owner.LeafCornerRadius;
        MinWidth = Owner.ActualContentWidth * 0.05;

        switch (Kind)
        {
            case LeafKind.User:
                _engine.Shim.ForegroundOverride = null;
                Background = Owner.UserBackground;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                Width = double.NaN;
                MaxWidth = Owner.ActualContentWidth * 0.75;
                break;
            case LeafKind.Assistant:
                _engine.Shim.ForegroundOverride = null;
                ClearValue(BackgroundProperty);
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                break;
            case LeafKind.PersistantMessage:
            case LeafKind.EphemeralMessage:
                _engine.Shim.ForegroundOverride = ChromeStyling.GrayForeground;
                ClearValue(BackgroundProperty);
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                break;
            default:
                ConditionalDebug.Fail($"Invalid {nameof(LeafKind)}");
                break;
        }
    }

}