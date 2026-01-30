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
    private const string BasicSource = @"
# Heading `Code` 1\
## Heading 2 **Bold** ##\
Body [Link](http://uri.com 'title')

### Heading 3
Body 1a
Body 1b continued\
  Body 1c continued\

Body   2a
    Body 2b continued


#### Heading  4
##### Heading 5
###### Heading 6

Body   3
![Image](http://uri.com 'title')

* _Unordered 1a_
  Continued 1b

- Unordered 2 level 1
  + Unordered 3 level 2

  + Unordered 4 level 2

    + Unordered 5 level 3

1. **Ordered 1a** with stop
   Continued 1a
2877) Ordered 2a with close brace
      Continued 2b

      Continued 2c

      Continued 2d

      3023. Ordered 3 level 2

      30234. Ordered 4a level 2
        Continued 4b

             Continued 4c

             5. Ordered 5 level 3

      6. Ordered 6 level 2

Body 4

  ```lang with indent
**CodeFence**
FenceLine 2

    FenceLine 3 lang indent subtracted
  FenceLine 4
        FenceLine 5

  ```
    **Pre 1** indent preserved

        Pre 2
Not Pre

>Quote1 no space sep
Continue

Table:

|A01|A02|
|-|-|
|a10|a11|
";

    private const string ExpectBasicHtml = @"<h1>Heading <code>Code</code> 1\</h1>
<h2>Heading 2 <strong>Bold</strong> ##\</h2>
<p>Body <a href=""http://uri.com"" title=""title"">Link</a></p>
<h3>Heading 3</h3>
<p>Body 1a Body 1b continued<br />Body 1c continued\</p>
<p>Body   2a Body 2b continued</p>
<h4>Heading  4</h4>
<h5>Heading 5</h5>
<h6>Heading 6</h6>
<p>Body   3 <img src=""http://uri.com"" alt=""Image"" title=""title"" /></p>
<ul>
<li>
<p><em>Unordered 1a</em> Continued 1b</p>
</li>
</ul>
<ul>
<li>
<p>Unordered 2 level 1</p>
<ul>
<li>
<p>Unordered 3 level 2</p>
</li>
<li>
<p>Unordered 4 level 2</p>
<ul>
<li>
<p>Unordered 5 level 3</p>
</li>
</ul>
</li>
</ul>
</li>
</ul>
<ol start=""1"">
<li>
<p><strong>Ordered 1a</strong> with stop Continued 1a</p>
</li>
</ol>
<ol start=""2877"">
<li>
<p>Ordered 2a with close brace Continued 2b</p>
<p>Continued 2c</p>
<p>Continued 2d</p>
<ol start=""3023"">
<li>
<p>Ordered 3 level 2</p>
</li>
</ol>
<ol start=""30234"">
<li>
<p>Ordered 4a level 2 Continued 4b</p>
<p>Continued 4c</p>
<ol start=""5"">
<li>
<p>Ordered 5 level 3</p>
</li>
</ol>
</li>
</ol>
<ol start=""6"">
<li>
<p>Ordered 6 level 2</p>
</li>
</ol>
</li>
</ol>
<p>Body 4</p>
<pre><code class=""language-lang"">**CodeFence**
FenceLine 2

  FenceLine 3 lang indent subtracted
FenceLine 4
      FenceLine 5

</code></pre>
<pre><code>**Pre 1** indent preserved

    Pre 2
</code></pre>
<p>Not Pre</p>
<blockquote>
<p>Quote1 no space sep Continue</p>
</blockquote>
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
</table>";

    private const string ExpectBasicMarkdown = @"# Heading `Code` 1\

## Heading 2 **Bold** ##\

Body [Link](http://uri.com ""title"")

### Heading 3

Body 1a Body 1b continued\
Body 1c continued\

Body   2a Body 2b continued

#### Heading  4

##### Heading 5

###### Heading 6

Body   3 ![Image](http://uri.com ""title"")

* *Unordered 1a* Continued 1b

- Unordered 2 level 1

  + Unordered 3 level 2

  + Unordered 4 level 2

    + Unordered 5 level 3

1. **Ordered 1a** with stop Continued 1a

2877. Ordered 2a with close brace Continued 2b

      Continued 2c

      Continued 2d

      3023. Ordered 3 level 2

      30234. Ordered 4a level 2 Continued 4b

             Continued 4c

             5. Ordered 5 level 3

      6. Ordered 6 level 2

Body 4

```lang
**CodeFence**
FenceLine 2

  FenceLine 3 lang indent subtracted
FenceLine 4
      FenceLine 5

```

    **Pre 1** indent preserved

        Pre 2

Not Pre

> Quote1 no space sep Continue

Table:

|A01|A02|
|---|---|
|a10|a11|";

    private const string ExpectBasicUnicodeMono = @"# Heading Code 1\

## Heading 2 Bold ##\

Body Link

### Heading 3

Body 1a Body 1b continued
Body 1c continued\

Body   2a Body 2b continued

#### Heading  4

##### Heading 5

###### Heading 6

Body   3 Image

• Unordered 1a Continued 1b

• Unordered 2 level 1

  ◦ Unordered 3 level 2

  ◦ Unordered 4 level 2

    ▪ Unordered 5 level 3

1. Ordered 1a with stop Continued 1a

2877. Ordered 2a with close brace Continued 2b

      Continued 2c

      Continued 2d

      3023. Ordered 3 level 2

      30234. Ordered 4a level 2 Continued 4b

             Continued 4c

             5. Ordered 5 level 3

      6. Ordered 6 level 2

Body 4

    **CodeFence**
    FenceLine 2

      FenceLine 3 lang indent subtracted
    FenceLine 4
          FenceLine 5


    **Pre 1** indent preserved

        Pre 2

Not Pre

> Quote1 no space sep Continue

Table:

A01   A02
─────────
a10   a11";

    [Fact]
    public void ToString_Basic_Html()
    {
        var obj = new MarkDocument(BasicSource);
        var text = obj.ToString(TextFormat.Html);

        File.WriteAllText($"./{nameof(ToString_Basic_Html)}.html", text);
        File.WriteAllText($"./{nameof(ToString_Basic_Html)}.markdig.html", Markdig.Markdown.ToHtml(BasicSource));

        ConditionalDebug.WriteLine(text, false);

        Assert.Equal(ExpectBasicHtml, text);
    }

    [Fact]
    public void ToString_Basic_Markdown()
    {
        var obj = new MarkDocument(BasicSource);
        var text = obj.ToString(TextFormat.Markdown);

        File.WriteAllText($"./{nameof(ToString_Basic_Markdown)}.md", text);
        File.WriteAllText($"./{nameof(ToString_Basic_Markdown)}.markdig.md", Markdig.Markdown.Normalize(BasicSource));

        ConditionalDebug.WriteLine(text, false);

        Assert.Equal(ExpectBasicMarkdown, text);
    }

    [Fact]
    public void ToString_Basic_UnicodeMono()
    {
        var obj = new MarkDocument(BasicSource);
        var text = obj.ToString(TextFormat.Unicode);

        File.WriteAllText($"./{nameof(ToString_Basic_UnicodeMono)}.txt", text);
        File.WriteAllText($"./{nameof(ToString_Basic_UnicodeMono)}.markdig.txt", Markdig.Markdown.ToPlainText(BasicSource));

        ConditionalDebug.WriteLine(text, false);

        Assert.Equal(ExpectBasicUnicodeMono, text);
    }

    [Fact]
    public void Update_Basic_IgnoreInline()
    {
        var obj = UpdateWriteOut(BasicSource, MarkOptions.IgnoreInline);

        // # Heading `Code` 1\\\\\n## Heading 2 **Bold** ##\\\\\nBody [Link]([http://uri.com](http://uri.com) 'title')
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("# Heading `Code` 1\\\n## Heading 2 **Bold** ##\\\nBody [Link](", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal("http://uri.com", obj[0].Elements[1].Text);
        Assert.False(obj[0].Elements[1].Link?.IsImage);
        Assert.Equal("http://uri.com", obj[0].Elements[1].Link?.ToString());
        Assert.Null(obj[0].Elements[1].Link?.Title);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[2].Styling);
        Assert.Equal(" 'title')", obj[0].Elements[2].Text);
        //
        // ### Heading 3\\\nBody 1a\\\nBody 1b continued\\\\\nBody 1c continued\\
        Assert.Equal(BlockKind.Para, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("### Heading 3\nBody 1a\nBody 1b continued\\\nBody 1c continued\\", obj[1].Elements[0].Text);
        //
        // Body   2a\\\nBody 2b continued
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Body   2a\nBody 2b continued", obj[2].Elements[0].Text);
        //
        // #### Heading  4\\\n##### Heading 5\\\n###### Heading 6
        Assert.Equal(BlockKind.Para, obj[3].Kind);
        Assert.Equal(0, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("#### Heading  4\n##### Heading 5\n###### Heading 6", obj[3].Elements[0].Text);
        //
        // Body   3\\\n![Image]([http://uri.com](http://uri.com) 'title')
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(0, obj[4].QuoteLevel);
        Assert.Equal(0, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('\0', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("Body   3\n![Image](", obj[4].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[1].Styling);
        Assert.Equal("http://uri.com", obj[4].Elements[1].Text);
        Assert.False(obj[4].Elements[1].Link?.IsImage);
        Assert.Equal("http://uri.com", obj[4].Elements[1].Link?.ToString());
        Assert.Null(obj[4].Elements[1].Link?.Title);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[2].Styling);
        Assert.Equal(" 'title')", obj[4].Elements[2].Text);
        //
        // * _Unordered 1a_\\\nContinued 1b
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(0, obj[5].QuoteLevel);
        Assert.Equal(0, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("* _Unordered 1a_\nContinued 1b", obj[5].Elements[0].Text);
        //
        // - Unordered 2 level 1\\\n+ Unordered 3 level 2
        Assert.Equal(BlockKind.Para, obj[6].Kind);
        Assert.Equal(0, obj[6].QuoteLevel);
        Assert.Equal(0, obj[6].ListLevel);
        Assert.Equal(0, obj[6].ListOrder);
        Assert.Equal('\0', obj[6].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[6].Elements[0].Styling);
        Assert.Equal("- Unordered 2 level 1\n+ Unordered 3 level 2", obj[6].Elements[0].Text);
        //
        // + Unordered 4 level 2
        Assert.Equal(BlockKind.Para, obj[7].Kind);
        Assert.Equal(0, obj[7].QuoteLevel);
        Assert.Equal(0, obj[7].ListLevel);
        Assert.Equal(0, obj[7].ListOrder);
        Assert.Equal('\0', obj[7].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[0].Styling);
        Assert.Equal("+ Unordered 4 level 2", obj[7].Elements[0].Text);
        //
        // + Unordered 5 level 3
        Assert.Equal(BlockKind.IndentedCode, obj[8].Kind);
        Assert.Equal(0, obj[8].QuoteLevel);
        Assert.Equal(0, obj[8].ListLevel);
        Assert.Equal(0, obj[8].ListOrder);
        Assert.Equal('\0', obj[8].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[8].Elements[0].Styling);
        Assert.Equal("+ Unordered 5 level 3", obj[8].Elements[0].Text);
        //
        // 1. **Ordered 1a** with stop\\\nContinued 1a\\\n2877) Ordered 2a with close brace\\\nContinued 2b
        Assert.Equal(BlockKind.Para, obj[9].Kind);
        Assert.Equal(0, obj[9].QuoteLevel);
        Assert.Equal(0, obj[9].ListLevel);
        Assert.Equal(0, obj[9].ListOrder);
        Assert.Equal('\0', obj[9].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[9].Elements[0].Styling);
        Assert.Equal("1. **Ordered 1a** with stop\nContinued 1a\n2877) Ordered 2a with close brace\nContinued 2b", obj[9].Elements[0].Text);
        //
        //   Continued 2c\n\n  Continued 2d\n\n  3023. Ordered 3 level 2\n\n  30234. Ordered 4a level 2\n    Continued 4b\n\n         Continued 4c\n\n         5. Ordered 5 level 3\n\n  6. Ordered 6 level 2
        Assert.Equal(BlockKind.IndentedCode, obj[10].Kind);
        Assert.Equal(0, obj[10].QuoteLevel);
        Assert.Equal(0, obj[10].ListLevel);
        Assert.Equal(0, obj[10].ListOrder);
        Assert.Equal('\0', obj[10].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[10].Elements[0].Styling);
        Assert.Equal("  Continued 2c\n\n  Continued 2d\n\n  3023. Ordered 3 level 2\n\n  30234. Ordered 4a level 2\n    Continued 4b\n\n         Continued 4c\n\n         5. Ordered 5 level 3\n\n  6. Ordered 6 level 2", obj[10].Elements[0].Text);
        //
        // Body 4
        Assert.Equal(BlockKind.Para, obj[11].Kind);
        Assert.Equal(0, obj[11].QuoteLevel);
        Assert.Equal(0, obj[11].ListLevel);
        Assert.Equal(0, obj[11].ListOrder);
        Assert.Equal('\0', obj[11].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[11].Elements[0].Styling);
        Assert.Equal("Body 4", obj[11].Elements[0].Text);
        //
        // ```lang\n**CodeFence**\nFenceLine 2\n\n  FenceLine 3 lang indent subtracted\nFenceLine 4\n      FenceLine 5\n\n```
        Assert.Equal(BlockKind.FencedCode, obj[12].Kind);
        Assert.Equal(0, obj[12].QuoteLevel);
        Assert.Equal(0, obj[12].ListLevel);
        Assert.Equal(0, obj[12].ListOrder);
        Assert.Equal('\0', obj[12].ListBullet);
        Assert.Equal("lang", obj[12].Lang);
        Assert.Equal(InlineStyling.Default, obj[12].Elements[0].Styling);
        Assert.Equal("**CodeFence**\nFenceLine 2\n\n  FenceLine 3 lang indent subtracted\nFenceLine 4\n      FenceLine 5\n", obj[12].Elements[0].Text);
        //
        // **Pre 1** indent preserved\n\n    Pre 2
        Assert.Equal(BlockKind.IndentedCode, obj[13].Kind);
        Assert.Equal(0, obj[13].QuoteLevel);
        Assert.Equal(0, obj[13].ListLevel);
        Assert.Equal(0, obj[13].ListOrder);
        Assert.Equal('\0', obj[13].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[13].Elements[0].Styling);
        Assert.Equal("**Pre 1** indent preserved\n\n    Pre 2", obj[13].Elements[0].Text);
        //
        // Not Pre
        Assert.Equal(BlockKind.Para, obj[14].Kind);
        Assert.Equal(0, obj[14].QuoteLevel);
        Assert.Equal(0, obj[14].ListLevel);
        Assert.Equal(0, obj[14].ListOrder);
        Assert.Equal('\0', obj[14].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[14].Elements[0].Styling);
        Assert.Equal("Not Pre", obj[14].Elements[0].Text);
        //
        // >Quote1 no space sep\\\nContinue
        Assert.Equal(BlockKind.Para, obj[15].Kind);
        Assert.Equal(0, obj[15].QuoteLevel);
        Assert.Equal(0, obj[15].ListLevel);
        Assert.Equal(0, obj[15].ListOrder);
        Assert.Equal('\0', obj[15].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[15].Elements[0].Styling);
        Assert.Equal(">Quote1 no space sep\nContinue", obj[15].Elements[0].Text);
        //
        // Table:
        Assert.Equal(BlockKind.Para, obj[16].Kind);
        Assert.Equal(0, obj[16].QuoteLevel);
        Assert.Equal(0, obj[16].ListLevel);
        Assert.Equal(0, obj[16].ListOrder);
        Assert.Equal('\0', obj[16].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[16].Elements[0].Styling);
        Assert.Equal("Table:", obj[16].Elements[0].Text);
        //
        // |A01|A02|\\\n|-|-|\\\n|a10|a11|
        Assert.Equal(BlockKind.Para, obj[17].Kind);
        Assert.Equal(0, obj[17].QuoteLevel);
        Assert.Equal(0, obj[17].ListLevel);
        Assert.Equal(0, obj[17].ListOrder);
        Assert.Equal('\0', obj[17].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[17].Elements[0].Styling);
        Assert.Equal("|A01|A02|\n|-|-|\n|a10|a11|", obj[17].Elements[0].Text);
        //
        Assert.Equal(18, obj.Count);

    }

    [Fact]
    public void Update_Basic_DefaultOpts()
    {
        var obj = UpdateWriteOut(BasicSource);
        //
        // # Heading `Code` 1\\
        Assert.Equal(BlockKind.H1, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Heading ", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Code, obj[0].Elements[1].Styling);
        Assert.Equal("Code", obj[0].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[2].Styling);
        Assert.Equal(" 1\\", obj[0].Elements[2].Text);
        //
        // ## Heading 2 **Bold** ##\\
        Assert.Equal(BlockKind.H2, obj[1].Kind);
        Assert.Equal(0, obj[1].QuoteLevel);
        Assert.Equal(0, obj[1].ListLevel);
        Assert.Equal(0, obj[1].ListOrder);
        Assert.Equal('\0', obj[1].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[0].Styling);
        Assert.Equal("Heading 2 ", obj[1].Elements[0].Text);
        Assert.Equal(InlineStyling.Strong, obj[1].Elements[1].Styling);
        Assert.Equal("Bold", obj[1].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[1].Elements[2].Styling);
        Assert.Equal(" ##\\", obj[1].Elements[2].Text);
        //
        // Body [Link](http://uri.com "title")
        Assert.Equal(BlockKind.Para, obj[2].Kind);
        Assert.Equal(0, obj[2].QuoteLevel);
        Assert.Equal(0, obj[2].ListLevel);
        Assert.Equal(0, obj[2].ListOrder);
        Assert.Equal('\0', obj[2].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[0].Styling);
        Assert.Equal("Body ", obj[2].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[2].Elements[1].Styling);
        Assert.Equal("Link", obj[2].Elements[1].Text);
        Assert.False(obj[2].Elements[1].Link?.IsImage);
        Assert.Equal("http://uri.com", obj[2].Elements[1].Link?.ToString());
        Assert.Equal("title", obj[2].Elements[1].Link?.Title);
        //
        // ### Heading 3
        Assert.Equal(BlockKind.H3, obj[3].Kind);
        Assert.Equal(0, obj[3].QuoteLevel);
        Assert.Equal(0, obj[3].ListLevel);
        Assert.Equal(0, obj[3].ListOrder);
        Assert.Equal('\0', obj[3].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[3].Elements[0].Styling);
        Assert.Equal("Heading 3", obj[3].Elements[0].Text);
        //
        // Body 1a Body 1b continued\\\nBody 1c continued\\
        Assert.Equal(BlockKind.Para, obj[4].Kind);
        Assert.Equal(0, obj[4].QuoteLevel);
        Assert.Equal(0, obj[4].ListLevel);
        Assert.Equal(0, obj[4].ListOrder);
        Assert.Equal('\0', obj[4].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[0].Styling);
        Assert.Equal("Body 1a Body 1b continued", obj[4].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[1].Styling);
        Assert.Equal("\n", obj[4].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[4].Elements[2].Styling);
        Assert.Equal("Body 1c continued\\", obj[4].Elements[2].Text);
        //
        // Body   2a Body 2b continued
        Assert.Equal(BlockKind.Para, obj[5].Kind);
        Assert.Equal(0, obj[5].QuoteLevel);
        Assert.Equal(0, obj[5].ListLevel);
        Assert.Equal(0, obj[5].ListOrder);
        Assert.Equal('\0', obj[5].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[5].Elements[0].Styling);
        Assert.Equal("Body   2a Body 2b continued", obj[5].Elements[0].Text);
        //
        // #### Heading  4
        Assert.Equal(BlockKind.H4, obj[6].Kind);
        Assert.Equal(0, obj[6].QuoteLevel);
        Assert.Equal(0, obj[6].ListLevel);
        Assert.Equal(0, obj[6].ListOrder);
        Assert.Equal('\0', obj[6].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[6].Elements[0].Styling);
        Assert.Equal("Heading  4", obj[6].Elements[0].Text);
        //
        // ##### Heading 5
        Assert.Equal(BlockKind.H5, obj[7].Kind);
        Assert.Equal(0, obj[7].QuoteLevel);
        Assert.Equal(0, obj[7].ListLevel);
        Assert.Equal(0, obj[7].ListOrder);
        Assert.Equal('\0', obj[7].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[7].Elements[0].Styling);
        Assert.Equal("Heading 5", obj[7].Elements[0].Text);
        //
        // ###### Heading 6
        Assert.Equal(BlockKind.H6, obj[8].Kind);
        Assert.Equal(0, obj[8].QuoteLevel);
        Assert.Equal(0, obj[8].ListLevel);
        Assert.Equal(0, obj[8].ListOrder);
        Assert.Equal('\0', obj[8].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[8].Elements[0].Styling);
        Assert.Equal("Heading 6", obj[8].Elements[0].Text);
        //
        // Body   3 ![Image](http://uri.com "title")
        Assert.Equal(BlockKind.Para, obj[9].Kind);
        Assert.Equal(0, obj[9].QuoteLevel);
        Assert.Equal(0, obj[9].ListLevel);
        Assert.Equal(0, obj[9].ListOrder);
        Assert.Equal('\0', obj[9].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[9].Elements[0].Styling);
        Assert.Equal("Body   3 ", obj[9].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[9].Elements[1].Styling);
        Assert.Equal("Image", obj[9].Elements[1].Text);
        Assert.True(obj[9].Elements[1].Link?.IsImage);
        Assert.Equal("http://uri.com", obj[9].Elements[1].Link?.ToString());
        Assert.Equal("title", obj[9].Elements[1].Link?.Title);
        //
        // * *Unordered 1a* Continued 1b
        Assert.Equal(BlockKind.Para, obj[10].Kind);
        Assert.Equal(0, obj[10].QuoteLevel);
        Assert.Equal(1, obj[10].ListLevel);
        Assert.Equal(0, obj[10].ListOrder);
        Assert.Equal('*', obj[10].ListBullet);
        Assert.Equal(InlineStyling.Emphasis, obj[10].Elements[0].Styling);
        Assert.Equal("Unordered 1a", obj[10].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[10].Elements[1].Styling);
        Assert.Equal(" Continued 1b", obj[10].Elements[1].Text);
        //
        // - Unordered 2 level 1
        Assert.Equal(BlockKind.Para, obj[11].Kind);
        Assert.Equal(0, obj[11].QuoteLevel);
        Assert.Equal(1, obj[11].ListLevel);
        Assert.Equal(0, obj[11].ListOrder);
        Assert.Equal('-', obj[11].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[11].Elements[0].Styling);
        Assert.Equal("Unordered 2 level 1", obj[11].Elements[0].Text);
        //
        // + Unordered 3 level 2
        Assert.Equal(BlockKind.Para, obj[12].Kind);
        Assert.Equal(0, obj[12].QuoteLevel);
        Assert.Equal(2, obj[12].ListLevel);
        Assert.Equal(0, obj[12].ListOrder);
        Assert.Equal('+', obj[12].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[12].Elements[0].Styling);
        Assert.Equal("Unordered 3 level 2", obj[12].Elements[0].Text);
        //
        // + Unordered 4 level 2
        Assert.Equal(BlockKind.Para, obj[13].Kind);
        Assert.Equal(0, obj[13].QuoteLevel);
        Assert.Equal(2, obj[13].ListLevel);
        Assert.Equal(0, obj[13].ListOrder);
        Assert.Equal('+', obj[13].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[13].Elements[0].Styling);
        Assert.Equal("Unordered 4 level 2", obj[13].Elements[0].Text);
        //
        // + Unordered 5 level 3
        Assert.Equal(BlockKind.Para, obj[14].Kind);
        Assert.Equal(0, obj[14].QuoteLevel);
        Assert.Equal(3, obj[14].ListLevel);
        Assert.Equal(0, obj[14].ListOrder);
        Assert.Equal('+', obj[14].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[14].Elements[0].Styling);
        Assert.Equal("Unordered 5 level 3", obj[14].Elements[0].Text);
        //
        // 1. **Ordered 1a** with stop Continued 1a
        Assert.Equal(BlockKind.Para, obj[15].Kind);
        Assert.Equal(0, obj[15].QuoteLevel);
        Assert.Equal(1, obj[15].ListLevel);
        Assert.Equal(1, obj[15].ListOrder);
        Assert.Equal('\0', obj[15].ListBullet);
        Assert.Equal(InlineStyling.Strong, obj[15].Elements[0].Styling);
        Assert.Equal("Ordered 1a", obj[15].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[15].Elements[1].Styling);
        Assert.Equal(" with stop Continued 1a", obj[15].Elements[1].Text);

        //
        // 2877. Ordered 2a with close brace Continued 2b
        Assert.Equal(BlockKind.Para, obj[16].Kind);
        Assert.Equal(0, obj[16].QuoteLevel);
        Assert.Equal(1, obj[16].ListLevel);
        Assert.Equal(2877, obj[16].ListOrder);
        Assert.Equal('\0', obj[16].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[16].Elements[0].Styling);
        Assert.Equal("Ordered 2a with close brace Continued 2b", obj[16].Elements[0].Text);
        //
        // Continued 2c
        Assert.Equal(BlockKind.Para, obj[17].Kind);
        Assert.Equal(0, obj[17].QuoteLevel);
        Assert.Equal(1, obj[17].ListLevel);
        Assert.Equal(0, obj[17].ListOrder);
        Assert.Equal('\0', obj[17].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[17].Elements[0].Styling);
        Assert.Equal("Continued 2c", obj[17].Elements[0].Text);
        //
        // Continued 2d
        Assert.Equal(BlockKind.Para, obj[18].Kind);
        Assert.Equal(0, obj[18].QuoteLevel);
        Assert.Equal(1, obj[18].ListLevel);
        Assert.Equal(0, obj[18].ListOrder);
        Assert.Equal('\0', obj[18].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[18].Elements[0].Styling);
        Assert.Equal("Continued 2d", obj[18].Elements[0].Text);
        //
        // 3023. Ordered 3 level 2
        Assert.Equal(BlockKind.Para, obj[19].Kind);
        Assert.Equal(0, obj[19].QuoteLevel);
        Assert.Equal(2, obj[19].ListLevel);
        Assert.Equal(3023, obj[19].ListOrder);
        Assert.Equal('\0', obj[19].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[19].Elements[0].Styling);
        Assert.Equal("Ordered 3 level 2", obj[19].Elements[0].Text);
        //
        // 30234. Ordered 4a level 2 Continued 4b
        Assert.Equal(BlockKind.Para, obj[20].Kind);
        Assert.Equal(0, obj[20].QuoteLevel);
        Assert.Equal(2, obj[20].ListLevel);
        Assert.Equal(30234, obj[20].ListOrder);
        Assert.Equal('\0', obj[20].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[20].Elements[0].Styling);
        Assert.Equal("Ordered 4a level 2 Continued 4b", obj[20].Elements[0].Text);
        //
        // Continued 4c
        Assert.Equal(BlockKind.Para, obj[21].Kind);
        Assert.Equal(0, obj[21].QuoteLevel);
        Assert.Equal(2, obj[21].ListLevel);
        Assert.Equal(0, obj[21].ListOrder);
        Assert.Equal('\0', obj[21].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[21].Elements[0].Styling);
        Assert.Equal("Continued 4c", obj[21].Elements[0].Text);
        //
        // 5. Ordered 5 level 3
        Assert.Equal(BlockKind.Para, obj[22].Kind);
        Assert.Equal(0, obj[22].QuoteLevel);
        Assert.Equal(3, obj[22].ListLevel);
        Assert.Equal(5, obj[22].ListOrder);
        Assert.Equal('\0', obj[22].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[22].Elements[0].Styling);
        Assert.Equal("Ordered 5 level 3", obj[22].Elements[0].Text);
        //
        // 6. Ordered 6 level 2
        Assert.Equal(BlockKind.Para, obj[23].Kind);
        Assert.Equal(0, obj[23].QuoteLevel);
        Assert.Equal(2, obj[23].ListLevel);
        Assert.Equal(6, obj[23].ListOrder);
        Assert.Equal('\0', obj[23].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[23].Elements[0].Styling);
        Assert.Equal("Ordered 6 level 2", obj[23].Elements[0].Text);
        //
        // Body 4
        Assert.Equal(BlockKind.Para, obj[24].Kind);
        Assert.Equal(0, obj[24].QuoteLevel);
        Assert.Equal(0, obj[24].ListLevel);
        Assert.Equal(0, obj[24].ListOrder);
        Assert.Equal('\0', obj[24].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[24].Elements[0].Styling);
        Assert.Equal("Body 4", obj[24].Elements[0].Text);
        //
        // ``````lang\n**CodeFence**\nFenceLine 2\n\n  FenceLine 3 lang indent subtracted\nFenceLine 4\n      FenceLine 5\n\n``````
        Assert.Equal(BlockKind.FencedCode, obj[25].Kind);
        Assert.Equal(0, obj[25].QuoteLevel);
        Assert.Equal(0, obj[25].ListLevel);
        Assert.Equal(0, obj[25].ListOrder);
        Assert.Equal('\0', obj[25].ListBullet);
        Assert.Equal("lang", obj[25].Lang);
        Assert.Equal(InlineStyling.Default, obj[25].Elements[0].Styling);
        Assert.Equal("**CodeFence**\nFenceLine 2\n\n  FenceLine 3 lang indent subtracted\nFenceLine 4\n      FenceLine 5\n", obj[25].Elements[0].Text);
        //
        //     **Pre 1** indent preserved\n\n        Pre 2
        Assert.Equal(BlockKind.IndentedCode, obj[26].Kind);
        Assert.Equal(0, obj[26].QuoteLevel);
        Assert.Equal(0, obj[26].ListLevel);
        Assert.Equal(0, obj[26].ListOrder);
        Assert.Equal('\0', obj[26].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[26].Elements[0].Styling);
        Assert.Equal("**Pre 1** indent preserved\n\n    Pre 2", obj[26].Elements[0].Text);
        //
        // Not Pre
        Assert.Equal(BlockKind.Para, obj[27].Kind);
        Assert.Equal(0, obj[27].QuoteLevel);
        Assert.Equal(0, obj[27].ListLevel);
        Assert.Equal(0, obj[27].ListOrder);
        Assert.Equal('\0', obj[27].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[27].Elements[0].Styling);
        Assert.Equal("Not Pre", obj[27].Elements[0].Text);
        //
        // > Quote1 no space sep Continue
        Assert.Equal(BlockKind.Para, obj[28].Kind);
        Assert.Equal(1, obj[28].QuoteLevel);
        Assert.Equal(0, obj[28].ListLevel);
        Assert.Equal(0, obj[28].ListOrder);
        Assert.Equal('\0', obj[28].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[28].Elements[0].Styling);
        Assert.Equal("Quote1 no space sep Continue", obj[28].Elements[0].Text);
        //
        // Table:
        Assert.Equal(BlockKind.Para, obj[29].Kind);
        Assert.Equal(0, obj[29].QuoteLevel);
        Assert.Equal(0, obj[29].ListLevel);
        Assert.Equal(0, obj[29].ListOrder);
        Assert.Equal('\0', obj[29].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[29].Elements[0].Styling);
        Assert.Equal("Table:", obj[29].Elements[0].Text);
        //
        // |A01|A02|\n|---|---|\n|a10|a11|
        Assert.Equal(BlockKind.TableCode, obj[30].Kind);
        Assert.Equal(0, obj[30].QuoteLevel);
        Assert.Equal(0, obj[30].ListLevel);
        Assert.Equal(0, obj[30].ListOrder);
        Assert.Equal('\0', obj[30].ListBullet);
        Assert.NotNull(obj[30].Table);
        Assert.Equal(2, obj[30].Table?.RowCount);
        Assert.Equal(2, obj[30].Table?.ColCount);
        //
        Assert.Equal(31, obj.Count);
    }

    [Fact]
    public void Update_Basic_DefaultOpts_Roundtrip()
    {
        var obj0 = new MarkDocument(BasicSource);
        var mark0 = obj0.ToString(TextFormat.Markdown);
        ConditionalDebug.WriteLine("// OBJ0:", false);
        ConditionalDebug.WriteLine(mark0, false);

        var obj1 = new MarkDocument(mark0);

        ConditionalDebug.WriteLine("//", false);
        ConditionalDebug.WriteLine("//", false);
        ConditionalDebug.WriteLine("//", false);
        ConditionalDebug.WriteLine("// OBJ1:", false);
        ConditionalDebug.WriteLine(obj1.ToString(TextFormat.Markdown));

        var b0 = obj0;
        var b1 = obj1;

        _out.WriteLine("Check...");
        var count = Math.Min(b0.Count, b1.Count);

        for (int n = 0; n < count; ++n)
        {
            if (!b0[n].Equals(b1[n]))
            {
                // See if we can see what's wrong
                _out.WriteLine("");
                _out.WriteLine("FAILED BLOCK: " + n);

                _out.WriteLine("");
                _out.WriteLine("DOC0 BLOCK:");
                _out.WriteLine($"Item.Count: {b0[n].Elements.Count}");
                _out.WriteLine($"Kind: {b0[n].Kind}");
                _out.WriteLine($"QuoteLevel: {b0[n].QuoteLevel}");
                _out.WriteLine($"ListLevel: {b0[n].ListLevel}");
                _out.WriteLine($"Lang: {b0[n].Lang}");
                _out.WriteLine("start>>");
                _out.WriteLine(b0[n].ToString(TextFormat.Markdown));
                _out.WriteLine("<<end");

                _out.WriteLine("");
                _out.WriteLine("DOC1 BLOCK:");
                _out.WriteLine($"Item.Count: {b1[n].Elements.Count}");
                _out.WriteLine($"Kind: {b1[n].Kind}");
                _out.WriteLine($"QuoteLevel: {b1[n].QuoteLevel}");
                _out.WriteLine($"ListLevel: {b1[n].ListLevel}");
                _out.WriteLine($"Lang: {b1[n].Lang}");
                _out.WriteLine("start>>");
                _out.WriteLine(b1[n].ToString(TextFormat.Markdown));
                _out.WriteLine("<<end");

                Assert.Equal(b0[n].Elements[0].Text, b1[n].Elements[0].Text);
                Assert.Fail("Failed at block " + n);
            }
        }

        Assert.Equal(b0.Count, b1.Count);
        Assert.True(obj0.Equals(obj1));
    }

}