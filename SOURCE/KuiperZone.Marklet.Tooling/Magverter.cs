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
/// Converts numeric values to magnitude string representations.
/// </summary>
public static class Magverter
{
    // https://gist.github.com/marbel82/5bfa2010b534c681a3c618a98719c862
    private static readonly BitMag[] MagValues = Enum.GetValues<BitMag>();

    // https://www.techtarget.com/searchstorage/definition/Kilo-mega-giga-tera-peta-and-all-that
    private static readonly string[] BitSuffix = [" b", " Kb", " Mb", " Gb", " Tb", " Pb", " Eb", " Zb"];
    private static readonly string[] ByteSuffix = [" B", " KB", " MB", " GB", " TB", " PB", " EB", " ZB" ];

    /// <summary>
    /// Converts a byte "value" to a friendly string. I.e. 2050 gives "2.0 KB" etc.
    /// </summary>
    public static string ToFriendlyBytes(long value, BitMag mag = BitMag.Auto)
    {
        return ToBitByteString(value, mag, ByteSuffix);
    }

    /// <summary>
    /// Converts a bit "value" to a friendly string. I.e. 2050 gives "2.0 Kb" etc.
    /// </summary>
    public static string ToFriendlyBits(long value, BitMag mag = BitMag.Auto)
    {
        return ToBitByteString(value, mag, BitSuffix);
    }

    private static long GetMagThreshold(long value, BitMag mag)
    {
        long rslt = 1024;

        foreach (var item in MagValues)
        {
            if (mag == item)
            {
                break;
            }

            rslt *= 1024;
        }

        if (value < rslt)
        {
            return 1024;
        }

        return rslt;
    }

    private static string ToBitByteString(long value, BitMag mag, string[] suffix)
    {
        bool negative = value < 0;
        value = Math.Abs(value);

        if (value == 0)
        {
            return "0";
        }

        int idx = 0;
        double dz = value;
        var threshold = GetMagThreshold(value, mag);

        while((value >= threshold || (mag != BitMag.Auto && dz >= 102.4 && dz < 1024)) && idx < suffix.Length - 1)
        {
            idx += 1;
            value /= 1024;
            dz /= 1024;
        }

        string rslt;

        if (idx > 0)
        {
            // KB or greater
            // Use current culture here
            rslt = dz.ToString("0.0", CultureInfo.CurrentCulture) + suffix[idx];
        }
        else
        {
            // Small byte number
            // Integer below 1024
            rslt = value.ToString(CultureInfo.InvariantCulture) + suffix[idx];
        }

        // Negative bytes?
        return negative ? "-" + rslt : rslt;
    }

}