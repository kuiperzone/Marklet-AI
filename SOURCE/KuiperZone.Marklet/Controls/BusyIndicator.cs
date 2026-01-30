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
using Avalonia;
using KuiperZone.Marklet.PixieChrome;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Provides busy feedback.
/// </summary>
public sealed class BusyIndicator : Border
{
    private const int IndicatorCount = 3;
    private static readonly ChromeStyling Styling = ChromeStyling.Global;
    private static readonly TimeSpan Delay0 = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan Delay1 = TimeSpan.FromMilliseconds(50);

    private readonly Ellipse[] _indicators = new Ellipse[IndicatorCount];
    private readonly Grid _grid = new();
    private readonly StackPanel _panel = new();
    private readonly TextBlock _message = new();
    private readonly DispatcherTimer _timer = new();
    private int _counter = 0;
    private bool _overlap;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public BusyIndicator()
    {
        _grid.Margin = new(ChromeSizes.TwoCh, ChromeSizes.SmallSpacerPx, ChromeSizes.TwoCh, ChromeSizes.HugeSpacerPx);
        _grid.RowDefinitions.Add(new(GridLength.Auto));
        _grid.RowDefinitions.Add(new(GridLength.Auto));
        Child = _grid;

        _panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
        Grid.SetRow(_panel, 0);
        _grid.Children.Add(_panel);

        _message.IsVisible = false;
        _message.FontSize = ChromeFonts.SmallFontSize;
        _message.Margin = new(0.0, ChromeSizes.OneCh, 0.0, 0.0);
        _message.Foreground = ChromeStyling.ForegroundGray;
        _message.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _message.TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis;
        Message = "Doing stuff...";
        Grid.SetRow(_message, 1);
        _grid.Children.Add(_message);

        var size = ChromeFonts.DefaultFontSize;
        _panel.Spacing = 2.0 * size / 3.0;

        for (int n = 0; n < IndicatorCount; ++n)
        {
            var circle = new Ellipse();
            circle.Width = size;
            circle.Height = size;

            _indicators[n] = circle;
            _panel.Children.Add(circle);
        }

        _timer.Tick += TimerTickHandler;
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
        _timer.Start();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    private void Reset()
    {
        _counter = 0;
        _overlap = false;
        _timer.Interval = Delay0;

        foreach (var item in _indicators)
        {
            item.Fill = ChromeStyling.ForegroundGray;
        }
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        if (_overlap)
        {
            _overlap = false;
            _timer.Interval = Delay0;
        }
        else
        {
            _overlap = true;
            _timer.Interval = Delay1;

            if (++_counter > IndicatorCount)
            {
                Reset();
                return;
            }
        }

        for (int n = 0; n < _counter; ++n)
        {
            if (n == _counter - 1 || (_overlap && n == _counter - 2))
            {
                _indicators[n].Fill = Styling.AccentBrush;
                continue;
            }

            _indicators[n].Fill = ChromeStyling.ForegroundGray;
        }
    }
}