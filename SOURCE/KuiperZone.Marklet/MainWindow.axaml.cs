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
using Avalonia.Input;
using Avalonia.Interactivity;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Controls;
using Avalonia;

namespace KuiperZone.Marklet;

/// <summary>
/// Application main window.
/// </summary>
public partial class MainWindow : ChromeWindow
{
    private const double MissionNominalWidth = 300.0;
    private const double MissionResetWidth = 200.0;
    private const double MissionMaxWidthF = 0.75;
    private const double PrompterMaxHeightF = 0.40;

    private static readonly ContentSettings ContentSettings = ContentSettings.Global;

    private readonly LightButton _pinButton;
    private readonly MemoryGarden _memoryGarden = new(new SqliteGardener());
    private readonly ColumnDefinition _missionColumn;
    private readonly EditorState _editorState;
    private readonly StubMessages _stub = new();
    private GardenLeaf? _chunking;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MainWindow()
        : base(false)
    {
        const string NSpace = $"{nameof(MainWindow)}.constructor";
        ConditionalDebug.WriteLine(NSpace, "Constructor");

        DataContext = ChromeStyling.Global;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        InitializeComponent();

        Title = App.DisplayTitle;
        ChromeBar.LeftGroup.Add(Symbols.Menu, new ContextMenu(), "Menu");

        _pinButton = ChromeBar.RightGroup.Add(Symbols.Keep, "Always on top");
        _pinButton.CanToggle = true;
        _pinButton.Click += (_, __) => Topmost = !Topmost;

        // Initialize complex GardenShed column behaviour
        _missionColumn = outerHorzGrid.ColumnDefinitions[0];
        _missionColumn.MinWidth = mission.MinWidth;
        _missionColumn.Width = new(MissionNominalWidth, GridUnitType.Pixel);
        mission.CurrentBinChanged += MissionBinChangedHandler;
        mission.SettingsClick += SettingsClickHandler;
        mission.AboutClick += AboutClickHandler;
#if DEBUG
        mission.InspectClick += InspectClickHandler;
#endif

        _editorState = new(this);
        UpdateContentSettings();

        prompter.Margin = ContentViewer.HorizontalOffset;
        prompter.EditWindowClick += EditWindowClickHandler;
        prompter.TextChanged += PromptTextChangedHandler;
        prompter.SubmitClick += (_, __) => SubmitMessage(prompter.Text);

        ContentSettings.Changed += (_, __) => UpdateContentSettings();

        _stub.ChunkReceived += StubChunkedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        contentViewer.HandleKey(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnResized(WindowResizedEventArgs e)
    {
        base.OnResized(e);

        _missionColumn.MaxWidth = e.ClientSize.Width * MissionMaxWidthF;
        prompter.MaxHeight = Math.Max(PrompterMaxHeightF * e.ClientSize.Height, 100.0);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnStylingChanged(bool init)
    {
        base.OnStylingChanged(init);

        // May as well do this programmatically
        contentViewer.SelectionBrush = Styling.SemiAccent;
        contentViewer.LinkForeground = Styling.LinkForeground;
        contentViewer.LinkHoverBrush = Styling.LinkHover;
        contentViewer.QuoteDecor = Styling.AccentBrush;
        contentViewer.RuleLine = ChromeStyling.ForegroundGray;
        contentViewer.FencedBackground = Styling.BackgroundLow;
        contentViewer.FencedBorder = ChromeStyling.ForegroundGray;
        contentViewer.FencedCornerRadius = Styling.SmallCornerRadius;
        contentViewer.LeafCornerRadius = Styling.LargeCornerRadius;

        // Needed to follow changing accent color
        contentViewer.UserBackground = ContentSettings.ToUserBrush(Styling.AccentColor);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpenedIdle()
    {
        base.OnOpenedIdle();

        _memoryGarden.Open();

        // TBD Temp populate
        StubMessages.Populate(_memoryGarden);

        // Seem like this must come after Populate()
        mission.Garden = _memoryGarden;
        contentViewer.Garden = _memoryGarden;
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

    private void UpdateContentSettings()
    {
        contentViewer.Zoom.Default = ContentSettings.DefaultScale;
        contentViewer.ContentWidth = ContentSettings.Width;
        contentViewer.FontFamily = ContentSettings.BodyFont.ToFamily();
        contentViewer.FontSizeCorrection = ContentSettings.BodyFont.ToCorrection();

        contentViewer.UserBackground = ContentSettings.ToUserBrush(Styling.AccentColor);
        contentViewer.HeadingFamily = ContentSettings.HeadingFont.ToFamily();
        contentViewer.HeadingSizeCorrection = ContentSettings.HeadingFont.ToCorrection();
        contentViewer.HeadingForeground = ContentSettings.ToHeadingBrush();

        contentViewer.FencedForeground = ContentSettings.ToFencedBrush();
        contentViewer.DefaultWrapping = ContentSettings.DefaultFencedWrap;

        prompter.MaxWidth = Math.Min(ContentSettings.Width.ToPixels(), ContentWidth.Medium.ToPixels());
    }

    private void SubmitMessage(string? content)
    {
        prompter.Text = null;
        var selected = _memoryGarden.Selected;

        if (selected != null)
        {
            selected.Append(LeafKind.User, content);
            contentViewer.IsBusy = true;
            contentViewer.ScrollToEnd();

            _stub.StartNext();
            _chunking?.StopStream();
            _chunking = null;
        }
    }

    private void MissionBinChangedHandler(object? _, EventArgs __)
    {
        if (_missionColumn.ActualWidth < MissionResetWidth)
        {
            _missionColumn.Width = new(MissionNominalWidth, GridUnitType.Pixel);
        }
    }

    private void StubChunkedHandler(object? _, EventArgs __)
    {
        var selected = _memoryGarden.Selected;

        if (selected != null)
        {
            contentViewer.IsBusy = false;
            _chunking ??= selected.AppendStream(LeafKind.Assistant);

            if (_stub.Chunk == null)
            {
                _chunking.StopStream();
                _chunking = null;
                return;
            }

            _chunking.AppendStream(_stub.Chunk);
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

    private async void SettingsClickHandler(object? _, EventArgs __)
    {
        var window = new AppSettingsWindow();
        await window.ShowDialog(this);
    }

    private async void AboutClickHandler(object? _, EventArgs __)
    {
        //var window = new DevelopmentWindow();
        var window = new AboutWindow();
        await window.ShowDialog(this);
    }

#if DEBUG
    private async void InspectClickHandler(object? _, EventArgs __)
    {
        var window = new InspectWindow();
        await window.ShowDialog(this);
    }
#endif
}