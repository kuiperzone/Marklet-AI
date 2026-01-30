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

internal sealed class PixieTogglePage : PixiePageBase
{
    private readonly PixieCheckBox _disabledCheck;
    private readonly PixieSwitch _disabledSwitch;

    public PixieTogglePage()
    {
        Title = "Toggles";
        Symbol = Symbols.ToggleOn;

        // SWITCH
        var group = NewGroup(nameof(PixieSwitch));

        var control = NewControl<PixieSwitch>(group);
        AddAccoutrements(control);
        control.IsChecked = true;
        control.ValueChanged += (o, _) => _disabledSwitch!.IsChecked = ((PixieSwitch)o!).IsChecked;

        _disabledSwitch = NewControl<PixieSwitch>(group, false);
        _disabledSwitch.IsChecked = true;

        // CHECKBOX
        group = NewGroup(nameof(PixieCheckBox));

        var check = NewControl<PixieCheckBox>(group);
        check.IsChecked = true;
        check.ValueChanged += (o, _) => _disabledCheck!.IsChecked = ((PixieCheckBox)o!).IsChecked;

        _disabledCheck = NewControl<PixieCheckBox>(group, false);
        _disabledCheck.IsChecked = true;


        // RADIO
        group = NewGroup(nameof(PixieRadio));

        NewControl<PixieRadio>(group);
        NewControl<PixieRadio>(group);
        NewControl<PixieRadio>(group, false).IsChecked = true;
    }
}