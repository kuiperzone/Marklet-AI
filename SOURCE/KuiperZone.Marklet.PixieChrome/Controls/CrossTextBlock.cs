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
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using System.Text;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Provides cross-selectable text derived directly from <see cref="TextBlock"/>.
/// </summary>
/// <remarks>
/// The <see cref="CrossTextBlock"/> class is similar to <see cref="SelectableTextBlock"/>, however, it may share a
/// common <see cref="CrossTracker"/> instance with others, allowing for the cross selection of text from multiple
/// instances arranged vertically in the view. It also supports non-jankey clickable URI links with <see
/// cref="CrossRun"/>. It will respond to CTRL+C and CTRL+A provided Focusable is to true.
/// </remarks>
public class CrossTextBlock : TextBlock, ICrossTrackable, ICrossTrackOwner
{
    private Cursor? _holdCursor;
    private CrossRun? _hoverLink;
    private IBrush? _hoverForeground;
    private TextDecorationCollection? _hoverDecorations;

    private CrossTracker? _tracker;
    private Visual? _rootTop;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// Assigns default ContextMenu.
    /// </remarks>
    public CrossTextBlock()
    {
        // Don't use default property (must be explicitly set)
        Focusable = true;
        IsTabStop = false;
        FocusAdorner = null;

        ContextMenu = CrossContextMenu.Global;
        Cursor = ChromeCursors.IBeam;
    }

    /// <summary>
    /// Constructor with initial <see cref="CrossTracker"/> and menu.
    /// </summary>
    /// <remarks>
    /// The instance is not focusable. Intended where the instance is one of many children within a more sophisticated
    /// control.
    /// </remarks>
    public CrossTextBlock(ContextMenu? menu)
    {
        Cursor = ChromeCursors.IBeam;

        if (menu != null)
        {
            ContextMenu = menu;
        }

        // Ensure off
        Diag.ThrowIfTrue(Focusable);
    }

    /// <summary>
    /// Fixed default value for <see cref="SelectionBrushProperty"/>.
    /// </summary>
    public static readonly ImmutableSolidColorBrush DefaultSelectionBrush = new(0x803584E4);

    /// <summary>
    /// Fixed default value for links.
    /// </summary>
    /// <remarks>
    /// A mid-intensity "blue" used as the default brush where <see cref="CrossRun.Uri"/> is not null.
    /// </remarks>
    public static readonly ImmutableSolidColorBrush DefaultLinkBrush = new(0xFF3584E4);

    /// <summary>
    /// Fixed default value for <see cref="LinkHoverBrushProperty"/>.
    /// </summary>
    public static readonly ImmutableSolidColorBrush DefaultHoverLinkHoverBrush = new(0xB03584E4);

    /// <summary>
    /// Defines the <see cref="SelectionBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> SelectionBrushProperty =
        AvaloniaProperty.Register<CrossTextBlock, IBrush>(nameof(SelectionBrush), DefaultSelectionBrush);

    /// <summary>
    /// Defines the <see cref="LinkHoverBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LinkHoverBrushProperty =
        AvaloniaProperty.Register<CrossTextBlock, IBrush?>(nameof(LinkHoverBrush), DefaultHoverLinkHoverBrush);

    /// <summary>
    /// Defines the <see cref="LinkHoverUnderline"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> LinkHoverUnderlineProperty =
        AvaloniaProperty.Register<CrossTextBlock, bool>(nameof(LinkHoverUnderline), true);

    /// <inheritdoc cref="ICrossTrackOwner.Tracker"/>
    public CrossTracker? Tracker
    {
        get { return _tracker; }

        set
        {
            if (_tracker != value)
            {
                _tracker?.RemoveInternal(this);

                _tracker = value;

                if (value != null && this.IsAttachedToVisualTree())
                {
                    value.AddInternal(this);
                }
            }
        }
    }

    /// <inheritdoc cref="ICrossTrackable.TrackKey"/>
    public CrossKey TrackKey
    {
        get
        {
            if (_rootTop != null)
            {
                return CrossKey.GetKeyPoint(this, _tracker?.Container ?? _rootTop);
            }

            return CrossKey.Empty;
        }
    }

    /// <inheritdoc cref="ICrossTrackable.TrackPrefix"/>
    public string? TrackPrefix { get; set; }

    /// <inheritdoc cref="ICrossTrackable.TrackSeparator"/>
    public string? TrackSeparator { get; set; }

    /// <inheritdoc cref="ICrossTrackable.IsEmpty"/>
    public bool IsEmpty
    {
        get
        {
            if (Inlines != null && Inlines.Count != 0)
            {
                return false;
            }

            return string.IsNullOrEmpty(Text);
        }
    }

    /// <inheritdoc cref="ICrossTrackable.TextLength"/>
    public int TextLength
    {
        get
        {
            if (Inlines?.Count > 0)
            {
                int length = 0;

                var inlines = Inlines;
                int count = inlines.Count;

                for (int n = 0; n < count; ++n)
                {
                    if (inlines[n] is Run run && run.Text != null)
                    {
                        length += run.Text.Length;
                    }
                }

                return Math.Max(length, 0);
            }

            return Text?.Length ?? 0;
        }
    }

    /// <inheritdoc cref="ICrossTrackable.SelectionStart"/>
    public int SelectionStart { get; private set; }

    /// <inheritdoc cref="ICrossTrackable.SelectionEnd"/>
    public int SelectionEnd { get; private set; }

    /// <inheritdoc cref="ICrossTrackable.HasSelection"/>
    public bool HasSelection
    {
        get { return SelectionStart != SelectionEnd && !IsEmpty; }
    }

    /// <inheritdoc cref="ICrossTrackable.IsPointerSelectEnabled"/>
    public bool IsPointerSelectEnabled
    {
        get { return IsEffectivelyEnabled; }
    }

    /// <summary>
    /// Gets whether the instance has styled inline content.
    /// </summary>
    public bool HasComplexContent
    {
        get { return Inlines != null && Inlines.Count != 0; }
    }

    /// <summary>
    /// Gets or sets the selection background brush.
    /// </summary>
    /// <remarks>
    /// Note that there is no "SelectionForeground" property. Instead, <see cref="SelectionBrush"/> should always be set
    /// to a semi-opaque (approx 50%) brush.
    /// </remarks>
    public IBrush SelectionBrush
    {
        get { return GetValue(SelectionBrushProperty); }
        set { SetValue(SelectionBrushProperty, value); }
    }

    /// <summary>
    /// Gets or sets the foreground brush for hover links.
    /// </summary>
    /// <remarks>
    /// A value of null implies no color change on hover. The default is a shade of blue.
    /// </remarks>
    public IBrush? LinkHoverBrush
    {
        get { return GetValue(LinkHoverBrushProperty); }
        set { SetValue(LinkHoverBrushProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether links are underlined when hovered.
    /// </summary>
    public bool LinkHoverUnderline
    {
        get { return GetValue(LinkHoverUnderlineProperty); }
        set { SetValue(LinkHoverUnderlineProperty, value); }
    }

    /// <inheritdoc cref="ICrossTrackable.SelectNone"/>
    public bool SelectNone()
    {
        if (SelectionStart != 0 || SelectionEnd != 0)
        {
            return Select(0, 0);
        }

        return false;
    }

    /// <inheritdoc cref="ICrossTrackable.Select(int, int)"/>
    public bool Select(int start, int end)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(Select)}";
        Diag.WriteLine(NSpace, $"start: {start}, {SelectionStart}");

        if (Tracker != null)
        {
            return Tracker.SelectSingle(this, start, end);
        }

        return SelectInternal(start, end);
    }

    /// <inheritdoc cref="ICrossTrackable.SelectAll"/>
    public bool SelectAll()
    {
        return Select(0, TextLength);
    }

    /// <summary>
    /// Extends current selection, <see cref="SelectionStart"/>, to a pixel point relative the control's bounds.
    /// </summary>
    /// <remarks>
    /// The result is true on success, or false if <see cref="HasSelection"/> is false when called.
    /// </remarks>
    public bool SelectExtend(Point endPoint)
    {
        if (HasSelection)
        {
            // No need to call tracker
            return SelectInternal(SelectionStart, GetTextPosition(endPoint));
        }

        return false;
    }

    /// <inheritdoc cref="ICrossTrackable.GetEffectiveText(WhatText)"/>
    public string? GetEffectiveText(WhatText what)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(GetEffectiveText)}";
        Diag.WriteLine(NSpace, what);

        if (!GetNormalizedSelectedRange(out int start, out int end, out int length))
        {
            Diag.WriteLine(NSpace, "No selection");

            if (what == WhatText.SelectedOrNull)
            {
                return null;
            }

            start = 0;
            end = length;
        }

        if (what == WhatText.All)
        {
            start = 0;
            end = length;
        }

        Diag.WriteLine(NSpace, $"{nameof(HasComplexContent)}: {HasComplexContent}");

        if (HasComplexContent)
        {
            return GetComplexText(start, end);
        }

        var text = Text;
        Diag.WriteLine(NSpace, $"Plain range: [{start}, {end})");
        return text?.Substring(start, end - start);
    }

    /// <inheritdoc cref="ICrossTrackable.GetTextPosition(Point)"/>
    public int GetTextPosition(Point point)
    {
        point -= new Point(Padding.Left, Padding.Top);
        return TextLayout.HitTestPoint(point).TextPosition;
    }

    /// <summary>
    /// Implements internal interface method.
    /// </summary>
    bool ICrossTrackable.SelectInternal(int start, int end)
    {
        return SelectInternal(start, end);
    }

    /// <summary>
    /// Copies either all or selected text to the clipboard according to "what" and returns true on
    /// success, or false if nothing was copied.
    /// </summary>
    public bool CopyText(WhatText what)
    {
        return this.CopyToClipboard(GetEffectiveText(what));
    }

    /// <summary>
    /// Gets the normalized range and returns true where "start" is less than "end".
    /// </summary>
    /// <remarks>
    /// Where <see cref="IsEmpty"/> is true, the result is always false with both "start" and "end" equals to 0.
    /// </remarks>
    public bool GetNormalizedSelectedRange(out int start, out int end)
    {
        return GetNormalizedSelectedRange(out start, out end, out _);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _rootTop = e.Root as Visual;
        _tracker?.AddInternal(this);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _rootTop = null;
        _tracker?.RemoveInternal(this);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(OnPointerPressed)}";

        base.OnPointerPressed(e);
        var info = e.GetCurrentPoint(this);
        var props = info.Properties;

        if (!props.IsLeftButtonPressed)
        {
            return;
        }

        if (_hoverLink?.Uri != null)
        {
            // Must get copy before reset
            var uri = _hoverLink.Uri;

            ResetHover();
            SelectNone();

            if (Tracker?.OnLinkClick(uri) != true)
            {
                ChromeApplication.SafeLaunchUri(uri);
            }

            return;
        }

        if (IsPointerSelectEnabled)
        {
            Diag.WriteLine(NSpace, $"Left press for {nameof(TrackKey)}: {TrackKey}");
            Diag.WriteLine(NSpace, $"Input point: {info.Position}");

            // Important to get key
            // strokes if we are focusable
            Focus();

            var pos = GetTextPosition(info.Position);
            Diag.WriteLine(NSpace, $"Hit Pos: {pos}");

            // Order important.
            // Call public in order to force clear on tracker and set this instance.
            Select(pos, pos);

            // Do this last
            _tracker?.SetAnchor(this, pos);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var info = e.GetCurrentPoint(this);
        var props = info.Properties;

        if (props.IsRightButtonPressed)
        {
            return;
        }

        // Do not replace IsInsideBounds() for IsPointerOver
        if (!props.IsLeftButtonPressed)
        {
            if (IsInsideContent(info.Position))
            {
                // LINK HOVER
                // Not easy to get right (care needed with resets)
                SetHoverCursor(GetLinkAtIndex(GetTextPosition(info.Position)));
                return;
            }

            ResetHover();
            return;
        }

        // LEFT BUTTON IS PRESSED HERE
        ResetHover();

        if (IsPointerSelectEnabled)
        {
            SelectInternal(SelectionStart, GetTextPosition(info.Position));
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        ResetHover();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        ResetHover();

        if (_tracker == null)
        {
            SelectNone();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void RenderTextLayout(DrawingContext context, Point origin)
    {
        // We must take care not invalidate the instance here.
        if (SelectionStart != SelectionEnd)
        {
            var brush = SelectionBrush;

            if (brush != null)
            {
                // Presumably we don't need to clamp range here.
                var start = Math.Min(SelectionStart, SelectionEnd);
                var length = Math.Max(SelectionStart, SelectionEnd) - start;

                var rects = TextLayout.HitTestTextRange(start, length);

                using (context.PushTransform(Matrix.CreateTranslation(origin)))
                {
                    foreach (var items in rects)
                    {
                        context.FillRectangle(brush, PixelRect.FromRect(items, 1.0).ToRect(1.0));
                    }
                }
            }
        }

        base.RenderTextLayout(context, origin);
    }

    /// <summary>
    /// Overrides. The instance must be Focusable and Focused for this to work.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = CopyText(WhatText.SelectedOrNull);
            return;
        }

        if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = SelectAll();
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = SelectNone();
            return;
        }
    }

    private bool SetHoverCursor(CrossRun? link)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(SetHoverCursor)}";

        if (link != null)
        {
            if (link != _hoverLink)
            {
                Diag.WriteLine(NSpace, "Hovering");
                ResetHover();

                // Hold
                _hoverLink = link;
                _holdCursor = Cursor;
                _hoverForeground = link.Foreground;
                _hoverDecorations = link.TextDecorations;

                // Temporary
                Cursor = ChromeCursors.Hand;

                if (LinkHoverUnderline)
                {
                    link.TextDecorations = Avalonia.Media.TextDecorations.Underline;
                }

                var hover = LinkHoverBrush;

                if (hover != null)
                {
                    link.Foreground = hover;
                }
            }

            return true;
        }

        ResetHover();
        return false;
    }

    private bool ResetHover()
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(ResetHover)}";

        if (_hoverLink != null)
        {
            Diag.WriteLine(NSpace, "Reset hover");
            Cursor = _holdCursor;
            _hoverLink.Foreground = _hoverForeground;
            _hoverLink.TextDecorations = _hoverDecorations;

            _hoverLink = null;
            _holdCursor = null;
            _hoverForeground = null;
            _hoverDecorations = null;
            return true;
        }

        return false;
    }

    private bool GetNormalizedSelectedRange(out int start, out int end, out int totalLength)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(GetNormalizedSelectedRange)}";

        // Always set this irrespective of selection
        // We use this to spare calculating the length again by the caller
        totalLength = TextLength;

        start = Math.Min(Math.Min(SelectionStart, SelectionEnd), totalLength);
        end = Math.Min(Math.Max(SelectionStart, SelectionEnd), totalLength);

        Diag.WriteLine(NSpace, $"[{start}, {end}), length: {totalLength}");
        return start < totalLength && start < end;
    }

    private string GetComplexText(int start, int end)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(GetComplexText)}";
        Diag.WriteLine(NSpace, $"Range: [{start}, {end})");

        if (start >= end || Inlines == null)
        {
            // If we are here, then we have complex
            // content, and the result is never null.
            return "";
        }

        int count = 0;
        int total = end - start;

        StringBuilder? buffer = null;

        foreach (var item in Inlines)
        {
            if (item is Run run && run.Text != null)
            {
                if (total > 0)
                {
                    var frag = run.Text;
                    int fragLen = frag.Length;

                    if (start < count + fragLen)
                    {
                        int t0 = Math.Max(start - count, 0);
                        int subLen = Math.Min(fragLen - t0, total);

                        total -= subLen;
                        // ConditionalDebug.WriteLine(NSpace, $"Substring: {t0}, {subLen}, where total now: {total}");

                        frag = frag.Substring(t0, subLen);
                        // ConditionalDebug.WriteLine(NSpace, $"Frag: `{frag}`");

                        buffer ??= new(Math.Max(subLen, 64));

                        switch (run.BaselineAlignment)
                        {
                            case BaselineAlignment.Superscript:
                                buffer.Append(frag.ToSuperscript());
                                break;
                            case BaselineAlignment.Subscript:
                                buffer.Append(frag.ToSubscript());
                                break;
                            default:
                                buffer.Append(frag);
                                break;
                        }
                    }

                    count += fragLen;
                    continue;
                }

                break;
            }
        }

        return buffer?.ToString() ?? "";
    }

    private CrossRun? GetLinkAtIndex(int index)
    {
        if (Inlines != null && Inlines.Count != 0)
        {
            int count = 0;
            var inlines = Inlines;

            for (int n = 0; n < inlines.Count; ++n)
            {
                if (inlines[n] is CrossRun span)
                {
                    if (span.Text != null)
                    {
                        int length = span.Text.Length;

                        if (index >= count && index < count + length)
                        {
                            return span.Uri != null ? span : null;
                        }

                        count += length;
                    }

                    continue;
                }

                if (inlines[n] is Run run && run.Text != null)
                {
                    int length = run.Text.Length;

                    if (index >= count && index < count + length)
                    {
                        return null;
                    }

                    count += length;
                }
            }
        }

        return null;
    }

    private bool IsInsideContent(Point point)
    {
        // Point is relative to Control (not its parent)
        var b = Bounds;
        var p = Padding;
        return point.X >= p.Left && point.X < b.Width - p.Right && point.Y >= p.Top && point.Y < b.Height - p.Bottom;
    }

    private bool SelectInternal(int start, int end)
    {
        // Do not call tracker
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(SelectInternal)}";

        // We can clamp against 0 which is cheap.
        // But we do not want to clamp on text length
        // which could be expensive and doesn't appear necessary.
        start = Math.Max(start, 0);
        end = Math.Max(end, 0);

        if (SelectionStart != start || SelectionEnd != end)
        {
            Diag.WriteLine(NSpace, $"start: {start}, {SelectionStart}");
            Diag.WriteLine(NSpace, $"end: {end}, {SelectionEnd}");
            SelectionStart = start;
            SelectionEnd = end;

            InvalidateVisual();
            return true;
        }

        return false;
    }
}