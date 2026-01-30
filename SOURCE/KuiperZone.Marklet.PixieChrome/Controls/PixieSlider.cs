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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A <see cref="PixieControl{T}"/> composite housing a <see cref="Slider"/> instance.
/// </summary>
public sealed class PixieSlider : PixieControl<Slider>
{
    private string? _footer;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieSlider()
        : base(true, Avalonia.Layout.VerticalAlignment.Center)
    {
        SetPseudoFocusControl(ChildControl);
        ChildControl.BorderThickness = default;
        ChildControl.BorderBrush = Brushes.Transparent;
        ChildControl.ValueChanged += SliderChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="Format"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<PixieSlider, string?>(nameof(FormatProperty));

    /// <summary>
    /// Gets or sets the numeric format string.
    /// </summary>
    /// <remarks>
    /// The value is used with <see cref="Footer"/> variables. Default is null.
    /// </remarks>
    public string? Format
    {
        get { return GetValue(FormatProperty); }
        set { SetValue(FormatProperty, value); }
    }

    /// <summary>
    /// Overrides and supports variables "{VALUE}", "{MIN}" and "{MAX}".
    /// </summary>
    public override string? Footer
    {
        get { return _footer; }

        set
        {
            if (_footer != value)
            {
                _footer = value;
                SetFooterVariables(Format);
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == FormatProperty)
        {
            SetFooterVariables(change.GetNewValue<string?>());
            return;
        }
    }

    private void SetFooterVariables(string? format)
    {
        if (!string.IsNullOrEmpty(_footer))
        {
            var v0 = GetVariable(ChildControl.Value, format);
            var min = GetVariable(ChildControl.Minimum, format);
            var max = GetVariable(ChildControl.Maximum, format);
            base.Footer = _footer?.Replace("{VALUE}", v0).Replace("{MIN}", min).Replace("{MAX}", max);
            return;
        }

        base.Footer = _footer;
    }

    private string GetVariable(double value, string? format)
    {
        if (string.IsNullOrEmpty(format))
        {
            if (ChildControl?.SmallChange < 1.0)
            {
                format = "#,##0.###";
            }
            else
            if (ChildControl?.SmallChange < 10.0)
            {
                format = "#,##0.##";
            }
            else
            if (ChildControl?.SmallChange < 100.0)
            {
                format = "#,##0.#";
            }
            else
            {
                format = "#,##0";
            }
        }

        return value.ToString(format);
    }


    private void SliderChangedHandler(object? _, RangeBaseValueChangedEventArgs e)
    {
        SetFooterVariables(Format);
        OnValueChanged();
    }
}
