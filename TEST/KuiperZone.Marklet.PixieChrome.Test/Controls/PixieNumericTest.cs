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

using Avalonia.Layout;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class PixieNumericTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        var obj = new PixieNumeric();

        AssertDirect(obj, PixieNumeric.ValueProperty, 0m, 5m);
        AssertDirect(obj, PixieNumeric.MinValueProperty, 0m, 2m);
        AssertDirect(obj, PixieNumeric.MaxValueProperty, 10m, 100m);
        AssertDirect(obj, PixieNumeric.IncrementProperty, 1m, 2m);
        AssertDirect(obj, PixieNumeric.AcceptFractionInputProperty, true, false);
        AssertDirect(obj, PixieNumeric.AlwaysShowBorderProperty, false, true);
        AssertDirect(obj, PixieNumeric.CanEditProperty, true, false);
        AssertDirect(obj, PixieNumeric.UnitsProperty, null, "kg");
    }

    [Fact]
    public void StyledProperties_ChangeAndClear()
    {
        var obj = new PixieNumeric();

        AssertStyled(obj, PixieNumeric.FormatProperty, null, "#");
        AssertStyled(obj, PixieNumeric.ControlWidthProperty, 100.0, 30.0);
        AssertStyled(obj, PixieNumeric.ControlAlignmentProperty, HorizontalAlignment.Right, HorizontalAlignment.Center);
    }

    [Fact]
    public void ValueChanged_Occurs()
    {
        var obj = new PixieNumeric();
        var receiver = new ChangeReceiver();
        obj.ValueChanged += receiver.ChangedHandler;

        obj.Value = 2.1m;
        Assert.Equal(2.1m, obj.Value);
        Assert.Equal(1, receiver.Counter);

        obj.IncrementValue();
        Assert.Equal(2, receiver.Counter);

        obj.DecrementValue();
        Assert.Equal(3, receiver.Counter);
    }


    [Fact]
    public void Value_Clamps()
    {
        var obj = new PixieNumeric();

        obj.MinValue = 2m;
        Assert.Equal(2m, obj.Value);

        obj.MaxValue = 3m;
        obj.Value = 4m;
        Assert.Equal(3m, obj.Value);
    }

    [Fact]
    public void Increment_ThrowIfZeroOrNegative()
    {
        var obj = new PixieNumeric();
        Assert.Throws<ArgumentOutOfRangeException>(() => obj.Increment = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => obj.Increment = -1);
    }

    [Fact]
    public void IncrementValue_PositiveRange_IncrementsAndClampsMax()
    {
        var obj = new PixieNumeric();

        // Set known range
        obj.MinValue = 1m;
        obj.MaxValue = 4.5m;
        obj.Increment = 0.5m;
        obj.Value = 3.2m; // <- intentional

        Assert.True(obj.IncrementValue());
        Assert.Equal(3.5m, obj.Value); // <- initial

        Assert.True(obj.IncrementValue());
        Assert.Equal(4.0m, obj.Value);

        Assert.True(obj.IncrementValue());
        Assert.Equal(4.5m, obj.Value);

        Assert.False(obj.IncrementValue());
        Assert.Equal(4.5m, obj.Value);

        // Round to max
        obj.Value = 4.2m;
        Assert.True(obj.IncrementValue());
        Assert.Equal(4.5m, obj.Value);
    }

    [Fact]
    public void IncrementValue_NegativeRange_IncrementsAndClampsMax()
    {
        var obj = new PixieNumeric();

        // Set known range
        obj.MinValue = -4.5m;
        obj.MaxValue = -1m;
        obj.Increment = 0.5m;
        obj.Value = -3.2m; // <- intentional

        Assert.True(obj.IncrementValue());
        Assert.Equal(-3.0m, obj.Value); // <- initial

        Assert.True(obj.IncrementValue());
        Assert.Equal(-2.5m, obj.Value);

        Assert.True(obj.IncrementValue());
        Assert.Equal(-2.0m, obj.Value);

        Assert.True(obj.IncrementValue());
        Assert.Equal(-1.5m, obj.Value);

        Assert.True(obj.IncrementValue());
        Assert.Equal(-1.0m, obj.Value);

        Assert.False(obj.IncrementValue());
        Assert.Equal(-1.0m, obj.Value);

        // Round to min
        obj.Value = -1.2m;
        Assert.True(obj.IncrementValue());
        Assert.Equal(-1.0m, obj.Value);
    }

    [Fact]
    public void DecrementValue_PositiveRange_DecrementsAndClampsMax()
    {
        var obj = new PixieNumeric();

        // Set known range
        obj.MinValue = 1m;
        obj.MaxValue = 4.5m;
        obj.Increment = 0.5m;
        obj.Value = 3.2m; // <- intentional

        Assert.True(obj.DecrementValue());
        Assert.Equal(3.0m, obj.Value); // <- initial

        Assert.True(obj.DecrementValue());
        Assert.Equal(2.5m, obj.Value);

        Assert.True(obj.DecrementValue());
        Assert.Equal(2.0m, obj.Value);

        Assert.True(obj.DecrementValue());
        Assert.Equal(1.5m, obj.Value);

        Assert.True(obj.DecrementValue());
        Assert.Equal(1.0m, obj.Value);

        Assert.False(obj.DecrementValue());
        Assert.Equal(1.0m, obj.Value);

        // Round to max
        obj.Value = 1.2m;
        Assert.True(obj.DecrementValue());
        Assert.Equal(1.0m, obj.Value);
    }

    [Fact]
    public void DecrementValue_NegativeRange_IncrementsAndClampsMax()
    {
        var obj = new PixieNumeric();

        // Set known range
        obj.MinValue = -3.0m;
        obj.MaxValue = -1m;
        obj.Increment = 0.5m;
        obj.Value = -1.2m; // <- intentional

        Assert.True(obj.DecrementValue());
        Assert.Equal(-1.5m, obj.Value); // <- initial

        Assert.True(obj.DecrementValue());
        Assert.Equal(-2.0m, obj.Value);

        Assert.True(obj.DecrementValue());
        Assert.Equal(-2.5m, obj.Value);

        Assert.True(obj.DecrementValue());
        Assert.Equal(-3.0m, obj.Value);

        Assert.False(obj.DecrementValue());
        Assert.Equal(-3.0m, obj.Value);

        // Round to min
        obj.Value = -2.9m;
        Assert.True(obj.DecrementValue());
        Assert.Equal(-3.0m, obj.Value);
    }

}
