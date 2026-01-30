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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Displays markdown content.
/// </summary>
/// <remarks>
/// The class is not aware <see cref="ChromeStyling"/> and properties are "direct".
/// </remarks>
public class MarkView : MarkControl, ICrossTrackOwner
{
    /// <summary>
    /// Gets <see cref="ChromeStyling.Global"/> for convenience.
    /// </summary>
    protected static readonly ChromeStyling Styling = ChromeStyling.Global;

    /// <summary>
    /// The <see cref="DispatchCoalescer{T}.Posted"/> occurs whenever a property of this base class changes.
    /// </summary>
    /// <remarks>
    /// Subclass to handle the change.
    /// </remarks>
    protected readonly DispatchCoalescer? Refresher;

    private readonly StackPanel _panel = new();
    private readonly List<MarkVisualHost> _cache = new();

    private readonly MarkDocument _source = new();
    private Control? _header;
    private Control? _footer;

    private string? _content;
    private bool _isChromeStyled;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkView()
        : this(new CrossTracker())
    {
    }

    /// <summary>
    /// Constructor with shared selection host.
    /// </summary>
    public MarkView(CrossTracker tracker)
    {
        Focusable = true;
        IsTabStop = false;
        FocusAdorner = null;

        Tracker = tracker;
        ContextMenu = CrossContextMenu.Global;
        base.Child = _panel;

        Refresher = new(DispatcherPriority.Render);
        Refresher.Posted += RefresherHandler;
        Zoom.Changed += RefresherHandler;
    }

    /// <summary>
    /// Constructor with owner, the properties of which are copied to this.
    /// </summary>
    /// <remarks>
    /// Intended where the instance is one of many children within a more sophisticated control.
    /// </remarks>
    protected MarkView(MarkControl owner, CrossTracker tracker)
        : base(owner)
    {
        // Ensure these are off
        ConditionalDebug.ThrowIfTrue(Focusable);
        ConditionalDebug.ThrowIfTrue(IsTabStop);

        Tracker = tracker;
        base.Child = _panel;
    }

    /// <summary>
    /// Defines the <see cref="LinkClick"/> event.
    /// </summary>
    public static readonly RoutedEvent<LinkClickEventArgs> LinkClickEvent =
        RoutedEvent.Register<MarkView, LinkClickEventArgs>(nameof(LinkClick), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkView, string?> ContentProperty =
        AvaloniaProperty.RegisterDirect<MarkView, string?>(nameof(Content), o => o.Content, (o, v) => o.Content = v);

    /// <summary>
    /// Defines the <see cref="IsChromeStyled"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkView, bool> IsChromeStyledProperty =
        AvaloniaProperty.RegisterDirect<MarkView, bool>(nameof(IsChromeStyled), o => o.IsChromeStyled, (o, v) => o.IsChromeStyled = v);

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

    /// <summary>
    /// Gets or sets the markdown content.
    /// </summary>
    /// <remarks>
    /// Note that after setting, actual rending may occur later. The default value is null.
    /// </remarks>
    public string? Content
    {
        get { return _content; }
        set { SetAndRaise(ContentProperty, ref _content, value); }
    }

    /// <summary>
    /// Gets or sets whether visual properties follow <see cref="ChromeStyling"/> values and respond to changes.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool IsChromeStyled
    {
        get { return _isChromeStyled; }
        set { SetAndRaise(IsChromeStyledProperty, ref _isChromeStyled, value); }
    }

    /// <summary>
    /// Gets the shared <see cref="CrossTracker"/> instance.
    /// </summary>
    public CrossTracker Tracker { get; }

    /// <summary>
    /// Gets or sets the markdown options used for processing <see cref="Content"/>.
    /// </summary>
    public MarkOptions Options { get; set; }

    /// <summary>
    /// Gets or sets a header Control instance.
    /// </summary>
    /// <remarks>
    /// The control instance will be inserted at the top of the view.
    /// </remarks>
    public Control? Header
    {
        get { return _header; }

        set
        {
            if (_header != value)
            {
                if (_header != null)
                {
                    _panel.Children.Remove(_header);
                }

                _header = value;

                if (value != null)
                {
                    _panel.Children.Insert(0, value);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a footer Control instance.
    /// </summary>
    /// <remarks>
    /// The footer Control instance will be inserted at the top of the view.
    /// </remarks>
    public Control? Footer
    {
        get { return _footer; }

        set
        {
            if (_footer != value)
            {
                if (_footer != null)
                {
                    _panel.Children.Remove(_footer);
                }

                _footer = value;

                if (value != null)
                {
                    _panel.Children.Insert(_panel.Children.Count, value);
                }
            }
        }
    }

    /// <summary>
    /// Gets the first <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    /// <remarks>
    /// The value is 0 by default.
    /// </remarks>
    public ulong TrackKey0 { get; protected set; }

    /// <summary>
    /// Gets the last <see cref="ICrossTrackable.TrackKey"/> value consumed.
    /// </summary>
    /// <remarks>
    /// The <see cref="TrackKey1"/> value must always be equal or greater than <see cref="TrackKey0"/>.
    /// </remarks>
    public ulong TrackKey1 { get; protected set; }

    /// <summary>
    /// Do not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    protected new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Deselects any selected text.
    /// </summary>
    /// <remarks>
    /// Simply calls <see cref="CrossTracker.SelectNone"/>.
    /// </remarks>
    public bool SelectNone()
    {
        return Tracker.SelectNone();
    }

    /// <summary>
    /// Selects just this block.
    /// </summary>
    /// <remarks>
    /// Applicable only where <see cref="Tracker"/> is shared, otherwise behaviour of <see cref="SelectBlock"/> and <see
    /// cref="SelectAll"/> are the same.
    /// </remarks>
    public bool SelectBlock()
    {
        return Tracker.Select(TrackKey0, TrackKey1) != 0;
    }

    /// <summary>
    /// Simply calls <see cref="CrossTracker.SelectAll"/>.
    /// </summary>
    public bool SelectAll()
    {
        return Tracker.SelectAll();
    }

    /// <summary>
    /// Gets an array of internal blocks kinds primarily for test.
    /// </summary>
    /// <remarks>
    /// The result is subject to coalescing and blocks may be combined. Blocks contained within quote or list levels
    /// return a single block of <see cref="BlockKind.Para"/> irrespective of their contents. The result excludes <see
    /// cref="Header"/> and <see cref="Footer"/>.
    /// </remarks>
    public BlockKind[] GetBlockKinds()
    {
        var array = new BlockKind[_cache.Count];

        for (int n = 0; n < _cache.Count; ++n)
        {
            if (_cache[n] is MarkBlockHost block)
            {
                array[n] = block.Kind;
            }
        }

        return array;
    }

    /// <summary>
    /// Required to feed click event from internal control <see cref="LinkClickEvent"/>.
    /// </summary>
    internal virtual void LinkClickHandler(object? _, LinkClickEventArgs e)
    {
        e.Source = this;
        e.RoutedEvent = LinkClickEvent;
        RaiseEvent(e);
    }

    /// <summary>
    /// Forces a visual refresh where properties may have changed.
    /// </summary>
    /// <remarks>
    /// Only necessary where the "owner" constructor was used.
    /// </remarks>
    protected void RefreshLook()
    {
        int nm1 = _cache.Count - 1;

        for (int n = 0; n < _cache.Count; ++n)
        {
            _cache[n].RefreshLook(n == 0, n == nm1);
        }
    }

    /// <summary>
    /// Called only where <see cref="IsChromeStyled"/> is true and <see cref="ChromeStyling.Global"/> properties
    /// change.
    /// </summary>
    protected virtual void OnStylingChanged()
    {
        // This will asynchronously invoke Refresh() provided has Refresher coalescer.
        SelectionBrush = Styling.SemiAccent;
        LinkForeground = Styling.LinkForeground;
        LinkHoverBrush = Styling.LinkHover;

        HeadingForeground = Styling.AccentBrush;
        MonoFamily = ChromeFonts.MonospaceFamily;

        QuoteDecor = Styling.AccentBrush;
        RuleLine = ChromeStyling.ForegroundGray;

        FencedBorder = ChromeStyling.ForegroundGray;
        FencedCornerRadius = Styling.SmallCornerRadius;
        FencedBackground = Styling.BackgroundLow;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (_isChromeStyled)
        {
            Styling.StylingChanged += StylingChangedHandler;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_isChromeStyled)
        {
            Styling.StylingChanged -= StylingChangedHandler;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        const string NSpace = $"{nameof(MarkView)}.{nameof(OnPropertyChanged)}";

        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ContentProperty)
        {
            if (_source.Update(_content, Options))
            {
                ConditionalDebug.WriteLine(NSpace, "Update content");

                // TBD Test performance?
                // UpdateInternal(_source);

                // Supply coalesced clone
                UpdateInternal(_source.Coalesce());
            }

            return;
        }

        if (p == IsChromeStyledProperty)
        {
            var value = change.GetNewValue<bool>();

            if (value)
            {
                OnStylingChanged();

                if (this.IsAttachedToVisualTree())
                {
                    Styling.StylingChanged += StylingChangedHandler;
                }
            }
            else
            if (!value)
            {
                Styling.StylingChanged -= StylingChangedHandler;
            }

            return;
        }

        if (p == ContextMenuProperty)
        {
            var value = change.GetNewValue<ContextMenu?>();

            foreach (var item in _cache)
            {
                // Set null to clear as now set on parent
                item.Control.ContextMenu = null;
            }

            return;
        }

        if (IsMarkControlProperty(change.Property))
        {
            // Asynchronous
            Refresher?.Post();
            return;
        }

        if (p == ChildProperty && change.OldValue != null)
        {
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }
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
            e.Handled = Tracker.CopyText(WhatText.SelectedOrNull);
            return;
        }

        if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = Tracker.SelectAll();
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = Tracker.SelectNone();
            return;
        }
    }

    private void UpdateInternal(MarkDocument doc)
    {
        const string NSpace = $"{nameof(MarkView)}.{nameof(UpdateInternal)}";
        ConditionalDebug.WriteLine(NSpace, "UPDATE PROCESSSING");

        int index = 0;
        int cacheN = 0;
        int blockCount = doc.Count;
        var buffer = new List<Control>(blockCount);

        TrackKey0 = 0;
        TrackKey1 = 0;

        if (_header != null)
        {
            buffer.Add(_header);
        }

        while (index < blockCount)
        {
            if (cacheN < _cache.Count)
            {
                var cache = _cache[cacheN];

                if (cache.ConsumeUpdates(doc, ref index) != MarkConsumed.Incompatible)
                {
                    cacheN += 1;
                    buffer.Add(cache.Control);
                    SetTrackKey(cache);
                    continue;
                }

                _cache.RemoveRange(cacheN, _cache.Count - cacheN);
            }

            var host = MarkVisualHost.New(this, doc, ref index);

            _cache.Add(host);
            cacheN = _cache.Count;

            buffer.Add(host.Control);
            SetTrackKey(host);
        }

        _cache.RemoveRange(cacheN, _cache.Count - cacheN);
        _cache.TrimCapacity();

        if (_footer != null)
        {
            buffer.Add(_footer);
        }

        // WRITE NEW CONTENTS
        // Relies on extension method that avoids removing objects from visual tree.
        var children = _panel.Children;
        children.Replace(buffer);

        if (children.Capacity - children.Count > 32)
        {
            children.Capacity = children.Count;
        }

        ConditionalDebug.WriteLine(NSpace, "END OF UPDATE");
    }

    private void SetTrackKey(MarkVisualHost host)
    {
        if (host.TrackKey0 != 0U)
        {
            if (TrackKey0 == 0)
            {
                TrackKey0 = host.TrackKey0;
            }

            TrackKey1 = host.TrackKey1;
            ConditionalDebug.ThrowIfLessThan(TrackKey1, TrackKey0);
        }
    }

    private void RefresherHandler(object? _, EventArgs __)
    {
        RefreshLook();
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        OnStylingChanged();
    }
}