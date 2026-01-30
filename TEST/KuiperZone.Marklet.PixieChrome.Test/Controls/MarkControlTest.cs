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

using Avalonia;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class MarkControlTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        var obj = new MarkControl();

        // Initially null
        Assert.Null(obj.ContextMenu);

        AssertDirect(obj, MarkControl.ForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FontFamilyProperty, ChromeFonts.DefaultFamily, new("none"));
        AssertDirect(obj, MarkControl.FontSizeProperty, MarkControl.DefaultFontSize, 45);
        AssertDirect(obj, MarkControl.FontSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.SelectionBrushProperty, MarkControl.DefaultSelectionBrush, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.LinkForegroundProperty, MarkControl.DefaultLinkForeground, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.LinkHoverBrushProperty, MarkControl.DefaultLinkHover, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.LinkHoverUnderlineProperty, true, false);
        AssertDirect(obj, MarkControl.HeadingForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.HeadingFamilyProperty, ChromeFonts.DefaultFamily, new("none"));
        AssertDirect(obj, MarkControl.HeadingSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.MonoFamilyProperty, new FontFamily("monospace"), new("none"));
        AssertDirect(obj, MarkControl.MonoSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.QuoteFamilyProperty, null, ChromeFonts.DefaultFamily);
        AssertDirect(obj, MarkControl.QuoteSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.QuoteItalicProperty, false, true);
        AssertDirect(obj, MarkControl.QuoteDecorProperty, MarkControl.DefaultQuoteDecor, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.RuleLineProperty, MarkControl.NeutralForegroundBrush, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedBackgroundProperty, MarkControl.DefaultFencedBackground, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedBorderProperty, MarkControl.NeutralForegroundBrush, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedCornerRadiusProperty, default, new CornerRadius(87));
        AssertDirect(obj, MarkControl.DefaultWrappingProperty, false, true);
    }

}
