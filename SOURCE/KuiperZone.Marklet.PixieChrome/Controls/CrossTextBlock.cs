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
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using System.Text;
using Avalonia.VisualTree;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using Avalonia.Interactivity;

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
    private int _textLengthChange;

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
        Cursor = ChromeCursors.IBeamCursor;
    }

    /// <summary>
    /// Constructor which assigns <see cref="Tracker"/>
    /// </summary>
    public CrossTextBlock(CrossTracker tracker)
        : this()
    {
        _tracker = tracker;
        TrackKey = CrossTracker.NextKey();
    }

    /// <summary>
    /// Constructor with initial <see cref="CrossTracker"/> and menu.
    /// </summary>
    /// <remarks>
    /// Intended where the instance is one of many children within a more sophisticated control.
    /// </remarks>
    public CrossTextBlock(CrossTracker tracker, ContextMenu? menu)
    {
        _tracker = tracker;
        TrackKey = CrossTracker.NextKey();
        Cursor = ChromeCursors.IBeamCursor;

        if (menu != null)
        {
            ContextMenu = menu;
        }

        // Ensure off
        ConditionalDebug.ThrowIfTrue(Focusable);
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
    /// Defines the <see cref="LinkClick"/> event.
    /// </summary>
    public static readonly RoutedEvent<LinkClickEventArgs> LinkClickEvent =
        RoutedEvent.Register<CrossTextBlock, LinkClickEventArgs>(nameof(LinkClick), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="SelectionBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<CrossTextBlock, IBrush?>(nameof(SelectionBrush), DefaultSelectionBrush);

    /// <summary>
    /// Defines the <see cref="LinkHoverBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LinkHoverBrushProperty =
        AvaloniaProperty.Register<CrossTextBlock, IBrush?>(nameof(LinkHoverBrush), DefaultHoverLinkHoverBrush);

    /// <summary>
    /// Defines the <see cref="LinkHoverUnderline"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> LinkHoverUnderlineProperty =
        AvaloniaProperty.Register<CrossTextBlock, bool>(nameof(LinkHoverBrush), true);

    /// <summary>
    /// Defines the <see cref="Tracker"/> property.
    /// </summary>
    public static readonly DirectProperty<CrossTextBlock, CrossTracker?> TrackerProperty =
        AvaloniaProperty.RegisterDirect<CrossTextBlock, CrossTracker?>(nameof(Tracker),
            o => o.Tracker, (o, v) => o.Tracker = v);

    /// <summary>
    /// Occurs when the user clicks on an URI within the text.
    /// </summary>
    /// <remarks>
    /// When a link is clicked, the default behaviour is to attempt to open the link in an external browser. However,
    /// this event is invoked first and, if <see cref="RoutedEventArgs.Handled"/> is set to true, the link will NOT be
    /// opened when the event returns.
    /// </remarks>
    public event EventHandler<LinkClickEventArgs> LinkClick
    {
        add { AddHandler(LinkClickEvent, value); }
        remove { RemoveHandler(LinkClickEvent, value); }
    }

    /// <inheritdoc cref="ICrossTrackOwner.Tracker"/>
    public CrossTracker? Tracker
    {
        get { return _tracker; }
        set { SetAndRaise(TrackerProperty, ref _tracker, value); }
    }

    /// <inheritdoc cref="ICrossTrackable.TrackKey"/>
    public ulong TrackKey { get; private set; }

    /// <inheritdoc cref="ICrossTrackable.HasSelection"/>
    public bool HasSelection
    {
        get { return SelectionStart != SelectionEnd && !IsEmpty; }
    }

    /// <inheritdoc cref="ICrossTrackable.HasComplexContent"/>
    public bool HasComplexContent
    {
        get { return Inlines != null && Inlines.Count != 0; }
    }

    /// <inheritdoc cref="ICrossTrackable.SelectionStart"/>
    public int SelectionStart { get; private set; }

    /// <inheritdoc cref="ICrossTrackable.SelectionEnd"/>
    public int SelectionEnd { get; private set; }

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
            if (Inlines != null && Inlines.Count != 0)
            {
                int length = 0;

                foreach (var item in Inlines)
                {
                    if (item is Run run && run.Text != null)
                    {
                        length += run.Text.Length;
                    }
                }

                return length;
            }

            return Text?.Length ?? 0;
        }
    }

    /// <summary>
    /// Gets or sets the selection background brush.
    /// </summary>
    /// <remarks>
    /// Note that there is no "SelectionForeground" property. Instead, the <see cref="SelectionBrush"/> should always be
    /// set to a semi-opaque (approx 50%) brush. Setting to null disables selection of text.
    /// </remarks>
    public IBrush? SelectionBrush
    {
        get { return GetValue(SelectionBrushProperty); }
        set { SetValue(SelectionBrushProperty, value); }
    }

    /// <summary>
    /// Gets or sets the foreground brush for hover links.
    /// </summary>
    /// <remarks>
    /// A value of null shows implies no color change on hover. The default is a shade of blue.
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
        return Select(0, 0);
    }

    /// <inheritdoc cref="ICrossTrackable.Select(int, int)"/>
    public bool Select(int start, int end)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(Select)}";
        ConditionalDebug.WriteLine(NSpace, $"start: {start}, {SelectionStart}");

        if (Tracker != null)
        {
            return Tracker.SelectInternal(this, start, end);
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
            var hit = TextLayout.HitTestPoint(GetContentPoint(endPoint));
            return SelectInternal(SelectionStart, hit.TextPosition);
        }

        return false;
    }

    /// <inheritdoc cref="ICrossTrackable.GetEffectiveText(WhatText)"/>
    public string? GetEffectiveText(WhatText what)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(GetEffectiveText)}";
        ConditionalDebug.WriteLine(NSpace, what);

        if (!GetNormalizedSelectedRange(out int start, out int end, out int length))
        {
            ConditionalDebug.WriteLine(NSpace, "No selection");

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

        ConditionalDebug.WriteLine(NSpace, $"{nameof(HasComplexContent)}: {HasComplexContent}");

        if (HasComplexContent)
        {
            return GetComplexText(start, end);
        }

        var text = Text;
        ConditionalDebug.WriteLine(NSpace, $"Plain range: [{start}, {end})");
        return text?.Substring(start, end - start);
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

    bool ICrossTrackable.SelectInternal(int start, int end)
    {
        return SelectInternal(start, end);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(OnAttachedToVisualTree)}";

        base.OnAttachedToVisualTree(e);
        Tracker?.AddInternal(this);

        if (e.Root is TopLevel top)
        {
            ConditionalDebug.WriteLine(NSpace, "ATTACHED TO TOP");
            top.PointerMoved += TopLevelMovedHandler;
            top.PointerPressed += TopLevelPressedHandler;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Important or we could have a memory leak
        // Also called if IsVisible property is set to false.
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(OnDetachedFromVisualTree)}";

        base.OnDetachedFromVisualTree(e);
        Tracker?.RemoveInternal(this);

        if (e.Root is TopLevel top)
        {
            ConditionalDebug.WriteLine(NSpace, "DETACHED FROM TOP");
            top.PointerMoved -= TopLevelMovedHandler;
            top.PointerPressed -= TopLevelPressedHandler;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(OnPropertyChanged)}";

        var p = e.Property;
        base.OnPropertyChanged(e);

        if (p == TrackerProperty)
        {
            ConditionalDebug.WriteLine(NSpace, "ASSIGN NEW TRACKER");

            // Remove old
            e.GetOldValue<CrossTracker?>()?.RemoveInternal(this);

            var tracker = e.GetNewValue<CrossTracker?>();

            if (tracker != null)
            {
                TrackKey = CrossTracker.NextKey();
                ConditionalDebug.WriteLine(NSpace, $"New track key: {TrackKey}");

                if (this.IsAttachedToVisualTree())
                {
                    ConditionalDebug.WriteLine(NSpace, "Within visual tree");
                    tracker.AddInternal(this);
                }
            }
            else
            {
                ConditionalDebug.WriteLine(NSpace, "Tracker is null");
                TrackKey = 0;
            }

            return;
        }

        if (p == TextProperty ||
            p == IsEffectivelyEnabledProperty && !e.GetNewValue<bool>() ||
            (p == SelectionBrushProperty && e.GetNewValue<IBrush?>() == null))
        {
            ResetHover();

            if (HasSelection)
            {
                SelectNone();
            }

            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(OnPointerMoved)}";

        base.OnPointerMoved(e);

        if (_hoverLink == null)
        {
            var info = e.GetCurrentPoint(this);
            var props = info.Properties;

            if (props.IsLeftButtonPressed &&
                e.Pointer.Captured == this &&
                SelectionBrush != null &&
                _tracker?.SelectingCount != 0)
            {
                // Selection movement
                var point = GetContentPoint(info.Position);
                var hit = TextLayout.HitTestPoint(point);
                SelectInternal(SelectionStart, hit.TextPosition);
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(OnPointerPressed)}";

        // Needed!!!!
        e.Handled = true;
        var info = e.GetCurrentPoint(this);

        if (!info.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (_hoverLink?.Uri != null)
        {
            var uri = _hoverLink.Uri;

            ResetHover();
            SelectNone();

            var le = new LinkClickEventArgs(LinkClickEvent, this, uri);

            RaiseEvent(le);

            if (le.Handled)
            {
                return;
            }

            var top = TopLevel.GetTopLevel(this);

            if (top == null)
            {
                return;
            }

            Dispatcher.UIThread.Post(async () => await top.Launcher.LaunchUriAsync(uri) );
        }

        if (e.Pointer.Captured == this && SelectionBrush != null)
        {
            ConditionalDebug.WriteLine(NSpace, $"Left press for {nameof(TrackKey)}: {TrackKey}");
            ConditionalDebug.WriteLine(NSpace, $"Input point: {info.Position}");

            // Important bit
            Focus();
            _tracker?.StartSelect(this, true);

            var adjPoint = GetContentPoint(info.Position);
            ConditionalDebug.WriteLine(NSpace, $"Point in text: {adjPoint}");

            var hitPos = TextLayout.HitTestPoint(adjPoint).TextPosition;
            ConditionalDebug.WriteLine(NSpace, $"Hit Pos: {hitPos}");

            Select(hitPos, hitPos);
        }

        base.OnPointerPressed(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.Pointer.Captured == this)
        {
            e.Pointer.Capture(null);
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
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void RenderTextLayout(DrawingContext context, Point origin)
    {
        var length = TextLength;

        // Normally, we allow changes to Text without updating
        // the selection range. This works well, but here is
        // the only place where we should reset selection.
        if (_textLengthChange != length)
        {
            // We must take care not invalidate the instance here.
            _textLengthChange = length;
            SelectionStart = 0;
            SelectionEnd = 0;
            _tracker?.RemoveSelection(this);
        }

        if (SelectionStart != SelectionEnd)
        {
            var brush = SelectionBrush;

            if (brush != null)
            {
                // Presumably we don't need to clamp range here.
                var start = Math.Min(SelectionStart, SelectionEnd);
                length = Math.Max(SelectionStart, SelectionEnd) - start;

                // Avalonia 11.3.5 breaks this
                var rects = TextLayout.HitTestTextRange(start, length);

                using (context.PushTransform(Matrix.CreateTranslation(origin)))
                {
                    foreach (var items in rects)
                    {
                        context.FillRectangle(brush, PixelRect.FromRect(items, 1.0).ToRect(1));
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

    /// <summary>
    /// Needed to capture new blocks.
    /// </summary>
    private void TopLevelMovedHandler(object? _, PointerEventArgs e)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(TopLevelMovedHandler)}";
        var info = e.GetCurrentPoint(this);
        var props = info.Properties;

        // Do not replace IsInsideBounds() for IsPointerOver
        if (IsInsideBounds(info.Position) &&
            IsEffectivelyEnabled &&
            !props.IsRightButtonPressed &&
            !props.IsMiddleButtonPressed &&
            !props.IsBarrelButtonPressed)
        {
            if (!props.IsLeftButtonPressed && IsInsideContent(info.Position))
            {
                // LINK HOVER
                // Not easy to get right (care needed with resets)
                var adjPoint = GetContentPoint(info.Position);
                var index = TextLayout.HitTestPoint(adjPoint).TextPosition;
                SetHoverCursor(GetLinkAtIndex(index));
                return;
            }


            if (props.IsLeftButtonPressed &&
                SelectionBrush != null &&
                e.Pointer.Captured != this &&
                _tracker != null &&
                e.Pointer.Captured is CrossTextBlock lastCap && _tracker == lastCap.Tracker)
            {
                // CAPTURE
                e.Handled = true;
                info.Pointer.Capture(this);
                ConditionalDebug.WriteLine(NSpace, $"DRAG CAPTURE ON {nameof(TrackKey)}: {TrackKey}");
                ConditionalDebug.WriteLine(NSpace, $"Tracker selecting: {_tracker.HasValidSelection}");

                if (_tracker.SelectingCount != 0)
                {
                    // START DRAG
                    var index = TextLayout.HitTestPoint(GetContentPoint(info.Position)).TextPosition;

                    switch (_tracker.DragSelect(this))
                    {
                        case DragDirection.FromStart:
                            ConditionalDebug.WriteLine(NSpace, DragDirection.FromStart);
                            SelectInternal(SelectionStart, index);
                            break;
                        case DragDirection.FromEnd:
                            ConditionalDebug.WriteLine(NSpace, DragDirection.FromEnd);
                            SelectInternal(SelectionEnd, index);
                            break;
                        case DragDirection.LeftToRight:
                            ConditionalDebug.WriteLine(NSpace, DragDirection.LeftToRight);
                            SelectInternal(0, index);
                            break;
                        case DragDirection.RightToLeft:
                            ConditionalDebug.WriteLine(NSpace, DragDirection.RightToLeft);
                            SelectInternal(TextLength, index);
                            break;
                        default:
                            throw new InvalidOperationException($"Unexpected drag result");
                    }
                }

                ResetHover();
                return;
            }
        }

        ResetHover();
    }

    /// <summary>
    /// Needed to de-select when clicked outside of block.
    /// </summary>
    private void TopLevelPressedHandler(object? _, PointerPressedEventArgs e)
    {
        if (e.Pointer.Captured != this && IsEffectivelyEnabled)
        {
            var pos = e.GetPosition(this);
            var visual = this.GetVisualAt(pos);

            if (visual == null || visual == this)
            {
                SelectNone();
            }
        }
    }

    private bool SetHoverCursor(CrossRun? link)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(SetHoverCursor)}";

        if (link != null)
        {
            if (link != _hoverLink)
            {
                ConditionalDebug.WriteLine(NSpace, "Hovering");
                ResetHover();

                // Hold
                _hoverLink = link;
                _holdCursor = Cursor;
                _hoverForeground = link.Foreground;
                _hoverDecorations = link.TextDecorations;

                // Temporary
                Cursor = ChromeCursors.HandCursor;

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
            ConditionalDebug.WriteLine(NSpace, "Reset hover");
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

        ConditionalDebug.WriteLine(NSpace, $"[{start}, {end}), length: {totalLength}");
        return start < totalLength && start < end;
    }

    private string GetComplexText(int start, int end)
    {
        const string NSpace = $"{nameof(CrossTextBlock)}.{nameof(GetComplexText)}";
        ConditionalDebug.WriteLine(NSpace, $"Range: [{start}, {end})");

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
                        ConditionalDebug.WriteLine(NSpace, $"Substring: {t0}, {subLen}, where total now: {total}");

                        frag = frag.Substring(t0, subLen);
                        ConditionalDebug.WriteLine(NSpace, $"Frag: `{frag}`");

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
        if (Inlines != null)
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

    private Point GetContentPoint(Point point)
    {
        // Result could be negative
        return point - new Point(Padding.Left, Padding.Top);
    }

    private bool IsInsideBounds(Point point)
    {
        // Point is relative to Control (not its parent)
        var b = Bounds;
        return point.X >= 0 && point.X < b.Width && point.Y >= 0 && point.Y < b.Height;
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
        ConditionalDebug.WriteLine(NSpace, $"start: {start}, {SelectionStart}");
        ConditionalDebug.WriteLine(NSpace, $"end: {end}, {SelectionEnd}");

        start = Math.Max(start, 0);
        end = Math.Max(end, 0);

        if (SelectionStart != start || SelectionEnd != end)
        {
            SelectionStart = start;
            SelectionEnd = end;
            InvalidateVisual();
            return true;
        }

        return false;
    }
}