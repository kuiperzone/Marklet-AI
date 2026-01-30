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

using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.Tooling.Markdown;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class MarkRuleHostTest
{
    const int RuleIndex = 0;
    const int ParaIndex = 1;
    const int HeadIndex = 2;
    const int CodeIndex = 3;
    const int IndentIndex = 4;
    const int RejectIndex = 5;

    private readonly ITestOutputHelper _out;

    public MarkRuleHostTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void MarkRuleHost_AcceptsAndRejects()
    {
        var obj = Assert_New_ConsumeUpdates(RuleIndex, typeof(MarkRuleHost));

        // Regression
        // This will be brittle to minor changes but will spot unintended ones.
        _out.WriteLine(obj.ToString());

        var exp = "Control=Rectangle, QuoteLevel=0, ListLevel=0, Kind=Rule, Control.Margin=0,0,0,0, Rule.Fill=Gray, Rule.Height=1.5";

        Assert.Equal(exp, obj.ToString());
    }

    [Fact]
    public void MarkTextHost_Para_AcceptsAndRejects()
    {
        var obj = Assert_New_ConsumeUpdates(ParaIndex, typeof(MarkTextHost));

        // Regression
        // This will be brittle to minor changes but will spot unintended ones.
        _out.WriteLine(obj.ToString());

        var exp = "Control=CrossTextBlock, QuoteLevel=0, ListLevel=0, Kind=Para, Control.Margin=0,7,0,7, CrossText.Background=Transparent, " +
        "CrossText.TextWrapping=Wrap, CrossText.VerticalAlignment=Top, CrossText.FontFamily=$Default, CrossText.FontSize=14, " +
        "CrossText.FontWeight=Normal, CrossText.LineHeight=25.2, CrossText.LetterSpacing=1.3, CrossText.Foreground=Black, CrossText.Text=change";

        Assert.Equal(exp, obj.ToString());
    }

    [Fact]
    public void MarkTextHost_Heading_AcceptsAndRejects()
    {
        var obj = Assert_New_ConsumeUpdates(HeadIndex, typeof(MarkTextHost));

        // Regression
        _out.WriteLine(obj.ToString());

        var exp = "Control=CrossTextBlock, QuoteLevel=0, ListLevel=0, Kind=H1, Control.Margin=0,30.099999999999998,0,7, CrossText.Background=Transparent, " +
        "CrossText.TextWrapping=Wrap, CrossText.VerticalAlignment=Top, CrossText.FontFamily=$Default, CrossText.FontSize=23.099999999999998, "+
        "CrossText.FontWeight=Bold, CrossText.LineHeight=NaN, CrossText.LetterSpacing=2.145, CrossText.Foreground=Black, CrossText.Text=change";

        Assert.Equal(exp, obj.ToString());
    }

    [Fact]
    public void MarkCodeHost_Fenced_AcceptsAndRejects()
    {
        var obj = Assert_New_ConsumeUpdates(CodeIndex, typeof(MarkCodeHost));

        // Regression
        _out.WriteLine(obj.ToString());

        var exp = "Control=InnerBorder, QuoteLevel=0, ListLevel=0, Kind=FencedCode, Control.Margin=0,42,0,42, CrossText.Background=Transparent, " +
        "CrossText.TextWrapping=NoWrap, CrossText.VerticalAlignment=Top, CrossText.FontFamily=monospace, CrossText.FontSize=14, CrossText.FontWeight=Normal, " +
        "CrossText.LineHeight=25.2, CrossText.LetterSpacing=0, CrossText.Foreground=Black, CrossText.Text=change, Scroller.Focusable=False, " +
        "Scroller.VerticalAlignment=Top, Scroller.HorizontalAlignment=Stretch, Scroller.VerticalContentAlignment=Top, Scroller.HorizontalContentAlignment=Left, " +
        "Lang.Text=lang, Lang.FontSize=14, Lang.FontFamily=$Default, Border.MinWidth=112, Border.BorderBrush=Gray, Border.Background=#40808080";

        Assert.Equal(exp, obj.ToString());
    }

    [Fact]
    public void MarkCodeHost_Indented_AcceptsAndRejects()
    {
        var obj = Assert_New_ConsumeUpdates(IndentIndex, typeof(MarkCodeHost));

        // Regression
        _out.WriteLine(obj.ToString());

        var exp = "Control=StackPanel, QuoteLevel=0, ListLevel=0, Kind=IndentedCode, Control.Margin=28,14,0,0, CrossText.Background=Transparent, " +
        "CrossText.TextWrapping=NoWrap, CrossText.VerticalAlignment=Top, CrossText.FontFamily=monospace, CrossText.FontSize=14, CrossText.FontWeight=Normal, " +
        "CrossText.LineHeight=25.2, CrossText.LetterSpacing=0, CrossText.Foreground=Black, CrossText.Text=change, Scroller.Focusable=False, Scroller.VerticalAlignment=Top, " +
        "Scroller.HorizontalAlignment=Stretch, Scroller.VerticalContentAlignment=Top, Scroller.HorizontalContentAlignment=Left, Lang.Text=NULL, Lang.FontSize=NULL, " +
        "Lang.FontFamily=NULL, Border.MinWidth=NULL, Border.BorderBrush=NULL, Border.Background=NULL";

        Assert.Equal(exp, obj.ToString());
    }

    private MarkBlockHost Assert_New_ConsumeUpdates(int blockIndex, Type expectType)
    {
        var doc = NewDoc();

        int index = blockIndex;
        var obj = MarkBlockHost.New(new(), doc, ref index);
        Assert.IsType(expectType, obj);

        // Again
        index = blockIndex;
        Assert.Equal(MarkConsumed.NoChange, obj.ConsumeUpdates(doc, ref index));
        Assert.Equal(blockIndex + 1, index);

        // Rejects
        index = RejectIndex;
        Assert.Equal(MarkConsumed.Incompatible, obj.ConsumeUpdates(doc, ref index));
        Assert.Equal(RejectIndex, index);

        if (blockIndex != RuleIndex)
        {
            // Change value
            // Not test for Rule
            doc = NewDoc("change");

            index = blockIndex;
            Assert.Equal(MarkConsumed.Changed, obj.ConsumeUpdates(doc, ref index));
            Assert.Equal(blockIndex + 1, index);
        }

        return obj;
    }

    private MarkDocument NewDoc(string value = "value")
    {
        // Block 0: rule
        // Block 1: para
        // Block 2: Heading
        // Block 3: Code
        // Block 4: Indent
        // Block 5 : Reject (H6)
        return new MarkDocument($"***\n{value}\n# {value}\n```lang\n{value}\n```\n    {value}\n###### EOL");
    }
}
