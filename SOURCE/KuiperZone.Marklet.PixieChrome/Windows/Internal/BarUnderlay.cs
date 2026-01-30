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
using Avalonia.Media;

namespace KuiperZone.Marklet.PixieChrome.Windows.Internal;

/// <summary>
/// Implements <see cref="IBarUnderlay"/>.
/// </summary>
internal sealed class BarUnderlay : Border, IBarUnderlay
{
    private TextBlock? _title;
    private FontWeight _titleWeight = FontWeight.Bold;
    private FontStyle _titleStyle;
    private bool _isLeftSide = true;
    private double _boundsWidth;
    private double _borderWidth;

    public BarUnderlay()
    {
        IsVisible = false;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
    }

    /// <summary>
    /// Implements <see cref="IBarUnderlay.IsLeftSide"/>.
    /// </summary>
    public bool IsLeftSide
    {
        get { return _isLeftSide; }

        set
        {
            if (_isLeftSide != value)
            {
                var t = _borderWidth;
                BorderWidth = 0.0;

                _isLeftSide = value;
                HorizontalAlignment = value ? Avalonia.Layout.HorizontalAlignment.Left : Avalonia.Layout.HorizontalAlignment.Right;
                BorderWidth = t;
            }
        }
    }

    /// <summary>
    /// Implements <see cref="IBarUnderlay.BoundsWidth"/>.
    /// </summary>
    public double BoundsWidth
    {
        get { return _boundsWidth; }

        set
        {
            if (_boundsWidth != value)
            {
                Width = value;
                _boundsWidth = value;
                IsVisible = value > 0.0 || double.IsNaN(value);
            }
        }
    }

    /// <summary>
    /// Implements <see cref="IBarUnderlay.BorderWidth"/>.
    /// </summary>
    public double BorderWidth
    {
        get { return _borderWidth; }

        set
        {
            if (_borderWidth != value)
            {
                _borderWidth = value;
                BorderThickness = _isLeftSide ? new(0.0, 0.0, value, 0.0) : new(value, 0.0, 0.0, 0.0);

            }
        }
    }

    /// <summary>
    /// Implements <see cref="IBarUnderlay.Title"/>.
    /// </summary>
    public string? Title
    {
        get { return _title?.Text; }
        set { GetTitleBlock().Text = value; }
    }

    /// <summary>
    /// Implements <see cref="IBarUnderlay.TitleWeight"/>.
    /// </summary>
    public FontWeight TitleWeight
    {
        get { return _titleWeight; }

        set
        {
            if (_titleWeight != value)
            {
                _titleWeight = value;

                if (_title != null)
                {
                    _title.FontWeight = value;
                }
            }
        }
    }

    /// <summary>
    /// Implements <see cref="IBarUnderlay.TitleStyle"/>.
    /// </summary>
    public FontStyle TitleStyle
    {
        get { return _titleStyle; }

        set
        {
            if (_titleStyle != value)
            {
                _titleStyle = value;

                if (_title != null)
                {
                    _title.FontStyle = value;
                }
            }
        }
    }

    private TextBlock GetTitleBlock()
    {
        if (_title == null)
        {
            _title = new();
            _title.Background = Brushes.Transparent;
            _title.TextAlignment = TextAlignment.Center;
            _title.FontWeight = _titleWeight;
            _title.FontStyle = _titleStyle;
            _title.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            _title.TextWrapping = TextWrapping.NoWrap;
            _title.TextTrimming = TextTrimming.CharacterEllipsis;
            Child = _title;
        }

        return _title;
    }
}
