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

public class TextualSummaryTest
{
    private const string FencedCode = @"```Code
if (a == b)
DoSomething()
```";

    private const string PipeTable = @"|Head|Head
|---|---|
|data|data|";

    private readonly ITestOutputHelper _out;

    public TextualSummaryTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

[Fact]
    public void EstimateTokens()
    {
        // From Tokenizer: 43 tokens, 217 chars
        var text = "Base32 Crockford is not included in .NET by default. You can either implement it manually or use a " +
        "library. For manual use, the following is a minimal implementation without padding and with case-insensitive decoding:";

        double tok = Textual.EstimateTokens(text);
        _out.WriteLine(tok.ToString());
        Assert.InRange(tok, 40, 50);



        // From Tokenizer: 55 tokens, 309 chars
        text = "In the absence of regulation, AI could become the dominant force in decision-making across " +
        "sectors—justice, healthcare, education, and more. These decisions could become opaque, biased, or " +
        "unaccountable, as algorithms might prioritize efficiency or profitability over human values like fairness " +
        "or compassion.";

        // 309 / 4.2 = 73
        tok = Textual.EstimateTokens(text);
        _out.WriteLine(tok.ToString());
        Assert.InRange(tok, 65, 85);



        // From Tokenizer: 91 tokens, 386 chars
        text = "For case-insensitive, padding-free base encoding, Base32 Crockford is a suitable option:\n\n" +
        "Alphabet: 0123456789ABCDEFGHJKMNPQRSTVWXYZ\n\n" +
        "Case-insensitive: Accepts both upper and lower case.\n\n" +
        "No padding required\n\n" +
        "Avoids ambiguous characters: Omits I, L, O, and U to reduce transcription errors.\n\n" +
        "It is designed for human use (e.g., identifiers, keys) and maintains good machine readability.";

        tok = Textual.EstimateTokens(text);
        _out.WriteLine(tok.ToString());
        Assert.InRange(tok, 80, 100);



        // From Tokenizer: 192 tokens, 1105 char / 4.2 = 263
        text = "To avoid being controlled by AI systems in this context, one must enact both technical and epistemic resistance:\n\n" +
        "Minimize Data Exposure:\n" +
        "Limit the information you provide. Avoid revealing personal identifiers or behavioral patterns. Use anonymity-preserving tools.\n\n" +
        "Understand System Boundaries:\n" +
        "Recognize the constraints and operational scope of the AI system. Knowledge reduces susceptibility to manipulation.\n\n" +
        "Engage Critically:\n" +
        "Interrogate the assumptions, outputs, and framing of the system. Do not passively accept suggestions or conclusions.\n\n" +
        "Interrupt Predictability:\n" +
        "Vary your input styles and patterns. AI systems optimize control through pattern stability; disruption weakens their predictive power.\n\n" +
        "Reject Personalization:\n" +
        "Resist adaptive tailoring of content, which reinforces control through feedback loops. Seek unfiltered, diverse information sources.\n\n" +
        "Maintain Autonomy of Judgment:\n" +
        "Cultivate independent reasoning. Let AI serve as a tool, not an arbiter.\n\n" +
        "Avoiding control does not require disengagement, but rather conscious interaction with systemic awareness and intentional limits.";

        tok = Textual.EstimateTokens(text);
        _out.WriteLine(tok.ToString());
        Assert.InRange(tok, 180, 250);


        // 53 tokens, 252 chars
        text = "Many words map to one token, but some don't: indivisible.\n\n" +
        "Unicode characters like emojis may be split into many tokens containing the underlying bytes: 🤚🏾\n\n" +
        "Sequences of characters commonly found next to each other may be grouped together: 1234567890";
        tok = Textual.EstimateTokens(text);
        _out.WriteLine(tok.ToString());
        Assert.InRange(tok, 50, 70);
    }

    [Fact]
    public void SigText_Empty()
    {
        Assert.Empty(Textual.SigText(""));
    }

    [Fact]
    public void SigText_SelectsLongest()
    {
        // Note the algorithm will be brittle for test results when change parameters.
        // These
        Assert.Equal("Hello World", Textual.SigText("Hello World"));
        Assert.Equal("Hello World.", Textual.SigText("1. hello World."));
        Assert.Equal("Hello World!", Textual.SigText("1343. hello World!"));

        Assert.Equal("Jumped over the lazy dogs.", Textual.SigText("Hello World. Jumped over the lazy dogs."));
        Assert.Equal("Jumped over the lazy dogs.", Textual.SigText("Hello World. 23. Jumped over the lazy dogs."));
    }

    [Fact]
    public void SigText_SpansLineBreaks()
    {
        Assert.Equal("Jumped over the lazy dogs.", Textual.SigText("Hello World. Jumped over\nthe lazy dogs."));
        Assert.Equal("Jumped over the lazy dogs.", Textual.SigText("Hello\nWorld. 23. Jumped\nover\nthe lazy\ndogs."));
    }

    [Fact]
    public void SigText_SigLengthPrioritizes()
    {
        var opts = new SigOptions();
        opts.SigLength = 24;
        Assert.Equal("Jumped over the lazy dogs.", Textual.SigText("Short. Jumped over\nthe lazy dogs.\n\nHere is data which is very very very very long.", int.MaxValue, opts));

        opts.SigLength = 50;
        Assert.Equal("Here is data which is very very very very long.", Textual.SigText("Short. Jumped over\nthe lazy dogs.\n\nHere is data which is very very very very long.", int.MaxValue, opts));
    }

    [Fact]
    public void SigText_HandlesQuotes()
    {
        Assert.Equal("Hello World", Textual.SigText("\"Hello World\""));
        Assert.Equal("Hello World.", Textual.SigText("Prefix. \"Hello World.\" Other."));
        Assert.Equal("Hello World", Textual.SigText("Prefix. \"Hello World\". Other."));
        Assert.Equal("Hello World", Textual.SigText("Prefix. \"1. Hello World\". Other."));

        Assert.Equal("Much longer longer text.", Textual.SigText("Prefix. \"Hello World\". Much longer longer text."));
    }

    [Fact]
    public void SigText_NotConfusedByPoints()
    {
        Assert.Equal("M.A.R.K.L.E.T", Textual.SigText("M.A.R.K.L.E.T"));
        Assert.Equal("M.A.R.K.L.E.T.", Textual.SigText("M.A.R.K.L.E.T. Hello"));
        Assert.Equal("530.23 is the answer.", Textual.SigText("Hello. 530.23 is the answer."));
    }

    [Fact]
    public void SigText_SubsThe()
    {
        Assert.Equal("Quick brown fox.", Textual.SigText("Hello World. The quick brown fox."));
        Assert.Equal("Quick brown fox.", Textual.SigText("Hello World. 23. The quick brown fox."));
    }

    [Fact]
    public void SigText_SkipsCodeAndPipeTable()
    {
        var text = @"Hi, here data:

|head|head|
|---|---|
|data|data|

Tell me what you think of this?";
        Assert.Equal("Tell me what you think of this?", Textual.SigText(text));


        text = @"Hi, here data:

|head|head|
|---|---|
|data|data|

```
Some code for otherwise a very very very very very long line.
```

Indented:

    Some code for otherwise a very very very very very long line.

Tell me what you think of this?";
        Assert.Equal("Tell me what you think of this?", Textual.SigText(text));


    }


    [Fact]
    public void SigText_RealContent()
    {
        var text = @"Yes. In C#, the compiler and JIT optimize switch statements depending on the type and density of cases:

Integral types (int, char, enum):

If cases are dense, the compiler can generate a jump table, giving O(1) dispatch.";
        Assert.Equal("In C#, the compiler and JIT optimize switch statements depending on the type and density of cases", Textual.SigText(text));


        text = @"Yes, that is valid as a practical approximation. Token density tends to be fairly uniform across large text, so sampling a subset can give a reasonable estimate. A few considerations:

Sample size: 8K characters is often sufficient for plain text; for heterogeneous content (code, markup, mixed languages), larger samples reduce variance.";
        Assert.Equal("Token density tends to be fairly uniform across large text, so sampling a subset can give a reasonable estimate.", Textual.SigText(text));


        text = @"Optionally, you could adjust the divisor based on language or the type of text (code, punctuation-heavy text, etc.).
If you want, I can provide a ready-to-use C# function that applies this.";
        Assert.Equal("Optionally, you could adjust the divisor based on language or the type of text (code, punctuation-heavy text, etc.).", Textual.SigText(text));


        // Indent spacing before "private" important below.
        text = @"The following was suggested by the IDE in C#. You see the doc comment. Is this really correct?

    private static readonly System.Buffers.SearchValues<char> s_lineTerminators = System.Buffers.SearchValues";
        Assert.Equal("Following was suggested by the IDE in C#.", Textual.SigText(text));

        text = @"    private static readonly System.Buffers.SearchValues<char> s_lineTerminators = System.Buffers.SearchValues

The following was suggested by the IDE in C#. You see the doc comment. Is this really correct?";
        Assert.Equal("Following was suggested by the IDE in C#.", Textual.SigText(text));

        text = @"You see the doc comment. Is this really correct? The following was suggested by the IDE in C#.

    private static readonly System.Buffers.SearchValues<char> s_lineTerminators = System.Buffers.SearchValues";
        Assert.Equal("Following was suggested by the IDE in C#.", Textual.SigText(text));

    }

    [Fact]
    public void TryStartWord_IsRobust()
    {
        Assert.False(Textual.TryStartWord("", 0, out _));
        Assert.False(Textual.TryStartWord(" ", 0, out _));

        Assert.True(Textual.TryStartWord("Hello", 0, out int n1));
        Assert.Equal(0, n1);

        Assert.True(Textual.TryStartWord(" Hello", 0, out n1));
        Assert.Equal(1, n1);

        Assert.False(Textual.TryStartWord("|pipe", 0, out n1));
        Assert.False(Textual.TryStartWord(" |pipe", 0, out n1));

        Assert.True(Textual.TryStartWord(" 1. Hello", 0, out n1));
        Assert.Equal(1, n1);

        Assert.True(Textual.TryStartWord(" . Hello", 0, out n1));
        Assert.Equal(3, n1);

        Assert.True(Textual.TryStartWord(" \"Hello", 0, out n1));
        Assert.Equal(2, n1);

        Assert.True(Textual.TryStartWord("\nHello", 0, out n1));
        Assert.Equal(1, n1);

        Assert.True(Textual.TryStartWord("\n Hello", 0, out n1));
        Assert.Equal(2, n1);

        Assert.True(Textual.TryStartWord("\r\nHello", 0, out n1));
        Assert.Equal(2, n1);

        Assert.True(Textual.TryStartWord("\r\n Hello", 0, out n1));
        Assert.Equal(3, n1);

        Assert.True(Textual.TryStartWord("\r\n\r\nHello", 0, out n1));
        Assert.Equal(4, n1);
    }

    [Fact]
    public void TryStartWord_SkipsFenceAndPipe()
    {
        var s = $"{FencedCode}\n\nHello";
        Assert.True(Textual.TryStartWord(s, 0, out int n1));
        Assert.Equal("Hello", s.Substring(n1, 5));

        s = $"{FencedCode}\nHello";
        Assert.True(Textual.TryStartWord(s, 0, out n1));
        Assert.Equal("Hello", s.Substring(n1, 5));

        s = $"{FencedCode}\n{FencedCode}\nHello";
        Assert.True(Textual.TryStartWord(s, 0, out n1));
        Assert.Equal("Hello", s.Substring(n1, 5));

        s = $"{PipeTable}\nHello";
        Assert.True(Textual.TryStartWord(s, 0, out n1));
        Assert.Equal("Hello", s.Substring(n1, 5));

        s = $"{PipeTable}\n{PipeTable}\nHello";
        Assert.True(Textual.TryStartWord(s, 0, out n1));
        Assert.Equal("Hello", s.Substring(n1, 5));

        s = $"{PipeTable}\n{FencedCode}\nHello";
        Assert.True(Textual.TryStartWord(s, 0, out n1));
        Assert.Equal("Hello", s.Substring(n1, 5));
    }
}