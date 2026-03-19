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

using KuiperZone.Marklet.Tooling.Markdown;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.Tooling.Test;

public class StyledContainerTest
{
    private readonly ITestOutputHelper _out;

    public StyledContainerTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void GetWidth()
    {
        var obj = new StyledContainer();
        Assert.Equal(0, obj.GetWidth());

        obj.Elements.Add(new("1234\n\n12"));
        Assert.Equal(4, obj.GetWidth());

        obj.Elements.Add(new("3456789"));
        Assert.Equal(9, obj.GetWidth());
    }

    [Fact]
    public void GetHeight()
    {
        var obj = new StyledContainer();
        Assert.Equal(1, obj.GetHeight());

        obj.Elements.Add(new("1234"));
        Assert.Equal(1, obj.GetHeight());

        obj.Elements.Add(new("1234\n12"));
        Assert.Equal(2, obj.GetHeight());

        obj.Elements.Add(new("1234\n\n12"));
        Assert.Equal(4, obj.GetHeight());
    }

}