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
using Avalonia.Media.Immutable;
using KuiperZone.Marklet.PixieChrome.Shared;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Base class for <see cref="MarkView"/> and container classes.
/// </summary>
/// <remarks>
/// The class is not aware <see cref="ChromeStyling"/> and properties are "direct".
/// </remarks>
public class MarkControl : Border
{
    // Internal (not private) for test
    internal const double DefaultFontSize = 14.0;
    internal const double LineHeightMultiple = 1.8;
    internal const double LetterSpacing = 1.3;
    internal const bool DefaultLinkHoverUnderline = true;
    internal static readonly ImmutableSolidColorBrush DefaultSelectionBrush = CrossTextBlock.DefaultSelectionBrush;
    internal static readonly FontFamily DefaultMonoFamily = "monospace";
    internal static readonly ImmutableSolidColorBrush DefaultLinkForeground = CrossTextBlock.DefaultLinkBrush;
    internal static readonly ImmutableSolidColorBrush DefaultLinkHover = CrossTextBlock.DefaultHoverLinkHoverBrush;
    internal static readonly ImmutableSolidColorBrush DefaultQuoteDecor = new(0xFF5FAF5F);
    internal static readonly ImmutableSolidColorBrush DefaultFencedBackground = new(0x40808080);

    private static readonly HashSet<AvaloniaProperty> ChangeSet = new();

    // Backing fields
    private IBrush? _foreground;
    private FontFamily _fontFamily = FontFamily.Default;
    private double _fontSize = DefaultFontSize;
    private double _fontSizeCorrection = 1.0;
    private IBrush? _selectionBrush = DefaultSelectionBrush;
    private IBrush? _linkForeground = DefaultLinkForeground;
    private IBrush? _linkHoverBrush = DefaultLinkHover;
    private bool _linkHoverUnderline = DefaultLinkHoverUnderline;
    private IBrush? _headingForeground;
    private FontFamily _headingFamily = FontFamily.Default;
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

        IsTabStopProperty.OverrideDefaultValue(typeof(MarkControl), false);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkControl()
    {
        Zoom = new();
    }

    /// <summary>
    /// Protected constructor with shared zoom.
    /// </summary>
    protected MarkControl(MarkControl owner)
    {
        Zoom = owner.Zoom;
        CopyProperties(owner);
    }

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
        o => o.FontFamily, (o, v) => o.FontFamily = v, FontFamily.Default);

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
    public static readonly DirectProperty<MarkControl, IBrush?> SelectionBrushProperty =
        AvaloniaProperty.RegisterDirect<MarkControl, IBrush?>(nameof(SelectionBrush),
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
    internal static readonly ImmutableSolidColorBrush NeutralForegroundBrush = new(0xFF808080);

    /// <summary>
    /// Fixed brush. Semi-gray.
    /// </summary>
    internal static readonly ImmutableSolidColorBrush DefaultCodeBackground = new(0x80808080);

    /// <summary>
    /// Fixed brush. Semi-opaque yellow.
    /// </summary>
    internal static readonly ImmutableSolidColorBrush MarkBackgroundBrush = new(0x80C4A000);

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
    /// set to a semi-opaque (approx 50%) brush. Setting to null disables selection of text.
    /// </remarks>
    public IBrush? SelectionBrush
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
    /// The default is <see cref="FontFamily.Default"/>.
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
    /// <remarks>
    /// The default is "monospace".
    /// </remarks>
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
    public Zoomer Zoom { get; }

    /// <summary>
    /// Gets the <see cref="FontSize"/> multiplied by the <see cref="Zoomer.Fraction"/>.
    /// </summary>
    protected internal double ScaledFontSize
    {
        get { return FontSize * Zoom.Fraction; }
    }

    /// <summary>
    /// Gets the <see cref="LineHeightMultiple"/> multiplied by the <see cref="Zoomer.Fraction"/>.
    /// </summary>
    protected internal double ScaledLineHeight
    {
        get { return LineHeightMultiple * FontSize * Zoom.Fraction; }
    }

    /// <summary>
    /// Gets the scaled letter spacing the <see cref="Zoomer.Fraction"/>.
    /// </summary>
    protected internal double ScaledLetterSpacing
    {
        get { return LetterSpacing * Zoom.Fraction; }
    }

    /// <summary>
    /// Gets the width a non-rule line.
    /// </summary>
    protected internal double LinePixels
    {
        get { return Zoom.Fraction; }
    }

    /// <summary>
    /// Gets the width of a "rule" line.
    /// </summary>
    protected internal double RulePixels
    {
        get { return 1.5 * Zoom.Fraction; }
    }

    /// <summary>
    /// Gets a number of pixels very approx equal to one character width.
    /// </summary>
    protected internal double OneCh
    {
        get { return 0.5 * ScaledFontSize; }
    }

    /// <summary>
    /// Gets the number of indentation pixels derived from <see cref="FontSize"/>.
    /// </summary>
    protected internal double TabPx
    {
        get { return 4.0 * OneCh; }
    }

    /// <summary>
    /// Returns true if "property" belongs to this class, excluding base class properties.
    /// </summary>
    protected static bool IsMarkControlProperty(AvaloniaProperty property)
    {
        return ChangeSet.Contains(property);
    }

    /// <summary>
    /// Copies properties of "other" to this.
    /// </summary>
    /// <remarks>
    /// The method does NOT raise property changes. The base class properties of "other", i.e. <see
    /// cref="Border.Background"/> etc., are not copied. The <see cref="Zoom"/> value is not copied.
    /// </remarks>
    protected void CopyProperties(MarkControl other)
    {
        _foreground = other._foreground;
        _fontFamily = other._fontFamily;
        _fontSize = other._fontSize;
        _fontSizeCorrection = other._fontSizeCorrection;
        _selectionBrush = other._selectionBrush;
        _linkForeground = other._linkForeground;
        _linkHoverBrush = other._linkHoverBrush;
        _linkHoverUnderline = other._linkHoverUnderline;
        _headingForeground = other._headingForeground;
        _headingFamily = other._headingFamily;
        _headingSizeCorrection = other._headingSizeCorrection;
        _monoFamily = other._monoFamily;
        _monoSizeCorrection = other._monoSizeCorrection;
        _quoteFamily = other._quoteFamily;
        _quoteSizeCorrection = other._quoteSizeCorrection;
        _quoteItalic = other._quoteItalic;
        _quoteDecor = other._quoteDecor;
        _ruleLine = other._ruleLine;
        _fencedForeground = other._fencedForeground;
        _fencedBackground = other._fencedBackground;
        _fencedBorder = other._fencedBorder;
        _fencedCornerRadius = other._fencedCornerRadius;
        _defaultWrapping = other._defaultWrapping;
    }

}