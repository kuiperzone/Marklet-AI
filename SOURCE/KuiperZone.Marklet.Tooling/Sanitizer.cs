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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Sanitizes text for safe use in titles, body, group names, and chat metadata.
/// </summary>
public static class Sanitizer
{
    /// <summary>
    /// Substitution character "�".
    /// </summary>
    public const char SubChar = '\uFFFD';

    /// <summary>
    /// Sanitizes "text", normalizing, substituting or omitting characters according to "flags".
    /// </summary>
    /// <remarks>
    /// The result is truncated to "max" characters. The value may be int.MaxValue for no
    /// truncation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Max less than 0</exception>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Sanitize(string? text, SanFlags flags, int max = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(max, nameof(max));

        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        if (max == 0)
        {
            return "";
        }

        bool hasTrim = flags.HasFlag(SanFlags.Trim);

        // Pre-trim
        if (hasTrim)
        {
            text = text.Trim();

            if (text.Length == 0)
            {
                return "";
            }
        }

        // Sanitize before Normalize()
        text = PreSanitize(text, flags, max, out bool hasCombining);

        // Intentional: first allow composition (NFC) to create natural precomposed chars (é, ñ…)
        // Then strip any remaining combining marks that couldn't be composed
        if (hasCombining && flags.HasFlag(SanFlags.NormC))
        {
            // This should not throw.
            // We have stripped out "other" characters and invalid surrogate pairs.
            text = text.Normalize(NormalizationForm.FormC);
        }

        if (hasCombining && flags.HasFlag(SanFlags.NoCombining))
        {
            int n0 = 0;
            int length = text.Length;
            StringBuilder? buffer = null;

            for (int n = 0; n < length; ++n)
            {
                if (IsCombining(text[n]))
                {
                    // Omit
                    buffer ??= new(length);
                    buffer.Append(text.AsSpan(n0, n - n0));
                    n0 = n + 1;
                    continue;
                }
            }

            if (buffer != null)
            {
                if (n0 < text.Length)
                {
                    buffer.Append(text.AsSpan(n0));
                }

                text = buffer.ToString();
            }
        }

        if (text.Length > max)
        {
            text = text.Substring(0, max);

            if (hasTrim)
            {
                text = text.Trim();
            }
        }

        return text;
    }

    /// <summary>
    /// Throws if "str" exceeds "max" characters, otherwise returns the string.
    /// </summary>
    /// <exception cref="ArgumentException">Exceeds maximum length</exception>
    [return: NotNullIfNotNull(nameof(str))]
    public static string? AssertLength(string? str, int max, string? param)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(max, nameof(max));

        if (str != null && str.Length > max)
        {
            throw new ArgumentException($"{param ?? "String"} exceeds maximum length of {max}", param);
        }

        return str;
    }

    /// <summary>
    /// Ensures "value" is safe to go into a log or debug record.
    /// </summary>
    /// <remarks>
    /// Control, combining, surrogate and non-printing characters, including new-lines, are substituted with hex-codes.
    /// If "showNullOrEmpty" is true, null and empty strings return "{null}" and "{empty}" respectively. The result of
    /// "value" is otherwise truncated to "max" characters and surrounded by backtick '`' characters if "ticks" is true.
    /// The return value may, therefore, exceed "max" by 2 characters.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Max less than 0</exception>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToDebugSafe(string? value, bool showNullOrEmpty = true, bool ticks = false, int max = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(max, nameof(max));

        if (showNullOrEmpty)
        {
            if (value == null)
            {
                return "{null}";
            }

            if (value.Length == 0)
            {
                return "{empty}";
            }
        }
        else
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (max == 0)
        {
            return value;
        }

        int n0 = 0;
        StringBuilder? buffer = null;

        // Limit scan length, but + 1 to force truncation
        int scanLen = ToScanLength(value.Length, max);

        for (int n = 0; n < scanLen; ++n)
        {
            char c = value[n];
            switch (CharUnicodeInfo.GetUnicodeCategory(c))
            {
                case UnicodeCategory.Control:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.EnclosingMark:
                case UnicodeCategory.Format:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.OtherNotAssigned:
                    buffer ??= new(scanLen);
                    buffer.Append(value.AsSpan(n0, n - n0));

                    if (c == '\0')
                    {
                        buffer.Append("\\0");
                    }
                    else
                    if (c == '\t')
                    {
                        buffer.Append("\\t");
                    }
                    else
                    if (c == '\n')
                    {
                        buffer.Append("\\n");
                    }
                    else
                    if (c == '\r')
                    {
                        buffer.Append("\\r");
                    }
                    else
                    if (c < '\u0080')
                    {
                        buffer.Append("\\x");
                        buffer.Append(((byte)c).ToString("X2"));
                    }
                    else
                    {
                        buffer.Append("\\u");
                        buffer.Append(((int)c).ToString("X4"));
                    }

                    n0 = n + 1;
                    continue;
                default:
                    continue;
            }
        }

        if (buffer != null)
        {
            if (n0 < value.Length)
            {
                buffer.Append(value.AsSpan(n0));
            }

            value = buffer.ToString();
        }

        value = value.Truncate(max, TruncStyle.EndEllipses);

        if (ticks)
        {
            return string.Concat("`", value, "`");
        }

        return value;
    }

    /// <summary>
    /// Overload: ToDebugSafe(value, true, false, max)
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Max less than 0</exception>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToDebugSafe(string? value, int max)
    {
        return ToDebugSafe(value, true, false, max);
    }

    /// <summary>
    /// Overload: ToDebugSafe(value, showNullOrEmpty, false, max)
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Max less than 0</exception>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToDebugSafe(string? value, bool showNullOrEmpty, int max)
    {
        return ToDebugSafe(value, showNullOrEmpty, false, max);
    }

    private static string PreSanitize(string text, SanFlags flags, int max, out bool hasCombining)
    {
        // Performs basic sanitization by:
        // A. Remove or substitute all ASCII controls, except \n and optionally TAB
        // B. Remove invalid surrogate pairs
        // C. Optionally substituting surrogate pairs, thus given UCS-2 output
        hasCombining = false;

        bool subFeed = flags.HasFlag(SanFlags.SubFeed);
        bool subSpace = flags.HasFlag(SanFlags.SubSpace);
        bool subControl = flags.HasFlag(SanFlags.SubControl);

        int n0 = 0;
        bool prevHigh = false;
        StringBuilder? buffer = null;
        int scanLen = ToScanLength(text.Length, max);

        for (int n = 0; n < scanLen; ++n)
        {
            var c0 = text[n];

            if (c0 > 0xFF)
            {
                // UNICODE
                var cat = CharUnicodeInfo.GetUnicodeCategory(c0);

                if (!hasCombining)
                {
                    hasCombining = IsCombining(cat);
                }

                switch (cat)
                {
                    case UnicodeCategory.Control:
                        if (subControl)
                        {
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            n0 = n + 1;
                        }
                        prevHigh = false;
                        continue;
                    case UnicodeCategory.SpaceSeparator:
                        if (subSpace && c0 != ' ')
                        {
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            buffer.Append(' ');
                            n0 = n + 1;
                        }
                        prevHigh = false;
                        continue;
                    case UnicodeCategory.Surrogate:
                        if (char.IsHighSurrogate(c0))
                        {
                            prevHigh = true;
                            var c1 = n < scanLen - 1 ? text[n + 1] : '\0';

                            if (n == scanLen - 1 || !char.IsLowSurrogate(c1))
                            {
                                buffer ??= new(scanLen);
                                buffer.Append(text.AsSpan(n0, n - n0));

                                if (c1 != '\0' && char.IsLowSurrogate(c1))
                                {
                                    // Skip next
                                    // Valid high-low
                                    // n + 1 is valid here
                                    n += 1;
                                    prevHigh = false;
                                }

                                n0 = n + 1;
                            }

                            continue;
                        }

                        // Must be low surrogate to get here
                        if (!prevHigh)
                        {
                            // Invalid low
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            n0 = n + 1;
                        }

                        prevHigh = false;
                        continue;
                    case UnicodeCategory.OtherNotAssigned:
                        buffer ??= new(scanLen);
                        buffer.Append(text.AsSpan(n0, n - n0));
                        n0 = n + 1;
                        prevHigh = false;
                        continue;
                    default:
                        if (subFeed && c0 == '\u2029')
                        {
                            // Paragraph separator (double)
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            buffer.Append("\n\n");
                            n0 = n + 1;
                        }
                        prevHigh = false;
                        continue;
                }
            }


            // Not unicode, so cannot be high surrogate
            prevHigh = false;

            if (c0 > '\x7F')
            {
                if (subFeed && c0 == '\u0085')
                {
                    // NEL only
                    buffer ??= new(scanLen);
                    buffer.Append(text.AsSpan(n0, n - n0));
                    buffer.Append('\n');
                    n0 = n + 1;
                }
                else
                if (subControl && c0 < '\xA0')
                {
                    // LATIN-1 CONTROLS
                    buffer ??= new(scanLen);
                    buffer.Append(text.AsSpan(n0, n - n0));

                    if (c0 == '\u0085')
                    {
                        // NEL
                        buffer.Append('\n');
                    }

                    n0 = n + 1;
                }

                continue;
            }

            // ASCII
            if ((c0 < ' ' && c0 != '\n' && c0 != '\t') || c0 == '\x7F')
            {
                switch (c0)
                {
                    case '\0':
                        buffer ??= new(scanLen);
                        buffer.Append(text.AsSpan(n0, n - n0));
                        buffer.Append(SubChar);
                        n0 = n + 1;
                        continue;
                    case '\r':
                        buffer ??= new(scanLen);
                        buffer.Append(text.AsSpan(n0, n - n0));
                        buffer.Append('\n');

                        if (n < scanLen - 1 && text[n + 1] == '\n')
                        {
                            // Skip windows \r\n
                            n += 1;
                        }

                        n0 = n + 1;
                        continue;
                    case '\u000B':
                        if (subFeed)
                        {
                            // Vertical tab - treat as linebreak
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            buffer.Append('\n');
                            n0 = n + 1;
                        }
                        continue;
                    case '\u000C':
                        if (subFeed)
                        {
                            // Formfeed - double break
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            buffer.Append("\n\n");
                            n0 = n + 1;
                        }
                        continue;
                    default:
                        if (subControl)
                        {
                            // Omit
                            buffer ??= new(scanLen);
                            buffer.Append(text.AsSpan(n0, n - n0));
                            buffer.Append(GetControlPictograph(c0));
                            n0 = n + 1;
                        }
                        continue;
                }
            }
        }

        if (buffer != null)
        {
            if (n0 < text.Length)
            {
                buffer.Append(text.AsSpan(n0));
            }

            return buffer.ToString();
        }

        return text;
    }

    private static int ToScanLength(int len, int max)
    {
        // Enough buffer for worst-case expansion from formfeed/paragraph sep
        const int Extra = 64;

        // Avoid signed integer overflow
        if (max > int.MaxValue - Extra)
        {
            return len;
        }

        return Math.Min(len, max + Extra);
    }

    private static bool IsCombining(char c)
    {
        var cat = CharUnicodeInfo.GetUnicodeCategory(c);
        return cat == UnicodeCategory.SpacingCombiningMark || cat == UnicodeCategory.NonSpacingMark || cat == UnicodeCategory.EnclosingMark;
    }

    private static bool IsCombining(UnicodeCategory cat)
    {
        return cat == UnicodeCategory.SpacingCombiningMark || cat == UnicodeCategory.NonSpacingMark || cat == UnicodeCategory.EnclosingMark;
    }

    private static char GetControlPictograph(char c)
    {
        switch (c)
        {
            case '\u0000': return '\u2400'; // NULL
            case '\u0001': return '\u2401'; // SOH
            case '\u0002': return '\u2402'; // STX
            case '\u0003': return '\u2403'; // ETX
            case '\u0004': return '\u2404'; // EOT
            case '\u0005': return '\u2405'; // ENQ
            case '\u0006': return '\u2406'; // ACK
            case '\u0007': return '\u2407'; // BELL
            case '\u0008': return '\u2408'; // BS

            // We normally want these
            case '\u0009': return '\u2409'; // TAB
            case '\u000A': return '\u240A'; // LF
            case '\u000B': return '\u240B'; // VT
            case '\u000C': return '\u240C'; // FF
            case '\u000D': return '\u240D'; // CR

            case '\u000E': return '\u240E'; // SO
            case '\u000F': return '\u240F'; // SI
            case '\u0010': return '\u2410'; // DLE
            case '\u0011': return '\u2411'; // DC1
            case '\u0012': return '\u2412'; // DC2
            case '\u0013': return '\u2413'; // DC3
            case '\u0014': return '\u2414'; // DC4
            case '\u0015': return '\u2415'; // NAK
            case '\u0016': return '\u2416'; // SYN
            case '\u0017': return '\u2417'; // ETB
            case '\u0018': return '\u2418'; // CAN
            case '\u0019': return '\u2419'; // EM
            case '\u001A': return '\u241A'; // SUB
            case '\u001B': return '\u241B'; // ESC
            case '\u001C': return '\u241C'; // FS
            case '\u001D': return '\u241D'; // GS
            case '\u001E': return '\u241E'; // RS
            case '\u001F': return '\u241F'; // US

            case '\u007F': return '\u2421'; // DEL

            default: return SubChar;
        }
    }
}
