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

using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.Tooling.Test;

public class SanitizerTest
{
    private readonly ITestOutputHelper _out;

    public SanitizerTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void Sanitize_Default_SubsWinCrLf()
    {
        var flags = SanFlags.Default;

        var exp = "\nLine0\nLine2\tLine2\nLine3\x1F\n";
        var act = Sanitizer.Sanitize("\r\nLine0\rLine2\tLine2\nLine3\x1F\r\n", flags);
        Assert.Equal(exp, act);
    }

    [Fact]
    public void Sanitize_DefaultWithTrim_Trims()
    {
        var flags = SanFlags.Default | SanFlags.Trim;

        var exp = "Line0\nLine2\tLine2\nLine3\x1F";
        var act = Sanitizer.Sanitize("\r\nLine0\rLine2\tLine2\nLine3\x1F\r\n", flags);
        Assert.Equal(exp, act);
    }

    [Fact]
    public void Sanitize_Default_TrimsMax()
    {
        var flags = SanFlags.Default | SanFlags.Trim;

        var exp = "Line0\nL";
        var act = Sanitizer.Sanitize("\r\nLine0\r\nLine2\tLine2\rLine3\x1F\r\n", flags, 7);
        Assert.Equal(exp, act);

        exp = "Line0";
        act = Sanitizer.Sanitize("\r\nLine0\r\nLine2\tLine2\rLine3\x1F\r\n", flags, 6);
        Assert.Equal(exp, act);

        exp = "Line0";
        act = Sanitizer.Sanitize("\r\nLine0\rLine2\tLine2\nLine3\x1F\r\n", flags, 5);
        Assert.Equal(exp, act);
    }

    [Fact]
    public void Sanitize_Default_OmitsInvalidSurrogates()
    {
        const char high = '\uD83D'; // 🗀
        const char low = '\uDDC0';

        var flags = SanFlags.Default;
        Assert.Equal($"AB", Sanitizer.Sanitize($"AB", flags));

        // Invalid removed
        var exp = $"A{high}{low}B\n\tLine2";
        var act = Sanitizer.Sanitize($"A{high}{low}{low}{low}{high}B\n\tLine2{high}", flags);
        Assert.Equal(exp, act);

        // 𐆃
        act = Sanitizer.Sanitize("\uD800\uDD83", flags);
        Assert.Equal("\uD800\uDD83", act);
    }

    [Fact]
    public void Sanitize_NormC_NoCombining()
    {
        string c0 = "a\u0304\u0308"; // ā̈  composite
        string c1 = "XX";
        string c2 = "c\u0327"; // ç
        string c3 = "\uD83D\uDDC0"; // 🗀
        string c4 = "X"; // X

        var flags = SanFlags.NormC | SanFlags.NoCombining;
        string s = Sanitizer.Sanitize($"{c0}{c1}{c2}{c3}{c4}", flags);
        _out.WriteLine($"C Form: {s}");
        _out.WriteLine($"C Length: {s.Length}");

        for (int n = 0; n < s.Length; ++n)
        {
            _out.WriteLine($"[{n}]: 0x{(int)s[n]:X4}");
        }

        Assert.Equal($"\u0101XX\u00E7\uD83D\uDDC0X", s);
    }

    [Fact]
    public void Sanitize_SubControlNormC_PreserveTabs()
    {
        var flags = SanFlags.SubControl | SanFlags.NormC;

        var exp = "\tHello \t Tab\t";
        var act = Sanitizer.Sanitize("\tHello \t Tab\t", flags);
        Assert.Equal(exp, act);
    }

    [Fact]
    public void Sanitize_SubFeed_Substitutes()
    {
        const char Sub = Sanitizer.SubChar;
        var flags = SanFlags.SubFeed;
        string s = Sanitizer.Sanitize("\0 = Null, \u0085 = NEL, \x0C = FF, \u2029 = PS, \u001B = Escape", flags);
        Assert.Equal($"{Sub} = Null, \n = NEL, \n\n = FF, \n\n = PS, \u001B = Escape", s);
    }

    [Fact]
    public void Sanitize_SubControl_Substitutes()
    {
        const char Sub = Sanitizer.SubChar;
        var flags = SanFlags.SubControl;
        string s = Sanitizer.Sanitize("\0 = Null, \u0085 = NEL, \u0008 = Backspace, \u001B = Escape", flags);
        Assert.Equal($"{Sub} = Null, \n = NEL, ␈ = Backspace, ␛ = Escape", s);
    }

    [Fact]
    public void Sanitize_SubSpace_Substitutes()
    {
        const char Sub = Sanitizer.SubChar;
        var flags = SanFlags.SubSpace;
        string s = Sanitizer.Sanitize("\0 = Null, \u0085 = NEL, \u2002\u2003 = enem, \u2029 = PS, \u001B = Escape", flags);
        Assert.Equal($"{Sub} = Null, \u0085 = NEL, \x20\x20 = enem, \u2029 = PS, \u001B = Escape", s);
    }

    [Fact]
    public void Sanitize_VerticalTab_TreatedAsLineBreak()
    {
        var flags = SanFlags.SubFeed;
        var input = "Line1\u000BLine2";
        var expected = "Line1\nLine2";

        Assert.Equal(expected, Sanitizer.Sanitize(input, flags));
    }

    [Fact]
    public void Sanitize_InvalidLoneLowSurrogate_ReplacedOrOmitted()
    {
        const char loneLow = '\uDDC0'; // lone low surrogate

        var flags = SanFlags.Default;
        var input = $"A{loneLow}B";
        var result = Sanitizer.Sanitize(input, flags);

        // Current behavior: omitted → "AB"
        Assert.Equal("AB", result);
    }

    [Fact]
    public void Sanitize_PostNormalizeCombiningRemoval()
    {
        // This verifies intentional order: NFC first → then strip remaining combining
        var input = "a\u0301\u0308";           // a + acute + diaeresis
        var flags = SanFlags.NormC | SanFlags.NoCombining;

        // After NFC → á (precomposed) + remaining diaeresis → then strip diaeresis → á
        var expected = "\u00E1"; // just "á"

        Assert.Equal(expected, Sanitizer.Sanitize(input, flags));
    }

    [Fact]
    public void Sanitize_MaxLengthWithLateExpansion()
    {
        var flags = SanFlags.SubFeed | SanFlags.Trim;
        var input = "Hello\x0CWorld"; // formfeed at end
        var max = 10;

        // After sub: "Hello\n\nWorld" (12 chars) → truncated to 10 → "Hello\n\nWo"
        var result = Sanitizer.Sanitize(input, flags, max);
        Assert.True(result.Length <= max);
        Assert.StartsWith("Hello\n\n", result); // at least the expansion is partially preserved
    }
    [Fact]

    public void Sanitize_ThrashNoThrow()
    {
        var flags = SanFlags.NormC | SanFlags.NoCombining | SanFlags.SubControl | SanFlags.Trim;

        var seed = Random.Shared.Next();
        _out.WriteLine($"SEED: {seed}");

        bool started = false;
        var sw = new Stopwatch();
        var rand = new Random(seed);

        for (int n = 0; n < 1000; ++n)
        {
            var count = rand.Next(50, 200);
            var sb = new StringBuilder(count);

            for (int a = 0; a < count; ++a)
            {
                sb.Append((char)rand.Next(char.MinValue, char.MaxValue));
            }

            Sanitizer.Sanitize(sb.ToString(), flags);

            if (!started)
            {
                started = true;
                sw.Start();
            }
        }

        sw.Stop();
        _out.WriteLine($"TIME: {sw.Elapsed}");
    }

    [Fact]
    public void ToDebugSafe()
    {
        Assert.Null(Sanitizer.ToDebugSafe(null, false));
        Assert.Empty(Sanitizer.ToDebugSafe("", false));
        Assert.Equal("{null}", Sanitizer.ToDebugSafe(null, true));
        Assert.Equal("{empty}", Sanitizer.ToDebugSafe("", true));

        Assert.Equal("Hello\\n\\t\u0101World\\x1F\\uD83D", Sanitizer.ToDebugSafe("Hello\n\t\u0101World\x1F\uD83D", false));
        Assert.Equal("`Hello\\n\\t\u0101World\\x1F\\uD83D`", Sanitizer.ToDebugSafe("Hello\n\t\u0101World\x1F\uD83D", false, true));
    }

}