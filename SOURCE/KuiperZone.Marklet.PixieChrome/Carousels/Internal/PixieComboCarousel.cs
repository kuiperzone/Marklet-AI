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

using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Carousels.Internal;

internal sealed class PixieComboCarousel : PixieCarousel
{
    public PixieComboCarousel()
    {
        Title = nameof(PixieCombo);
        Symbol = Symbols.ArrowCircleDown;

        var group = CreateGroup(nameof(PixieCombo));

        var control = CreateControl<PixieCombo>(group);
        AddAccoutrements(control);
        control.SetItemsAs<CornerSize>();
        control.SelectedIndex = 0;
        control.Footer = "DEFAULT ALIGNED:"; // <- needs colon
        control.ValueChanged += ValueChangedHandler;



        // Has text handlers
        control = CreateControl<PixieCombo>(group);
        control.SubjectAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        control.SetItemsAs<CornerSize>();
        Diag.ThrowIfNotNull(control.Text);

        control.SelectedIndex = 0;
        control.Footer = "LEFT ALIGNED:"; // <- needs colon
        control.ValueChanged += ValueChangedHandler;
        control.EditSubmitted += EditSubmittedHandler;
        control.RightButton.IsVisible = true;
        control.RightButton.Content = "Try Text";
        control.RightButton.Tag = control;
        control.RightButton.Click += TrySetTextHandler; // <- should do nothing



        control = CreateControl<PixieCombo>(group);
        control.IsEditable = true;
        control.SetItemsAs<CornerSize>();
        control.SelectedIndex = 0;
        control.Footer = "EDITABLE:"; // <- needs colon
        control.ValueChanged += ValueChangedHandler;
        control.EditSubmitted += EditSubmittedHandler;



        control = CreateControl<PixieCombo>(group);
        control.SubjectAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        control.IsEditable = true;
        control.SetItemsAs<CornerSize>();
        control.SelectedIndex = 0;
        control.Footer = "LEFT ALIGNED EDITABLE: "; // <- needs colon
        control.ValueChanged += ValueChangedHandler;
        control.EditSubmitted += EditSubmittedHandler;

        control.RightButton.IsVisible = true;
        control.RightButton.Content = "Try Text";
        control.RightButton.Tag = control;
        control.RightButton.Click += TrySetTextHandler; // <- should set



        control = CreateControl<PixieCombo>(group, false, "Disabled: Long title Long title Long title Long title");
        control.SetItemsAs<CornerSize>();
        control.IsEditable = true;
        control.SelectedIndex = 0;
        control.ValueChanged += ValueChangedHandler;
        control.EditSubmitted += EditSubmittedHandler;
    }

    private void ValueChangedHandler(object? sender, EventArgs __)
    {
        if (sender is PixieCombo combo && combo.Footer != null)
        {
            var s = combo.Footer;
            int p = s.IndexOf(':');

            if (p > -1)
            {
                combo.Footer = s.Substring(0, p) + ": Index: " + combo.SelectedIndex + ", Text: " + combo.Text;
            }
        }
    }

    private void EditSubmittedHandler(object? sender, RoutedEventArgs __)
    {
        if (sender is PixieCombo combo && combo.Footer != null)
        {
            var s = combo.Footer;
            int p = s.IndexOf(':');

            if (p > -1)
            {
                combo.Footer = s.Substring(0, p) + ": SUBMITTED: " + combo.Text;
            }
        }
    }

    private void TrySetTextHandler(object? sender, EventArgs __)
    {
        if (sender is LightButton btn && btn.Tag is PixieCombo combo)
        {
            // Try set text (does nothing if not editable)
            combo.Text = "Hello";
        }
    }
}