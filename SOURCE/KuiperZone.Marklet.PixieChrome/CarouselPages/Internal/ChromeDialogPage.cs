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

using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

internal sealed class ChromeDialogPage : PixiePageBase
{
    private readonly PixieCard _messageButton;
    private readonly PixieCard _exceptionButton;
    private readonly PixieCard _childrenButton;

    public ChromeDialogPage()
    {
        Title = nameof(ChromeDialog);
        Symbol = Symbols.Window;

        // BUTTON
        var group = NewGroup(nameof(PixieCard));
        group.IsCollapsible = false;

        var control = NewControl<PixieCard>(group, "Message");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowSimpleClickHandler;
        control.Footer = "Result: ";
        _messageButton = control;

        control = NewControl<PixieCard>(group, "Exception");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowExceptionClickHandler;
        control.Footer = "Result: ";
        _exceptionButton = control;

        control = NewControl<PixieCard>(group, "Children");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowChildrenClickHandler;
        control.Footer = "Result: ";
        _childrenButton = control;

        control = NewControl<PixieCard>(group, "Scrolling Children");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowScrollingChildrenClickHandler;
        control.Footer = "Result: ";

        control = NewControl<PixieCard>(group, "No Buttons");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowNoButtonsClickHandler;
    }

    private async void ShowSimpleClickHandler(object? sender, EventArgs __)
    {
        var rslt = await ChromeDialog.ShowDialog(sender, "Pone exempli gratia bona", DialogButtons.Proceed | DialogButtons.Ok | DialogButtons.Cancel);
        _messageButton.Footer = "Result: " + rslt;
    }

    private async void ShowExceptionClickHandler(object? sender, EventArgs __)
    {
        try
        {
            throw new InvalidOperationException("Test error");
        }
        catch (Exception ex)
        {
            var rslt = await ChromeDialog.ShowDialog(sender, ex);
            _exceptionButton.Footer = "Result: " + rslt;
        }
    }

    private async void ShowChildrenClickHandler(object? sender, EventArgs __)
    {
        var window = new ChromeDialog();
        window.Title = ChromeApplication.Current.Name;
        window.Message = $"{nameof(PixieControl)} Children";
        window.Buttons = DialogButtons.Proceed | DialogButtons.Ok | DialogButtons.Cancel;

        window.Children.Add(NewControl<PixieEditor>(null));
        window.Children.Add(NewControl<PixieSwitch>(null));
        window.Children.Add(NewControl<PixieCheckBox>(null));
        var rslt = await window.ShowDialog(sender);
        _childrenButton.Footer = "Result: " + rslt;
    }

    private async void ShowScrollingChildrenClickHandler(object? sender, EventArgs __)
    {
        var window = new ChromeDialog();
        window.Title = ChromeApplication.Current.Name;
        window.Message = $"{nameof(PixieControl)} Children";
        window.Buttons = DialogButtons.Proceed | DialogButtons.Ok | DialogButtons.Cancel;

        window.Children.Add(NewControl<PixieAccentPicker>(null));

        for (int n = 0; n < 6; ++n)
        {
            window.Children.Add(NewControl<PixieEditor>(null));
            window.Children.Add(NewControl<PixieSwitch>(null));
            window.Children.Add(NewControl<PixieCheckBox>(null));
        }

        var rslt = await window.ShowDialog(sender);
        _childrenButton.Footer = "Result: " + rslt;
    }

    private async void ShowNoButtonsClickHandler(object? sender, EventArgs __)
    {
        await ChromeDialog.ShowDialog(sender, "Pone exempli gratia bona", DialogButtons.None);
    }

}