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
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using Avalonia.LogicalTree;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Abstract base for <see cref="DeckCard"/> context menu.
/// </summary>
internal abstract class FolderMenu : ContextMenu
{
    /// <summary>
    /// Constructor.
    /// </summary>
    protected FolderMenu(BasketKind kind)
    {
        Kind = kind;
        Opened += OpenedHandler;

        RenameItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.Edit} Rename Folder");
        RenameItem.Click += RenameHandler;

        NewChatItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.Chat} New {DeckFormat.Chat.DisplayName(DisplayStyle.Default)} Here");
        NewChatItem.Click += NewChatHandler;

        NewNoteItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.EditNote} New {DeckFormat.Note.DisplayName(DisplayStyle.Default)} Here");
        NewNoteItem.Click += NewNoteHandler;

        ArchiveItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.Archive} Move to {BasketKind.Archive.DisplayName()}");
        ArchiveItem.Click += ArchiveHandler;

        WasteItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.Delete} Move to {BasketKind.Waste.DisplayName()}");
        WasteItem.Click += WasteHandler;

        RestoreItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.RestorePage} Restore Folder");
        RestoreItem.Click += RestoreHandler;

        DeleteItem.Header = ChromeFonts.CreateRunBlock($"{Symbols.DeleteForever} Delete Permanently");
        DeleteItem.Click += DeleteHandler;
    }

    /// <summary>
    /// Gets the <see cref="BasketKind"/> pertaining to the basket.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(ContextMenu);

    /// <summary>
    /// Gets the open caller.
    /// </summary>
    protected FolderView? Caller { get; private set; }

    /// <summary>
    /// Gets the source basket.
    /// </summary>
    /// <remarks>
    /// We can't know the instance here until the menu is opened, but we expect it to be same as <see cref="Kind"/>.
    /// </remarks>
    protected GardenBasket? Basket { get; private set; }

    /// <summary>
    /// Gets whether to confirm deletion.
    /// </summary>
    protected bool ImmediateDelete { get; set; }

    /// <summary>
    /// Gets "Rename" menu item.
    /// </summary>
    protected readonly MenuItem RenameItem = new();

    /// <summary>
    /// Gets "New Chat" menu item.
    /// </summary>
    protected readonly MenuItem NewChatItem = new();

    /// <summary>
    /// Gets "New Note" menu item.
    /// </summary>
    protected readonly MenuItem NewNoteItem = new();

    /// <summary>
    /// Gets "Archive" menu item.
    /// </summary>
    protected readonly MenuItem ArchiveItem = new();

    /// <summary>
    /// Gets "restore" menu item.
    /// </summary>
    protected readonly MenuItem RestoreItem = new();

    /// <summary>
    /// Gets "Waste" menu item.
    /// </summary>
    protected readonly MenuItem WasteItem = new();

    /// <summary>
    /// Gets "Delete" menu item.
    /// </summary>
    protected readonly MenuItem DeleteItem = new();

    /// <summary>
    /// Gets applicable context.
    /// </summary>
    public static FolderMenu Get(BasketKind kind)
    {
        switch (kind)
        {
            case BasketKind.Recent:
                HomeMenu.Global.Close();
                return HomeMenu.Global;
            case BasketKind.Notes:
                NotesMenu.Global.Close();
                return NotesMenu.Global;
            case BasketKind.Archive:
                ArchiveMenu.Global.Close();
                return ArchiveMenu.Global;
            case BasketKind.Waste:
                WasteMenu.Global.Close();
                return WasteMenu.Global;
            default:
                throw new ArgumentException($"Invalid {nameof(BasketKind)} {kind}", nameof(kind));
        }
    }

    /// <summary>
    /// Opens the menu with <see cref="PlacementMode.Pointer"/> at the "control".
    /// </summary>
    public static bool OpenAt(BasketKind kind, Control? control)
    {
        if (control == null)
        {
            return false;
        }

        var menu = Get(kind);

        if (menu.Normalize())
        {
            menu.HorizontalOffset = 0;
            menu.VerticalOffset = 0;
            menu.Placement = PlacementMode.Pointer;
            menu.Open(control);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void Close()
    {
        base.Close();
        Caller = null;
        Basket = null;
    }

    private void SetSignal(string? folder, SignalFlags flags)
    {
        var contents = Basket?.GetFolderContents(folder);

        if (contents?.Count > 0)
        {
            // Only need one
            contents[0].VisualSignals = flags;
        }
    }

    private void OpenedHandler(object? _, EventArgs __)
    {
        Basket = null;
        Caller = this.FindLogicalAncestorOfType<FolderView>();
        Diag.ThrowIfNotEqual(Kind, Caller?.Owner.Kind);

        if (Caller?.Owner is BasketView view)
        {
            // Not expected for search results here
            Basket = view.Basket;

            int count = Basket?.CountInFolder(Caller?.FolderName) ?? 0;
            ArchiveItem.IsEnabled = count != 0;
            WasteItem.IsEnabled = count != 0;
            RestoreItem.IsEnabled = count != 0;
        }

        Focus();
    }

    private void RenameHandler(object? _, EventArgs __)
    {
        Caller?.StartRename();
    }

    private void NewChatHandler(object? _, EventArgs __)
    {
        if (Caller != null && Basket != null)
        {
            var item = new GardenDeck(DeckFormat.Chat, Kind);
            item.IsFocused = true;
            item.Folder = Caller.FolderName;
            item.VisualSignals = SignalFlags.OpenFolder;
            Basket.Garden.Insert(item);
        }
    }

    private void NewNoteHandler(object? _, EventArgs __)
    {
        if (Caller != null && Basket != null)
        {
            var item = new GardenDeck(DeckFormat.Note, Kind);
            item.IsFocused = true;
            item.Folder = Caller.FolderName;
            item.VisualSignals = SignalFlags.OpenFolder;
            Basket.Garden.Insert(item);
        }
    }

    private void RestoreHandler(object? _, EventArgs __)
    {
        if (Caller?.FolderName != null)
        {
            var sig = Caller.IsOpen ? SignalFlags.FolderAttention | SignalFlags.OpenFolder : SignalFlags.FolderAttention;
            SetSignal(Caller.FolderName, sig);
            Basket!.RestoreFolder(Caller.FolderName);
        }
    }

    private void ArchiveHandler(object? _, EventArgs __)
    {
        if (Caller?.FolderName != null)
        {
            var sig = Caller.IsOpen ? SignalFlags.FolderAttention | SignalFlags.OpenFolder : SignalFlags.FolderAttention;
            SetSignal(Caller.FolderName, sig);
            Basket!.MoveBasket(Caller.FolderName, BasketKind.Archive);
        }
    }

    private void WasteHandler(object? _, EventArgs __)
    {
        if (Caller?.FolderName != null)
        {
            SetSignal(Caller.FolderName, SignalFlags.FolderAttention);
            Basket!.MoveBasket(Caller.FolderName, BasketKind.Waste);
        }
    }

    private async void DeleteHandler(object? _, EventArgs __)
    {
        // Get reference before Close()
        var caller = Caller;
        var basket = Basket;

        if (caller?.FolderName != null && basket != null)
        {
            int count = basket.CountInFolder(caller.FolderName);

            if (ImmediateDelete || count == 0)
            {
                basket.DeleteFolder(caller.FolderName);
                return;
            }

            var window = new ChromeDialog();
            window.Message = $"Delete folder permanently?";
            window.Details = "\n" + caller.FolderName + $"\n\nThis will delete {count} items(s).";
            window.Buttons = DialogButtons.Delete | DialogButtons.Cancel;
            await window.ShowDialog(caller);

            if (window.IsPositiveResult)
            {
                basket.DeleteFolder(caller.FolderName);
            }
        }
    }

    private sealed class HomeMenu : FolderMenu
    {
        public static readonly FolderMenu Global = new HomeMenu();

        private HomeMenu()
            : base(BasketKind.Recent)
        {
            Items.Add(RenameItem);

            Items.Add(new Separator());

            Items.Add(NewChatItem);
            Items.Add(NewNoteItem);

            Items.Add(new Separator());

            Items.Add(ArchiveItem);
            Items.Add(WasteItem);

            Items.Add(new Separator());

            Items.Add(DeleteItem);
        }
    }

    private sealed class NotesMenu : FolderMenu
    {
        public static readonly FolderMenu Global = new NotesMenu();

        private NotesMenu()
            : base(BasketKind.Notes)
        {
            Items.Add(RenameItem);

            Items.Add(new Separator());

            Items.Add(NewNoteItem);

            Items.Add(new Separator());

            Items.Add(ArchiveItem);
            Items.Add(WasteItem);

            Items.Add(new Separator());

            Items.Add(DeleteItem);
        }
    }

    private sealed class ArchiveMenu : FolderMenu
    {
        public static readonly FolderMenu Global = new ArchiveMenu();

        private ArchiveMenu()
            : base(BasketKind.Archive)
        {
            Items.Add(RenameItem);

            Items.Add(new Separator());

            Items.Add(RestoreItem);
            Items.Add(WasteItem);

            Items.Add(new Separator());

            Items.Add(DeleteItem);
        }
    }

    private sealed class WasteMenu : FolderMenu
    {
        public static readonly FolderMenu Global = new WasteMenu();

        private WasteMenu()
            : base(BasketKind.Waste)
        {
            ImmediateDelete = true;
            Items.Add(RestoreItem);
            Items.Add(DeleteItem);
        }
    }

}
