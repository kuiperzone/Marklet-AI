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

using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Code blocks.
/// </summary>
internal sealed class MarkCodeHost : MarkTextHost
{
    private sealed class InnerBorder : Border;

    private readonly StackPanel _panel = new();
    private readonly ScrollViewer _scroller = new();
    private readonly DispatcherTimer _scrollTimer = new();
    private readonly DockPanel? _dock;
    private readonly InnerBorder? _border;
    private readonly TextBlock? _langLabel;
    private readonly LightButton? _copyButton;
    private readonly LightButton? _wrapButton;
    private readonly Rectangle? _rule;

    private bool _initialized;
    private bool _isWrapped;
    private double _scrollDelta;
    private double _scrollPosY;

    public MarkCodeHost(MarkView owner, IReadOnlyMarkBlock source)
        : base(owner, source)
    {
        ConditionalDebug.ThrowIfFalse(Kind.IsCode());

        // If these change, we want to know so we can adapt
        ConditionalDebug.ThrowIfFalse(double.IsNaN(CrossText.Width));
        ConditionalDebug.ThrowIfNotEqual(HorizontalAlignment.Stretch, CrossText.HorizontalAlignment);

        // COMMON CONTROLS
        _panel.Orientation = Orientation.Vertical;
        _panel.VerticalAlignment = VerticalAlignment.Top;

        _scroller.Focusable = false;
        _scroller.VerticalAlignment = VerticalAlignment.Top;
        _scroller.VerticalContentAlignment = VerticalAlignment.Top;
        _scroller.HorizontalContentAlignment = HorizontalAlignment.Left;

        _scrollTimer.Interval = TimeSpan.FromMilliseconds(50);
        _scrollTimer.Tick += ScrollTimerTickHandler;

        if (Kind == BlockKind.FencedCode)
        {
            // FENCED CODE ONLY
            _dock = new();

            // Not focusable - there could be many
            _copyButton = new();
            DockPanel.SetDock(_copyButton, Dock.Right);
            _copyButton.Tip = "Copy";
            _copyButton.Focusable = false;
            _copyButton.Content = Symbols.ContentCopy + " Copy";
            _copyButton.VerticalAlignment = VerticalAlignment.Center;
            _copyButton.Click += CopyClickHandler;
            _dock.Children.Add(_copyButton);

            _wrapButton = new();
            DockPanel.SetDock(_wrapButton, Dock.Right);
            _wrapButton.Tip = "Wrap";
            _wrapButton.Focusable = false;
            _wrapButton.Content = Symbols.WrapText;
            _wrapButton.VerticalAlignment = VerticalAlignment.Center;
            _wrapButton.Click += WrapClickHandler;
            _dock.Children.Add(_wrapButton);

            _langLabel = new();
            _langLabel.FontWeight = FontWeight.Bold;
            _langLabel.TextWrapping = TextWrapping.NoWrap;
            _langLabel.VerticalAlignment = VerticalAlignment.Center;
            _langLabel.TextTrimming = TextTrimming.CharacterEllipsis;
            _dock.Children.Add(_langLabel);

            _rule = new();
            _panel.Children.Add(_dock);
            _panel.Children.Add(_rule);

            // Order important
            // Must come after rule
            _scroller.Content = CrossText;
            _scroller.SizeChanged += ScrollerSizeChangedHandler;
            _panel.Children.Add(_scroller);

            _border = new();
            _border.Child = _panel;
            Control = _border;
            SetScrollWrap(owner.DefaultWrapping);
            return;
        }

        // Other monospace
        _scroller.Content = CrossText;
        _panel.Children.Add(_scroller);
        Control = _panel;
        SetScrollWrap(owner.DefaultWrapping);
    }

    public override void RefreshLook(bool top, bool bottom)
    {
        base.RefreshLook(top, bottom);
        RefreshInternal();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override MarkConsumed ConsumeUpdates(IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        bool pending = IsPending;
        var rslt = base.ConsumeUpdates(sequence, ref index);

        if (rslt == MarkConsumed.Changed)
        {
            if (pending || true)
            {
                RefreshInternal();
            }

            _scroller?.ScrollToHome();
            _scrollTimer?.Stop();
            _langLabel?.Text = Source.Lang;
        }

        return rslt;
    }

    /// <summary>
    /// Extends.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder(base.ToString());

        Append(buffer, "Scroller.Focusable", _scroller.Focusable);
        Append(buffer, "Scroller.VerticalAlignment", _scroller.VerticalAlignment);
        Append(buffer, "Scroller.HorizontalAlignment", _scroller.HorizontalAlignment);
        Append(buffer, "Scroller.VerticalContentAlignment", _scroller.VerticalContentAlignment);
        Append(buffer, "Scroller.HorizontalContentAlignment", _scroller.HorizontalContentAlignment);

        Append(buffer, "Lang.Text", _langLabel?.Text);
        Append(buffer, "Lang.FontSize", _langLabel?.FontSize);
        Append(buffer, "Lang.FontFamily", _langLabel?.FontFamily);

        Append(buffer, "Border.MinWidth", _border?.MinWidth);
        Append(buffer, "Border.BorderBrush", _border?.BorderBrush);
        Append(buffer, "Border.Background", _border?.Background);

        return buffer.ToString();
    }

    protected override void SetChildMargin(bool isFirst, bool isLast)
    {
        if (Kind == BlockKind.FencedCode)
        {
            var pad = Owner.FontSize * 3.0;
            Control.Margin = new(0, pad, 0, pad);
            return;
        }

        double indent = Kind.IsIndented() ? Owner.TabPx : 0.0;
        Control.Margin = new(indent, Owner.OneCh * 2.0, 0.0, 0.0);
    }

    private void RefreshInternal()
    {
        var ch = Owner.OneCh;
        CrossText.Padding = new(0, ch, 0, ch * 3.0);

        if (Kind == BlockKind.FencedCode)
        {
            // FENCED CODE ONLY
            _panel.Spacing = ch * 1.5;
            _panel.Margin = new(ch * 3, ch * 1.5, ch * 3, ch);
            CrossText.SetForeground(Owner.FencedForeground ?? Owner.Foreground);

            // Internal controls not expected to be null
            _dock!.MinHeight = Owner.ScaledLineHeight;

            _border!.MinWidth = ch * 16.0;
            _border.BorderBrush = Owner.FencedBorder;
            _border.SetBackground(Owner.FencedBackground);

            if (Owner.FencedBorder != null)
            {
                _border.CornerRadius = Owner.FencedCornerRadius;
                _border.BorderThickness = new(Owner.LinePixels);
            }
            else
            {
                _border.CornerRadius = default;
                _border.BorderThickness = default;
            }

            _langLabel!.FontFamily = Owner.HeadingFamily;
            _langLabel.FontSize = Owner.ScaledFontSize * Owner.HeadingSizeCorrection;
            _langLabel.SetForeground(Owner.Foreground);

            _wrapButton!.Foreground = Owner.Foreground;
            _wrapButton.FontSize = Owner.ScaledFontSize;

            _copyButton!.Foreground = Owner.Foreground;
            _copyButton.FontSize = Owner.ScaledFontSize;

            SetRuleBrush(Owner.FencedBorder);
            return;
        }

        CrossText.SetForeground(Owner.Foreground);
    }

    private void SetScrollWrap(bool codeWrap)
    {
        bool init = _initialized;

        // Once only
        _initialized = true;

        // Break this down
        bool showScroller = true;

        // Here codeWrap applies only to fenced and indented.
        // We initialize on construction, but only fenced code has button.
        if (Kind == BlockKind.FencedCode || Kind == BlockKind.IndentedCode)
        {
            showScroller = !codeWrap;
        }

        if (showScroller && (_isWrapped || !init))
        {
            _initialized = true;
            _isWrapped = false;
            _wrapButton?.IsChecked = false;

            CrossText.TextWrapping = TextWrapping.NoWrap;
            _scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            _panel.PointerMoved += ScrollerPointerMovedHandler;
            _panel.PointerReleased += ScrollerPointerReleasedHandler;
            _panel.PointerExited += ScrollerPointerExitedHandler;
            return;
        }

        if (!showScroller && (!_isWrapped || !init))
        {
            _isWrapped = true;
            _wrapButton?.IsChecked = true;

            _scrollTimer.Stop();
            CrossText.TextWrapping = TextWrapping.Wrap;
            _scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            _panel.PointerMoved -= ScrollerPointerMovedHandler;
            _panel.PointerReleased -= ScrollerPointerReleasedHandler;
            _panel.PointerExited -= ScrollerPointerExitedHandler;
        }
    }

    private void SetRuleBrush(IBrush? brush)
    {
        if (brush != null)
        {
            _rule?.Fill = brush;
            _rule?.Height = Owner.LinePixels;
            return;
        }

        _rule?.Fill = null;
        _rule?.Height = 0;
    }

    private void ScrollerPointerMovedHandler(object? _, PointerEventArgs e)
    {
        if (_scroller != null && _scroller.Extent.Width > _scroller.Viewport.Width && CrossText.HasSelection)
        {
            var info = e.GetCurrentPoint(_panel);
            var props = info.Properties;

            if (props.IsLeftButtonPressed)
            {
                var point = info.Position;

                _scrollDelta = 0;

                if (point.X < 0)
                {
                    _scrollDelta = -Owner.FontSize;
                }
                else
                if (point.X >= _panel.Bounds.Width)
                {
                    _scrollDelta = Owner.FontSize;
                }

                if (_scrollDelta != 0)
                {
                    _scrollPosY = Math.Max(point.Y - CrossText.Margin.Top - CrossText.Padding.Top, 0.0);
                    _scrollTimer.Restart();
                    ScrollTimerTickHandler(null, EventArgs.Empty);
                    return;
                }
            }
        }

        _scrollTimer.Stop();
    }

    private void ScrollerPointerReleasedHandler(object? _, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            _scrollTimer.Stop();
        }
    }

    private void ScrollerPointerExitedHandler(object? _, PointerEventArgs e)
    {
        _scrollTimer.Stop();
    }

    private void ScrollerSizeChangedHandler(object? _, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width != e.PreviousSize.Width)
        {
            CrossText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var uw = CrossText.DesiredSize.Width;
            _wrapButton?.IsVisible = uw > _scroller.Bounds.Width;
        }
    }

    private void ScrollTimerTickHandler(object? _, EventArgs __)
    {
        if (_isWrapped || Control?.IsEffectivelyVisible != true)
        {
            // Dead
            _scrollTimer.Stop();
            return;
        }

        var offX = _scroller.Offset.X;
        _scroller.Offset = new(offX + _scrollDelta, _scroller.Offset.Y);

        if (offX == _scroller.Offset.X || _scroller.Bounds.Width < 3.0)
        {
            // Clamped or wrapping
            _scrollTimer.Stop();
            return;
        }

        // Scroll left or right
        double x = _scrollDelta < 0.0 ? 0.0 : _scroller.Bounds.Width - 1.0;
        CrossText.SelectExtend(new Point(x + _scroller.Offset.X, _scrollPosY));
    }

    private void WrapClickHandler(object? _, RoutedEventArgs __)
    {
        // Toggle
        SetScrollWrap(!_isWrapped);
    }

    private void CopyClickHandler(object? _, RoutedEventArgs __)
    {
        const string NSpace = $"{nameof(MarkCodeHost)}.{nameof(CopyClickHandler)}";
        ConditionalDebug.WriteLine(NSpace, "Clicked");
        CrossText.CopyText(WhatText.SelectedOrAll);
    }
}