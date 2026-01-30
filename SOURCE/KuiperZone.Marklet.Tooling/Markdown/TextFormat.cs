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
/// Text output format.
/// </summary>
public enum TextFormat
{
    /// <summary>
    /// Normalized markdown.
    /// </summary>
    Markdown = 0,

    /// <summary>
    /// Plain Unicode text without markdown styling.
    /// </summary>
    /// <remarks>
    /// The result is superficially similar to markdown with '#' prefixes used for headings. However, inline markdown
    /// styling is omitted and Unicode bullets are used for lists. Fenced is shown as indented, while tables are
    /// presented in a very nice way. The result cannot be parsed as markdown. The output should be rendered in
    /// monospace with Unicode support.
    /// </remarks>
    Unicode,

    /// <summary>
    /// HTML.
    /// </summary>
    Html,
}
