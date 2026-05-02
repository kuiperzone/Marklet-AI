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
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Low-level control for displaying <see cref="CarouselPage"/> items.
/// </summary>
public sealed class CarouselControl : Border
{
    /// <summary>
    /// Default <see cref="ContentMaxWidth"/> value.
    /// </summary>
    public const double DefaultContentMaxWidth = 540;

    private readonly ScrollPanel _contentPanel = new();
    private readonly TextEditor _searchEditor = new();
    private int _pageIndex = -1;
    private bool _isSearching;
    private bool _groupVisible;

    /// <summary>
    /// Constructor.
    /// </summary>
    public CarouselControl()
    {
        // See ChromeStyling.axaml
        GroupClasses.Add("pill-list");
        GroupClasses.Add("shade-background");

        IndexGroup.Classes.Add("corner-small");
        IndexGroup.Children.Add(_searchEditor);
        IndexGroup.PropertyChanged += IndexPropertyChangedHandler;
        IndexGroup.AttachedToVisualTree += (_, __) => UpdateIndex(true);
        IndexGroup.DetachedFromVisualTree += (_, __) => DeactivatePages();

        _searchEditor.MaxLines = 1;
        _searchEditor.MaxLength = 32;
        _searchEditor.Watermark = "Find setting...";
        _searchEditor.MinWidth = 0;
        _searchEditor.Width = double.NaN;
        _searchEditor.IsVisible = false;
        _searchEditor.Margin = new(0.0, ChromeSizes.LargePx);
        _searchEditor.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        _searchEditor.KeyDown += SearchKeyDownHandler;
        _searchEditor.TextChanged += SearchChangedHandler;

        base.Child = _contentPanel;
        _contentPanel.ContentMaxWidth = DefaultContentMaxWidth;
        _contentPanel.ContentMargin = ChromeSizes.HugePadding;
        _contentPanel.VerticalSpacing = ChromeSizes.HugePx;
        _contentPanel.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
    }

    /// <summary>
    /// Defines the <see cref="IsSearching"/> property.
    /// </summary>
    public static readonly DirectProperty<CarouselControl, bool> IsSearchingProperty =
        AvaloniaProperty.RegisterDirect<CarouselControl, bool>(nameof(IsSearching),
        o => o.IsSearching, (o, v) => o.IsSearching = v);

    /// <summary>
    /// Occurs when page <see cref="PageIndex"/> changes.
    /// </summary>
    public event EventHandler<EventArgs>? PageChanged;

    /// <summary>
    /// Do not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Gets the <see cref="PixieGroup"/> index control.
    /// </summary>
    /// <remarks>
    /// The contents are managed by <see cref="CarouselControl"/>, which the owner should . The owner should host this
    /// control separately but not attempt to modify contents. The Background, Margin, Padding etc. may be set externally.
    /// </remarks>
    public PixieGroup IndexGroup { get; } = new();

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
    /// <remarks>
    /// The default is true.
    /// </remarks>
    public bool BringContentIntoViewOnFocus
    {
        get { return _contentPanel.BringIntoViewOnFocusChange; }
        set { _contentPanel.BringIntoViewOnFocusChange = value; }
    }

    /// <summary>
    /// Gets or sets the currently selected page index.
    /// </summary>
    /// <remarks>
    /// Changing the value will force <see cref="IsSearching"/> to false.
    /// </remarks>
    public int PageIndex
    {
        get { return _pageIndex; }

        set
        {
            if (value != _pageIndex)
            {
                ShowContent(value);
            }
        }
    }

    /// <summary>
    /// Gets the page title which changes when <see cref="PageIndex"/> changes.
    /// </summary>
    public string? PageTitle { get; private set; }

    /// <summary>
    /// Gets a list of pages to show when the control becomes visible.
    /// </summary>
    /// <remarks>
    /// The sequence should be populated prior to the control becoming visible.
    /// </remarks>
    public List<CarouselPage> Pages { get; } = new();

    /// <summary>
    /// Gets a list of XAML style class names to add to <see cref="PixieGroup"/> controls in <see cref="Pages"/>.
    /// </summary>
    /// <remarks>
    /// The sequence should be populated prior to the control becoming visible.
    /// </remarks>
    public List<string> GroupClasses { get; } = new();

    /// <summary>
    /// Gets or sets whether controls are considered "fluid".
    /// </summary>
    /// <remarks>
    /// The sequence should be populated prior to the control becoming visible. The value is passed to <see
    /// cref="CarouselPage.Activate(bool)"/>. Default is true.
    /// </remarks>
    public bool IsFluid { get; set; } = true;

    /// <summary>
    /// Gets whether the left panel is in "search mode".
    /// </summary>
    public bool IsSearching
    {
        get { return _isSearching; }
        set { SetAndRaise(IsSearchingProperty, ref _isSearching, value); }
    }

    /// <summary>
    /// Calls <see cref="CarouselPage.Apply()"/> on all items <see cref="Pages"/>.
    /// </summary>
    public void Apply()
    {
        foreach (var item in Pages)
        {
            item.Apply();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(OnPropertyChanged)}";

        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == IsSearchingProperty)
        {
            Diag.WriteLine(NSpace, "IsSearching: " + change.GetNewValue<bool>());

            if (change.GetNewValue<bool>())
            {
                _searchEditor.IsVisible = true;
                _searchEditor.Focus();
                ClearIndex();
                return;
            }

            _searchEditor.Clear();
            _searchEditor.IsVisible = false;
            UpdateIndex(true);
            return;
        }
    }

    private void ClearIndex()
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(ClearIndex)}";
        Diag.WriteLine(NSpace, "Clear");

        if (IndexGroup.Children.Count > 1)
        {
            // Leave search at index 0
            IndexGroup.Children.RemoveRange(1, IndexGroup.Children.Count - 1);
        }
    }

    private void UpdateIndex(bool forceEffective)
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(UpdateIndex)}";
        Diag.WriteLine(NSpace, "UPDATE INDEX");

        if (IsSearching)
        {
            Diag.WriteLine(NSpace, "Set Searching = false");
            IsSearching = false;
            return;
        }

        bool effective = IndexGroup.IsEffectivelyVisible;
        Diag.WriteLine(NSpace, "Force: " + forceEffective);
        Diag.WriteLine(NSpace, "IsSearching: " + IsSearching);
        Diag.WriteLine(NSpace, "Visible: " + _groupVisible + ", " + effective);

        if (_groupVisible != effective || forceEffective)
        {
            ClearIndex();
            _groupVisible = effective;

            if (effective)
            {
                int index = 0;
                Diag.WriteLine(NSpace, "Activating");

                foreach (var item in Pages)
                {
                    if (item.Init(index++, GroupClasses))
                    {
                        item.IndexClick += IndexClickHandler;
                    }

                    if (item.IndexDivider != null)
                    {
                        Diag.WriteLine(NSpace, "Divider");
                        IndexGroup.Children.Add(item.IndexDivider);
                    }

                    if (item.IndexCard != null)
                    {
                        IndexGroup.Children.Add(item.IndexCard);
                    }

                    item.Activate(IsFluid);
                }
            }
            else
            {
                Diag.WriteLine(NSpace, "Deactivating");

                foreach (var item in Pages)
                {
                    item.Deactivate();
                }
            }

            ShowContent(PageIndex, 0);
        }
    }

    private void DeactivatePages()
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(DeactivatePages)}";
        Diag.WriteLine(NSpace, "Deactivating");

        foreach (var item in Pages)
        {
            item.Deactivate();
        }
    }

    private void ShowContent(CarouselPage? page)
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(ShowContent)}";
        Diag.WriteLine(NSpace, "Page: " + page?.PageIndex ?? "null");

        var current = _pageIndex;
        Diag.WriteLine(NSpace, "Current: " + _pageIndex);

        try
        {
            PageTitle = null;
            _contentPanel.Children.Clear();

            if (page?.IndexCard != null)
            {
                PageTitle = page.Title;
                page.IndexCard.IsChecked = true;
                _contentPanel.Children.AddRange(page.Children);
            }
        }
        finally
        {
            _contentPanel.NormalizedY = 0.0;
            _pageIndex = page?.PageIndex ?? -1;

            if (_pageIndex != current)
            {
                Diag.WriteLine(NSpace, "INVOKE " + nameof(PageChanged));
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void ShowContent(int pageIndex, int defaultIndex = -1)
    {
        ShowContent(GetPageOrDefault(pageIndex, defaultIndex));
    }

    private CarouselPage? GetPageOrDefault(int index, int defaultIndex = -1)
    {
        if (index < 0 || index >= Pages.Count)
        {
            index = defaultIndex;
        }

        if (index > -1 && index < Pages.Count)
        {
            return Pages[index];
        }

        return null;
    }

    private bool FindKeyword(string? keyword, List<PixieFinding> findings)
    {
        keyword = keyword?.Trim();

        if (string.IsNullOrEmpty(keyword))
        {
            return false;
        }

        int count = findings.Count;

        foreach (var page in Pages)
        {
            foreach (var item in page.Children)
            {
                if (item is PixieGroup group)
                {
                    group.Search(keyword, findings);
                    continue;
                }

                if (item is PixieControl control)
                {
                    control.Search(keyword, findings);
                    continue;
                }

                if (item is TextBlock block)
                {
                    if (block.Text?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        findings.Add(new(block));
                    }

                    continue;
                }
            }
        }

        return findings.Count != count;
    }

    private CarouselPage? GetFindingPage(PixieFinding? finding)
    {
        if (finding?.Source != null)
        {
            var source = finding.Source;

            // Page
            foreach (var entry in Pages)
            {
                // Children are nominally PixieGroup
                foreach (var item in entry.Children)
                {
                    if (item == source)
                    {
                        return entry;
                    }

                    if (item is PixieGroup group && group.Children.Contains(source))
                    {
                        return entry;
                    }
                }
            }
        }

        return null;
    }

    private void IndexPropertyChangedHandler(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(IndexPropertyChangedHandler)}";

        var p = e.Property;

        if (p == IsVisibleProperty)
        {
            Diag.WriteLine(NSpace, "IsVisibleProperty: " + e.GetNewValue<bool>());
            UpdateIndex(false);
            return;
        }

        if (p == BoundsProperty)
        {
            Diag.WriteLine(NSpace, "Bounds");

            if (!IsSearching)
            {
                UpdateIndex(false);
            }

            return;
        }
    }

    private void IndexClickHandler(object? sender, EventArgs _)
    {
        if (sender is CarouselPage page && Pages.Contains(page))
        {
            PageIndex = page.PageIndex;
        }
    }

    private void SearchKeyDownHandler(object? _, KeyEventArgs e)
    {
        if (e.PhysicalKey == PhysicalKey.Escape)
        {
            e.Handled = true;
            IsSearching = false;
        }
    }

    private void SearchChangedHandler(object? _, EventArgs __)
    {
        if (IsSearching)
        {
            ClearIndex();
            var findings = new List<PixieFinding>(8);

            if (FindKeyword(_searchEditor.Text, findings))
            {
                var width = _searchEditor.Bounds.Width;

                foreach (var item in findings)
                {
                    IndexGroup.Children.Add(item);

                    item.MaxWidth = width;
                    item.Click += FindingClickHandler;
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(_searchEditor.Text))
            {
                var none = new TextBlock();
                none.Text = "None";
                none.Foreground = ChromeStyling.GrayForeground;
                none.Margin = new(0.0, ChromeSizes.HugePx);
                none.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                IndexGroup.Children.Add(none);
            }
        }
    }

    private void FindingClickHandler(object? sender, EventArgs __)
    {
        const string NSpace = $"{nameof(CarouselControl)}.{nameof(FindingClickHandler)}";
        Diag.WriteLine(NSpace, "Clicked");

        if (sender is PixieFinding finding)
        {
            Diag.WriteLine(NSpace, "Finding: " + finding.Title);

            var page = GetFindingPage(finding);
            Diag.WriteLine(NSpace, "PageIndex: " + page?.PageIndex ?? "-1");

            // Changing PageIndex
            IsSearching = false;
            ShowContent(page);
            Diag.ThrowIfTrue(IsSearching);

            (finding.Source as PixieControl)?.Attention();
        }
    }

}
