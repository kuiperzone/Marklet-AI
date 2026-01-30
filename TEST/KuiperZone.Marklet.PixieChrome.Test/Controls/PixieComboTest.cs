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

using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class PixieComboTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        var obj = NewObj();
        AssertDirect(obj, PixieCombo.SelectedIndexProperty, -1, 2);
        AssertDirect(obj, PixieCombo.IsEditableProperty, false, true);
        AssertDirect(obj, PixieCombo.MaxEditLengthProperty, 256, 3);

        // Must enable first
        obj = NewObj();
        obj.IsEditable = true;
        AssertDirect(obj, PixieCombo.TextProperty, null, "Text");
    }

    [Fact]
    public void StyledProperties_ChangeAndClear()
    {
        var obj = NewObj();

        // Initial values may be subject to change
        AssertStyled(obj, PixieCombo.MaxDropHeightProperty, 300.0, 30);
        AssertStyled(obj, PixieCombo.MinControlWidthProperty, 100.0, 30);
        AssertStyled(obj, PixieCombo.MaxControlWidthProperty, double.PositiveInfinity, 30);
    }

    [Fact]
    public void SelectedIndex_ValueChangedOccurs()
    {
        var obj = NewObj();
        var receiver = new ChangeReceiver();
        obj.ValueChanged += receiver.ChangedHandler;
        Assert.Equal(-1, obj.SelectedIndex);

        obj.SelectedIndex = 1;
        Assert.Equal(1, obj.SelectedIndex);
        Assert.Equal(1, receiver.Counter);
    }

    [Fact]
    public void Text_NotEditable_NotChanged()
    {
        var obj = NewObj();
        obj.Text = "Hello";
        Assert.Null(obj.Text);
        Assert.Equal(-1, obj.SelectedIndex);

        obj.SelectedIndex = 0;

        obj.Text = "Hello2";
        Assert.Equal(0, obj.SelectedIndex);
    }

    [Fact]
    public void Text_Editable_ValueChangedNotOccurs()
    {
        var obj = NewObj();

        var receiver = new ChangeReceiver();
        obj.ValueChanged += receiver.ChangedHandler;

        obj.IsEditable = true;
        obj.Text = "Hello";
        Assert.Equal("Hello", obj.Text);
        Assert.Equal(-1, obj.SelectedIndex);
        Assert.Equal(0, receiver.Counter);
    }

    [Fact]
    public void Text_Editable_SelectedIndexTracks()
    {
        var obj = NewObj();

        // Expect initial
        Assert.Null(obj.Text);

        obj.IsEditable = true;
        obj.Text = "Hello";
        Assert.Equal(-1, obj.SelectedIndex);

        int exp = (int)WhatText.SelectedOrAll;
        obj.Text = WhatText.SelectedOrAll.ToString();
        Assert.Equal(exp, obj.SelectedIndex);
        Assert.Equal(WhatText.SelectedOrAll.ToString(), obj.Text);

        // Set index out of range
        obj.SelectedIndex = 99;
        Assert.Equal(-1, obj.SelectedIndex);
        Assert.Equal(WhatText.SelectedOrAll.ToString(), obj.Text);
    }

    private static PixieCombo NewObj()
    {
        var obj = new PixieCombo();

        foreach (var item in Enum.GetValues<WhatText>())
        {
            obj.Items.Add(item.ToString());
        }

        return obj;
    }

}
