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

using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Windows;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Subclass of <see cref="PixieCard"/> which displays <see cref="GardenDeck"/> header for use with <see
/// cref="BasketView"/>.
/// </summary>
internal sealed class DeckCard : PixieCard
{
    private long _changeCounter = -1;
    private IDeckDrop? _dropTarget;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DeckCard(GardenDeck source, bool isRoot)
    {
        const string NSpace = $"{nameof(DeckCard)}.constructor";
        ConditionalDebug.WriteLine(NSpace, source.Title);

        Source = source;
        IsRootFolder = isRoot;
        source.VisualComponent = this;

        BorderThickness = default;

        RightButton.IsVisible = true;
        RightButton.Content = Symbols.MoreVert;
        RightButton.DropMenu = CardMenu.Get(this);
        IsHoverButton = true;

        Click += (_, __) => Source.IsFocused = true;
        DoubleTapped += (_, __) => ShowPropertiesWindow();
    }

    /// <summary>
    /// Gets the associated source data.
    /// </summary>
    public GardenDeck Source { get; }

    /// <summary>
    /// Gets whether search result.
    /// </summary>
    public bool IsRootFolder { get; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(PixieCard);

    /// <summary>
    /// Refresh view from <see cref="Source"/>, including a friendly time update.
    /// </summary>
    public void RefreshVisual(bool isSearching)
    {
        const string NSpace = $"{nameof(DeckCard)}.{nameof(RefreshVisual)}";

        // Reset
        var f = Source.VisualSignals;
        Source.VisualSignals = SignalFlags.None;

        if (_changeCounter != Source.VisualCounter)
        {
            ConditionalDebug.WriteLine(NSpace, $"REFRESH: {Source.Title}");
            ConditionalDebug.WriteLine(NSpace, $"Searching: {isSearching}");
            _changeCounter = Source.VisualCounter;

            var kind = Source.Kind;
            bool ephem = Source.IsEphemeral;
            var name = kind.DisplayName(DisplayKind.Default, ephem);

            Title = Source.GetTitleOrDefault("New " + name);
            RightSymbol = Source.IsPinned ? Symbols.Keep : null;

            if (ephem || kind != Source.Basket.DefaultDeck())
            {
                var symbol = kind.MaterialSymbol(ephem);
                name = kind.DisplayName(DisplayKind.Upper, ephem);
                Header = $"{symbol} {name}";
            }
            else
            {
                Header = null;
            }
        }

        if (isSearching)
        {
            Footer = Source.SearchSnippet ?? Source.Updated.ToFriendly();
        }
        else
        {
            Footer = Source.Updated.ToFriendly();
        }

        if (f.HasFlag(SignalFlags.ItemAttention))
        {
            Attention(ChromeStyling.PixieHover);
        }
    }

    /// <summary>
    /// Brings up a properties dialog box and returns true if modified.
    /// </summary>
    public async void ShowPropertiesWindow()
    {
        var window = new DeckPropertiesWindow(Source);

        // Dialog handles the change TBD
        await window.ShowDialog(this);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(DeckCard)}.{nameof(OnPointerExited)}";
        base.OnPointerExited(e);

        var info = e.GetCurrentPoint(this);
        var props = info.Properties;

        if (props.IsLeftButtonPressed)
        {
            ConditionalDebug.WriteLine(NSpace, "Capture");
            e.Pointer.Capture(this);
            Cursor = Styling.IsActualThemeDark ? ChromeCursors.DocumentDark48 : ChromeCursors.DocumentLight48;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(DeckCard)}.{nameof(OnPointerMoved)}";
        base.OnPointerMoved(e);

        var captured = e.Pointer.Captured;

        if (captured == this)
        {
            ConditionalDebug.WriteLine(NSpace, "Is captured");
            var target = GetTarget(e);

            // Ensure foreign folder and not self
            if (target != _dropTarget)
            {
                ConditionalDebug.WriteLine(NSpace, "Drag target");
                CancelTarget();
                _dropTarget = target;
                target?.StartDrop(Source);
            }

            return;
        }

        // Reset for edge cases
        CancelTarget();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckCard)}.{nameof(OnPointerExited)}";
        base.OnPointerReleased(e);

        var target = CancelTarget();

        if (e.Pointer.Captured == this)
        {
            ConditionalDebug.WriteLine(NSpace, "Capture released");
            e.Handled = true;
            e.Pointer.Capture(null);
            target?.AcceptDrop(Source);
        }
    }

    private IDeckDrop? CancelTarget()
    {
        var target = _dropTarget;

        _dropTarget = null;
        target?.CancelDrop();

        if (target != null)
        {
            Cursor = null;
        }

        return target;
    }

    private IDeckDrop? GetTarget(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(DeckCard)}.{nameof(GetTarget)}";
        var top = TopLevel.GetTopLevel(this);

        if (top != null && top.InputHitTest(e.GetPosition(top)) is Control control)
        {
            ConditionalDebug.WriteLine(NSpace, "Control: " + control);
            return control.FindLogicalAncestorOfType<IDeckDrop>(true);
        }

        return null;
    }
}
