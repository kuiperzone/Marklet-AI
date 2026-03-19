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

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Identifies the <see cref="GardenBasket"/> instance.
/// </summary>
/// <remarks>
/// Integer values are written to storage and must not change.
/// </remarks>
public enum DeckKind : byte
{
    /// <summary>
    /// None invalid.
    /// </summary>
    None = 0,

    /// <summary>
    /// Home chat.
    /// </summary>
    Chat = 1,

    /// <summary>
    /// Note data.
    /// </summary>
    Note = 2,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets whether the value is legal for <see cref="GardenDeck.Kind"/>.
    /// </summary>
    public static bool IsLegal(this DeckKind src)
    {
        return src >= DeckKind.Chat && src <= DeckKind.Note;
    }

    /// <summary>
    /// Throws if <see cref="IsLegal(DeckKind)"/> is false.
    /// </summary>
    /// <exception cref="ArgumentException">Not legal</exception>
    public static void ThrowIfNotLegal(this DeckKind src, string? paramName = null)
    {
        if (!IsLegal(src))
        {
            throw new ArgumentException($"Not legal {nameof(DeckKind)} {src}", paramName);
        }
    }

}