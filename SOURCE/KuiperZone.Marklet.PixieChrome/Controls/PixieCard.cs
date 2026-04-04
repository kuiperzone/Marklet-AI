// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A subclass of <see cref="PixieControl"/> which has a checkable background, serving as toggle control itself.
/// Moreover, this control supports the user renaming of <see cref="PixieControl.Title"/> from the interface.
/// </summary>
/// <remarks>
/// The <see cref="IsChecked"/> is exclusive inside an instance of <see cref="PixieGroup"/>.
/// </remarks>
public class PixieCard : PixieControl
{
    // Backing fields
    private ICommand? _command;
    private object? _commandParameter;
    private bool _isChecked;
    private bool _canToggle;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieCard()
    {
        SetPseudoFocusControl(this);
    }

    /// <summary>
    /// Defines the <see cref="Click"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<PixieCard, RoutedEventArgs>(nameof(Click), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="Command"/> event.
    /// </summary>
    public static readonly DirectProperty<PixieCard, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<PixieCard, ICommand?>(nameof(Command),
        o => o.Command, (o, v) => o.Command = v);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCard, object?> CommandParameterProperty =
        AvaloniaProperty.RegisterDirect<PixieCard, object?>(nameof(CommandParameter),
        o => o.CommandParameter, (o, v) => o.CommandParameter = v);

    /// <summary>
    /// Defines the <see cref="IsChecked"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCard, bool> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<PixieCard, bool>(nameof(IsChecked),
        o => o.IsChecked, (o, v) => o.IsChecked = v);

    /// <summary>
    /// Defines the <see cref="CanToggle"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieCard, bool> CanToggleProperty =
        AvaloniaProperty.RegisterDirect<PixieCard, bool>(nameof(CanToggle),
        o => o.CanToggle, (o, v) => o.CanToggle = v);

    /// <summary>
    /// Occurs when background area is left-clicked or invoked by key press.
    /// </summary>
    /// <remarks>
    /// The sender is "this" instance.
    /// </remarks>
    public event EventHandler<RoutedEventArgs> Click
    {
        add { AddHandler(ClickEvent, value); }
        remove { RemoveHandler(ClickEvent, value); }
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the background is left-clicked or invoked by key
    /// press.
    /// </summary>
    public ICommand? Command
    {
        get { return _command; }
        set { SetAndRaise(CommandProperty, ref _command, value); }
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command"/>.
    /// </summary>
    public object? CommandParameter
    {
        get { return _commandParameter; }
        set { SetAndRaise(CommandParameterProperty, ref _commandParameter, value); }
    }

    /// <summary>
    /// Gets whether the control background is checked (does not apply to child button).
    /// </summary>
    /// <remarks>
    /// When inside <see cref="PixieGroup"/> container, this acts like a radio button with exclusive state.
    /// </remarks>
    public bool IsChecked
    {
        get { return _isChecked; }
        set { SetAndRaise(IsCheckedProperty, ref _isChecked, value); }
    }

    /// <summary>
    /// Gets whether <see cref="IsChecked"/> can be toggled by the pointer or keyboard (does not apply to
    /// child button).
    /// </summary>
    /// <remarks>
    /// When false, the <see cref="IsChecked"/> can still be set programmatically. The default is false.
    /// </remarks>
    public bool CanToggle
    {
        get { return _canToggle; }
        set { SetAndRaise(CanToggleProperty, ref _canToggle, value); }
    }

    /// <summary>
    /// Gets the checked brush used for <see cref="PixieCard"/> background.
    /// </summary>
    public static ImmutableSolidColorBrush CheckedBrush
    {
        get { return Styling.Accent35; }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override ImmutableSolidColorBrush HoverBrush
    {
        get { return IsChecked ? CheckedBrush : base.HoverBrush; }
    }

    /// <summary>
    /// Starts the renaming of <see cref="PixieControl.Title"/> in the user interface and returns true if editing was
    /// initiated.
    /// </summary>
    /// <remarks>
    /// The renaming is handled by the parent <see cref="PixieGroup"/> and this method simply calls <see
    /// cref="PixieGroup.StartRename(PixieCard, int, int)"/> on <see cref="PixieControl.Group"/>.
    /// </remarks>
    public bool StartRename(int maxLength, int minLength = 1)
    {
        return Group?.StartRename(this, maxLength, minLength) == true;
    }

    /// <summary>
    /// Brings into view and calls attention.
    /// </summary>
    public override void Attention(IBrush? background = null)
    {
        if (!IsChecked)
        {
            base.Attention(background);
        }
    }

    /// <summary>
    /// Internal use.
    /// </summary>
    internal void OnClick()
    {
        var command = Command;
        var parameter = CommandParameter;

        if (CanToggle)
        {
            IsChecked = !IsChecked;
        }

        if (command != null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    internal override void ClearAdornerBackground(bool delayed)
    {
        if (!IsChecked)
        {
            base.ClearAdornerBackground(delayed);
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

        if (p == IsCheckedProperty)
        {
            AdornerBackground = change.GetNewValue<bool>() ? CheckedBrush : null;
            Group?.SetCardOnCheckedChanged(this);
            OnValueChanged();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            e.Handled = true;
            OnClick();
            return;
        }

        base.OnPointerReleased(e);
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
            OnClick();
            return;
        }

        base.OnKeyDown(e);
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        if (IsChecked)
        {
            // The other way to do this would be to make CheckedBrush
            // a fully styled property and add to ChromeStyling.axaml
            AdornerBackground = CheckedBrush;
        }
    }
}
