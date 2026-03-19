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

using KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Displays custom control types for development inspection purposes.
/// </summary>
public sealed class InspectWindow : CarouselWindow
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public InspectWindow()
    {
        // See ChromeStyling.axaml
        PageClasses.Add("shade-background");

        Title = "Controls";

        Pages.Add(new PixieGroupPage());
        Pages.Add(new PixieCardPage());
        Pages.Add(new PixieColorPage());
        Pages.Add(new PixieComboPage());
        Pages.Add(new PixieControlPage());
        Pages.Add(new PixieEditorPage());
        Pages.Add(new PixieNumericPage());
        Pages.Add(new PixieMarkViewPage());
        Pages.Add(new PixieProgressPage());
        Pages.Add(new PixieSelectablePage());
        Pages.Add(new PixieSliderPage());
        Pages.Add(new PixieTogglePage());

        Pages.Add(new ChromeDialogPage() { IsSectionStart = true });
        Pages.Add(new LightButtonPage());
    }
}
