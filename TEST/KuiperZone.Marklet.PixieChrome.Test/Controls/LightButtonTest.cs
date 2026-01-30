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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class LightButtonTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        var obj = new LightButton();

        AssertDirect(obj, LightButton.ContentProperty, null, "Hello");
        AssertDirect(obj, LightButton.ContentAlignmentProperty, TextAlignment.Center, TextAlignment.Left);
        AssertDirect(obj, LightButton.FontWeightProperty, FontWeight.Normal, FontWeight.Bold);
        AssertDirect(obj, LightButton.IsRepeatableProperty, false, true);
        AssertDirect(obj, LightButton.IsCheckedProperty, false, true);
        AssertDirect(obj, LightButton.CanToggleProperty, false, true);
        AssertDirect(obj, LightButton.DropMenuProperty, null, new ContextMenu());
        AssertDirect(obj, LightButton.FontSizeProperty, double.NaN, 5.0);
        AssertDirect(obj, LightButton.FontScaleProperty, 1.0, 2.0);
        AssertDirect(obj, LightButton.GestureProperty, null, new KeyGesture(Key.B));

        // Cannot compare equality on NaN
        // AssertDirect(obj, LightButton.ContentPaddingProperty, new Thickness(double.NaN), new Thickness(5.0));
    }

    [Fact]
    public void StyledProperties_ChangeAndClear()
    {
        var obj = new LightButton();

        AssertStyled(obj, LightButton.ForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertStyled(obj, LightButton.HoverBackgroundProperty, ChromeStyling.ButtonHover, ChromeBrushes.RedAccent);
        AssertStyled(obj, LightButton.CheckedPressedBackgroundProperty, ChromeStyling.ButtonCheckedPressed, ChromeBrushes.RedAccent);
        AssertStyled(obj, LightButton.CheckedPressedForegroundProperty, null, ChromeBrushes.RedAccent);
        AssertStyled(obj, LightButton.DisabledForegroundProperty, ChromeStyling.ForegroundGray, ChromeBrushes.RedAccent);
        AssertStyled(obj, LightButton.FocusBorderBrushProperty, null, ChromeBrushes.RedAccent);
    }

}
