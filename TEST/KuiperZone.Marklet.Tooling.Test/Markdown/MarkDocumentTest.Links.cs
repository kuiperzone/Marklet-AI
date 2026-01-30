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
    [Fact]
    public void Update_PlainLink_Detects()
    {
        var text = @"https://local.com.";

        var obj = UpdateWriteOut(text);
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("https://local.com", obj[0].Elements[0].Text);
        Assert.False(obj[0].Elements[0].Link?.IsImage);
        Assert.Equal("https://local.com", obj[0].Elements[0].Link?.ToString());
        Assert.Null(obj[0].Elements[0].Link?.Title);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal(".", obj[0].Elements[1].Text);
        //
        Assert.Single(obj);
    }

    [Fact]
    public void Update_PlainLink_IgnoreInline_StillDetects()
    {
        var text = @"https://local.com.";

        var obj = UpdateWriteOut(text, MarkOptions.IgnoreInline);
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("https://local.com", obj[0].Elements[0].Text);
        Assert.False(obj[0].Elements[0].Link?.IsImage);
        Assert.Equal("https://local.com", obj[0].Elements[0].Link?.ToString());
        Assert.Null(obj[0].Elements[0].Link?.Title);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal(".", obj[0].Elements[1].Text);
        //
        Assert.Single(obj);
    }

    [Fact]
    public void Update_PlainLink_IgnorePlainLinks_NoDetection()
    {
        var text = @"https://local.com.";

        var obj = UpdateWriteOut(text, MarkOptions.IgnorePlainLinks);

        // https://local.com.
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("https://local.com.", obj[0].Elements[0].Text);
        //
        Assert.Single(obj);
    }

    [Fact]
    public void Update_AutoLink()
    {
        var text = @"<https://local.com>.";

        var obj = UpdateWriteOut(text);
        // [https://local.com](https://local.com).
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("https://local.com", obj[0].Elements[0].Text);
        Assert.False(obj[0].Elements[0].Link?.IsImage);
        Assert.Equal("https://local.com", obj[0].Elements[0].Link?.ToString());
        Assert.Null(obj[0].Elements[0].Link?.Title);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal(".", obj[0].Elements[1].Text);
        //
        Assert.Single(obj);
    }

    [Fact]
    public void Update_IdentifiesLink()
    {
        var obj = UpdateWriteOut("Start [Link](http://local.com) End");

        // Start [Link](http://local.com) End
        Assert.Equal(BlockKind.Para, obj[0].Kind);
        Assert.Equal(0, obj[0].QuoteLevel);
        Assert.Equal(0, obj[0].ListLevel);
        Assert.Equal(0, obj[0].ListOrder);
        Assert.Equal('\0', obj[0].ListBullet);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[0].Styling);
        Assert.Equal("Start ", obj[0].Elements[0].Text);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[1].Styling);
        Assert.Equal("Link", obj[0].Elements[1].Text);
        Assert.False(obj[0].Elements[1].Link?.IsImage);
        Assert.Equal("http://local.com", obj[0].Elements[1].Link?.ToString());
        Assert.Null(obj[0].Elements[1].Link?.Title);
        Assert.Equal(InlineStyling.Default, obj[0].Elements[2].Styling);
        Assert.Equal(" End", obj[0].Elements[2].Text);
    }

}