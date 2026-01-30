// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// AUTHOR    : Andrew Thomas
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

namespace KuiperZone.Marklet.Tooling.Test;

public class MagverterTest
{
    [Fact]
    public void ToFriendlyBytes()
    {
        Console.WriteLine(Magverter.ToFriendlyBytes(1024 * 1099 * 800, BitMag.Giga));

        Assert.Equal("999 B", Magverter.ToFriendlyBytes(999, BitMag.Auto));
        Assert.Equal("1.0 KB", Magverter.ToFriendlyBytes(1024, BitMag.Auto));
        Assert.Equal("8.0 KB", Magverter.ToFriendlyBytes(1024 * 8, BitMag.Auto));
        Assert.Equal("8.6 MB", Magverter.ToFriendlyBytes(1024 * 1099 * 8, BitMag.Auto));

        Assert.Equal("8792.0 KB", Magverter.ToFriendlyBytes(1024 * 1099 * 8, BitMag.Kilo));
        Assert.Equal("0.8 GB", Magverter.ToFriendlyBytes(1024 * 1024 * 800, BitMag.Giga));

        // Expected
        Assert.Equal("80.0 MB", Magverter.ToFriendlyBytes(1024 * 1024 * 80, BitMag.Giga));
    }

    [Fact]
    public void ToFriendlyBits()
    {
        Console.WriteLine(Magverter.ToFriendlyBits(1024 * 1099 * 800, BitMag.Giga));

        Assert.Equal("999 b", Magverter.ToFriendlyBits(999, BitMag.Auto));
        Assert.Equal("1.0 Kb", Magverter.ToFriendlyBits(1024, BitMag.Auto));
        Assert.Equal("8.0 Kb", Magverter.ToFriendlyBits(1024 * 8, BitMag.Auto));
        Assert.Equal("8.6 Mb", Magverter.ToFriendlyBits(1024 * 1099 * 8, BitMag.Auto));

        Assert.Equal("8792.0 Kb", Magverter.ToFriendlyBits(1024 * 1099 * 8, BitMag.Kilo));
        Assert.Equal("0.8 Gb", Magverter.ToFriendlyBits(1024 * 1024 * 800, BitMag.Giga));

        // Expected
        Assert.Equal("80.0 Mb", Magverter.ToFriendlyBits(1024 * 1024 * 80, BitMag.Giga));
    }

}