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

using Xunit.Abstractions;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class ChromeFontsTest
{
    private readonly ITestOutputHelper _out;

    public ChromeFontsTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void GetRun_Succeeds()
    {
        var obj = ChromeFonts.GetRun("A");
        Assert.NotNull(obj);
        Assert.Single(obj);
        Assert.Equal(ChromeFonts.DefaultFontSize, obj[0].FontSize);

        obj = ChromeFonts.GetRun(Symbols.ActionKey);
        Assert.NotNull(obj);
        Assert.Single(obj);
        Assert.Equal(ChromeFonts.SymbolFontSize, obj[0].FontSize);


        obj = ChromeFonts.GetRun("A" + Symbols.ActionKey + "B");
        Assert.NotNull(obj);
        Assert.Equal(3, obj.Count);
        Assert.Equal(ChromeFonts.DefaultFontSize, obj[0].FontSize);
        Assert.Equal(ChromeFonts.SymbolFontSize, obj[1].FontSize);
        Assert.Equal(ChromeFonts.DefaultFontSize, obj[2].FontSize);
    }


}
