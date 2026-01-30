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

using Avalonia;
using ReactiveUI;

namespace KuiperZone.Marklet.PixieChrome;

// Implementation of size related properties for partial class.
public sealed partial class ChromeStyling
{
    private const double SmallCornerF = 0.50;
    private CornerSize _corners;

    /// <summary>
    /// Gets or sets window and control corner size.
    /// </summary>
    /// <remarks>
    /// Chaning invokes <see cref="StylingChanged"/>.
    /// </remarks>
    public CornerSize Corners
    {
        get { return _corners; }

        set
        {
            if (_corners != value)
            {
                _corners = value;

                this.RaisePropertyChanged(nameof(Corners));
                this.RaisePropertyChanged(nameof(LargeCornerRadius));
                this.RaisePropertyChanged(nameof(LargeCornerTop));
                this.RaisePropertyChanged(nameof(LargeCornerBottom));
                this.RaisePropertyChanged(nameof(SmallCornerRadius));
                this.RaisePropertyChanged(nameof(SmallCornerTop));
                this.RaisePropertyChanged(nameof(SmallCornerBottom));

                OnStylingChanged();
            }
        }
    }

    /// <summary>
    /// Gets the corner pixel radius for controls considered a larger "group".
    /// </summary>
    public CornerRadius LargeCornerRadius
    {
        get { return new(_corners.ToPixels()); }
    }

    /// <summary>
    /// Gets <see cref="LargeCornerRadius"/> for top only.
    /// </summary>
    public CornerRadius LargeCornerTop
    {
        get
        {
            var px = _corners.ToPixels();
            return new(px, px, 0, 0);
        }
    }

    /// <summary>
    /// Gets <see cref="LargeCornerRadius"/> for bottom only.
    /// </summary>
    public CornerRadius LargeCornerBottom
    {
        get
        {
            var px = _corners.ToPixels();
            return new(0, 0, px, px);
        }
    }

    /// <summary>
    /// Gets the corner pixel radius for controls considered smaller individual items.
    /// </summary>
    public CornerRadius SmallCornerRadius
    {
        get { return new(_corners.ToPixels() * SmallCornerF); }
    }

    /// <summary>
    /// Gets <see cref="SmallCornerRadius"/> for top only.
    /// </summary>
    public CornerRadius SmallCornerTop
    {
        get
        {
            var px = _corners.ToPixels() * SmallCornerF;
            return new(px, px, 0, 0);
        }
    }

    /// <summary>
    /// Gets <see cref="SmallCornerRadius"/> for bottom only.
    /// </summary>
    public CornerRadius SmallCornerBottom
    {
        get
        {
            var px = _corners.ToPixels() * SmallCornerF;
            return new(0, 0, px, px);
        }
    }

}
