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

namespace KuiperZone.Marklet.Tooling.Test;

/// <summary>
/// These test were written by AI as way of experimentation. Suspect doing this more will lead to brittle tests that are
/// poorly understood in the future.
/// </summary>
public class TextualSearchTest
{

    [Theory]
    [InlineData("hello world this is a test", "world", SearchFlags.None, 6)]
    [InlineData("Hello World hello again", "hello", SearchFlags.IgnoreCase, 0)]
    [InlineData("The quick brown fox jumps", "quick", SearchFlags.IgnoreCase, 4)]
    public void Search_FindsFirstOccurrence(string text, string sub, SearchFlags flags, int expected)
    {
        int result = text.Search(sub, flags);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Search_WholeWordRespectsBoundaries()
    {
        string text = "cat dog catalog scatter category cat.";
        Assert.Equal(0, text.Search("cat", SearchFlags.Word));
    }

    [Theory]
    [InlineData("One two three four five six.", "three", 40, "two three four five six.")]
    [InlineData("Hello world. This is a nice sentence here.", "nice", 45, "This is a nice sentence here.")]
    [InlineData("Short. Very short. Target is here now.", "Target", 35, "Target is here now.")]
    public void PrettySearch_SnapsBackToSentenceStartWhenPossible(
        string text,
        string sub,
        int maxLength,
        string expectedSubstring)
    {
        var snippet = text.PrettySearch(sub, maxLength, SearchFlags.None);
        Assert.NotNull(snippet);
        Assert.Contains(expectedSubstring, snippet);
        Assert.True(snippet.Length <= maxLength);
    }

    [Fact]
    public void PrettySearch_AtBeginningReturnsFromIndexZero()
    {
        string text = "Target starts right at the beginning of the text.";
        var snippet = text.PrettySearch("Target", 60, SearchFlags.None);

        Assert.NotNull(snippet);
        Assert.StartsWith("Target starts right", snippet);
        // The explicit n==0 check should ensure we don't skip position 0
    }

    [Fact]
    public void PrettySearch_ReturnsZeroWhenNoBreakFoundBeforeIndex()
    {
        // No punctuation or line breaks in the look-back window → should start from 0
        string text = "ThisIsAVeryLongContinuousStringWithoutAnyBreaksBeforeTheTargetHereAndThenSomeMoreTextAfter";
        var snippet = text.PrettySearch("Target", 50, SearchFlags.None);

        Assert.NotNull(snippet);
        Assert.Equal("TargetHereAndThenSomeMoreTextAfter", snippet);
    }

    [Fact]
    public void PrettySearch_UsesLimitedLookbackMaxBack()
    {
        // More than 24 chars of non-breaking text before the match
        string prefix = new('a', 30);           // > MaxBack
        string text = prefix + " some words target here after";

        var snippet = text.PrettySearch("target", 60, SearchFlags.None);

        // Should **not** go all the way back to index 0 — limited by MaxBack
        Assert.NotNull(snippet);
        Assert.DoesNotContain("aaaaaaaaaa", snippet.Substring(0, 10)); // not from very start
        Assert.Contains("some words target here", snippet);
    }

    [Theory]
    [InlineData("Line one. Line two.  Line three with target inside.", "target", "Line three with target inside.")]
    [InlineData("Hello!\n\nAnother paragraph. Target appears here.", "Target", "Target appears here.")]
    public void PrettySearch_PrefersSentenceBoundaryAfterSpace(
        string text,
        string sub,
        string expectedContains)
    {
        var snippet = text.PrettySearch(sub, 70, SearchFlags.None);
        Assert.NotNull(snippet);
        Assert.Contains(expectedContains, snippet);
    }

    [Fact]
    public void PrettySearch_ReturnsNullOnNoMatch()
    {
        Assert.Null("Nothing to see here.".PrettySearch("missingterm", 50, SearchFlags.None));
        Assert.Null("".PrettySearch("x", 20, SearchFlags.None));
    }

    [Fact]
    public void PrettySearch_IndexAndSnippetAgree()
    {
        string text = "Mary had a little lamb, little lamb, little lamb.";
        string sub = "little";

        int idx = text.Search(sub, SearchFlags.Word);
        Assert.Equal(11, idx);

        var snippet = text.PrettySearch(sub, 50, SearchFlags.Word);
        Assert.NotNull(snippet);
        Assert.Contains("little lamb", snippet);
    }
}