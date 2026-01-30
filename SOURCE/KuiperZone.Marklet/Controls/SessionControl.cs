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

using Avalonia;
using Avalonia.Controls;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using System.Diagnostics.CodeAnalysis;
using KuiperZone.Marklet.PixieChrome.Windows;
using Avalonia.Interactivity;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Subclass of <see cref="PixieButton"/> for use with <see cref="GardenBinView"/>.
/// </summary>
public sealed class SessionControl : PixieButton
{
    private static readonly ContextMenu s_homeMenu;
    private static readonly ContextMenu s_archiveMenu;
    private static readonly ContextMenu s_wasteMenu;
    private static readonly MenuItem s_restoreMenuItem;

    private long _changeCounter = -1;

    static SessionControl()
    {
        // HOME BIN
        s_homeMenu = new ContextMenu();

        var item = new MenuItem();
        item.Header = "Archive";
        //item.InputGesture = new(Key.C, KeyModifiers.Control);
        item.Click += ArchiveClickHandler;
        s_homeMenu.Items.Add(item);

        item = new MenuItem();
        item.Header = "Move to Waste";
        //item.InputGesture = new(Key.A, KeyModifiers.Control);
        item.Click += WasteClickHandler;
        s_homeMenu.Items.Add(item);

        item = new MenuItem();
        item.Header = "Delete Permanently";
        item.Click += DeleteClickHandler;
        s_homeMenu.Items.Add(item);


        // ARCHIVE BIN
        s_archiveMenu = new ContextMenu();

        item = new MenuItem();
        item.Header = "Move to Home";
        //item.InputGesture = new(Key.C, KeyModifiers.Control);
        item.Click += HomeClickHandler;
        s_archiveMenu.Items.Add(item);

        item = new MenuItem();
        item.Header = "Move to Waste";
        //item.InputGesture = new(Key.A, KeyModifiers.Control);
        item.Click += WasteClickHandler;
        s_archiveMenu.Items.Add(item);

        item = new MenuItem();
        item.Header = "Delete Permanently";
        item.Click += DeleteClickHandler;
        s_archiveMenu.Items.Add(item);


        // WASTE BIN
        s_wasteMenu = new ContextMenu();
        s_wasteMenu.Opened += ContextMenuOpened;

        s_restoreMenuItem = new MenuItem();
        s_restoreMenuItem.Header = "Restore";
        //item.InputGesture = new(Key.C, KeyModifiers.Control);
        s_restoreMenuItem.Click += RestoreClickHandler;
        s_wasteMenu.Items.Add(s_restoreMenuItem);

        item = new MenuItem();
        item.Header = "Delete Permanently";
        item.Click += DeleteClickHandler;
        s_wasteMenu.Items.Add(item);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public SessionControl(GardenSession instance)
    {
        const string NSpace = $"{nameof(SessionControl)}.constructor";
        ConditionalDebug.WriteLine(NSpace, instance.Title);

        Source = instance;

        BorderThickness = default;
        RightButton.IsVisible = true;
        //RightButton.Content = Symbols.MoreVert;
        RightButton.DropMenu = GetApplicableMenu(instance);
        //IsChildVisibleOnHoverOnly = true;

        Source = instance;
        instance.VisualTag = this;

        Refresh();
        BackgroundClick += (_, __) => Source.IsSelected = true;
    }

    /// <summary>
    /// Gets the associated source.
    /// </summary>
    public GardenSession Source { get; }

    /// <summary>
    /// Gets whether modified since construction.
    /// </summary>
    public bool IsUpdated { get; private set; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(PixieButton);

    /// <summary>
    /// Returns the more recent of the two.
    /// </summary>
    public static SessionControl? MoreRecent(SessionControl? b0, SessionControl? b1)
    {
        if (b0 == null)
        {
            return b1;
        }

        if (b1 == null)
        {
            return b0;
        }

        if (b0.Source.CompareTo(b1.Source) > -1)
        {
            return b0;
        }

        return b1;
    }

    /// <summary>
    /// Refresh view, including friendly time update.
    /// </summary>
    public void Refresh()
    {
        const string NSpace = $"{nameof(SessionControl)}.{nameof(Refresh)}";

        if (_changeCounter != Source.VisualCounter)
        {
            ConditionalDebug.WriteLine(NSpace, $"REFRESH: {Source.Title}");

            IsUpdated = _changeCounter > -1;
            Title = Source.Title ?? "New conversation";
            _changeCounter = Source.VisualCounter;
        }

        IsBackgroundChecked = Source.IsSelected;
        Footer = Source.UpdateTime.ToFriendlyString();
    }

    /// <summary>
    /// Gets whether this <see cref="Source"/> is a valid member of the given data "bin".
    /// </summary>
    /// <remarks>
    /// The result is false where the button has been transferred to different "bin".
    /// </remarks>
    public bool IsMemberOf([NotNullWhen(true)] GardenBin? bin)
    {
        if (bin != null)
        {
            return Source.Owner == bin.Owner && bin.Owner.GetBin(Source.HomeBin, Source.IsWaste) == bin;
        }

        return false;
    }

    private static ContextMenu GetApplicableMenu(GardenSession session)
    {
        if (session.IsWaste)
        {
            return s_wasteMenu;
        }

        switch (session.HomeBin)
        {
            case BinKind.Archive:
                return s_archiveMenu;
            default:
                return s_homeMenu;
        }
    }

    private static SessionControl? GetClickOwner(object? sender)
    {
        const string NSpace = $"{nameof(SessionControl)}.{nameof(GetClickOwner)}";
        ConditionalDebug.WriteLine(NSpace, $"CLICK SENDER: {sender?.GetType().Name}");

        if (sender is StyledElement element)
        {
            if (element.GetParent<ContextMenu>()?.Tag is SessionControl b0)
            {
                ConditionalDebug.WriteLine(NSpace, "Owner set by key handler");
                return b0;
            }

            var b1 = element.GetParent<SessionControl>();
            ConditionalDebug.WriteLine(NSpace, $"Button title: {b1?.Title}");
            ConditionalDebug.ThrowIfNull(b1);
            return b1;
        }

        ConditionalDebug.Fail("Fail to get sender as StyledElement");
        return null;
    }

    private static void ContextMenuOpened(object? sender, RoutedEventArgs __)
    {
        const string NSpace = $"{nameof(SessionControl)}.{nameof(ContextMenu)}";
        ConditionalDebug.WriteLine(NSpace, $"OPENED: {sender?.GetType().Name}");

        if (sender == s_wasteMenu)
        {
            var owner = GetClickOwner(sender);
            s_restoreMenuItem.Header = "Restore to " + owner?.Source.HomeBin;
        }
    }

    private static void HomeClickHandler(object? sender, EventArgs __)
    {
        GetClickOwner(sender)?.Source.HomeBin = BinKind.Home;
    }

    private static void ArchiveClickHandler(object? sender, EventArgs __)
    {
        GetClickOwner(sender)?.Source.HomeBin = BinKind.Archive;
    }

    private static void WasteClickHandler(object? sender, EventArgs __)
    {
        GetClickOwner(sender)?.Source.IsWaste = true;
    }

    private static void RestoreClickHandler(object? sender, EventArgs __)
    {
        GetClickOwner(sender)?.Source.IsWaste = false;
    }

    private static async void DeleteClickHandler(object? sender, EventArgs __)
    {
        var button = GetClickOwner(sender);

        if (button != null)
        {
            if (await MessageDialog.ShowDialog(button, "Permanently delete conversation?", DialogButtons.Yes | DialogButtons.Cancel) == DialogButtons.Yes)
            {
                button.Source.Delete();
            }
        }
    }
}
