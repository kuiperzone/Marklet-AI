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

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A <see cref="PixieControl{T}"/> composite housing a <see cref="TextEditor"/> instance.
/// </summary>
public class PixieEditor : PixieControl<TextEditor>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieEditor()
    {
        Subject.Width = ChromeSizes.OneCh * 28.0;
        Subject.MinWidth = ChromeSizes.OneCh * 16.0;
        Subject.Margin = new(0.0, ChromeSizes.SmallPx);
        Subject.MaxLines = 1;
        Subject.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        Subject.TextChanged += ValueChangedHandler;
    }

    /// <summary>
    /// Constructor which initializes for multiline editor which accepts return.
    /// </summary>
    public PixieEditor(bool multiline)
        : this()
    {
        if (multiline)
        {
            Subject.AcceptsReturn = true;
            Subject.Height = 210.0;
            Subject.Width = double.NaN;
            Subject.MaxLines = 0;
            Subject.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        }
    }

}
