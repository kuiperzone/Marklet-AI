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

public class TextualSafeTest
{
    private readonly ITestOutputHelper _out;

    public TextualSafeTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void ContainsAt()
    {
        Assert.False(Textual.ContainsAt("Hello world", "", 0));

        Assert.True(Textual.ContainsAt("Hello world", "Hello", 0));
        Assert.False(Textual.ContainsAt("Hello world", "hello", 0));
        Assert.True(Textual.ContainsAt("Hello world", "hello", 0, StringComparison.OrdinalIgnoreCase));

        Assert.True(Textual.ContainsAt("Hello world", "world", 6));
        Assert.False(Textual.ContainsAt("Hello world", "World", 6));
        Assert.True(Textual.ContainsAt("Hello world", "WORLD", 6, StringComparison.OrdinalIgnoreCase));

        Assert.False(Textual.ContainsAt("Hello world", "World", 0));
        Assert.False(Textual.ContainsAt("Hello world", "World", 7));
        Assert.False(Textual.ContainsAt("Hello world", "World", -1));
        Assert.False(Textual.ContainsAt("Hello world", "World", 20));
    }


}