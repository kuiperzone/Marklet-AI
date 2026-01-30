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

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Interface for adding controls to <see cref="ChromeWindow"/>.
/// </summary>
public interface IChromeBar
{
    /// <summary>
    /// Gets whether the chromebar is visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets whether both <see cref="LeftGroup"/> and <see cref="RightGroup"/> have zero buttons.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets the current height in DIPs.
    /// </summary>
    double BarHeight { get; }

    /// <summary>
    /// Gets the interface for buttons on the left-side.
    /// </summary>
    /// <remarks>
    /// The window will call <see cref="ISubLightBar.HandleKeyGesture"/> on these buttons in response to key presses.
    /// </remarks>
    ISubLightBar LeftGroup { get; }

    /// <summary>
    /// Gets the interface for buttons on the right-side.
    /// </summary>
    /// <remarks>
    /// The window will call <see cref="ISubLightBar.HandleKeyGesture"/> on these buttons in response to key presses.
    /// </remarks>
    ISubLightBar RightGroup { get; }

    /// <summary>
    /// Gets or sets the title string.
    /// </summary>
    /// <remarks>
    /// If <see cref="Title"/> is null, the title shown will follow the Window.Title value. Where not null, it overrides
    /// the Window.Title. It may, therefore, be set to an empty string to display no title. The default is null.
    /// </remarks>
    string? Title { get; set; }
}
