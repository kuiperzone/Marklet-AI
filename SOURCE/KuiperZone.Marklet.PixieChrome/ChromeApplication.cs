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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Application base class.
/// </summary>
/// <remarks>
/// The <see cref="Initialize"/> method reads configuration from <see cref="ApplicationHost.ConfigDirectory"/> and
/// initializes <see cref="ChromeStyling.Global"/>. Assumes IClassicDesktopStyleApplicationLifetime.
/// </remarks>
public class ChromeApplication : Application
{
    /// <summary>
    /// Subclass to provide "appId"in reverse DNS form, i.e. "zone.kuiper.marklet".
    /// </summary>
    /// <exception cref="ArgumentException">appId</exception>
    protected ChromeApplication(string appId)
    {
        Host = new(appId);
    }

    /// <summary>
    /// Gets the initialized <see cref="ChromeApplication"/> instance or throws.
    /// </summary>
    /// <remarks>
    /// Can be called only after <see cref="Initialize"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Application not initialized</exception>
    public static new ChromeApplication Current
    {
        get { return (ChromeApplication)(Application.Current ?? throw new InvalidOperationException("Application not initialized")); }
    }

    /// <summary>
    /// Gets the main window.
    /// </summary>
    public static TopLevel? MainWindow
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the currently active window.
    /// </summary>
    public static TopLevel? ActiveWindow
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (var item in desktop.Windows)
                {
                    if (item.IsActive)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the global <see cref="AppearanceSettings"/> instance.
    /// </summary>
    /// <remarks>
    /// Data is valid on construction, but updated by <see cref="Initialize"/>.
    /// </remarks>
    public static readonly AppearanceSettings AppearanceSettings = AppearanceSettings.Global;

    /// <summary>
    /// Gets the global <see cref="WindowSettings"/> instance.
    /// </summary>
    /// <remarks>
    /// Data is valid on construction, but updated by <see cref="Initialize"/>.
    /// </remarks>
    public static readonly WindowSettings WindowSettings = WindowSettings.Global;

    /// <summary>
    /// Gets whether <see cref="Initialize"/> has been called.
    /// </summary>
    public static bool IsInitalized { get; private set; }

    /// <summary>
    /// Gets application host information.
    /// </summary>
    /// <remarks>
    /// Instance is not fully initialized until <see cref="Initialize"/> is called.
    /// </remarks>
    public ApplicationHost Host { get; }

    /// <summary>
    /// Initializes <see cref="Host"/> and reads configuration from <see cref="ApplicationHost.ConfigDirectory"/> and
    /// initializes <see cref="ChromeStyling.Global"/>.
    /// </summary>
    /// <remarks>
    /// The application should override, load "App.axaml" and then call this base method in that order.
    /// </remarks>
    public override void Initialize()
    {
        const string NSpace = $"{nameof(ChromeApplication)}.{nameof(Initialize)}";

        ConditionalDebug.WriteLine(NSpace, $"Initializing: {nameof(ApplicationHost)}");
        Host.Initialize();

        // CONFIGURATION
        ConditionalDebug.WriteLine(NSpace, $"Reading: {nameof(AppearanceSettings)}");
        AppearanceSettings.Read(Path.Combine(Host.ConfigDirectory, "appearance-settings.json"));

        ConditionalDebug.WriteLine(NSpace, $"Reading: {nameof(WindowSettings)}");
        WindowSettings.Read(Path.Combine(Host.ConfigDirectory, "window-settings.json"));

        // Expect config directory to exist in test
        ConditionalDebug.ThrowIfNull(AppearanceSettings.SettingsPath);
        ConditionalDebug.ThrowIfNull(WindowSettings.SettingsPath);

        // STYLING
        ConditionalDebug.WriteLine(NSpace, $"Initializing: {nameof(ChromeStyling)}");
        ChromeStyling.Global.Initialize(this, AppearanceSettings);

        IsInitalized = true;
    }
}
