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
using Xunit.Abstractions;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class WindowCarouselPageTest
{
    private readonly ITestOutputHelper _out;

    public WindowCarouselPageTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void RoundTrip_SettingsEqual()
    {
        var obj = new WindowCarouselPage();
        obj.IsChangeWritable = false;

        // Updating control with new values should update Global instance
        var s0 = NewSettings();
        obj.UpdateControls(s0);

        var s1 = new WindowSettings();
        obj.UpdateSettings(s1);

        _out.WriteLine("s0: " + s0.Serialize());
        _out.WriteLine("s1: " + s1.Serialize());
        Assert.Equal(s0, s1);
    }

    private static WindowSettings NewSettings()
    {
        var obj = new WindowSettings();

        // Change from default state
        obj.IsChromeWindow = false;
        obj.DialogFollows = true;
        obj.ControlStyle = ChromeControlStyle.DiamondArrows;
        obj.ControlBackground = ChromeControlBackground.Square;

        return obj;
    }
}
