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

public class MarkElementTest
{
    [Fact]
    public void SplitLines()
    {
        var obj = new MarkElement("");
        var split = obj.SplitLines();
        Assert.Single(split);

        obj = new MarkElement("abc\n 012");
        split = obj.SplitLines(InlineStyling.Strong);
        Assert.Equal(2, split.Length);

        Assert.Equal("abc", split[0].Text);
        Assert.Equal(InlineStyling.Strong, split[0].Styling);

        Assert.Equal(" 012", split[1].Text);
        Assert.Equal(InlineStyling.Strong, split[1].Styling);
    }

    [Fact]
    public void ToString_Mark_NoStyles()
    {
        var obj = new MarkElement("Line1\nLine2 <> \"text\" & 'text'");
        var s = obj.ToString(TextFormat.Markdown);
        ConditionalDebug.WriteLine(s);
        Assert.Equal("Line1\\\nLine2 <> \"text\" & 'text'", s);
    }

    [Fact]
    public void ToString_Mark_Styles()
    {
        InlineStyling style = (InlineStyling)0xFFFF;

        var obj = new MarkElement("Line1\nLine2 <> \"text\" & 'text'", style);
        var s = obj.ToString(TextFormat.Markdown);
        ConditionalDebug.WriteLine(obj.Text);
        ConditionalDebug.WriteLine("Parsed:");
        ConditionalDebug.WriteLine(s);
        Assert.Equal("***`<samp><ins><del><sub><sup><mark>$Line1\\\nLine2 <> \"text\" & 'text'$</mark></sup></sub></del></ins></samp>`***", s);
    }

    [Fact]
    public void ToString_Mark_Styles_Link()
    {
        InlineStyling style = (InlineStyling)0xFFFF;

        var link = new LinkInfo("http://local.com", "title \"&amp;\" ");
        var obj = new MarkElement("Line1\nLine2 <> \"text\" & 'text'", style, link);
        var s = obj.ToString(TextFormat.Markdown);
        ConditionalDebug.WriteLine(obj.Text);
        ConditionalDebug.WriteLine(s);
        Assert.Equal("[***`<samp><ins><del><sub><sup><mark>$Line1\\\nLine2 <> \"text\" & 'text'$</mark></sup></sub></del></ins></samp>`***](http://local.com \"title \"&amp;\" \")", s);
    }

    [Fact]
    public void ToString_Html_NoStyles()
    {
        var obj = new MarkElement("Line1\nLine2 <> \"text\" & 'text'");
        var s = obj.ToString(TextFormat.Html);
        ConditionalDebug.WriteLine(obj.Text);
        ConditionalDebug.WriteLine(s);
        Assert.Equal("Line1<br />Line2 &lt;&gt; &quot;text&quot; &amp; 'text'", s);
    }

    [Fact]
    public void ToString_Html_Styles()
    {
        InlineStyling style = (InlineStyling)0xFFFF;

        var obj = new MarkElement("Line1\nLine2 <> \"text\" & 'text'", style);
        var s = obj.ToString(TextFormat.Html);
        ConditionalDebug.WriteLine(s);

        // We have two "samp" because of Math styling
        Assert.Equal("<em><strong><code><samp><ins><del><sub><sup><mark><samp>Line1<br />Line2 &lt;&gt; &quot;text&quot; &amp; 'text'</samp></mark></sup></sub></del></ins></samp></code></strong></em>", s);
    }

    [Fact]
    public void ToString_Html_Styles_Link()
    {
        InlineStyling style = (InlineStyling)0xFFFF;

        var link = new LinkInfo("http://local.com", "title \"&amp;\" ");
        var obj = new MarkElement("Line1\nLine2 <> \"text\" & 'text'", style, link);
        var s = obj.ToString(TextFormat.Html);
        ConditionalDebug.WriteLine(s);

        // We have two "samp" because of Math styling
        Assert.Equal("<em><strong><code><samp><ins><del><sub><sup><mark><samp><a href=\"http://local.com\" title=\"title &quot;&amp;amp;&quot; \">Line1<br />Line2 &lt;&gt; &quot;text&quot; &amp; 'text'</a></samp></mark></sup></sub></del></ins></samp></code></strong></em>", s);
    }

}