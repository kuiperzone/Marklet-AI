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

namespace KuiperZone.Marklet.Tooling.Test;

public partial class MarkDocumentTest : BaseTest
{
    private const string QuotedSource = @"
> > # Heading `Code` 1 QL2\
> ## Heading 2 **Bold** QL1 ##\
> Body0 QL1
> >Body1 QL2
> >>Body2 QL3
> Body3 stays QL3
> >>> Body4 QL4
> >* _Unordered 1_ QL2 level 1 no space sep
> >  Continued 1
> > * Unordered 2 level 1
> > 1. **Ordered 1** QL2 level 1
> >    Continued 1
> > > Not continued QL3
> > 2. Ordered 2 QL2 level 1
> > Continued 2a
> >
> >    Continued 2b
> >
> >    Continued 2c
> >
> >    3. Ordered 3 QL2 level 2
> >       Continued 3a
> >
> >       Continued 3b
> >
> >
>>  Body 5
> >
> >   ``` LANG
> > **CodeFence 1** QL2
> > FenceLine 2 QL2
> >
> >     FenceLine 3
> >   FenceLine 4
> >         FenceLine 5
> >
> >   ```
> >     **Pre 1** QL2
> >
> >         Pre 2 QL2
> >
> > Table:
> >
> > |A01|A02|
> > |-|-|
> > |a10|a11|
> >
Final line QL0
";

    private const string ExpectQuotedHtml = @"<blockquote>
<blockquote>
<h1>Heading <code>Code</code> 1 QL2\</h1>
</blockquote>
<h2>Heading 2 <strong>Bold</strong> QL1 ##\</h2>
<p>Body0 QL1</p>
<blockquote>
<p>Body1 QL2</p>
<blockquote>
<p>Body2 QL3 Body3 stays QL3</p>
<blockquote>
<p>Body4 QL4</p>
</blockquote>
</blockquote>
<ul>
<li>
<p><em>Unordered 1</em> QL2 level 1 no space sep Continued 1</p>
</li>
<li>
<p>Unordered 2 level 1</p>
</li>
</ul>
<ol start=""1"">
<li>
<p><strong>Ordered 1</strong> QL2 level 1 Continued 1</p>
</li>
</ol>
<blockquote>
<p>Not continued QL3 2. Ordered 2 QL2 level 1 Continued 2a</p>
</blockquote>
<p>Continued 2b</p>
<p>Continued 2c</p>
<ol start=""3"">
<li>
<p>Ordered 3 QL2 level 2 Continued 3a</p>
<p>Continued 3b</p>
</li>
</ol>
<p>Body 5</p>
<pre><code class=""language-LANG"">**CodeFence 1** QL2
FenceLine 2 QL2

  FenceLine 3
FenceLine 4
      FenceLine 5

</code></pre>
<pre><code>**Pre 1** QL2

    Pre 2 QL2
</code></pre>
<p>Table:</p>
<table style=""border-collapse:collapse;"">
<tr style=""border-style:solid; border-width:0 0 2px 0;"">
<th style=""text-align:left;"">A01</th>
<th style=""text-align:left;"">A02</th>
</tr>
<tr>
<td style=""text-align:left;"">a10</td>
<td style=""text-align:left;"">a11</td>
</tr>
</table>
</blockquote>
</blockquote>
<p>Final line QL0</p>";

    private const string ExpectQuotedMarkdown = @"> > # Heading `Code` 1 QL2\
>_
> ## Heading 2 **Bold** QL1 ##\
>_
> Body0 QL1
>_
> > Body1 QL2
> >_
> > > Body2 QL3 Body3 stays QL3
> > >_
> > > > Body4 QL4
> >_
> > * *Unordered 1* QL2 level 1 no space sep Continued 1
> >_
> > * Unordered 2 level 1
> >_
> > 1. **Ordered 1** QL2 level 1 Continued 1
> >_
> > > Not continued QL3 2. Ordered 2 QL2 level 1 Continued 2a
> >_
> > Continued 2b
> >_
> > Continued 2c
> >_
> > 3. Ordered 3 QL2 level 2 Continued 3a
> >_
> >    Continued 3b
> >_
> > Body 5
> >_
> > ```LANG
> > **CodeFence 1** QL2
> > FenceLine 2 QL2
> >
> >   FenceLine 3
> > FenceLine 4
> >       FenceLine 5
> >
> > ```
> >_
> >     **Pre 1** QL2
> >
> >         Pre 2 QL2
> >_
> > Table:
> >_
> > |A01|A02|
> > |---|---|
> > |a10|a11|

Final line QL0";

    private const string ExpectQuotedUnicodeMono = @"> > # Heading Code 1 QL2\
>_
> ## Heading 2 Bold QL1 ##\
>_
> Body0 QL1
>_
> > Body1 QL2
> >_
> > > Body2 QL3 Body3 stays QL3
> > >_
> > > > Body4 QL4
> >_
> > • Unordered 1 QL2 level 1 no space sep Continued 1
> >_
> > • Unordered 2 level 1
> >_
> > 1. Ordered 1 QL2 level 1 Continued 1
> >_
> > > Not continued QL3 2. Ordered 2 QL2 level 1 Continued 2a
> >_
> > Continued 2b
> >_
> > Continued 2c
> >_
> > 3. Ordered 3 QL2 level 2 Continued 3a
> >_
> >    Continued 3b
> >_
> > Body 5
> >_
> >     **CodeFence 1** QL2
> >     FenceLine 2 QL2
> >_
> >       FenceLine 3
> >     FenceLine 4
> >           FenceLine 5
> >_
> >_
> >     **Pre 1** QL2
> >_
> >         Pre 2 QL2
> >_
> > Table:
> >_
> > A01   A02
> > ─────────
> > a10   a11

Final line QL0";

    [Fact]
    public void ToString_Quoted_Html()
    {
        var obj = new MarkDocument(QuotedSource);
        var text = obj.ToString(TextFormat.Html);

        // Best to examine files.
        // The IDE trims spaces and removes lines in output.
        File.WriteAllText($"./{nameof(ToString_Quoted_Html)}.html", AddSpacePreserver(text));
        File.WriteAllText($"./{nameof(ToString_Quoted_Html)}.markdig.html", Markdig.Markdown.ToHtml(QuotedSource));

        Assert.Equal(ExpectQuotedHtml, text);
    }

    [Fact]
    public void ToString_Quoted_Markdown()
    {
        var obj = new MarkDocument(QuotedSource);
        var text = obj.ToString(TextFormat.Markdown);

        File.WriteAllText($"./{nameof(ToString_Quoted_Markdown)}.md", AddSpacePreserver(text));
        File.WriteAllText($"./{nameof(ToString_Quoted_Markdown)}.markdig.md", Markdig.Markdown.Normalize(QuotedSource));

        Assert.Equal(RemoveSpacePreserver(ExpectQuotedMarkdown), text);
    }

    [Fact]
    public void ToString_Quoted_UnicodeMono()
    {
        var obj = new MarkDocument(QuotedSource);
        var text = obj.ToString(TextFormat.Unicode);

        File.WriteAllText($"./{nameof(ToString_Quoted_UnicodeMono)}.txt", AddSpacePreserver(text));
        File.WriteAllText($"./{nameof(ToString_Quoted_UnicodeMono)}.markdig.txt", Markdig.Markdown.ToPlainText(QuotedSource));

        Assert.Equal(RemoveSpacePreserver(ExpectQuotedUnicodeMono), text);
    }

    [Fact]
    public void Update_Quoted_DefaultOpts()
    {
        // MARKDIG IS INCORRECT 0.41.3
        // This test can only act as regression
        var obj = UpdateWriteOut(QuotedSource);
        //
        // > > # Heading `Code` 1 QL2\\
        Assert.Equal(BlockKind.H1, obj[0].Kind);
        Assert.Equal(2, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Heading ", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Code, obj[0].Elements[1].Styling);
        Assert.Equal("Code", obj[0].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[2].Styling);
        Assert.Equal(" 1 QL2\\", obj[0].Elements[2].Text);
        //
        // > ## Heading 2 **Bold** QL1 ##\\
        Assert.Equal(BlockKind.H2, obj[1].Kind);
        Assert.Equal(1, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Heading 2 ", obj[1].Elements[0].Text);
        Assert.Equal(InlineStyling.Strong, obj[1].Elements[1].Styling);
        Assert.Equal("Bold", obj[1].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[2].Styling);
        Assert.Equal(" QL1 ##\\", obj[1].Elements[2].Text);
        //
        // > Body0 QL1
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(1, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Body0 QL1", obj[2].Elements[0].Text);
        //
        // > > Body1 QL2
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(2, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Body1 QL2", obj[3].Elements[0].Text);
        //
        // > > > Body2 QL3 Body3 stays QL3
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(3, obj[4].QuoteLevel);
        Assert.Equal(0, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('\0', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("Body2 QL3 Body3 stays QL3", obj[4].Elements[0].Text);
        //
        // > > > > Body4 QL4
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(4, obj[5].QuoteLevel);
        Assert.Equal(0, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("Body4 QL4", obj[5].Elements[0].Text);
        //
        // > > * *Unordered 1* QL2 level 1 no space sep Continued 1
        Assert.Equal(BlockKind.Para, obj[6].Kind);
        Assert.Equal(2, obj[6].QuoteLevel);
        Assert.Equal(1, obj[6].ListLevel);
        Assert.Equal(0, obj[6].ListOrder);
        Assert.Equal('*', obj[6].ListBullet);
        Assert.Equal(InlineStyling.Emphasis, obj[6].Elements[0].Styling);
        Assert.Equal("Unordered 1", obj[6].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[6].Elements[1].Styling);
        Assert.Equal(" QL2 level 1 no space sep Continued 1", obj[6].Elements[1].Text);
        //
        // > > * Unordered 2 level 1
        Assert.Equal(BlockKind.Para, obj[7].Kind);
        Assert.Equal(2, obj[7].QuoteLevel);
        Assert.Equal(1, obj[7].ListLevel);
        Assert.Equal(0, obj[7].ListOrder);
        Assert.Equal('*', obj[7].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[0].Styling);
        Assert.Equal("Unordered 2 level 1", obj[7].Elements[0].Text);
        //
        // > > 1. **Ordered 1** QL2 level 1 Continued 1
        Assert.Equal(BlockKind.Para, obj[8].Kind);
        Assert.Equal(2, obj[8].QuoteLevel);
        Assert.Equal(1, obj[8].ListLevel);
        Assert.Equal(1, obj[8].ListOrder);
        Assert.Equal('\0', obj[8].ListBullet);
        Assert.Equal(InlineStyling.Strong, obj[8].Elements[0].Styling);
        Assert.Equal("Ordered 1", obj[8].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[8].Elements[1].Styling);
        Assert.Equal(" QL2 level 1 Continued 1", obj[8].Elements[1].Text);
        //
        // > > > Not continued QL3 2. Ordered 2 QL2 level 1 Continued 2a
        Assert.Equal(BlockKind.Para, obj[9].Kind);
        Assert.Equal(3, obj[9].QuoteLevel);
        Assert.Equal(0, obj[9].ListLevel);
        Assert.Equal(0, obj[9].ListOrder);
        Assert.Equal('\0', obj[9].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[9].Elements[0].Styling);
        Assert.Equal("Not continued QL3 2. Ordered 2 QL2 level 1 Continued 2a", obj[9].Elements[0].Text);
        //
        // > > Continued 2b
        Assert.Equal(BlockKind.Para, obj[10].Kind);
        Assert.Equal(2, obj[10].QuoteLevel);
        Assert.Equal(0, obj[10].ListLevel);
        Assert.Equal(0, obj[10].ListOrder);
        Assert.Equal('\0', obj[10].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[10].Elements[0].Styling);
        Assert.Equal("Continued 2b", obj[10].Elements[0].Text);
        //
        // > > Continued 2c
        Assert.Equal(BlockKind.Para, obj[11].Kind);
        Assert.Equal(2, obj[11].QuoteLevel);
        Assert.Equal(0, obj[11].ListLevel);
        Assert.Equal(0, obj[11].ListOrder);
        Assert.Equal('\0', obj[11].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[11].Elements[0].Styling);
        Assert.Equal("Continued 2c", obj[11].Elements[0].Text);
        //
        // > > 3. Ordered 3 QL2 level 2 Continued 3a
        Assert.Equal(BlockKind.Para, obj[12].Kind);
        Assert.Equal(2, obj[12].QuoteLevel);
        Assert.Equal(1, obj[12].ListLevel);
        Assert.Equal(3, obj[12].ListOrder);
        Assert.Equal('\0', obj[12].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[12].Elements[0].Styling);
        Assert.Equal("Ordered 3 QL2 level 2 Continued 3a", obj[12].Elements[0].Text);
        //
        // > > Continued 3b
        Assert.Equal(BlockKind.Para, obj[13].Kind);
        Assert.Equal(2, obj[13].QuoteLevel);
        Assert.Equal(1, obj[13].ListLevel);
        Assert.Equal(0, obj[13].ListOrder);
        Assert.Equal('\0', obj[13].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[13].Elements[0].Styling);
        Assert.Equal("Continued 3b", obj[13].Elements[0].Text);
        //
        // > > Body 5
        Assert.Equal(BlockKind.Para, obj[14].Kind);
        Assert.Equal(2, obj[14].QuoteLevel);
        Assert.Equal(0, obj[14].ListLevel);
        Assert.Equal(0, obj[14].ListOrder);
        Assert.Equal('\0', obj[14].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[14].Elements[0].Styling);
        Assert.Equal("Body 5", obj[14].Elements[0].Text);
        //
        // > > ``````LANG\n> > **CodeFence 1** QL2\n> > FenceLine 2 QL2\n> >\n> >   FenceLine 3\n> > FenceLine 4\n> >       FenceLine 5\n> >\n> > ``````
        Assert.Equal(BlockKind.FencedCode, obj[15].Kind);
        Assert.Equal(2, obj[15].QuoteLevel);
        Assert.Equal(0, obj[15].ListLevel);
        Assert.Equal(0, obj[15].ListOrder);
        Assert.Equal('\0', obj[15].ListBullet);
        Assert.Equal("LANG", obj[15].Lang);
        Assert.Equal(InlineStyling.Default, obj[15].Elements[0].Styling);
        Assert.Equal("**CodeFence 1** QL2\nFenceLine 2 QL2\n\n  FenceLine 3\nFenceLine 4\n      FenceLine 5\n", obj[15].Elements[0].Text);
        //
        // > >    **Pre 1** QL2\n> >\n> >        Pre 2 QL2
        Assert.Equal(BlockKind.IndentedCode, obj[16].Kind);
        Assert.Equal(2, obj[16].QuoteLevel);
        Assert.Equal(0, obj[16].ListLevel);
        Assert.Equal(0, obj[16].ListOrder);
        Assert.Equal('\0', obj[16].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[16].Elements[0].Styling);
        Assert.Equal("**Pre 1** QL2\n\n    Pre 2 QL2", obj[16].Elements[0].Text);
        //
        // > > Table:
        Assert.Equal(BlockKind.Para, obj[17].Kind);
        Assert.Equal(2, obj[17].QuoteLevel);
        Assert.Equal(0, obj[17].ListLevel);
        Assert.Equal(0, obj[17].ListOrder);
        Assert.Equal('\0', obj[17].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[17].Elements[0].Styling);
        Assert.Equal("Table:", obj[17].Elements[0].Text);
        //
        // > > |A01|A02|\n> > |---|---|\n> > |a10|a11|
        Assert.Equal(BlockKind.TableCode, obj[18].Kind);
        Assert.Equal(2, obj[18].QuoteLevel);
        Assert.Equal(0, obj[18].ListLevel);
        Assert.Equal(0, obj[18].ListOrder);
        Assert.Equal('\0', obj[18].ListBullet);
        Assert.NotNull(obj[18].Table);
        Assert.Equal(2, obj[18].Table?.RowCount);
        Assert.Equal(2, obj[18].Table?.ColCount);
        //
        // Final line QL0
        Assert.Equal(BlockKind.Para, obj[19].Kind);
        Assert.Equal(0, obj[19].QuoteLevel);
        Assert.Equal(0, obj[19].ListLevel);
        Assert.Equal(0, obj[19].ListOrder);
        Assert.Equal('\0', obj[19].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[19].Elements[0].Styling);
        Assert.Equal("Final line QL0", obj[19].Elements[0].Text);
        //
        Assert.Equal(20, obj.Count);
    }

    private static string AddSpacePreserver(string expect)
    {
        // This is normal. Empty quoted lines end
        // with a space, but are stripped off by the IDE.
        return expect.Replace(" \n", "_\n");
    }

    private static string RemoveSpacePreserver(string expect)
    {
        // This is normal. Empty quoted lines end
        // with a space, but are stripped off by the IDE.
        return expect.Replace("_\n", " \n");
    }

}