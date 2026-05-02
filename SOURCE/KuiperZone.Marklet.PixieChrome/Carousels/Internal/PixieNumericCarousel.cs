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

internal sealed class PixieNumericCarousel : PixieCarousel
{
    public PixieNumericCarousel()
    {
        Title = nameof(PixieNumeric);
        Symbol = Symbols.ArrowCircleDown;

        var group = CreateGroup(nameof(PixieNumeric));

        var control = CreateControl<PixieNumeric>(group);
        AddAccoutrements(control);
        control.Units = "Kg";
        control.MinValue = -10;
        control.MaxValue = 10;
        control.Default = 0;
        control.Increment = 0.5m;
        control.CanEdit = true;
        control.Footer = "CanEdit = true, Value: {VALUE} [{MIN}, {MAX}]";

        control = CreateControl<PixieNumeric>(group);
        AddAccoutrements(control);
        control.Units = "Kg";
        control.MinValue = -10;
        control.MaxValue = 10;
        control.Increment = 0.5m;
        control.CanEdit = false;
        control.Footer = "CanEdit = false, Value: {VALUE} [{MIN}, {MAX}]";

        control = CreateControl<PixieNumeric>(group);
        AddAccoutrements(control);
        control.Units = "Kg";
        control.MinValue = -10;
        control.MaxValue = 10;
        control.Increment = 0.5m;
        control.CanEdit = true;
        control.AcceptFractionInput = false;
        control.Footer = "CanEdit = true, AcceptFractionInput = false, but increments by 0.5";

        control = CreateControl<PixieNumeric>(group);
        AddAccoutrements(control);
        control.MinValue = -10;
        control.MaxValue = 10;
        control.CanEdit = true;
        control.AlwaysShowBorder = true;
        control.ControlAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        control.Footer = "LEFT ALIGNED, CanEdit = true, AlwaysShowBorder = true";

        CreateControl<PixieNumeric>(group, false);
    }
}