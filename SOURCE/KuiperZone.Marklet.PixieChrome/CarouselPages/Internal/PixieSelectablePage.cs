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

internal sealed class PixieSelectablePage : PixiePageBase
{
    public PixieSelectablePage()
    {
        Title = nameof(PixieSelectable);
        Symbol = Symbols.TextFields;

        var group = NewGroup(nameof(PixieSelectable));

        NewControl<PixieSelectable>(group);

        var control = NewControl<PixieSelectable>(group);
        control.Title = "Multiline\nTitle\nMultiline\nTitle";
        AddAccoutrements(control);

        control = NewControl<PixieSelectable>(group);
        AddAccoutrements(control);
        control.LeftButton.Content = "Left";

        control = NewControl<PixieSelectable>(group, false);
        AddAccoutrements(control);
        control.RightButton.Content = "Right";
    }
}