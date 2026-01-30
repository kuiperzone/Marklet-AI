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
    private const string StyledSource = @"
**Strong1** *Em1* __Strong2__ _Em2_ `Inline Code`
<b>Bold</b> <i>Italic</i> <s>Strike</s> <u>Underline</u> <samp>Samp</samp>
<sup>Sup</Sup><sub>Sub</sub> <mark>Mark</mark>**<em>Mixed</em>**
<Invalid1></Invalid2><Invalid2></Invalid1>";

    private const string ExpectStyledHtml = @"<p><strong>Strong1</strong> <em>Em1</em> <strong>Strong2</strong> <em>Em2</em> <code>Inline Code</code> <strong>Bold</strong> <em>Italic</em> <del>Strike</del> <ins>Underline</ins> <samp>Samp</samp> <sup>Sup</sup><sub>Sub</sub> <mark>Mark</mark><em><strong>Mixed</strong></em></p>";

    private const string ExpectStyledMarkdown = @"**Strong1** *Em1* **Strong2** *Em2* `Inline Code` **Bold** *Italic* <del>Strike</del> <ins>Underline</ins> <samp>Samp</samp> <sup>Sup</sup><sub>Sub</sub> <mark>Mark</mark>***Mixed***";

    private const string ExpectStyledUnicodeMono = @"Strong1 Em1 Strong2 Em2 Inline Code Bold Italic Strike Underline Samp SᵘᵖSᵤb MarkMixed";

    [Fact]
    public void ToString_Styled_Html()
    {
        var obj = new MarkDocument(StyledSource);
        var text = obj.ToString(TextFormat.Html);

        File.WriteAllText($"./{nameof(ToString_Styled_Html)}.html", text);
        File.WriteAllText($"./{nameof(ToString_Styled_Html)}.markdig.html", Markdig.Markdown.ToHtml(StyledSource));

        _out.WriteLine(text);

        Assert.Equal(ExpectStyledHtml, text);
    }

    [Fact]
    public void ToString_Styled_Markdown()
    {
        var obj = new MarkDocument(StyledSource);
        var text = obj.ToString(TextFormat.Markdown);

        File.WriteAllText($"./{nameof(ToString_Styled_Markdown)}.md", text);
        File.WriteAllText($"./{nameof(ToString_Styled_Markdown)}.markdig.md", Markdig.Markdown.Normalize(StyledSource));

        _out.WriteLine(text);

        Assert.Equal(ExpectStyledMarkdown, text);
    }

    [Fact]
    public void ToString_Styled_UnicodeMono()
    {
        var obj = new MarkDocument(StyledSource);
        var text = obj.ToString(TextFormat.Unicode);

        File.WriteAllText($"./{nameof(ToString_Styled_UnicodeMono)}.txt", text);
        File.WriteAllText($"./{nameof(ToString_Styled_UnicodeMono)}.markdig.txt", Markdig.Markdown.ToPlainText(StyledSource));

        _out.WriteLine(text);

        Assert.Equal(ExpectStyledUnicodeMono, text);
    }

    [Fact]
    public void Update_Styled_DefaultOpts()
    {
        var obj = UpdateWriteOut(StyledSource);

        // **Strong1** *Em1* **Strong2** *Em2* `Inline Code` **Bold** *Italic* <del>Strike</del> <ins>Underline</ins> <samp>Samp</samp> <sup>Sup</sup><sub>Sub</sub> <mark>Mark</mark>***Mixed***
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Strong, obj[0].Elements[0].Styling);
        Assert.Equal("Strong1", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal(" ", obj[0].Elements[1].Text);
        Assert.Equal(InlineStyling.Emphasis, obj[0].Elements[2].Styling);
        Assert.Equal("Em1", obj[0].Elements[2].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[3].Styling);
        Assert.Equal(" ", obj[0].Elements[3].Text);
        Assert.Equal(InlineStyling.Strong, obj[0].Elements[4].Styling);
        Assert.Equal("Strong2", obj[0].Elements[4].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[5].Styling);
        Assert.Equal(" ", obj[0].Elements[5].Text);
        Assert.Equal(InlineStyling.Emphasis, obj[0].Elements[6].Styling);
        Assert.Equal("Em2", obj[0].Elements[6].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[7].Styling);
        Assert.Equal(" ", obj[0].Elements[7].Text);
        Assert.Equal(InlineStyling.Code, obj[0].Elements[8].Styling);
        Assert.Equal("Inline Code", obj[0].Elements[8].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[9].Styling);
        Assert.Equal(" ", obj[0].Elements[9].Text);
        Assert.Equal(InlineStyling.Strong, obj[0].Elements[10].Styling);
        Assert.Equal("Bold", obj[0].Elements[10].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[11].Styling);
        Assert.Equal(" ", obj[0].Elements[11].Text);
        Assert.Equal(InlineStyling.Emphasis, obj[0].Elements[12].Styling);
        Assert.Equal("Italic", obj[0].Elements[12].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[13].Styling);
        Assert.Equal(" ", obj[0].Elements[13].Text);
        Assert.Equal(InlineStyling.Strike, obj[0].Elements[14].Styling);
        Assert.Equal("Strike", obj[0].Elements[14].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[15].Styling);
        Assert.Equal(" ", obj[0].Elements[15].Text);
        Assert.Equal(InlineStyling.Underline, obj[0].Elements[16].Styling);
        Assert.Equal("Underline", obj[0].Elements[16].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[17].Styling);
        Assert.Equal(" ", obj[0].Elements[17].Text);
        Assert.Equal(InlineStyling.Mono, obj[0].Elements[18].Styling);
        Assert.Equal("Samp", obj[0].Elements[18].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[19].Styling);
        Assert.Equal(" ", obj[0].Elements[19].Text);
        Assert.Equal(InlineStyling.Sup, obj[0].Elements[20].Styling);
        Assert.Equal("Sup", obj[0].Elements[20].Text);
        Assert.Equal(InlineStyling.Sub, obj[0].Elements[21].Styling);
        Assert.Equal("Sub", obj[0].Elements[21].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[22].Styling);
        Assert.Equal(" ", obj[0].Elements[22].Text);
        Assert.Equal(InlineStyling.Mark, obj[0].Elements[23].Styling);
        Assert.Equal("Mark", obj[0].Elements[23].Text);
        Assert.Equal(InlineStyling.Emphasis | InlineStyling.Strong, obj[0].Elements[24].Styling);
        Assert.Equal("Mixed", obj[0].Elements[24].Text);
        //
        Assert.Single(obj);
    }

    [Fact]
    public void Update_Styled_IgnoreInline()
    {
        // **Strong1** *Em1* __Strong2__ _Em2_ `Inline Code`
        // <b>Bold</b> <i>Italic</i> <s>Strike</s> <u>Underline</u> <samp>Samp</samp>
        // <small>Small</SMALL> <BIG>Big</big> <sup>Sup</Sup><sub>Sub</sub> <mark>Mark</mark>
        // **<em>Mixed</em>**
        // <Invalid1></Invalid2><Invalid2></Invalid1>";
        var obj = UpdateWriteOut(StyledSource, MarkOptions.IgnoreInline);

        // **Strong1** *Em1* __Strong2__ _Em2_ `Inline Code`\\\n<b>Bold</b> <i>Italic</i> <s>Strike</s> <u>Underline</u> <samp>Samp</samp>\\\n<sup>Sup</Sup><sub>Sub</sub> <mark>Mark</mark>**<em>Mixed</em>**\\\n<Invalid1></Invalid2><Invalid2></Invalid1>
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("**Strong1** *Em1* __Strong2__ _Em2_ `Inline Code`\n<b>Bold</b> <i>Italic</i> <s>Strike</s> <u>Underline</u> <samp>Samp</samp>", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal("\n", obj[0].Elements[1].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[2].Styling);
        Assert.Equal("<sup>Sup</Sup><sub>Sub</sub> <mark>Mark</mark>**<em>Mixed</em>**\n<Invalid1></Invalid2><Invalid2></Invalid1>", obj[0].Elements[2].Text);
        //
        Assert.Single(obj);
    }
}
