// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// AUTHOR    : Andrew Thomas
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

using KuiperZone.Marklet.Tooling.Markdown;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.Tooling.Test;

public class MarkTableTest
{
    private readonly ITestOutputHelper _out;

    public MarkTableTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void ToPaddedBlock()
    {
        const string Expect =   "H00         H01         H02\n" +
                                "───────────────────────────\n" +
                                "Cell10    Cell11     Cell12\n" +
                                "         Next Line         \n" +
                                "───────────────────────────\n" +
                                "C20         C21         C22";

        var obj = NewTable();
        var block = obj.ToPaddedBlock();
        ConditionalDebug.WriteLine(block.ToString(TextFormat.Unicode), false);

        Assert.Equal(Expect, block.ToString(TextFormat.Unicode));
    }

    [Fact]
    public void ToString_Html()
    {
        const string Expect =
            @"<table style=""border-collapse:collapse;"">
<tr style=""border-style:solid; border-width:0 0 2px 0;"">
<th style=""text-align:left;"">H00</th>
<th style=""text-align:center;"">H01</th>
<th style=""text-align:right;"">H02</th>
</tr>
<tr style=""border-style:solid; border-width:0 0 1px 0;"">
<td style=""text-align:left;"">Cell10</td>
<td style=""text-align:center;"">Cell11
Next Line</td>
<td style=""text-align:right;"">Cell12</td>
</tr>
<tr>
<td style=""text-align:left;"">C20</td>
<td style=""text-align:center;"">C21</td>
<td style=""text-align:right;"">C22</td>
</tr>
</table>";

        var obj = NewTable();
        var str = obj.ToString(TextFormat.Html);
        ConditionalDebug.WriteLine(str, false);

        Assert.Equal(Expect, str);
    }

    [Fact]
    public void ToString_Markdown()
    {
        const string Expect = @"|H00|H01|H02|
|---|:---:|---:|
|Cell10|Cell11<br />Next Line|Cell12|
|*C20*|**C21**|`C22`|";

        var obj = NewTable();
        var str = obj.ToString(TextFormat.Markdown);
        ConditionalDebug.WriteLine(str, false);

        Assert.Equal(Expect, str);
    }

    [Fact]
    public void ToString_UnicodeMono()
    {
        const string Expect =
"H00         H01         H02\n" +
"───────────────────────────\n" +
"Cell10    Cell11     Cell12\n" +
"         Next Line         \n" +
"───────────────────────────\n" +
"C20         C21         C22";

        var obj = NewTable();
        var str = obj.ToString(TextFormat.Unicode);
        ConditionalDebug.WriteLine(str, false);

        Assert.Equal(Expect, str);
    }

    private static MarkTable NewTable()
    {
        var obj = new MarkTable(3, 3);
        obj.SetAlign(0, ColAlign.Left);
        obj.SetAlign(1, ColAlign.Center);
        obj.SetAlign(2, ColAlign.Right);

        obj.Cells[0].Elements.Add(new("H00"));
        obj.Cells[1].Elements.Add(new("H01"));
        obj.Cells[2].Elements.Add(new("H02"));

        obj.Cells[3].Elements.Add(new("Cell10"));
        obj.Cells[4].Elements.Add(new("Cell11\nNext Line"));
        obj.Cells[5].Elements.Add(new("Cell12"));

        obj.Cells[6].Elements.Add(new("C20", InlineStyling.Emphasis));
        obj.Cells[7].Elements.Add(new("C21", InlineStyling.Strong));
        obj.Cells[8].Elements.Add(new("C22", InlineStyling.Code));

        return obj;
    }

}