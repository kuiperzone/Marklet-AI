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
using Avalonia.Data;
using Avalonia.Input;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A subclass of <see cref="PixieControl"/> displaying an internal <see cref="CheckBox"/>.
/// </summary>
public sealed class PixieCheckBox : PixieControl
{
    private readonly CheckBox _checkBox = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieCheckBox()
        : base(false, Avalonia.Layout.VerticalAlignment.Center)
    {
        SetChildControl(_checkBox);
        SetPseudoFocusControl(_checkBox);

        _checkBox.VerticalAlignment = VerticalContentAlignment;
        _checkBox.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _checkBox.IsCheckedChanged += CheckedChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="IsChecked"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<PixieCheckBox, bool>(nameof(IsChecked),
        defaultBindingMode: BindingMode.TwoWay);

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
            _checkBox.IsChecked = !_checkBox.IsChecked;
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
            _checkBox.Content = change.GetNewValue<string?>();
            return;
        }

        if (p == IsCheckedProperty)
        {
            _checkBox.IsChecked = change.GetNewValue<bool>();
            OnValueChanged();
            return;
        }
    }

    private void CheckedChangedHandler(object? _, EventArgs __)
    {
        IsChecked = _checkBox.IsChecked == true;
    }
}
