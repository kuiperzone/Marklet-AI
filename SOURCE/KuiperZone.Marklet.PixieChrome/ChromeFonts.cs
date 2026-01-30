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

using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Chrome specific font families, notably <see cref="SymbolFamily"/>. Additionally, it defines fixed font sizes.
/// </summary>
public static class ChromeFonts
{
    private const double SmallFontSizeF = 0.85;
    private const double LargeFontSizeF = 1.25;
    private const double HugeFontSizeF = 1.65;
    private const double LargeSymbolFontSizeF = 1.25;
    private const double HugeSymbolFontSizeF = 1.65;

    static ChromeFonts()
    {
        const string NSpace = $"{nameof(ChromeFonts)}.Static";
        const string AssetPrefix = "fonts:ChromeFonts";

        DefaultFamily = FontFamily.Default;
        ConditionalDebug.WriteLine(NSpace, "LOADING FONTS");

        try
        {
            var fc = new EmbeddedFontCollection(
                new Uri(AssetPrefix, UriKind.Absolute),
                new Uri("avares://KuiperZone.Marklet.PixieChrome/Assets/Fonts", UriKind.Absolute));

            FontManager.Current.AddFontCollection(fc);
            MonospaceFamily = new($"{AssetPrefix}#Source Code Pro");
            SymbolFamily = new($"{AssetPrefix}#Material Symbols");
        }
        catch (Exception e)
        {
            // Unless we catch this, unit tests that don't need fonts will fail.
            ConditionalDebug.WriteLine(NSpace, "WARNING: Font load failed");
            ConditionalDebug.WriteLine(NSpace, e);
            MonospaceFamily = FontFamily.Default;
            SymbolFamily = FontFamily.Default;
        }
    }

    /// <summary>
    /// Application default font size.
    /// </summary>
    public const double DefaultFontSize = 14.0;

    /// <summary>
    /// Gets the approximate line height for <see cref="DefaultFontSize"/>.
    /// </summary>
    public const double DefaultLineHeight = DefaultFontSize * 1.2;

    /// <summary>
    /// Gets a font size smaller than <see cref="DefaultFontSize"/>.
    /// </summary>
    public const double SmallFontSize = DefaultFontSize * SmallFontSizeF;

    /// <summary>
    /// Gets the approximate lineheight for <see cref="SmallFontSize"/>.
    /// </summary>
    public const double SmallLineHeight = DefaultLineHeight * SmallFontSizeF;

    /// <summary>
    /// Gets a font size larger than <see cref="DefaultFontSize"/>.
    /// </summary>
    public const double LargeFontSize = DefaultFontSize * LargeFontSizeF; // 17.5

    /// <summary>
    /// Gets the approximate lineheight for <see cref="LargeFontSize"/>.
    /// </summary>
    public const double LargeLineHeight = DefaultLineHeight * LargeFontSizeF;

    /// <summary>
    /// Gets a font size larger than <see cref="LargeFontSize"/>.
    /// </summary>
    public const double HugeFontSize = DefaultFontSize * HugeFontSizeF; // 23.1

    /// <summary>
    /// Gets the approximate lineheight for <see cref="HugeFontSize"/>.
    /// </summary>
    public const double HugeLineHeight = DefaultLineHeight * HugeFontSizeF;

    /// <summary>
    /// Constant default symbol font size.
    /// </summary>
    public const double SymbolFontSize = 16.0;

    /// <summary>
    /// Gets the large font size for symbols.
    /// </summary>
    public const double LargeSymbolFontSize = SymbolFontSize * LargeSymbolFontSizeF;

    /// <summary>
    /// Gets the huge font size for symbols.
    /// </summary>
    public const double HugeSymbolFontSize = SymbolFontSize * HugeSymbolFontSizeF;

    /// <summary>
    /// Gets the default application font family.
    /// </summary>
    /// <remarks>
    /// This is simply <see cref="FontFamily.Default"/>, however, it provides a common place to switch font for entire
    /// application.
    /// </remarks>
    public static readonly FontFamily DefaultFamily;

    /// <summary>
    /// Gets the  monospace font family.
    /// </summary>
    /// <remarks>
    /// This font is to be found in "Assets".
    /// </remarks>
    public static readonly FontFamily MonospaceFamily;

    /// <summary>
    /// Gets the symbol font family.
    /// </summary>
    public static readonly FontFamily SymbolFamily;

    /// <summary>
    /// Examines first few characters of text and returns either <see cref="SymbolFamily"/> if the chars include Unicode
    /// private use characters, or "def" then <see cref="DefaultFamily"/>.
    /// </summary>
    public static FontFamily GetSymbolOrDefaultFamily(string? text, FontFamily? def = null)
    {
        if (!string.IsNullOrEmpty(text))
        {
            int max = Math.Min(text.Length, 4);

            for (int n = 0; n < max; ++n)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(text[n]) == UnicodeCategory.PrivateUse)
                {
                    return SymbolFamily;
                }
            }
        }

        return def ?? DefaultFamily;
    }

    /// <summary>
    /// Returns a new Run sequence comprising symbols and text of font families <see cref="SymbolFamily"/> and <see
    /// cref="DefaultFamily"/> respectively.
    /// </summary>
    public static InlineCollection? NewTextRun(string? text, double fontSize = double.NaN, double scale = 1.0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        Run? trail = null;
        bool lastSymbol = false;
        StringBuilder buffer = new(16);
        var inlines = new InlineCollection();

        for(int n = 0; n < text.Length; ++n)
        {
            char c = text[n];
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            bool symbol = cat == UnicodeCategory.PrivateUse;

            if (symbol != lastSymbol)
            {
                if (lastSymbol && cat == UnicodeCategory.SpaceSeparator)
                {
                    // EN SPACE
                    c = '\u2002';
                }

                if (trail != null)
                {
                    trail.Text = buffer.ToString();
                    inlines.Add(trail);
                    buffer.Clear();
                }

                lastSymbol = symbol;
                trail = NewRun(symbol, fontSize, scale);
            }

            buffer.Append(c);
        }

        trail ??= NewRun(lastSymbol, fontSize, scale);
        trail.Text = buffer.ToString();
        inlines.Add(trail);
        return inlines;
    }

    /// <summary>
    /// Returns a new TextBlock containing mixed symbols and text.
    /// </summary>
    public static TextBlock NewTextBlock(string text)
    {
        var block = new TextBlock();
        block.Inlines = NewTextRun(text);
        return block;
    }

    private static Run NewRun(bool symbol, double fontSize, double fontScale)
    {
        var run = new Run();
        run.BaselineAlignment = BaselineAlignment.Center;
        run.FontFamily = symbol ? SymbolFamily : DefaultFamily;

        if (fontSize > 0.0)
        {
            run.FontSize = fontSize;
            return run;
        }

        run.FontSize = (symbol ? SymbolFontSize : DefaultFontSize) * fontScale;
        return run;
    }
}
