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

using Avalonia.Media;
using KuiperZone.Marklet.CarouselPages;
using KuiperZone.Marklet.Settings;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.Test;

public class ContentCarouselPageTest
{
    private readonly ITestOutputHelper _out;

    public ContentCarouselPageTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void RoundTrip_SettingsEqual()
    {
        // IMPORTANT TEST
        var obj = new ContentSettingsCarouselPage();
        obj.IsChangeWritable = false;

        // Updating control with new values should update Global instance
        var s0 = NewSettings();
        obj.UpdateControls(s0);

        var s1 = new ContentSettings();
        obj.UpdateSettings(s1);

        _out.WriteLine("s0: " + s0.Serialize());
        _out.WriteLine("s1: " + s1.Serialize());
        Assert.Equal(s0, s1);
    }

    private static ContentSettings NewSettings()
    {
        var obj = new ContentSettings();

        obj.BodyFont = Shared.FontCategory.Slab;
        obj.Width = Shared.ContentWidth.Narrow;
        obj.DefaultScale = 127;
        obj.HeadingFont = Shared.FontCategory.Mono;
        obj.HeadingForeground = Colors.AliceBlue.ToUInt32();
        obj.FencedForeground = Colors.Aquamarine.ToUInt32();
        obj.DefaultFencedWrap = true;
        obj.UserBaseColor = Colors.BlanchedAlmond.ToUInt32();

        return obj;
    }
}
