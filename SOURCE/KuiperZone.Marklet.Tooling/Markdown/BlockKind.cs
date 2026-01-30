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

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <summary>
/// Markdown block kind.
/// </summary>
public enum BlockKind
{
    /// <summary>
    /// Default paragraph.
    /// </summary>
    Para = 0,

    /// <summary>
    /// Heading level 1.
    /// </summary>
    H1,

    /// <summary>
    /// Heading level 2.
    /// </summary>
    H2,

    /// <summary>
    /// Heading level 3.
    /// </summary>
    H3,

    /// <summary>
    /// Heading level 4.
    /// </summary>
    H4,

    /// <summary>
    /// Heading level 5.
    /// </summary>
    H5,

    /// <summary>
    /// Heading level 6.
    /// </summary>
    H6,

    /// <summary>
    /// Horizontal rule.
    /// </summary>
    Rule,

    /// <summary>
    /// Fenced code block.
    /// </summary>
    FencedCode,

    /// <summary>
    /// Indented code block.
    /// </summary>
    IndentedCode,

    /// <summary>
    /// Table padded with spaces.
    /// </summary>
    TableCode,

    /// <summary>
    /// Math code block.
    /// </summary>
    MathCode,
}

/// <summary>
/// Markdown extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns true if the kind is a heading.
    /// </summary>
    public static bool IsHeading(this BlockKind src)
    {
        return src >= BlockKind.H1 && src <= BlockKind.H6;
    }

    /// <summary>
    /// Returns true if the kind is considered text, including headings.
    /// </summary>
    public static bool IsText(this BlockKind src)
    {
        return src == BlockKind.Para || IsHeading(src);
    }

    /// <summary>
    /// Returns true if the kind is preformatted code.
    /// </summary>
    public static bool IsCode(this BlockKind src, bool accoutrements = true)
    {
        return src == BlockKind.FencedCode || src == BlockKind.IndentedCode ||
            (accoutrements && (src == BlockKind.TableCode || src == BlockKind.MathCode));
    }

    /// <summary>
    /// Returns true if the kind is expected to have indentation added.
    /// </summary>
    public static bool IsIndented(this BlockKind src)
    {
        return src == BlockKind.IndentedCode || src == BlockKind.MathCode;
    }

}
