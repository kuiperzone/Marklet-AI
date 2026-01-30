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

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Extends <see cref="ChromeApplication"/> to manage the MainWindow of type T.
/// </summary>
public class ChromeApplication<T> : ChromeApplication where T : Window, new()
{
    private T? _mainWindow;
    private bool _sizedChanged;
    private readonly DispatcherTimer _sizeTimer = new();

    /// <summary>
    /// Subclass to provide "appId"in reverse DNS form, i.e. "zone.kuiper.marklet".
    /// </summary>
    /// <exception cref="ArgumentException">appId</exception>
    protected ChromeApplication(string appId)
        : base(appId)
    {
        _sizeTimer.Interval = TimeSpan.FromMilliseconds(1000);
        _sizeTimer.Tick += TimerTickHandler;
    }

    /// <summary>
    /// Gets the global <see cref="AppearanceSettings"/> instance.
    /// </summary>
    /// <remarks>
    /// Data is valid on construction, but updated by <see cref="Initialize"/>.
    /// </remarks>
    public static readonly WindowPersistence MainPersistence = new();

    /// <summary>
    /// Gets or sets whether to manage MainWindow persistence, i.e. size and state.
    /// </summary>
    public bool IsMainWindowPersistenceEnabled { get; set; } = true;

    /// <summary>
    /// Extends to read <see cref="MainPersistence"/> which is applied to the MainWindow of type T.
    /// </summary>
    public override void Initialize()
    {
        const string NSpace = $"{nameof(ChromeApplication)}.{nameof(Initialize)}";

        base.Initialize();

        ConditionalDebug.WriteLine(NSpace, $"Reading: {nameof(MainPersistence)}");
        MainPersistence.Read(Path.Combine(Host.ConfigDirectory, "main-window.json"));

        // Expect config directory to exist in test
        ConditionalDebug.ThrowIfNull(MainPersistence.SettingsPath);
    }

    /// <summary>
    /// Override initialization.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        const string NSpace = $"{nameof(ChromeApplication)}.{nameof(OnFrameworkInitializationCompleted)}";

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ConditionalDebug.WriteLine(NSpace, "Application is desktop");
            _mainWindow = new();

            desktop.MainWindow = _mainWindow;

            if (IsMainWindowPersistenceEnabled)
            {
                MainPersistence.SetWindow(_mainWindow);
            }

            _mainWindow.SizeChanged += MainWindowSizeChangedHandler;
            _sizeTimer.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void MainWindowSizeChangedHandler(object? _, SizeChangedEventArgs e)
    {
        if (IsMainWindowPersistenceEnabled)
        {
            // Delayed write to file
            _sizedChanged = true;
            _sizeTimer.Restart();
        }
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(ChromeApplication)}.{nameof(TimerTickHandler)}";

        try
        {
            if (IsMainWindowPersistenceEnabled && _sizedChanged && _mainWindow != null)
            {
                ConditionalDebug.WriteLine(NSpace, $"Writing persistence to: {MainPersistence.SettingsPath}");
                MainPersistence.CopyFrom(_mainWindow);
                MainPersistence.Write();
            }
        }
        finally
        {
            _sizedChanged = false;
        }
    }

}