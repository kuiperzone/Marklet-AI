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
using KuiperZone.Marklet.Stack.Garden;
using Avalonia;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Presents and manages the collection of <see cref="GardenBin"/> instances provided by <see cref="MemoryGarden"/>.
/// </summary>
public sealed class MissionControl : Border
{
    private const int BufferColumn = 0;
    private const int SeparatorColumn = 1;
    private const int BinViewColumn = 2;
    private static readonly ChromeStyling Styling = ChromeStyling.Global;

    private readonly Grid _grid = new();
    private readonly DockPanel _bufferPanel = new();
    private readonly Border _separator = new();
    private readonly List<BufferButton> _radioButtons = new(8);

    private readonly BufferButton _homeButton;
    private readonly ScrollableBinView _homeBin = new();

    private readonly BufferButton _searchButton = new(Symbols.Search);

    private readonly ScrollableBinView _archiveBin = new();
    private readonly ScrollableBinView _wasteBin = new();

    private readonly LightButton _aboutButton;
    private readonly LightButton _settingsButton;

#if DEBUG
    private readonly LightButton _inspectButton;
#endif

    private MemoryGarden? _garden;
    private GardenBin? _currentBin;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MissionControl()
    {
        _grid.RowDefinitions.Add(new(GridLength.Star));
        _grid.ColumnDefinitions.Add(new(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new(1.0, GridUnitType.Pixel));
        _grid.ColumnDefinitions.Add(new(GridLength.Star));
        Child = _grid;

        Grid.SetColumn(_bufferPanel, BufferColumn);
        _grid.Children.Add(_bufferPanel);

        Grid.SetColumn(_separator, SeparatorColumn);
        _grid.Children.Add(_separator);

        _homeButton = CreateRadioBin("Home", Symbols.Home, _homeBin);
        CreateRadioBin("Search", Symbols.Search, null);
        CreateRadioBin("Archive", Symbols.Pinboard, _archiveBin);
        CreateRadioBin("Waste Bin", Symbols.Delete, _wasteBin);

        _settingsButton = CreateBottomButton("Settings", Symbols.Settings);
        _settingsButton.Click += (_, __) => SettingsClick?.Invoke(this, EventArgs.Empty);

        _aboutButton = CreateBottomButton("About", Symbols.Info);
        _aboutButton.Click += (_, __) => AboutClick?.Invoke(this, EventArgs.Empty);

        base.MinWidth = BufferButton.ButtonSize;
        _homeButton.IsChecked = true;

#if DEBUG
        _inspectButton = CreateBottomButton("Control development", Symbols.FrameBug);
        _inspectButton.Click += (_, __) => InspectClick?.Invoke(this, EventArgs.Empty);
#endif
    }

    /// <summary>
    /// Occurs when <see cref="CurrentBin"/> changes.
    /// </summary>
    public event EventHandler<EventArgs>? CurrentBinChanged;

    /// <summary>
    /// Occurs when the settings button is clicked.
    /// </summary>
    public event EventHandler<EventArgs>? SettingsClick;

    /// <summary>
    /// Occurs when the about button is clicked.
    /// </summary>
    public event EventHandler<EventArgs>? AboutClick;

#if DEBUG
    /// <summary>
    /// Occurs when the development inspection button is clicked.
    /// </summary>
    public event EventHandler<EventArgs>? InspectClick;
#endif

    /// <summary>
    /// Gets or sets the garden.
    /// </summary>
    public MemoryGarden? Garden
    {
        get { return _garden; }

        set
        {
            if (_garden != value)
            {
                _garden = value;
                _homeBin.View.SourceBin = value?.HomeBin;
                _archiveBin.View.SourceBin = value?.ArchiveBin;
                _wasteBin.View.SourceBin = value?.WasteBin;
                CurrentBin = value?.HomeBin;
            }
        }
    }

    /// <summary>
    /// Gets the currently displayed <see cref="GardenBin"/>.
    /// </summary>
    /// <remarks>
    /// The value may be null at any time.
    /// </remarks>
    public GardenBin? CurrentBin
    {
        get { return _currentBin; }

        private set
        {
            if (_currentBin != value)
            {
                _currentBin = value;
                CurrentBinChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Replaces and makes readonly.
    /// </summary>
    public new double MinWidth
    {
        get { return base.MinWidth; }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        StylingChangedHandler(null, EventArgs.Empty);
        ChromeStyling.Global.StylingChanged += StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ChromeStyling.Global.StylingChanged -= StylingChangedHandler;
    }

    private BufferButton CreateRadioBin(string name, string symbol, ScrollableBinView? view)
    {
        var button = new BufferButton(symbol, view);
        button.Tip = name;
        button.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        button.Click += RadioCheckedChangedHandler;

        DockPanel.SetDock(button, Dock.Top);
        _bufferPanel.Children.Add(button);

        _radioButtons.Add(button);

        if (view != null)
        {
            view.Margin = new(ChromeSizes.OneCh, 0.0);
            Grid.SetColumn(view, BinViewColumn);
            _grid.Children.Add(view);
        }

        return button;
    }

    private LightButton CreateBottomButton(string name, string symbol)
    {
        var button = new BufferButton(symbol);
        button.Tip = name;
        button.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

        DockPanel.SetDock(button, Dock.Bottom);
        _bufferPanel.Children.Add(button);

        _radioButtons.Add(button);
        return button;
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        _bufferPanel.Background = Styling.BufferBarBrush;

        if (Styling.BufferBarBrush == Styling.BackgroundLow)
        {
            // A bit of fudge, but there needs to be a separator here.
            _separator.Background = Styling.BorderBrush;
        }
        else
        {
            _separator?.Background = null;
        }

        var foreground = Styling.BufferBarBrush.Color.IsDark() ? ChromeBrushes.White : ChromeBrushes.Black;

        foreach (var item in _radioButtons)
        {
            item.HoverForeground = foreground;
            item.CheckedPressedForeground = foreground;
            item.FocusBorderBrush = Styling.AccentBrush;
        }
    }

    private void RadioCheckedChangedHandler(object? sender, RoutedEventArgs __)
    {
        foreach (var item in _radioButtons)
        {
            if (item != sender)
            {
                item.IsChecked = false;
                continue;
            }

            CurrentBin = item.IsChecked ? item.ScrollableView?.View.SourceBin : null;
        }
    }

    /// <summary>
    /// Wrap <see cref="GardenBinView"/> in a scroller with accoutrements.
    /// </summary>
    private sealed class ScrollableBinView : StackPanel
    {
        private readonly ScrollViewer _scroller = new();

        public ScrollableBinView()
        {
            IsVisible = false;
            View.IsVisible = false;

            _scroller.Content = View;
            _scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            Orientation = Avalonia.Layout.Orientation.Vertical;
            Children.Add(_scroller);
        }

        public GardenBinView View = new(true);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsVisibleProperty)
            {
                // Feed - so control is directly aware
                View.IsVisible = change.GetNewValue<bool>();
            }
        }
    }

    /// <summary>
    /// Custom highlight behaviour.
    /// </summary>
    private sealed class BufferButton : LightButton
    {
        private const double CheckedWidth = 4.0;
        private const double ButtonFontSize = ChromeFonts.HugeSymbolFontSize;

        public const double ButtonSize = CheckedWidth + MinBoxSize * ButtonFontSize / ChromeFonts.SymbolFontSize;

        /// <summary>
        /// Regular but oversized constructor.
        /// </summary>
        public BufferButton(string symbol)
        {
            Content = symbol;
            FontSize = ButtonFontSize;
            MinWidth = ButtonSize;
            MinHeight = ButtonSize;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;

            Foreground = ChromeStyling.ForegroundGray;

        }

        /// <summary>
        /// Toggle constructor.
        /// </summary>
        public BufferButton(string symbol, ScrollableBinView? view)
            : this(symbol)
        {
            CanToggle = true;
            ScrollableView = view;
            BorderThickness = new(CheckedWidth, 0.0, 0.0, 0.0);
            HoverBackground = ChromeBrushes.Transparent;
            CheckedPressedBackground = ChromeBrushes.Transparent;
            CheckedChanged += CheckedChangedHandler;
        }

        public ScrollableBinView? ScrollableView { get; }

        /// <summary>
        /// Overrides.
        /// </summary>
        //protected override Type StyleKeyOverride { get; } = typeof(LightButton);

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            StylingChangedHandler(null, EventArgs.Empty);
            ChromeStyling.Global.StylingChanged += StylingChangedHandler;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            ChromeStyling.Global.StylingChanged -= StylingChangedHandler;
        }

        private void StylingChangedHandler(object? _, EventArgs __)
        {
            BorderBrush = IsChecked ? Styling.AccentBrush : null;
        }

        private void CheckedChangedHandler(object? _, RoutedEventArgs __)
        {
            ScrollableView?.IsVisible = IsChecked;
            BorderBrush = IsChecked ? Styling.AccentBrush : null;
        }
    }
}