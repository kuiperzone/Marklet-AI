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

using System.Text.Json;
using Avalonia.Media;

namespace KuiperZone.Marklet.PixieChrome.Settings;

/// <summary>
/// Serializable appearance settings accepted by <see cref="ChromeStyling"/>.
/// </summary>
public sealed class AppearanceSettings : SettingsBase, IEquatable<SettingsBase>
{
    /// <summary>
    /// Gets a global instance loaded on <see cref="ChromeApplication.Initialize"/>.
    /// </summary>
    public static AppearanceSettings Global { get; } = new();

    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    public ChromeTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets whether to prefer the "Fluent Dark" (black) over the default dark theme.
    /// </summary>
    public bool PreferFluentDark { get; set; }

    /// <summary>
    /// Gets or sets the application accent color.
    /// </summary>
    /// <remarks>
    /// A default 0 value will be interpreted as a default color or brush.
    /// </remarks>
    public uint AccentColor { get; set; }

    /// <summary>
    /// Gets or sets the application corner size.
    /// </summary>
    public CornerSize Corners { get; set; }

    /// <inheritdoc cref="SettingsBase.Reset"/>
    public override void Reset()
    {
        CopyFrom(new());
    }

    /// <inheritdoc cref="SettingsBase.Deserialize(string)"/>
    public override void Deserialize(string json)
    {
        CopyFrom(JsonSerializer.Deserialize(json, SettingsSerializer.Default.AppearanceSettings));
    }

    /// <inheritdoc cref="SettingsBase.Serialize"/>
    public override string? Serialize()
    {
        return JsonSerializer.Serialize(this, SettingsSerializer.Default.AppearanceSettings);
    }

    /// <inheritdoc cref="SettingsBase.TrimLegal"/>
    public override void TrimLegal()
    {
        Theme = Theme.TrimLegal();
        Corners = Corners.TrimLegal();
    }

    /// <inheritdoc cref="SettingsBase.Equals(SettingsBase?)"/>
    public override bool Equals(SettingsBase? other)
    {
        return other is AppearanceSettings s &&
            Theme == s.Theme &&
            PreferFluentDark == s.PreferFluentDark &&
            AccentColor == s.AccentColor &&
            Corners == s.Corners;
    }

    /// <summary>
    /// Gets the <see cref="AccentColor"/> as color with default value.
    /// </summary>
    public Color ToAccentColor()
    {
        return ToColor(AccentColor, ChromeBrushes.DefaultAccent.Color);
    }

    /// <summary>
    /// Copies setting from "other" to "this".
    /// </summary>
    public void CopyFrom(AppearanceSettings? other)
    {
        if (other != null)
        {
            Theme = other.Theme;
            PreferFluentDark = other.PreferFluentDark;
            AccentColor = other.AccentColor;
            Corners = other.Corners;
        }
    }

}
