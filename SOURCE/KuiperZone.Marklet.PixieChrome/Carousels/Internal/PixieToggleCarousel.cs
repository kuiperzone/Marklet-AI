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

namespace KuiperZone.Marklet.PixieChrome.Carousels.Internal;

internal sealed class PixieToggleCarousel : PixieCarousel
{
    private readonly PixieCheckBox _disabledCheck;
    private readonly PixieSwitch _disabledSwitch;

    public PixieToggleCarousel()
    {
        Title = "Toggles";
        Symbol = Symbols.ToggleOn;

        // SWITCH
        var group = CreateGroup(nameof(PixieSwitch));

        var control = CreateControl<PixieSwitch>(group);
        AddAccoutrements(control);
        control.IsChecked = true;
        control.ValueChanged += (o, _) => _disabledSwitch!.IsChecked = ((PixieSwitch)o!).IsChecked;

        _disabledSwitch = CreateControl<PixieSwitch>(group, false);
        _disabledSwitch.IsChecked = true;

        // CHECKBOX
        group = CreateGroup(nameof(PixieCheckBox));

        var check = CreateControl<PixieCheckBox>(group);
        check.IsChecked = true;
        check.ValueChanged += (o, _) => _disabledCheck!.IsChecked = ((PixieCheckBox)o!).IsChecked;

        _disabledCheck = CreateControl<PixieCheckBox>(group, false);
        _disabledCheck.IsChecked = true;


        // RADIO
        group = CreateGroup(nameof(PixieRadio));

        CreateControl<PixieRadio>(group);
        CreateControl<PixieRadio>(group);
        CreateControl<PixieRadio>(group, false).IsChecked = true;
    }
}