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
using KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows.Internal;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class CarouselEntriesTest
{
    [Fact]
    public void Initialize_Populates()
    {
        var obj = NewObj();
        Assert.True(obj.IsInitialized);
        Assert.NotEmpty(obj);
    }

    [Fact]
    public void Find_LocatesEntry()
    {
        var obj = NewObj();
        var findings = new List<PixieFinding>();

        // Expected to exist
        Assert.True(obj.Find(nameof(PixieSlider), findings));
        Assert.NotEmpty(findings);

        Assert.NotNull(obj.GetFindEntry(findings[0]));
    }

    private CarouselEntries NewObj(string? classes = null)
    {
        var obj = new CarouselEntries();

        var list = new List<CarouselPage>();
        list.Add(new PixieComboPage());
        list.Add(new PixieControlPage());
        list.Add(new PixieSliderPage());
        list.Add(new PixieTogglePage());

        if (classes != null)
        {
            obj.Initialize(list, [classes]);
            return obj;
        }

        obj.Initialize(list, null);
        return obj;
    }
}
