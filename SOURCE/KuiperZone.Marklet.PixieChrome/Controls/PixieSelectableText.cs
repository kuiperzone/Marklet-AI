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
/// Composite control with selectable <see cref="Text"/> content.
/// </summary>
public class PixieSelectableText : PixieControl
{
    private readonly CrossTextBlock _block = new();

    private string? _text;
    private double _titleSize = ChromeFonts.DefaultFontSize;
    private TextAlignment _titleAlignment;
    private TextWrapping _textWrapping = TextWrapping.Wrap;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PixieSelectableText()
    {
        SetSubject(_block);

        _block.Focusable = true;
        _block.FocusAdorner = null;

        _block.FontWeight = TitleWeight;
        _block.FontSize = _titleSize;
        _block.TextAlignment = _titleAlignment;
        _block.VerticalAlignment = VerticalContentAlignment;
        _block.Margin = new(0.0, VerticalContentOffset, 0.0, VerticalContentOffset);
    }

    /// <summary>
    /// Defines the <see cref="TextAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieSelectableText, string?> TextProperty =
        AvaloniaProperty.RegisterDirect<PixieSelectableText, string?>(nameof(TitleSize),
        o => o.Text, (o, v) => o.Text = v);

    /// <summary>
    /// Defines the <see cref="TextAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieSelectableText, double> TextSizeProperty =
        AvaloniaProperty.RegisterDirect<PixieSelectableText, double>(nameof(TitleSize),
        o => o.TitleSize, (o, v) => o.TitleSize = v, ChromeFonts.DefaultFontSize);

    /// <summary>
    /// Defines the <see cref="TextAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieSelectableText, TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.RegisterDirect<PixieSelectableText, TextAlignment>(nameof(TextAlignment),
        o => o.TextAlignment, (o, v) => o.TextAlignment = v);

    /// <summary>
    /// Defines the <see cref="TextWrapping"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieSelectableText, TextWrapping> TextWrappingProperty =
        AvaloniaProperty.RegisterDirect<PixieSelectableText, TextWrapping>(nameof(TextWrapping),
        o => o.TextWrapping, (o, v) => o.TextWrapping = v, TextWrapping.Wrap);

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public string? Text
    {
        get { return _text; }
        set { SetAndRaise(TextProperty, ref _text, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="Text"/> font size.
    /// </summary>
    public double TitleSize
    {
        get { return _titleSize; }
        set { SetAndRaise(TextSizeProperty, ref _titleSize, value); }
    }

    /// <summary>
    /// Gets or sets the <see cref="Text"/> alignment.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get { return _titleAlignment; }
        set { SetAndRaise(TextAlignmentProperty, ref _titleAlignment, value); }
    }

    /// <summary>
    /// Gets or sets <see cref="Text"/> wrapping.
    /// </summary>
    /// <remarks>
    /// The default here is <see cref="TextWrapping.Wrap"/>, with character ellipses trimming.
    /// </remarks>
    public TextWrapping TextWrapping
    {
        get { return _textWrapping; }
        set { SetAndRaise(TextWrappingProperty, ref _textWrapping, value); }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == TextProperty)
        {
            _block.Text = change.GetNewValue<string?>();
            return;
        }

        if (p == TextSizeProperty)
        {
            _block.FontSize = change.GetNewValue<double>();
            return;
        }

        if (p == TextAlignmentProperty)
        {
            _block.TextAlignment = change.GetNewValue<TextAlignment>();
            return;
        }

        if (p == TextWrappingProperty)
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
