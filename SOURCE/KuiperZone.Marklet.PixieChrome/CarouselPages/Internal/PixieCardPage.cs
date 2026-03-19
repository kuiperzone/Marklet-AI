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
using Avalonia;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

internal sealed class PixieCardPage : PixiePageBase
{
    public PixieCardPage()
    {
        Title = nameof(PixieCard);
        Symbol = Symbols.KeyboardCommandKey;

        // BUTTON
        var group = NewGroup(nameof(PixieCard));

        var button = NewControl<PixieCard>(group);
        AddAccoutrements(button);
        button.Title += " on Hover";
        button.RightButton.IsVisible = true;
        button.CanToggle = true;
        button.RightButton.Click += ShowDialogClickHandler;
        button.Footer = "Background is checkable";

        button = NewControl<PixieCard>(group);
        button.Title += " Regular";
        button.CanToggle = true;
        button.RightButton.IsVisible = true;
        button.RightButton.Content = Symbols.NewWindow + " Show Dialog";
        button.RightButton.Click += ShowDialogClickHandler;
        button.Footer = "Background is checkable";

        button = NewControl<PixieCard>(group, false);
        button.Title += " Disabled";
        button.RightButton.IsVisible = true;
        button.RightButton.Content = Symbols.NewWindow + " Disabled";
    }

    private static async void ShowDialogClickHandler(object? sender, EventArgs __)
    {
        await ChromeDialog.ShowDialog((Window)TopLevel.GetTopLevel(sender as Visual)!, "Dialog Window");
    }
}