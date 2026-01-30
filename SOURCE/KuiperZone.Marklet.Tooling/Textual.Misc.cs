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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Miscellaneous text routines that serve as a string extension.
/// </summary>
public static partial class Textual
{
    /// <summary>
    /// The maximum length returned by <see cref="GetFriendlyNameOf"/>.
    /// </summary>
    public const int DefaultMaxFriendlyName = 48;

    private static readonly Dictionary<char, Tuple<char, char>> SuperSub;

    static Textual()
    {
        var dict = new Dictionary<char, Tuple<char, char>>(96);
        SuperSub = dict;

        dict.Add('0', Tuple.Create('⁰', '₀'));
        dict.Add('1', Tuple.Create('¹', '₁'));
        dict.Add('2', Tuple.Create('²', '₂'));
        dict.Add('3', Tuple.Create('³', '₃'));
        dict.Add('4', Tuple.Create('⁴', '₄'));
        dict.Add('5', Tuple.Create('⁵', '₅'));
        dict.Add('6', Tuple.Create('⁶', '₆'));
        dict.Add('7', Tuple.Create('⁷', '₇'));
        dict.Add('8', Tuple.Create('⁸', '₈'));
        dict.Add('9', Tuple.Create('⁹', '₉'));

        dict.Add('A', Tuple.Create('ᴬ', '?'));
        dict.Add('B', Tuple.Create('ᴮ', '?'));
        dict.Add('C', Tuple.Create('?', '?'));
        dict.Add('D', Tuple.Create('ᴰ', '?'));
        dict.Add('E', Tuple.Create('ᴱ', '?'));
        dict.Add('F', Tuple.Create('?', '?'));
        dict.Add('G', Tuple.Create('ᴳ', '?'));
        dict.Add('H', Tuple.Create('ᴴ', '?'));
        dict.Add('I', Tuple.Create('ᴵ', '?'));
        dict.Add('J', Tuple.Create('ᴶ', '?'));
        dict.Add('K', Tuple.Create('ᴷ', '?'));
        dict.Add('L', Tuple.Create('ᴸ', '?'));
        dict.Add('M', Tuple.Create('ᴹ', '?'));
        dict.Add('N', Tuple.Create('ᴺ', '?'));
        dict.Add('O', Tuple.Create('ᴼ', '?'));
        dict.Add('P', Tuple.Create('ᴾ', '?'));
        dict.Add('Q', Tuple.Create('?', '?'));
        dict.Add('R', Tuple.Create('ᴿ', '?'));
        dict.Add('S', Tuple.Create('?', '?'));
        dict.Add('T', Tuple.Create('ᵀ', '?'));
        dict.Add('U', Tuple.Create('ᵁ', '?'));
        dict.Add('V', Tuple.Create('ⱽ', '?'));
        dict.Add('W', Tuple.Create('ᵂ', '?'));
        dict.Add('X', Tuple.Create('?', '?'));
        dict.Add('Y', Tuple.Create('?', '?'));
        dict.Add('Z', Tuple.Create('?', '?'));

        dict.Add('a', Tuple.Create('ᵃ', 'ₐ'));
        dict.Add('b', Tuple.Create('ᵇ', '?'));
        dict.Add('c', Tuple.Create('ᶜ', '?'));
        dict.Add('d', Tuple.Create('ᵈ', '?'));
        dict.Add('e', Tuple.Create('ᵉ', 'ₑ'));
        dict.Add('f', Tuple.Create('ᶠ', '?'));
        dict.Add('g', Tuple.Create('ᵍ', '?'));
        dict.Add('h', Tuple.Create('ʰ', 'ₕ'));
        dict.Add('i', Tuple.Create('ⁱ', 'ᵢ'));
        dict.Add('j', Tuple.Create('ʲ', 'ⱼ'));
        dict.Add('k', Tuple.Create('ᵏ', 'ₖ'));
        dict.Add('l', Tuple.Create('ˡ', 'ₗ'));
        dict.Add('m', Tuple.Create('ᵐ', 'ₘ'));
        dict.Add('n', Tuple.Create('ⁿ', 'ₙ'));
        dict.Add('o', Tuple.Create('ᵒ', 'ₒ'));
        dict.Add('p', Tuple.Create('ᵖ', 'ₚ'));
        dict.Add('q', Tuple.Create('?', '?'));
        dict.Add('r', Tuple.Create('ʳ', 'ᵣ'));
        dict.Add('s', Tuple.Create('ˢ', 'ₛ'));
        dict.Add('t', Tuple.Create('ᵗ', 'ₜ'));
        dict.Add('u', Tuple.Create('ᵘ', 'ᵤ'));
        dict.Add('v', Tuple.Create('ᵛ', 'ᵥ'));
        dict.Add('w', Tuple.Create('ʷ', '?'));
        dict.Add('x', Tuple.Create('ˣ', 'ₓ'));
        dict.Add('y', Tuple.Create('ʸ', '?'));
        dict.Add('z', Tuple.Create('ᶻ', '?'));

        dict.Add('+', Tuple.Create('⁺', '₊'));
        dict.Add('-', Tuple.Create('⁻', '₋'));
        dict.Add('=', Tuple.Create('⁼', '₌'));
        dict.Add('(', Tuple.Create('⁽', '₍'));
        dict.Add(')', Tuple.Create('⁾', '₎'));
    }

    /// <summary>
    /// Gets a friendly string for a type or value name by inserting spaces where case changes in the string.
    /// </summary>
    /// <remarks>
    /// Intended for use by the user interface where the input string is expected to be the result of nameof(). For
    /// example, converts "ErrorReport" to "Error Report" and "maxLength" to "Max Length". The result is truncated if
    /// the string exceeds "maxLength" characters.
    /// </remarks>
    /// <exception cref="ArgumentException">Too long</exception>
    public static string GetFriendlyNameOf(this string src, int maxLength = DefaultMaxFriendlyName)
    {
        if (src.Length > maxLength)
        {
            src = src.Substring(0, maxLength);
        }

        int len0 = src.Length;
        var sb = new StringBuilder(len0 + 2);

        // GetValue => get-value
        // IsOK => is-ok
        int p0 = 0;
        int p1 = 0;
        int len1 = 0;

        for (int n = 0; n < len0; ++n)
        {
            var c = src[n];

            if (char.IsAsciiLetterUpper(c))
            {
                if (n - 1 > p1)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(src.AsSpan(p0, n - p0));
                    p0 = n;
                    len1 = n + 1;
                }

                p1 = n;
                continue;
            }

            if (!char.IsAsciiLetterOrDigit(c))
            {
                return SplitFinal(src.Trim());
            }
        }

        if (sb.Length != 0)
        {
            if (len1 <= len0)
            {
                sb.Append(' ');
                sb.Append(src.AsSpan(p0));
            }

            return SplitFinal(sb.ToString());
        }

        return SplitFinal(src);
    }

    /// <summary>
    /// Returns the length in graphemes (user perceived characters).
    /// </summary>
    /// <remarks>
    /// Serves as a string extension. Tab characters have a width of 1.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">startIndex, or length</exception>
    public static int GetVisualLength(this string text, int startIndex = 0, int length = 1)
    {
        if (length < 0)
        {
            length = text.Length - startIndex;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex + length, text.Length);

        if (length == 0)
        {
            return 0;
        }

        int width = 0;
        int elemCount = 0;
        var enumerator = StringInfo.GetTextElementEnumerator(text, startIndex);

        while (enumerator.MoveNext())
        {
            var e = enumerator.GetTextElement();
            width += GetElementDisplayWidth(e);

            elemCount += e.Length;

            if (elemCount >= length)
            {
                return width;
            }
        }

        return width;
    }

    /// <summary>
    /// Where possible, maps text to superscript unicode characters. Numeric digits '0' to '9', and the characters '+',
    /// '-', '=', '(', ')' are supported.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension. Support for latin letter characters is sketchy, with many characters unmappable.
    /// </remarks>
    public static string ToSuperscript(this string src)
    {
        return ToSuperSub(src, true);
    }

    /// <summary>
    /// Where possible, maps text to subscript unicode characters. Numeric digits '0' to '9', and
    /// the characters '+', '-', '=', '(', ')' are supported.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension. Support for latin letter characters is sketchy, with many characters unmappable.
    /// </remarks>
    public static string ToSubscript(this string src)
    {
        return ToSuperSub(src, false);
    }

    private static string SplitFinal(string s)
    {
        if (s.Length == 0)
        {
            return "";
        }

        var c = s[0];

        if (char.IsAsciiLetterLower(c))
        {
            return Truncate(char.ToUpperInvariant(c) + s.Substring(1), DefaultMaxFriendlyName, TruncStyle.EndEllipses);
        }

        return Truncate(s, DefaultMaxFriendlyName, TruncStyle.EndEllipses);
    }

    private static int GetElementDisplayWidth(string element)
    {
        if (element.Length == 0)
        {
            return 0;
        }

        var category = CharUnicodeInfo.GetUnicodeCategory(element, 0);

        if (category == UnicodeCategory.Format)
        {
            return 0;
        }

        // All Unicode format characters (Cf) are zero-width
        int codePoint = char.ConvertToUtf32(element, 0);

        // Control characters except TAB, CR, LF
        if (category == UnicodeCategory.Control && codePoint != 0x09 && codePoint != 0x0A && codePoint != 0x0D)
        {
            return 0;
        }

        // Soft Hyphen (U+00AD) and deprecated Mongolian Vowel Separator (U+180E)
        if (codePoint == 0x00AD || codePoint == 0x180E)
        {
            return 0;
        }

        if ((codePoint >= 0x1100 && codePoint <= 0x115F) || // Hangul Jamo
            (codePoint >= 0x2329 && codePoint <= 0x232A) || // Angle brackets
            (codePoint >= 0x2E80 && codePoint <= 0xA4CF) || // CJK, Yi
            (codePoint >= 0xAC00 && codePoint <= 0xD7A3) || // Hangul Syllables
            (codePoint >= 0xF900 && codePoint <= 0xFAFF) || // CJK Compatibility Ideographs
            (codePoint >= 0xFE10 && codePoint <= 0xFE19) || // Vertical punctuation
            (codePoint >= 0xFE30 && codePoint <= 0xFE6F) || // CJK Compatibility Forms
            (codePoint >= 0xFF00 && codePoint <= 0xFF60) || // Fullwidth ASCII variants
            (codePoint >= 0xFFE0 && codePoint <= 0xFFE6) || // Fullwidth symbols
            (codePoint >= 0x1F300 && codePoint <= 0x1FAFF)) // Emoji
        {
            return 2;
        }

        return 1;
    }

    private static string ToSuperSub(string text, bool super)
    {
        // https://rupertshepherd.info/resource_pages/superscript-letters-in-unicode
        // https://stackoverflow.com/questions/17908593/how-to-find-the-unicode-of-the-subscript-alphabet
        if (text.Length == 0)
        {
            return text;
        }

        int count = text.Length;
        var sb = new StringBuilder(count);

        for (int n = 0; n < count; ++n)
        {
            char c = text[n];

            if (SuperSub.TryGetValue(c, out Tuple<char, char>? tuple))
            {
                var x = super ? tuple.Item1 : tuple.Item2;

                if (x != '?')
                {
                    sb.Append(x);
                    continue;
                }
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

}
