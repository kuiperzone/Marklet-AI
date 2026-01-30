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

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Visual group manager for <see cref="PixieControl"/> children.
/// </summary>
/// <remarks>
/// This class will maintain the background, corners and <see cref="PixieControl.Group"/> properties of children derived
/// from <see cref="PixieControl"/>. Other class types may be added, but these should ideally be present only at the
/// start or end of the <see cref="Children"/>. The groups's own background should not usually be set. Instead, the <see
/// cref="ChildBackground"/> and <see cref="ChildCorner"/> may be bound to styling. It also maintains exclusivity for
/// <see cref="PixieRadio"/> and <see cref="PixieButton"/>. The default HorizontalAlignment is "Stretch".
/// </remarks>
public class PixieGroup : StackPanel
{
    /// <summary>
    /// Shortcut for <see cref="ChromeStyling.Global"/>.
    /// </summary>
    protected static readonly ChromeStyling Styling = ChromeStyling.Global;

    private const bool DefaultIsOpen = true;
    private readonly TextBlock _titleBlock = new();
    private readonly TextBlock _taglineBlock = new();
    private readonly DispatchCoalescer _rebuild = new(DispatcherPriority.Render);
    private readonly DispatchCoalescer _update = new(DispatcherPriority.Render);
    private PixieButton? _collapseButton;

    // Backing fields
    private string? _topTitle;
    private string? _topFooter;
    private bool _isOpen = DefaultIsOpen;
    private bool _isCollapsable;
    private string? _collapseHeader;
    private string? _collapseFooter;
    private string? _collapseSymbol;
    private string? _collapseTitle;

    static PixieGroup()
    {
        UseLayoutRoundingProperty.OverrideDefaultValue<PixieGroup>(false);
        OrientationProperty.OverrideDefaultValue<PixieGroup>(Avalonia.Layout.Orientation.Vertical);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieGroup()
    {
        ConditionalDebug.ThrowIfTrue(base.UseLayoutRounding);
        ConditionalDebug.ThrowIfNotEqual(Avalonia.Layout.Orientation.Vertical, base.Orientation);
        ConditionalDebug.ThrowIfNotEqual(Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalAlignment);

        _titleBlock.IsVisible = false;
        _titleBlock.FontWeight = FontWeight.Bold;
        _titleBlock.TextWrapping = TextWrapping.Wrap;

        _taglineBlock.FontSize = ChromeFonts.SmallFontSize;
        _taglineBlock.TextWrapping = TextWrapping.Wrap;
        _taglineBlock.Foreground = ChromeStyling.ForegroundGray;
        _taglineBlock.IsVisible = false;

        _rebuild.Posted += RebuildPostHandler;
        _update.Posted += UpdatePostHandler;
        Children.CollectionChanged += PixieChildrenChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="TopTitle"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> TopTitleProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(TopTitle),
        o => o.TopTitle, (o, v) => o.TopTitle = v);

    /// <summary>
    /// Defines the <see cref="TopFooter"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> TopFooterProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(TopFooter),
        o => o.TopFooter, (o, v) => o.TopFooter = v);

    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, bool>(nameof(IsOpen),
        o => o.IsOpen, (o, v) => o.IsOpen = v, DefaultIsOpen);

    /// <summary>
    /// Defines the <see cref="IsCollapsable"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, bool> IsCollapsableProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, bool>(nameof(IsCollapsable),
        o => o.IsCollapsable, (o, v) => o.IsCollapsable = v);

    /// <summary>
    /// Defines the <see cref="CollapseHeader"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> CollapseHeaderProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(CollapseHeader),
        o => o.CollapseHeader, (o, v) => o.CollapseHeader = v);

    /// <summary>
    /// Defines the <see cref="CollapseFooter"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> CollapseFooterProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(CollapseFooter),
        o => o.CollapseFooter, (o, v) => o.CollapseFooter = v);

    /// <summary>
    /// Defines the <see cref="CollapseSymbol"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> CollapseSymbolProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(CollapseSymbol),
        o => o.CollapseSymbol, (o, v) => o.CollapseSymbol = v);

    /// <summary>
    /// Defines the <see cref="CollapseTitle"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> CollapseTitleProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(CollapseTitle),
        o => o.CollapseTitle, (o, v) => o.CollapseTitle = v);

    /// <summary>
    /// Defines the <see cref="IsCornerGrouped"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCornerGroupedProperty =
        AvaloniaProperty.Register<PixieGroup, bool>(nameof(IsCornerGrouped));

    /// <summary>
    /// Defines the <see cref="CollapseTitleWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> CollapseTitleWeightProperty =
        AvaloniaProperty.Register<PixieGroup, FontWeight>(nameof(CollapseTitleWeight), FontWeight.Normal);

    /// <summary>
    /// Defines the <see cref="ChildBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ChildBackgroundProperty =
        AvaloniaProperty.Register<PixieGroup, IBrush>(nameof(ChildBackground), Brushes.Transparent);

    /// <summary>
    /// Defines the <see cref="ChildCorner"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> ChildCornerProperty =
        AvaloniaProperty.Register<PixieGroup, CornerRadius>(nameof(ChildCorner));

    /// <summary>
    /// Defines the <see cref="ChildBorder"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ChildBorderProperty =
        AvaloniaProperty.Register<PixieGroup, IBrush?>(nameof(ChildBorder));

    /// <summary>
    /// Defines the <see cref="ChildIndent"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ChildIndentProperty =
        AvaloniaProperty.Register<PixieGroup, double>(nameof(ChildIndent));


    /// <summary>
    /// Gets or sets the top title text. This is a direct property.
    /// </summary>
    public string? TopTitle
    {
        get { return _topTitle; }
        set { SetAndRaise(TopTitleProperty, ref _topTitle, value); }
    }

    /// <summary>
    /// Gets or sets the tagline text shown under <see cref="TopTitle"/>. There is no "top header". This is a direct
    /// property.
    /// </summary>
    public string? TopFooter
    {
        get { return _topFooter; }
        set { SetAndRaise(TopFooterProperty, ref _topFooter, value); }
    }

    /// <summary>
    /// Gets or sets whether the group is open (true), or collapsed (false). This is a direct property.
    /// </summary>
    /// <remarks>
    /// The group will effectively be hidden if both <see cref="IsOpen"/> is false and <see cref="IsCollapsable"/> are
    /// false. The default is true.
    /// </remarks>
    public bool IsOpen
    {
        get { return _isOpen; }
        set { SetAndRaise(IsOpenProperty, ref _isOpen, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a header control allowing the user to toggle <see cref="IsOpen"/> and collapse or
    /// show the group's contents. This is a direct property.
    /// </summary>
    /// <remarks>
    /// The group will effectively be hidden if both <see cref="IsOpen"/> is false and <see cref="IsCollapsable"/> are
    /// false. The default is false.
    /// </remarks>
    public bool IsCollapsable
    {
        get { return _isCollapsable; }
        set { SetAndRaise(IsCollapsableProperty, ref _isCollapsable, value); }
    }

    /// <summary>
    /// Gets or sets the text shown in the collapse control above <see cref="CollapseTitle"/>. This is a direct property.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsable"/> property must be true otherwise this text value is not shown. The default is null.
    /// </remarks>
    public string? CollapseHeader
    {
        get { return _collapseHeader; }
        set { SetAndRaise(CollapseHeaderProperty, ref _collapseHeader, value); }
    }

    /// <summary>
    /// Gets or sets the text shown in the collapse header below <see cref="CollapseTitle"/>. This is a direct property.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsable"/> property must be true otherwise this text value is not shown. The default is null.
    /// </remarks>
    public string? CollapseFooter
    {
        get { return _collapseFooter; }
        set { SetAndRaise(CollapseFooterProperty, ref _collapseFooter, value); }
    }

    /// <summary>
    /// Gets or sets the symbol shown in the collapse control. This is a direct property.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsable"/> property must be true otherwise this text value is not shown. The default is null.
    /// </remarks>
    public string? CollapseSymbol
    {
        get { return _collapseSymbol; }
        set { SetAndRaise(CollapseSymbolProperty, ref _collapseSymbol, value); }
    }

    /// <summary>
    /// Gets or sets the title shown in the collapse control. This is a direct property.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsable"/> property must be true otherwise the title is not shown. The default is null.
    /// </remarks>
    public string? CollapseTitle
    {
        get { return _collapseTitle; }
        set { SetAndRaise(CollapseTitleProperty, ref _collapseTitle, value); }
    }

    /// <summary>
    /// Gets or sets whether the <see cref="ChildCorner"/> is applied to the group as a whole, i.e. top and bottom
    /// children only with children in between having no corners.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool IsCornerGrouped
    {
        get { return GetValue(IsCornerGroupedProperty); }
        set { SetValue(IsCornerGroupedProperty, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="CollapseTitle"/> font weight.
    /// </summary>
    public FontWeight CollapseTitleWeight
    {
        get { return GetValue(CollapseTitleWeightProperty); }
        set { SetValue(CollapseTitleWeightProperty, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="PixieControl"/> child background.
    /// </summary>
    public IBrush ChildBackground
    {
        get { return GetValue(ChildBackgroundProperty); }
        set { SetValue(ChildBackgroundProperty, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="PixieControl"/> corner radius of children.
    /// </summary>
    /// <remarks>
    /// The value is applied to top and bottom children only where <see cref="IsCornerGrouped"/> is true.
    /// </remarks>
    public CornerRadius ChildCorner
    {
        get { return GetValue(ChildCornerProperty); }
        set { SetValue(ChildCornerProperty, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="PixieControl"/> child background.
    /// </summary>
    public IBrush? ChildBorder
    {
        get { return GetValue(ChildBorderProperty); }
        set { SetValue(ChildBorderProperty, value); }
    }

    /// <summary>
    /// Gets or sets child indentation in pixels.
    /// </summary>
    /// <remarks>
    /// The default is 0.
    /// </remarks>
    public double ChildIndent
    {
        get { return GetValue(ChildIndentProperty); }
        set { SetValue(ChildIndentProperty, value); }
    }

    /// <summary>
    /// Replaces children.
    /// </summary>
    [Content]
    public new Avalonia.Controls.Controls Children { get; } = new();

    /// <summary>
    /// Do not use. Always Vertical.
    /// </summary>
    [Obsolete($"Do not use. Always Vertical.", true)]
    protected new Avalonia.Layout.Orientation Orientation
    {
        get { return base.Orientation; }
        set { base.Orientation = value; }
    }

    /// <summary>
    /// Do not use. Always false.
    /// </summary>
    /// <remarks>
    /// Panel.Spacing gives inconsistent results: https://github.com/AvaloniaUI/Avalonia/issues/19680
    /// </remarks>
    [Obsolete($"Do not use. Always false.", true)]
    protected new bool UseLayoutRounding
    {
        get { return base.UseLayoutRounding; }
        set { base.UseLayoutRounding = value; }
    }

    /// <summary>
    /// Searches <see cref="Children"/> for <see cref="PixieControl"/> instances containing the given "keyword" and
    /// returns true if one or more are found.
    /// </summary>
    /// <remarks>
    /// Results are appended to "results" and may include self. The return value is always false for null or empty
    /// strings.
    /// </remarks>
    public bool Find(string? keyword, List<PixieFinding> findings)
    {
        const StringComparison Comp = StringComparison.OrdinalIgnoreCase;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        bool flag = false;

        foreach (var item in Children)
        {
            if (item is PixieControl control)
            {
                flag |= control.Find(keyword, findings);
                continue;
            }

            if (item is PixieGroup group)
            {
                // Group within a group
                flag |= group.Find(keyword, findings);
                continue;
            }
        }

        if (!flag)
        {
            if (TopTitle?.Contains(keyword, Comp) == true ||
                TopFooter?.Contains(keyword, Comp) == true)
            {
                // Self
                flag = true;
                findings.Add(new PixieFinding(this));
            }

            if (IsCollapsable &&
                (CollapseHeader?.Contains(keyword, Comp) == true ||
                CollapseTitle?.Contains(keyword, Comp) == true ||
                CollapseFooter?.Contains(keyword, Comp) == true))
            {
                // Self
                flag = true;
                findings.Add(new PixieFinding(this));
            }
        }

        return flag;
    }

    /// <summary>
    /// Called by <see cref="PixieRadio"/>.
    /// </summary>
    internal void CheckedChanged(PixieRadio obj)
    {
        if (obj.IsChecked)
        {
            foreach (var item in Children)
            {
                if (item != obj && item is PixieRadio other)
                {
                    other.IsChecked = false;
                }
            }
        }
    }

    /// <summary>
    /// Called by <see cref="PixieButton"/>.
    /// </summary>
    internal void CheckedChanged(PixieButton obj)
    {
        if (obj.IsBackgroundChecked)
        {
            foreach (var item in Children)
            {
                if (item != obj && item is PixieButton other)
                {
                    other.IsBackgroundChecked = false;
                }
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Styling.StylingChanged += StylingChangedHandler;
        RebuildChildren();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChildBackgroundProperty || p == ChildCornerProperty ||
            p == ChildIndentProperty || p == IsCornerGroupedProperty ||
            p == ChildBorderProperty)
        {
            if (!_rebuild.IsPending)
            {
                _update.Post();
            }

            return;
        }

        if (p == TopTitleProperty)
        {
            var text = change.GetNewValue<string?>();

            if (!string.IsNullOrEmpty(text))
            {
                _titleBlock.Text = text;
                _titleBlock.IsVisible = true;
                return;
            }

            _titleBlock.Text = null;
            _titleBlock.IsVisible = false;
            return;
        }

        if (p == TopFooterProperty)
        {
            var text = change.GetNewValue<string?>();

            if (!string.IsNullOrEmpty(text))
            {
                _taglineBlock.Text = text;
                _taglineBlock.IsVisible = true;
                return;
            }

            _taglineBlock.Text = null;
            _taglineBlock.IsVisible = false;
            return;
        }

        if (p == IsOpenProperty)
        {
            SetButtonOpen(_collapseButton, change.GetNewValue<bool>());
            _rebuild.Post();
            _update.Cancel();
            return;
        }

        if (p == IsCollapsableProperty)
        {
            _collapseButton = change.GetNewValue<bool>() ? NewCollapseButton() : null;
            _rebuild.Post();
            _update.Cancel();
            return;
        }

        if (p == CollapseHeaderProperty)
        {
            _collapseButton?.SetValue(PixieControl.HeaderProperty, change.GetNewValue<string?>());
            return;
        }

        if (p == CollapseTitleProperty)
        {
            _collapseButton?.SetValue(PixieControl.TitleProperty, change.GetNewValue<string?>());
            return;
        }

        if (p == CollapseTitleWeightProperty)
        {
            _collapseButton?.SetValue(PixieControl.TitleWeightProperty, change.GetNewValue<FontWeight>());
            return;
        }

        if (p == CollapseFooterProperty)
        {
            _collapseButton?.SetValue(PixieControl.FooterProperty, change.GetNewValue<string?>());
            return;
        }

        if (p == CollapseSymbolProperty)
        {
            _collapseButton?.SetValue(PixieControl.LeftSymbolProperty, change.GetNewValue<string?>());
            return;
        }
    }

    private static void SetButtonOpen(PixieButton? button, bool open)
    {
        if (button != null)
        {
            button.RightSymbol = open ? Symbols.KeyboardArrowDown : Symbols.KeyboardArrowRight;
        }
    }

    private PixieButton NewCollapseButton()
    {
        var button = new PixieButton();
        button.Header = CollapseHeader;
        button.Title = CollapseTitle;
        button.TitleWeight = CollapseTitleWeight;
        button.Footer = CollapseFooter;
        button.LeftSymbol = CollapseSymbol;
        SetButtonOpen(button, IsOpen);

        button.BackgroundClick += CollapseClickHandler;

        return button;
    }

    private void UpdateChildren(List<Control>? rebuild = null)
    {
        var radius = ChildCorner;
        var background = ChildBackground;
        var border = ChildBorder;
        var borderPx = border != null ? 1.0 : 0.0;
        var indent = ChildIndent;
        bool block = IsCornerGrouped;

        PixieControl? firstPixie = null;
        PixieControl? lastPixie = null;

        CornerRadius topRadius = block ? new(radius.TopLeft, radius.TopRight, 0.0, 0.0) : radius;
        CornerRadius bottomRadius = block ?  new(0.0, 0.0, radius.BottomLeft, radius.BottomRight) : radius;
        CornerRadius sideRadius = block ? default : radius;

        // Treat these as logically separate even tho currently same
        Thickness topThickness = new (borderPx);
        Thickness bottomThickness = new (borderPx);
        Thickness sideThickness = new (borderPx);

        if (_collapseButton != null && IsCollapsable) // <- must check both
        {
            firstPixie = _collapseButton;
            lastPixie = firstPixie;
            rebuild?.Add(firstPixie);

            firstPixie.BorderBrush = border;
            firstPixie.Background = background;
        }

        if (IsOpen)
        {
            foreach (var item in Children)
            {
                rebuild?.Add(item);

                if (item is PixieControl pixie)
                {
                    if (block && item.IsVisible)
                    {
                        firstPixie ??= pixie;
                        lastPixie = pixie;
                    }

                    pixie.BorderBrush = border;
                    pixie.Background = background;

                    pixie.LeftIndent = indent;
                    pixie.CornerRadius = sideRadius;
                    pixie.BorderThickness = sideThickness;
                }
            }
        }

        if (firstPixie != null && lastPixie != null)
        {
            if (firstPixie == lastPixie)
            {
                firstPixie.CornerRadius = radius;
                firstPixie.BorderThickness = new (borderPx);
            }
            else
            {
                firstPixie.CornerRadius = topRadius;
                firstPixie.BorderThickness = topThickness;

                lastPixie.CornerRadius = bottomRadius;
                lastPixie.BorderThickness = bottomThickness;
            }
        }

        // Accoutrements
        Control? head = null;
        const double spacer = ChromeFonts.DefaultFontSize; // <- use as space size

        // Order important (reverse)
        rebuild?.Insert(0, _taglineBlock);
        _taglineBlock.Margin = new(0, spacer / 3.0, 0, 0);

        if (_taglineBlock.IsVisible)
        {
            head = _taglineBlock;
        }


        rebuild?.Insert(0, _titleBlock);
        _titleBlock.Margin = default;

        if (_titleBlock.IsVisible)
        {
            head ??= _titleBlock;
        }

        if (head != null)
        {
            var m = head.Margin;
            head.Margin = new(m.Left, m.Top, m.Right, spacer);
        }
    }

    private void RebuildChildren()
    {
        var rebuild = new List<Control>(Children.Count + 4);
        UpdateChildren(rebuild);
        base.Children.Replace(rebuild);
    }

    private void RebuildPostHandler(object? _, EventArgs __)
    {
        RebuildChildren();
    }

    private void UpdatePostHandler(object? _, EventArgs __)
    {
        UpdateChildren();
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        UpdateChildren();
    }

    private void ChildEnteredHandler(object? sender, EventArgs __)
    {
        foreach (var item in Children)
        {
            if (item != sender && item is PixieControl pixie)
            {
                pixie.ClearFocusBackground();
            }
        }
    }

    private void PixieChildrenChangedHandler(object? _, NotifyCollectionChangedEventArgs e)
    {
        var items = e.OldItems;

        if (items != null && e.Action != NotifyCollectionChangedAction.Move)
        {
            foreach (var item in items)
            {
                if (item is PixieControl pixie)
                {
                    pixie.Group = null;
                    pixie.LeftIndent = 0.0;
                    pixie.PointerEntered -= ChildEnteredHandler;

                    // Must remove immediate to detach the parent
                    base.Children.Remove(pixie);
                }
            }
        }

        items = e.NewItems;

        if (items != null && e.Action != NotifyCollectionChangedAction.Move)
        {
            foreach (var item in items)
            {
                if (item is PixieControl pixie)
                {
                    if (pixie.Group == null)
                    {
                        pixie.Group = this;
                        pixie.PointerEntered += ChildEnteredHandler;
                    }
                    else
                    if (pixie.Group != this)
                    {
                        throw new InvalidOperationException($"{pixie.GetType().Name} already belongs to different {nameof(PixieGroup)}");
                    }

                    if (pixie is PixieRadio radio)
                    {
                        CheckedChanged(radio);
                    }
                    else
                    if (pixie is PixieButton btn)
                    {
                        CheckedChanged(btn);
                    }

                }
            }
        }

        _rebuild.Post();
        _update.Cancel();
    }

    private void PixiePropertyChange(object? _, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsVisibleProperty)
        {
            _rebuild.Post();
            _update.Cancel();
        }
    }

    private void CollapseClickHandler(object? _, EventArgs __)
    {
        IsOpen = !IsOpen;
    }
}
