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

using KuiperZone.Marklet.Tooling.Markdown.Internal;

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <summary>
/// Immutable content item containing text, styling, and optional link.
/// </summary>
public sealed class MarkElement : IEquatable<MarkElement>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public MarkElement()
    {
        Text = "";
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// No validation is performed on "text" and it should not, therefore, contain markdown styling syntax. Newline
    /// characters may be used, but they must be Linux '\'n' newlines only.
    /// </remarks>
    public MarkElement(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// No validation is performed on "text" and it should not, therefore, contain markdown styling syntax. Newline
    /// characters may be used, but they must be Linux '\'n' newlines only.
    /// </remarks>
    public MarkElement(string text, LinkInfo? link)
    {
        Text = text;
        Link = link;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// No validation is performed on "text" and it should not, therefore, contain markdown styling syntax. Newline
    /// characters may be used, but they must be Linux '\'n' newlines only.
    /// </remarks>
    public MarkElement(string text, InlineStyling styling, LinkInfo? link = null)
    {
        Text = text;
        Styling = styling;
        Link = link;
    }

    /// <summary>
    /// Constructor assigns "text", but with styling and link from "other".
    /// </summary>
    public MarkElement(string text, MarkElement other)
    {
        Text = text;
        Styling = other.Styling;
        Link = other.Link;
    }

    /// <summary>
    /// A static instance containing a newline only.
    /// </summary>
    public static readonly MarkElement Newline = new("\n");

    /// <summary>
    /// Gets the text fragment.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the style flags.
    /// </summary>
    public InlineStyling Styling { get; }

    /// <summary>
    /// Gets the link.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    public LinkInfo? Link { get; }

    /// <summary>
    /// Returns true if the element is "empty".
    /// </summary>
    /// An element is empty if <see cref="Text"/> is empty and <see cref="Link"/> is null.
    public bool IsEmpty
    {
        get { return Text.Length == 0 && Link == null; }
    }

    /// <summary>
    /// Overload of <see cref="Split(int, int, InlineStyling, bool)"/> where all items with have the same <see
    /// cref="Styling"/> value.
    /// </summary>
    /// <remarks>
    /// This variant will return a maximum of two items.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">index or length</exception>
    public MarkElement[] Split(int index, bool preserveLink = true)
    {
        return Split(index, Text.Length - index, Styling, preserveLink);
    }

    /// <summary>
    /// Overload of <see cref="Split(int, int, InlineStyling, bool)"/>.
    /// </summary>
    /// <remarks>
    /// This variant will return a maximum of two items.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">index or length</exception>
    public MarkElement[] Split(int index, InlineStyling styling, bool preserveLink = true)
    {
        return Split(index, Text.Length - index, styling, preserveLink);
    }

    /// <summary>
    /// Overload of <see cref="Split(int, int, InlineStyling, bool)"/> where all items with have the same <see
    /// cref="Styling"/> value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">index or length</exception>
    public MarkElement[] Split(int index, int length, bool preserveLink = true)
    {
        return Split(index, length, Styling, preserveLink);
    }

    /// <summary>
    /// Splits the instance in to a maximum of 3 elements.
    /// </summary>
    /// <remarks>
    /// The <see cref="Text"/> is split between [0, index), [index, length) and [length, Text.Length). The "midStyling"
    /// value is always assigned to the center region defined by [index, length). If "index" is 0, the first element is
    /// omitted. Likewise, if "length" extends to the end of <see cref="Text"/>, the last item is omitted. The resulting
    /// array will always contain between 1 and 3 items, with a single item returned where "index" equals 0 and "length"
    /// equals the <see cref="Text"/> length. The <see cref="Link"/> value is assigned to all resulting items if
    /// "preserveLink" is true.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">index or length</exception>
    public MarkElement[] Split(int index, int length, InlineStyling midStyling, bool preserveLink = true)
    {
        var link = preserveLink ? Link : null;

        int count = 1;

        if (index > 0)
        {
            count += 1;
        }

        if (length < Text.Length)
        {
            count += 1;
        }

        var array = new MarkElement[count];

        if (count == 1 && midStyling == Styling && link == Link)
        {
            array[0] = this;
            return array;
        }

        count = 0;

        if (index > 0)
        {
            array[count++] = new MarkElement(Text.Substring(0, index), Styling, link);
        }

        array[count++] = new MarkElement(Text.Substring(index, length), midStyling, link);

        if (length < Text.Length)
        {
            array[count] = new MarkElement(Text.Substring(length), Styling, link);
        }

        return array;
    }

    /// <summary>
    /// Overload of <see cref="SplitLines(InlineStyling, bool)"/> where all items have the <see cref="Styling"/> value.
    /// </summary>
    public MarkElement[] SplitLines(bool trim = false)
    {
        return SplitLines(Styling, trim);
    }

    /// <summary>
    /// Splits <see cref="Text"/> at Linux newline characters and returns an array containing at least one item with the
    /// same <see cref="Styling"/> value.
    /// </summary>
    /// <remarks>
    /// The "styling" value is assigned to all items, along with the <see cref="Link"/> instance of "this". If <see
    /// cref="Text"/> contains no newline characters, the result is an array with a single item. Item text is trimmed
    /// left and right if "trim" is true.
    /// </remarks>
    public MarkElement[] SplitLines(InlineStyling styling, bool trim = false)
    {
        var lines = Text.Split('\n', trim ? StringSplitOptions.TrimEntries : StringSplitOptions.None);
        var array = new MarkElement[lines.Length];

        if (lines.Length == 1 && Styling == styling && !trim)
        {
            array[0] = this;
            return array;
        }

        for (int n = 0; n < lines.Length; ++n)
        {
            array[n] = new MarkElement(lines[n], styling, Link);
        }

        return array;
    }

    /// <summary>
    /// Returns a string in the given "format".
    /// </summary>
    public string ToString(TextFormat format)
    {
        switch (format)
        {
            case TextFormat.Unicode:
                return new PlainElementWriter(this).ToString();
            case TextFormat.Html:
                return new HtmlElementWriter(this).ToString();
            default:
                return new MarkElementWriter(this).ToString();
        }
    }

    /// <summary>
    /// Equivalent to <see cref="ToString(TextFormat)"/> with <see cref="TextFormat.Markdown"/>.
    /// </summary>
    public override string ToString()
    {
        return new MarkElementWriter(this).ToString();
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(MarkElement? other)
    {
        if (other == this)
        {
            return true;
        }

        return other != null && other.Text == Text && other.Styling == Styling && LinkInfo.Equals(other.Link, Link);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MarkElement);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Text, Styling, Link);
    }

}
