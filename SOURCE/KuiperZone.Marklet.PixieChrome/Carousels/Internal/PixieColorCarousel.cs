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

/// <summary>
/// Control carousel for development inspection only.
/// </summary>
internal sealed class PixieColorCarousel : PixieCarousel
{
    public PixieColorCarousel()
    {
        Title = nameof(PixieColorPicker);
        Symbol = Symbols.Palette;

        var group = CreateGroup(nameof(PixieColorPicker));
        var control = CreateControl<PixieColorPicker>(group);
        AddAccoutrements(control);
        control.IsDefaultColorVisible = true;

        control = CreateControl<PixieAccentPicker>(group);
        AddAccoutrements(control);
        control.IsDefaultColorVisible = true;
        control.Title = "Should not be visible";
        control.LeftSymbol = Symbols.Palette;

        control = CreateControl<PixieAccentPicker>(group);
        control.IsDefaultColorVisible = true;
        control.Title = "Should not be visible";
        control.LeftSymbol = Symbols.Palette;

        CreateControl<PixieAccentPicker>(group, false).IsDefaultColorVisible = true;
    }
}