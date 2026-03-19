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
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using Avalonia.Input;
using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Main menu for <see cref="BasketView"/>.
/// </summary>
internal sealed class BasketMenu : ContextMenu
{
    private readonly BasketCase _bcase;
    private readonly MoreBar _folderMore;
    private readonly MoreBar _historyMore;

    /// <summary>
    /// Constructor.
    /// </summary>
    public BasketMenu(BasketCase bcase)
    {
        _bcase = bcase;
        _folderMore = bcase.FolderMore;
        ConditionalDebug.ThrowIfNull(_folderMore);

        _historyMore = bcase.HistoryMore;
        ConditionalDebug.ThrowIfNull(_historyMore);

        Kind = bcase.Kind;
        var deckKind = Kind.DefaultDeck();

        var main = bcase.Mission;
        var view = bcase.View;

        if (Kind.CanInstigateNew())
        {
            var menu = new MenuItem();
            menu.Header = ChromeFonts.NewRunBlock($"{Symbols.EditSquare} New {deckKind.DisplayName(DisplayKind.Default)}");
            menu.InputGesture = BasketKeys.NewGesture;

            // We can handle this directly
            Items.Add(menu);
            menu.Click += (_, __) => main.OnNewClicked();

            if (Kind == BasketKind.Recent)
            {
                // Special entry for this
                menu = new MenuItem();
                menu.Header = ChromeFonts.NewRunBlock($"{Symbols.HourglassBottom} New Ephemeral");
                menu.InputGesture = BasketKeys.EphemeralGesture;
                Items.Add(menu);
                menu.Click += (_, __) => main.OnNewClicked(true);
            }

            Items.Add(new Separator());
        }

        SearchItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Search} Search {deckKind.DisplayName(DisplayKind.Plural)}\u2026");
        SearchItem.InputGesture = BasketKeys.SearchGesture;
        SearchItem.ToggleType = MenuItemToggleType.CheckBox;
        SearchItem.IsChecked = true; // <- leave initial space (will be reset)
        Items.Add(SearchItem);

        PinTopItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Keep} Show Pinned at Top");
        PinTopItem.InputGesture = BasketKeys.PinTopGesture;
        PinTopItem.ToggleType = MenuItemToggleType.CheckBox;
        PinTopItem.IsChecked = true; // <- leave initial space (will be reset)
        PinTopItem.Click += (_, __) => view.IsPinTop = !view.IsPinTop;
        Items.Add(PinTopItem);

        Items.Add(new Separator());

        if (Kind != BasketKind.Waste)
        {
            NewFolderItem.Header = ChromeFonts.NewRunBlock($"{Symbols.CreateNewFolder} New Folder\u2026");
            NewFolderItem.InputGesture = BasketKeys.FolderGesture;
            Items.Add(NewFolderItem);
        }

        ExpandFoldersItem.Header = ChromeFonts.NewRunBlock($"{Symbols.FolderOpen} Expand Folders");
        ExpandFoldersItem.InputGesture = BasketKeys.ExpandGesture;
        ExpandFoldersItem.Click += (_, __) => view.ExpandFolders();
        Items.Add(ExpandFoldersItem);

        CloseFoldersItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Folder} Close Folders");
        CloseFoldersItem.InputGesture = BasketKeys.CloseGesture;
        CloseFoldersItem.Click += (_, __) => view.CloseFolders();
        Items.Add(CloseFoldersItem);

        Items.Add(new Separator());

        if (Kind != BasketKind.Waste)
        {
            PruneItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Delete} Prune {Kind.DisplayName()}\u2026");
            Items.Add(PruneItem);
        }
        else
        {
            EmptyItem.Header = ChromeFonts.NewRunBlock($"{Symbols.DeleteForever} Empty {Kind.DisplayName()}\u2026");
            Items.Add(EmptyItem);
        }

        Opened += OpenedHandler;
    }

    public BasketKind Kind { get; }

    public MenuItem SearchItem { get; } = new();
    public MenuItem PinTopItem { get; } = new();
    public MenuItem NewFolderItem { get; } = new();
    public MenuItem ExpandFoldersItem { get; } = new();
    public MenuItem CloseFoldersItem { get; } = new();
    public MenuItem PruneItem { get; } = new();
    public MenuItem EmptyItem { get; } = new();

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(ContextMenu);

    private void OpenedHandler(object? _, EventArgs __)
    {
        Focus();
        SearchItem.IsChecked = _bcase.IsSearching;
        PinTopItem.IsChecked = _bcase.View.IsPinTop;
        ExpandFoldersItem.IsEnabled = _bcase.FolderHeader.Count > 0;
        CloseFoldersItem.IsEnabled = _bcase.FolderHeader.Count > 0;
    }

}
