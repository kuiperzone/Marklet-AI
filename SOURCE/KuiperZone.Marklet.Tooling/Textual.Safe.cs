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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Text processing routines that do not throw if position range out of bounds.
/// </summary>
public static partial class Textual
{
    // New line function separators, including paragraph separators.
    // LF(0x0A), CR, VT(0x0B), NEL(0x85), LS(0x2028), FF(0C), PS(0x2029)
    // Small array faster than HashSet, but not by much.
    // https://www.unicode.org/standard/reports/tr13/tr13-5.html
    private static readonly char[] LineTerms = ['\n', '\r', '\x0B', '\u0085', '\u2028', '\x0C', '\u2029'];

    /// <summary>
    /// Safely returns whether character at "index" is a space, or returns "def".
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range.
    /// </remarks>
    public static bool IsSpaceAt(this string src, int index, bool def)
    {
        if (index > -1 && index < src.Length)
        {
            if (src[index] == '\n')
            {
                return def;
            }

            return CharUnicodeInfo.GetUnicodeCategory(src[index]) == UnicodeCategory.SpaceSeparator;
        }

        return def;
    }

    /// <summary>
    /// Safely returns whether character at "index" is a space or tab, or returns "def".
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range.
    /// </remarks>
    public static bool IsSpaceOrTabAt(this string src, int index, bool def)
    {
        if (index > -1 && index < src.Length)
        {
            if (src[index] == '\n')
            {
                return def;
            }

            return CharUnicodeInfo.GetUnicodeCategory(src[index]) == UnicodeCategory.SpaceSeparator || src[index] == '\t';
        }

        return def;
    }

    /// <summary>
    /// Safely returns whether character at "index" equals "c".
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range but returns false instead.
    /// </remarks>
    public static bool IsCharAt(this string src, char c, int index)
    {
        // Safe: Returns false if out of range
        return index > -1 && index < src.Length && src[index] == c;
    }

    /// <summary>
    /// Returns whether character at "index" is preceded by an escpaping "\" character.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range but returns false instead.
    /// </remarks>
    public static bool IsEscapedAt(this string src, int index)
    {
        // Safe: Returns false if out of range
        int lc = 0;
        while (IsCharAt(src, '\\', --index)) lc += 1;
        return lc % 2 == 1;
    }

    /// <summary>
    /// Returns whether character at "index" is used to terminate a line.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range but returns "def" instead. Includes
    /// LF, CR, VT, NEL, LINE SEP, and PARA SEP.
    /// </remarks>
    public static bool IsLineTermAt(this string src, int index, bool def)
    {
        if (index > -1 && index < src.Length)
        {
            return LineTerms.Contains(src[index]);
        }

        return def;
    }

    /// <summary>
    /// Returns first index of any known line terminator character, or -1 if none.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "startIndex" is out of range but returns -1 instead.
    /// </remarks>
    public static int IndexOfLineTerm(this string src, int startIndex = 0)
    {
        if (startIndex > -1 && startIndex < src.Length)
        {
            return src.IndexOfAny(LineTerms, startIndex);
        }

        return -1;
    }

    /// <summary>
    /// Returns whether character at "index" is used to terminate the end of a sentence, including colon ":" characters.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range but returns false instead. A result
    /// of true does not guarantee that the end of a sentence. It only indicates a character used to terminate
    /// sentences.
    /// </remarks>
    public static bool IsSentenceTermAt(this string src, int index, bool def)
    {
        if (index > -1 && index < src.Length)
        {
            // These are RTL terminators and were removed.
            // case '\u06D4':
            // case '\u061F':
            // case '\u0589':
            // case '\u055E':
            switch (src[index])
            {
                case '.':
                case '!':
                case '?':

                case ':':
                case '\uFF1A': return true;

                case '\u037E': // greek question mark
                case '\u2026': // ellipses …
                case '\u0964':
                case '\u0965':
                case '\u3002':
                case '\uFF1F': // wide question mark
                case '\uFF01':
                case '\u055C': return true;
                default: return false;
            }
        }

        return def;
    }

    /// <summary>
    /// Returns whether character at "index" is an open quote.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range but returns false instead.
    /// </remarks>
    public static bool IsOpenQuoteAt(this string src, int index, bool def)
    {
        if (index > -1 && index < src.Length)
        {
            switch (src[index])
            {
                case '"':
                case '\'':
                case '\u2018': // single quote
                case '\u201C': // double quote
                case '«':
                case '\u2039': // single arrow quote
                    return true;
                default:
                    return false;
            }
        }

        return def;
    }

    /// <summary>
    /// Returns whether character at "index" is a close quote.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension and does not throw if "index" is out of range but returns false instead.
    /// </remarks>
    public static bool IsCloseQuoteAt(this string src, int index, bool def)
    {
        if (index > -1 && index < src.Length)
        {
            switch (src[index])
            {
                case '"':
                case '\'':
                case '\u2019': // single quote
                case '\u201D': // double quote
                case '»':
                case '\u203A': // single arrow quote
                    return true;
                default:
                    return false;
            }
        }

        return def;
    }

    /// <summary>
    /// Returns whether the string contains "sub" at "index".
    /// </summary>
    /// <remarks>
    /// Serves as a string extension. It does not throw if "index" is out of range but returns false instead. It always
    /// returns false if "sub" is empty.
    /// </remarks>
    public static bool ContainsAt(this string src, string sub, int index, StringComparison comp = StringComparison.Ordinal)
    {
        int subLen = sub.Length;

        if (index > -1 && src.Length - index >= subLen)
        {
            if (subLen == 0)
            {
                return false;
            }

            if (comp == StringComparison.Ordinal)
            {
                for (int n = 0; n < subLen; ++n)
                {
                    if (src[index + n] != sub[n])
                    {
                        return false;
                    }
                }

                return true;
            }

            return string.Compare(src, index, sub, 0, subLen, comp) == 0;
        }

        return false;
    }

}
