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
using Avalonia.Media;
using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Windows.Internal;

/// <summary>
/// Chrome client-side system bar.
/// </summary>
internal sealed class ChromeBar : Panel, IChromeBar
{
    private readonly DispatchCoalescer _titleAligner = new();
    private readonly ButtonGrid _buttonGrid;

    private readonly TextBlock _titleBlock = new();
    private string? _title;
    private BarUnderlay? _underlay;

    public ChromeBar(ChromeWindow owner)
    {
        // Set first as child will read it
        Owner = owner;
        IsVisible = owner.IsChromeAlwaysVisible;

        _buttonGrid = new ButtonGrid(this);
        _buttonGrid.Margin = ChromeSizes.RegularPadding;

        LeftGroup = _buttonGrid.LeftGroup;
        _buttonGrid.LeftGroup.ButtonsChanged += (_, __) => UpdateVisible();

        RightGroup = _buttonGrid.RightGroup;
        _buttonGrid.RightGroup.ButtonsChanged += (_, __) => UpdateVisible();

        _titleBlock.FontWeight = FontWeight.Bold;
        _titleBlock.TextWrapping = TextWrapping.NoWrap;
        _titleBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        _titleBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        _titleBlock.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        Children.Add(_titleBlock);

        Children.Add(_buttonGrid);
        _buttonGrid.MinimizeButton.Click += MinimizeClickHandler;
        _buttonGrid.MaximizeButton.Click += MaximizeRestoreClickHandler;
        _buttonGrid.CloseButton.Click += CloseClickHandler;

        _titleAligner.Posted += (_, __) => AlignTitle();
        owner.PropertyChanged += WindowPropertyChangedHandler;

        // Important for mouse events
        Background = Brushes.Transparent;
    }

    public readonly ChromeWindow Owner;

    /// <summary>
    /// Implements <see cref="IChromeBar.IsEmpty"/>.
    /// </summary>
    public bool IsEmpty
    {
        get { return LeftGroup.Buttons.Count == 0 && RightGroup.Buttons.Count == 0; }
    }

    /// <summary>
    /// Implements <see cref="IChromeBar.BarHeight"/>.
    /// </summary>
    public double BarHeight
    {
        get { return _buttonGrid.Height; }
    }

    /// <summary>
    /// Implements <see cref="IChromeBar.LeftGroup"/>.
    /// </summary>
    public ISubLightBar LeftGroup { get; }

    /// <summary>
    /// Implements <see cref="IChromeBar.RightGroup"/>.
    /// </summary>
    public ISubLightBar RightGroup { get; }

    /// <summary>
    /// Implements <see cref="IChromeBar.Title"/>.
    /// </summary>
    public string? Title
    {
        get { return _title; }

        set
        {
            value = value?.TrimTitle();

            if (_title != value)
            {
                _title = value;
                _titleBlock.Text = value ?? Owner.Title?.TrimTitle();
                _titleAligner.Post();
            }
        }
    }

    /// <summary>
    /// Gets the underlay control.
    /// </summary>
    public IBarUnderlay Underlay
    {
        get
        {
            // Lazy
            if (_underlay == null)
            {
                _underlay = new();
                Children.Insert(0, _underlay);
                _underlay.SizeChanged += (_, __) => _titleAligner.Post();
            }

            return _underlay;
        }
    }

    public void RefreshStyling()
    {
        // Order important
        _buttonGrid.RefreshStyling();
        Height = BarHeight;
    }

    public void UpdateWindowSettings(bool dialog)
    {
        UpdateVisible();

        // Order important
        _buttonGrid.UpdateWindowSettings(dialog);
        Height = BarHeight;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _titleAligner.Post();
    }

    private void UpdateVisible()
    {
        try
        {
            if (Owner.IsChromeWindow)
            {
                IsVisible = true;
                _titleBlock.IsVisible = true;
                return;
            }

            IsVisible = Owner.IsChromeAlwaysVisible && !IsEmpty;
            _titleBlock.IsVisible = _title != null;
        }
        finally
        {
            _titleAligner.Post();
        }
    }

    private void AlignTitle()
    {
        const string NSpace = $"{nameof(ChromeBar)}.{nameof(AlignTitle)}";

        if (_titleBlock.IsEffectivelyVisible)
        {
            ConditionalDebug.WriteLine(NSpace, "Align title");
            const int StarColumn = ButtonGrid.StarColumn;
            const int ColumnCount = ButtonGrid.ColumnCount;

            double leftSum = 0.0;
            double rightSum = 0.0;
            double uw = _underlay?.BoundsWidth ?? 0.0;
            double tw = _titleBlock.DesiredSize.Width;
            double sw = _buttonGrid.ColumnDefinitions[StarColumn].ActualWidth;

            if (tw > sw - uw)
            {
                ConditionalDebug.WriteLine(NSpace, "Recalculating");

                for (int n = 0; n < ColumnCount; ++n)
                {
                    var col = _buttonGrid.ColumnDefinitions[n];

                    if (n < StarColumn)
                    {
                        leftSum += col.ActualWidth;
                    }
                    else
                    if (n > StarColumn)
                    {
                        rightSum += col.ActualWidth;
                    }
                }
            }

            _titleBlock.Margin = new(Math.Max(leftSum, uw), 0.0, rightSum, 0.0);
        }
    }

    private void WindowPropertyChangedHandler(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        var p = e.Property;

        if (p == Window.TitleProperty)
        {
            if (_title == null)
            {
                _titleBlock.Text = e.GetNewValue<string?>()?.TrimTitle();
                _titleAligner.Post();
            }

            return;
        }

        if (p == ChromeWindow.IsChromeAlwaysVisibleProperty)
        {
            UpdateVisible();
            return;
        }
    }

    private void OnTopClickHandler(object? _, EventArgs __)
    {
        Owner.Topmost = !Owner.Topmost;
    }

    private void MinimizeClickHandler(object? _, RoutedEventArgs __)
    {
        Owner.WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreClickHandler(object? _, RoutedEventArgs __)
    {
        var s = Owner.WindowState;
        Owner.WindowState = s != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseClickHandler(object? _, RoutedEventArgs __)
    {
        Owner.Close();
    }
}
