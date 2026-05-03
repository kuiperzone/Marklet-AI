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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Controls;
using Avalonia;
using Avalonia.Threading;

namespace KuiperZone.Marklet.Windows;

/// <summary>
/// Application main window.
/// </summary>
public partial class MainWindow : ChromeWindow
{
    private const double MissionMaxWidthF = 0.75;
    private const double PrompterMaxHeightF = 0.40;

    private static readonly ContentSettings ContentSettings = ContentSettings.Global;

    private readonly DispatcherTimer _sizeTimer = new();
    private readonly LightButton _searchButton;
    private readonly LightButton _pinButton;
    private readonly ColumnDefinition _missionColumn;
    private readonly EditorState _editorState;
    private readonly StubBot _stub = new();
    private GardenLeaf? _chunker;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MainWindow()
        : base(false)
    {
        const string NSpace = $"{nameof(MainWindow)}.constructor";
        Diag.WriteLine(NSpace, "Constructor");

        DataContext = ChromeStyling.Global;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        InitializeComponent();

        ChromeBar.Title = App.DisplayTitle;
        ChromeBar.FontFamily = AppFonts.VintageFamily;
        Title = ChromeApplication.Current.Name;

        _missionColumn = outerHorzGrid.ColumnDefinitions[1];

        _searchButton = ChromeBar.LeftGroup.AddButton(Symbols.Search, "Search");
        _searchButton.Click += (_, __) => mission.IsSearching = !mission.IsSearching;

        _pinButton = ChromeBar.RightGroup.AddButton(Symbols.Keep, "Always on top");
        _pinButton.Classes.Add("accent-checked");
        _pinButton.CanToggle = true;
        _pinButton.Click += (_, __) => Topmost = !Topmost;

        bufferBar.Basket = mission.Basket;
        bufferBar.Changed += BufferBarChangedHandler;

        navigator.Viewer = viewer;
        navigator.Margin = new(0.0, 0.0, ChromeSizes.TabPx, 0.0);

        _editorState = new(this);
        UpdateWindowSettings();
        UpdateContentSettings();

        zoomStatus.MinWidth = 5.0 * ChromeSizes.OneCh;
        AddStatusButton(zoomBar, Symbols.Replay).Click += (_, __) => viewer.Zoom.Reset();
        AddRepeatableStatusButton(zoomBar, Symbols.Add).Click += (_, __) => viewer.Zoom.Increment();
        AddRepeatableStatusButton(zoomBar, Symbols.Remove).Click += (_, __) => viewer.Zoom.Decrement();
        UpdateZoomStatus();

        mission.Changed += MissionChangedHandler;
        mission.NewClicked += MissionNewClickedHandler;
        viewer.Zoom.Changed += (_, __) => UpdateZoomStatus();
        prompter.EditWindowClick += EditWindowClickHandler;
        prompter.TextChanged += PromptTextChangedHandler;
        prompter.SubmitClick += (_, __) => SubmitMessage(prompter.Text);

        Settings.Changed += (_, __) => UpdateWindowSettings();
        ContentSettings.Changed += (_, __) => UpdateContentSettings();

        _stub.ChunkReceived += StubChunkedHandler;

        _sizeTimer.Interval = TimeSpan.FromSeconds(5.0);
        _sizeTimer.Tick += (_, __) => UpdateGardenSize();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        mission.HandleKeyGesture(e);
        navigator.HandleKeyGesture(e);
        viewer.HandleKeyGesture(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnResized(WindowResizedEventArgs e)
    {
        base.OnResized(e);

        _missionColumn.MaxWidth = MissionMaxWidthF * e.ClientSize.Width;
        prompter.MaxHeight = Math.Max(PrompterMaxHeightF * e.ClientSize.Height, 100.0);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnStylingChanged(bool init)
    {
        base.OnStylingChanged(init);

        // May as well do this programmatically
        viewer.SelectionBrush = Styling.Accent50;
        viewer.LinkForeground = Styling.LinkForeground;
        viewer.LinkHoverBrush = Styling.LinkHover;
        viewer.QuoteDecor = Styling.AccentBrush;
        viewer.RuleLine = ChromeStyling.GrayForeground;
        viewer.FencedBackground = Styling.BackgroundLow;
        viewer.FencedBorder = ChromeStyling.GrayForeground;
        viewer.FencedCornerRadius = Styling.SmallCornerRadius;

        viewer.LeafCornerRadius = Styling.LargeCornerRadius;
        viewer.UserBackground = ContentSettings.ToUserBrush(Styling.AccentColor);

        if (Equals(bufferBar.Background, mission.Background))
        {
            bufferBar.BorderBrush = Styling.BorderBrush;
        }
        else
        {
            bufferBar.BorderBrush = null;
        }

    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == TopmostProperty)
        {
            _pinButton.IsChecked = change.GetNewValue<bool>();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override async void OnOpenedIdle()
    {
        base.OnOpenedIdle();

        await GlobalGarden.Global.OpenAsync(this);

        GardenChanged();
        GlobalGarden.Global.Changed += (_, e) => GardenChanged(e);
    }

    private static LightButton AddStatusButton(LightBar bar, string content, string? tip = null)
    {
        var button = bar.AddButton(content, tip);
        button.FontSize = ChromeFonts.SmallFontSize;
        button.Margin = new(0.0);
        button.ContentPadding = new(1.0);
        return button;
    }

    private static LightButton AddRepeatableStatusButton(LightBar bar, string content, string? tip = null)
    {
        var button = AddStatusButton(bar, content, tip);
        button.RepeatInterval = 500;
        return button;
    }

    private void UpdateContentSettings()
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(UpdateContentSettings)}";
        Diag.WriteLine(NSpace, "Updating");

        var body = ContentSettings.BodyFont;
        viewer.Zoom.Default = ContentSettings.DefaultScale;
        viewer.ContentWidth = ContentSettings.Width;
        viewer.FontFamily = body.ToFamily();
        viewer.FontSizeCorrection = body.ToCorrection();

        viewer.UserBackground = ContentSettings.ToUserBrush(Styling.AccentColor);
        viewer.HeadingFamily = ContentSettings.HeadingFont.ToFamily();
        viewer.HeadingSizeCorrection = ContentSettings.HeadingFont.ToCorrection();
        viewer.HeadingForeground = ContentSettings.ToHeadingBrush();

        viewer.FencedForeground = ContentSettings.ToFencedBrush();
        viewer.DefaultWrapping = ContentSettings.DefaultFencedWrap;

        prompter.MaxWidth = Math.Min(ContentSettings.Width.ToPixels(), ContentWidth.Medium.ToPixels());
    }

    private void UpdateWindowSettings()
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(UpdateWindowSettings)}";
        Diag.WriteLine(NSpace, "Updating");
        mission.IsSearchButtonVisible = !Settings.IsChromeWindow;
    }

    private void UpdateGardenSize()
    {
        var provider = GlobalGarden.Global.Provider;

        if (provider != null)
        {
            var size = provider?.GetSize() ?? 0;
            gardenSize.Text = size > 0 ? Magverter.ToFriendlyBytes(size) : null;
            return;
        }

        gardenSize.Text = "Caching";
    }

    private void SubmitMessage(string? content)
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(SubmitMessage)}";

        prompter.Text = null;
        var focused = mission.GetNew(true) ?? GlobalGarden.Global.Focused;
        Diag.WriteLine(NSpace, "Current or new: " + focused?.Title);

        if (focused != null)
        {
            // TBD Temporary code
            bool reply = true;
            var format = LeafFormat.UserMessage;

            if (focused.Format == DeckFormat.Note && focused.Count == 0)
            {
                reply = false;
                format = LeafFormat.UserNote;
            }

            focused.Append(format, content);

            if (focused.Garden == null)
            {
                Diag.WriteLine(NSpace, "Insert new");
                GlobalGarden.Global.Insert(focused);
            }

            if (reply)
            {
                viewer.IsBusy = true;
                _stub.StartReply(content);
            }

            viewer.ScrollToEnd();
        }
    }

    private void UpdateZoomStatus()
    {
        zoomStatus.Text = viewer.Zoom.Scale + "%";
    }

    private async void GardenChanged(GardenChangedEventArgs? e = null)
    {
        _sizeTimer.Stop();

        var global = GlobalGarden.Global;
        var status = global.Status;

        if (status == GardenStatus.Readonly)
        {
            gardenStatus.Foreground = ChromeBrushes.WarningBrush;
            gardenStatus.Text = global.IsDefault() ? "DEFAULT: " + status.ToMessage() : status.ToMessage();
        }
        else
        if (status.IsOpen())
        {
            gardenStatus.Foreground = ChromeStyling.GrayForeground;
            gardenStatus.Text = global.IsDefault() ? "DEFAULT" : status.ToMessage();
        }
        else
        {
            gardenStatus.Text = status.ToMessage();
            gardenStatus.Foreground = ChromeBrushes.CriticalBrush;
        }

        if (e != null && e.Basket != BasketKind.None)
        {
            // Don't access disk frequently for basket changes
            _sizeTimer.Start();
            return;
        }

        overlay.Clear();
        mission.GetNew(true); // <- clear
        UpdateGardenSize();

        if (global.Status == GardenStatus.Lost)
        {
            await ChromeDialog.ShowDialog(this, "Database connection lost");
        }
    }

    private void MissionChangedHandler(object? _, EventArgs e)
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(MissionChangedHandler)}";
        Diag.WriteLine(NSpace, $"Kind: {mission.Basket}");

        bufferBar.Basket = mission.Basket;
        prompter.IsEnabled = mission.CanReply;

        if (mission.GetNew(false) == null)
        {
            overlay.Clear();
        }
    }

    private void BufferBarChangedHandler(object? _, EventArgs e)
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(BufferBarChangedHandler)}";
        Diag.WriteLine(NSpace, $"Kind: {bufferBar.Basket}");
        mission.Basket = bufferBar.Basket;
    }

    private void MissionNewClickedHandler(object? _, EventArgs __)
    {
        var pending = mission.GetNew(false);
        prompter.IsEnabled = mission.CanReply;

        if (pending != null && mission.CanReply)
        {
            prompter.FocusEditor();
            overlay.ShowPrompt(pending);
            return;
        }

        overlay.Clear();
    }

    private void StubChunkedHandler(object? _, EventArgs __)
    {
        var focused = GlobalGarden.Global.Focused;

        if (focused != null)
        {
            viewer.IsBusy = false;
            _chunker ??= focused.Append(LeafFormat.AssistantMessage, null, LeafFlags.Streaming);

            if (_stub.Chunk == null)
            {
                _chunker.StopStream();
                _chunker = null;
                return;
            }

            _chunker.AppendStream(_stub.Chunk);
        }
    }

    private void PromptTextChangedHandler(object? _, RoutedEventArgs __)
    {
        _editorState.Text = prompter.Text;
    }

    private async void EditWindowClickHandler(object? _, RoutedEventArgs __)
    {
        var window = new PromptWindow();

        _editorState.CopyTo(window);

        if (await window.ShowDialog<bool>(this))
        {
            SubmitMessage(window.Text);
        }

        // Do last
        _editorState.CopyFrom(window);
    }

}