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

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Visual group manager for <see cref="PixieControl"/> children.
/// </summary>
/// <remarks>
/// This class will maintain the background, corners and <see cref="PixieControl.Group"/> properties of children derived
/// from <see cref="PixieControl"/>. Other class types may be added, but these should ideally be present only at the
/// start or end of the <see cref="Children"/>. The groups's own background should not normally be set. Instead, the
/// <see cref="ChildBackground"/> and <see cref="ChildCorner"/> may be bound to styling. <see cref="PixieGroup"/> also
/// maintains exclusivity for <see cref="PixieRadio"/> and <see cref="PixieCard"/>. The initial values of the base class
/// Spacing and HorizontalAlignment are set to 1.0 and "Stretch" respectively on construction.
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
    private readonly DispatchCoalescer _update = new(DispatcherPriority.Render);
    private readonly DispatchCoalescer _rebuild = new(DispatcherPriority.Render);
    private PixieCard? _collapser;

    // Backing fields
    private string? _topTitle;
    private string? _topFooter;
    private bool _isOpen = DefaultIsOpen;
    private bool _isCollapsible;
    private string? _collapseHeader;
    private string? _collapseFooter;
    private string? _collapseSymbol;
    private string? _collapseTitle;
    private bool _isCollapseHoverButton;
    private ContextMenu? _collapseDropMenu;

    static PixieGroup()
    {
        UseLayoutRoundingProperty.OverrideDefaultValue<PixieGroup>(false);
        OrientationProperty.OverrideDefaultValue<PixieGroup>(Avalonia.Layout.Orientation.Vertical);
        SpacingProperty.OverrideDefaultValue<PixieGroup>(1.0);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieGroup()
    {
        Diag.ThrowIfTrue(base.UseLayoutRounding);
        Diag.ThrowIfNotEqual(Avalonia.Layout.Orientation.Vertical, base.Orientation);
        Diag.ThrowIfNotEqual(Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalAlignment);

        _titleBlock.IsVisible = false;
        _titleBlock.FontWeight = FontWeight.Bold;
        _titleBlock.TextWrapping = TextWrapping.Wrap;

        _taglineBlock.FontSize = ChromeFonts.SmallFontSize;
        _taglineBlock.TextWrapping = TextWrapping.Wrap;
        _taglineBlock.Foreground = ChromeStyling.GrayForeground;
        _taglineBlock.IsVisible = false;

        _update.Posted += (_, __) => UpdateChildren();
        _rebuild.Posted += (_, __) => RebuildChildren();
        Children.CollectionChanged += PixieChildrenChangedHandler;
    }

    /// <summary>
    /// Defines the <see cref="ChildRenaming"/> event.
    /// </summary>
    public static readonly RoutedEvent<GroupChildRenamingEventArgs> ChildRenamingEvent =
        RoutedEvent.Register<PixieGroup, GroupChildRenamingEventArgs>(nameof(ChildRenaming), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="CollapseRenaming"/> event.
    /// </summary>
    public static readonly RoutedEvent<GroupChildRenamingEventArgs> CollapseRenamingEvent =
        RoutedEvent.Register<PixieGroup, GroupChildRenamingEventArgs>(nameof(CollapseRenaming), RoutingStrategies.Direct);

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
    /// Defines the <see cref="IsCollapsible"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, bool> IsCollapsibleProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, bool>(nameof(IsCollapsible),
        o => o.IsCollapsible, (o, v) => o.IsCollapsible = v);

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
    /// Defines the <see cref="IsCollapseHoverButton"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, bool> IsCollapseHoverButtonProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, bool>(nameof(IsCollapseHoverButton),
        o => o.IsCollapseHoverButton, (o, v) => o.IsCollapseHoverButton = v);

    /// <summary>
    /// Defines the <see cref="CollapseTitle"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, string?> CollapseTitleProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, string?>(nameof(CollapseTitle),
        o => o.CollapseTitle, (o, v) => o.CollapseTitle = v);

    /// <summary>
    /// Defines the <see cref="CollapseDropMenu"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieGroup, ContextMenu?> CollapseDropMenuProperty =
        AvaloniaProperty.RegisterDirect<PixieGroup, ContextMenu?>(nameof(CollapseDropMenu),
        o => o.CollapseDropMenu, (o, v) => o.CollapseDropMenu = v);

    /// <summary>
    /// Defines the <see cref="CollapseTitleWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> CollapseTitleWeightProperty =
        AvaloniaProperty.Register<PixieGroup, FontWeight>(nameof(CollapseTitleWeight), FontWeight.Normal);

    /// <summary>
    /// Defines the <see cref="IsCapped"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCappedProperty =
        AvaloniaProperty.Register<PixieGroup, bool>(nameof(IsCapped));

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
    /// Defines the <see cref="ChildIndent"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ChildIndentProperty =
        AvaloniaProperty.Register<PixieGroup, double>(nameof(ChildIndent));

    /// <summary>
    /// Occurs when the user attempts to rename the title of a <see cref="PixieCard"/> child instance.
    /// </summary>
    /// <remarks>
    /// The <see cref="RoutedEventArgs.Source"/> provides the <see cref="PixieCard"/> instance. The handler has the
    /// option to deny the change by setting <see cref="SubmittedEventArgs.IsRejected"/> to true
    /// </remarks>
    public event EventHandler<GroupChildRenamingEventArgs> ChildRenaming
    {
        add { AddHandler(ChildRenamingEvent, value); }
        remove { RemoveHandler(ChildRenamingEvent, value); }
    }

    /// <summary>
    /// Occurs when the user attempts to rename the <see cref="CollapseTitle"/> in the user interface.
    /// </summary>
    /// <remarks>
    /// The handler has the option to deny the change by setting <see cref="SubmittedEventArgs.IsRejected"/> to true.
    /// </remarks>
    public event EventHandler<GroupChildRenamingEventArgs> CollapseRenaming
    {
        add { AddHandler(CollapseRenamingEvent, value); }
        remove { RemoveHandler(CollapseRenamingEvent, value); }
    }

    /// <summary>
    /// Gets or sets the top title text.
    /// </summary>
    /// <remarks>
    /// This text appears above the main control.
    /// </remarks>
    public string? TopTitle
    {
        get { return _topTitle; }
        set { SetAndRaise(TopTitleProperty, ref _topTitle, value); }
    }

    /// <summary>
    /// Gets or sets the tagline text shown under <see cref="TopTitle"/>.
    /// </summary>
    /// <remarks>
    /// This text appears above the main control but below <see cref="TopTitle"/>.
    /// </remarks>
    public string? TopFooter
    {
        get { return _topFooter; }
        set { SetAndRaise(TopFooterProperty, ref _topFooter, value); }
    }

    /// <summary>
    /// Gets or sets whether the group is open (true), or collapsed (false).
    /// </summary>
    /// <remarks>
    /// The group will effectively be hidden if both <see cref="IsOpen"/> is false and <see cref="IsCollapsible"/> are
    /// false. The default is true.
    /// </remarks>
    public bool IsOpen
    {
        get { return _isOpen; }
        set { SetAndRaise(IsOpenProperty, ref _isOpen, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a control allowing the user to toggle <see cref="IsOpen"/> and collapse the group's
    /// contents.
    /// </summary>
    /// <remarks>
    /// The group will effectively be hidden if both <see cref="IsOpen"/> is false and <see cref="IsCollapsible"/> are
    /// false. The default is false.
    /// </remarks>
    public bool IsCollapsible
    {
        get { return _isCollapsible; }
        set { SetAndRaise(IsCollapsibleProperty, ref _isCollapsible, value); }
    }

    /// <summary>
    /// Gets or sets the text shown in the collapse control above <see cref="CollapseTitle"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsible"/> property must be true otherwise this text value is not shown. The default is null.
    /// </remarks>
    public string? CollapseHeader
    {
        get { return _collapseHeader; }
        set { SetAndRaise(CollapseHeaderProperty, ref _collapseHeader, value); }
    }

    /// <summary>
    /// Gets or sets the text shown in the collapse header below <see cref="CollapseTitle"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsible"/> property must be true otherwise this text value is not shown. The default is null.
    /// </remarks>
    public string? CollapseFooter
    {
        get { return _collapseFooter; }
        set { SetAndRaise(CollapseFooterProperty, ref _collapseFooter, value); }
    }

    /// <summary>
    /// Gets or sets the symbol shown in the collapse control.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsible"/> property must be true otherwise this text value is not shown. The default is null.
    /// </remarks>
    public string? CollapseSymbol
    {
        get { return _collapseSymbol; }
        set { SetAndRaise(CollapseSymbolProperty, ref _collapseSymbol, value); }
    }

    /// <summary>
    /// Gets or sets whether the button used for <see cref="CollapseDropMenu"/> has an opacity of 0 when the pointer is
    /// not hovering over the control.
    /// </summary>
    public bool IsCollapseHoverButton
    {
        get { return _isCollapseHoverButton; }
        set { SetAndRaise(IsCollapseHoverButtonProperty, ref _isCollapseHoverButton, value); }
    }

    /// <summary>
    /// Gets or sets the title shown in the collapse control.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsible"/> property must be true otherwise the title is not shown. The default is null.
    /// </remarks>
    public string? CollapseTitle
    {
        get { return _collapseTitle; }
        set { SetAndRaise(CollapseTitleProperty, ref _collapseTitle, value); }
    }

    /// <summary>
    /// Gets or sets a ContextMenu associated with the collapse control.
    /// </summary>
    /// <remarks>
    /// When not null, a button appears in the collapse control. The default is null.
    /// </remarks>
    public ContextMenu? CollapseDropMenu
    {
        get { return _collapseDropMenu; }
        set { SetAndRaise(CollapseDropMenuProperty, ref _collapseDropMenu, value); }
    }

    /// <summary>
    /// Gets or sets whether the <see cref="ChildCorner"/> is applied to the group as a whole, i.e. top and bottom
    /// children only with children in between having no corners.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool IsCapped
    {
        get { return GetValue(IsCappedProperty); }
        set { SetValue(IsCappedProperty, value); }
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
    /// The value is applied to top and bottom children only where <see cref="IsCapped"/> is true.
    /// </remarks>
    public CornerRadius ChildCorner
    {
        get { return GetValue(ChildCornerProperty); }
        set { SetValue(ChildCornerProperty, value); }
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
    /// Gets the current member with <see cref="PixieCard.IsChecked"/> equal to true.
    /// </summary>
    public PixieCard? CheckedCard {get; private set;}

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
    public bool Search(string? keyword, List<PixieFinding> findings)
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
                flag |= control.Search(keyword, findings);
                continue;
            }

            if (item is PixieGroup group)
            {
                // Group within a group
                flag |= group.Search(keyword, findings);
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

            if (IsCollapsible &&
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
    /// Starts the rename editor for <see cref="CollapseTitle"/> and returns true on success.
    /// </summary>
    /// <remarks>
    /// The <see cref="IsCollapsible"/> value must be true otherwise the result is false. The editor is closed if it
    /// loses focus or <see cref="Children"/> changes. The <see cref="CollapseRenaming"/> occurs when the user makes the
    /// change with "this" instance as the sender. The result only indicates the that editor started, not that the user
    /// confirmed the rename.
    /// </remarks>
    public bool StartRename(int maxLength, int minLength = 1)
    {
        if (_isCollapsible && _collapser != null)
        {
            return StartRename(_collapser, maxLength, minLength);
        }

        return false;
    }

    /// <summary>
    /// Starts the rename editor for the given "child" and returns true on success.
    /// </summary>
    /// <remarks>
    /// The "child" must be a member of <see cref="Children"/> otherwise the result is false. The editor is closed if it
    /// loses focus or <see cref="Children"/> changes. The <see cref="ChildRenaming"/> occurs when the user makes
    /// the change with "child" as the "sender".
    /// </remarks>
    public bool StartRename(PixieCard child, int maxLength, int minLength = 1)
    {
        if (!child.IsEffectivelyVisible || !child.IsEffectivelyEnabled)
        {
            return false;
        }

        Diag.ThrowIfNotSame(this, child.Group);
        int index = base.Children.IndexOf(child);

        if (index < 0)
        {
            return false;
        }

        // The renamer will take care of everything else
        var obj = new GroupRenamer(child, maxLength, minLength);

        if (child != _collapser)
        {
            obj.ContentPadding = new(ChildIndent, 0.0, 0.0, 0.0);
        }

        obj.Renaming += RenamedHandler;
        base.Children.Insert(index + 1, obj);

        return true;
    }

    /// <summary>
    /// Brings into view and calls attention.
    /// </summary>
    /// <remarks>
    /// The call does nothing unless <see cref="IsCollapsible"/> is true.
    /// </remarks>
    public virtual void Attention(IBrush? background = null)
    {
        _collapser?.Attention(background);
    }

    /// <summary>
    /// Called by <see cref="PixieRadio"/>.
    /// </summary>
    internal void SetRadioOnCheckedChanged(PixieRadio obj)
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
    /// Called by <see cref="PixieCard"/>.
    /// </summary>
    internal void SetCardOnCheckedChanged(PixieCard? obj)
    {
        if (obj?.IsChecked == true)
        {
            if (CheckedCard != obj)
            {
                CheckedCard?.IsChecked = false;
            }

            CheckedCard = obj;
        }
        else
        {
            CheckedCard?.IsChecked = false;
            CheckedCard = null;
        }
    }

    internal void Remove(GroupRenamer obj)
    {
        if (obj.Source != null)
        {
            base.Children.Remove(obj);
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RebuildChildren();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Down || e.Key == Key.Up)
        {
            bool next = false;
            PixieCard? last = null;

            foreach (var item in Children)
            {
                if (item is PixieCard card)
                {
                    if (card.IsChecked)
                    {
                        if (e.Key == Key.Up && last != null)
                        {
                            e.Handled = true;
                            last.OnClick();
                            return;
                        }

                        if (e.Key == Key.Down && !next)
                        {
                            next = true;
                            continue;
                        }
                    }

                    if (next)
                    {
                        e.Handled = true;
                        card.OnClick();
                        return;
                    }

                    last = card;
                }
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ChildBackgroundProperty || p == ChildCornerProperty ||
            p == ChildIndentProperty || p == IsCappedProperty)
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

            if (text != null)
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

            if (text != null)
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
            SetGroupOpen(_collapser, change.GetNewValue<bool>());
            _rebuild.Post();
            _update.Cancel();
            return;
        }

        if (p == IsCollapsibleProperty)
        {
            _collapser = change.GetNewValue<bool>() ? CreateCollapser() : null;
            _rebuild.Post();
            _update.Cancel();
            return;
        }

        if (p == CollapseHeaderProperty)
        {
            _collapser?.Header = change.GetNewValue<string?>();
            return;
        }

        if (p == CollapseTitleProperty)
        {
            _collapser?.Title = change.GetNewValue<string?>();
            return;
        }

        if (p == CollapseDropMenuProperty)
        {
            var value = change.GetNewValue<ContextMenu?>();
            _collapser?.RightButton.DropMenu = value;
            _collapser?.RightButton.IsVisible = value != null;
            return;
        }

        if (p == CollapseTitleWeightProperty)
        {
            _collapser?.TitleWeight = change.GetNewValue<FontWeight>();
            return;
        }

        if (p == CollapseFooterProperty)
        {
            _collapser?.Footer = change.GetNewValue<string?>();
            return;
        }

        if (p == CollapseSymbolProperty)
        {
            _collapser?.LeftSymbol = change.GetNewValue<string?>();
            return;
        }

        if (p == IsCollapseHoverButtonProperty)
        {
            _collapser?.IsHoverButton = change.GetNewValue<bool>();
            return;
        }
    }

    private static void SetGroupOpen(PixieCard? control, bool open)
    {
        if (control != null)
        {
            control.RightSymbol = open ? Symbols.KeyboardArrowDown : Symbols.KeyboardArrowRight;
        }
    }

    private PixieCard CreateCollapser()
    {
        var obj = new PixieCard();
        obj.Group = this;
        obj.Header = _collapseHeader;
        obj.Title = _collapseTitle;
        obj.TitleWeight = CollapseTitleWeight;
        obj.Footer = _collapseFooter;
        obj.LeftSymbol = _collapseSymbol;
        obj.IsHoverButton = _isCollapseHoverButton;
        obj.RightButton.DropMenu = _collapseDropMenu;
        obj.RightButton.IsVisible = _collapseDropMenu != null;

        SetGroupOpen(obj, IsOpen);
        obj.Click += (_, __) => IsOpen = !IsOpen;

        return obj;
    }

    private void UpdateChildren(List<Control>? rebuild = null)
    {
        var radius = ChildCorner;
        var background = ChildBackground;
        var indent = ChildIndent;
        bool block = IsCapped;

        PixieControl? firstPixie = null;
        PixieControl? lastPixie = null;

        CornerRadius topRadius = block ? new(radius.TopLeft, radius.TopRight, 0.0, 0.0) : radius;
        CornerRadius bottomRadius = block ? new(0.0, 0.0, radius.BottomLeft, radius.BottomRight) : radius;
        CornerRadius sideRadius = block ? default : radius;

        if (_collapser != null && IsCollapsible) // <- must check both
        {
            firstPixie = _collapser;
            lastPixie = firstPixie;
            rebuild?.Add(firstPixie);

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

                    pixie.LeftIndent = indent;
                    pixie.Background = background;
                    pixie.CornerRadius = sideRadius;
                }
            }
        }

        if (firstPixie != null && lastPixie != null)
        {
            if (firstPixie == lastPixie)
            {
                firstPixie.CornerRadius = radius;
            }
            else
            {
                firstPixie.CornerRadius = topRadius;
                lastPixie.CornerRadius = bottomRadius;
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

                    // Must remove immediate to detach the parent
                    base.Children.Remove(pixie);

                    if (CheckedCard == pixie)
                    {
                        SetCardOnCheckedChanged(null);
                    }
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
                    }
                    else
                    if (pixie.Group != this)
                    {
                        throw new InvalidOperationException($"{pixie.GetType().Name} already belongs to different {nameof(PixieGroup)}");
                    }

                    if (pixie is PixieCard btn)
                    {
                        SetCardOnCheckedChanged(btn);
                    }
                    else
                    if (pixie is PixieRadio radio)
                    {
                        SetRadioOnCheckedChanged(radio);
                    }
                }
            }
        }

        _rebuild.Post();
        _update.Cancel();
    }

    private void RenamedHandler(object? sender, SubmittedEventArgs e)
    {
        GroupChildRenamingEventArgs es;

        if (sender == _collapser)
        {
            es = new(CollapseRenamingEvent, null, e);
        }
        else
        {
            es = new(ChildRenamingEvent, (PixieCard)sender!, e);
        }

        RaiseEvent(es);
        e.Handled = es.Handled;
        e.IsRejected = es.IsRejected;
    }
}
