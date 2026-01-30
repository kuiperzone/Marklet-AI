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

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A <see cref="PixieControl{T}"/> composite housing a <see cref="TextEditor"/> instance.
/// </summary>
public sealed class PixieEditor : PixieControl<TextEditor>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieEditor()
    {
        ChildControl.Width = ChromeSizes.OneCh * 28.0;
        ChildControl.MinWidth = ChromeSizes.OneCh * 16.0;
        ChildControl.MaxLines = 1;
        ChildControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        ChildControl.TextChanged += ValueChangedHandler;
    }

    /// <summary>
    /// Constructor which initializes for multiline editor which accepts return.
    /// </summary>
    public PixieEditor(bool multiline)
        : this()
    {
        if (multiline)
        {
            ChildControl.AcceptsReturn = true;
            ChildControl.Height = 210.0;
            ChildControl.Width = double.NaN;
            ChildControl.MaxLines = 0;
            ChildControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        }
    }

}
