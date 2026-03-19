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

using Avalonia.Controls;
using KuiperZone.Marklet.Stack.Garden;
using Avalonia;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Windows;
using KuiperZone.Marklet.Controls.Internal.Mission;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Subclass of <see cref="LightButton"/> used by <see cref="BufferBar"/>.
/// </summary>
internal sealed class BufferButton : LightButton, IDeckDrop
{
    private const double CheckedWidth = 4.0;
    private const double ButtonFontSize = ChromeFonts.HugeSymbolFontSize;
    private static readonly ChromeStyling Styling = ChromeStyling.Global;

    public const double ButtonSize = CheckedWidth + MinBoxSize * ButtonFontSize / ChromeFonts.SymbolFontSize;

    /// <summary>
    /// Basket constructor.
    /// </summary>
    public BufferButton(BasketKind basket)
        : this()
    {
        Basket = basket;

        CanToggle = true;
        Content = basket.MaterialSymbol();
        VerticalAlignment = VerticalAlignment.Top;

        CheckedChanged += CheckedChangedHandler;
    }

    /// <summary>
    /// Non-basket constructor.
    /// </summary>
    public BufferButton(string symbol, string? tip)
        : this()
    {
        Content = symbol;
        Tip = tip;
        VerticalAlignment = VerticalAlignment.Bottom;
    }

    private BufferButton()
    {
        FontSize = ButtonFontSize;
        MinWidth = ButtonSize;
        MinHeight = ButtonSize;
        BorderThickness = new(CheckedWidth, 0.0, 0.0, 0.0);
        HorizontalAlignment = HorizontalAlignment.Center;

        Foreground = ChromeStyling.GrayForeground;
        HoverBackground = ChromeBrushes.Transparent;
        CheckedPressedBackground = ChromeBrushes.Transparent;
    }

    /// <summary>
    /// Gets the basket code.
    /// </summary>
    public BasketKind Basket { get; }

    /// <inheritdoc cref="IDeckDrop.CanDrop"/>
    public bool CanDrop(object obj)
    {
        return GetDropDeck(obj) != null || GetDropFolder(obj) != null;
    }

    /// <inheritdoc cref="IDeckDrop.CancelDrop"/>
    public void CancelDrop()
    {
        Background = null;
    }

    /// <inheritdoc cref="IDeckDrop.StartDrop"/>
    public void StartDrop(object obj)
    {
        if (CanToggle)
        {
            Background = CanDrop(obj) ? ChromeBrushes.ReadyHover : ChromeBrushes.CriticalHover;
        }
    }

    /// <inheritdoc cref="IDeckDrop.AcceptDrop"/>
    public bool AcceptDrop(object obj)
    {
        CancelDrop();
        var deck = GetDropDeck(obj);

        if (deck != null)
        {
            // That's it. The event handlers will take care of themselves.
            deck.Basket = Basket;
            deck.VisualSignals |= SignalFlags.ItemAttention | SignalFlags.OpenFolder;
            return true;
        }

        var folder = GetDropFolder(obj);

        if (folder != null)
        {
            // Move to this
            var owner = folder.Owner;
            owner.Garden.GetBasket(owner.Kind).MoveBasket(folder.FolderName, Basket);
            return true;
        }

        return false;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        StylingChangedHandler(null, EventArgs.Empty);
        ChromeStyling.Global.StylingChanged += StylingChangedHandler;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ChromeStyling.Global.StylingChanged -= StylingChangedHandler;
    }

    private GardenDeck? GetDropDeck(object obj)
    {
        if (!CanToggle || Basket == BasketKind.None)
        {
            return null;
        }

        if (obj is GardenDeck deck)
        {
            if (Basket != deck.Basket && (Basket == deck.Origin || Basket == BasketKind.Waste || Basket == BasketKind.Archive))
            {
                return deck;
            }

            return null;
        }

        if (obj is DeckCard card)
        {
            deck = card.Source;

            if (Basket != deck.Basket && (Basket == deck.Origin || Basket == BasketKind.Waste || Basket == BasketKind.Archive))
            {
                return deck;
            }
        }

        return null;
    }

    private FolderView? GetDropFolder(object obj)
    {
        if (!CanToggle || Basket == BasketKind.None)
        {
            return null;
        }

        if (obj is FolderView view && view.FolderName != null)
        {
            if (Basket != view.Owner.Kind && (Basket == BasketKind.Waste || Basket == BasketKind.Archive))
            {
                return view;
            }
        }

        return null;
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        // Assume dark background
        var foreground = ChromeBrushes.White;

        HoverForeground = foreground;
        CheckedPressedForeground = foreground;
        FocusBorderBrush = Styling.AccentBrush;
        BorderBrush = IsChecked ? Styling.AccentBrush : null;
    }

    private void CheckedChangedHandler(object? _, RoutedEventArgs __)
    {
        BorderBrush = IsChecked ? Styling.AccentBrush : null;
    }
}
