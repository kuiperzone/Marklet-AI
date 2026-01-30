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

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Interface for underlay background in <see cref="ChromeWindow.ChromeBar"/> area.
/// </summary>
/// <remarks>
/// Its only purpose is to extend a left-side background area into the <see cref="ChromeWindow.ChromeBar"/> area. This
/// is a style common in some interfaces. The width of the underlay must be maintained using <see cref="BoundsWidth"/>.
/// It is not shown where <see cref="BoundsWidth"/> is 0.
/// </remarks>
public interface IBarUnderlay
{
    /// <summary>
    /// Gets whether the underlaying is visible.
    /// </summary>
    /// <remarks>
    /// The result is false where <see cref="BoundsWidth"/> is 0 or less.
    /// </remarks>
    bool IsVisible { get; }

    /// <summary>
    /// Gets or sets whether underlay resides on the left (or right where false).
    /// </summary>
    /// <remarks>
    /// Default is true.
    /// </remarks>
    bool IsLeftSide { get; set; }

    /// <summary>
    /// Gets or sets the width of the underlay.
    /// </summary>
    /// <remarks>
    /// This must be controlled by the <see cref="ChromeWindow"/> subclass in responds to a resizing event. The <see
    /// cref="IsVisible"/> is false where this is 0 or less.
    /// </remarks>
    double BoundsWidth { get; set; }

    /// <summary>
    /// Gets or sets the background brush.
    /// </summary>
    IBrush? Background { get; set; }

    /// <summary>
    /// Gets or sets the border brush.
    /// </summary>
    /// <remarks>
    /// The border appears on the opposite side to that indicated by <see cref="IsLeftSide"/>.
    /// </remarks>
    IBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the border width, i.e. the left or right side thickness.
    /// </summary>
    /// <remarks>
    /// The border appears on the opposite side to that indicated by <see cref="IsLeftSide"/>.
    /// </remarks>
    double BorderWidth { get; set; }

    /// <summary>
    /// Gets or sets the title of the underlay.
    /// </summary>
    /// <remarks>
    /// This is separate and in addition to any window title. Text should typically be very short otherwise it may
    /// overlap buttons.
    /// </remarks>
    string? Title { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Title"/> font-weight.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="FontWeight.Bold"/>.
    /// </remarks>
    FontWeight TitleWeight { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Title"/> font-weight.
    /// </summary>
    FontStyle TitleStyle { get; set; }
}
