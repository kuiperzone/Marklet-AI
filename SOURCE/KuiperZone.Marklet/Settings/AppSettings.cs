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
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Settings;

/// <summary>
/// Application settings.
/// </summary>
public sealed class AppSettings : SettingsBase, IEquatable<SettingsBase>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AppSettings()
    {
        FileVersion = App.Version.ToString(App.VersionParts);
    }

    /// <summary>
    /// Gets a global instance loaded on <see cref="App.Initialize"/>.
    /// </summary>
    public static AppSettings Global { get; } = new();

    /// <summary>
    /// Gets the version when as read, written or constructed.
    /// </summary>
    public string FileVersion { get; internal set; }

    /// <inheritdoc cref="SettingsBase.Reset"/>
    public override void Reset()
    {
        CopyFrom(new());
    }

    /// <inheritdoc cref="SettingsBase.Deserialize(string)"/>
    public override void Deserialize(string json)
    {
        CopyFrom(JsonSerializer.Deserialize(json, AppSerializer.Default.AppSettings));
    }

    /// <inheritdoc cref="SettingsBase.Serialize"/>
    public override string? Serialize()
    {
        FileVersion = App.Version.ToString(App.VersionParts);
        return JsonSerializer.Serialize(this, AppSerializer.Default.AppSettings);
    }

    /// <inheritdoc cref="SettingsBase.TrimLegal"/>
    public override void TrimLegal()
    {
    }

    /// <inheritdoc cref="SettingsBase.Equals(SettingsBase?)"/>
    public override bool Equals(SettingsBase? other)
    {
        return other is AppSettings s &&
            FileVersion == s.FileVersion;
    }

    /// <summary>
    /// Copies setting from "other" to "this".
    /// </summary>
    public void CopyFrom(AppSettings? other)
    {
        if (other != null)
        {
            FileVersion = other.FileVersion;
        }
    }

    /// <summary>
    /// Returns 0 if <see cref="FileVersion"/> and <see cref="App.Version"/> are equal, or +1 if <see
    /// cref="App.Version"/> is later (upgrade), or -1 if <see cref="FileVersion"/> is later (downgrade)
    /// </summary>
    public int CompareVersion()
    {
        try
        {
            return App.Version.CompareTo(new Version(FileVersion));
        }
        catch (Exception e)
        {
            ConditionalDebug.Fail(e.ToString());
            return +1;
        }
    }
}
