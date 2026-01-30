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

using System.Text;
using Avalonia.Layout;
using Avalonia.Media;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Text block.
/// </summary>
internal class MarkTextHost : MarkBlockHost
{
    public MarkTextHost(MarkView owner, IReadOnlyMarkBlock source)
        : base(owner, source)
    {
        const string NSpace = $"{nameof(MarkTextHost)}.{nameof(MarkTextHost)}";
        ConditionalDebug.WriteLine(NSpace, $"Constructor: {source.Kind}");
        ConditionalDebug.ThrowIfEqual(BlockKind.Rule, Kind);

        CrossText = new CrossTextBlock(Owner.Tracker, null); // <- null intentional (parent has menu)

        Control = CrossText;
        TrackKey0 = CrossText.TrackKey;
        TrackKey1 = TrackKey0;

        ConditionalDebug.ThrowIfZero(CrossText.TrackKey);
        CrossText.Background = Brushes.Transparent; // <- for selectable text
        CrossText.TextWrapping = TextWrapping.Wrap;
        CrossText.VerticalAlignment = VerticalAlignment.Top;

        if (!source.Kind.IsCode(false))
        {
            CrossText.LinkClick += owner.LinkClickHandler;
        }
    }

    public CrossTextBlock CrossText { get; }

    public override void RefreshLook(bool isFirst, bool isLast)
    {
        ConditionalDebug.ThrowIfTrue(IsPending);
        SetChildMargin(isFirst, isLast);
        RefreshInternal();
        RefreshInline();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override MarkConsumed ConsumeUpdates(IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        bool pending = IsPending;
        var rslt = base.ConsumeUpdates(sequence, ref index);

        if (rslt == MarkConsumed.Changed)
        {
            if (pending)
            {
                RefreshInternal();
            }

            UpdateInline(Source.Elements);
        }

        return rslt;
    }

    /// <summary>
    /// Extends.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder(base.ToString());

        Append(buffer, $"{nameof(CrossText)}.Background", CrossText.Background);
        Append(buffer, $"{nameof(CrossText)}.TextWrapping", CrossText.TextWrapping);
        Append(buffer, $"{nameof(CrossText)}.VerticalAlignment", CrossText.VerticalAlignment);
        Append(buffer, $"{nameof(CrossText)}.FontFamily", CrossText.FontFamily);
        Append(buffer, $"{nameof(CrossText)}.FontSize", CrossText.FontSize);
        Append(buffer, $"{nameof(CrossText)}.FontWeight", CrossText.FontWeight);
        Append(buffer, $"{nameof(CrossText)}.LineHeight", CrossText.LineHeight);
        Append(buffer, $"{nameof(CrossText)}.LetterSpacing", CrossText.LetterSpacing);
        Append(buffer, $"{nameof(CrossText)}.Foreground", CrossText.Foreground);

        // First few chars only
        Append(buffer, $"{nameof(CrossText)}.Text",
            CrossText?.GetEffectiveText(WhatText.All)?.Truncate(8, TruncStyle.EndEllipses) ?? "NULL");

        return buffer.ToString();
    }

    private void RefreshInternal()
    {
        var child = CrossText;

        // Scale handles headers
        double kindScale = Kind.ToXamlFontScale();
        child.FontWeight = Kind.ToXamlFontWeight();

        child.SelectionBrush = Owner.SelectionBrush;
        child.LinkHoverBrush = Owner.LinkHoverBrush;
        child.LinkHoverUnderline = Owner.LinkHoverUnderline;

        bool hasStyle = false;
        bool hasFamily = false;
        bool hasForeground = false;
        bool hasLineHeight = false;
        bool hasLetterSpacing = false;
        double scaledSize = Owner.ScaledFontSize * kindScale;

        if (Kind.IsHeading())
        {
            hasForeground = true;
            child.SetForeground(Owner.HeadingForeground);

            hasFamily = true;
            child.FontFamily = Owner.HeadingFamily;
            child.FontSize = scaledSize * Owner.HeadingSizeCorrection;

            // Slightly smaller for this
            hasLineHeight = true;
            child.LineHeight = double.NaN;
        }
        else
        if (Kind.IsCode())
        {
            hasFamily = true;
            child.FontFamily = Owner.MonoFamily;
            child.FontSize = scaledSize * Owner.MonoSizeCorrection;

            if (Kind == BlockKind.TableCode)
            {
                // Slightly smaller for this
                hasLineHeight = true;
                child.LineHeight = double.NaN;
            }

            // Do not set letter spacing for monospace code
            hasLetterSpacing = true;
            child.LetterSpacing = 0.0;
        }
        else
        if (Source.QuoteLevel > 0)
        {
            if (Owner.QuoteItalic)
            {
                hasStyle = true;
                child.FontStyle = FontStyle.Italic;
            }

            if (Owner.QuoteFamily != null)
            {
                hasFamily = true;
                child.FontFamily = Owner.QuoteFamily;
                child.FontSize = scaledSize * Owner.QuoteSizeCorrection;
            }
        }

        if (!hasStyle)
        {
            child.FontStyle = Kind.ToXamlFontStyle();
        }

        if (!hasFamily)
        {
            child.FontFamily = Owner.FontFamily;
            child.FontSize = scaledSize * Owner.FontSizeCorrection;
        }

        if (!hasLineHeight)
        {
            child.LineHeight = Owner.ScaledLineHeight * kindScale;
        }

        if (!hasLetterSpacing)
        {
            child.LetterSpacing = Owner.ScaledLetterSpacing * kindScale;
        }

        if (!hasForeground)
        {
            child.SetForeground(Owner.Foreground);
        }
    }

    private void RefreshInline()
    {
        // Null not expected
        var inlines = CrossText.Inlines!;
        int count = inlines.Count;

        for (int n = 0; n < count; ++n)
        {
            var span = (MarkRun)inlines[n];
            span.Update(null);
        }
    }

    private void UpdateInline(IReadOnlyList<MarkElement> elements)
    {
        int count = elements.Count;

        // Null not expected
        var inlines = CrossText.Inlines!;

        inlines.Clear();

        for (int n = 0; n < count; ++n)
        {
            inlines.Add(new MarkRun(Owner, Kind, elements[n]));
        }
    }

}