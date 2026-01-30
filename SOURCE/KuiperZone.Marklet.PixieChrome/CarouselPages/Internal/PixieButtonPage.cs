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

using Avalonia.Controls;
using Avalonia;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

internal sealed class PixieButtonPage : PixiePageBase
{
    public PixieButtonPage()
    {
        Title = nameof(PixieButton);
        Symbol = Symbols.KeyboardCommandKey;

        // BUTTON
        var group = NewGroup(nameof(PixieButton));

        var button = NewControl<PixieButton>(group);
        AddAccoutrements(button);
        button.Title += " on Hover";
        button.RightButton.IsVisible = true;
        button.CanToggleBackground = true;
        button.RightButton.Click += ShowDialogClickHandler;
        button.Footer = "Background is checkable";

        button = NewControl<PixieButton>(group);
        button.Title += " Regular";
        button.CanToggleBackground = true;
        button.RightButton.IsVisible = true;
        button.RightButton.Classes.Add("regular-background");
        button.RightButton.Content = Symbols.NewWindow + " Show Dialog";
        button.RightButton.Click += ShowDialogClickHandler;
        button.Footer = "Background is checkable";

        button = NewControl<PixieButton>(group);
        button.Title += " Multiline";
        button.CanToggleBackground = true;
        button.RightButton.IsVisible = true;
        button.RightButton.Classes.Add("regular-background");
        button.RightButton.Content = Symbols.NewWindow + " Show\nDialog";
        button.RightButton.Click += ShowDialogClickHandler;
        button.Footer = "Background is checkable";

        button = NewControl<PixieButton>(group, false);
        button.Title += " Disabled";
        button.RightButton.IsVisible = true;
        button.RightButton.Classes.Add("regular-background");
        button.RightButton.Content = Symbols.NewWindow + " Disabled";
    }

    private static async void ShowDialogClickHandler(object? sender, EventArgs __)
    {
        await MessageDialog.ShowDialog((Window)TopLevel.GetTopLevel(sender as Visual)!, "Dialog Window");
    }
}