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
using Avalonia.Layout;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Simple hosting <see cref="LightButton"/> instances.
/// </summary>
/// <remarks>
/// Essentially just a <see cref="DockPanel"/> with a custom interface.
/// </remarks>
public class LightBar : DockPanel, ISubLightBar
{
    private readonly List<LightButton> _buttons = new();
    private double _fontScale = 1.0;
    private Dock _contentDirection;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public LightBar()
    {
        Buttons = _buttons;
        HorizontalSpacing = 5.0;
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
    /// Defines the <see cref="ContentDirection"/> property.
    /// </summary>
    public static readonly DirectProperty<LightBar, Dock> ContentDirectionProperty =
        AvaloniaProperty.RegisterDirect<LightBar, Dock>(nameof(ContentDirection),
        o => o.ContentDirection, (o, v) => o.ContentDirection = v);

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

    /// <summary>
    /// Gets or sets the <see cref="Dock"/> direction of controls added via <see cref="AddButton(string?, string?)"/>
    /// and related overloads.
    /// </summary>
    public Dock ContentDirection
    {
        get { return _contentDirection; }
        set { SetAndRaise(ContentDirectionProperty, ref _contentDirection, value); }
    }


    /// <inheritdoc cref="ISubLightBar.Buttons"/>
    public IReadOnlyList<LightButton> Buttons { get; }

    /// <summary>
    /// Replaces base Children with readonly list.
    /// </summary>
    public new IReadOnlyList<Control> Children
    {
        get { return base.Children; }
    }

    /// <summary>
    /// Gets the actual button height within the control, irrespective of whether <see cref="Buttons"/> is empty.
    /// </summary>
    public double ButtonHeight { get; private set; } = LightButton.MinBoxSize;

    /// <inheritdoc cref="ISubLightBar.Clear"/>
    public virtual void Clear()
    {
        if (Children.Count != 0)
        {
            _buttons.Clear();
            base.Children.Clear();
            RaiseEvent(new RoutedEventArgs(ButtonsChangedEvent, this));
        }
    }

    /// <summary>
    /// Adds a control.
    /// </summary>
    /// <remarks>
    /// The <see cref="DockPanel()"/> is called on the control using <see cref="ContentDirection"/>.
    /// </remarks>
    public void Add(Control obj)
    {
        base.Children.Add(obj);
        SetDirection(obj, _contentDirection);

        if (obj is LightButton button)
        {
            Update(button);
            _buttons.Add(button);
        }

        RaiseEvent(new RoutedEventArgs(ButtonsChangedEvent, this));
    }

    /// <inheritdoc cref="ISubLightBar.AddButton(string?, string?)"/>
    public LightButton AddButton(string? content, string? tip = null)
    {
        var obj = new LightButton();
        obj.Content = content;
        obj.Tip = tip;
        Add(obj);
        return obj;
    }

    /// <summary>
    /// Overload providing a click handler.
    /// </summary>
    public LightButton AddButton(string content, EventHandler<RoutedEventArgs>? click, string? tip = null)
    {
        var obj = AddButton(content, tip);
        obj.Click += click;
        return obj;
    }

    /// <summary>
    /// Overload providing a <see cref="ContextMenu"/> instance which is opened when the button is clicked.
    /// </summary>
    public LightButton AddButton(string content, ContextMenu? menu, string? tip = null)
    {
        var obj = AddButton(content, tip);
        obj.DropMenu = menu;
        return obj;
    }

    /// <inheritdoc cref="ISubLightBar.Remove(Control)"/>
    public bool Remove(Control obj)
    {
        if (base.Children.Remove(obj))
        {
            if (obj is LightButton button)
            {
                _buttons.Remove(button);
            }

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

        if (p == ContentDirectionProperty)
        {
            var array = Children.ToArray();
            base.Children.Clear();

            foreach (var item in array)
            {
                SetDirection(item, _contentDirection);
                base.Children.Add(item);
            }
        }
    }

    private static double ClampFontScale(double scale)
    {
        if (double.IsNaN(scale))
        {
            throw new ArgumentException("Invalid NaN");
        }

        return Math.Clamp(scale, LightButton.MinFontScale, LightButton.MaxFontScale);
    }

    private static void SetDirection(Control obj, Dock value)
    {
        SetDock(obj, value);
        SetHorizontalAlignment(obj, value);
        SetVerticalAlignment(obj, value);
    }

    private static void SetHorizontalAlignment(Control obj, Dock value)
    {
        if (value == Dock.Left)
        {
            obj.HorizontalAlignment = HorizontalAlignment.Left;
            return;
        }

        if (value == Dock.Right)
        {
            obj.HorizontalAlignment = HorizontalAlignment.Right;
            return;
        }

        obj.HorizontalAlignment = HorizontalAlignment.Center;
    }

    private static void SetVerticalAlignment(Control obj, Dock value)
    {
        if (value == Dock.Top)
        {
            obj.VerticalAlignment = VerticalAlignment.Top;
            return;
        }

        if (value == Dock.Bottom)
        {
            obj.VerticalAlignment = VerticalAlignment.Bottom;
        }

        obj.VerticalAlignment = VerticalAlignment.Center;
    }

    private void Update(LightButton button)
    {
        button.FontScale = _fontScale;
        button.MinWidth = ButtonHeight;
        button.MinHeight = ButtonHeight;
    }
}
