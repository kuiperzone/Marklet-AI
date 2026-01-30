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

using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages;

/// <summary>
/// Concrete subclass of <see cref="SettingsCarouselPage{T}"/> which hosts <see cref="WindowSettings"/>.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control. Rather instances of this type are shown in <see
/// cref="CarouselWindow"/>.
/// </remarks>
public sealed class WindowCarouselPage : SettingsCarouselPage<WindowSettings>
{
    private readonly PixieSwitch _chromeSwitch = new();
    private readonly PixieSwitch _compactSwitch = new();
    private readonly PixieCombo _styleCombo = new();
    private readonly PixieCombo _backgroundCombo = new();

    private readonly PixieSwitch _dialogFollowsSwitch = new();
    private readonly PixieSwitch _taskbarSwitch = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public WindowCarouselPage()
        : base(WindowSettings.Global)
    {
        Title = "Window";
        Symbol = Symbols.Window;

        // MAIN WINDOW
        var group = new PixieGroup();
        group.TopTitle = "Main Window";
        Children.Add(group);

        _chromeSwitch.Title = "Integrated Titlebar";
        _chromeSwitch.LeftSymbol = Symbols.Ad;
        _chromeSwitch.Footer = "Application draws titlebar with integrated buttons. If unselected, use system titlebar.";
        group.Children.Add(_chromeSwitch);

        _compactSwitch.Title = "Compact Titlebar";
        _compactSwitch.LeftSymbol = Symbols.Compress;
        _compactSwitch.Footer = "Make integrated titlebars a little more compact";
        group.Children.Add(_compactSwitch);

        _styleCombo.Title = "Control Buttons";
        _styleCombo.LeftSymbol = Symbols.ExpandContent;
        _styleCombo.Footer = "Customize minimize, maximize and close buttons to align with desktop";
        _styleCombo.SetItemsAs<ChromeControlStyle>();
        group.Children.Add(_styleCombo);

        _backgroundCombo.Title = "Button Background";
        _backgroundCombo.LeftSymbol = Symbols.DisabledByDefault;
        _backgroundCombo.Footer = "Window control button background";
        _backgroundCombo.SetItemsAs<ChromeControlBackground>();
        group.Children.Add(_backgroundCombo);


        // CHILD WINDOW
        group = new PixieGroup();
        group.TopTitle = "Child Windows";
        Children.Add(group);

        _dialogFollowsSwitch.Title = "Dialogs Follow Main Window";
        _dialogFollowsSwitch.LeftSymbol = Symbols.SelectWindow;
        _dialogFollowsSwitch.Footer = "Dialog windows align with main window settings where possible";
        group.Children.Add(_dialogFollowsSwitch);

        _taskbarSwitch.Title = "Show in Taskbar";
        _taskbarSwitch.LeftSymbol = Symbols.Ad;
        _taskbarSwitch.Footer = "Child windows are shown in taskbar when opened";
        group.Children.Add(_taskbarSwitch);


        // RESET
        Children.Add(NewResetGroup($"Reset {Title} defaults"));

        // Update in construction not OpOpened().
        // Possible that some ChangedHandlers may be delayed.
        UpdateControls(Settings);
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateControls(WindowSettings settings)
    {
        _chromeSwitch.IsChecked = settings.IsChromeWindow;
        _compactSwitch.IsChecked = settings.IsCompact;

        _styleCombo.SelectedIndex = (int)settings.ControlStyle;
        _backgroundCombo.SelectedIndex = (int)settings.ControlBackground;

        _dialogFollowsSwitch.IsChecked = settings.DialogFollows;
        _taskbarSwitch.IsChecked = settings.ShowDialogInTaskbar;

        UpdateControlEnabledStates();
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateSettings(WindowSettings settings)
    {
        settings.IsChromeWindow = _chromeSwitch.IsChecked;
        settings.IsCompact = _compactSwitch.IsChecked;

        settings.ControlStyle = _styleCombo.GetSelectedIndexAs<ChromeControlStyle>();
        settings.ControlBackground = _backgroundCombo.GetSelectedIndexAs<ChromeControlBackground>();

        settings.DialogFollows = _dialogFollowsSwitch.IsChecked;
        settings.ShowDialogInTaskbar = _taskbarSwitch.IsChecked;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void OnOpened()
    {
        _chromeSwitch.ValueChanged += ControlValueChangedHandler;
        _compactSwitch.ValueChanged += ControlValueChangedHandler;
        _styleCombo.ValueChanged += ControlValueChangedHandler;
        _backgroundCombo.ValueChanged += ControlValueChangedHandler;
        _dialogFollowsSwitch.ValueChanged += ControlValueChangedHandler;
        _taskbarSwitch.ValueChanged += ControlValueChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void UpdateControlEnabledStates()
    {
        base.UpdateControlEnabledStates();
        bool chrome = _chromeSwitch.IsChecked;
        _styleCombo.IsEnabled = chrome;
        _backgroundCombo.IsEnabled = chrome;
    }

}