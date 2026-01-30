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

using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Extension methods for assisting with markdown rendering.
/// </summary>
internal static partial class HelperExt
{
    private static readonly TextDecorationCollection StrikeUnderline = new(
        [new TextDecoration() { Location = TextDecorationLocation.Underline }, new TextDecoration() { Location = TextDecorationLocation.Strikethrough }]
    );

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetBackground(this Border src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Background = brush;
        }
        else
        {
            src.ClearValue(Border.BackgroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetForeground(this TextBlock src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Foreground = brush;
        }
        else
        {
            src.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetBackground(this TextBlock src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Background = brush;
        }
        else
        {
            src.ClearValue(TextBlock.BackgroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetBackground(this TextElement src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Background = brush;
        }
        else
        {
            src.ClearValue(TextElement.BackgroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetBackground(this TemplatedControl src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Background = brush;
        }
        else
        {
            src.ClearValue(TemplatedControl.BackgroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetForeground(this TextElement src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Foreground = brush;
        }
        else
        {
            src.ClearValue(TextElement.ForegroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetForeground(this TemplatedControl src, IBrush? brush)
    {
        if (brush != null)
        {
            src.Foreground = brush;
        }
        else
        {
            src.ClearValue(TemplatedControl.ForegroundProperty);
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static double ToXamlFontScale(this BlockKind src)
    {
        switch (src)
        {
            case BlockKind.H1: return 1.65;
            case BlockKind.H2: return 1.45;
            case BlockKind.H3: return 1.20;
            case BlockKind.H4: return 1.05;
            default: return 1.0;
        }
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static FontStyle ToXamlFontStyle(this BlockKind src)
    {
        return (src == BlockKind.H6) ? FontStyle.Italic : FontStyle.Normal;
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static FontWeight ToXamlFontWeight(this BlockKind src)
    {
        return (src >= BlockKind.H1 && src <= BlockKind.H4) ? FontWeight.Bold : FontWeight.Normal;
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetFontStyle(this TextElement src, InlineStyling styling)
    {
        if (styling.HasFlag(InlineStyling.Emphasis))
        {
            src.FontStyle = FontStyle.Italic;
            return;
        }

        src.ClearValue(TextElement.FontStyleProperty);
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetFontWeight(this TextElement src, InlineStyling styling)
    {
        if (styling.HasFlag(InlineStyling.Strong))
        {
            src.FontWeight = FontWeight.Bold;
            return;
        }

        src.ClearValue(TextElement.FontWeightProperty);
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static void SetBaseline(this Inline src, InlineStyling styling, double fontSize)
    {
        // At time of writing, Avalonia 11.3.4, BaselineAlignment.Superscript aligns
        // to top while Subscript aligns to bottom OK, but neither reduce the font size
        // (we do that below). But note that BaselineAlignment.Top aligns to bottom
        // (broken). No doubt, Avalonia behaviour will change in future.
        if (styling.HasFlag(InlineStyling.Sup))
        {
            src.FontSize = fontSize * 0.8;
            src.BaselineAlignment = BaselineAlignment.Superscript;
            return;
        }

        if (styling.HasFlag(InlineStyling.Sub))
        {
            src.FontSize = fontSize * 0.8;
            src.BaselineAlignment = BaselineAlignment.Subscript;
            return;
        }

        src.ClearValue(TextElement.FontSizeProperty);
        src.ClearValue(Inline.BaselineAlignmentProperty);
    }

    /// <summary>
    /// Extension method.
    /// </summary>
    public static TextDecorationCollection? ToXamlTextDecorations(this InlineStyling src)
    {
        if (src.HasFlag(InlineStyling.Underline))
        {
            if (src.HasFlag(InlineStyling.Strike))
            {
                return StrikeUnderline;
            }

            return TextDecorations.Underline;
        }

        if (src.HasFlag(InlineStyling.Strike))
        {
            return TextDecorations.Strikethrough;
        }

        return null;
    }
}