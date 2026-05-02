// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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

using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Test;

public class ZuidTest
{
    [Fact]
    public void DefaultStruct_Ok()
    {
        // For info
        Diag.WriteLine(Zuid.SequenceBits.ToString());

        Zuid obj = default;
        Assert.True(obj.IsEmpty);
        Assert.Equal(Zuid.MinTime, obj.Timestamp);
        Assert.Equal("0000000000000000", obj.ToString());
        Assert.Equal(default, obj);

        // Prevent inadvertent change of
        // characteristics which will break databases in wild
        Assert.Equal(1970, Zuid.MinTime.Year);
        Assert.Equal(2527, Zuid.MaxTime.Year);
    }

    [Fact]
    public void New_MinTime()
    {
        var obj = Zuid.New(Zuid.MinTime);
        Assert.Equal("1970-01-01 00:00:00.000Z", obj.ToString(true));

        Assert.False(obj.IsEmpty);
        Assert.True(obj.Value > 0);
        Assert.Equal(Zuid.MinTime, obj.Timestamp);
        Assert.NotEqual(default, obj);
    }

    [Fact]
    public void New_MaxTime()
    {
        var obj = Zuid.New(Zuid.MaxTime);
        Diag.WriteLine(obj.ToString(true));

        // Ensure a viable maximum time
        Assert.True(obj.Timestamp > new DateTime(2300, 01, 21));

        Assert.False(obj.IsEmpty);
        Assert.Equal(Zuid.MaxTime, obj.Timestamp);
        Assert.NotEqual(default, obj);
    }

    [Fact]
    public void New_Ok()
    {
        var obj = Zuid.New();
        Diag.WriteLine(obj.ToString());

        Assert.False(obj.IsEmpty);
        Assert.True(obj.Value > 0);
        Assert.True(obj.Timestamp > Zuid.MinTime);
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
