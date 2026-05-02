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
using Avalonia.Platform.Storage;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Settings;

namespace KuiperZone.Marklet.Settings;

/// <summary>
/// Serializable "database" settings.
/// </summary>
public sealed class DatabaseSettings : SettingsBase, IEquatable<SettingsBase>
{
    /// <summary>
    /// Default file extension.
    /// </summary>
    public const string Extension = ".mkdb";

    /// <summary>
    /// Default filename.
    /// </summary>
    public const string DefaultName = "default" + Extension;

    /// <summary>
    /// Gets whether has filesystem permission.
    /// </summary>
    /// <remarks>
    /// Disable when running in sandbox without filesystem permissions.
    /// </remarks>
    public static readonly bool HasPermissions = ChromeApplication.Current.Host.HasFileSystemPermission;

    /// <summary>
    /// Gets the fully qualified default data filepath.
    /// </summary>
    public static readonly string DefaultPath = Path.Combine(ChromeApplication.Current.Host.DataDirectory, DefaultName);

    /// <summary>
    /// Gets a global instance loaded on <see cref="App.Initialize"/>.
    /// </summary>
    public static DatabaseSettings Global { get; } = new();

    /// <summary>
    /// Gets or sets whether to use <see cref="DefaultPath"/> or <see cref="CustomPath"/>.
    /// </summary>
    public bool UseCustom { get; set; }

    /// <summary>
    /// Gets or sets the custom data filepath.
    /// </summary>
    public string? CustomPath { get; set; }

    /// <summary>
    /// Returns a new instance of <see cref="FilePickerOpenOptions"/> suitable for use in an file dialog window.
    /// </summary>
    public static FilePickerOpenOptions GetOpenOptions(string? title = "Import DB")
    {
        var t0 = new FilePickerFileType("Marklet DB");
        t0.Patterns = ["*" + Extension];

        var opts = new FilePickerOpenOptions();
        opts.Title = title;
        opts.SuggestedFileType = t0;
        opts.FileTypeFilter = [t0];

        return opts;
    }

    /// <summary>
    /// Returns a new instance of <see cref="FilePickerSaveOptions"/> suitable for use in a file dialog window.
    /// </summary>
    public static FilePickerSaveOptions GetSaveOptions(string? title = "Export DB")
    {
        var t0 = new FilePickerFileType("Marklet DB");
        t0.Patterns = ["*" + Extension];

        var opts = new FilePickerSaveOptions();
        opts.Title = title;
        opts.SuggestedFileType = t0;
        opts.DefaultExtension = Extension;
        opts.FileTypeChoices = [t0, FilePickerFileTypes.All];

        return opts;
    }

    /// <summary>
    /// Gets the actual filepath to use.
    /// </summary>
    public string GetActualPath()
    {
        if (!HasPermissions || !UseCustom || string.IsNullOrEmpty(CustomPath))
        {
            return DefaultPath;
        }

        return CustomPath;
    }

    /// <summary>
    /// Gets whether <see cref="GetActualPath"/> is <see cref="DefaultPath"/>.
    /// </summary>
    public bool IsDefaultPath()
    {
        return GetActualPath() == DefaultPath;
    }

    /// <summary>
    /// Returns either: A. the "path" if it exists as a file, or B. the path with a default filename name if "path" exists as
    /// a directory. The "directory" part is given out.
    /// </summary>
    public static string? TryCustomPath(string? path, out string? directory)
    {
        path = path?.Trim();

        if (string.IsNullOrEmpty(path))
        {
            directory = null;
            return null;
        }

        if (path.EndsWith('/') || path.EndsWith('\\'))
        {
            directory = path;

            if (Directory.Exists(directory))
            {
                return Path.Combine(directory, DefaultName);
            }

            return null;
        }

        if (File.Exists(path))
        {
            directory = Path.GetDirectoryName(path);
            return path;
        }

        if (Directory.Exists(path))
        {
            directory = path;
            return Path.Combine(directory, DefaultName);
        }

        directory = Path.GetDirectoryName(path);

        if (Directory.Exists(directory))
        {
            return path;
        }

        return null;
    }

    /// <summary>
    /// Overload.
    /// </summary>
    public static string? TryCustomPath(string? path)
    {
        return TryCustomPath(path, out _);
    }

    /// <inheritdoc cref="SettingsBase.Reset"/>
    public override void Reset()
    {
        UseCustom = false;
        CustomPath = null;
    }

    /// <inheritdoc cref="SettingsBase.Deserialize(string)"/>
    public override void Deserialize(string json)
    {
        CopyFrom(JsonSerializer.Deserialize(json, AppSerializer.Default.DatabaseSettings));
    }

    /// <inheritdoc cref="SettingsBase.Serialize"/>
    public override string? Serialize()
    {
        return JsonSerializer.Serialize(this, AppSerializer.Default.DatabaseSettings);
    }

    /// <inheritdoc cref="SettingsBase.TrimLegal"/>
    public override void TrimLegal()
    {
        CustomPath = CustomPath?.Trim();

        if (string.IsNullOrEmpty(CustomPath))
        {
            CustomPath = null;
            UseCustom = false;
        }
    }

    /// <inheritdoc cref="SettingsBase.Equals(SettingsBase?)"/>
    public override bool Equals(SettingsBase? other)
    {
        return other is DatabaseSettings s &&
            UseCustom == s.UseCustom &&
            CustomPath == s.CustomPath;
    }

    /// <summary>
    /// Copies setting from "other" to "this".
    /// </summary>
    public void CopyFrom(DatabaseSettings? other)
    {
        if (other != null)
        {
            UseCustom = other.UseCustom;
            CustomPath = other.CustomPath;
        }
    }
}
