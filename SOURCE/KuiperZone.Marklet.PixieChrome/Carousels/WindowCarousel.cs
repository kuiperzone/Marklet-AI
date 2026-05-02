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

using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.Carousels;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage{T}"/> which hosts <see cref="WindowSettings"/>.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control. Rather instances of this type are shown in <see
/// cref="CarouselDialog"/>.
/// </remarks>
public sealed class WindowCarousel : CarouselPage<WindowSettings>
{
    private readonly PixieSwitch _chromeSwitch = new();
    private readonly PixieSwitch _compactSwitch = new();
    private readonly PixieCombo _styleCombo = new();
    private readonly PixieCombo _backgroundCombo = new();

    private readonly PixieSwitch _dialogAccentSwitch = new();
    private readonly PixieSwitch _taskbarSwitch = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public WindowCarousel()
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
        _chromeSwitch.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_chromeSwitch);

        _compactSwitch.Title = "Compact Titlebar";
        _compactSwitch.LeftSymbol = Symbols.Compress;
        _compactSwitch.Footer = "Make integrated titlebars a little more compact";
        _compactSwitch.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_compactSwitch);

        _styleCombo.Title = "Control Buttons";
        _styleCombo.IsTranslateFriendly = true;
        _styleCombo.LeftSymbol = Symbols.ExpandContent;
        _styleCombo.Footer = "Customize minimize, maximize and close buttons to align with desktop";
        _styleCombo.SetItemsAs<ChromeControlStyle>();
        _styleCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_styleCombo);

        _backgroundCombo.Title = "Button Background";
        _backgroundCombo.IsTranslateFriendly = true;
        _backgroundCombo.LeftSymbol = Symbols.DisabledByDefault;
        _backgroundCombo.Footer = "Window control button background";
        _backgroundCombo.SetItemsAs<ChromeControlBackground>();
        _backgroundCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_backgroundCombo);


        // CHILD WINDOW
        group = new PixieGroup();
        group.TopTitle = "Child Windows";
        Children.Add(group);

        _taskbarSwitch.Title = "Show in Taskbar";
        _taskbarSwitch.LeftSymbol = Symbols.Ad;
        _taskbarSwitch.Footer = "Child windows are shown in taskbar when opened";
        _taskbarSwitch.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_taskbarSwitch);


        _dialogAccentSwitch.Title = "Dialog Border Accent";
        _dialogAccentSwitch.LeftSymbol = Symbols.Palette;
        _dialogAccentSwitch.Footer = "Dialog windows use accent color for the border";
        _dialogAccentSwitch.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_dialogAccentSwitch);

        // RESET
        Children.Add(CreateResetGroup());
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

        _taskbarSwitch.IsChecked = settings.ShowDialogInTaskbar;
        _dialogAccentSwitch.IsChecked = settings.DialogAccentBorder;

        EnableControls();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void EnableControls()
    {
        bool chrome = _chromeSwitch.IsChecked;
        _compactSwitch.IsEnabled = chrome;
        _styleCombo.IsEnabled = chrome;
        _backgroundCombo.IsEnabled = chrome;
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

        settings.ShowDialogInTaskbar = _taskbarSwitch.IsChecked;
        settings.DialogAccentBorder = _dialogAccentSwitch.IsChecked;
    }

}