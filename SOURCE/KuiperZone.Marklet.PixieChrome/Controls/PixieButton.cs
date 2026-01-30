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

using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Immutable;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A subclass of <see cref="PixieControl{T}"/> which has a checkable background, serving as toggle control itself, with
/// an additional right-aligned <see cref="PixieControl{T}.ChildControl"/> button.
/// </summary>
/// <remarks>
/// The <see cref="IsBackgroundChecked"/> is exclusive inside an instance of <see cref="PixieGroup"/>. The <see
/// cref="PixieControl{T}.ChildControl"/> provides an additional button which appears to the left of <see
/// cref="PixieControl.RightButton"/>. By default, the <see cref="LightButton"/> IsVisible property is false and the
/// button is initialized to show a default "menu" symbol when visible.
/// </remarks>
public class PixieButton : PixieControl
{
    // Backing fields
    private ICommand? _backgroundCommand;
    private object? _backgroundCommandParameter;
    private bool _isBackgroundChecked;
    private bool _canToggleBackground;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieButton()
    {
        SetPseudoFocusControl(this);
    }

    /// <summary>
    /// Defines the <see cref="BackgroundClick"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> BackgroundClickEvent =
        RoutedEvent.Register<PixieButton, RoutedEventArgs>(nameof(BackgroundClick), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="BackgroundCommand"/> event.
    /// </summary>
    public static readonly DirectProperty<PixieButton, ICommand?> BackgroundCommandProperty =
        AvaloniaProperty.RegisterDirect<PixieButton, ICommand?>(nameof(BackgroundCommand),
        o => o.BackgroundCommand, (o, v) => o.BackgroundCommand = v);

    /// <summary>
    /// Defines the <see cref="BackgroundCommandParameter"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieButton, object?> BackgroundCommandParameterProperty =
        AvaloniaProperty.RegisterDirect<PixieButton, object?>(nameof(BackgroundCommandParameter),
        o => o.BackgroundCommandParameter, (o, v) => o.BackgroundCommandParameter = v);

    /// <summary>
    /// Defines the <see cref="IsBackgroundChecked"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieButton, bool> IsBackgroundCheckedProperty =
        AvaloniaProperty.RegisterDirect<PixieButton, bool>(nameof(IsBackgroundChecked),
        o => o.IsBackgroundChecked, (o, v) => o.IsBackgroundChecked = v);

    /// <summary>
    /// Defines the <see cref="CanToggleBackground"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieButton, bool> CanToggleBackgroundProperty =
        AvaloniaProperty.RegisterDirect<PixieButton, bool>(nameof(CanToggleBackground),
        o => o.CanToggleBackground, (o, v) => o.CanToggleBackground = v);

    /// <summary>
    /// Occurs when background area clicked.
    /// </summary>
    /// <remarks>
    /// This is distinct from the child button. The sender is "this" instance.
    /// </remarks>
    public event EventHandler<RoutedEventArgs> BackgroundClick
    {
        add { AddHandler(BackgroundClickEvent, value); }
        remove { RemoveHandler(BackgroundClickEvent, value); }
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the background is clicked.
    /// </summary>
    /// <remarks>
    /// This is distinct from the child button.
    /// </remarks>
    public ICommand? BackgroundCommand
    {
        get { return _backgroundCommand; }
        set { SetAndRaise(BackgroundCommandProperty, ref _backgroundCommand, value); }
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="BackgroundCommand"/>.
    /// </summary>
    public object? BackgroundCommandParameter
    {
        get { return _backgroundCommandParameter; }
        set { SetAndRaise(BackgroundCommandParameterProperty, ref _backgroundCommandParameter, value); }
    }

    /// <summary>
    /// Gets whether the control background is checked (does not apply to child button).
    /// </summary>
    /// <remarks>
    /// When inside <see cref="PixieGroup"/> container, this acts like a radio button with exclusive state. This is
    /// distinct from the child button.
    /// </remarks>
    public bool IsBackgroundChecked
    {
        get { return _isBackgroundChecked; }
        set { SetAndRaise(IsBackgroundCheckedProperty, ref _isBackgroundChecked, value); }
    }

    /// <summary>
    /// Gets whether <see cref="IsBackgroundChecked"/> can be toggled by the pointer or keyboard (does not apply to
    /// child button).
    /// </summary>
    /// <remarks>
    /// When false, the <see cref="IsBackgroundChecked"/> can still be set programmatically. This is distinct from the
    /// child button. The default is false.
    /// </remarks>
    public bool CanToggleBackground
    {
        get { return _canToggleBackground; }
        set { SetAndRaise(CanToggleBackgroundProperty, ref _canToggleBackground, value); }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override ImmutableSolidColorBrush HoverBrush
    {
        get { return IsBackgroundChecked ? CheckedBrush : base.HoverBrush; }
    }

    /// <summary>
    /// Gets checked brush.
    /// </summary>
    protected ImmutableSolidColorBrush CheckedBrush
    {
        get { return Styling.SemiAccent; }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    internal override void ClearFocusBackground()
    {
        if (!IsBackgroundChecked)
        {
            base.ClearFocusBackground();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Styling.StylingChanged += StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == IsBackgroundCheckedProperty)
        {
            AdornerBackground = change.GetNewValue<bool>() ? CheckedBrush : null;
            Group?.CheckedChanged(this);
            OnValueChanged();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        AdornerBackground = IsBackgroundChecked ? CheckedBrush : null;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var info = e.GetCurrentPoint(this);

        if (info.Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            OnBackgroundClick();
            return;
        }

        base.OnPointerPressed(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsFocused && e.KeyModifiers == KeyModifiers.None &&
            (e.PhysicalKey == PhysicalKey.Enter ||
            e.PhysicalKey == PhysicalKey.Space))
        {
            e.Handled = true;
            OnBackgroundClick();
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnBackgroundClick()
    {
        var command = BackgroundCommand;
        var parameter = BackgroundCommandParameter;

        if (CanToggleBackground)
        {
            IsBackgroundChecked = !IsBackgroundChecked;
        }

        if (command != null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }

        RaiseEvent(new RoutedEventArgs(BackgroundClickEvent, this));
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        if (IsBackgroundChecked)
        {
            // The other way to do this would be to make CheckedBrush
            // a fully styled property and add to ChromeStyling.axaml
            AdornerBackground = CheckedBrush;
        }
    }
}
