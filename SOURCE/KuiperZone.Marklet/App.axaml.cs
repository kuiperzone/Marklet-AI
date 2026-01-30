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

using System.Reflection;
using Avalonia.Markup.Xaml;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.Tooling;

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

        Copyright = assembly?.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ??
            throw new InvalidOperationException("Failed to retrieve Assembly version");

        // Settings must read Version.
        // Assign these in a constructor.
        AppSettings = AppSettings.Global;
        ContentSettings = ContentSettings.Global;
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Single instance only</exception>
    public App()
        : base("zone.kuiper.marklet")
    {
        const string NSpace = $"{nameof(App)}.constructor";
        ConditionalDebug.WriteLine(NSpace, $"AppId: {Host.AppId}");
        ConditionalDebug.ThrowIfNullOrEmpty(Host.AppId);

        Name = "Marklet";
    }

    /// <summary>
    /// Gets whether the application display title.
    /// </summary>
    public const string DisplayTitle = "M\u2219A\u2219R\u2219K\u2219L\u2219E\u2219T";

    /// <summary>
    /// Gets the number of parts included in <see cref="Version"/>.
    /// </summary>
    public const int VersionParts = 2;

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public static Version Version { get; }

    /// <summary>
    /// Gets the copyright statement.
    /// </summary>
    public static string Copyright { get; }

    /// <summary>
    /// Gets the project website name.
    /// </summary>
    public static string? WebName { get; }

    /// <summary>
    /// Gets the web address associated with <see cref="WebName"/>.
    /// </summary>
    public static Uri WebUrl { get; } = new("https://kuiper.zone/marklet-ai/");

    /// <summary>
    /// Gets the public repository web address.
    /// </summary>
    public static Uri RepoUrl { get; } = new("https://github.com/kuiperzone/Marklet-AI");

    /// <summary>
    /// Gets the public social web address.
    /// </summary>
    public static Uri XUrl { get; } = new("https://x.com/kuiperzone");

    /// <summary>
    /// Gets the global <see cref="AppSettings"/> instance.
    /// </summary>
    public static readonly AppSettings AppSettings;

    /// <summary>
    /// Gets the global <see cref="ContentSettings"/> instance.
    /// </summary>
    public static readonly ContentSettings ContentSettings;

    /// <summary>
    /// Override.
    /// </summary>
    public override void Initialize()
    {
        const string NSpace = $"{nameof(App)}.{nameof(Initialize)}";

        // LOAD FIRST
        ConditionalDebug.WriteLine(NSpace, "Loading XAML");
        AvaloniaXamlLoader.Load(this);

        base.Initialize();

        ConditionalDebug.WriteLine(NSpace, $"Reading: {nameof(WindowSettings)}");
        AppSettings.Read(Path.Combine(Host.ConfigDirectory, "app-settings.json"));

        ConditionalDebug.WriteLine(NSpace, $"Reading: {nameof(AppSettings)}");
        ContentSettings.Read(Path.Combine(Host.ConfigDirectory, "content-settings.json"));

        // Expect config directory to exist in test
        ConditionalDebug.ThrowIfNull(AppSettings.SettingsPath);
        ConditionalDebug.ThrowIfNull(ContentSettings.SettingsPath);
    }
}