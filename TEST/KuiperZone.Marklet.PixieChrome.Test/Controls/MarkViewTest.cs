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

using System.Text;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Tooling.Markdown;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class MarkViewTest : ControlTestBase
{
    private readonly ITestOutputHelper _out;

    public MarkViewTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void ContextMenu_InitialNull()
    {
        var obj = new MarkView();

        // Has no initial context
        Assert.NotNull(obj.ContextMenu);
    }

    [Fact]
    public void Content_CorrectChildCount()
    {
        var obj = new MarkView();
        obj.Content = GetMessage("value0");

        var exp = "H1,Para,FencedCode,H3,IndentedCode,TableCode,Para,Para,Para,Rule,Para,Para,Rule,Para,Para,Para";
        var act = ToString(obj.GetBlockKinds());
        _out.WriteLine(act);

        Assert.Equal(exp, act);

        // Force change
        obj.Content = GetMessage("value1");

        act = ToString(obj.GetBlockKinds());
        _out.WriteLine(act);

        Assert.Equal(exp, act);
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
