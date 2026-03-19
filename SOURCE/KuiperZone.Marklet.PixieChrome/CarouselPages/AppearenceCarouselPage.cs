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

namespace KuiperZone.Marklet.PixieChrome.CarouselPages;

/// <summary>
/// Concrete subclass of <see cref="SettingsCarouselPage{T}"/> which hosts <see cref="AppearanceSettings"/>.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control. Rather instances of this type are shown in <see
/// cref="CarouselWindow"/>.
/// </remarks>
public sealed class AppearanceCarouselPage : SettingsCarouselPage<AppearanceSettings>
{
    private static readonly bool IsInitFluent = AppearanceSettings.Global.PreferFluentDark;

    private readonly PixieCombo _themeCombo = new();
    private readonly PixieSwitch _fluentSwitch = new();
    private readonly string _fluentFooter = "Prefer Fluent colors (black) for the dark theme";
    private readonly PixieCombo _cornerCombo = new();

    private readonly PixieAccentPicker _accentPicker = new();
    private readonly PixieAccentPicker _tintPicker = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public AppearanceCarouselPage()
        : base(AppearanceSettings.Global)
    {
        Title = "Appearance";
        Symbol = Symbols.DesktopWindows;

        // THEME
        var group = new PixieGroup();
        group.TopTitle = "Theme and Accent";
        Children.Add(group);

        _themeCombo.Title = "Main Theme";
        _themeCombo.IsTranslateFriendly = true;
        _themeCombo.LeftSymbol = Symbols.DarkMode;
        _themeCombo.Footer = "Set light or dark theme";
        _themeCombo.SetItemsAs<ChromeTheme>();
        group.Children.Add(_themeCombo);

        _fluentSwitch.Title = "Prefer Fluent Dark";
        group.Children.Add(_fluentSwitch);

        _accentPicker.Header = "Accent Color";
        _accentPicker.IsSecondaryVisible = true;
        group.Children.Add(_accentPicker);

        _tintPicker.Header = "Tint Overlay";
        _tintPicker.DefaultColorLabel = "None";
        _tintPicker.IsDefaultColorVisible = true;
        _tintPicker.Footer = "Adds a color overlay to the user interface";
        group.Children.Add(_tintPicker);

        // CORNER OTHER
        group = new PixieGroup();
        Children.Add(group);

        _cornerCombo.Title = "Corner Size";
        _cornerCombo.IsTranslateFriendly = true;
        _cornerCombo.LeftSymbol = Symbols.RoundedCorner;
        _cornerCombo.Footer = "Set the corner size of windows and controls";
        _cornerCombo.SetItemsAs<CornerSize>();
        group.Children.Add(_cornerCombo);


        // RESET
        Children.Add(NewResetGroup($"Reset {Title} defaults"));

        // Update in construction not OpOpened().
        // Possible that some ChangedHandlers may be delayed.
        UpdateControls(Settings);
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateControls(AppearanceSettings settings)
    {
        _themeCombo.SelectedIndex = (int)settings.Theme;
        _fluentSwitch.IsChecked = settings.PreferFluentDark;
        _accentPicker.ChosenColor = settings.ToAccentColor();
        _tintPicker.ChosenColor = settings.ToTintColor();
        _cornerCombo.SelectedIndex = (int)settings.Corners;

        UpdateControlEnabledStates();
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateSettings(AppearanceSettings settings)
    {
        settings.Theme = _themeCombo.GetSelectedIndexAs<ChromeTheme>();
        settings.PreferFluentDark = _fluentSwitch.IsChecked;
        settings.AccentColor = _accentPicker.ChosenColor.ToUInt32();
        settings.TintColor = _tintPicker.ChosenColor.ToUInt32();
        settings.Corners = _cornerCombo.GetSelectedIndexAs<CornerSize>();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void OnOpened()
    {
        _themeCombo.ValueChanged += ControlValueChangedHandler;
        _fluentSwitch.ValueChanged += ControlValueChangedHandler;
        _accentPicker.ValueChanged += ControlValueChangedHandler;
        _tintPicker.ValueChanged += ControlValueChangedHandler;
        _cornerCombo.ValueChanged += ControlValueChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void UpdateControlEnabledStates()
    {
        base.UpdateControlEnabledStates();
        bool check = _fluentSwitch.IsChecked;
        _fluentSwitch.Footer = check == IsInitFluent ? _fluentFooter : "APPLICATION RESTART REQUIRED";
    }

}