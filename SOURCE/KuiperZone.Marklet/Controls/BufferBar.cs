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

using Avalonia.Controls;
using KuiperZone.Marklet.Stack.Garden;
using Avalonia;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Windows;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Presents and manages the collection of <see cref="GardenBasket"/> instances provided by <see cref="MemoryGarden"/>.
/// </summary>
public sealed class BufferBar : Border
{
    private readonly DockPanel _panel = new();
    private BasketKind _basket;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public BufferBar()
    {
        base.Child = _panel;
        base.MinWidth = BufferButton.ButtonSize;

        AddRadio(BasketKind.Recent);
        AddRadio(BasketKind.Notes);
        AddRadio(BasketKind.Archive);
        AddRadio(BasketKind.Waste);

        var b = new BufferButton(Symbols.Settings, "Settings");
        b.Click += SettingsClickHandler;
        _panel.Children.Add(b);
        DockPanel.SetDock(b, Dock.Bottom);

        b = new(Symbols.Info, "About");
        b.Click += AboutClickHandler;
        _panel.Children.Add(b);
        DockPanel.SetDock(b, Dock.Bottom);

#if DEBUG
        b = new(Symbols.CategorySearch, "Inspect");
        b.Click += InspectClickHandler;
        _panel.Children.Add(b);
        DockPanel.SetDock(b, Dock.Bottom);
#endif
    }

    /// <summary>
    /// Occurs when <see cref="Basket"/> changes.
    /// </summary>
    public event EventHandler<BasketChangedEventArgs>? BoxChanged;

    /// <summary>
    /// Gets or sets currently selected basket kind.
    /// </summary>
    public BasketKind Basket
    {
        get { return _basket; }

        set
        {
            if (_basket != value)
            {
                _basket = value;

                foreach (var item in _panel.Children)
                {
                    if (item is BufferButton button && button.CanToggle)
                    {
                        button.IsChecked = button.Basket == value;
                    }
                }

                BoxChanged?.Invoke(this, new(value));
            }
        }
    }

    /// <summary>
    /// Replaces and makes readonly.
    /// </summary>
    public new double MinWidth
    {
        get { return base.MinWidth; }
    }

    /// <summary>
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    private void AddRadio(BasketKind kind)
    {
        var a = new BufferButton(kind);
        a.IsChecked = kind == _basket;
        a.CheckedChanged += RadioChangedHandler;
        _panel.Children.Add(a);
        DockPanel.SetDock(a, Dock.Top);
    }

    private void RadioChangedHandler(object? sender, RoutedEventArgs __)
    {
        if (sender is BufferButton button && button.IsChecked)
        {
            Basket = button.Basket;
        }
    }

    private async void SettingsClickHandler(object? _, RoutedEventArgs __)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            await new AppSettingsWindow().ShowDialog(window);
        }
    }

    private async void AboutClickHandler(object? _, RoutedEventArgs __)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            await new AboutWindow().ShowDialog(window);
        }
    }

#if DEBUG
    private async void InspectClickHandler(object? _, RoutedEventArgs __)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            await new InspectWindow().ShowDialog(window);
        }
    }
#endif
}