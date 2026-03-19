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

internal sealed class LightButtonPage : PixiePageBase
{
    public LightButtonPage()
    {
        Title = nameof(LightButton);
        Symbol = Symbols.LightbulbCircle;

        // BUTTON
        var group = NewGroup(nameof(PixieCard));
        group.IsCollapsible = false;

        var button = NewControl<PixieControl>(group, "Transparent");
        button.RightButton.IsVisible = true;

        button = NewControl<PixieControl>(group, "CanToggle");
        button.RightButton.IsVisible = true;
        button.RightButton.CanToggle = true;
        button.RightButton.Content = "Toggle";

        button = NewControl<PixieControl>(group, "Class: regular-background");
        button.RightButton.IsVisible = true;
        button.RightButton.Content = "Regular";
        button.RightButton.Classes.Add("regular-background");

        button = NewControl<PixieControl>(group, "Class: accent-background");
        button.RightButton.IsVisible = true;
        button.RightButton.Content = "Accent Background";
        button.RightButton.Classes.Add("accent-background");

        button = NewControl<PixieControl>(group, "Class: accent-checked");
        button.RightButton.IsVisible = true;
        button.RightButton.Content = "Accent Hover";
        button.RightButton.Classes.Add("accent-checked");

        button = NewControl<PixieControl>(group, "Class: accent-checked");
        button.RightButton.IsVisible = true;
        button.RightButton.CanToggle = true;
        button.RightButton.Content = "Accent Toggle";
        button.RightButton.Classes.Add("accent-checked");

        button = NewControl<PixieControl>(group, "Class: dialog-button, regular-background");
        button.RightButton.IsVisible = true;
        button.RightButton.Content = "Dialog Button";
        button.RightButton.Classes.Add("dialog-button");
        button.RightButton.Classes.Add("regular-background");

    }

}