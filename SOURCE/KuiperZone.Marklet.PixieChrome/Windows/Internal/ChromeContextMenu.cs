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
using Avalonia.Interactivity;

namespace KuiperZone.Marklet.PixieChrome.Windows.Internal;

/// <summary>
/// Chrome bar context menu.
/// </summary>
internal sealed class ChromeContextMenu : ContextMenu
{
    private readonly Window _window;
    private readonly MenuItem _minimizeMenuItem = new();
    private readonly MenuItem _maximizeMenuItem = new();
    private readonly MenuItem _onTopMenuItem = new();
    private readonly MenuItem _closeMenuItem = new();

    public ChromeContextMenu(Window window)
    {
        _window = window;
        window.PropertyChanged += WindowPropertyChangedHandler;
        Opened += ContextMenuOpenedHandler;

        _minimizeMenuItem.Header = "Hide";
        _minimizeMenuItem.Click += MinimizeClickHandler;
        Items.Add(_minimizeMenuItem);

        // Header will be initialized later
        _maximizeMenuItem.Click += MaximizeRestoreClickHandler;
        Items.Add(_maximizeMenuItem);

        Items.Add(new Separator());

        _onTopMenuItem.Header = "Always on Top";
        _onTopMenuItem.ToggleType = MenuItemToggleType.CheckBox;
        _onTopMenuItem.Click += OnTopClickHandler;
        Items.Add(_onTopMenuItem);

        Items.Add(new Separator());

        _closeMenuItem.Header = "Close";
        _closeMenuItem.Click += CloseClickHandler;
        Items.Add(_closeMenuItem);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(ContextMenu);

    private void WindowPropertyChangedHandler(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        var p = e.Property;

        if (p == WindowBase.TopmostProperty)
        {
            _onTopMenuItem?.IsChecked = e.GetNewValue<bool>();
            return;
        }
    }

    private void OnTopClickHandler(object? _, EventArgs __)
    {
        _window.Topmost = !_window.Topmost;
    }

    private void ContextMenuOpenedHandler(object? _, EventArgs __)
    {
        _minimizeMenuItem.IsVisible = _window.CanMinimize;
        _maximizeMenuItem.IsVisible = _window.CanMaximize;
        _maximizeMenuItem.Header = _window.WindowState == WindowState.Maximized ? "Restore" : "Maximize";
        _onTopMenuItem!.IsVisible = _window == ChromeApplication.MainWindow;
        this.Normalize();
    }

    private void MinimizeClickHandler(object? _, RoutedEventArgs __)
    {
        _window.WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreClickHandler(object? _, RoutedEventArgs __)
    {
        var s = _window.WindowState;
        _window.WindowState = s != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseClickHandler(object? _, RoutedEventArgs __)
    {
        _window.Close();
    }
}
