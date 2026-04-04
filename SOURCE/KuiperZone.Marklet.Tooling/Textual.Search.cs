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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Find option flags.
/// </summary>
[Flags]
public enum FindFlags
{
    /// <summary>
    /// Simple case sensitive.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// Case insensitive.
    /// </summary>
    IgnoreCase = 0x0001,

    /// <summary>
    /// Whole words only.
    /// </summary>
    Word = 0x0002,
}

/// <summary>
/// Miscellaneous text routines that serve text as a string extension.
/// </summary>
public static partial class Textual
{
    /// <summary>
    /// Returns the first index of "subtext" string from the given "startIndex" according to flags, or -1 if not found.
    /// </summary>
    /// <remarks>
    /// This is an extended "IndexOf" routine. The "scanLimit" givens the maximum number of positions to examine from "startIndex". It may exceed the length of
    /// "src", but cannot be negative. The "scanLimit" may be used to limit the length searched.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">startIndex, or negative scanLimit</exception>
    public static int Find(this string src, string? subtext, int startIndex, FindFlags flags, int scanLimit = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, src.Length, nameof(startIndex));
        ArgumentOutOfRangeException.ThrowIfNegative(scanLimit, nameof(scanLimit));

        scanLimit = Math.Min(src.Length - startIndex, scanLimit);

        if (string.IsNullOrEmpty(subtext) || subtext.Length > Math.Min(src.Length, scanLimit))
        {
            return -1;
        }

        var comp = flags.HasFlag(FindFlags.IgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        while ((startIndex = src.IndexOf(subtext, startIndex, scanLimit, comp)) > -1)
        {
            if (!flags.HasFlag(FindFlags.Word) ||
                (IsWordBoundaryAt(src, startIndex - 1) &&
                IsWordBoundaryAt(src, startIndex + subtext.Length)))
            {
                return startIndex;
            }

            startIndex += subtext.Length;
            scanLimit = Math.Min(src.Length - startIndex, scanLimit);
        }

        return -1;
    }

    /// <summary>
    /// Returns the first index of "subtext" string according to "flags", or -1 if not found.
    /// </summary>
    /// <remarks>
    /// This is an extended "IndexOf" routine. The "scanLimit" givens the maximum number of positions to examine from
    /// "startIndex". It may exceed the length of "src", but cannot be negative. The "scanLimit" may be used to limit
    /// the length searched.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">startIndex, or negative scanLimit</exception>
    public static int Find(this string src, string? subtext, FindFlags flags, int scanLimit = int.MaxValue)
    {
        return Find(src, subtext, 0, flags, scanLimit);
    }

    /// <summary>
    /// Finds first instance of "subtext" in "src" and returns a "snippet" on success.
    /// </summary>
    /// <remarks>
    /// The result is may include short a text prior to the "sub" occurrance and is null if no match is found.
    /// Otherwise, the returned string may not exceed "maxLength". The "scanLimit" may be used to limit the length
    /// searched.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">negative scanLimit</exception>
    public static string? PrettyFind(this string src, string? subtext, int maxLength, FindFlags flags, int scanLimit = int.MaxValue)
    {
        var index = Find(src, subtext, 0, flags, scanLimit);

        if (index < 0)
        {
            return null;
        }

        index = PrettyStart(src, index, maxLength);
        src = Truncate(src.Substring(index), maxLength, TruncStyle.EndEllipses);
        return TrimPretty(src, false, false);
    }

    private static bool IsWordBoundaryAt(string s, int index)
    {
        // Consider these NOT word characters (boundaries)
        if (index < 0 || index >= s.Length)
        {
            return true;
        }

        return !char.IsLetterOrDigit(s[index]);
    }

    private static int PrettyStart(string src, int index, int maxLength)
    {
        const int MaxBack = 24;

        bool space = false;
        int end = Math.Max(index - Math.Min(MaxBack, maxLength / 2), -1);

        // index-1 = word boundary
        for (int n = index - 1; n > end; --n)
        {
            if (n == 0)
            {
                return 0;
            }

            if (IsSpaceAt(src, n, false))
            {
                space = true;
                index = n + 1;
                continue;
            }

            if (src[n] < ' ' || src.IsLineTermAt(n, false) || (space && src.IsSentenceTermAt(n, false)))
            {
                return n + 1;
            }

            space = false;
        }

        return index;
    }
}
