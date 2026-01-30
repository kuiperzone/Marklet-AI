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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Sanitisation flags used with <see cref="Sanitizer"/>.
/// </summary>
[Flags]
public enum SanFlags
{
    /// <summary>
    /// Removes any invalid unicode surrogate pairs, unassigned characters, substitutes '\x0' with U+FFFD and
    /// substitutes Windows CR/LF pairs with the interoperable U+000A (LF) character.
    /// </summary>
    /// <remarks>
    /// Single U+000D (CR) character are always substituted for LF. If only this flag is given, content is otherwise
    /// unchanged.
    /// </remarks>
    Default = 0,

    /// <summary>
    /// String value is trimmed left and right.
    /// </summary>
    Trim = 0x0001,

    /// <summary>
    /// Substitute alternate line feed characters with one or more U+000A (LF), as appropriate.
    /// </summary>
    /// <remarks>
    /// If specified, the following are substituted with: U+000B (VT), U+000C (FF), U+0085 (NEL) and U+2029 (PS).
    /// </remarks>
    SubFeed = 0x0002,

    /// <summary>
    /// Substitute or omit control characters, except for LF and TAB and those referenced by <see cref="SubFeed"/>.
    /// </summary>
    /// <remarks>
    /// ASCII control characters, except for LF and TAB and those of <see cref="SubFeed"/>, are substituted with
    /// pictographs. All characters in the control block [U+0080, U+009F] are omitted, except for U+0085 (NEL).
    /// </remarks>
    SubControl = SubFeed | 0x0004,

    /// <summary>
    /// Substitute Unicode space characters for 0x20.
    /// </summary>
    SubSpace = 0x0008,

    /// <summary>
    /// Normalize to Unicode C form.
    /// </summary>
    NormC = 0x0100,

    /// <summary>
    /// Remove combining marks.
    /// </summary>
    /// <remarks>
    /// If used with <see cref="NormC"/>, any remaining combining marks are removed AFTER normalization.
    /// </remarks>
    NoCombining = 0x0200,
}
