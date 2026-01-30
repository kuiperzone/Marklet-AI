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

using Avalonia.Threading;
using KuiperZone.Marklet.CarouselPages;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet;

/// <summary>
/// Application about window.
/// </summary>
public sealed class AboutWindow : CarouselWindow
{
    private const int HomeIndex = 0;
    private const int LicenseIndex = 1;
    private const int MaximsIndex = 2;
    private const int TechnicalIndex = 3;
    private const int DonateIndex = 4;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutWindow()
    {
        Title = "About";

        Width = 800;
        MinWidth = 600;

        Height = 800;
        MinHeight = 400;

        ContentMaxWidth = 800;

        PageClasses.Add("chrome-low");
        BringContentIntoViewOnFocus = false;

        Pages.Add(new AboutHomeCarouselPage(this));
        Pages.Add(new AboutLicenseCarouselPage());
        Pages.Add(new AboutMaximsCarouselPage());
        Pages.Add(new AboutTechnicalCarouselPage());
        Pages.Add(new AboutDonateCarouselPage());
    }

    internal void OnLink(Uri uri)
    {
        switch (uri.Host)
        {
            case "home":
                PageIndex = HomeIndex;
                return;
            case "license":
                PageIndex = LicenseIndex;
                return;
            case "maxims":
                PageIndex = MaximsIndex;
                return;
            case "technical":
                PageIndex = TechnicalIndex;
                return;
            case "donate":
                PageIndex = DonateIndex;
                return;
            default:
                Dispatcher.UIThread.Post(async () => await Launcher.LaunchUriAsync(uri) );
                return;
        }
    }

    internal void LinkClickHandler(object? _, LinkClickEventArgs e)
    {
        if (e.IsAppLink)
        {
            e.Handled = true;
            OnLink(e.Uri);
        }
    }
}
