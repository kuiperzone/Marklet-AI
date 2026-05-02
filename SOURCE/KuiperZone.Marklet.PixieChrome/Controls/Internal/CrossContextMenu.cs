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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Provides common ContextMenu for <see cref="CrossTextBlock"/>.
/// </summary>
internal static class CrossContextMenu
{
    public static readonly ContextMenu Global;

    static CrossContextMenu()
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
    }

    private static void ContextOpenedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(CrossContextMenu)}.{nameof(ContextOpenedHandler)}";
        Diag.WriteLine(NSpace, "Opened menu");

        var parent = Global.FindLogicalAncestorOfType<ICrossTrackOwner>();

        if (parent != null)
        {
            // First menu is Copy
            var copy = (MenuItem)Global.Items[0]!;

            var tracker = parent.Tracker;

            if (tracker != null)
            {
                Diag.WriteLine(NSpace, "Has tracker");
                copy.IsEnabled = tracker.SelectionCount > 0;
                return;
            }

            if (parent is ICrossTrackable cross)
            {
                Diag.WriteLine(NSpace, "Is cross-trackable");
                copy.IsEnabled = cross.HasSelection;
                return;
            }

            copy.IsEnabled = false;
        }
    }

    private static void CopyHandler(object? _, RoutedEventArgs e)
    {
        const string NSpace = $"{nameof(CrossContextMenu)}.{nameof(CopyHandler)}";
        Diag.WriteLine(NSpace, "Copy text");

        e.Handled = true;
        var parent = Global.FindLogicalAncestorOfType<ICrossTrackOwner>();

        if (parent != null)
        {
            var tracker = parent.Tracker;

            if (tracker != null)
            {
                Diag.WriteLine(NSpace, "Has tracker");
                tracker.CopyText(WhatText.SelectedOrNull);
                return;
            }

            if (parent is ICrossTrackable cross && parent is Visual visual)
            {
                Diag.WriteLine(NSpace, "Is cross-trackable");
                visual.CopyToClipboard(cross.GetEffectiveText(WhatText.SelectedOrNull));
                return;
            }
        }
    }

    private static void SelectAllHandler(object? _, RoutedEventArgs e)
    {
        const string NSpace = $"{nameof(CrossContextMenu)}.{nameof(SelectAllHandler)}";
        Diag.WriteLine(NSpace, "Select all");

        e.Handled = true;
        var parent = Global.FindLogicalAncestorOfType<ICrossTrackOwner>();

        if (parent != null)
        {
            var tracker = parent.Tracker;

            if (tracker != null)
            {
                Diag.WriteLine(NSpace, "Has tracker");
                tracker.SelectAll();
                return;
            }

            if (parent is ICrossTrackable cross)
            {
                Diag.WriteLine(NSpace, "Is cross-trackable");
                cross.SelectAll();
                return;
            }
        }
    }

}