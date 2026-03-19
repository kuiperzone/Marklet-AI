// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class MarkControlTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        var obj = new MarkTestControl();

        // Initially null
        Assert.Null(obj.ContextMenu);

        AssertDirect(obj, MarkControl.ForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FontFamilyProperty, ChromeFonts.DefaultFamily, new("none"));
        AssertDirect(obj, MarkControl.FontSizeProperty, 14.0, 45.0);
        AssertDirect(obj, MarkControl.FontSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.SelectionBrushProperty, CrossTextBlock.DefaultSelectionBrush, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.LinkForegroundProperty, CrossTextBlock.DefaultLinkBrush, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.LinkHoverBrushProperty, CrossTextBlock.DefaultHoverLinkHoverBrush, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.LinkHoverUnderlineProperty, true, false);
        AssertDirect(obj, MarkControl.HeadingForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.HeadingFamilyProperty, ChromeFonts.DefaultFamily, new("none"));
        AssertDirect(obj, MarkControl.HeadingSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.MonoFamilyProperty, ChromeFonts.MonospaceFamily, new("none"));
        AssertDirect(obj, MarkControl.MonoSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.QuoteFamilyProperty, null, ChromeFonts.DefaultFamily);
        AssertDirect(obj, MarkControl.QuoteSizeCorrectionProperty, 1.0, 12.0);
        AssertDirect(obj, MarkControl.QuoteItalicProperty, false, true);
        AssertDirect(obj, MarkControl.QuoteDecorProperty, new(0xFF5FAF5F), ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.RuleLineProperty, new(0xFF808080), ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedBackgroundProperty, new(0x40808080), ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedBorderProperty, new(0xFF808080), ChromeBrushes.RedAccent);
        AssertDirect(obj, MarkControl.FencedCornerRadiusProperty, default, new CornerRadius(87));
        AssertDirect(obj, MarkControl.DefaultWrappingProperty, false, true);
    }

    private class MarkTestControl : MarkControl
    {
        protected override void ImmediateRefresh()
        {
        }
    }

}
