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

using System.Text;
using KuiperZone.Marklet.Tooling.Markdown;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.Tooling.Test;

public partial class MarkDocumentTest : BaseTest
{
    private readonly ITestOutputHelper _out;

    public MarkDocumentTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void Update_HtmlBlocks_TreatAsIndented()
    {
        var text = @"
<blockquote>
    <p>Text</p>
</blockquote>

Normal text";

        var obj = UpdateWriteOut(text);
        // <blockquote>\n    <p>Text</p>\n</blockquote>
        Assert.Equal(BlockKind.IndentedCode, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("<blockquote>\n    <p>Text</p>\n</blockquote>", obj[0].Elements[0].Text);
        //
        // Normal text
        Assert.Equal(BlockKind.Para, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Normal text", obj[1].Elements[0].Text);
        //
        Assert.Equal(2, obj.Count);
    }

    [Fact]
    public void Update_Tab_Preserves()
    {
        var text = @"
* Unordered 1 level 1
    * Unordered 2 level 2
        * Unordered 3 level 3
    * Unordered 4 level 2

  ```lang with indent
    **CodeFence**
        FenceLine 2
```
    Pre1
        Pre1
";
        text = text.Replace("    ", "\t");

        var obj = UpdateWriteOut(text);
        // * Unordered 1 level 1
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(1, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('*', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Unordered 1 level 1", obj[0].Elements[0].Text);
        //
        // * Unordered 2 level 2
        Assert.Equal(BlockKind.Para, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(2, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('*', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Unordered 2 level 2", obj[1].Elements[0].Text);
        //
        // * Unordered 3 level 3
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(3, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('*', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Unordered 3 level 3", obj[2].Elements[0].Text);
        //
        // * Unordered 4 level 2
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(0, obj[3].QuoteLevel);
        Assert.Equal(2, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('*', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Unordered 4 level 2", obj[3].Elements[0].Text);
        //
        // ``````lang\n  **CodeFence**\n  \tFenceLine 2\n``````
        Assert.Equal(BlockKind.FencedCode, obj[4].Kind);
        Assert.Equal(0, obj[4].QuoteLevel);
        Assert.Equal(1, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('\0', obj[4].ListBullet);
        Assert.Equal("lang", obj[4].Lang);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("  **CodeFence**\n  \tFenceLine 2", obj[4].Elements[0].Text);
        //
        // ``````\n\tPre1\n\t\tPre1\n``````
        Assert.Equal(BlockKind.FencedCode, obj[5].Kind);
        Assert.Equal(0, obj[5].QuoteLevel);
        Assert.Equal(0, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("\tPre1\n\t\tPre1", obj[5].Elements[0].Text);
        //
        Assert.Equal(6, obj.Count);

    }


    [Fact]
    public void Update_FencedCodeInList()
    {
        var text = @"
- a
  ```
  c
  d
  ```
- e";

        var obj = UpdateWriteOut(text);
        // - a
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(1, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('-', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("a", obj[0].Elements[0].Text);
        //
        // ``````\nc\nd\n``````
        Assert.Equal(BlockKind.FencedCode, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(1, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("c\nd", obj[1].Elements[0].Text);
        //
        // - e
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(1, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('-', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("e", obj[2].Elements[0].Text);
        //
        Assert.Equal(3, obj.Count);

    }

    [Fact]
    public void Update_MaintainsSync()
    {
        var obj = new MarkDocument();
        var sb = new StringBuilder();
        sb.Append("Hello ");

        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // Hello
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Hello", obj[0].Elements[0].Text);
        Assert.Single(obj);

        sb.Append("*world*\n\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // Hello *world*
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Hello ", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Emphasis, obj[0].Elements[1].Styling);
        Assert.Equal("world", obj[0].Elements[1].Text);
        Assert.Single(obj);

        sb.Append("Para2\n\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // Hello *world*
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Hello ", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Emphasis, obj[0].Elements[1].Styling);
        Assert.Equal("world", obj[0].Elements[1].Text);
        //
        // Para2
        Assert.Equal(BlockKind.Para, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Para2", obj[1].Elements[0].Text);
        //
        Assert.Equal(2, obj.Count);

        sb.Append("Para3\n\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // Para2
        Assert.Equal(BlockKind.Para, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Para2", obj[1].Elements[0].Text);
        //
        // Para3
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Para3", obj[2].Elements[0].Text);
        //
        Assert.Equal(3, obj.Count);

        sb.Append("> Quote\n\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // Para3
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Para3", obj[2].Elements[0].Text);
        //
        // > Quote
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(1, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Quote", obj[3].Elements[0].Text);
        //
        Assert.Equal(4, obj.Count);

        sb.Append("> * List\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // > Quote
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(1, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Quote", obj[3].Elements[0].Text);
        //
        // > * List
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(1, obj[4].QuoteLevel);
        Assert.Equal(1, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('*', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("List", obj[4].Elements[0].Text);
        //
        Assert.Equal(5, obj.Count);

        sb.Append("> \n>   Item\n\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // > Quote
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(1, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Quote", obj[3].Elements[0].Text);
        //
        // > * List
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(1, obj[4].QuoteLevel);
        Assert.Equal(1, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('*', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("List", obj[4].Elements[0].Text);
        //
        // > Item
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(1, obj[5].QuoteLevel);
        Assert.Equal(1, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("Item", obj[5].Elements[0].Text);
        //
        Assert.Equal(6, obj.Count);


        sb.Append("```\nCode\n```\n\n");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // > Item
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(1, obj[5].QuoteLevel);
        Assert.Equal(1, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("Item", obj[5].Elements[0].Text);
        //
        // ```\nCode\n```
        Assert.Equal(BlockKind.FencedCode, obj[6].Kind);
        Assert.Equal(0, obj[6].QuoteLevel);
        Assert.Equal(0, obj[6].ListLevel);
        Assert.Equal(0, obj[6].ListOrder);
        Assert.Equal('\0', obj[6].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[6].Elements[0].Styling);
        Assert.Equal("Code", obj[6].Elements[0].Text);

        sb.Append("Final");
        obj.Update(sb.ToString());
        WriteTestCode(obj);
        //
        // Hello *world*
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Hello ", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Emphasis, obj[0].Elements[1].Styling);
        Assert.Equal("world", obj[0].Elements[1].Text);
        //
        // Para2
        Assert.Equal(BlockKind.Para, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Para2", obj[1].Elements[0].Text);
        //
        // Para3
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Para3", obj[2].Elements[0].Text);
        //
        // > Quote
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(1, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Quote", obj[3].Elements[0].Text);
        //
        // > * List
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(1, obj[4].QuoteLevel);
        Assert.Equal(1, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('*', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("List", obj[4].Elements[0].Text);
        //
        // > Item
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(1, obj[5].QuoteLevel);
        Assert.Equal(1, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("Item", obj[5].Elements[0].Text);
        //
        // ```\nCode\n```
        Assert.Equal(BlockKind.FencedCode, obj[6].Kind);
        Assert.Equal(0, obj[6].QuoteLevel);
        Assert.Equal(0, obj[6].ListLevel);
        Assert.Equal(0, obj[6].ListOrder);
        Assert.Equal('\0', obj[6].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[6].Elements[0].Styling);
        Assert.Equal("Code", obj[6].Elements[0].Text);
        //
        // Final
        Assert.Equal(BlockKind.Para, obj[7].Kind);
        Assert.Equal(0, obj[7].QuoteLevel);
        Assert.Equal(0, obj[7].ListLevel);
        Assert.Equal(0, obj[7].ListOrder);
        Assert.Equal('\0', obj[7].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[0].Styling);
        Assert.Equal("Final", obj[7].Elements[0].Text);
        //
        Assert.Equal(8, obj.Count);

        obj.Update("Other");
        WriteTestCode(obj);
        //
        // Other
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Other", obj[0].Elements[0].Text);
        //
        Assert.Single(obj);

    }

    [Fact]
    public void Coalesce_Empty()
    {
        var obj = new MarkDocument();
        var clone = obj.Coalesce();
        WriteTestCode(clone);

        Assert.Empty(obj);
    }

    [Fact]
    public void Coalesce_CombinesParagraphs()
    {
        var content = @"
Para 0
# Head 1
## Head 2

Para 1

Para 2

> Quote 0
>
> Quote 1
>
> > Quote 3
>
> Quote 4

Para 4

Para 5

Para 6

* List 1

  Cont

  Cont

Para 6

|Table|
|--
|Data";

        var obj = new MarkDocument(content).Coalesce();
        WriteTestCode(obj);

        // Para 0
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Para 0", obj[0].Elements[0].Text);
        //
        // # Head 1
        Assert.Equal(BlockKind.H1, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Head 1", obj[1].Elements[0].Text);
        //
        // ## Head 2
        Assert.Equal(BlockKind.H2, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Head 2", obj[2].Elements[0].Text);
        //
        // Para 1\\\n\\\nPara 2
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(0, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Para 1", obj[3].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[1].Styling);
        Assert.Equal("\n\n", obj[3].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[2].Styling);
        Assert.Equal("Para 2", obj[3].Elements[2].Text);
        //
        // > Quote 0\\\n>\\\n>Quote 1
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(1, obj[4].QuoteLevel);
        Assert.Equal(0, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('\0', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("Quote 0", obj[4].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[1].Styling);
        Assert.Equal("\n\n", obj[4].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[2].Styling);
        Assert.Equal("Quote 1", obj[4].Elements[2].Text);
        //
        // > > Quote 3
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(2, obj[5].QuoteLevel);
        Assert.Equal(0, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("Quote 3", obj[5].Elements[0].Text);
        //
        // > Quote 4
        Assert.Equal(BlockKind.Para, obj[6].Kind);
        Assert.Equal(1, obj[6].QuoteLevel);
        Assert.Equal(0, obj[6].ListLevel);
        Assert.Equal(0, obj[6].ListOrder);
        Assert.Equal('\0', obj[6].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[6].Elements[0].Styling);
        Assert.Equal("Quote 4", obj[6].Elements[0].Text);
        //
        // Para 4\\\n\\\nPara 5\\\n\\\nPara 6
        Assert.Equal(BlockKind.Para, obj[7].Kind);
        Assert.Equal(0, obj[7].QuoteLevel);
        Assert.Equal(0, obj[7].ListLevel);
        Assert.Equal(0, obj[7].ListOrder);
        Assert.Equal('\0', obj[7].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[0].Styling);
        Assert.Equal("Para 4", obj[7].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[1].Styling);
        Assert.Equal("\n\n", obj[7].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[2].Styling);
        Assert.Equal("Para 5", obj[7].Elements[2].Text);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[3].Styling);
        Assert.Equal("\n\n", obj[7].Elements[3].Text);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[4].Styling);
        Assert.Equal("Para 6", obj[7].Elements[4].Text);
        //
        // * List 1
        Assert.Equal(BlockKind.Para, obj[8].Kind);
        Assert.Equal(0, obj[8].QuoteLevel);
        Assert.Equal(1, obj[8].ListLevel);
        Assert.Equal(0, obj[8].ListOrder);
        Assert.Equal('*', obj[8].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[8].Elements[0].Styling);
        Assert.Equal("List 1", obj[8].Elements[0].Text);
        //
        // Cont
        Assert.Equal(BlockKind.Para, obj[9].Kind);
        Assert.Equal(0, obj[9].QuoteLevel);
        Assert.Equal(1, obj[9].ListLevel);
        Assert.Equal(0, obj[9].ListOrder);
        Assert.Equal('\0', obj[9].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[9].Elements[0].Styling);
        Assert.Equal("Cont", obj[9].Elements[0].Text);
        //
        // Cont
        Assert.Equal(BlockKind.Para, obj[10].Kind);
        Assert.Equal(0, obj[10].QuoteLevel);
        Assert.Equal(1, obj[10].ListLevel);
        Assert.Equal(0, obj[10].ListOrder);
        Assert.Equal('\0', obj[10].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[10].Elements[0].Styling);
        Assert.Equal("Cont", obj[10].Elements[0].Text);
        //
        // Para 6
        Assert.Equal(BlockKind.Para, obj[11].Kind);
        Assert.Equal(0, obj[11].QuoteLevel);
        Assert.Equal(0, obj[11].ListLevel);
        Assert.Equal(0, obj[11].ListOrder);
        Assert.Equal('\0', obj[11].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[11].Elements[0].Styling);
        Assert.Equal("Para 6", obj[11].Elements[0].Text);
        //
        // |Table|\n|---|\n|Data|
        Assert.Equal(BlockKind.TableCode, obj[12].Kind);
        Assert.Equal(0, obj[12].QuoteLevel);
        Assert.Equal(0, obj[12].ListLevel);
        Assert.Equal(0, obj[12].ListOrder);
        Assert.Equal('\0', obj[12].ListBullet);
        Assert.Equal(InlineStyling.Strong, obj[12].Elements[0].Styling);
        Assert.Equal("Table", obj[12].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[12].Elements[1].Styling);
        Assert.Equal("\n─────\nData", obj[12].Elements[1].Text);
        //
        Assert.Equal(13, obj.Count);
   }

}