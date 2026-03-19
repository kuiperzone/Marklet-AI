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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using Avalonia.Input;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Displays contents of a <see cref="GardenBasket"/> instance.
/// </summary>
internal sealed class BasketToolbar : DockPanel
{
    private readonly LightButton? _rightButton;
    private readonly BasketCase _bcase;

    public BasketToolbar(BasketCase bcase)
    {
        Kind = bcase.Kind;
        _bcase = bcase;

        var owner = bcase.View;
        Garden = owner.Garden;
        Basket = owner.Basket;
        ConditionalDebug.ThrowIfNotEqual(Kind, owner.Kind);
        ConditionalDebug.ThrowIfNotEqual(Kind, Basket.Kind);

        owner.ViewChanged += ViewChangedHandler;

        HorizontalSpacing = 4.0;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

        var leftButton = new LightButton();
        leftButton.Content = Symbols.Menu;
        leftButton.DropMenu = bcase.Menu;
        leftButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        leftButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        SetDock(leftButton, Dock.Left);
        Children.Add(leftButton);

        var head = new TextBlock();
        head.Text = Kind.DisplayName().ToUpperInvariant();
        head.FontSize = ChromeFonts.SmallFontSize;
        head.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        head.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        SetDock(head, Dock.Left);
        Children.Add(head);

        var bar = new LightBar();
        bar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        SetDock(bar, Dock.Right);
        Children.Add(bar);

        SearchButton = bar.AddButton(Symbols.Search, "Search");
        SearchButton.Gesture = new(Key.F, KeyModifiers.Shift | KeyModifiers.Control);
        SearchButton.Classes.Add("accent-checked");

        if (Kind.CanInstigateNew())
        {
            _rightButton = bar.AddButton("New " + Symbols.EditSquare);
            _rightButton.Classes.Add("accent-background");
            _rightButton.Tip = "Start new " + Kind.DefaultDeck().DisplayName(DisplayKind.Lower);
            _rightButton.Click += NewClickHandler;
        }
        else
        if (Kind == BasketKind.Waste)
        {
            _rightButton = bar.AddButton("Empty " + Symbols.DeleteForever, "Empty " + Kind.DisplayName());
            _rightButton.Classes.Add("critical-background");
            _rightButton.Tip = "Empty " + Kind.DisplayName();
            _rightButton.Click += (_, __) => _bcase.ShowPurgeWindow();
        }
    }

    /// <summary>
    /// Gets the <see cref="BasketKind"/> assigned on construction.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Gets the <see cref="MemoryGarden"/> source data.
    /// </summary>
    public MemoryGarden Garden { get; }

    /// <summary>
    /// Gets the <see cref="GardenBasket"/> source data.
    /// </summary>
    public GardenBasket Basket { get; }

    /// <summary>
    /// Gets the search button.
    /// </summary>
    public LightButton SearchButton { get; }

    private void NewClickHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(NewClickHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);
        ConditionalDebug.ThrowIfFalse(Kind.CanInstigateNew());

        if (Kind.CanInstigateNew())
        {
            _bcase.Mission.OnNewClicked(false);
        }
    }

    private void NewEphemeralHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(NewEphemeralHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);
        ConditionalDebug.ThrowIfFalse(Kind.CanInstigateNew());

        if (Kind.CanInstigateNew())
        {
            _bcase.Mission.OnNewClicked(true);
        }
    }

    private void ViewChangedHandler(object? _, EventArgs __)
    {
        var enabled = Garden.Gardener != null;
        SearchButton.IsEnabled = enabled;
        _rightButton?.IsEnabled = enabled;

        if (!enabled)
        {
            _bcase.IsSearching = false;
        }

        var empty = Basket.IsEmpty;
        _bcase.Menu.PruneItem.IsEnabled = !empty;
        _bcase.Menu.EmptyItem.IsEnabled = !empty;

        if (Kind == BasketKind.Waste)
        {
            _rightButton?.IsEnabled = !empty;
        }
    }

}