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

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

/// <summary>
/// Control carousel for development inspection only.
/// </summary>
internal sealed class PixieEditorPage : PixiePageBase
{
    public PixieEditorPage()
    {
        Title = nameof(PixieEditor);
        Symbol = Symbols.EditSquare;

        // EDITOR
        var group = NewGroup(nameof(PixieEditor));

        var control = NewControl<PixieEditor>(group, "");
        AddAccoutrements(control);
        control.LeftSymbol = Symbols.Archive;
        control.Subject.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group);
        control.Title += " Left";
        control.Subject.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group);
        control.Title += " Right";
        control.Subject.HasCopyButton = true;
        control.Subject.HasRevealButton = true;
        control.Subject.HasMatchCaseButton = true;
        control.Subject.HasMatchWordButton = true;
        control.Subject.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        control.Subject.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group, "Long title Long title Long title Long title");
        control.Subject.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group);
        control.Title = null;
        control.Subject.Text = nameof(PixieEditor) + "\nMultiline";
        control.Subject.AcceptsReturn = true;
        control.Subject.Width = double.NaN;
        control.Subject.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        control.Subject.Height = 210;


        control = NewControl<PixieEditor>(group, false);
        control.Subject.Text = nameof(PixieEditor);
    }
}