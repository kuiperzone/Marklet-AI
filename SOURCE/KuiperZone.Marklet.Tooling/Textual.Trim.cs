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
/// Text processing routines associated with trimming and truncation of text.
/// </summary>
public static partial class Textual
{
    /// <summary>
    /// Trims the string at the start of space and tab characters, but does not trim new lines.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension.
    /// </remarks>
    public static string TrimSpaceStart(this string src)
    {
        for (int n = 0; n < src.Length; ++n)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(src[n]) != UnicodeCategory.SpaceSeparator && src[n] != '\t')
            {
                if (n != 0)
                {
                    return src.Substring(n);
                }

                return src;
            }
        }

        return "";
    }

    /// <summary>
    /// Trims the string at the end of space and tab characters, but does not trim new lines.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension.
    /// </remarks>
    public static string TrimSpaceEnd(this string src)
    {
        for (int n = src.Length - 1; n > -1; --n)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(src[n]) != UnicodeCategory.SpaceSeparator && src[n] != '\t')
            {
                if (n != src.Length - 1)
                {
                    return src.Substring(0, n + 1);
                }

                return src;
            }
        }

        return "";
    }

    /// <summary>
    /// Trims the string at the start and end of space and tab characters, but does not trim new lines.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension.
    /// </remarks>
    public static string TrimSpace(this string src)
    {
        return TrimSpaceStart(TrimSpaceEnd(src));
    }

    /// <summary>
    /// Trims consecutive spaces and/or newlines anywhere within the string.
    /// </summary>
    /// <remarks>
    /// Newlines are replaced with a single space where "preseveLines" is false. However, if "trimSides" is true, the
    /// string is subjected to string.Trim() before processing which will remove starting and ending newlines, even
    /// where "preserveLines" is true. By default, given: "  Hello \n  World  ", the result is "Hello World". Serves as
    /// a string extension.
    /// </remarks>
    public static string TrimPretty(this string src, bool preseveLines = false, bool trimSides = true)
    {
        if (trimSides)
        {
            src = src.Trim();
        }

        if (src.Length == 0)
        {
            return src;
        }

        int posN = 0;
        int n0 = 0;
        StringBuilder? buffer = null;

        while (posN < src.Length)
        {
            if (IsSpaceTabOrFeed(src, posN, preseveLines))
            {
                // Skip multiple spaces and optionally newlines
                int ton = posN;
                while (IsSpaceTabOrFeed(src, ++ton, preseveLines)) ;

                if (ton > posN + 1 || (!preseveLines && ton == posN + 1 && IsLineTermAt(src, posN, false)))
                {
                    buffer ??= new(src.Length);
                    buffer.Append(src.AsSpan(n0, posN - n0));
                    buffer.Append(' ');
                    n0 = ton;
                    posN = ton;
                    continue;
                }
            }

            posN += 1;
        }

        if (buffer != null)
        {
            if (n0 < src.Length)
            {
                buffer.Append(src.AsSpan(n0));
            }

            return buffer.ToString();
        }

        return src;
    }

    /// <summary>
    /// Trims and truncates the string at "maxLength" and the first newline character.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Less than 0</exception>
    public static string? TrimTitle(this string src, int maxLength = 64)
    {
        if (!string.IsNullOrEmpty(src))
        {
            src = Truncate(src.Trim(), maxLength);

            int p = src.IndexOf('\n');

            if (p > -1)
            {
                src = src.Substring(0, p);
            }

            return src.TrimEnd();
        }

        return src;
    }


    /// <summary>
    /// Truncates the string simply if it exceeds "maxLength" characters.
    /// </summary>
    /// <remarks>
    /// Equivalent to <see cref="Truncate(string, int, TruncStyle, bool)"/> with <see cref="TruncStyle.End"/>.
    /// Serves as a string extension.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Less than 0</exception>
    public static string Truncate(this string src, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength, nameof(maxLength));

        if (src.Length > maxLength)
        {
            return TrimSurrogate(src.Substring(0, maxLength));
        }

        return src;
    }

    /// <summary>
    /// Truncates the string if it exceeds "maxLength" characters according to the <see cref="TruncStyle"/> specified.
    /// </summary>
    /// <remarks>
    /// Unicode ellipses "…" are used over "..." where "unicode" is true. No account is made of new line separators.
    /// Serves as a string extension.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Less than 0</exception>
    public static string Truncate(this string src, int maxLength, TruncStyle style, bool unicode = true)
    {
        const string Ellipses = "\u2026";
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength, nameof(maxLength));

        if (src.Length > maxLength)
        {
            string ellipses = unicode ? Ellipses : "...";

            if (style == TruncStyle.CenterEllipses)
            {
                if (maxLength > ellipses.Length + 1)
                {
                    // 01234567   -> 0...7 [5], or 01…67
                    // 01234567   -> 01...7 [6], or 012…67

                    // 012345678  -> 01...78 [7], or 012…678
                    // 0123456789 -> 01...89 [7], or 012…789
                    // 0123456789 -> 012...89 [8], or 0123…789
                    // 0123456789 -> 012...789 [9], or 0123…6789
                    int x0 = maxLength / 2 - (unicode ? 0 : 1);
                    int x1 = src.Length - x0 + (maxLength % 2 == 0 ? 1 : 0);
                    return TrimSurrogate(string.Concat(src.AsSpan(0, x0), ellipses, src.AsSpan(x1)));
                }

                style = TruncStyle.EndEllipses;
            }

            if (style == TruncStyle.EndEllipses)
            {
                // Debatable whether this should check for
                // "greater than" or "greater than or equals".
                // With ">", at len of 1, it will return
                // just a single char which can be misleading
                // as no truncation indicated. With ">=", it
                // returns only "…" which gives no info other
                // it has been truncated.
                if (maxLength >= ellipses.Length)
                {
                    // len=6, max=5
                    // 012345  -> 01...
                    return TrimSurrogate(string.Concat(src.AsSpan(0, maxLength - ellipses.Length), ellipses));
                }

                style = TruncStyle.End;
            }
            else
            if (style == TruncStyle.StartEllipses)
            {
                // See note above.
                if (maxLength >= ellipses.Length)
                {
                    // len=6, max=5
                    // 012345  -> ...45
                    return TrimSurrogate(string.Concat(ellipses, src.AsSpan(src.Length - maxLength + ellipses.Length)));
                }

                style = TruncStyle.Start;
            }

            if (style == TruncStyle.Start)
            {
                // len=6, max=5
                // 012345  -> 12345
                return TrimSurrogate(src.Substring(src.Length - maxLength));
            }

            // len=6, max=5
            // 012345  -> 01234
            return TrimSurrogate(src.Substring(0, maxLength));
        }

        return src;
    }

    private static bool IsSpaceTabOrFeed(string src, int index, bool preserveLines)
    {
        return IsSpaceOrTabAt(src, index, false) || (!preserveLines && IsLineTermAt(src, index, false));
    }

    private static string TrimSurrogate(string src)
    {
        if (src.Length != 0 && char.IsHighSurrogate(src[^1]))
        {
            return src.Substring(0, src.Length - 1);
        }

        return src;
    }
}