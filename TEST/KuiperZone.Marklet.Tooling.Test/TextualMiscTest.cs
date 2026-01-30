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

using Xunit.Abstractions;

namespace KuiperZone.Marklet.Tooling.Test;

public class TextualMiscTest
{
    private readonly ITestOutputHelper _out;

    public TextualMiscTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void GetFriendlyNameOf()
    {
        Assert.Equal("A", Textual.GetFriendlyNameOf("a"));
        Assert.Equal("OK", Textual.GetFriendlyNameOf("OK"));
        Assert.Equal("Ok Hello", Textual.GetFriendlyNameOf("OkHello"));
        Assert.Equal("OKHello", Textual.GetFriendlyNameOf("OKHello"));
        Assert.Equal("Hello World", Textual.GetFriendlyNameOf("HelloWorld"));
        Assert.Equal("Hello World", Textual.GetFriendlyNameOf("helloWorld"));
        Assert.Equal("Hello", Textual.GetFriendlyNameOf("hello"));
        Assert.Equal("Worl D", Textual.GetFriendlyNameOf("WorlD"));
        Assert.Equal("One23 One", Textual.GetFriendlyNameOf("One23One"));

        // Edge cases
        Assert.Empty(Textual.GetFriendlyNameOf(""));
        Assert.Equal("Hello-World", Textual.GetFriendlyNameOf("Hello-World"));
        Assert.Equal("HelloWorld", Textual.GetFriendlyNameOf("\nHelloWorld"));
        Assert.Equal("HelloWorld", Textual.GetFriendlyNameOf(" HelloWorld "));
    }

    [Fact]
    public void ToSuperscript_HandlesMappedChars()
    {
        var test = "0123456789+-=()abcdefghijklmnoprstuvwxyzABDEGHIJKLMNOPRTUVW";
        var exp = "⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾ᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷˣʸᶻᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁⱽᵂ";
        Assert.Equal(exp, Textual.ToSuperscript(test));

        // Char not supported
        Assert.Equal("ᴴᵉˡˡᵒ$", Textual.ToSuperscript("Hello$"));
    }

    [Fact]
    public void ToSubscript_HandlesMappedChars()
    {
        var test = "0123456789+-=()aehijklmnoprstuvx";
        var exp =  "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎ₐₑₕᵢⱼₖₗₘₙₒₚᵣₛₜᵤᵥₓ";
        Assert.Equal(exp, Textual.ToSubscript(test));

        // Char not supported
        Assert.Equal("Hₑₗₗₒ$", Textual.ToSubscript("Hello$"));
    }

}