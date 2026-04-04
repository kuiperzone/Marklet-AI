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
using KuiperZone.Marklet.Windows;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Displays contents of a <see cref="GardenBasket"/> instance.
/// </summary>
internal sealed class BasketToolbar : DockPanel
{
    private readonly LightButton _leftButton = new();
    private readonly LightButton? _rightButton;
    private readonly LightBar _buttons = new();

    public BasketToolbar(BasketView view)
    {
        Kind = view.Kind;
        Basket = view.Basket;
        View = view;
        ConditionalDebug.ThrowIfNotEqual(view.Kind, Basket.Kind);

        HorizontalSpacing = 4.0;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

        Menu = new(this);

        _leftButton.Content = Symbols.Menu;
        _leftButton.DropMenu = Menu;
        _leftButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _leftButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        SetDock(_leftButton, Dock.Left);
        Children.Add(_leftButton);

        var head = new TextBlock();
        head.Text = Kind.DisplayName().ToUpperInvariant();
        head.FontSize = ChromeFonts.SmallFontSize;
        head.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        head.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        SetDock(head, Dock.Left);
        Children.Add(head);

        _buttons.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        SetDock(_buttons, Dock.Right);
        Children.Add(_buttons);

        Menu.SearchItem.Click += (_, __) => view.ToggleSearch();
        Menu.NewFolderItem.Click += (_, __) => view.ShowNewFolderAsync();
        Menu.PruneItem.Click += (_, __) => ShowPruningWindow();
        Menu.EmptyItem.Click += (_, __) => ShowPurgeWindow();

        SearchButton = _buttons.AddButton(Symbols.Search, "Search");
        SearchButton.Gesture = new(Key.F, KeyModifiers.Shift | KeyModifiers.Control);
        SearchButton.Classes.Add("accent-checked");
        SearchButton.Click += (_, __) => view.ToggleSearch();

        if (Kind.CanInstigateNew())
        {
            _rightButton = _buttons.AddButton("New " + Symbols.EditSquare);
            _rightButton.Classes.Add("accent-background");
            _rightButton.Tip = "Start new " + Kind.DefaultDeck().DisplayName(DisplayKind.Lower);
            _rightButton.Click += NewClickHandler;
        }
        else
        if (Kind == BasketKind.Waste)
        {
            _rightButton = _buttons.AddButton("Empty " + Symbols.DeleteForever, "Empty " + Kind.DisplayName());
            _rightButton.Classes.Add("critical-background");
            _rightButton.Tip = "Empty " + Kind.DisplayName();
            _rightButton.Click += (_, __) => ShowPurgeWindow();
        }

        view.ViewChanged += ViewChangedHandler;
    }

    /// <summary>
    /// Gets the <see cref="BasketKind"/> assigned on construction.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Gets the <see cref="GardenBasket"/> source data.
    /// </summary>
    public GardenBasket Basket { get; }

    /// <summary>
    /// Gets the view.
    /// </summary>
    public BasketView View { get; }

    /// <summary>
    /// Gets the drop menu.
    /// </summary>
    public readonly BasketMenu Menu;

    /// <summary>
    /// Gets the search button.
    /// </summary>
    public LightButton SearchButton { get; }

    /// <summary>
    /// Handles button key gestures.
    /// </summary>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        if (_leftButton.HandleKeyGesture(e))
        {
            return true;
        }

        return _buttons.HandleKeyGesture(e);
    }

    private void NewClickHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(NewClickHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);
        ConditionalDebug.ThrowIfFalse(Kind.CanInstigateNew());

        if (Kind.CanInstigateNew())
        {
            View.Mission.OnNewClicked(false);
        }
    }

    /// <summary>
    /// Shows pruning dialog window.
    /// </summary>
    public async void ShowPruningWindow()
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(ShowPruningWindow)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);

        var basket = View.Basket;
        var window = new PruneWindow(basket);
        await window.ShowDialog(this);

        if (window.IsPositiveResult)
        {
            basket.Prune(window.Options);
        }
    }

    /// <summary>
    /// Shows empty basket dialog window.
    /// </summary>
    public async void ShowPurgeWindow()
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(ShowPurgeWindow)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);

        var basket = View.Basket;
        var confirm = new ChromeDialog();
        confirm.Message = $"Empty {Kind} now?";
        confirm.Details = "All items will be permanently deleted.";
        confirm.Buttons = DialogButtons.Delete | DialogButtons.Cancel;

        if (await confirm.ShowDialog(this) == DialogButtons.Delete)
        {
            basket.DeleteAll();
        }
    }

    private void NewEphemeralHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(NewEphemeralHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);
        ConditionalDebug.ThrowIfFalse(Kind.CanInstigateNew());

        if (Kind.CanInstigateNew())
        {
            View.Mission.OnNewClicked(true);
        }
    }

    private void ViewChangedHandler(object? _, EventArgs __)
    {
        var empty = Basket.IsEmpty;
        Menu.PruneItem.IsEnabled = !empty;
        Menu.EmptyItem.IsEnabled = !empty;

        if (Kind == BasketKind.Waste)
        {
            _rightButton?.IsEnabled = !empty;
        }
    }

}