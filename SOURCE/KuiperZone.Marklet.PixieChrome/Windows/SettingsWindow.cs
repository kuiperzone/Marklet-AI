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

using KuiperZone.Marklet.PixieChrome.CarouselPages;
using KuiperZone.Marklet.PixieChrome.Settings;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Settings window base class which displays <see cref="AppearanceSettings"/> and <see cref="WindowSettings"/> by
/// default.
/// </summary>
/// <remarks>
/// This class adds <see cref="AppearanceCarouselPage"/> and <see cref="WindowCarouselPage"/> to <see
/// cref="CarouselWindow.Pages"/>, and holds the selected page index. Subclass may extend.
/// </remarks>
public class SettingsWindow : CarouselWindow
{
    private static int s_globalIndex;
    private static readonly WindowPersistence s_persistence = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// <see cref="CarouselWindow.Pages"/> are populated, while <see cref="CarouselWindow.PageClasses"/> has defaults.
    /// </remarks>
    public SettingsWindow()
    {
        Title = "Settings";

        // See ChromeStyling.axaml
        PageClasses.Add("chrome-high");

        Width = 1000;
        MinWidth = 700;

        Height = 700;
        MinHeight = 400;

        LeftPanelMinWidth = 230;
        LeftPanelMaxWidth = 350;
        ContentMaxWidth = 540;

        IsSearchVisible = true;
        Pages.Add(new AppearanceCarouselPage());
        Pages.Add(new WindowCarouselPage());
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        // Order important
        base.OnOpened(e);
        PageIndex = s_globalIndex;
        s_persistence.SetWindow(this);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        s_globalIndex = PageIndex;
        s_persistence.CopyFrom(this);
        base.OnClosed(e);
    }

}
