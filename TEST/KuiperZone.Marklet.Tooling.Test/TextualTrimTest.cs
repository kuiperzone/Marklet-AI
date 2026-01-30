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

public class TextualTrimTest
{
    [Fact]
    public void TrimSpace()
    {
        Assert.Equal("", Textual.TrimSpace(""));
        Assert.Equal("", Textual.TrimSpace(" "));
        Assert.Equal("Hello world", Textual.TrimSpace("  \tHello world\t "));

        // Not currently implement escaped spacing
        Assert.Equal("Hello world\\", Textual.TrimSpace("  Hello world\\    "));
    }

    [Fact]
    public void TrimPretty_PreserveLinesAndNoSides()
    {
        Assert.Equal("", Textual.TrimPretty("", true, false));
        Assert.Equal(" ", Textual.TrimPretty("   ", true, false));
        Assert.Equal(" Hello world ", Textual.TrimPretty("  Hello   world   ", true, false));
        Assert.Equal(" Hello world ", Textual.TrimPretty("  \tHello\t \tworld\t   ", true, false));

        Assert.Equal(" Hello \n\n world ", Textual.TrimPretty("  \tHello\t \n\n\t world\t   ", true, false));
        Assert.Equal(" \n\nHello world\n\n ", Textual.TrimPretty("  \n\nHello   world\n\n   ", true, false));
        Assert.Equal(" \n\n \nHello world\n \n\n ", Textual.TrimPretty("  \n\n  \nHello   world\n  \n\n   ", true, false));
    }

    [Fact]
    public void TrimPretty_Default()
    {
        Assert.Equal("", Textual.TrimPretty(""));
        Assert.Equal("", Textual.TrimPretty("   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  Hello   world   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  \tHello\t \tworld\t   "));

        Assert.Equal("Hello world", Textual.TrimPretty("  \tHello\nworld\t   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  \tHello\r\nworld\t   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  \tHello\n\nworld\t   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  \tHello\r\rworld\t   "));

        Assert.Equal("Hello\\nworld", Textual.TrimPretty("  \tHello\\nworld\t   "));

        Assert.Equal("Hello world", Textual.TrimPretty("  \r\nHello\r\nworld\r\n   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  \tHello\t \n\n\t world\t   "));
        Assert.Equal("Hello world", Textual.TrimPretty("  \n\n  \nHello \n  world\n  \n\n   "));
    }

    [Fact]
    public void Truncate_HandlesEmpty()
    {
        Assert.Empty(Textual.Truncate("", 5, TruncStyle.End));
        Assert.Empty(Textual.Truncate("", 5, TruncStyle.StartEllipses));
        Assert.Empty(Textual.Truncate("", 5, TruncStyle.CenterEllipses)!);
    }

    [Fact]
    public void Truncate_DoesNotTruncateShort()
    {
        // No truncation
        Assert.Equal("123", Textual.Truncate("123", 3, TruncStyle.End));
        Assert.Equal("123", Textual.Truncate("123", 4, TruncStyle.End));

        Assert.Equal("123", Textual.Truncate("123", 3, TruncStyle.Start));
        Assert.Equal("123", Textual.Truncate("123", 4, TruncStyle.StartEllipses));

        Assert.Equal("0123", Textual.Truncate("0123", 4, TruncStyle.CenterEllipses));
        Assert.Equal("0123", Textual.Truncate("0123", 5, TruncStyle.CenterEllipses));
    }

    [Fact]
    public void Truncate_End()
    {
        Assert.Equal("12", Textual.Truncate("1234567890", 2, TruncStyle.End, false));
        Assert.Equal("123", Textual.Truncate("1234567890", 3, TruncStyle.End, false));

        Assert.Equal("12", Textual.Truncate("1234567890", 2, TruncStyle.End, true));
        Assert.Equal("123", Textual.Truncate("1234567890", 3, TruncStyle.End, true));
    }

    [Fact]
    public void Truncate_EndEllipses()
    {
        Assert.Equal("1...", Textual.Truncate("1234567890", 4, TruncStyle.EndEllipses, false));
        Assert.Equal("123...", Textual.Truncate("1234567890", 6, TruncStyle.EndEllipses, false));

        Assert.Equal("123…", Textual.Truncate("1234567890", 4, TruncStyle.EndEllipses, true));
        Assert.Equal("12345…", Textual.Truncate("1234567890", 6, TruncStyle.EndEllipses, true));

        Assert.Equal("He…", Textual.Truncate("Hello", 3, TruncStyle.EndEllipses));
    }

    [Fact]
    public void Truncate_EndEllipses_FallsBackToEnd()
    {
        Assert.Equal("12", Textual.Truncate("1234567890", 2, TruncStyle.EndEllipses, false));
        Assert.Equal("...", Textual.Truncate("1234567890", 3, TruncStyle.EndEllipses, false));

        Assert.Equal("1…", Textual.Truncate("1234567890", 2, TruncStyle.EndEllipses, true));
        Assert.Equal("…", Textual.Truncate("1234567890", 1, TruncStyle.EndEllipses, true));
    }

    [Fact]
    public void Truncate_Start()
    {
        Assert.Equal("90", Textual.Truncate("1234567890", 2, TruncStyle.Start, false));
        Assert.Equal("890", Textual.Truncate("1234567890", 3, TruncStyle.Start, false));

        Assert.Equal("90", Textual.Truncate("1234567890", 2, TruncStyle.Start, true));
        Assert.Equal("890", Textual.Truncate("1234567890", 3, TruncStyle.Start, true));
    }

    [Fact]
    public void Truncate_StartEllipses()
    {
        Assert.Equal("...0", Textual.Truncate("1234567890", 4, TruncStyle.StartEllipses, false));
        Assert.Equal("...890", Textual.Truncate("1234567890", 6, TruncStyle.StartEllipses, false));

        Assert.Equal("…890", Textual.Truncate("1234567890", 4, TruncStyle.StartEllipses, true));
        Assert.Equal("…67890", Textual.Truncate("1234567890", 6, TruncStyle.StartEllipses, true));
    }

    [Fact]
    public void Truncate_StartEllipses_FallsBackToStart()
    {
        Assert.Equal("90", Textual.Truncate("1234567890", 2, TruncStyle.StartEllipses, false));
        Assert.Equal("...", Textual.Truncate("1234567890", 3, TruncStyle.StartEllipses, false));

        Assert.Equal("…0", Textual.Truncate("1234567890", 2, TruncStyle.StartEllipses, true));
        Assert.Equal("…", Textual.Truncate("1234567890", 1, TruncStyle.StartEllipses, true));
    }

    [Fact]
    public void Truncate_CenterEllipses()
    {
        // 01234567   -> 0...7 [5]
        // 01234567   -> 01...7 [6]
        Assert.Equal("0...7", Textual.Truncate("01234567", 5, TruncStyle.CenterEllipses, false));
        Assert.Equal("01...7", Textual.Truncate("01234567", 6, TruncStyle.CenterEllipses, false));

        // 01234567   -> 01…67 [5]
        // 01234567   -> 012…67 [6]
        Assert.Equal("01…67", Textual.Truncate("01234567", 5, TruncStyle.CenterEllipses, true));
        Assert.Equal("012…67", Textual.Truncate("01234567", 6, TruncStyle.CenterEllipses, true));

        // 012345678  -> 01...78 [7]
        // 0123456789 -> 01...89 [7]
        Assert.Equal("01...78", Textual.Truncate("012345678", 7, TruncStyle.CenterEllipses, false));
        Assert.Equal("01...89", Textual.Truncate("0123456789", 7, TruncStyle.CenterEllipses, false));

        // 012345678  -> 012…678 [7]
        // 0123456789 -> 012…789 [7]
        Assert.Equal("012…678", Textual.Truncate("012345678", 7, TruncStyle.CenterEllipses, true));
        Assert.Equal("012…789", Textual.Truncate("0123456789", 7, TruncStyle.CenterEllipses, true));

        // 0123456789 -> 012...89 [8]
        // 0123456789 -> 012...789 [9]
        Assert.Equal("012...89", Textual.Truncate("0123456789", 8, TruncStyle.CenterEllipses, false));
        Assert.Equal("012...789", Textual.Truncate("0123456789", 9, TruncStyle.CenterEllipses, false));

        // 0123456789 -> 0123…789 [8]
        // 0123456789 -> 0123…6789 [9]
        Assert.Equal("0123…789", Textual.Truncate("0123456789", 8, TruncStyle.CenterEllipses, true));
        Assert.Equal("0123…6789", Textual.Truncate("0123456789", 9, TruncStyle.CenterEllipses, true));
    }

    [Fact]
    public void Truncate_CenterEllipses_FallsBackToRight()
    {
        Assert.Equal("0...", Textual.Truncate("01234567", 4, TruncStyle.CenterEllipses, false));
        Assert.Equal("...", Textual.Truncate("01234567", 3, TruncStyle.CenterEllipses, false));

        Assert.Equal("0…", Textual.Truncate("01234567", 2, TruncStyle.CenterEllipses, true));
        Assert.Equal("…", Textual.Truncate("01234567", 1, TruncStyle.CenterEllipses, true));
    }

}