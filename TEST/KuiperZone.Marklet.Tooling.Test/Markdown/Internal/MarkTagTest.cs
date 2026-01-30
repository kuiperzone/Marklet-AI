// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Headroom
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

using KuiperZone.Marklet.Tooling.Markdown.Internal;
using Xunit.Abstractions;

namespace KuiperZone.Marklet.Tooling.Test;

public class MarkTagTest : BaseTest
{
    private readonly ITestOutputHelper _out;

    public MarkTagTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void TryTagAt_IsComplete()
    {
        var obj = TryTagAt("<b>", 0);
        Assert.NotNull(obj);
        Assert.Equal("b", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(3, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<code> ", 4);
        Assert.NotNull(obj);
        Assert.Equal("code", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(6, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<code >", 4);
        Assert.NotNull(obj);
        Assert.Equal("code", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(7, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<a href=\" \">", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(12, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Equal("href=\" \"", obj.Attributes);

        obj = TryTagAt("Text<a href=\"h\">", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(12, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Equal("href=\"h\"", obj.Attributes);

        obj = TryTagAt("Text<a href=\"http://local\" style=\"font:bold\">", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(41, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Equal("href=\"http://local\" style=\"font:bold\"", obj.Attributes);

        obj = TryTagAt("Text<a href = \"http://local\" >", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(26, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Equal("href = \"http://local\"", obj.Attributes);

        // Closed
        obj = TryTagAt("</b>", 0);
        Assert.NotNull(obj);
        Assert.Equal("b", obj.Name);
        Assert.False(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(4, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("</xyz> world", 0);
        Assert.NotNull(obj);
        Assert.Equal("xyz", obj.Name);
        Assert.False(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(6, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        // Self close
        obj = TryTagAt("<b/>", 0);
        Assert.NotNull(obj);
        Assert.Equal("b", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(4, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("<code />", 0);
        Assert.NotNull(obj);
        Assert.Equal("code", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(8, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<code />", 4);
        Assert.NotNull(obj);
        Assert.Equal("code", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(8, obj.Length);
        Assert.True(obj.IsComplete);

        obj = TryTagAt("Text<a href = \"http://local\"\t/>", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(27, obj.Length);
        Assert.True(obj.IsComplete);
        Assert.Equal("href = \"http://local\"", obj.Attributes);
    }

    [Fact]
    public void TryTagAt_IsIncomplete()
    {
        var obj = TryTagAt("<b", 0);
        Assert.NotNull(obj);
        Assert.Equal("b", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(3, obj.Length);
        Assert.False(obj.IsComplete); // <- not complete
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<code ", 4);
        Assert.NotNull(obj);
        Assert.Equal("code", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(6, obj.Length);
        Assert.False(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<a href=\" \"", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(3, obj.Length); // <- correct given incomplete
        Assert.False(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<a href=\"http://local\" style=\"font:bold\"\t", 4);
        Assert.NotNull(obj);
        Assert.Equal("a", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(3, obj.Length);
        Assert.False(obj.IsComplete);
        Assert.Null(obj.Attributes);

        // Close
        obj = TryTagAt("</b", 0);
        Assert.NotNull(obj);
        Assert.Equal("b", obj.Name);
        Assert.False(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(4, obj.Length);
        Assert.False(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("</code", 0);
        Assert.NotNull(obj);
        Assert.Equal("cod", obj.Name);
        Assert.False(obj.IsOpen);
        Assert.True(obj.IsClose);
        Assert.Equal(6, obj.Length);
        Assert.False(obj.IsComplete);
        Assert.Null(obj.Attributes);

        obj = TryTagAt("Text<code ", 4);
        Assert.NotNull(obj);
        Assert.Equal("code", obj.Name);
        Assert.True(obj.IsOpen);
        Assert.False(obj.IsClose);
        Assert.Equal(6, obj.Length);
        Assert.False(obj.IsComplete);
        Assert.Null(obj.Attributes);
    }

    [Fact]
    public void TryTagAt_ParseAttributes()
    {
        var obj = MarkTag.TryTagAt("<a href=\"http://local\" style=\"font:bold\">Text</a>", 0);
        Assert.NotNull(obj);
        Assert.Equal("href=\"http://local\" style=\"font:bold\"", obj.Attributes);
        Assert.Equal("http://local", obj.GetAttrib("href"));
        Assert.Equal("font:bold", obj.GetAttrib("style"));
        Assert.Null(obj.GetAttrib(""));
        Assert.Null(obj.GetAttrib("other"));

        // Single quote
        obj = MarkTag.TryTagAt("<a href = 'http://local'>", 0);
        Assert.NotNull(obj);
        Assert.Equal("http://local", obj.GetAttrib("href"));

        // Empty
        obj = MarkTag.TryTagAt("<a href = ''>", 0);
        Assert.NotNull(obj);
        Assert.Equal("", obj.GetAttrib("href"));

        // Escaped
        obj = MarkTag.TryTagAt("<a href = \"http://local\\\"escaped\">", 0);
        Assert.NotNull(obj);
        Assert.Equal("http://local\\\"escaped", obj.GetAttrib("href"));

        // Failed
        obj = MarkTag.TryTagAt("<a href = >", 0);
        Assert.NotNull(obj);
        Assert.Null(obj.GetAttrib("href"));

        obj = MarkTag.TryTagAt("<a href = \">", 0);
        Assert.NotNull(obj);
        Assert.Null(obj.GetAttrib("href"));

        obj = MarkTag.TryTagAt("<a href=''", 0);
        Assert.NotNull(obj);
        Assert.Null(obj.GetAttrib("href"));
    }

    [Fact]
    public void TryTagAt_Null()
    {
        var obj = TryTagAt("", 0);
        Assert.Null(obj);

        obj = TryTagAt("b>", 0);
        Assert.Null(obj);

        obj = TryTagAt("\\<b>", 1);
        Assert.Null(obj);

        obj = TryTagAt("A\\<b>", 2);
        Assert.Null(obj);
    }

    [Fact]
    public void FindClose_IsValid()
    {
        Assert.Equal(0, MarkTag.FindClose("</b>", "b", 0));
        Assert.Equal(0, MarkTag.FindClose("</code>", "code", 0));
        Assert.Equal(6, MarkTag.FindClose("012345</Code>", "code", 0));
        Assert.Equal(7, MarkTag.FindClose("<Code>A</code>", "code", 1));
        Assert.Equal(8, MarkTag.FindClose("\\<Code>A</code>", "code", 0));
        Assert.Equal(8, MarkTag.FindClose("\\<Code>A</code>", "code", 1));
        Assert.Equal(-1, MarkTag.FindClose("</coder>", "code", 0));

        // Nested
        Assert.Equal(15, MarkTag.FindClose("<i><b>A</b></i></b>", "b", 0));
        Assert.Equal(-1, MarkTag.FindClose("<b><i><b>A</b></i></b>", "b", 0));
        Assert.Equal(-1, MarkTag.FindClose("A<b><i><b>A</b></i>B</b>", "b", 0));

        Assert.Equal(-1, MarkTag.FindClose("", "", 0));
        Assert.Equal(-1, MarkTag.FindClose("", "", 1));
        Assert.Equal(-1, MarkTag.FindClose("A", "", -1));
        Assert.Equal(-1, MarkTag.FindClose("A", "B", -1));
        Assert.Equal(-1, MarkTag.FindClose("", "B", -1));
        Assert.Equal(-1, MarkTag.FindClose("<code", "code", 0));
        Assert.Equal(-1, MarkTag.FindClose("<code />", "code", 0));
    }

    [Fact]
    public void FindClose_IsNull()
    {
        Assert.Equal(-1, MarkTag.FindClose("", "", 0));
        Assert.Equal(-1, MarkTag.FindClose("", "", 1));
        Assert.Equal(-1, MarkTag.FindClose("A", "", -1));
        Assert.Equal(-1, MarkTag.FindClose("A", "B", -1));
        Assert.Equal(-1, MarkTag.FindClose("", "B", -1));
        Assert.Equal(-1, MarkTag.FindClose("<code", "code", 0));
    }

    private static MarkTag? TryTagAt(string content, int index = 0)
    {
        WriteIndentedDebug();
        WriteIndentedDebug();
        WriteIndentedDebug($"CONTENT: {content}");
        return Write(MarkTag.TryTagAt(content, index));
    }

    private static MarkTag? Write(MarkTag? obj)
    {
        const string ObjName = "obj";

        if (obj == null)
        {
            WriteAssertNull(ObjName);
            return null;
        }

        WriteAssertNotNull(ObjName);
        WriteAssertEqual(obj.Name, $"{ObjName}.{nameof(obj.Name)}");
        WriteAssertEqual(obj.IsOpen, $"{ObjName}.{nameof(obj.IsOpen)}");
        WriteAssertEqual(obj.IsClose, $"{ObjName}.{nameof(obj.IsClose)}");
        WriteAssertEqual(obj.Length, $"{ObjName}.{nameof(obj.Length)}");
        WriteAssertEqual(obj.IsComplete, $"{ObjName}.{nameof(obj.IsComplete)}");
        WriteAssertEqual(obj.Attributes, $"{ObjName}.{nameof(obj.Attributes)}");

        return obj;
    }
}