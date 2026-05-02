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

using System.Reflection;
using Avalonia.Markup.Xaml;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Windows;

namespace KuiperZone.Marklet;

/// <summary>
/// Application class.
/// </summary>
public partial class App : ChromeApplication<MainWindow>
{
    static App()
    {
        var assembly = Assembly.GetAssembly(typeof(AppSettings));

        Version = assembly?.GetName()?.Version ??
            throw new InvalidOperationException("Failed to retrieve Assembly version");

        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ??
            throw new InvalidOperationException("Failed to retrieve Assembly Copyright");

        DisplayVersion = Version.ToString(2);

#if DEBUG
        IsDebug = true;
        DisplayVersion += " Debug";
#endif

#if PREVIEW
        IsPreview = true;
        DisplayVersion += " Preview";
#endif
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Single instance only</exception>
    public App()
        : base("zone.kuiper.marklet")
    {
        const string NSpace = $"{nameof(App)}.constructor";
        Diag.WriteLine(NSpace, $"AppId: {Host.AppId}");
        Diag.ThrowIfNullOrEmpty(Host.AppId);

        Name = "Marklet";
    }

    /// <summary>
    /// Gets the number of parts included in <see cref="Version"/>.
    /// </summary>
    public const int VersionParts = 2;

    /// <summary>
    /// Gets whether the application display title.
    /// </summary>
    public const string DisplayTitle = "M\u2219A\u2219R\u2219K\u2219L\u2219E\u2219T";

    /// <summary>
    /// Gets the application version from the <see cref="ChromeApplication"/> subclass assembly.
    /// </summary>
    public static Version Version { get; }

    /// <summary>
    /// Gets whether DEBUG is defined.
    /// </summary>
    public static bool IsDebug { get; }

    /// <summary>
    /// Gets whether PREVIEW is defined.
    /// </summary>
    public static bool IsPreview { get; }

    /// <summary>
    /// Gets the display version from the <see cref="ChromeApplication"/> subclass assembly.
    /// </summary>
    /// <remarks>
    /// This comprises <see cref="Version"/> with <see cref="VersionParts"/> elements, plus a possible version suffix.
    /// In order to access the suffix, the csproj should include: {AssemblyMetadata Include="VersionSuffix" Value="$(VersionSuffix)" /}
    /// </remarks>
    public static string DisplayVersion { get; }

    /// <summary>
    /// Gets the copyright statement from the <see cref="ChromeApplication"/> subclass assembly.
    /// </summary>
    public static string? Copyright { get; }

    /// <summary>
    /// Gets the web address.
    /// </summary>
    public static Uri WebUrl { get; } = new("https://kuiper.zone/marklet-ai/");

    /// <summary>
    /// Gets the public repository web address.
    /// </summary>
    public static Uri RepoUrl { get; } = new("https://github.com/kuiperzone/Marklet-AI");

    /// <summary>
    /// Override.
    /// </summary>
    public override void Initialize()
    {
        const string NSpace = $"{nameof(App)}.{nameof(Initialize)}";

        // LOAD FIRST
        Diag.WriteLine(NSpace, "Loading XAML");
        AvaloniaXamlLoader.Load(this);

        base.Initialize();

        Diag.WriteLine(NSpace, $"Reading: {nameof(AppSettings)}");
        AppSettings.Global.Read(Path.Combine(Host.ConfigDirectory, "app-settings.json"));

        Diag.WriteLine(NSpace, $"Reading: {nameof(ContentSettings)}");
        ContentSettings.Global.Read(Path.Combine(Host.ConfigDirectory, "content-settings.json"));

        Diag.WriteLine(NSpace, $"Reading: {nameof(DatabaseSettings)}");
        DatabaseSettings.Global.Read(Path.Combine(Host.ConfigDirectory, "data-settings.json"));

        // Expect config directory to exist in test
        Diag.ThrowIfNull(AppSettings.Global.SettingsPath);
        Diag.ThrowIfNull(ContentSettings.Global.SettingsPath);
        Diag.ThrowIfNull(DatabaseSettings.Global.SettingsPath);
    }

}