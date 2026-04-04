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

using System.Text;
using Avalonia.Collections;
using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling.Markdown;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class MarkEngineTest : ControlTestBase
{
    private readonly ITestOutputHelper _out;

    public MarkEngineTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void Rebuild_CorrectChildCount()
    {
        const MarkOptions Opts = MarkOptions.Markdown | MarkOptions.Sanitize;
        var obj = new MarkEngine(new MarkControl(), new AvaloniaList<Control>());

        obj.Rebuild(new MarkDocument(GetMessage("value0"), Opts));

        var exp = "H1,Para,Para,FencedCode,H3,IndentedCode,TableCode,Para,Para,Para,Rule,Para,Para,Rule,Para,Para,Para";
        var act = ToString(obj.GetBlockKinds());
        _out.WriteLine(act);
        Assert.Equal(exp, act);

        // Tweak but no structural change
        // Worth repeating as caches visual controls
        obj.Rebuild(new MarkDocument(GetMessage("value1"), Opts));

        act = ToString(obj.GetBlockKinds());
        _out.WriteLine(act);
        Assert.Equal(exp, act);


        // Simple
        exp = "Para";
        obj.Rebuild(new MarkDocument("Message", Opts));
        act = ToString(obj.GetBlockKinds());
        _out.WriteLine(act);
        Assert.Equal("Para", act);


        // COALESCED
        exp = "H1,Para,FencedCode,H3,IndentedCode,TableCode,Para,Para,Para,Rule,Para,Para,Rule,Para,Para,Para";
        obj.Rebuild(new MarkDocument(GetMessage("value1"), Opts | MarkOptions.Coalesce));

    }

    private static string ToString(BlockKind[] kinds)
    {
        var sb = new StringBuilder();

        foreach (var item in kinds)
        {
            if (sb.Length != 0)
            {
                sb.Append(',');
            }

            sb.Append(item);
        }

        return sb.ToString();
    }

    private static string GetMessage(string value)
    {
        return @$"# H1

Para combined with below.

{value}

```csharp
{value}

```
### H3 {value}

    // Indented {value}


| ID | Status   |
|----|----------|
| 3  | {value}  |

Para {value}

1. List
2. List {value}

    3. Sub itemA
    4. Sub itemB

***

* List item1
* List item2
    * Sub itemA

***

> This is quote {value}
>> This is a reply

Para link: [link text](http://example.com) which points to http://example.com.
";
    }
}
