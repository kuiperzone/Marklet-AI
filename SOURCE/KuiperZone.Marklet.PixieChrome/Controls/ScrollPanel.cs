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
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Threading;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// <see cref="ScrollViewer"/> with an inner vertical <see cref="StackPanel"/>.
/// </summary>
/// <remarks>
/// The inner panel is aligned Top and horizontally centered. The <see cref="ScrollViewer.VerticalScrollBarVisibility"/>
/// is initialized to Auto. The <see cref="Children"/> are centered in the view.
/// </remarks>
public sealed class ScrollPanel : ScrollViewer
{
    private readonly StackPanel _contentPanel = new();
    private readonly DispatchCoalescer _dispatcher = new(DispatcherPriority.ContextIdle);
    private double _pendingY;

    private Thickness _contentPadding;
    private double _contentMinWidth;
    private double _contentMaxWidth = double.PositiveInfinity;
    private double _verticalSpacing;

    static ScrollPanel()
    {
        VerticalScrollBarVisibilityProperty.OverrideDefaultValue<ScrollPanel>(ScrollBarVisibility.Auto);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ScrollPanel()
    {
        Content = _contentPanel;
        Children = _contentPanel.Children;

        _contentPanel.Orientation = Avalonia.Layout.Orientation.Vertical;
        _contentPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

        // Stretch not center
        ConditionalDebug.ThrowIfNotEqual(ScrollBarVisibility.Auto, VerticalScrollBarVisibility);
        ConditionalDebug.ThrowIfNotEqual(Avalonia.Layout.HorizontalAlignment.Stretch, _contentPanel.HorizontalAlignment);

        _dispatcher.Posted += NormYHandler;
    }

    /// <summary>
    /// Defines the <see cref="ContentMargin"/> property.
    /// </summary>
    public static readonly DirectProperty<ScrollPanel, Thickness> ContentMarginProperty =
        AvaloniaProperty.RegisterDirect<ScrollPanel, Thickness>(nameof(ContentMargin),
        o => o.ContentMargin, (o, v) => o.ContentMargin = v);

    /// <summary>
    /// Defines the <see cref="ContentMinWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<ScrollPanel, double> ContentMinWidthProperty =
        AvaloniaProperty.RegisterDirect<ScrollPanel, double>(nameof(ContentMinWidth),
        o => o.ContentMinWidth, (o, v) => o.ContentMinWidth = v);

    /// <summary>
    /// Defines the <see cref="ContentMaxWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<ScrollPanel, double> ContentMaxWidthProperty =
        AvaloniaProperty.RegisterDirect<ScrollPanel, double>(nameof(ContentMaxWidth),
        o => o.ContentMaxWidth, (o, v) => o.ContentMaxWidth = v, double.PositiveInfinity);

    /// <summary>
    /// Defines the <see cref="VerticalSpacing"/> property.
    /// </summary>
    public static readonly DirectProperty<ScrollPanel, double> VerticalSpacingProperty =
        AvaloniaProperty.RegisterDirect<ScrollPanel, double>(nameof(VerticalSpacing),
        o => o.VerticalSpacing, (o, v) => o.VerticalSpacing = v);

    /// <summary>
    /// Gets the children of the panel.
    /// </summary>
    [Content]
    public AvaloniaList<Control> Children { get; }

    /// <summary>
    /// Gets or sets the the inner content padding.
    /// </summary>
    public Thickness ContentMargin
    {
        get { return _contentPadding; }
        set { SetAndRaise(ContentMarginProperty, ref _contentPadding, value); }
    }

    /// <summary>
    /// Gets or sets the inner content minimum width.
    /// </summary>
    public double ContentMinWidth
    {
        get { return _contentMinWidth; }
        set { SetAndRaise(ContentMinWidthProperty, ref _contentMinWidth, value); }
    }

    /// <summary>
    /// Gets or sets the inner content minimum width.
    /// </summary>
    public double ContentMaxWidth
    {
        get { return _contentMaxWidth; }
        set { SetAndRaise(ContentMaxWidthProperty, ref _contentMaxWidth, value); }
    }

    /// <summary>
    /// Gets or sets the the inner panel spacing between items.
    /// </summary>
    public double VerticalSpacing
    {
        get { return _verticalSpacing; }
        set { SetAndRaise(VerticalSpacingProperty, ref _verticalSpacing, value); }
    }

    /// <summary>
    /// Gets or sets a normalized vertical position of range [0, 1].
    /// </summary>
    /// <remarks>
    /// Setting is asynchronous and the getter won't return the value until rendered. Setting NaN does nothing.
    /// </remarks>
    public double NormalizedY
    {
        get
        {
            var range = Extent.Height - Viewport.Height;
            return Math.Max(Offset.Y / range, 0.0);
        }

        set
        {
            if (!double.IsNaN(value))
            {
                _pendingY = value;
                _dispatcher.Post();
            }
        }
    }

    /// <summary>
    /// Gets the actual internal content width in pixels.
    /// </summary>
    public double ActualContentWidth
    {
        get { return _contentPanel.Bounds.Width; }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(ScrollViewer);

    /// <summary>
    /// Hide Content from XAML.
    /// </summary>
    private new object? Content
    {
        get { return base.Content; }
        set { base.Content = value; }
    }

    /// <summary>
    /// Returns true if the scroller is vertically at the bottom, or within "margin" pixels of the bottom.
    /// </summary>
    public bool IsBottom(double margin = 16.0)
    {
        if (Extent.Height <= Viewport.Height)
        {
            return true;
        }

        var maxY = Extent.Height - Viewport.Height;
        return Offset.Y >= maxY - margin;
    }

    /// <summary>
    /// Override.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == ContentProperty && change.OldValue != null)
        {
            throw new InvalidOperationException($"Cannot set {p.Name} on {GetType().Name}");
        }

        if (p == ContentMarginProperty)
        {
            _contentPanel.Margin = change.GetNewValue<Thickness>();
            return;
        }

        if (p == ContentMinWidthProperty)
        {
            _contentPanel.MinWidth = change.GetNewValue<double>();
            return;
        }

        if (p == ContentMaxWidthProperty)
        {
            _contentPanel.MaxWidth = change.GetNewValue<double>();
            return;
        }

        if (p == VerticalSpacingProperty)
        {
            _contentPanel.Spacing = change.GetNewValue<double>();
            return;
        }
    }

    /// <summary>
    /// Override.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _contentPanel.Width = Math.Max(e.NewSize.Width - ContentMargin.Width(), ContentMinWidth);
    }

    private bool SetNormalizedVerticalPosition(double normY)
    {
        if (double.IsNaN(normY))
        {
            return false;
        }

        var range = Extent.Height - Viewport.Height;

        if (range > double.Epsilon)
        {
            Offset = new Vector(Offset.X, range * Math.Clamp(normY, 0.0, 1.0));
            return true;
        }

        return false;
    }

    private void NormYHandler(object? _, EventArgs __)
    {
        SetNormalizedVerticalPosition(_pendingY);
        _pendingY = 0.0;
    }

}