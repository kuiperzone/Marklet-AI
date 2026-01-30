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

using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

internal sealed class DialogPage : PixiePageBase
{
    private readonly PixieButton _messageButton;
    private readonly PixieButton _exceptionButton;
    private readonly PixieButton _childrenButton;
    private readonly PixieButton _scrollingButton;

    public DialogPage()
    {
        Title = nameof(MessageDialog);
        Symbol = Symbols.Window;

        // BUTTON
        var group = NewGroup(nameof(PixieButton));
        group.IsCollapsable = false;

        var control = NewControl<PixieButton>(group, "Message");
        control.Classes.Add("regular-background");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowSimpleClickHandler;
        control.Footer = "Result: ";
        _messageButton = control;

        control = NewControl<PixieButton>(group, "Exception");
        control.Classes.Add("regular-background");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowExceptionClickHandler;
        control.Footer = "Result: ";
        _exceptionButton = control;

        control = NewControl<PixieButton>(group, "Children");
        control.Classes.Add("regular-background");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowChildrenClickHandler;
        control.Footer = "Result: ";
        _childrenButton = control;

        control = NewControl<PixieButton>(group, "Scrolling Children");
        control.Classes.Add("regular-background");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowScrollingChildrenClickHandler;
        control.Footer = "Result: ";
        _scrollingButton = control;

        control = NewControl<PixieButton>(group, "No Buttons");
        control.Classes.Add("regular-background");
        control.RightButton.Content = "Show";
        control.RightButton.IsVisible = true;
        control.RightButton.Click += ShowNoButtonsClickHandler;
    }

    private async void ShowSimpleClickHandler(object? sender, EventArgs __)
    {
        var rslt = await MessageDialog.ShowDialog(sender, "Pone exempli gratia bona", DialogButtons.Ok | DialogButtons.Cancel);
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
            var rslt = await MessageDialog.ShowDialog(sender, ex);
            _exceptionButton.Footer = "Result: " + rslt;
        }
    }

    private async void ShowChildrenClickHandler(object? sender, EventArgs __)
    {
        var window = new MessageDialog();
        window.Title = ChromeApplication.Current.Name;
        window.Message = $"{nameof(PixieControl)} Children";
        window.Buttons = DialogButtons.Ok | DialogButtons.Cancel;

        window.Children.Add(NewControl<PixieEditor>(null));
        window.Children.Add(NewControl<PixieSwitch>(null));
        window.Children.Add(NewControl<PixieCheckBox>(null));
        var rslt = await window.ShowDialog<DialogButtons>(MessageDialog.GetWindow(sender));
        _childrenButton.Footer = "Result: " + rslt;
    }

    private async void ShowScrollingChildrenClickHandler(object? sender, EventArgs __)
    {
        var window = new MessageDialog();
        window.Title = ChromeApplication.Current.Name;
        window.Message = $"{nameof(PixieControl)} Children";
        window.Buttons = DialogButtons.Ok | DialogButtons.Cancel;

        window.Children.Add(NewControl<PixieAccentPicker>(null));

        for (int n = 0; n < 6; ++n)
        {
            window.Children.Add(NewControl<PixieEditor>(null));
            window.Children.Add(NewControl<PixieSwitch>(null));
            window.Children.Add(NewControl<PixieCheckBox>(null));
        }

        var rslt = await window.ShowDialog<DialogButtons>(MessageDialog.GetWindow(sender));
        _childrenButton.Footer = "Result: " + rslt;
    }

    private async void ShowNoButtonsClickHandler(object? sender, EventArgs __)
    {
        await MessageDialog.ShowDialog(sender, "Pone exempli gratia bona", DialogButtons.None);
    }

}