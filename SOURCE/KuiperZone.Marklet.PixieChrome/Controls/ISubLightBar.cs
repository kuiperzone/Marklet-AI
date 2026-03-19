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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Provides a public facing interface to a sub-set of <see cref="LightBar"/> properties.
/// </summary>
public interface ISubLightBar
{
    /// <summary>
    /// Gets the sequence of buttons.
    /// </summary>
    IReadOnlyList<LightButton> Buttons { get; }

    /// <summary>
    /// Removes all buttons.
    /// </summary>
    void Clear();

    /// <summary>
    /// Adds a button and returns the the new button.
    /// </summary>
    LightButton AddButton(string? content, string? tip = null);

    /// <summary>
    /// Overload providing a click handler.
    /// </summary>
    LightButton AddButton(string content, EventHandler<RoutedEventArgs>? handler, string? tip = null);

    /// <summary>
    /// Overload providing a <see cref="ContextMenu"/> instead of a handler (menu opened when the button is clicked).
    /// </summary>
    LightButton AddButton(string content, ContextMenu? menu, string? tip = null);

    /// <summary>
    /// Removes the "button" and returns true on success.
    /// </summary>
    bool Remove(Control button);

    /// <summary>
    /// Calls <see cref="LightButton.OnClick"/> on the first button with a <see cref="LightButton.Gesture"/>
    /// the returns true for the given key input and returns true on success.
    /// </summary>
    /// <remarks>
    /// No event occurs where <see cref="RoutedEventArgs.Handled"/> is already true, and the result is false. On
    /// success, the <see cref="RoutedEventArgs.Handled"/> value is set to true on return.
    /// </remarks>
    bool HandleKeyGesture(KeyEventArgs e);
}
