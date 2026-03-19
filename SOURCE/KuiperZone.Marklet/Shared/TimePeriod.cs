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

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Common <see cref="TimeSpan"/> identifiers.
/// </summary>
/// <remarks>
/// Integer values may be written to disk and should not change.
/// </remarks>
public enum TimePeriod
{
    /// <summary>
    /// None, zero or immediate default.
    /// </summary>
    None = 0,

    /// <summary>
    /// 1 day.
    /// </summary>
    OneDay = 1,

    /// <summary>
    /// 3 days.
    /// </summary>
    ThreeDays = 3,

    /// <summary>
    /// 7 days.
    /// </summary>
    SevenDays = 7,

    /// <summary>
    /// 14 days.
    /// </summary>
    FourteenDays = 14,

    /// <summary>
    /// 30 days.
    /// </summary>
    ThirtyDays = 30,

    /// <summary>
    /// 90 days.
    /// </summary>
    NinetyDays = 90,

    /// <summary>
    /// 180 days.
    /// </summary>
    OneEightyDays = 180,

    /// <summary>
    /// 180 days.
    /// </summary>
    ThreeSixtyFiveDays = 365,

    /// <summary>
    /// Never.
    /// </summary>
    Never = -1,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets the equivalent TimeSpan.
    /// </summary>
    public static TimeSpan ToSpan(this TimePeriod src)
    {
        switch (src)
        {
            case TimePeriod.None:
                return TimeSpan.Zero;
            case TimePeriod.OneDay:
                return TimeSpan.FromDays(1);
            case TimePeriod.ThreeDays:
                return TimeSpan.FromDays(3);
            case TimePeriod.SevenDays:
                return TimeSpan.FromDays(7);
            case TimePeriod.FourteenDays:
                return TimeSpan.FromDays(14);
            case TimePeriod.ThirtyDays:
                return TimeSpan.FromDays(30);
            case TimePeriod.NinetyDays:
                return TimeSpan.FromDays(90);
            case TimePeriod.OneEightyDays:
                return TimeSpan.FromDays(180);
            case TimePeriod.ThreeSixtyFiveDays:
                return TimeSpan.FromDays(365);
            default:
                // Never
                return TimeSpan.MaxValue;
        }
    }

    /// <summary>
    /// Gets a display name.
    /// </summary>
    public static string DisplayName(this TimePeriod src)
    {
        switch (src)
        {
            case TimePeriod.None:
                return "None";
            case TimePeriod.OneDay:
                return "1 Day";
            case TimePeriod.Never:
                return "Never";
            default:
                return ToSpan(src).TotalDays + " Days";
        }
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static bool IsLegal(this TimePeriod src, bool allowZero = false, bool allowNever = true)
    {
        if (src == TimePeriod.None && !allowZero)
        {
            return false;
        }

        if (src == TimePeriod.Never && !allowNever)
        {
            return false;
        }

        return Enum.IsDefined(src);
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static TimePeriod TrimLegal(this TimePeriod src, bool allowZero = false)
    {
        return IsLegal(src, allowZero) ? src : TimePeriod.Never;
    }
}
