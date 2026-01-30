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
using Avalonia.Input;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A subclass of <see cref="PixieControl"/> displaying an internal <see cref="RadioButton"/>.
/// </summary>
/// <remarks>
/// Radio exclusivity is managed by <see cref="PixieGroup"/>.
/// </remarks>
public sealed class PixieRadio : PixieControl
{
    private readonly RadioButton _radio = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieRadio()
        : base(false, Avalonia.Layout.VerticalAlignment.Center)
    {
        SetChildControl(_radio);
        SetPseudoFocusControl(_radio);

        _radio.VerticalAlignment = VerticalContentAlignment;
        _radio.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _radio.IsCheckedChanged += CheckedChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="IsChecked"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<PixieRadio, bool>(nameof(IsChecked));

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public bool IsChecked
    {
        get { return GetValue(IsCheckedProperty); }
        set { SetValue(IsCheckedProperty, value); }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var info = e.GetCurrentPoint(this);

        if (info.Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            _radio.IsChecked = true;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == TitleProperty)
        {
            _radio.Content = change.GetNewValue<string?>();
            return;
        }

        if (p == IsCheckedProperty)
        {
            if (_radio.IsChecked != change.GetNewValue<bool>())
            {
                _radio.IsChecked = change.GetNewValue<bool>();
                Group?.CheckedChanged(this);
                OnValueChanged();
            }

            return;
        }
    }

    private void CheckedChangedHandler(object? _, EventArgs __)
    {
        IsChecked = _radio.IsChecked == true;
        Group?.CheckedChanged(this);
        OnValueChanged();
    }
}
