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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using Avalonia.Input;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using Avalonia;
using Avalonia.Media;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Control housing local search (find) within the currently rendered <see cref="GardenDeck"/>.
/// </summary>
public class DeckNavigator : Border
{
    /// <summary>
    /// Defines minimum height.
    /// </summary>
    public const double MinimumHeight = ChromeFonts.DefaultLineHeight * 2.0;

    private const int EditorColumn = 0;
    private const int InfoColumn = 1;
    private const int ButtonColumn = 2;

    private static readonly ChromeStyling Styling = ChromeStyling.Global;
    private readonly Grid _grid = new();
    private readonly LightBar _buttons = new();
    private readonly TextEditor _editor = new();
    private readonly TextBlock _info = new();
    private readonly LightButton _toggle;

    private bool _isExpanded;
    private DeckViewer? _viewer;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DeckNavigator()
    {
        Child = _grid;
        Padding = ChromeSizes.StandardPadding;

        // Not expanded on created
        Diag.ThrowIfTrue(_isExpanded);
        _grid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        _grid.MinHeight = ChromeFonts.DefaultLineHeight * 2.0;

        _grid.ColumnDefinitions.Add(new(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new(GridLength.Auto));

        Grid.SetColumn(_editor, EditorColumn);
        _grid.Children.Add(_editor);
        _editor.IsVisible = _isExpanded;
        _editor.MaxLines = 1;
        _editor.MaxLength = SearchOptions.MaxLength;
        _editor.Width = ChromeSizes.OneCh * 30.0;
        _editor.Watermark = "Press enter to find\u2026";
        _editor.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _editor.HasBackButton = false;
        _editor.HasMatchCaseButton = true;
        _editor.HasMatchWordButton = true;
        _editor.Submitted += SearchSubmittedHandler;
        _editor.TextChanging += SearchChangingHandler;

        Grid.SetColumn(_info, InfoColumn);
        _grid.Children.Add(_info);
        _info.IsVisible = _isExpanded;
        _info.FontSize = ChromeFonts.SmallFontSize;
        _info.Margin = new(ChromeSizes.OneCh, 0.0);
        _info.Width = ChromeSizes.OneCh * 10.0;
        _info.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _info.TextAlignment = TextAlignment.Center;
        _info.TextWrapping = TextWrapping.NoWrap;
        _info.TextTrimming = TextTrimming.CharacterEllipsis;

        Grid.SetColumn(_buttons, ButtonColumn);
        _grid.Children.Add(_buttons);
        _buttons.AddButton(Symbols.ArrowUpwardAlt, PreviousClickHandler, "Previous or Page Top");
        _buttons.AddButton(Symbols.ArrowDownwardAlt, NextClickHandler, "Next or Page Bottom");
        _toggle = _buttons.AddButton(Symbols.Search, "Toggle Find");
        _toggle.Gesture = new(Key.F, KeyModifiers.Control);
        _toggle.Click += (_, __) => IsExpanded = !IsExpanded;
        _toggle.Classes.Add("accent-checked");

        UpdatePositionText();
    }

    /// <summary>
    /// Gets or sets the <see cref="DeckViewer"/> instance.
    /// </summary>
    public bool IsExpanded
    {
        get { return _isExpanded; }

        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                UpdateState();

                if (value)
                {
                    _editor.Focus();
                }
                else
                {
                    _editor.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="DeckViewer"/> instance.
    /// </summary>
    public DeckViewer? Viewer
    {
        get { return _viewer; }

        set
        {
            if (_viewer != value)
            {
                _viewer?.Changed -= ViewerChangedHandler;
                _viewer = value;
                value?.Changed += ViewerChangedHandler;
            }
        }
    }

    /// <summary>
    /// Do not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    protected new IBrush? Background
    {
        get { return base.Background; }
        set { base.Background = value; }
    }

    /// <summary>
    /// Handles button key gestures.
    /// </summary>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        if (e.Handled)
        {
            return false;
        }

        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;

            if (!GlobalGarden.IsMissionSearch)
            {
                IsExpanded = true;

                // We handle single line only
                var text = _viewer?.Tracker?.GetEffectiveText(WhatText.SelectedOrNull)?.TrimTitle(SearchOptions.MaxLength);

                if (!string.IsNullOrEmpty(text))
                {
                    _editor.Text = text;
                }

                _editor.Focus();
                _editor.SelectAll();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Styling.StylingChanged += StylingChangedHandler;
        GlobalGarden.MissionChanged += MissionChangedHandler;
        StylingChangedHandler(null, EventArgs.Empty);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
        GlobalGarden.MissionChanged -= MissionChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(DeckNavigator)}.{nameof(OnKeyDown)}";
        Diag.WriteLine(NSpace, $"Key: {e.Key}, {e.KeyModifiers}");

        base.OnKeyDown(e);

        if (IsExpanded && e.Key == Key.Escape)
        {
            e.Handled = true;
            IsExpanded = false;
        }
    }

    private SearchFlags GetFlags()
    {
        var flags = _editor.IsMatchCaseChecked ? SearchFlags.None : SearchFlags.IgnoreCase;

        if (_editor.IsMatchWordChecked)
        {
            return flags | SearchFlags.Word;
        }

        return flags;
    }

    private void UpdateState()
    {
        bool mission = GlobalGarden.IsMissionSearch;

        _toggle.IsChecked = _isExpanded;
        _toggle.IsVisible = !mission;

        _editor.IsVisible = _isExpanded && !mission;
        _info.IsVisible = _isExpanded || mission;
    }

    private void UpdatePositionText()
    {
        if (_viewer?.IsSearching == true)
        {
            int pos = _viewer.KeywordPos;
            int count = _viewer.KeywordCount;

            if (count == 0)
            {
                _info.Inlines = null;
                _info.Text = "None";
                _info.Foreground = ChromeBrushes.CriticalBrush;
                return;
            }

            _info.Foreground = ChromeStyling.GrayForeground;

            if (pos < 1)
            {
                _info.Inlines = ChromeFonts.GetRun(Symbols.VerticalAlignTop);
                return;
            }

            if (pos > count)
            {
                _info.Inlines = ChromeFonts.GetRun(Symbols.VerticalAlignBottom);
                return;
            }

            _info.Inlines = null;
            _info.Text = string.Concat(_viewer.KeywordPos.ToString(), " of ", _viewer.KeywordCount.ToString());
            return;
        }

        _info.Foreground = ChromeStyling.GrayForeground;
        _info.Inlines = null;
        _info.Text = "None";
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        if (GlobalGarden.IsMissionSearch)
        {
            base.Background = Styling.BackgroundLow;
            return;
        }

        base.Background = Styling.Background;
    }

    private void PreviousClickHandler(object? _, EventArgs __)
    {
        _viewer?.PreviousKeywordOrHome();
    }

    private void NextClickHandler(object? _, EventArgs __)
    {
        _viewer?.NextKeywordOrEnd();
    }

    private void SearchSubmittedHandler(object? _, EventArgs __)
    {
        if (!GlobalGarden.IsMissionSearch)
        {
            GlobalGarden.Search = new(_editor.Text, GetFlags());
        }
    }

    private void SearchChangingHandler(object? _, EventArgs __)
    {
        if (!GlobalGarden.IsMissionSearch && string.IsNullOrWhiteSpace(_editor.Text))
        {
            // Clear not search
            GlobalGarden.Search = null;
        }
    }

    private void ViewerChangedHandler(object? _, EventArgs __)
    {
        UpdatePositionText();
    }

    private void MissionChangedHandler(object? _, EventArgs __)
    {
        UpdateState();
        StylingChangedHandler(null, EventArgs.Empty);

        if (!GlobalGarden.IsMissionSearch)
        {
            GlobalGarden.Search = new(_editor.Text, GetFlags());
        }

        UpdatePositionText();
    }

}