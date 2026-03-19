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

using System.Globalization;

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
    private static readonly TimeSpan ThreeDays = TimeSpan.FromDays(3);

    /// <summary>
    /// Calls TrimExcess() only Capacity exceed Count by delta.
    /// </summary>
    public static void TrimCapacity<T>(this List<T> src, int delta = 32)
    {
        if (src.Capacity - src.Count > delta)
        {
            src.TrimExcess();
        }
    }

    /// <summary>
    /// Calls TrimExcess() only Capacity exceed Count by delta.
    /// </summary>
    public static void TrimCapacity<T>(this HashSet<T> src, int delta = 32)
    {
        if (src.Capacity - src.Count > delta)
        {
            src.TrimExcess();
        }
    }

    /// <summary>
    /// Calls TrimExcess() only Capacity exceed Count by delta.
    /// </summary>
    public static void TrimCapacity<T1, T2>(this SortedList<T1, T2> src, int delta = 32) where T1 : notnull
    {
        if (src.Capacity - src.Count > delta)
        {
            src.TrimExcess();
        }
    }

    /// <summary>
    /// Returns language neutral "friendly" local time string.
    /// </summary>
    /// <remarks>
    /// The method should be polled periodically to update the time.
    /// </remarks>
    public static string ToFriendly(this DateTime src, bool weekDays = true)
    {
        DateTime now = DateTime.Now;

        src = src.ToLocalTime();
        var delta = now - src;

        if (delta >= TimeSpan.Zero)
        {
            if (delta < OneDay && src.DayOfYear == now.DayOfYear)
            {
                return string.Concat(GetLocalToday(), src.ToShortTimeString());
            }

            if (weekDays && delta < ThreeDays)
            {
                return string.Concat(src.ToString("dddd"), " ", src.ToShortTimeString());
            }
        }

        return src.ToShortDateString();
    }

    private static string GetLocalToday()
    {
        // Simple fallback map for common languages
        switch(CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
        {
            case "en": return "Today ";
            case "fr": return "Aujourd'hui ";
            case "de": return "Heute ";
            case "es": return "Hoy ";
            case "it": return "Oggi ";
            case "nl": return "Vandaag ";
            case "sv": return "Idag ";
            case "ru": return "Сегодня ";
            case "pl": return "Dzisiaj ";
            case "pt": return "Hoje ";
            case "ja": return "今日 ";
            case "zh": return "今天 ";
            case "ko": return "오늘 ";
            default: return "";
        };
    }
}