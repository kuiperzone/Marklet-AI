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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// A low-level control used to display standard <see cref="DialogButtons"/> at the bottom of a dialog window.
/// </summary>
public class DialogControls : Border
{
    /// <summary>
    /// Margin assigned on construction.
    /// </summary>
    public const double UniformPadding = ChromeSizes.StandardPx * 3.3;

    private static readonly DialogButtons[] AllButtons = Enum.GetValues<DialogButtons>();
    private readonly StackPanel _panel = new();
    private DialogButtons _buttons;
    private DialogButtons _disabledButtons;
    private bool _isShown;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DialogControls()
    {
        Padding = new(UniformPadding);

        _panel.Spacing = 8.0;
        _panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        _panel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

        base.Child = _panel;

        _buttons = DialogButtons.Close;
        AddButton(DialogButtons.Close);
    }

    /// <summary>
    /// Defines the <see cref="Click"/> event.
    /// </summary>
    public static readonly RoutedEvent<DialogClickEventArgs> ClickEvent =
        RoutedEvent.Register<DialogControls, DialogClickEventArgs>(nameof(Click), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="Buttons"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogControls, DialogButtons> ButtonsProperty =
        AvaloniaProperty.RegisterDirect<DialogControls, DialogButtons>(nameof(Buttons), o => o.Buttons, (o, v) => o.Buttons = v);

    /// <summary>
    /// Occurs when the user clicks button displayed by this control.
    /// </summary>
    public event EventHandler<DialogClickEventArgs> Click
    {
        add { AddHandler(ClickEvent, value); }
        remove { RemoveHandler(ClickEvent, value); }
    }

    /// <summary>
    /// Gets or sets the buttons.
    /// </summary>
    /// <remarks>
    /// The value must not contain an illegal combination of multiple "default" or "cancel" buttons otherwise the setter
    /// throws. The default is <see cref="DialogButtons.Close"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">Illegal button combination</exception>
    public DialogButtons Buttons
    {
        get { return _buttons; }
        set { SetAndRaise(ButtonsProperty, ref _buttons, value); }
    }

    /// <summary>
    /// Gets or sets which <see cref="Buttons"/> are disabled.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="DialogButtons.None"/> which implies all buttons are enabled.
    /// </remarks>
    public DialogButtons DisabledButtons
    {
        get { return _disabledButtons; }

        set
        {
            if (_disabledButtons != value)
            {
                _disabledButtons = value;

                foreach (var item in _panel.Children)
                {
                    if (item.Tag is DialogButtons flag)
                    {
                        item.IsEnabled = !value.HasFlag(flag);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets a mutable button text dictionary.
    /// </summary>
    /// <remarks>
    /// The window may use this to change default button text before the control is displayed. Default is empty.
    /// </remarks>
    public Dictionary<DialogButtons, string> ButtonText { get; } = new();

    /// <summary>
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Handles the give key gesture by invoking <see cref="Click"/> and returns true on match.
    /// </summary>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(DialogControls)}.{nameof(OnKeyDown)}";
        Diag.WriteLine(NSpace, $"Physical key: {e.PhysicalKey}");

        if (e.KeyModifiers == KeyModifiers.None)
        {
            var flag = Buttons.GetCloseAction(e.Key);

            if (flag != DialogButtons.None && !DisabledButtons.HasFlag(flag))
            {
                Diag.WriteLine(NSpace, $"Close result: {flag}");
                e.Handled = true;
                OnRaise(flag);
                return true;
            }
        }

        base.OnKeyDown(e);
        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ButtonsProperty)
        {
            var value = change.GetNewValue<DialogButtons>();

            if (!value.IsCombinedLegal())
            {
                _buttons = change.GetOldValue<DialogButtons>();
                throw new ArgumentException("Illegal button combination");
            }

            if (IsEffectivelyVisible)
            {
                UpdateButtons();
            }

            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();

        if (!_isShown)
        {
            _isShown = true;
            UpdateButtons();
        }
    }

    private void OnRaise(DialogButtons button)
    {
        RaiseEvent(new DialogClickEventArgs(ClickEvent, this, button));
    }

    private void UpdateButtons()
    {
        _panel.Children.Clear();

        foreach (var item in AllButtons)
        {
            if (item != DialogButtons.None && Buttons.HasFlag(item))
            {
                AddButton(item);
            }
        }
    }

    private string GetButtonText(DialogButtons button)
    {
        if (ButtonText.TryGetValue(button, out string? text))
        {
            return text;
        }

        return button == DialogButtons.Ok ? "OK" : button.ToString().GetFriendlyNameOf();
    }

    private void AddButton(DialogButtons button)
    {
        Diag.ThrowIfFalse(button.IsSingleLegal());
        Diag.ThrowIfEqual(DialogButtons.None, button);

        var obj = new LightButton();
        obj.Classes.Add("dialog-button");
        obj.Content = GetButtonText(button);
        obj.Tag = button;
        obj.IsEnabled = !_disabledButtons.HasFlag(button);
        obj.Click += (_, __) => OnRaise(button);

        if (button.IsCritical())
        {
            obj.Classes.Add("critical-background");
        }
        else
        if (button.IsDefault())
        {
            obj.Classes.Add("accent-background");
        }
        else
        {
            obj.Classes.Add("regular-background");
        }

        _panel.Children.Add(obj);
    }
}