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
using Avalonia.Media;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Composite control with selectable <see cref="PixieControl.Title"/> text.
/// </summary>
public class PixieSelectable : PixieControl
{
    private readonly CrossTextBlock _block = new();

    private double _titleSize = ChromeFonts.DefaultFontSize;
    private TextAlignment _titleAlignment;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieSelectable()
        : base(false, Avalonia.Layout.VerticalAlignment.Center)
    {
        IsTitleVisible = false;
        SetSubject(_block);

        _block.Focusable = true;
        _block.FocusAdorner = null;

        _block.FontWeight = TitleWeight;
        _block.FontSize = _titleSize;
        _block.TextAlignment = _titleAlignment;
        _block.VerticalAlignment = VerticalContentAlignment;
        _block.Margin = new(0.0, VerticalContentOffset, 0.0, VerticalContentOffset);

        TitleWrapping = TextWrapping.Wrap;
    }

    /// <summary>
    /// Defines the <see cref="TitleAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieSelectable, double> TitleSizeProperty =
        AvaloniaProperty.RegisterDirect<PixieSelectable, double>(nameof(TitleSize),
        o => o.TitleSize, (o, v) => o.TitleSize = v, ChromeFonts.DefaultFontSize);

    /// <summary>
    /// Defines the <see cref="TitleAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieSelectable, TextAlignment> TitleAlignmentProperty =
        AvaloniaProperty.RegisterDirect<PixieSelectable, TextAlignment>(nameof(TitleAlignment),
        o => o.TitleAlignment, (o, v) => o.TitleAlignment = v);

    /// <summary>
    /// Gets or sets the title font size.
    /// </summary>
    public double TitleSize
    {
        get { return _titleSize; }
        set { SetAndRaise(TitleSizeProperty, ref _titleSize, value); }
    }

    /// <summary>
    /// Gets or sets the title text alignment.
    /// </summary>
    public TextAlignment TitleAlignment
    {
        get { return _titleAlignment; }
        set { SetAndRaise(TitleAlignmentProperty, ref _titleAlignment, value); }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == TitleProperty)
        {
            _block.Text = change.GetNewValue<string?>();
            return;
        }

        if (p == TitleSizeProperty)
        {
            _block.FontSize = change.GetNewValue<double>();
            return;
        }

        if (p == TitleAlignmentProperty)
        {
            _block.TextAlignment = change.GetNewValue<TextAlignment>();
            return;
        }

        if (p == TitleWrappingProperty)
        {
            _block.TextWrapping = change.GetNewValue<TextWrapping>();
            return;
        }

        if (p == TitleWeightProperty)
        {
            _block.FontWeight = change.GetNewValue<FontWeight>();
            return;
        }

        if (change.Property == IsEffectivelyEnabledProperty)
        {
            SetTextEnabled(_block, change.GetNewValue<bool>());
            return;
        }

    }
}
