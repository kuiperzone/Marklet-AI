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

using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Search options.
/// </summary>
public sealed class SearchOptions
{
    /// <summary>
    /// Maximum length of <see cref="Subtext"/>.
    /// </summary>
    public const int MaxSubLength = MemoryGarden.MaxMetaLength;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// A null or empty string will assign empty to <see cref="Subtext"/>. This will match no results.
    /// </remarks>
    public SearchOptions(string? subtext)
    {
        Subtext = MemoryGarden.Sanitize(subtext, MaxSubLength) ?? "";
    }

    /// <summary>
    /// Gets the text substring for which to search.
    /// </summary>
    /// <remarks>
    /// The value is NORM-C sanitized and trimmed on construction. It cannot exceed <see cref="MaxSubLength"/> in
    /// length.
    /// </remarks>
    public string Subtext { get; }

    /// <summary>
    /// Gets or sets the maximum "snippet" length.
    /// </summary>
    public int MaxSnippet { get; set; } = MemoryGarden.MaxMetaLength;

    /// <summary>
    /// Gets or sets the maximum number of characters in content string to search.
    /// </summary>
    /// <remarks>
    /// Searching within a content string is truncated at this limit.
    /// </remarks>
    public int ScanLimit { get; set; } = 64 * 1024;

    /// <summary>
    /// Gets or sets the maximum result count.
    /// </summary>
    /// <remarks>
    /// Search is terminated when this limited is reached.
    /// </remarks>
    public int MaxResults { get; set; } = 100;

    /// <summary>
    /// Gets or sets search option flags.
    /// </summary>
    public SearchFlags Flags { get; set; }
}
