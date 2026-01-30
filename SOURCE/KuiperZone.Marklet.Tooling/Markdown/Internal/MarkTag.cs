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

namespace KuiperZone.Marklet.Tooling.Markdown.Internal;

internal sealed class MarkTag
{
    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Gets the tag name in lowercase.
    /// </summary>
    public string Name { get; private set; } = "";

    /// <summary>
    /// Gets whether tag is open, such as "{b}". Includes self-closing such as "{a href=""/}".
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets whether tag is closing, such as "{/b}". Includes self-closing such as "{a href=""/}".
    /// </summary>
    public bool IsClose { get; private set; }

    /// <summary>
    /// Gets whether tag is complete. An incorrectly formatted tag such as "{code" gives false.
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Gets the total length in the source text.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Gets the attribute part between the name and end.
    /// </summary>
    public string? Attributes { get; private set; }

    public static MarkTag? TryTag(string src)
    {
        return TryTagAt(src, 0);
    }

    public static MarkTag? TryTagAt(string src, int index)
    {
        // https://spec.commonmark.org/0.31.2/#html-block
        if (index < 0 || index > src.Length - 2 || src[index] != '<' || src.IsEscapedAt(index))
        {
            return null;
        }

        int n0 = index + 1;
        bool close = src[index + 1] == '/';

        if (close)
        {
            // Close </b>
            n0 += 1;
        }

        if (n0 >= src.Length || !char.IsAsciiLetter(src[n0]))
        {
            // Name must start with letter
            return null;
        }

        // Check name for end point
        int n1 = n0 + 1;

        for (int n = n0 + 1; n < src.Length; ++n)
        {
            n1 = n;
            char c = src[n];

            if (!char.IsAsciiLetterOrDigit(c))
            {
                if (c != '/' && c != '>' && !src.IsSpaceOrTabAt(n, true))
                {
                    return null;
                }

                // OK
                break;
            }
        }

        var tag = new MarkTag();
        tag.Name = src.Substring(n0, n1 - n0).ToLowerInvariant();
        tag.IsOpen = !close;
        tag.IsClose = close;
        tag.Length = n1 - index + 1;

        // <a>
        // 012
        // 123
        // Find close brace
        int ePos = src.IndexOf('>', n0 + 1);

        if (ePos < 0)
        {
            // Incomplete
            return tag;
        }

        // Self close "<hr/>"
        if (src[ePos - 1] == '/')
        {
            if (tag.IsClose)
            {
                // Invalid
                return null;
            }

            tag.IsClose = true;
        }

        tag.IsComplete = true;
        tag.Length = ePos - index + 1;

        if (tag.IsOpen)
        {
            int aPos = n1 + 1;

            if (tag.IsClose)
            {
                ePos -= 1;
            }

            int len = ePos - aPos;

            if (len > 0)
            {
                var attribs = src.Substring(aPos, len).Trim();

                if (attribs.Length != 0)
                {
                    tag.Attributes = attribs;
                }
            }
        }

        return tag;
    }

    public static int FindClose(string src, string name, int startIndex)
    {
        return FindClose(src, name, startIndex, out _);
    }

    /// <summary>
    /// Finds index of HTML close element, i.e. "{/b}", of given "name" while skipping nested elements of same name.
    /// </summary>
    public static int FindClose(string src, string name, int startIndex, out int length)
    {
        length = 0;

        int count = 0;
        int maxLen = src.Length - name.Length - 2;

        // name = 4
        // 01234567
        // </name>
        while (startIndex < maxLen)
        {
            int pos = src.IndexOf('<', startIndex, maxLen - startIndex);

            if (pos < 0)
            {
                return -1;
            }

            startIndex = pos + 1;

            if (src.IsEscapedAt(pos))
            {
                continue;
            }

            if (src[pos + 1] == '/' && src[pos + name.Length + 2] == '>')
            {
                if (string.Compare(src, pos + 2, name, 0, name.Length, IgnoreCase) == 0)
                {
                    if (count == 0)
                    {
                        // </a>
                        // 0123
                        length = name.Length + 3;
                        return pos;
                    }

                    count -= 1;
                }

                startIndex += name.Length + 2;
                continue;
            }

            if (src[pos + name.Length + 1] == '>' &&
                string.Compare(src, pos + 1, name, 0, name.Length, IgnoreCase) == 0)
            {
                count += 1;
                startIndex += name.Length + 1;
            }
        }

        return -1;
    }

    /// <summary>
    /// Get attribute by name, i.e. "href".
    /// </summary>
    public string? GetAttrib(string name)
    {
        if (Attributes == null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        int index = Attributes.IndexOf(name, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return null;
        }

        // a="" -> ""
        // a = "" -> ""
        var sub = Attributes.Substring(index + name.Length).TrimStart('=', ' ');

        if (sub.Length < 2 || (sub[0] != '"' && sub[0] != '\''))
        {
            return null;
        }

        char q = sub[0];
        sub = sub.Substring(1);

        index = 0;

        while ((index = sub.IndexOf(q, index)) > -1)
        {
            if (sub.IsEscapedAt(index))
            {
                index += 1;
                continue;
            }

            return sub.Substring(0, index);
        }

        return null;
    }

}
