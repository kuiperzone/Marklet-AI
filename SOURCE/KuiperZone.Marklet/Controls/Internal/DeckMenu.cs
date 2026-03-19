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
using Avalonia.Input;
using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Controls.Internal;

/// <summary>
/// Provides common ContextMenu for <see cref="DeckViewer"/>.
/// </summary>
public static class DeckMenu
{
    /// <summary>
    /// Gets the shared ContextMenu.
    /// </summary>
    public static readonly ContextMenu Global;

    static DeckMenu()
    {
        Global = new ContextMenu();
        Global.Cursor = ChromeCursors.Arrow;
        Global.Opened += ContextOpenedHandler;

        var item = new MenuItem();
        item.Header = "Copy";
        item.InputGesture = new(Key.C, KeyModifiers.Control);
        item.Click += CopyHandler;
        Global.Items.Add(item);

        Global.Items.Add(new Separator());

        item = new MenuItem();
        item.Header = "Select All";
        item.InputGesture = new(Key.A, KeyModifiers.Control);
        item.Click += SelectAllHandler;
        Global.Items.Add(item);

        item = new MenuItem();
        item.Header = "Select Message";
        item.Click += SelectMessageHandler;
        Global.Items.Add(item);
    }

    private static void ContextOpenedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(DeckMenu)}.{nameof(ContextOpenedHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Opened menu");

        // First menu is Copy
        var parent = Global.GetParentOf<DeckLeaf>();
        ((MenuItem)Global.Items[0]!).IsEnabled = parent?.Tracker.SelectionCount > 0;
    }

    private static void CopyHandler(object? _, RoutedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckMenu)}.{nameof(CopyHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Copy text");

        e.Handled = true;
        Global.GetParentOf<DeckLeaf>()?.Tracker.CopyText(WhatText.SelectedOrNull);
    }

    private static void SelectAllHandler(object? _, RoutedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckMenu)}.{nameof(SelectAllHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Select all");

        e.Handled = true;
        Global.GetParentOf<DeckLeaf>()?.SelectAll();
    }

    private static void SelectMessageHandler(object? _, RoutedEventArgs e)
    {
        const string NSpace = $"{nameof(DeckMenu)}.{nameof(SelectAllHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Select all");

        e.Handled = true;
        Global.GetParentOf<DeckLeaf>()?.SelectBlock();
    }
}
