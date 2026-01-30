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
/// Column alignment used with <see cref="MarkTable"/>.
/// </summary>
public enum ListKind
{
    /// <summary>
    /// Block is not part of a list sequence.
    /// </summary>
    None = 0,

    /// <summary>
    /// Block heads an ordered list.
    /// </summary>
    Ordered,

    /// <summary>
    /// Blocks heads a bulleted unordered list.
    /// </summary>
    Bulleted,

    /// <summary>
    /// Block is a continuation of a list.
    /// </summary>
    Continuation,
}

/// <summary>
/// Markdown extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns true if the kind is either <see cref="ListKind.Ordered"/> or <see cref="ListKind.Bulleted"/>.
    /// </summary>
    public static bool IsHead(this ListKind src)
    {
        return src == ListKind.Ordered || src == ListKind.Bulleted;
    }
}