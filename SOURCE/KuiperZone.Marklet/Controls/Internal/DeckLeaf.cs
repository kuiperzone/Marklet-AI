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

namespace KuiperZone.Marklet.Controls.Internal;

/// <summary>
/// Subclass of <see cref="MarkView"/> for use by <see cref="DeckViewer"/>.
/// </summary>
public sealed class DeckLeaf : MarkView, ICrossTrackOwner
{
    private CrossTextBlock _header = new();

    /// <summary>
    /// Constructor with shared selection host.
    /// </summary>
    /// <remarks>
    /// The <see cref="Update"/> method must be called after construction.
    /// </remarks>
    public DeckLeaf(DeckViewer owner)
        : base(owner)
    {
        Owner = owner;
        ContextMenu = DeckMenu.Global;

        _header.Tracker = owner.Tracker;
        _header.TextWrapping = TextWrapping.NoWrap;
        _header.TextTrimming = TextTrimming.CharacterEllipsis;
        _header.Foreground = ChromeStyling.GrayForeground;
        _header.FontSize = ChromeFonts.SmallFontSize * Zoom.Fraction;
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public readonly DeckViewer Owner;

    /// <summary>
    /// Gets an indefinite change counter value.
    /// </summary>
    public long ChangeCounter { get; private set; }

    /// <summary>
    /// Gets the leaf kind.
    /// </summary>
    public LeafKind Kind { get; private set; }

    /// <inheritdoc cref="MarkControl.ImmediateRefresh"/>
    public new void ImmediateRefresh()
    {
        // Copy not always needed but inexpensive.
        CopyProperties(Owner);

        CornerRadius = Owner.LeafCornerRadius;
        MinWidth = Owner.ActualContentWidth * 0.05;

        switch (Kind)
        {
            case LeafKind.User:
                Background = Owner.UserBackground;
                Foreground = Owner.Foreground;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                Width = double.NaN;
                MaxWidth = Owner.ActualContentWidth * 0.75;
                break;
            case LeafKind.Assistant:
                ClearValue(BackgroundProperty);
                Foreground = Owner.Foreground;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                break;
            case LeafKind.DisplayMessage:
                ClearValue(BackgroundProperty);
                Foreground = ChromeStyling.GrayForeground;
                HeadingForeground = ChromeStyling.GrayForeground;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                break;
            case LeafKind.DisplayError:
                ClearValue(BackgroundProperty);
                Foreground = ChromeBrushes.RedLightAccent;
                HeadingForeground = ChromeBrushes.RedLightAccent;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                Width = Owner.ActualContentWidth;
                MaxWidth = Owner.ActualContentWidth;
                break;
            default:
                ConditionalDebug.Fail($"Invalid {nameof(LeafKind)}");
                break;
        }

        var ch = OneCh;
        var lh = ScaledLineHeight;

        // Don't set side margin
        Margin = new(0.0, lh, 0.0, ch);

        Padding = new(OneCh * 2.0);
        _header?.FontSize = ChromeFonts.SmallFontSize * Zoom.Fraction;

        base.ImmediateRefresh();
    }

    /// <summary>
    /// Update on "leaf" content.
    /// </summary>
    public void Update(GardenLeaf leaf)
    {
        const string NSpace = $"{nameof(DeckLeaf)}.{nameof(Update)}";
        ConditionalDebug.WriteLine(NSpace, $"Current change: {ChangeCounter}, leaf change: {leaf.VisualCounter}");

        // Rely on 64-bit ChangeCounter being unique.
        // We don't need too store the "leaf" reference.
        bool morph = Kind != leaf.Kind || ChangeCounter == 0;
        ConditionalDebug.WriteLine(NSpace, $"Leaf kind: {leaf.Kind}");

        if (morph || ChangeCounter != leaf.VisualCounter)
        {
            Kind = leaf.Kind;
            ChangeCounter = leaf.VisualCounter;

            var entity = leaf.Assistant ?? leaf.Kind.DefaultEntity();

            if (entity != null)
            {
                Header = _header;
                _header.Text = entity.ToUpperInvariant();
            }
            else
            {
                Header = null;
                _header.Text = null;
            }

            // Set options before Text as the parser will use them.
            Options = leaf.ParseOptions;
            ConditionalDebug.WriteLine(NSpace, $"Parse options: {leaf.ParseOptions}");

            // CONTENT UPDATE
            Content = leaf.Content;
        }

        if (morph)
        {
            ConditionalDebug.WriteLine(NSpace, $"Kind changed");
            ImmediateRefresh();
        }
    }

}