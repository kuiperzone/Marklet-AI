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

internal sealed class PixieSliderPage : PixiePageBase
{
    public PixieSliderPage()
    {
        Title = nameof(PixieSlider);
        Symbol = Symbols.Sliders;

        var group = NewGroup(nameof(PixieSlider));

        var control = NewControl<PixieSlider>(group);
        AddAccoutrements(control);
        control.Subject.Value = 5.0;
        control.Footer = "Slider value: {VALUE} [{MIN}, {MAX}]";

        control = NewControl<PixieSlider>(group, "");
        control.Footer = "Slider value: {VALUE} [{MIN}, {MAX}]";

        control = NewControl<PixieSlider>(group, false);
        control.Subject.Value = 5.0;
    }
}