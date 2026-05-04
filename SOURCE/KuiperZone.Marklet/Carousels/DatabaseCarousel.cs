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

using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Carousels;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;
using Avalonia;
using System.Threading.Tasks;

namespace KuiperZone.Marklet.Carousels;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage{T}"/> which hosts <see cref="DatabaseSettings"/>.
/// </summary>
public sealed class DatabaseCarousel : CarouselPage<DatabaseSettings>
{
    private static IStorageFolder? s_suggestedLocation;
    private readonly PixieSelectableText _workingPath = new();
    private readonly PixieLightBar _buttons = new();

    private readonly PixieSwitch _customSwitch = new();
    private readonly PixieEditor _pathText = new();

    private readonly LightButton _importButton;
    private readonly LightButton _exportButton;
    private readonly LightButton _purgeButton;
    private readonly LightButton? _insertButton;
    private readonly LightButton _reopenButton;
    private readonly LightButton _applyButton;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DatabaseCarousel()
        : base(DatabaseSettings.Global)
    {
        Title = "Database";
        Symbol = Symbols.Database;

        var group = new PixieGroup();
        Children.Add(group);

        // CURRENT WORKING
        group.Children.Add(_workingPath);
        _workingPath.Header = "Working Database";
        _workingPath.LeftSymbol = Symbols.Database;
        _workingPath.RightButton.IsVisible = true;
        _workingPath.RightButton.Content = Symbols.ContentCopy;
        _workingPath.RightButton.Click += (_, __) => Window.CopyToClipboard(_workingPath.Text);

        group.Children.Add(_buttons);
        var bar = _buttons.Subject;
        _importButton = bar.AddButton("Import", "Import content from external file");
        _importButton.Classes.Add("regular-background");
        _importButton.Click += ImportClickHandler;

        _exportButton = bar.AddButton("Export", "Export content to external file");
        _exportButton.Classes.Add("regular-background");
        _exportButton.Click += ExportClickHandler;

        if (App.IsDebug || App.IsPreview)
        {
            // Development use only
            _insertButton = bar.AddButton("Insert Test", "Populate with test content (provided only in pre-release software)");
            _insertButton.Classes.Add("regular-background");
            _insertButton.Click += InsertClickHandler;
        }

        _purgeButton = bar.AddButton("Purge", "Purge working database of all content");
        _purgeButton.Classes.Add("critical-background");
        _purgeButton.Click += PurgeClickHandler;

        _reopenButton = _buttons.RightButton;
        _reopenButton.IsVisible = true;
        _reopenButton.Content = "Open";
        _reopenButton.Tip = "Open working database";
        _reopenButton.Click += OpenClickHandler;


        // MAIN
        group = new PixieGroup();
        group.TopTitle = "Database on Start";
        Children.Add(group);

        group.Children.Add(_customSwitch);
        _customSwitch.Title = "Use Custom";
        _customSwitch.Footer = "Use custom database path below, or default location if unchecked";
        _customSwitch.LeftSymbol = Symbols.Star;
        _customSwitch.ValueChanged += ValueChangedHandler;

        _pathText.Header = "Filepath";
        _pathText.Footer = "";
        _pathText.Subject.AcceptsTab = false;
        _pathText.Subject.AcceptsReturn = false;
        _pathText.Subject.HasCopyButton = true;
        _pathText.IsEnabled = _customSwitch.IsChecked;
        _pathText.Subject.MaxLines = 1;
        _pathText.Subject.MaxLength = 2048;
        _pathText.Subject.Width = double.NaN;
        _pathText.Subject.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        _pathText.RightButton.IsVisible = true;
        _pathText.RightButton.Content = "Browse";
        _pathText.RightButton.Click += BrowserPathHandler;
        _pathText.ValueChanged += ValueChangedHandler;
        group.Children.Add(_pathText);

        var apply = new PixieControl<LightButton>();
        _applyButton = apply.Subject;
        _applyButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _applyButton.Content = "Save Change";
        _applyButton.Classes.Add("accent-background");
        _applyButton.Click += ApplyClickHandler;

        apply.Footer = "Specify a custom path for the database on application start. This could, for example, be on an encrypted " +
            "drive. Where a directory is given, a default filename is used. A location on a NAS is not recommended.";

        if (!DatabaseSettings.HasPermissions)
        {
            // Disable under flatpak
            group.IsEnabled = false;
            group.TopFooter = "Not available. Application must have filesystem permissions to set a custom path.";
        }

        group.Children.Add(apply);

        // RESET
        group = CreateResetGroup();
        Children.Add(group);
        group.IsEnabled = DatabaseSettings.HasPermissions;
    }

    /// <summary>
    /// Convenience.
    /// </summary>
    private static MemoryGarden GlobalGarden { get; } = Shared.GlobalGarden.Global;

    /// <summary>
    /// Convenience.
    /// </summary>
    private static string? WorkingSource
    {
        get { return GlobalGarden.Provider?.Source; }
    }

    private bool IsChangePending
    {
        get
        {
            var temp = new DatabaseSettings();
            UpdateSettings(temp);
            return !Settings.Equals(temp);
        }
    }

    /// <summary>
    /// Convenience.
    /// </summary>
    private Window? Window
    {
        get
        {
            // Just need any visual source
            return TopLevel.GetTopLevel(_pathText) as Window;
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        Apply();

        if (WorkingSource != Settings.GetActualPath())
        {
            _= RetryAsync();
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateControls(DatabaseSettings settings)
    {
        const string NSpace = $"{nameof(DatabaseCarousel)}.{nameof(UpdateControls)}";
        Diag.WriteLine(NSpace, "Update controls from settings");

        if (DatabaseSettings.HasPermissions)
        {
            _customSwitch.IsChecked = settings.UseCustom;
            _pathText.Subject.Text = settings.CustomPath;
        }

        UpdateWorking();
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateSettings(DatabaseSettings settings)
    {
        const string NSpace = $"{nameof(DatabaseCarousel)}.{nameof(UpdateSettings)}";
        Diag.WriteLine(NSpace, "Update setting from controls");

        if (DatabaseSettings.HasPermissions)
        {
            settings.UseCustom = _customSwitch.IsChecked;
            settings.CustomPath = _pathText.Subject.Text?.Trim();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void EnableControls()
    {
        const string NSpace = $"{nameof(DatabaseCarousel)}.{nameof(EnableControls)}";

        bool allow = !_customSwitch.IsChecked || DatabaseSettings.TryCustomPath(_pathText.Subject.Text) != null;
        Diag.WriteLine(NSpace, "Allow: " + allow);

        Diag.WriteLine(NSpace, "Changing: " + IsChangePending);
        Diag.WriteLine(NSpace, "Null provider: " + GlobalGarden.Provider == null);
        _applyButton.IsEnabled = allow && IsChangePending;
    }

    private void UpdateWorking()
    {
        const string NSpace = $"{nameof(DatabaseCarousel)}.{nameof(UpdateWorking)}";
        Diag.WriteLine(NSpace, "Update current info");

        var path = Settings.GetActualPath();
        _workingPath.Text = path;

        if (GlobalGarden.Provider?.Source == path)
        {
            var status = GlobalGarden.Status;

            if (status.IsOpen())
            {
                _workingPath.Footer = status.ToMessage();
            }
            else
            {
                _workingPath.Footer = "FAILURE: " + status.ToMessage();
            }
        }
        else
        {
            _workingPath.Footer = "Pending...";
        }

        _purgeButton.IsEnabled = File.Exists(path);

        if (GlobalGarden.IsEphemeral || GlobalGarden.Provider?.Source != path)
        {
            _reopenButton.Classes.Clear();
            _reopenButton.Classes.Add("accent-background");
            _reopenButton.IsEnabled = true;
        }
        else
        {
            _reopenButton.Classes.Clear();
            _reopenButton.Classes.Add("regular-background");
            _reopenButton.IsEnabled = false;
        }
    }

    private async void BrowserPathHandler(object? _, EventArgs __)
    {
        try
        {
            var provider = Window!.GetStorageProvider();
            s_suggestedLocation ??= await provider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

            var opts = new FolderPickerOpenOptions();
            opts.Title = "Select Directory";
            opts.SuggestedStartLocation = s_suggestedLocation;

            var result = await provider.OpenFolderPickerAsync(opts);

            if (result.Count == 1)
            {
                var path = result[0]?.TryGetLocalPath();

                if (path != null)
                {
                    _pathText.Subject.Text = path;
                    s_suggestedLocation = result[0];
                }
            }
        }
        catch (Exception e)
        {
            await ChromeDialog.ShowDialog(Window, e);
        }
    }

    private void ValueChangedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(DatabaseCarousel)}.{nameof(ValueChangedHandler)}";
        Diag.WriteLine(NSpace, "Changing");

        if (!DatabaseSettings.HasPermissions)
        {
            // Never change
            return;
        }

        var isCustom = _customSwitch.IsChecked;

        _pathText.Footer = "";

        if (isCustom)
        {
            var path = DatabaseSettings.TryCustomPath(_pathText.Subject.Text);

            if (path == null)
            {
                _pathText.Footer = "Directory not found";
            }
            else
            if (path != _pathText.Subject.Text)
            {
                _pathText.Footer = path;
            }
        }

        if (_pathText.IsEnabled != isCustom)
        {
            _pathText.IsEnabled = isCustom;

            if (isCustom)
            {
                _pathText.Focus();
            }
        }

        EnableControls();
    }

    private void ApplyClickHandler(object? _, EventArgs __)
    {
        _pathText.Subject.Text = DatabaseSettings.TryCustomPath(_pathText.Subject.Text);
        OnValueChanged(true);
        UpdateWorking();
    }

    private async Task RetryAsync()
    {
        // We need a window to show possible dialog windows
        var visual = Window;

        if (visual != null)
        {
            var dialog = new ChromeDialog();
            dialog.Message = Settings.IsDefaultPath() ? "Open DEFAULT database now?" : "Open custom database now?";
            dialog.Details = "If you click No, it will be opened on application restart.";
            dialog.Buttons = DialogButtons.Yes | DialogButtons.No;

            if (await dialog.ShowDialog(visual) == DialogButtons.Yes)
            {
                if (await GlobalGarden.OpenAsync(visual))
                {
                    UpdateWorking();
                }
            }
        }
    }

    private async void PurgeClickHandler(object? _, EventArgs __)
    {
        var confirm = new ChromeDialog();
        confirm.Message = $"Purge database now?";
        confirm.Details = "All content will be permanently deleted. This cannot be undone.";
        confirm.Buttons = DialogButtons.DeleteAll | DialogButtons.Cancel;

        if (await confirm.ShowDialog(Window) == DialogButtons.DeleteAll)
        {
            var flags = GlobalGarden.Provider?.Flags ?? ProviderFlags.WalNormal;
            GlobalGarden.Close();

            var path = Settings.GetActualPath();
            var provider = new SqliteProvider(path, flags);

            if (!MemoryGarden.Purge(provider))
            {
                await ChromeDialog.ShowDialog(Window, "Purge failed");
            }

            GlobalGarden.Open(provider);
            UpdateWorking();
        }
    }

    private async void ImportClickHandler(object? _, EventArgs __)
    {
        await ChromeDialog.ShowDialog(Window, "Not implemented (yet)");
    }

    private async void ExportClickHandler(object? _, EventArgs __)
    {
        await ChromeDialog.ShowDialog(Window, "Not implemented (yet)");
    }

    private async void OpenClickHandler(object? _, EventArgs __)
    {
        if (await GlobalGarden.OpenAsync(Window))
        {
            UpdateWorking();
        }
    }

    private async void InsertClickHandler(object? _, EventArgs __)
    {
        // Insert test messages
        var dialog = new ChromeDialog();
        dialog.Message = "Please wait...";
        dialog.Details = "This is a blocking operation";
        dialog.Buttons = DialogButtons.None;

        // This will block for large inserts
        dialog.Opened += (_, __) => DispatcherTimer.RunOnce(() => { StubBot.InsertTest(GlobalGarden); dialog.Close(); },
            TimeSpan.FromMilliseconds(500), DispatcherPriority.ContextIdle);

        await dialog.ShowDialog(Window);
        UpdateWorking();
    }
}