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

using System.Text;
using Avalonia.Layout;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Text block.
/// </summary>
internal class MarkTextHost : MarkBlockHost
{
    public MarkTextHost(MarkShim shim, IReadOnlyMarkBlock source)
        : base(shim, source)
    {
        const string NSpace = $"{nameof(MarkTextHost)}.{nameof(MarkTextHost)}";

        ConditionalDebug.WriteLine(NSpace, $"Constructor: {source.Kind}");
        ConditionalDebug.ThrowIfEqual(BlockKind.Rule, Kind);

        CrossText = new CrossTextBlock(null); // <- null intentional (parent has menu)
        CrossText.Tracker = shim.Owner.Tracker;
        Control = CrossText;
        Track0 = CrossText;
        Track1 = CrossText;

        CrossText.Background = Brushes.Transparent; // <- for selectable text
        CrossText.TextWrapping = TextWrapping.Wrap;
        CrossText.VerticalAlignment = VerticalAlignment.Top;

        ConditionalDebug.WriteLine(NSpace, $"Done ok");
    }

    public CrossTextBlock CrossText { get; }

    public override void Refresh(bool isFirst, bool isLast)
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
        const string NSpace = $"{nameof(MarkTextHost)}.{nameof(ConsumeUpdates)}";

        bool pending = IsPending;
        var rslt = base.ConsumeUpdates(sequence, ref index);
        ConditionalDebug.WriteLine(NSpace, "Consuming " + index);

        if (rslt == MarkConsumed.Changed)
        {
            ConditionalDebug.WriteLine(NSpace, "Changed");

            if (pending)
            {
                RefreshInternal();
            }

            UpdateInline(Source.Elements);
        }

        ConditionalDebug.WriteLine(NSpace, "Result: " + rslt);
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
        const string NSpace = $"{nameof(MarkTextHost)}.{nameof(RefreshInternal)}";
        ConditionalDebug.WriteLine(NSpace, "Refreshing");

        var child = CrossText;

        // Scale handles headers
        double kindScale = Kind.ToXamlFontScale();
        child.FontWeight = Kind.ToXamlFontWeight();

        var owner = Shim.Owner;
        var foreground = Shim.ActualForeground;
        child.SelectionBrush = owner.SelectionBrush;
        child.LinkHoverBrush = owner.LinkHoverBrush;
        child.LinkHoverUnderline = owner.LinkHoverUnderline;

        bool hasStyle = false;
        bool hasFamily = false;
        bool hasForeground = false;
        bool hasLineHeight = false;
        bool hasLetterSpacing = false;
        double fontSize = Shim.FontSize * kindScale;

        if (Kind.IsHeading())
        {
            ConditionalDebug.WriteLine(NSpace, "Is Heading");
            hasForeground = true;
            child.SetForeground(owner.HeadingForeground ?? foreground);

            hasFamily = true;
            child.FontFamily = owner.HeadingFamily;
            child.FontSize = fontSize * owner.HeadingSizeCorrection;

            // Slightly smaller for this
            hasLineHeight = true;
            child.LineHeight = double.NaN;
        }
        else
        if (Kind.IsCode())
        {
            ConditionalDebug.WriteLine(NSpace, "Is code");
            hasFamily = true;
            child.FontFamily = owner.MonoFamily;
            child.FontSize = fontSize * owner.MonoSizeCorrection;

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
            ConditionalDebug.WriteLine(NSpace, "Has quote level");

            if (owner.QuoteItalic)
            {
                hasStyle = true;
                child.FontStyle = FontStyle.Italic;
            }

            if (owner.QuoteFamily != null)
            {
                hasFamily = true;
                child.FontFamily = owner.QuoteFamily;
                child.FontSize = fontSize * owner.QuoteSizeCorrection;
            }
        }

        if (!hasStyle)
        {
            child.FontStyle = Kind.ToXamlFontStyle();
        }

        if (!hasFamily)
        {
            child.FontFamily = owner.FontFamily;
            child.FontSize = fontSize * owner.FontSizeCorrection;
        }

        if (!hasLineHeight)
        {
            child.LineHeight = Shim.LineHeight * kindScale;
        }

        if (!hasLetterSpacing)
        {
            child.LetterSpacing = Shim.LetterSpacing * kindScale;
        }

        if (!hasForeground)
        {
            child.SetForeground(foreground);
        }

        ConditionalDebug.WriteLine(NSpace, "Done ok");
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
        // This will clear the selection when block updated.
        // However, it was deemed unnecessary.
        // CrossText.SelectNone();

        int elemCount = elements.Count;
        var inlines = CrossText.Inlines!;

        inlines.Clear();
        inlines.Capacity = elemCount;

        for (int n = 0; n < elemCount; ++n)
        {
            inlines.Add(new MarkRun(Shim, Kind, elements[n]));
        }
    }

}