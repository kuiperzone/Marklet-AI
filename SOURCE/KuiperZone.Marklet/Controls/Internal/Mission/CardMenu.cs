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
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Abstract base for <see cref="DeckCard"/> context menu.
/// </summary>
internal abstract class CardMenu : ContextMenu
{
    /// <summary>
    /// Constructor.
    /// </summary>
    protected CardMenu()
    {
        Opened += OpenedHandler;

        // Title set on opening
        PinItem.InputGesture = BasketKeys.PinnedGesture;
        PinItem.Click += PinnedHandler;

        TouchItem.Header = ChromeFonts.NewRunBlock($"{Symbols.SwipeUp} Touch");
        TouchItem.InputGesture = BasketKeys.TouchGesture;
        TouchItem.Click += TouchHandler;

        // Title set on opening
        RenameItem.InputGesture = BasketKeys.RenameGesture;
        RenameItem.Click += RenameHandler;

        PropertiesItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Info} Properties");
        PropertiesItem.InputGesture = BasketKeys.PropertiesGesture;
        PropertiesItem.Click += PropertiesHandler;

        ArchiveItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Archive} Move to {BasketKind.Archive.DisplayName()}");
        ArchiveItem.InputGesture = BasketKeys.ArchiveGesture;
        ArchiveItem.Click += ArchiveHandler;

        // Title set on opening
        RestoreItem.InputGesture = BasketKeys.RestoreGesture;
        RestoreItem.Click += RestoreHandler;

        WasteItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Delete} Move to {BasketKind.Waste.DisplayName()}");
        WasteItem.InputGesture = BasketKeys.WasteGesture;
        WasteItem.Click += WasteHandler;

        DeleteItem.Header = ChromeFonts.NewRunBlock($"{Symbols.DeleteForever} Delete Permanently");
        DeleteItem.InputGesture = BasketKeys.DeleteGesture;
        DeleteItem.Click += DeleteHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(ContextMenu);

    /// <summary>
    /// Gets the open caller.
    /// </summary>
    protected DeckCard? Caller { get; private set; }

    /// <summary>
    /// Gets whether to delete immediately.
    /// </summary>
    protected bool ImmediateDelete { get; set; }

    /// <summary>
    /// Gets "Pinned" menu item.
    /// </summary>
    protected readonly MenuItem PinItem = new();

    /// <summary>
    /// Gets "Touch" menu item.
    /// </summary>
    protected readonly MenuItem TouchItem = new();

    /// <summary>
    /// Gets "Rename" menu item.
    /// </summary>
    protected readonly MenuItem RenameItem = new();

    /// <summary>
    /// Gets "properties" menu item.
    /// </summary>
    protected readonly MenuItem PropertiesItem = new();

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
    public static CardMenu Get(DeckCard card)
    {
        switch (card.Source.Basket)
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
                throw new ArgumentException($"Invalid {nameof(BasketKind)} {card.Source.Basket}", nameof(card));
        }
    }

    /// <summary>
    /// Programmatically handles key input for the given card.
    /// </summary>
    public static bool HandleKeyGesture(KeyEventArgs e, DeckCard card)
    {
        var obj = Get(card);

        try
        {
            obj.Init(card);
            return card.HandleKeyGesture(e);
        }
        finally
        {
            obj.Caller = null;
        }
    }

    /// <summary>
    /// Opens the menu with <see cref="PlacementMode.Pointer"/> at the "card".
    /// </summary>
    public static bool OpenAt(DeckCard? card)
    {
        if (card == null)
        {
            return false;
        }

        var menu = Get(card);

        if (menu.Normalize())
        {
            menu.HorizontalOffset = 0;
            menu.VerticalOffset = 0;
            menu.Placement = PlacementMode.Pointer;
            menu.Open(card);
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
    }

    private void Init(DeckCard? caller)
    {
        Caller = caller;

        if (caller == null)
        {
            return;
        }

        var deck = caller.Source;
        var name = deck.Kind.DisplayName(DisplayKind.Default, deck.IsEphemeral);
        RenameItem.Header = ChromeFonts.NewRunBlock($"{Symbols.Edit} Rename {name}");

        PinItem.IsChecked = deck.IsPinned;
        PinItem.Header = ChromeFonts.NewRunBlock(deck.IsPinned ? $"{Symbols.KeepOff} Remove Pin" : $"{Symbols.Keep} Add Pin");

        RestoreItem.IsVisible = deck.Basket != deck.Origin;
        RestoreItem.Header = ChromeFonts.NewRunBlock($"{Symbols.RestorePage} Restore to {deck.Origin.DisplayName()}");
    }

    private void OpenedHandler(object? _, EventArgs __)
    {
        Init(this.FindLogicalAncestorOfType<DeckCard>());
        ConditionalDebug.ThrowIfNull(Caller);
        Focus();
    }

    private void PinnedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(CardMenu)}.{nameof(PinnedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "CALLER: " + Caller);
        ConditionalDebug.ThrowIfNull(Caller);
        Caller?.Source.IsPinned = !Caller.Source.IsPinned;
    }

    private void TouchHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(CardMenu)}.{nameof(TouchHandler)}";
        ConditionalDebug.WriteLine(NSpace, "CALLER: " + Caller);
        ConditionalDebug.ThrowIfNull(Caller);
        Caller?.Source.Touch();
    }

    private void RenameHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(CardMenu)}.{nameof(RenameHandler)}";
        ConditionalDebug.WriteLine(NSpace, "CALLER: " + Caller);
        ConditionalDebug.ThrowIfNull(Caller);
        Caller?.StartRename(MemoryGarden.MaxMetaLength);
    }

    private void PropertiesHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(CardMenu)}.{nameof(PropertiesHandler)}";
        ConditionalDebug.WriteLine(NSpace, "CALLER: " + Caller);
        ConditionalDebug.ThrowIfNull(Caller);
        Caller?.ShowPropertiesWindow();
    }

    private void RestoreHandler(object? _, EventArgs __)
    {
        if (Caller != null)
        {
            var src = Caller.Source;
            src.Basket = src.Origin;
            src.VisualSignals |= SignalFlags.ItemAttention | SignalFlags.OpenFolder;
            return;
        }

        ConditionalDebug.Fail("Failed to find Caller");
        return;
    }

    private void ArchiveHandler(object? _, EventArgs __)
    {
        if (Caller != null)
        {
            var src = Caller.Source;
            src.Basket = BasketKind.Archive;
            src.VisualSignals |= SignalFlags.ItemAttention | SignalFlags.OpenFolder;
            return;
        }

        ConditionalDebug.Fail("Failed to find Caller");
        return;
    }

    private void WasteHandler(object? _, EventArgs __)
    {
        if (Caller != null)
        {
            var src = Caller.Source;
            src.Basket = BasketKind.Waste;
            return;
        }

        ConditionalDebug.Fail("Failed to find Caller");
        return;
    }

    private async void DeleteHandler(object? _, EventArgs __)
    {
        // Get reference before Close()
        var caller = Caller;

        if (caller != null)
        {
            if (ImmediateDelete)
            {
                caller.Source.Delete();
                return;
            }

            var window = new ChromeDialog();

            var kind = caller.Source.Kind.DisplayName(DisplayKind.Lower, false);
            window.Message = $"Delete {kind} permanently?";
            window.Details = "\n" + caller.Source.Title;
            window.Buttons = DialogButtons.Delete | DialogButtons.Cancel;
            await window.ShowDialog(caller);

            if (window.IsPositiveResult)
            {
                caller.Source.Delete();
            }
        }
    }

    private sealed class HomeMenu : CardMenu
    {
        public static readonly CardMenu Global = new HomeMenu();

        private HomeMenu()
        {
            Items.Add(PinItem);
            Items.Add(RenameItem);
            Items.Add(TouchItem);
            Items.Add(PropertiesItem);

            Items.Add(new Separator());

            Items.Add(ArchiveItem);
            Items.Add(WasteItem);

            Items.Add(new Separator());

            Items.Add(DeleteItem);
        }
    }

    private sealed class NotesMenu : CardMenu
    {
        public static readonly CardMenu Global = new NotesMenu();

        private NotesMenu()
        {
            Items.Add(PinItem);
            Items.Add(RenameItem);
            Items.Add(TouchItem);
            Items.Add(PropertiesItem);

            Items.Add(new Separator());

            Items.Add(ArchiveItem);
            Items.Add(WasteItem);

            Items.Add(new Separator());

            Items.Add(DeleteItem);
        }
    }

    private sealed class ArchiveMenu : CardMenu
    {
        public static readonly CardMenu Global = new ArchiveMenu();

        private ArchiveMenu()
        {
            Items.Add(PinItem);
            Items.Add(RenameItem);
            Items.Add(TouchItem);
            Items.Add(PropertiesItem);

            Items.Add(new Separator());

            Items.Add(RestoreItem);
            Items.Add(WasteItem);

            Items.Add(new Separator());

            Items.Add(DeleteItem);
        }
    }

    private sealed class WasteMenu : CardMenu
    {
        public static readonly CardMenu Global = new WasteMenu();

        private WasteMenu()
        {
            ImmediateDelete = true;
            Opened += OpenedHandler;

            Items.Add(TouchItem);
            Items.Add(PropertiesItem);

            Items.Add(new Separator());

            Items.Add(RestoreItem);
            Items.Add(DeleteItem);
        }
    }

}
