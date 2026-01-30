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
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Simple hosting <see cref="LightButton"/> instances.
/// </summary>
public sealed class LightBar : StackPanel, ISubLightBar
{
    private readonly List<LightButton> _buttons = new();
    private double _fontScale = 1.0;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// VerticalAlignment is initialized to Top.
    /// </remarks>
    public LightBar()
    {
        Buttons = _buttons;
        Spacing = 5.0;
        base.Orientation = Orientation.Horizontal;
        VerticalAlignment = VerticalAlignment.Top;
    }

    /// <summary>
    /// Defines the <see cref="ButtonsChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ButtonsChangedEvent =
        RoutedEvent.Register<LightBar, RoutedEventArgs>(nameof(ButtonsChanged), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="FontScale"/> property.
    /// </summary>
    public static readonly DirectProperty<LightBar, double> FontScaleProperty =
        AvaloniaProperty.RegisterDirect<LightBar, double>(nameof(FontScale),
        o => o.FontScale, (o, v) => o.FontScale = v, 1.0);

    /// <summary>
    /// Occurs when items are added or removed from <see cref="Buttons"/>.
    /// </summary>
    public event EventHandler<RoutedEventArgs> ButtonsChanged
    {
        add { AddHandler(ButtonsChangedEvent, value); }
        remove { RemoveHandler(ButtonsChangedEvent, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="LightButton.FontScale"/> for all buttons.
    /// </summary>
    /// <remarks>
    /// The value clamped between <see cref="LightButton.MinFontScale"/> and <see cref="LightButton.MaxFontScale"/>.
    /// Setting NaN throws. The default is 1.0.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid NaN</exception>
    public double FontScale
    {
        get { return _fontScale; }
        set { SetAndRaise(FontScaleProperty, ref _fontScale, ClampFontScale(value)); }
    }

    /// <inheritdoc cref="ISubLightBar.Buttons"/>
    public IReadOnlyList<LightButton> Buttons { get; }

    /// <summary>
    /// Gets the actual button height within the control, irrespective of whether <see cref="Buttons"/> is empty.
    /// </summary>
    public double ButtonHeight { get; private set; } = LightButton.MinBoxSize;

    /// <summary>
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Avalonia.Controls.Controls Children
    {
        get { return base.Children; }
    }

    /// <summary>
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Orientation Orientation
    {
        get { return base.Orientation; }
        set { base.Orientation = value; }
    }

    /// <inheritdoc cref="ISubLightBar.Clear"/>
    public void Clear()
    {
        _buttons.Clear();
        base.Children.Clear();
    }

    /// <inheritdoc cref="ISubLightBar.Add(string?, string?)"/>
    public LightButton Add(string? text, string? tip = null)
    {
        var button = new LightButton();
        button.Content = text;
        button.Tip = tip;
        button.VerticalAlignment = VerticalAlignment.Center;
        Update(button);

        base.Children.Add(button);
        _buttons.Add(button);
        RaiseEvent(new RoutedEventArgs(ButtonsChangedEvent, this));
        return button;
    }

    /// <summary>
    /// Overload providing a click handler.
    /// </summary>
    public LightButton Add(string text, EventHandler<RoutedEventArgs>? handler, string? tip = null)
    {
        var button = Add(text, tip);
        button.Click += handler;
        return button;
    }

    /// <summary>
    /// Overload providing a <see cref="ContextMenu"/> instance which is opened when the button is clicked.
    /// </summary>
    public LightButton Add(string text, ContextMenu? menu, string? tip = null)
    {
        var button = Add(text, tip);
        button.DropMenu = menu;
        return button;
    }

    /// <inheritdoc cref="ISubLightBar.Remove(LightButton)"/>
    public bool Remove(LightButton button)
    {
        if (button is LightButton lb && _buttons.Remove(lb) && base.Children.Remove(lb))
        {
            RaiseEvent(new RoutedEventArgs(ButtonsChangedEvent, this));
            return true;
        }

        return false;
    }

    /// <inheritdoc cref="ISubLightBar.HandleKeyGesture(KeyEventArgs)"/>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        if (!e.Handled)
        {
            foreach (var item in _buttons)
            {
                if (item.HandleKeyGesture(e))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;

        if (p == OrientationProperty && change.GetNewValue<Orientation>() != Orientation.Horizontal)
        {
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }

        base.OnPropertyChanged(change);

        if (p == FontScaleProperty)
        {
            ButtonHeight = LightButton.MinBoxSize * change.GetNewValue<double>();

            foreach (var item in _buttons)
            {
                Update(item);
            }

            return;
        }
    }

    private static double ClampFontScale(double scale)
    {
        if (double.IsNaN(scale))
        {
            throw new ArgumentException("Invalid NaN", nameof(FontScale));
        }

        return Math.Clamp(scale, LightButton.MinFontScale, LightButton.MaxFontScale);
    }

    private void Update(LightButton button)
    {
        button.FontScale = _fontScale;
        button.MinWidth = ButtonHeight;
        button.MinHeight = ButtonHeight;
    }
}
