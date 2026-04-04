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

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <summary>
/// Markdown parsing option flags.
/// </summary>
[Flags]
public enum MarkOptions
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0x00000000,

    /// <summary>
    /// Content should be pre-sanitized prior to parsing.
    /// </summary>
    /// <remarks>
    /// Substitutes most control characters, removes private and other ranges, and Unicode FORM-C normalizes.
    /// </remarks>
    Sanitize = 0x00000001,

    /// <summary>
    /// Parse block elements.
    /// </summary>
    /// <remarks>
    /// If not specified, content is treated, not a markdown, but as raw text. Invariably both <see cref="Blocks"/> and
    /// <see cref="Inlines"/> should be specified.
    /// </remarks>
    Blocks = 0x00000002,

    /// <summary>
    /// Parse inline elements.
    /// </summary>
    /// <remarks>
    /// If not specified, inline content is ignored. It does nothing unless <see cref="Blocks"/> is given. The case for
    /// omitting it is to process preserve content verbatim where possible, while still handling fenced code and other
    /// specialized regions. Plain texts links are still detected provided <see cref="PlainLinks"/> is given. Invariably
    /// both <see cref="Blocks"/> and <see cref="Inlines"/> should be specified.
    /// </remarks>
    Inlines = 0x00000004,

    /// <summary>
    /// Combines blocks and elements of matching kinds and styles where possible in order to minimize UI controls.
    /// </summary>
    /// <remarks>
    /// The option is ignored if <see cref="Blocks"/> is omitted.
    /// </remarks>
    Coalesce = 0x00000008,

    /// <summary>
    /// Detect plain unadorned URIs.
    /// </summary>
    /// <remarks>
    /// If given, the text "http://example.com" (without "[]()" surrounding brackets) will be detected as a link. If
    /// omitted, only links expressed in markdown form will be detected. It is ignored if <see cref="Blocks"/> is
    /// omitted.
    /// </remarks>
    PlainLinks = 0x00000010,

    /// <summary>
    /// Treat inline HTML tags as code elements.
    /// </summary>
    /// <remarks>
    /// The option is ignored unless both <see cref="Blocks"/> and <see cref="Inlines"/> are given.
    /// </remarks>
    InlineHtmlAsCode = 0x00000020,

    /// <summary>
    /// Treat "![]()" images as text links.
    /// </summary>
    ImageAsLink = 0x00000040,

    /// <summary>
    /// Indicates that the content is chunking and is unfinished.
    /// </summary>
    /// <remarks>
    /// The final update should not set this flag. Not currently used, but may be in future.
    /// </remarks>
    Chunking = 0x00000080,

    /// <summary>
    /// Composite of options for regular markdown without <see cref="Sanitize"/>.
    /// </summary>
    Markdown = Blocks | Inlines | PlainLinks
}
