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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using KuiperZone.Marklet.PixieChrome.CarouselPages;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows.Internal;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Base class window for displaying <see cref="CarouselPage"/> information.
/// </summary>
/// <remarks>
/// The class is concrete but empty. It handles the <see cref="ChromeWindow.ChromeBar"/> configuration.
/// </remarks>
public class CarouselWindow : ChromeWindow
{
    /// <summary>
    /// Default <see cref="LeftPanelMinWidth"/> value.
    /// </summary>
    public const double DefaultLeftPanelMinWidth = 175;

    /// <summary>
    /// Default <see cref="LeftPanelMaxWidth"/> value.
    /// </summary>
    public const double DefaultLeftPanelMaxWidth = 350;

    /// <summary>
    /// Default <see cref="ContentMaxWidth"/> value.
    /// </summary>
    public const double DefaultContentMaxWidth = 540;

    private readonly Grid _grid = new();
    private readonly Border _leftBorder = new();
    private readonly ScrollViewer _leftScroller = new();
    private readonly PixieGroup _leftPanel = new();
    private readonly ScrollPanel _contentPanel = new();
    private readonly TextEditor _searchEditor = new();
    private readonly LightButton _searchButton;
    private readonly CarouselEntries _entries = new();
    private int _pageIndex;

    static CarouselWindow()
    {
        WidthProperty.OverrideDefaultValue<CarouselWindow>(1000.0);
        MinWidthProperty.OverrideDefaultValue<CarouselWindow>(500.0);
        HeightProperty.OverrideDefaultValue<CarouselWindow>(700.0);
        MinHeightProperty.OverrideDefaultValue<CarouselWindow>(350);
        CanResizeProperty.OverrideDefaultValue<CarouselWindow>(true);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public CarouselWindow()
        : base(true)
    {
        // See ChromeStyling.axaml
        PageClasses.Add("chrome-corner-grouped");

        _grid.RowDefinitions.Add(new(GridLength.Star));
        _grid.ColumnDefinitions.Add(new(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new(GridLength.Star));
        Content = _grid;

        _leftPanel.Classes.Add("chrome-corner-small");
        _leftPanel.MinWidth = DefaultLeftPanelMinWidth;
        _leftPanel.MaxWidth = DefaultLeftPanelMaxWidth;
        _leftPanel.Margin = ChromeSizes.RegularPadding;

        _leftScroller.Content = _leftPanel;
        _leftScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

        Grid.SetRow(_leftBorder, 0);
        Grid.SetColumn(_leftBorder, 0);
        _grid.Children.Add(_leftBorder);
        _leftBorder.Child = _leftScroller;
        _leftBorder.BorderThickness = new(0.0, 0.0, 1.0, 0.0);
        _leftBorder.SizeChanged += LeftResizeHandler;

        Grid.SetRow(_contentPanel, 0);
        Grid.SetColumn(_contentPanel, 1);
        _grid.Children.Add(_contentPanel);
        _contentPanel.ContentMaxWidth = DefaultContentMaxWidth;
        _contentPanel.ContentMargin = ChromeSizes.HugePadding;
        _contentPanel.VerticalSpacing = ChromeSizes.HugeSpacerPx;
        _contentPanel.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

        _leftPanel.Children.Add(_searchEditor);
        _searchEditor.MaxLines = 1;
        _searchEditor.MaxLength = 32;
        _searchEditor.Watermark = "Search...";
        _searchEditor.MinWidth = 0;
        _searchEditor.Width = double.NaN;
        _searchEditor.Margin = new(0.0, ChromeSizes.MediumSpacerPx);
        _searchEditor.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        _searchEditor.KeyDown += SearchKeyDownHandler;
        _searchEditor.TextChanged += SearchTextChanged;

        _searchButton = ChromeBar.LeftGroup.Add(Symbols.Search, "Search");
        _searchButton.IsVisible = false;
        _searchButton.Click += (_, __) => ToggleSearch();

        Underlay.BorderWidth = _leftBorder.BorderThickness.Right;

        _entries.IndexClicked += IndexClickHandler;
    }

    /// <summary>
    /// Gets or sets the left panel minimum width.
    /// </summary>
    public double LeftPanelMinWidth
    {
        get { return _leftPanel.MinWidth; }
        set { _leftPanel.MinWidth = value; }
    }

    /// <summary>
    /// Gets or sets the left panel maximum width.
    /// </summary>
    public double LeftPanelMaxWidth
    {
        get { return _leftPanel.MaxWidth; }
        set { _leftPanel.MaxWidth = value; }
    }

    /// <summary>
    /// Gets or sets the content panel width.
    /// </summary>
    public double ContentMaxWidth
    {
        get { return _contentPanel.ContentMaxWidth; }
        set { _contentPanel.ContentMaxWidth = value; }
    }

    /// <summary>
    /// Gets or sets the content margin.
    /// </summary>
    /// <remarks>
    /// The default is suitable for most scenarios.
    /// </remarks>
    public Thickness ContentMargin
    {
        get { return _contentPanel.ContentMargin; }
        set { _contentPanel.ContentMargin = value; }
    }

    /// <summary>
    /// Gets or sets whether to bring newly focused items into view on focus change.
    /// </summary>
    public bool BringContentIntoViewOnFocus
    {
        get { return _contentPanel.BringIntoViewOnFocusChange; }
        set { _contentPanel.BringIntoViewOnFocusChange = value; }
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

            if (!value)
            {
                ShowLeftIndex();
            }
        }
    }

    /// <summary>
    /// Gets or sets the currely selected page index.
    /// </summary>
    public int PageIndex
    {
        get { return _pageIndex; }

        set
        {
            if (value != _pageIndex)
            {
                _pageIndex = value;
                ShowPageContent(value);
            }
        }
    }

    /// <summary>
    /// Gets a list of pages to show when the window is opened.
    /// </summary>
    /// <remarks>
    /// A subclass or call should populate this prior to opening the window. Pages should not be added while the window is open.
    /// </remarks>
    public List<CarouselPage> Pages { get; } = new();

    /// <summary>
    /// Gets a list of XAML style class names to add to the right panel <see cref="Pages"/>.
    /// </summary>
    /// <remarks>
    /// The default is empty. Subclass may add.
    /// </remarks>
    public List<string> PageClasses { get; } = new();

    /// <summary>
    /// Gets whether the left panel is in "search mode".
    /// </summary>
    protected bool IsSearching { get; private set; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnStylingChanged(bool init)
    {
        base.OnStylingChanged(init);

        // Track for underlay
        Underlay.Background = Styling.BackgroundLow;
        _leftBorder.Background = Styling.BackgroundLow;

        Underlay.BorderBrush = Styling.BorderBrush;
        _leftBorder.BorderBrush = Styling.BorderBrush;

        _entries.RefreshStyling();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        foreach (var item in Pages)
        {
            item.OnOpened();
        }

        _entries.Initialize(Pages, PageClasses);
        ShowLeftIndex(true);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        foreach (var item in Pages)
        {
            item.OnClosed();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.PhysicalKey == PhysicalKey.F && e.KeyModifiers == KeyModifiers.Control)
        {
            ToggleSearch();
        }
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

    private void ShowPageContent(PageEntry? page)
    {
        try
        {
            _contentPanel.Children.Clear();

            if (page != null)
            {
                ChromeBar.Title = page.IndexButton.Title;
                page.IndexButton.IsBackgroundChecked = true;
                _contentPanel.Children.AddRange(page.Children);
                return;
            }

            ChromeBar.Title = "";
        }
        finally
        {
            _contentPanel.NormalizedY = 0.0;
            _pageIndex = page?.PageIndex ?? -1;
        }
    }

    private void ShowPageContent(int index, int defaultIndex = -1)
    {
        ShowPageContent(_entries.GetPageOrDefault(index, defaultIndex));
    }

    private void ShowLeftIndex(bool opening = false)
    {
        if (IsSearching || opening)
        {
            IsSearching = false;

            ClearIndex();
            _searchEditor.Clear();
            _searchEditor.IsVisible = false;

            foreach (var entry in _entries)
            {
                if (entry.IndexDivider != null)
                {
                    _leftPanel.Children.Add(entry.IndexDivider);
                }

                _leftPanel.Children.Add(entry.IndexButton);

            }

            ShowPageContent(PageIndex, 0);
        }
    }

    private void ShowLeftSearch()
    {
        // No need to clear
        IsSearching = true;
        _searchEditor.IsVisible = true;
        _searchEditor.Focus();
    }

    private void ToggleSearch()
    {
        if (IsSearching)
        {
            ShowLeftIndex();
        }
        else
        {
            ShowLeftSearch();
        }
    }

    private void ClearIndex()
    {
        if (_leftPanel.Children.Count != 0)
        {
            // Leave search at index 0
            _leftPanel.Children.RemoveRange(1, _leftPanel.Children.Count - 1);
        }
    }

    private void LeftResizeHandler(object? _, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width != e.PreviousSize.Width)
        {
            Underlay.BoundsWidth = e.NewSize.Width;
        }
    }

    private void IndexClickHandler(object? _, IndexClickEventArgs e)
    {
        PageIndex = e.Page.PageIndex;
    }

    private void SearchKeyDownHandler(object? _, KeyEventArgs e)
    {
        if (e.PhysicalKey == PhysicalKey.Escape)
        {
            e.Handled = true;
            ShowLeftIndex();
        }
    }

    private void SearchTextChanged(object? _, EventArgs __)
    {
        if (!IsSearching)
        {
            return;
        }

        ClearIndex();
        var findings = new List<PixieFinding>(8);

        if (_entries.Find(_searchEditor.Text, findings))
        {
            var width = _searchEditor.Bounds.Width;

            foreach (var item in findings)
            {
                _leftPanel.Children.Add(item);

                item.MaxWidth = width;
                item.BackgroundClick += FindingClickHandler;
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(_searchEditor.Text))
        {
            var none = new TextBlock();
            none.Text = "None";
            none.Foreground = ChromeStyling.ForegroundGray;
            none.Margin = new(0.0, ChromeSizes.HugeSpacerPx);
            none.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            _leftPanel.Children.Add(none);
        }
    }

    private void FindingClickHandler(object? sender, EventArgs __)
    {
        if (sender is PixieFinding finding)
        {
            ShowLeftIndex();
            ShowPageContent(_entries.GetFindEntry(finding));
            (finding.Source as PixieControl)?.Attention();
        }
    }

}
