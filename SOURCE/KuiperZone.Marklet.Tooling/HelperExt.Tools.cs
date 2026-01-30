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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
    private static readonly TimeSpan OneWeek = TimeSpan.FromDays(7);

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
    /// Returns language neutral "friendly" time string using local time if "local" is true (otherwise UTC).
    /// </summary>
    /// <remarks>
    /// The method should be polled periodically to update the time.
    /// </remarks>
    public static string ToFriendlyString(this DateTime src, bool local = true)
    {
        DateTime now;

        if (local)
        {
            now = DateTime.Now;
            src = src.ToLocalTime();
        }
        else
        {
            now = DateTime.UtcNow;
            src = src.ToUniversalTime();
        }

        var delta = now - src;

        if (delta >= TimeSpan.Zero)
        {
            if (delta < OneDay && src.DayOfYear == now.DayOfYear)
            {
                return src.ToShortTimeString();
            }

            if (delta < OneWeek)
            {
                // Month-day
                return src.ToString("m");
            }
        }

        return src.ToShortDateString();
    }
}