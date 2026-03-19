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

using System.Text.Json;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.Settings;

/// <summary>
/// Serializable window settings.
/// </summary>
public sealed class WindowSettings : SettingsBase, IEquatable<SettingsBase>
{
    /// <summary>
    /// Gets a global instance loaded on <see cref="ChromeApplication.Initialize"/>.
    /// </summary>
    public static WindowSettings Global { get; } = new();

    /// <summary>
    /// Gets or sets whether to use a client drawn titlebar for the main window.
    /// </summary>
    public bool IsChromeWindow { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use a compact titlebar when <see cref="IsChromeWindow"/> is true.
    /// </summary>
    public bool IsCompact { get; set; }

    /// <summary>
    /// Gets or sets the window's minimize, maximize, close button layout when <see cref="IsChromeWindow"/> is true.
    /// </summary>
    public ChromeControlStyle ControlStyle { get; set; }

    /// <summary>
    /// Gets or sets the window's minimize, maximize, close button decoration when <see cref="IsChromeWindow"/> is true.
    /// </summary>
    public ChromeControlBackground ControlBackground { get; set; }

    /// <summary>
    /// Gets or sets whether to show dialog windows in the taskbar.
    /// </summary>
    public bool ShowDialogInTaskbar { get; set; } = true;

    /// <summary>
    /// Gets or sets whether dialog windows have accented border.
    /// </summary>
    public bool DialogAccentBorder { get; set; }

    /// <inheritdoc cref="SettingsBase.Reset"/>
    public override void Reset()
    {
        CopyFrom(new());
    }

    /// <inheritdoc cref="SettingsBase.Deserialize(string)"/>
    public override void Deserialize(string json)
    {
        CopyFrom(JsonSerializer.Deserialize(json, SettingsSerializer.Default.WindowSettings));
    }

    /// <inheritdoc cref="SettingsBase.Serialize"/>
    public override string? Serialize()
    {
        return JsonSerializer.Serialize(this, SettingsSerializer.Default.WindowSettings);
    }

    /// <inheritdoc cref="SettingsBase.TrimLegal"/>
    public override void TrimLegal()
    {
        ControlStyle = ControlStyle.TrimLegal();
        ControlBackground = ControlBackground.TrimLegal();
    }

    /// <inheritdoc cref="SettingsBase.Equals(SettingsBase?)"/>
    public override bool Equals(SettingsBase? other)
    {
        return other is WindowSettings s &&
            IsChromeWindow == s.IsChromeWindow &&
            IsCompact == s.IsCompact &&
            ControlStyle == s.ControlStyle &&
            ControlBackground == s.ControlBackground &&
            ShowDialogInTaskbar == s.ShowDialogInTaskbar &&
            DialogAccentBorder == s.DialogAccentBorder;
    }

    /// <summary>
    /// Copies setting from "other" to "this".
    /// </summary>
    public void CopyFrom(WindowSettings? other)
    {
        if (other != null)
        {
            IsChromeWindow = other.IsChromeWindow;
            IsCompact = other.IsCompact;
            ControlStyle = other.ControlStyle;
            ControlBackground = other.ControlBackground;
            ShowDialogInTaskbar = other.ShowDialogInTaskbar;
            DialogAccentBorder = other.DialogAccentBorder;
        }
    }

    /// <summary>
    /// Helper for <see cref="ChromeWindow"/>.
    /// </summary>
    public bool GetChromeWindow(bool isDialog)
    {
        return isDialog || IsChromeWindow;
    }

    /// <summary>
    /// Helper for <see cref="ChromeWindow"/>.
    /// </summary>
    public bool GetCompact(bool isDialog)
    {
        return !isDialog && IsCompact;
    }

    /// <summary>
    /// Helper for <see cref="ChromeWindow"/>.
    /// </summary>
    public ChromeControlStyle GetControlStyle(bool isDialog)
    {
        return isDialog ? ChromeControlStyle.LargeCloseOnly : ControlStyle;
    }

    /// <summary>
    /// Helper for <see cref="ChromeWindow"/>.
    /// </summary>
    public ChromeControlBackground GetControlBackground(bool isDialog)
    {
        return isDialog ? ChromeControlBackground.Default : ControlBackground;
    }

    /// <summary>
    /// Helper for <see cref="ChromeWindow"/>.
    /// </summary>
    public bool GetShowInTaskbar(bool isDialog)
    {
        return !isDialog || ShowDialogInTaskbar;
    }

    /// <summary>
    /// Helper for <see cref="ChromeWindow"/>.
    /// </summary>
    public IBrush? GetWindowBorder(bool isDialog)
    {
        if (isDialog && DialogAccentBorder)
        {
            return ChromeStyling.Global.AccentBrush;
        }

        return ChromeStyling.Global.WindowBorder;
    }

}
