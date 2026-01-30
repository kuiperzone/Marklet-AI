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

using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Test;

public class ZuidTest
{
    [Fact]
    public void DefaultStruct_Ok()
    {
        // For info
        ConditionalDebug.WriteLine(Zuid.SequenceBits.ToString());

        Zuid obj = default;
        Assert.True(obj.IsEmpty);
        Assert.Equal(Zuid.ZeroTime, obj.Timestamp);
        Assert.Equal("0000000000000000", obj.ToString());
        Assert.Equal(default, obj);
    }

    [Fact]
    public void New_MinTime()
    {
        var obj = Zuid.New(Zuid.ZeroTime);
        Assert.Equal("1970-01-01 00:00:00.000Z", obj.ToString(true));

        Assert.False(obj.IsEmpty);
        Assert.True(obj.Value > 0);
        Assert.Equal(Zuid.ZeroTime, obj.Timestamp);
        Assert.NotEqual(default, obj);
    }

    [Fact]
    public void New_MaxTime()
    {
        var obj = Zuid.New(Zuid.MaxTime);
        ConditionalDebug.WriteLine(obj.ToString(true));

        // Ensure a minimum value
        Assert.True(obj.Timestamp > new DateTime(2300, 01, 21));

        Assert.False(obj.IsEmpty);
        Assert.Equal(Zuid.MaxTime, obj.Timestamp);
        Assert.NotEqual(default, obj);
    }

    [Fact]
    public void New_Ok()
    {
        var obj = Zuid.New();
        ConditionalDebug.WriteLine(obj.ToString());

        Assert.False(obj.IsEmpty);
        Assert.True(obj.Value > 0);
        Assert.True(obj.Timestamp > Zuid.ZeroTime);
        Assert.True(obj.Timestamp < Zuid.MaxTime);
        Assert.NotEqual(default, obj);

        var now = DateTime.UtcNow;
        var hash = new HashSet<Zuid>();

        for (int n = 0; n < 1024; ++n)
        {
            Assert.True(hash.Add(Zuid.New(now)));
        }

        Assert.Equal(1024, hash.Count);
    }
}
