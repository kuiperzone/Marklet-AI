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

public class MarkBlockTest
{
    private readonly ITestOutputHelper _out;

    public MarkBlockTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void ToString_Markdown_Body()
    {
        var obj = new MarkBlock(BlockKind.Para);
        obj.Elements.Add(new("Line1\nLine2"));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("Line1\\\nLine2", s);
    }

    [Fact]
    public void ToString_Markdown_Body_WithLink()
    {
        var obj = new MarkBlock(BlockKind.Para);
        var link = new LinkInfo( new("http://example.com"), "Link Title");
        obj.Elements.Add(new("Line1\nLine2", link));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("[Line1\\\nLine2](http://example.com \"Link Title\")", s);
    }

    [Fact]
    public void ToString_Mark_Body_WithLinkAndStyle()
    {
        var obj = new MarkBlock(BlockKind.Para);
        var link = new LinkInfo( new("http://example.com"), "Link Title");
        obj.Elements.Add(new MarkElement("Line1\nLine2", InlineStyling.Strong, link));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("[**Line1\\\nLine2**](http://example.com \"Link Title\")", s);
    }

    [Fact]
    public void ToString_Markdown_OrderedList()
    {
        var obj = new MarkBlock();
        obj.ListLevel = 1;
        obj.ListOrder = 2;
        obj.Elements.Add(new("Item1", InlineStyling.Strong));
        obj.Elements.Add(new(" Item2"));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("2. **Item1** Item2", s);

        obj.Elements.Clear();
        obj.Elements.Add(new("Line1", InlineStyling.Strong));
        obj.Elements.Add(new("\nLine2"));

        s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("2. **Line1**\\\n   Line2", s);
    }

    [Fact]
    public void ToString_Markdown_UnorderedList()
    {
        var obj = new MarkBlock();
        obj.ListLevel = 1;
        obj.ListBullet = '*';
        obj.Elements.Add(new("Line1", InlineStyling.Strong));
        obj.Elements.Add(new("\nLine2"));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("* **Line1**\\\n  Line2", s);
    }

    [Fact]
    public void ToString_Markdown_Quoted()
    {
        var obj = new MarkBlock();
        obj.ListLevel = 1;
        obj.QuoteLevel = 2;
        obj.ListOrder = 1;
        obj.Elements.Add(new("Line1", InlineStyling.Strong));
        obj.Elements.Add(new("\nLine2"));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("> > 1. **Line1**\\\n> >    Line2", s);
    }

    [Fact]
    public void ToString_Markdown_Pre()
    {
        var obj = new MarkBlock(BlockKind.IndentedCode);
        obj.Elements.Add(new("Line1\nLine2"));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("Line1\nLine2", s);
    }

    [Fact]
    public void ToString_Html_Pre()
    {
        var obj = new MarkBlock(BlockKind.IndentedCode);
        obj.Elements.Add(new("Line1\nLine2"));

        var s = obj.ToString(TextFormat.Html);
        _out.WriteLine(s);
        Assert.Equal("<pre><code>Line1\nLine2\n</code></pre>", s);
    }

    [Fact]
    public void ToString_Markdown_CodeFence()
    {
        var obj = new MarkBlock(BlockKind.FencedCode);
        obj.Elements.Add(new("Line1\nLine2"));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("```\nLine1\nLine2\n```", s);
    }

    [Fact]
    public void ToString_Markdown_CodeFence_InvalidStyle()
    {
        var obj = new MarkBlock(BlockKind.FencedCode);
        obj.Elements.Add(new("Line1\nLine2", InlineStyling.Strong));

        var s = obj.ToString(TextFormat.Markdown);
        _out.WriteLine(s);
        Assert.Equal("```\nLine1\nLine2\n```", s);
    }

    [Fact]
    public void ToString_Html_CodeFence()
    {
        var obj = new MarkBlock(BlockKind.FencedCode);
        obj.Elements.Add(new("Line1\nLine2"));

         var s = obj.ToString(TextFormat.Html);
       _out.WriteLine(s);
        Assert.Equal("<pre><code>Line1\nLine2\n</code></pre>", s);
    }

    [Fact]
    public void ToString_Html_CodeFence_InvalidStyle()
    {
        var obj = new MarkBlock(BlockKind.FencedCode);
        obj.Elements.Add(new("Line1\nLine2", InlineStyling.Strong));

        var s = obj.ToString(TextFormat.Html);
        _out.WriteLine(s);
        Assert.Equal("<pre><code>Line1\nLine2\n</code></pre>", s);
    }

}