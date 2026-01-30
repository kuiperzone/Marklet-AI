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

using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class PixieControlTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        var obj = new PixieControl();
        AssertDirect(obj, PixieControl.HeaderProperty, null, "Text");
        AssertDirect(obj, PixieControl.TitleProperty, null, "Text");
        AssertDirect(obj, PixieControl.TitleWeightProperty, FontWeight.Normal, FontWeight.Thin);
        AssertDirect(obj, PixieControl.FooterProperty, null, "Text");
        AssertDirect(obj, PixieControl.FooterAlignmentProperty, TextAlignment.Left, TextAlignment.Center);
        AssertDirect(obj, PixieControl.LeftSymbolProperty, null, "Text");
        AssertDirect(obj, PixieControl.RightSymbolProperty, null, "Text");
    }

    [Fact]
    public void Attention_DoesNotThrow()
    {
        var obj = new PixieControl();
        obj.Attention();
    }
}
