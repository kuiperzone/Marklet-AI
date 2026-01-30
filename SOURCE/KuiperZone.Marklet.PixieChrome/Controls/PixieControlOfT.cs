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
using Avalonia.Layout;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A <see cref="PixieControl"/> composite housing a generic control of type T.
/// </summary>
public class PixieControl<T> : PixieControl
    where T : Control, new()
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieControl()
        : this(true, VerticalAlignment.Center)
    {
    }

    /// <summary>
    /// Subclass constructor.
    /// </summary>
    protected PixieControl(bool isTitleVisible, VerticalAlignment verticalAlignment)
        : base(isTitleVisible, verticalAlignment)
    {
        SetChildControl(ChildControl);
    }

    /// <summary>
    /// Gets the control item of generic type T.
    /// </summary>
    public T ChildControl { get; } = new();

    /// <summary>
    /// Overridden by subclass to perform initialization of "value" and return "value".
    /// </summary>
    protected virtual T InitControlItemOfT(T value)
    {
        return value;
    }
}
