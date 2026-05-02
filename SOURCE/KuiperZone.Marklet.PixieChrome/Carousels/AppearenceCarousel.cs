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
/// Concrete subclass of <see cref="CarouselPage{T}"/> which hosts <see cref="AppearanceSettings"/>.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control. Rather instances of this type are shown in <see
/// cref="CarouselDialog"/>.
/// </remarks>
public sealed class AppearanceCarousel : CarouselPage<AppearanceSettings>
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
    public AppearanceCarousel()
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
        _themeCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_themeCombo);

        _fluentSwitch.Title = "Prefer Fluent Dark";
        _fluentSwitch.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_fluentSwitch);

        _accentPicker.Header = "Accent Color";
        _accentPicker.IsSecondaryVisible = true;
        _accentPicker.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_accentPicker);

        _tintPicker.Header = "Tint Overlay";
        _tintPicker.DefaultColorLabel = "None";
        _tintPicker.IsDefaultColorVisible = true;
        _tintPicker.Footer = "Adds a color overlay to the user interface";
        _tintPicker.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_tintPicker);

        // CORNER OTHER
        group = new PixieGroup();
        Children.Add(group);

        _cornerCombo.Title = "Corner Size";
        _cornerCombo.IsTranslateFriendly = true;
        _cornerCombo.LeftSymbol = Symbols.RoundedCorner;
        _cornerCombo.Footer = "Set the corner size of windows and controls";
        _cornerCombo.SetItemsAs<CornerSize>();
        _cornerCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_cornerCombo);

        // RESET
        Children.Add(CreateResetGroup());
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

        EnableControls();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void EnableControls()
    {
        bool check = _fluentSwitch.IsChecked;
        _fluentSwitch.Footer = check == IsInitFluent ? _fluentFooter : "APPLICATION RESTART REQUIRED";
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

}