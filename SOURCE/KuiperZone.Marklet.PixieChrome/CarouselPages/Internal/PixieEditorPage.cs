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
        control.ChildControl.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group);
        control.Title += " Left";
        control.ChildControl.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group);
        control.Title += " Right";
        control.ChildControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        control.ChildControl.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group, "Long title Long title Long title Long title");
        control.ChildControl.Text = nameof(PixieEditor);

        control = NewControl<PixieEditor>(group);
        control.Title = null;
        control.ChildControl.Text = nameof(PixieEditor) + "\nMultiline";
        control.ChildControl.AcceptsReturn = true;
        control.ChildControl.Width = double.NaN;
        control.ChildControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        control.ChildControl.Height = 210;


        control = NewControl<PixieEditor>(group, false);
        control.ChildControl.Text = nameof(PixieEditor);
    }
}