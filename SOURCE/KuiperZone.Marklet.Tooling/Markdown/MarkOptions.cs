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
/// Markdown parsing option flags.
/// </summary>
[Flags]
public enum MarkOptions
{
    /// <summary>
    /// Indicates the content has already been sanitized and that the sanitization step can be omitted.
    /// </summary>
    Presan = 0x00000001,

    /// <summary>
    /// Ignore inline markdown and HTML elements.
    /// </summary>
    /// <remarks>
    /// When specified, markdown is, in effect, partially processed. The use case is for processing user input where
    /// text is to preserved verbatim where possible, while still handling fenced code and other specialized regions.
    /// Plain texts links are still detected provided <see cref="IgnorePlainLinks"/> is false.
    /// </remarks>
    IgnoreInline = 0x00000002,

    /// <summary>
    /// Do not detect plain unadorned URIs.
    /// </summary>
    /// <remarks>
    /// If omitted, the text "http://example.com" (without "[]()" surrounding brackets) will be detected as a link by
    /// default. If given, only links expressed in markdown form will be detected.
    /// </remarks>
    IgnorePlainLinks = 0x00000004,

    /// <summary>
    /// Treat inline HTML tags as code elements.
    /// </summary>
    /// <remarks>
    /// The option is ignored if <see cref="IgnoreInline"/> is given.
    /// </remarks>
    InlineHtmlAsCode = 0x00000008,

    /// <summary>
    /// Treat "![]()" images as text links.
    /// </summary>
    ImageAsLink = 0x00000010,

    /// <summary>
    /// Indicates that the content is chunking and is unfinished.
    /// </summary>
    /// <remarks>
    /// The final update should not set this flag.
    /// </remarks>
    Chunking = 0x00000020,
}
