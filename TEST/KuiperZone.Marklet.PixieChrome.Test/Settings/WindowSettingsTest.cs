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

using KuiperZone.Marklet.PixieChrome.Settings;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class WindowSettingsTest
{
    [Fact]
    public void Reset_EqualsDefault()
    {
        var obj = NewObj();
        obj.Reset();

        Assert.Equal(new WindowSettings(), obj);
    }

    [Fact]
    public void CopyFrom_EqualsSource()
    {
        var obj0 = NewObj();
        var obj1 = new WindowSettings();

        obj1.CopyFrom(obj0);
        Assert.Equal(obj0, obj1);
    }

    [Fact]
    public void Serialize_Deserializes()
    {
        var obj0 = NewObj();
        var json = obj0.Serialize();

        var obj1 = new WindowSettings();
        obj1.Deserialize(json!);
        Assert.Equal(obj0, obj1);
    }

    private static WindowSettings NewObj()
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
