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
/// Contains a sequence of <see cref="MarkElement"/> instances.
/// </summary>
/// <remarks>
/// Also serves as a base class for <see cref="MarkBlock"/>.
/// </remarks>
public interface IReadOnlyStyledContainer : IEquatable<IReadOnlyStyledContainer>
{
    /// <summary>
    /// Gets styled elements.
    /// </summary>
    IReadOnlyList<MarkElement> Elements { get; }

    /// <summary>
    /// Returns true if the instance has no displayable content.
    /// </summary>
    /// <remarks>
    /// The result is not the same as checking that the <see cref="Elements"/> count is equal to 0.
    /// </remarks>
    bool IsEmpty();

    /// <summary>
    /// Returns the display width of the text content in either characters or "graphemes" (user perceived characters).
    /// </summary>
    /// <remarks>
    /// The result is effectively the length of the longest line when <see cref="Elements"/> are combined into a single
    /// string, and is intended to be used in padding for fixed font output.
    /// </remarks>
    int GetWidth(bool graphemes);

    /// <summary>
    /// Equivalent to <see cref="GetWidth(bool)"/> with true.
    /// </summary>
    int GetWidth();

    /// <summary>
    /// Returns of the display height of the cell in characters.
    /// </summary>
    /// <remarks>
    /// The result is never less than 1, even where <see cref="Elements"/> is empty.
    /// </remarks>
    int GetHeight();

    /// <summary>
    /// Returns a string in the given "format".
    /// </summary>
    string ToString(TextFormat format);

    /// <summary>
    /// Equivalent to <see cref="ToString(TextFormat)"/> with <see cref="TextFormat.Markdown"/>.
    /// </summary>
    string ToString();
}
