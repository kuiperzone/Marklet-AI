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
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Base class for <see cref="MarkView"/> and container classes.
/// </summary>
/// <remarks>
/// The class is concrete but not intended to be used directly.
/// </remarks>
public class MarkControl : Border
{
    /// <summary>
    /// Gets <see cref="ChromeStyling.Global"/> for convenience.
    /// </summary>
    protected static readonly ChromeStyling Styling = ChromeStyling.Global;

    /// <summary>
    /// Property coalescer which calls <see cref="PropertyRefresh"/> asynchronous in respose to property to change.
    /// </summary>
    protected readonly DispatchCoalescer PropertyCoalescer = new(DispatcherPriority.Render);

    private const double DefaultFontSize = 14.0;
    private const bool DefaultLinkHoverUnderline = true;
    private static readonly ImmutableSolidColorBrush DefaultSelectionBrush = CrossTextBlock.DefaultSelectionBrush;
    private static readonly FontFamily DefaultMonoFamily = ChromeFonts.MonospaceFamily;
    private static readonly ImmutableSolidColorBrush DefaultLinkForeground = CrossTextBlock.DefaultLinkBrush;
    private static readonly ImmutableSolidColorBrush DefaultLinkHover = CrossTextBlock.DefaultHoverLinkHoverBrush;
    private static readonly ImmutableSolidColorBrush DefaultQuoteDecor = new(0xFF5FAF5F);
    private static readonly ImmutableSolidColorBrush DefaultFencedBackground = new(0x40808080);

    private static readonly HashSet<AvaloniaProperty> ChangeSet = new();

    // Backing fields
    private bool _isChromeStyled;
    private IBrush? _foreground;
    private FontFamily _fontFamily = ChromeFonts.DefaultFamily;
    private double _fontSize = DefaultFontSize;
    private double _fontSizeCorrection = 1.0;
    private IBrush _selectionBrush = DefaultSelectionBrush;
    private IBrush? _linkForeground = DefaultLinkForeground;
    private IBrush? _linkHoverBrush = DefaultLinkHover;
    private bool _linkHoverUnderline = DefaultLinkHoverUnderline;
    private IBrush? _headingForeground;
    private FontFamily _headingFamily = ChromeFonts.DefaultFamily;
    private double _headingSizeCorrection = 1.0;
    private FontFamily _monoFamily = DefaultMonoFamily;
    private double _monoSizeCorrection = 1.0;
    private FontFamily? _quoteFamily;
    private double _quoteSizeCorrection = 1.0;
    private bool _quoteItalic;
    private IBrush _quoteDecor = DefaultQuoteDecor;
    private IBrush? _ruleLine = NeutralForegroundBrush;
    private IBrush? _fencedForeground;
    private IBrush? _fencedBackground = DefaultFencedBackground;
    private IBrush? _fencedBorder = NeutralForegroundBrush;
    private CornerRadius _fencedCornerRadius;
    private bool _defaultWrapping;

    static MarkControl()
    {
        ChangeSet.Add(ForegroundProperty);
        ChangeSet.Add(FontFamilyProperty);
        ChangeSet.Add(FontSizeProperty);
        ChangeSet.Add(FontSizeCorrectionProperty);
        ChangeSet.Add(SelectionBrushProperty);
        ChangeSet.Add(LinkForegroundProperty);
        ChangeSet.Add(LinkHoverBrushProperty);
        ChangeSet.Add(LinkHoverUnderlineProperty);
        ChangeSet.Add(HeadingForegroundProperty);
        ChangeSet.Add(HeadingFamilyProperty);
        ChangeSet.Add(HeadingSizeCorrectionProperty);
        ChangeSet.Add(MonoFamilyProperty);
        ChangeSet.Add(MonoSizeCorrectionProperty);
        ChangeSet.Add(QuoteFamilyProperty);
        ChangeSet.Add(QuoteSizeCorrectionProperty);
        ChangeSet.Add(QuoteItalicProperty);
        ChangeSet.Add(QuoteDecorProperty);
        ChangeSet.Add(RuleLineProperty);
        ChangeSet.Add(FencedForegroundProperty);
        ChangeSet.Add(FencedBackgroundProperty);
        ChangeSet.Add(FencedBorderProperty);
        ChangeSet.Add(FencedCornerRadiusProperty);
        ChangeSet.Add(DefaultWrappingProperty);

        IsTabStopProperty.OverrideDefaultValue<MarkControl>(false);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// Subclass must set Border.Child base property. It can be set once only during construction.
    /// </remarks>
    public MarkControl()
        : this(null)
    {
    }

    /// <summary>
    /// Constructor with "shared" tracker.
    /// </summary>
    /// <remarks>
    /// If "shared" is null, same as default construction. Subclass must set Border.Child base property. It can be set
    /// once only during construction.
    /// </remarks>
    public MarkControl(CrossTracker? shared)
    {
        Tracker = shared ?? new(this);
        Zoom = new();

        Focusable = true;
        IsTabStop = false;
        FocusAdorner = null;

        Zoom.Changed += PropertyChangedHandler;
        PropertyCoalescer.Posted += PropertyChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="IsChromeStyled"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkView, bool> IsChromeStyledProperty =
        AvaloniaProperty.RegisterDirect<MarkView, bool>(nameof(IsChromeStyled), o => o.IsChromeStyled, (o, v) => o.IsChromeStyled = v);

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> ForegroundProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(Foreground),
        o => o.Foreground, (o, v) => o.Foreground = v);

    /// <summary>
    /// Defines the <see cref="FontFamily"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, FontFamily> FontFamilyProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, FontFamily>(nameof(FontFamily),
        o => o.FontFamily, (o, v) => o.FontFamily = v, ChromeFonts.DefaultFamily);

    /// <summary>
    /// Defines the <see cref="FontSize"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, double> FontSizeProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, double>(nameof(FontSize),
        o => o.FontSize, (o, v) => o.FontSize = v, DefaultFontSize);

    /// <summary>
    /// Defines the <see cref="FontSizeCorrection"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, double> FontSizeCorrectionProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, double>(nameof(FontSizeCorrection),
        o => o.FontSizeCorrection, (o, v) => o.FontSizeCorrection = v, 1.0);

    /// <summary>
    /// Defines the <see cref="SelectionBrush"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush> SelectionBrushProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush>(nameof(SelectionBrush),
        o => o.SelectionBrush, (o, v) => o.SelectionBrush = v, DefaultSelectionBrush);

    /// <summary>
    /// Defines the <see cref="LinkForeground"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> LinkForegroundProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(LinkForeground),
        o => o.LinkForeground, (o, v) => o.LinkForeground = v, DefaultLinkForeground);

    /// <summary>
    /// Defines the <see cref="LinkHoverBrush"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> LinkHoverBrushProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(LinkHoverBrush),
        o => o.LinkHoverBrush, (o, v) => o.LinkHoverBrush = v, DefaultLinkHover);

    /// <summary>
    /// Defines the <see cref="LinkHoverUnderline"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, bool> LinkHoverUnderlineProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, bool>(nameof(LinkHoverUnderline),
        o => o.LinkHoverUnderline, (o, v) => o.LinkHoverUnderline = v, DefaultLinkHoverUnderline);

    /// <summary>
    /// Defines the <see cref="HeadingForeground"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> HeadingForegroundProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(HeadingForeground),
        o => o.HeadingForeground, (o, v) => o.HeadingForeground = v);

    /// <summary>
    /// Defines the <see cref="HeadingFamily"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, FontFamily> HeadingFamilyProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, FontFamily>(nameof(HeadingFamily),
        o => o.HeadingFamily, (o, v) => o.HeadingFamily = v);

    /// <summary>
    /// Defines the <see cref="HeadingSizeCorrection"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, double> HeadingSizeCorrectionProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, double>(nameof(HeadingSizeCorrection),
        o => o.HeadingSizeCorrection, (o, v) => o.HeadingSizeCorrection = v, 1.0);

    /// <summary>
    /// Defines the <see cref="MonoFamily"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, FontFamily> MonoFamilyProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, FontFamily>(nameof(MonoFamily),
        o => o.MonoFamily, (o, v) => o.MonoFamily = v, DefaultMonoFamily);

    /// <summary>
    /// Defines the <see cref="MonoSizeCorrection"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, double> MonoSizeCorrectionProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, double>(nameof(MonoSizeCorrection),
        o => o.MonoSizeCorrection, (o, v) => o.MonoSizeCorrection = v, 1.0);

    /// <summary>
    /// Defines the <see cref="QuoteFamily"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, FontFamily?> QuoteFamilyProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, FontFamily?>(nameof(QuoteFamily),
        o => o.QuoteFamily, (o, v) => o.QuoteFamily = v);

    /// <summary>
    /// Defines the <see cref="QuoteSizeCorrection"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, double> QuoteSizeCorrectionProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, double>(nameof(QuoteSizeCorrection),
        o => o.QuoteSizeCorrection, (o, v) => o.QuoteSizeCorrection = v, 1.0);

    /// <summary>
    /// Defines the <see cref="QuoteItalic"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, bool> QuoteItalicProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, bool>(nameof(QuoteItalic),
        o => o.QuoteItalic, (o, v) => o.QuoteItalic = v);

    /// <summary>
    /// Defines the <see cref="QuoteDecor"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush> QuoteDecorProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush>(nameof(QuoteDecor),
        o => o.QuoteDecor, (o, v) => o.QuoteDecor = v);

    /// <summary>
    /// Defines the <see cref="RuleLine"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> RuleLineProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(RuleLine),
        o => o.RuleLine, (o, v) => o.RuleLine = v, NeutralForegroundBrush);

    /// <summary>
    /// Defines the <see cref="FencedForeground"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> FencedForegroundProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(FencedForeground),
        o => o.FencedForeground, (o, v) => o.FencedForeground = v, NeutralForegroundBrush);

    /// <summary>
    /// Defines the <see cref="FencedBackground"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> FencedBackgroundProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(FencedBackground),
        o => o.FencedBackground, (o, v) => o.FencedBackground = v, DefaultFencedBackground);

    /// <summary>
    /// Defines the <see cref="FencedBorder"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, IBrush?> FencedBorderProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(FencedBorder),
        o => o.FencedBorder, (o, v) => o.FencedBorder = v);

    /// <summary>
    /// Defines the <see cref="FencedCornerRadius"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, CornerRadius> FencedCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, CornerRadius>(nameof(FencedCornerRadius),
        o => o.FencedCornerRadius, (o, v) => o.FencedCornerRadius = v);

    /// <summary>
    /// Defines the <see cref="DefaultWrapping"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkControl, bool> DefaultWrappingProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, bool>(nameof(DefaultWrapping),
        o => o.DefaultWrapping, (o, v) => o.DefaultWrapping = v);

    /// <summary>
    /// Fixed brush. Solid gray.
    /// </summary>
    protected internal static readonly ImmutableSolidColorBrush NeutralForegroundBrush = new(0xFF808080);

    /// <summary>
    /// Fixed brush. Orange.
    /// </summary>
    protected internal static readonly ImmutableSolidColorBrush DefaultCodeForeground = new(0xFFC88800);

    /// <summary>
    /// Fixed brush. Semi-gray.
    /// </summary>
    protected internal static readonly ImmutableSolidColorBrush DefaultCodeBackground = new(0x10808080);

    /// <summary>
    /// Fixed brush. Semi-opaque yellow.
    /// </summary>
    protected internal static readonly ImmutableSolidColorBrush MarkBackgroundBrush = new(0x80C4A000);

    /// <summary>
    /// Fixed brush. Semi-opaque yellow.
    /// </summary>
    protected internal static readonly ImmutableSolidColorBrush KeywordBackgroundBrush = new(0xA0ED5B00);

    /// <summary>
    /// Gets or sets whether visual properties follow <see cref="ChromeStyling"/> values and respond to changes.
    /// </summary>
    /// <remarks>
    /// The base class default is false.
    /// </remarks>
    public bool IsChromeStyled
    {
        get { return _isChromeStyled; }
        set { SetAndRaise(IsChromeStyledProperty, ref _isChromeStyled, value); }
    }

    /// <summary>
    /// Gets or sets the default foreground brush used for text.
    /// </summary>
    /// <remarks>
    /// The default is null.
    /// </remarks>
    public IBrush? Foreground
    {
        get { return _foreground; }
        set { SetAndRaise(ForegroundProperty, ref _foreground, value); }
    }

    /// <summary>
    /// Gets or sets the font family used to draw the control's text.
    /// </summary>
    public FontFamily FontFamily
    {
        get { return _fontFamily; }
        set { SetAndRaise(FontFamilyProperty, ref _fontFamily, value); }
    }

    /// <summary>
    /// Gets or sets the size of the control's text in points.
    /// </summary>
    public double FontSize
    {
        get { return _fontSize; }
        set { SetAndRaise(FontSizeProperty, ref _fontSize, value); }
    }

    /// <summary>
    /// Gets or sets the paragraph font size correction value.
    /// </summary>
    /// <remarks>
    /// A tweakment used to adjust (increase) the font size a little to ensure inconsistent <see cref="FontFamily"/>
    /// names visually display text of consistent size. Typically it is used only when displaying "non-standard" or
    /// cursive fonts. Typical values are in the range [1.0, 1.2]. The default value is 1.0.
    /// </remarks>
    public double FontSizeCorrection
    {
        get { return _fontSizeCorrection; }
        set { SetAndRaise(FontSizeCorrectionProperty, ref _fontSizeCorrection, value); }
    }


    /// <summary>
    /// Gets or sets the selection background brush.
    /// </summary>
    /// <remarks>
    /// Note that there is no "SelectionForeground" property. Instead, the <see cref="SelectionBrush"/> should always be
    /// set to a semi-opaque (approx 50%) brush.
    /// </remarks>
    public IBrush SelectionBrush
    {
        get { return _selectionBrush; }
        set { SetAndRaise(SelectionBrushProperty, ref _selectionBrush, value); }
    }

    /// <summary>
    /// Gets or sets the brush for link text.
    /// </summary>
    /// <remarks>
    /// A value of null inherits <see cref="Foreground"/>.
    /// </remarks>
    public IBrush? LinkForeground
    {
        get { return _linkForeground; }
        set { SetAndRaise(LinkForegroundProperty, ref _linkForeground, value); }
    }

    /// <summary>
    /// Gets or sets the brush for link hover text.
    /// </summary>
    /// <remarks>
    /// A value of null shows no link hover color change.
    /// </remarks>
    public IBrush? LinkHoverBrush
    {
        get { return _linkHoverBrush; }
        set { SetAndRaise(LinkHoverBrushProperty, ref _linkHoverBrush, value); }
    }

    /// <summary>
    /// Gets or sets whether links are underlined when hovered.
    /// </summary>
    public bool LinkHoverUnderline
    {
        get { return _linkHoverUnderline; }
        set { SetAndRaise(LinkHoverUnderlineProperty, ref _linkHoverUnderline, value); }
    }

    /// <summary>
    /// Gets or sets the brush used for heading text.
    /// </summary>
    /// <remarks>
    /// The default is null, which inherits <see cref="Foreground"/>.
    /// </remarks>
    public IBrush? HeadingForeground
    {
        get { return _headingForeground; }
        set { SetAndRaise(HeadingForegroundProperty, ref _headingForeground, value); }
    }

    /// <summary>
    /// Gets or sets the font family used for headings.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="ChromeFonts.DefaultFamily"/>.
    /// </remarks>
    public FontFamily HeadingFamily
    {
        get { return _headingFamily; }
        set { SetAndRaise(HeadingFamilyProperty, ref _headingFamily, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="HeadingFamily"/> font size correction value.
    /// </summary>
    /// <remarks>
    /// A tweakment used to adjust (increase) the font size a little to ensure inconsistent <see cref="FontFamily"/>
    /// names visually display text of consistent size. Typically it is used only when displaying "non-standard" or
    /// cursive fonts. Typical values are in the range [1.0, 1.2]. The default value is 1.0.
    /// </remarks>
    public double HeadingSizeCorrection
    {
        get { return _headingSizeCorrection; }
        set { SetAndRaise(HeadingSizeCorrectionProperty, ref _headingSizeCorrection, value); }
    }

    /// <summary>
    /// Gets or sets the font family used for monospaced code.
    /// </summary>
    public FontFamily MonoFamily
    {
        get { return _monoFamily; }
        set { SetAndRaise(MonoFamilyProperty, ref _monoFamily, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="MonoFamily"/> font size correction value.
    /// </summary>
    /// <remarks>
    /// A tweakment used to adjust (increase) the font size a little to ensure inconsistent <see cref="FontFamily"/>
    /// names visually display text of consistent size. Typically it is used only when displaying "non-standard" or
    /// cursive fonts. Typical values are in the range [1.0, 1.2]. The default value is 1.0.
    /// </remarks>
    public double MonoSizeCorrection
    {
        get { return _monoSizeCorrection; }
        set { SetAndRaise(MonoSizeCorrectionProperty, ref _monoSizeCorrection, value); }
    }

    /// <summary>
    /// Gets or sets the font family used in quotation blocks.
    /// </summary>
    /// <remarks>
    /// If null, the font used follows the parent <see cref="TextBlock.FontFamily"/>. The default is null.
    /// </remarks>
    public FontFamily? QuoteFamily
    {
        get { return _quoteFamily; }
        set { SetAndRaise(QuoteFamilyProperty, ref _quoteFamily, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="QuoteFamily"/> font size correction value.
    /// </summary>
    /// <remarks>
    /// A tweakment used to adjust (increase) the font size a little to ensure inconsistent <see cref="FontFamily"/>
    /// names visually display text of consistent size. Typically it is used only when displaying "non-standard" or
    /// cursive fonts. Typical values are in the range [1.0, 1.2]. It is ignored if <see cref="QuoteFamily"/> is null.
    /// The default value is 1.0.
    /// </remarks>
    public double QuoteSizeCorrection
    {
        get { return _quoteSizeCorrection; }
        set { SetAndRaise(QuoteSizeCorrectionProperty, ref _quoteSizeCorrection, value); }
    }

    /// <summary>
    /// Gets or sets whether to show the contents of quotation in italic.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool QuoteItalic
    {
        get { return _quoteItalic; }
        set { SetAndRaise(QuoteItalicProperty, ref _quoteItalic, value); }
    }

    /// <summary>
    /// Gets or sets the brush for quote block decoration.
    /// </summary>
    public IBrush QuoteDecor
    {
        get { return _quoteDecor; }
        set { SetAndRaise(QuoteDecorProperty, ref _quoteDecor, value); }
    }

    /// <summary>
    /// Gets or sets the brush used for horizontal rule lines.
    /// </summary>
    /// <remarks>
    /// A value of null hides rule lines.
    /// </remarks>
    public IBrush? RuleLine
    {
        get { return _ruleLine; }
        set { SetAndRaise(RuleLineProperty, ref _ruleLine, value); }
    }

    /// <summary>
    /// Gets or sets the brush for fenced code foreground text.
    /// </summary>
    /// <remarks>
    /// A value of null inherits <see cref="Foreground"/>. The default is null.
    /// </remarks>
    public IBrush? FencedForeground
    {
        get { return _fencedForeground; }
        set { SetAndRaise(FencedForegroundProperty, ref _fencedForeground, value); }
    }

    /// <summary>
    /// Gets or sets the brush for fenced code background.
    /// </summary>
    /// <remarks>
    /// A value of null inherits the control's background. The default is a semi-opaque gray.
    /// </remarks>
    public IBrush? FencedBackground
    {
        get { return _fencedBackground; }
        set { SetAndRaise(FencedBackgroundProperty, ref _fencedBackground, value); }
    }

    /// <summary>
    /// Gets or sets the brush used for the border of the fenced code box.
    /// </summary>
    /// <remarks>
    /// A value of null hides the border.
    /// </remarks>
    public IBrush? FencedBorder
    {
        get { return _fencedBorder; }
        set { SetAndRaise(FencedBorderProperty, ref _fencedBorder, value); }
    }

    /// <summary>
    /// Gets or sets border radius in pixels of the fenced code box.
    /// </summary>
    public CornerRadius FencedCornerRadius
    {
        get { return _fencedCornerRadius; }
        set { SetAndRaise(FencedCornerRadiusProperty, ref _fencedCornerRadius, value); }
    }

    /// <summary>
    /// Gets or sets the default line wrap state of fenced and indented code.
    /// </summary>
    /// <remarks>
    /// Does not apply to tables and other special monospace regions.
    /// </remarks>
    public bool DefaultWrapping
    {
        get { return _defaultWrapping; }
        set { SetAndRaise(DefaultWrappingProperty, ref _defaultWrapping, value); }
    }

    /// <summary>
    /// Gets the zooming instance.
    /// </summary>
    public Zoomer Zoom { get; } = new();

    /// <inheritdoc cref="ICrossTrackOwner.Tracker"/>
    public CrossTracker Tracker { get; }

    /// <summary>
    /// Handles key input and returns true if handled.
    /// </summary>
    /// <remarks>
    /// The class does not receive key inputs itself. It returns false and does nothing if the supplied
    /// KeyEventArgs.Handled is true.
    /// </remarks>
    public virtual bool HandleKeyGesture(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(MarkControl)}.{nameof(HandleKeyGesture)}";
        ConditionalDebug.WriteLine(NSpace, $"KEY: {e.Key}, {e.KeyModifiers}");

        if (e.Handled)
        {
            ConditionalDebug.WriteLine(NSpace, "Already handled");
            return false;
        }

        if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control && Tracker.SelectionCount != 0)
        {
            e.Handled = true;
            ConditionalDebug.WriteLine(NSpace, $"CTRL-C accepted");

            Tracker.CopyText(WhatText.SelectedOrNull);
            return true;
        }

        // Only one instance to act on this key press
        if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            ConditionalDebug.WriteLine(NSpace, $"CTRL-A accepted");

            Tracker.SelectAll();
            return true;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None && Tracker.SelectionCount != 0)
        {
            e.Handled = true;
            ConditionalDebug.WriteLine(NSpace, $"ESCAPE accepted");

            Tracker.SelectNone();
            return true;
        }

        return Zoom.HandleKeyGesture(e);
    }

    /// <summary>
    /// Returns true if "property" belongs to this class, excluding base class properties.
    /// </summary>
    protected static bool IsMarkControlProperty(AvaloniaProperty property)
    {
        return ChangeSet.Contains(property);
    }

    /// <summary>
    /// Called when one or more of the properties of this base class have changed.
    /// </summary>
    /// <remarks>
    /// IMPORTANT. Base implementation does nothing and must be overridded.
    /// </remarks>
    protected virtual void PropertyRefresh()
    {
    }

    /// <summary>
    /// Called when <see cref="ChromeStyling.Global"/> properties change provided <see cref="IsChromeStyled"/> is true.
    /// </summary>
    protected virtual void OnStylingChanged()
    {
        // This will asynchronously invoke Refresh() using coalescer.
        if (IsChromeStyled)
        {
            SelectionBrush = Styling.Accent50;
            LinkForeground = Styling.LinkForeground;
            LinkHoverBrush = Styling.LinkHover;

            QuoteDecor = Styling.AccentBrush;
            RuleLine = ChromeStyling.GrayForeground;

            FencedBorder = ChromeStyling.GrayForeground;
            FencedCornerRadius = Styling.SmallCornerRadius;
            FencedBackground = Styling.BackgroundLow;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChildProperty)
        {
            if (change.OldValue != null)
            {
                // Set once on construction only
                throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
            }

            return;
        }

        if (IsMarkControlProperty(p))
        {
            PropertyCoalescer.Post();
            return;
        }

        if (p == IsChromeStyledProperty)
        {
            if (change.GetNewValue<bool>())
            {
                OnStylingChanged();
            }

            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Styling.StylingChanged += StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        OnStylingChanged();
    }

    private void PropertyChangedHandler(object? _, EventArgs __)
    {
        PropertyRefresh();
    }
}