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
using KuiperZone.Marklet.Controls;
using Avalonia;

namespace KuiperZone.Marklet.Windows;

/// <summary>
/// Application main window.
/// </summary>
public partial class MainWindow : ChromeWindow
{
    private const double MissionMaxWidthF = 0.75;
    private const double PrompterMaxHeightF = 0.40;

    private static readonly ContentSettings ContentSettings = ContentSettings.Global;

    private readonly LightButton _pinButton;
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
        _missionColumn = outerHorzGrid.ColumnDefinitions[1];
        _pinButton = ChromeBar.LeftGroup.AddButton(Symbols.Keep, "Always on top");
        _pinButton.Classes.Add("accent-checked");
        _pinButton.CanToggle = true;
        _pinButton.Click += (_, __) => Topmost = !Topmost;

        bufferBar.Basket = mission.Basket;
        deckViewer.Garden = mission.Garden;
        bufferBar.BoxChanged += BasketChangedHandler;
        mission.BasketChanged += BasketChangedHandler;
        mission.ViewChanged += MissionViewChangedHandler;
        mission.NewClicked += MissionNewClickedHandler;

        _editorState = new(this);
        UpdateContentSettings();

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
        deckViewer.HandleKeyGesture(e);
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
        deckViewer.SelectionBrush = Styling.Accent50;
        deckViewer.LinkForeground = Styling.LinkForeground;
        deckViewer.LinkHoverBrush = Styling.LinkHover;
        deckViewer.QuoteDecor = Styling.AccentBrush;
        deckViewer.RuleLine = ChromeStyling.GrayForeground;
        deckViewer.FencedBackground = Styling.BackgroundLow;
        deckViewer.FencedBorder = ChromeStyling.GrayForeground;
        deckViewer.FencedCornerRadius = Styling.SmallCornerRadius;


        deckViewer.LeafCornerRadius = Styling.LargeCornerRadius;
        deckViewer.UserBackground = ContentSettings.ToUserBrush(Styling.AccentColor);

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
    protected override void OnOpenedIdle()
    {
        base.OnOpenedIdle();

        OpenDatabase(SqliteGardener.NewMemory());
        StubMessages.Populate(mission.Garden);
    }

    private async void OpenDatabase(IMemoryGardener gardener)
    {
        var garden = mission.Garden;
        bool upgrade = MemoryGarden.IsUpgradeRequired(gardener);

        if (upgrade && gardener.IsReadOnly)
        {
            await ChromeDialog.ShowDialog(this, "It is necessary to upgrade the underlying database. However, the location is read-only.");
            return;
        }
        else
        if (upgrade)
        {
            if (await ChromeDialog.ShowDialog(this, "It is necessary to upgrade the underlying database. If other applications are sharing the same file, close them now.",
                DialogButtons.Continue | DialogButtons.Cancel) != DialogButtons.Continue)
            {
                return;
            }
        }

        garden.OpenDatabase(gardener);
    }

    private void UpdateContentSettings()
    {
        var body = ContentSettings.BodyFont;
        deckViewer.Zoom.Default = ContentSettings.DefaultScale;
        deckViewer.ContentWidth = ContentSettings.Width;
        deckViewer.FontFamily = body.ToFamily();
        deckViewer.FontSizeCorrection = body.ToCorrection();

        deckViewer.UserBackground = ContentSettings.ToUserBrush(Styling.AccentColor);
        deckViewer.HeadingFamily = ContentSettings.HeadingFont.ToFamily();
        deckViewer.HeadingSizeCorrection = ContentSettings.HeadingFont.ToCorrection();
        deckViewer.HeadingForeground = ContentSettings.ToHeadingBrush();

        deckViewer.FencedForeground = ContentSettings.ToFencedBrush();
        deckViewer.DefaultWrapping = ContentSettings.DefaultFencedWrap;

        prompter.MaxWidth = Math.Min(ContentSettings.Width.ToPixels(), ContentWidth.Medium.ToPixels());
    }

    private void SubmitMessage(string? content)
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(SubmitMessage)}";

        prompter.Text = null;
        var current = mission.GetNew(true) ?? mission.Garden.Current;
        ConditionalDebug.WriteLine(NSpace, "Current or new: " + current?.Title);

        if (current != null)
        {
            current.Append(LeafKind.User, content);

            if (current.Garden == null)
            {
                ConditionalDebug.WriteLine(NSpace, "Insert new");
                mission.Garden.Insert(current);
                ConditionalDebug.ThrowIfNull(current.Garden);
            }

            deckViewer.IsBusy = true;
            deckViewer.ScrollToEnd();

            _stub.StartNext();
            _chunking?.StopStream();
            _chunking = null;
        }
    }

    private void BasketChangedHandler(object? _, BasketChangedEventArgs e)
    {
        const string NSpace = $"{nameof(MainWindow)}.{nameof(BasketChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"Kind: {bufferBar.Basket}");

        mission.Basket = e.Basket;
        bufferBar.Basket = e.Basket;
        prompter.IsEnabled = mission.CanReply;
    }

    private void MissionViewChangedHandler(object? sender, EventArgs e)
    {
        prompter.IsEnabled = mission.CanReply;

        if (mission.GetNew(false) == null)
        {
            overlay.Clear();
        }
    }

    private void MissionNewClickedHandler(object? sender, EventArgs e)
    {
        prompter.IsEnabled = mission.CanReply;
        var pending = mission.GetNew(false);

        if (pending != null)
        {
            prompter.FocusEditor();
            overlay.ShowPrompt(pending);
            return;
        }

        overlay.Clear();
    }

    private void StubChunkedHandler(object? _, EventArgs __)
    {
        var selected = mission.Garden.Current;

        if (selected != null)
        {
            deckViewer.IsBusy = false;
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

}