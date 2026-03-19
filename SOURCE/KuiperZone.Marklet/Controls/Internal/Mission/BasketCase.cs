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

using Avalonia.Threading;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Windows;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using Avalonia.Controls;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Creates plethora of visual objects and handles rebuild schedule for <see cref="BasketView"/>.
/// </summary>
internal sealed class BasketCase
{
    public static readonly TimeSpan ShortInterval = TimeSpan.FromMilliseconds(350);
    public static readonly TimeSpan LongInterval = TimeSpan.FromMilliseconds(750);

    private readonly DispatcherTimer _scheduleTimer = new();
    private bool _isEnabled;
    private bool _isSearching;

    public BasketCase(BasketView view, MainMission mission)
    {
        Kind = view.Kind;
        View = view;
        Mission = mission;

        // Create these first
        FolderHeader = new("Folders", false);
        FolderHeader.NewFolder?.Click += NewFolderHandler;

        RootHeader = new("History", true);
        FolderMore = new(false);
        FolderMore.ViewChangeRequired += (_, __) => ScheduleRebuild(ShortInterval);

        HistoryMore = new(true);
        HistoryMore.ViewChangeRequired += (_, __) => ScheduleRebuild(ShortInterval);

        Menu = new(this);
        Toolbar = new(this);
        Toolbar.SearchButton.Click += (_, __) => IsSearching = !IsSearching;

        SearchEditor.IsVisible = false;
        SearchEditor.HasBackButton = false;
        SearchEditor.HasMatchCaseButton = true;
        SearchEditor.HasMatchWordButton = true;
        SearchEditor.MaxLength = MemoryGarden.MaxMetaLength;
        SearchEditor.MaxLines = 1;
        SearchEditor.Watermark = "Search\u2026";
        SearchEditor.TextChanged += SearchedChangedHandler;
        SearchEditor.CaseOrWordChecked += SearchedChangedHandler;

        Menu.SearchItem.Click += (_, __) => IsSearching = !IsSearching;
        Menu.NewFolderItem.Click += NewFolderHandler;
        Menu.PruneItem.Click += (_, __) => ShowPruningWindow();
        Menu.EmptyItem.Click += (_, __) => ShowPurgeWindow();

        None.Text = "None";
        None.FontSize = ChromeFonts.SmallFontSize;
        None.Foreground = ChromeStyling.GrayForeground;
        None.TextAlignment = Avalonia.Media.TextAlignment.Center;

        // Timer active only when visible
        _scheduleTimer.Tick += ScheduleTickHandler;
    }

    public event EventHandler<EventArgs>? ScheduleTick;

    /// <summary>
    /// Gets the basket kind.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public BasketView View { get; }

    /// <summary>
    /// Gets the ultimate owning control.
    /// </summary>
    public MainMission Mission { get; }

    /// <summary>
    /// Gets the tool bar.
    /// </summary>
    public BasketToolbar Toolbar { get; }

    /// <summary>
    /// Gets the search editor.
    /// </summary>
    public TextEditor SearchEditor { get; } = new();

    /// <summary>
    /// Gets the folder header.
    /// </summary>
    public FolderHeader FolderHeader { get; }

    /// <summary>
    /// Gets the root history header.
    /// </summary>
    public FolderHeader RootHeader { get; }

    /// <summary>
    /// Gets the basket main menu.
    /// </summary>
    public BasketMenu Menu { get; }

    /// <summary>
    /// Gets the folder more (see all) bar.
    /// </summary>
    public MoreBar FolderMore { get; }

    /// <summary>
    /// Gets the root history more (see all) bar.
    /// </summary>
    public MoreBar HistoryMore { get; }

    public TextBlock None { get; } = new();

    /// <summary>
    /// Gets or sets the <see cref="ScheduleTick"/> timer.
    /// </summary>
    public bool IsEnabled
    {
        get { return _isEnabled; }

        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                _scheduleTimer.Stop();

                if (value)
                {
                    // Immediate on show
                    ScheduleTick?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether searching.
    /// </summary>
    public bool IsSearching
    {
        get { return _isSearching; }

        set
        {
            if (_isSearching != value)
            {
                _isSearching = value;
                FolderHeader.NewFolder?.IsEnabled = !value;
                Menu.NewFolderItem?.IsEnabled = !value;
                Toolbar.SearchButton.IsChecked = value;

                if (value)
                {
                    SearchEditor.IsVisible = true;
                    SearchEditor.Focus();
                }
                else
                {
                    ScheduleRebuild(ShortInterval);
                }
            }
        }
    }

    /// <summary>
    /// Schedules a call of <see cref="ScheduleTick"/>.
    /// </summary>
    public void ScheduleRebuild(TimeSpan interval)
    {
        if (_isEnabled)
        {
            _scheduleTimer.Interval = interval;
            _scheduleTimer.Restart();
        }
    }

    /// <summary>
    /// Schedules a call of <see cref="ScheduleTick"/>.
    /// </summary>
    public void ScheduleRebuild()
    {
        ScheduleRebuild(ShortInterval);
    }

    /// <summary>
    /// Gets the search options.
    /// </summary>
    public SearchOptions? GetSearchOptions()
    {
        if (_isSearching)
        {
            var opts = new SearchOptions(SearchEditor.Text);

            if (!SearchEditor.IsMatchCaseChecked)
            {
                opts.Flags |= SearchFlags.IgnoreCase;
            }

            if (SearchEditor.IsMatchWordChecked)
            {
                opts.Flags |= SearchFlags.Word;
            }

            return opts;
        }

        return null;
    }

    /// <summary>
    /// The <see cref="FolderView"/> compare method.
    /// </summary>
    public int FolderCompare(FolderView x, FolderView y)
    {
        if (FolderMore.IsAlphaFolderSort)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(x.FolderName, y.FolderName);
        }

        return y.Updated.CompareTo(x.Updated);
    }

    /// <summary>
    /// Shows pruning dialog window.
    /// </summary>
    public async void ShowPruningWindow()
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(ShowPruningWindow)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);

        var basket = View.Basket;
        var window = new PruneWindow(basket);
        await window.ShowDialog(Mission);

        if (window.IsPositiveResult)
        {
            basket.Prune(window.Options);
        }
    }

    /// <summary>
    /// Shows empty basket dialog window.
    /// </summary>
    public async void ShowPurgeWindow()
    {
        const string NSpace = $"{nameof(BasketToolbar)}.{nameof(ShowPurgeWindow)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked: " + Kind);

        var basket = View.Basket;
        var confirm = new ChromeDialog();
        confirm.Message = $"Empty {Kind} now?";
        confirm.Details = "All items will be permanently deleted.";
        confirm.Buttons = DialogButtons.Delete | DialogButtons.Cancel;

        if (await confirm.ShowDialog(Mission) == DialogButtons.Delete)
        {
            basket.DeleteAll();
        }
    }

    private async void NewFolderHandler(object? _, EventArgs __)
    {
        var window = new NewFolderWindow(View.Basket);
        await window.ShowDialog(Mission);

        if (window.IsPositiveResult)
        {
            View.Basket.NewEmptyFolder(window.FolderName);
            ScheduleRebuild(ShortInterval);
        }
    }

    private void SearchedChangedHandler(object? _, EventArgs __)
    {
        if (_isSearching)
        {
            ScheduleRebuild(LongInterval);
        }
    }

    private void ScheduleTickHandler(object? _, EventArgs __)
    {
        _scheduleTimer.Stop();

        if (SearchEditor.IsVisible != _isSearching)
        {
            SearchEditor.Clear();
            SearchEditor.IsVisible = _isSearching;
        }

        ScheduleTick?.Invoke(this, EventArgs.Empty);
    }
}