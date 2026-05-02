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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using KuiperZone.Marklet.PixieChrome.Carousels;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Base class window for displaying <see cref="CarouselPage"/> information.
/// </summary>
/// <remarks>
/// The class is concrete but empty. It is primarily intended to be shown as a dialog using <see
/// cref="Window.ShowDialog(Window)"/>.
/// </remarks>
public class CarouselDialog : ChromeWindow
{
    /// <summary>
    /// Default <see cref="LeftPanelMinWidth"/> value.
    /// </summary>
    public const double DefaultLeftPanelMinWidth = 175;

    /// <summary>
    /// Default <see cref="LeftPanelMaxWidth"/> value.
    /// </summary>
    public const double DefaultLeftPanelMaxWidth = 300;

    /// <summary>
    /// Default <see cref="ContentMaxWidth"/> value.
    /// </summary>
    public const double DefaultContentMaxWidth = 540;

    private readonly CarouselControl _carousel = new();
    private readonly ColumnDefinition _indexColumn;
    private readonly Border _indexContainer = new();
    private readonly LightButton _searchButton;
    private readonly DialogControls? _controls;

    static CarouselDialog()
    {
        WidthProperty.OverrideDefaultValue<CarouselDialog>(1000.0);
        MinWidthProperty.OverrideDefaultValue<CarouselDialog>(500.0);
        HeightProperty.OverrideDefaultValue<CarouselDialog>(700.0);
        MinHeightProperty.OverrideDefaultValue<CarouselDialog>(350);
        CanResizeProperty.OverrideDefaultValue<CarouselDialog>(true);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// If "buttons" equals <see cref="DialogButtons.None"/>, the controls of <see cref="Pages"/>  will be "fluid" and
    /// will update their settings immediately on change. If buttons are provided, they should include a button with a
    /// "positive result" such as <see cref="DialogButtons.Ok"/>, and a cancel button such as <see
    /// cref="DialogButtons.Cancel"/>. It may also optionionally include <see cref="DialogButtons.Apply"/>.
    /// </remarks>
    public CarouselDialog(DialogButtons buttons = DialogButtons.None)
        : base(true)
    {
        Pages = _carousel.Pages;

        Grid grid = new();
        Content = grid;
        grid.RowDefinitions.Add(new (GridLength.Star));

        _indexColumn = new(new GridLength(1.0, GridUnitType.Star));
        _indexColumn.MinWidth = DefaultLeftPanelMinWidth;
        _indexColumn.MaxWidth = DefaultLeftPanelMaxWidth;

        grid.ColumnDefinitions.Add(_indexColumn);
        grid.ColumnDefinitions.Add(new (new GridLength(3.0, GridUnitType.Star)));

        var group = _carousel.IndexGroup;
        group.Margin = ChromeSizes.StandardPadding;

        var scroller = new ScrollViewer();
        scroller.Content = group;
        scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _indexContainer.Child = scroller;

        Grid.SetRow(_indexContainer, 0);
        Grid.SetColumn(_indexContainer, 0);
        Grid.SetRowSpan(_indexContainer, int.MaxValue);
        grid.Children.Add(_indexContainer);

        _indexContainer.Background = Styling.BackgroundLow;
        _indexContainer.BorderBrush = Styling.BorderBrush;
        _indexContainer.SizeChanged += IndexSizeChangedHandler;

        _indexContainer.BorderThickness = new(0.0, 0.0, 1.0, 0.0);
        Underlay.BorderWidth = 1.0;

        Grid.SetRow(_carousel, 0);
        Grid.SetColumn(_carousel, 1);
        grid.Children.Add(_carousel);
        _carousel.PageChanged += PageChangedHandler;

        _searchButton = ChromeBar.LeftGroup.AddButton(Symbols.Search, "Search");
        _searchButton.IsVisible = false;
        _searchButton.Click += (_, __) => _carousel.IsSearching = !_carousel.IsSearching;

        Buttons = buttons;

        if (buttons != DialogButtons.None)
        {
            _carousel.IsFluid = false;

            _controls = new();
            _controls.Buttons = buttons;
            _controls.BorderThickness = new(0.0, 1.0, 0.0, 0.0);
            _controls.Click += (_, e) => AcceptOrApply(e.Button);

            grid.RowDefinitions.Add(new(GridLength.Auto));
            Grid.SetRow(_controls, 1);
            Grid.SetColumn(_controls, 1);
            grid.Children.Add(_controls);
        }
    }

    /// <summary>
    /// Gets or sets the left panel minimum width.
    /// </summary>
    public double LeftPanelMinWidth
    {
        get { return _indexColumn.MinWidth; }
        set { _indexColumn.MinWidth = value; }
    }

    /// <summary>
    /// Gets or sets the left panel maximum width.
    /// </summary>
    public double LeftPanelMaxWidth
    {
        get { return _indexColumn.MaxWidth; }
        set { _indexColumn.MaxWidth = value; }
    }

    /// <summary>
    /// Gets or sets the content panel width.
    /// </summary>
    public double ContentMaxWidth
    {
        get { return _carousel.ContentMaxWidth; }
        set { _carousel.ContentMaxWidth = value; }
    }

    /// <summary>
    /// Gets or sets the content margin.
    /// </summary>
    /// <remarks>
    /// The default is suitable for most scenarios.
    /// </remarks>
    public Thickness ContentMargin
    {
        get { return _carousel.ContentMargin; }
        set { _carousel.ContentMargin = value; }
    }

    /// <summary>
    /// Gets or sets whether to bring newly focused items into view on focus change.
    /// </summary>
    /// <remarks>
    /// The default is true.
    /// </remarks>
    public bool BringContentIntoViewOnFocus
    {
        get { return _carousel.BringContentIntoViewOnFocus; }
        set { _carousel.BringContentIntoViewOnFocus = value; }
    }

    /// <summary>
    /// Gets or sets whether the search button is visible.
    /// </summary>
    public bool IsSearchVisible
    {
        get { return _searchButton.IsVisible; }

        set
        {
            _searchButton.IsVisible = value;
        }
    }

    /// <summary>
    /// Gets or sets the currely selected page index.
    /// </summary>
    public int PageIndex
    {
        get { return _carousel.PageIndex; }
        set { _carousel.PageIndex = value; }
    }

    /// <summary>
    /// Gets a list of pages to show when the window is opened.
    /// </summary>
    /// <remarks>
    /// A subclass or call should populate this prior to opening the window. Pages should not be added while the window is open.
    /// </remarks>
    public List<CarouselPage> Pages { get; }

    /// <summary>
    /// Gets the buttons displayed at the bottom of the window.
    /// </summary>
    /// <remarks>
    /// The value is set on construction, with a value of <see cref="DialogButtons.None"/> showing no buttons.
    /// </remarks>
    public DialogButtons Buttons { get; }

    /// <summary>
    /// Accepts the <see cref="Buttons"/> click.
    /// </summary>
    /// <remarks>
    /// Not used if <see cref="Buttons"/> equals <see cref="DialogButtons.None"/>. Where "button" indicates a position
    /// result, such as <see cref="DialogButtons.Ok"/>, <see cref="CarouselPage.Apply"/> is called on all <see
    /// cref="Pages"/>. Specifically where "button" equals <see cref="DialogButtons.Apply"/> or <see
    /// cref="DialogButtons.ApplyAll"/>, the window remains open. For all other values, it closes.
    /// </remarks>
    /// <exception cref="ArgumentException">Illegal combined button flag</exception>
    protected virtual void AcceptOrApply(DialogButtons button)
    {
        if (!button.IsSingleLegal())
        {
            throw new ArgumentException("Illegal combined button flag", nameof(button));
        }

        if (button.IsPositiveResult())
        {
            _carousel.Apply();

            if (button == DialogButtons.Apply || button == DialogButtons.ApplyAll)
            {
                return;
            }
        }

        Close(button);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnStylingChanged(bool init)
    {
        base.OnStylingChanged(init);

        _controls?.BorderBrush = Styling.BorderBrush;

        _indexContainer.Background = Styling.BackgroundLow;
        _indexContainer.BorderBrush = Styling.BorderBrush;

        // Track for underlay
        Underlay.Background = Styling.BackgroundLow;
        Underlay.BorderBrush = Styling.BorderBrush;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == TitleProperty)
        {
            Underlay.Title = change.GetNewValue<string?>();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            _carousel.IsSearching = true;
        }
    }

    private void IndexSizeChangedHandler(object? _, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width != e.PreviousSize.Width)
        {
            Underlay.BoundsWidth = e.NewSize.Width;
        }
    }

    private void PageChangedHandler(object? _, EventArgs __)
    {
        ChromeBar.Title = _carousel.PageTitle;
    }
}
