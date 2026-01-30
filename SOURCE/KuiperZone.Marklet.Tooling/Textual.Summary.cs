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
/// Text processing routines associated with summarising and extracting key information from text.
/// </summary>
public static partial class Textual
{
    private const int MaxSigClamp = 512;
    private const int MaxSigRecursion = 8;
    private const double MinCapWeightF = 0.6;
    private const double MaxCapWeightF = 2.0;
    private static readonly SigOptions DefaultSigOpts = new();

    /// <summary>
    /// Estimates the number of tokens using a language and model agnostic algorithm.
    /// </summary>
    /// <remarks>
    /// Serves as a string extension.
    /// </remarks>
    public static double EstimateTokens(this string src)
    {
        // Acknowledgement: Implementation of algorithm described by "stoerr" with changes. Thank you!
        // https://community.openai.com/t/how-to-do-a-quick-estimation-of-token-count-of-a-text/277764

        // C0 = 'NORabcdefghilnopqrstuvy' // plus space that is not following a space
        const double C0 = 0.2020182639633662;

        // C1 = '"#%)\*+56789<>?@Z[\\]^|§«äç\''
        const double C1 = 0.4790556468110302;

        // C2 = '-.ABDEFGIKWY_\r\tz{ü'
        const double C2 = 0.3042805747355606;

        // C3 = ',01234:~Üß' // incl. unicode characters > 255
        const double C3 = 0.6581971122770317;

        // C4 = space that is following a space
        const double C4 = 0.08086208692099685;

        // C5 = '!$&(/;=JX`j\n}ö'
        const double C5 = 0.4157646363858563;

        // C6 = 'CHLMPQSTUVfkmspwx&NBSP;'
        const double C6 = 0.2372744211422125;

        // Others
        const double CX = 0.980083857442348;

        double sum = 0.0;
        int len = src.Length;

        for (int n = 0; n < len; ++n)
        {
            var c = src[n];

            switch (c)
            {
                case 'N':
                case 'O':
                case 'R':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'l':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'y':
                    sum += C0;
                    continue;
                case '"':
                case '#':
                case '%':
                case ')':
                case '*':
                case '+':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '<':
                case '>':
                case '?':
                case '@':
                case 'Z':
                case '[':
                case '\\':
                case ']':
                case '^':
                case '|':
                case '§':
                // case '«': // removed (has no pair) will go to C3
                case 'ä':
                case 'ç':
                case '\'':
                    sum += C1;
                    continue;
                case '-':
                case '.':
                case 'A':
                case 'B':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'I':
                case 'K':
                case 'W':
                case 'Y':
                case '_':
                case '\r':
                case '\t':
                case 'z':
                case '{':
                case 'ü':
                    sum += C2;
                    continue;
                case ',':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case ':':
                case '~':
                case 'Ü':
                case 'ß':
                    sum += C3;
                    continue;
                case '!':
                case '$':
                case '&':
                case '(':
                case '/':
                case ';':
                case '=':
                case 'J':
                case 'X':
                case '`':
                case 'j':
                case '\n':
                case '}':
                case 'ö':
                    // C4 skip here
                    sum += C5;
                    continue;
                case 'C':
                case 'H':
                case 'L':
                case 'M':
                case 'P':
                case 'Q':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'k':
                case 'm':
                case 'w':
                case 'x':
                case '\u00A0':
                    sum += C6;
                    continue;
                default:
                    if (c == ' ')
                    {
                        sum += (n > 0 && src[n - 1] == ' ') ? C4 : C0;
                        continue;
                    }

                    if (c > '\u00FF')
                    {
                        if (char.IsSurrogate(c))
                        {
                            if (n + 1 < len && char.IsHighSurrogate(c) && char.IsLowSurrogate(src[n + 1]))
                            {
                                n += 1;
                            }

                            sum += CX;
                            continue;
                        }

                        sum += C3;
                        continue;
                    }

                    sum += CX;
                    continue;
            }
        }

        return sum;
    }

    /// <summary>
    /// Returns the first "significant text" fragment intended to be an algorithmic means of determining a "title" for a
    /// block of text.
    /// </summary>
    /// <remarks>
    /// The "weight" provides a weighting value, with higher values better. Text within markdown fences or pipe tables
    /// is not considered in text, while indented text will have low "weight" values. Newlines in the result string are
    /// substituted for spaces, and consecutive spaces are removed. If the result exceeds "maxLength", it is truncated
    /// and, optionally, appended with ellipses. The "maxLength" value has no effect on the result, but only its
    /// truncation. Serves as a string extension. This is annoying as there is no perfect algorithm and you can fiddle
    /// with this for ages. But it provides a crude fast algorithmic alternative to calling on a model.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Max less than 0</exception>
    public static string SigText(this string src, out double weight, int maxLength = int.MaxValue, SigOptions? opts = null)
    {
        // This is over-engineered. I enjoyed writing it. Sorry about that.
        opts ??= DefaultSigOpts;

        src = src.TrimStart();
        weight = TrySig(src, 1, opts, 0, out int n, out int length);

        if (length > 0 && weight > 0.0)
        {
            src = src.Substring(n, length).Trim(':', ',', ' '); // <- trim colon

            // Strip English "The" from start
            if (src.StartsWith("The ") && src.Length > 12) // <- fixed value
            {
                src = src.Substring(4);
            }

            src = src.TrimPretty(opts.PreserveLines, true);

            if (src.Length > 0)
            {
                if (opts.CapitalizeFirst && char.IsLower(src[0]))
                {
                    src = char.ToUpperInvariant(src[0]) + src.Substring(1);
                }

                return Truncate(src, maxLength, opts.TruncStyle);
            }
        }

        return "";
    }

    /// <summary>
    /// Overloaded variant of <see cref="SigText(string, out double, int, SigOptions)"/>.
    /// </summary>
    public static string SigText(this string src, int maxLength = int.MaxValue, SigOptions? opts = null)
    {
        return SigText(src, out _, maxLength, opts);
    }

    /// <summary>
    /// Find first word, skipping fenced code and pipe tables. Internal for test.
    /// </summary>
    internal static bool TryStartWord(string src, int n0, out int n1)
    {
        n1 = n0;
        int len = Math.Min(src.Length, MaxSigClamp);

        if (len == 0)
        {
            return false;
        }

        int consec = 0;
        bool pipe = false;
        bool paragraph = false;
        bool term0 = src.IsLineTermAt(n0 - 1, true);

        for (int n = n0; n < len; ++n)
        {
            var c = src[n];

            if (c < ' ' && c != '\n' && c != '\r' && c != '\t')
            {
                return false;
            }

            if (!pipe && IsStartCategory(CharUnicodeInfo.GetUnicodeCategory(c), c) &&
                (consec > 1 || term0 || src.IsSpaceOrTabAt(n - 1, true) || src.IsOpenQuoteAt(n - 1, true)))
            {
                n1 = n;
                return true;
            }

            bool termN = src.IsLineTermAt(n, false);

            if (term0 || termN)
            {
                term0 = false;
                int skipN = n;

                if (termN)
                {
                    skipN += 1;
                    consec += 1;

                    if (c == '\u2029')
                    {
                        // PARA
                        consec += 1;
                    }
                    else
                    if (c == '\r' && src.IsCharAt('\n', n + 1))
                    {
                        n += 1;
                        skipN += 1;
                    }
                }

                switch (TrySkipFence(src, skipN, out n1))
                {
                    case -1:
                        return false;
                    case +1:
                        return true;
                    default:
                        pipe = src.IsCharAt('|', skipN);
                        paragraph |= consec > 1 || pipe;
                        continue;
                }
            }

            consec = 0;
        }

        return false;
    }

    private static double TrySig(string src, int recCount, SigOptions opts, int n0, out int n1, out int length1)
    {
        ConditionalDebug.ThrowIfNegativeOrZero(recCount);

        if (src.Length == 0 || recCount > MaxSigRecursion)
        {
            n1 = n0;
            length1 = 0;
            return 0.0;
        }

        if (TryStartWord(src, n0, out n1))
        {
            // SpaceCount() aims to detect indented code and reduce likelyhood of selection
            int sc = SpaceCount(src, n0);
            double sw = 1.0 / (sc > 1 ? (sc * 0.65) : 1.0);

            var w1 = GetFragWeight(src, opts, n1, out length1) * sw;

            if (w1 >= opts.SigLength * MaxCapWeightF)
            {
                return w1;
            }

            var w2 = TrySig(src, ++recCount, opts, n1 + length1, out int n2, out int length2);

            if (w2 > w1)
            {
                n1 = n2;
                length1 = length2;
                return w2;
            }

            return w1;
        }

        length1 = 0;
        return 0.0;
    }

    private static int SpaceCount(string src, int n0)
    {
        int count = 0;
        var max = Math.Min(src.Length, MaxSigClamp);

        for (int n = n0; n < max; ++n)
        {
            if (src[n] == '\t')
            {
                count += 4;

                if (count > 24)
                {
                    return count;
                }

                continue;
            }

            if (src.IsSpaceAt(n, false))
            {
                count += 1;

                if (count > 24)
                {
                    return count;
                }

                continue;
            }

            if (src.IsLineTermAt(n, false))
            {
                count = 0;
                continue;
            }

            return count;
        }

        return count;
    }

    private static double GetFragWeight(string src, SigOptions opts, int n0, out int length)
    {
        // Clamp at a sensible maximum to prevent lengthy iteration if given spurious text.
        var max = Math.Min(src.Length, MaxSigClamp);

        int capSec = 0;
        double capWeight = 1.0;

        for (int n = n0; n < max; ++n)
        {
            var c = src[n];

            if (src.IsSentenceTermAt(n, true) &&
                (src.IsSpaceOrTabAt(n + 1, true) || src.IsLineTermAt(n + 1, true)) || src.IsCloseQuoteAt(n + 1, true))
            {
                length = n - n0 + 1;
                return Math.Min(length, opts.SigLength) * capWeight;
            }

            if (src.IsLineTermAt(n, false))
            {
                // Line end
                int skipN = n + 1;

                // Always set even if we return false
                length = n - n0;

                if (c == '\r' && src.IsCharAt('\n', skipN))
                {
                    continue;
                }

                if (c == '\u2029' || src.IsLineTermAt(skipN, true) || src.IsCharAt('|', skipN) || src.ContainsAt("```", skipN))
                {
                    // End of paragraph
                    return Math.Min(length, opts.SigLength) * capWeight;
                }
            }

            if (char.IsUpper(c))
            {
                // Text with acronyms or uppercase considered more important.
                capSec += 1;


                if (capSec > 1)
                {
                    capWeight = Math.Max(Math.Min(capSec * MinCapWeightF, MaxCapWeightF), capWeight);
                }
            }
            else
            {
                capSec = 0;
            }
        }

        length = max - n0;

        // Consider spurious if exceeds max
        return length > max ? 0 : Math.Min(length, opts.SigLength) * capWeight;
    }

    private static int TrySkipFence(string src, int n0, out int n1)
    {
        n1 = n0;

        if (n0 >= src.Length)
        {
            // Invalid
            return -1;
        }

        if (!src.ContainsAt("```", n0))
        {
            // No fence
            return 0;
        }

        // Skip fenced code (markdown)
        int pos = src.IndexOfLineTerm(n0 + 3);

        if (pos > -1)
        {
            pos = src.IndexOf("```", pos + 1);

            if (pos > -1)
            {
                pos = src.IndexOfLineTerm(pos + 3);

                if (pos > -1 && TryStartWord(src, pos, out n1))
                {
                    // Fenced and new word found
                    return +1;
                }
            }
        }

        // Invalid fence or nothing after fence
        return -1;
    }

    private static bool IsStartCategory(UnicodeCategory cat, char c)
    {
        // Removed:
        // case UnicodeCategory.CurrencySymbol:

        switch (cat)
        {
            case UnicodeCategory.UppercaseLetter:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.TitlecaseLetter:
            case UnicodeCategory.OtherLetter:
            case UnicodeCategory.DecimalDigitNumber:
            case UnicodeCategory.LetterNumber:
                // Not needed but should always exclude these if we add punct above
                // return c != '`' && c != '|';
                return true;
            default:
                return c == '¿';
        }
    }
}
