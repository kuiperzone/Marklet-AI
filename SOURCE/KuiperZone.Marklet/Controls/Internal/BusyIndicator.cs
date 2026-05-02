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
using Avalonia;
using KuiperZone.Marklet.PixieChrome;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using Avalonia.Media;

namespace KuiperZone.Marklet.Controls.Internal;

/// <summary>
/// Provides busy feedback.
/// </summary>
internal sealed class BusyIndicator : Border
{
    private const int IndicatorCount = 4;
    private static readonly ChromeStyling Styling = ChromeStyling.Global;

    private readonly Ellipse[] _indicators = new Ellipse[IndicatorCount];
    private readonly Grid _grid = new();
    private readonly StackPanel _panel = new();
    private readonly TextBlock _message = new();
    private readonly DispatcherTimer _timer = new();
    private int _counter = -IndicatorCount;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public BusyIndicator()
    {
        _grid.Margin = new(ChromeSizes.TwoCh, ChromeSizes.StandardPx, ChromeSizes.TwoCh, ChromeSizes.HugePx);
        _grid.RowDefinitions.Add(new(GridLength.Auto));
        _grid.RowDefinitions.Add(new(GridLength.Auto));
        Child = _grid;

        _panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
        Grid.SetRow(_panel, 0);
        _grid.Children.Add(_panel);

        _message.IsVisible = false;
        _message.FontSize = ChromeFonts.SmallFontSize;
        _message.Margin = new(0.0, ChromeSizes.OneCh, 0.0, 0.0);
        _message.Foreground = ChromeStyling.GrayForeground;
        _message.TextWrapping = TextWrapping.Wrap;
        _message.TextTrimming = TextTrimming.CharacterEllipsis;
        Message = "Thinking…";
        Grid.SetRow(_message, 1);
        _grid.Children.Add(_message);

        var size = ChromeFonts.DefaultFontSize * 0.6;
        _panel.Spacing = size;

        for (int n = 0; n < IndicatorCount; ++n)
        {
            var circle = new Ellipse();
            circle.Width = size;
            circle.Height = size;

            _indicators[n] = circle;
            _panel.Children.Add(circle);
        }

        _timer.Tick += TimerTickHandler;
        _timer.Interval = TimeSpan.FromMilliseconds(100);
    }

    /// <summary>
    /// Gets or sets the message string.
    /// </summary>
    public string? Message
    {
        get { return _message.Text; }

        set
        {
            _message.Text = value;
            _message.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Reset();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleProperty)
        {
            Reset();
        }
    }

    private void Reset()
    {
        _counter = -IndicatorCount;

        foreach (var item in _indicators)
        {
            item.Fill = ChromeStyling.GrayForeground;
        }

        _timer.IsEnabled = IsEffectivelyVisible;
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        int x0 = Math.Abs(_counter);
        int x = IndicatorCount - x0;
        var b0 = Styling.Accent50;
        var b1 = ChromeStyling.GrayForeground;

        if (_counter++ > 0)
        {
            x = x0;
            b0 = ChromeStyling.GrayForeground;
            b1 = Styling.Accent50;
        }

        for (int n = 0; n < IndicatorCount; ++n)
        {
            _indicators[n].Fill = n < x ? b0 : b1;
        }

        if (_counter > IndicatorCount)
        {
            _counter = -IndicatorCount;
        }
    }
}