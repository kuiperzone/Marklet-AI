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

using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class TextEditorTest : ControlTestBase
{
    [Fact]
    public void DirectProperties_ChangeCorrectly()
    {
        // None
        // Placeholder
    }

    [Fact]
    public void StyledProperties_ChangeAndClear()
    {
        var obj = new TextEditor();

        AssertStyled(obj, TextEditor.HasBackButtonProperty, true, false);
        AssertStyled(obj, TextEditor.HasCopyButtonProperty, false, true);
        AssertStyled(obj, TextEditor.HasRevealButtonProperty, false, true);
        AssertStyled(obj, TextEditor.AlwaysAcceptReturnOnPasteProperty, false, true);
    }
}
