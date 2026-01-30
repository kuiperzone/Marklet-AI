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

using Avalonia.Media;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Subclass of <see cref="MarkView"/> for use by <see cref="ContentViewer"/>.
/// </summary>
public sealed class ContentLeaf : MarkView, ICrossTrackOwner
{
    private readonly CrossTextBlock _header = new();
    private double _widthLimit;

    /// <summary>
    /// Constructor with shared selection host.
    /// </summary>
    /// <remarks>
    /// The <see cref="Update"/> method must be called after construction.
    /// </remarks>
    public ContentLeaf(ContentViewer owner)
        : base(owner, owner.Tracker)
    {
        // Null intentional in base constructor
        Owner = owner;

        // The timing on when we assign this matters!
        _header.Tracker = owner.Tracker;

        _header.TextWrapping = TextWrapping.NoWrap;
        _header.TextTrimming = TextTrimming.CharacterEllipsis;
        _header.Foreground = ChromeStyling.ForegroundGray;

        _widthLimit = owner.ActualContentWidth;

        ContextMenu = ContentContextMenu.Global;
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public readonly ContentViewer Owner;

    /// <summary>
    /// Gets an indefinite change counter value.
    /// </summary>
    public long ChangeCounter { get; private set; }

    /// <summary>
    /// Gets the leaf kind.
    /// </summary>
    public LeafKind Kind { get; private set; }

    /// <summary>
    /// Update pertainant properties from <see cref="Owner"/>. If "morph" is true, it indicates that morphology may have
    /// changed.
    /// </summary>
    public void Refresh(bool morph)
    {
        // Copy not always needed but inexpensive.
        CopyProperties(Owner);

        if (morph)
        {
            switch (Kind)
            {
                case LeafKind.User:
                    Background = Owner.UserBackground;
                    Foreground = Owner.Foreground;
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                    break;
                case LeafKind.Assistant:
                    ClearValue(BackgroundProperty);
                    Foreground = Owner.Foreground;
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    break;
                case LeafKind.DisplayMessage:
                    ClearValue(BackgroundProperty);
                    Foreground = ChromeStyling.ForegroundGray;
                    HeadingForeground = ChromeStyling.ForegroundGray;
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    break;
                case LeafKind.DisplayError:
                    ClearValue(BackgroundProperty);
                    Foreground = ChromeBrushes.RedLightAccent;
                    HeadingForeground = ChromeBrushes.RedLightAccent;
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    break;
                default:
                    ConditionalDebug.Fail($"Invalid {nameof(LeafKind)}");
                    break;
            }

            CornerRadius = Owner.LeafCornerRadius;
        }

        var ch = OneCh;
        var lh = ScaledLineHeight;

        // Don't set side margin
        Margin = new(0.0, lh, 0.0, ch);

        Padding = new(OneCh * 2.0);
        _header.FontSize = ChromeFonts.SmallFontSize * Zoom.Fraction;
        UpdateWidthLimit(Owner.ActualContentWidth, morph);
        RefreshLook();
    }

    /// <summary>
    /// Update on "leaf" content.
    /// </summary>
    public void Update(GardenLeaf leaf)
    {
        const string NSpace = $"{nameof(ContentLeaf)}.{nameof(Update)}";
        ConditionalDebug.WriteLine(NSpace, $"Current change: {ChangeCounter}, leaf change: {leaf.VisualCounter}");

        // Rely on 64-bit ChangeCounter being unique.
        // We don't need too store the "leaf" reference.
        bool morph = Kind != leaf.Kind || ChangeCounter == 0;
        ConditionalDebug.WriteLine(NSpace, $"Leaf kind: {leaf.Kind}");

        if (morph || ChangeCounter != leaf.VisualCounter)
        {
            Kind = leaf.Kind;
            ChangeCounter = leaf.VisualCounter;

            if (leaf.Model != null)
            {
                Header = _header;
                _header.Text = leaf.Model;
            }
            else
            {
                Header = null;
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
            Refresh(true);
        }
    }

    /// <summary>
    /// Updates the maximum permitted width.
    /// </summary>
    /// <remarks>
    /// This must be called by owner and we don't track property changes in this class.
    /// </remarks>
    public void UpdateWidthLimit(double width, bool force = false)
    {
        if (_widthLimit != width || force)
        {
            _widthLimit = width;

            if (width > 0.0)
            {
                MinWidth = width * 0.05;

                if (Kind == LeafKind.User)
                {
                    MaxWidth = width * 0.75;
                    return;
                }

                MaxWidth = width;
                return;
            }

            // Not normally expected
            MinWidth = 0.0;
            MaxWidth = double.PositiveInfinity;
        }
    }

    private CrossTextBlock? GetLazyHeader(string? model)
    {
        if (model != null)
        {
            _header.Text = model;
            return _header;
        }

        return null;
    }

}