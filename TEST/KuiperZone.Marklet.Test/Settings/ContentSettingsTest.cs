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
using KuiperZone.Marklet.Settings;

namespace KuiperZone.Marklet.Test;

public class ContentSettingsTest
{
    [Fact]
    public void Reset_EqualsDefault()
    {
        var obj = NewObj();
        obj.Reset();

        Assert.Equal(new ContentSettings(), obj);
    }

    [Fact]
    public void CopyFrom_EqualsSource()
    {
        var obj0 = NewObj();
        var obj1 = new ContentSettings();

        obj1.CopyFrom(obj0);
        Assert.Equal(obj0, obj1);
    }

    [Fact]
    public void Serialize_Deserializes()
    {
        var obj0 = NewObj();
        var json = obj0.Serialize();

        var obj1 = new ContentSettings();
        obj1.Deserialize(json!);
        Assert.Equal(obj0, obj1);
    }

    private static ContentSettings NewObj()
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
