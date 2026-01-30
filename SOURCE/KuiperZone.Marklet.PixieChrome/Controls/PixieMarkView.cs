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
/// A <see cref="PixieControl{T}"/> composite housing a style aware <see cref="MarkView"/> instance.
/// </summary>
public sealed class PixieMarkView : PixieControl<MarkView>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieMarkView()
        : base(false, Avalonia.Layout.VerticalAlignment.Top)
    {
        ContentPadding = ChromeSizes.LargePadding;
    }
}
