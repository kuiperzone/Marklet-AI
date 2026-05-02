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
/// Existing values are written to storage and must not change.
/// </remarks>
public enum BasketKind : byte
{
    /// <summary>
    /// None. Invalid or other non-basket ID.
    /// </summary>
    None = 0,

    /// <summary>
    /// Recent basket.
    /// </summary>
    Recent = 1,

    /// <summary>
    /// Notes basket.
    /// </summary>
    Notes = 2,

    /// <summary>
    /// Archive basket.
    /// </summary>
    Archive = 3,

    /// <summary>
    /// Wastebasket.
    /// </summary>
    Waste = 100, // <- explicit value (always last)
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets whether the value is legal for <see cref="GardenDeck.Format"/>.
    /// </summary>
    public static bool IsLegal(this BasketKind src)
    {
        // This is for compatibility across different version
        return (src > BasketKind.None && src <= BasketKind.Archive) || src == BasketKind.Waste;
    }

    /// <summary>
    /// Throws if <see cref="IsLegal(BasketKind)"/> is false.
    /// </summary>
    /// <exception cref="ArgumentException">Not legal</exception>
    public static void ThrowIfNotLegal(this BasketKind src, string? paramName = null)
    {
        if (!IsLegal(src))
        {
            throw new ArgumentException($"Not legal {nameof(BasketKind)} {src}", paramName);
        }
    }

    /// <summary>
    /// Gets the default or initial basket for this <see cref="DeckFormat"/> value.
    /// </summary>
    public static DeckFormat DefaultDeck(this BasketKind src)
    {
        switch (src)
        {
            case BasketKind.Recent:
                return DeckFormat.Chat;
            case BasketKind.Notes:
                return DeckFormat.Note;
            default:
                return DeckFormat.None;
        }
    }

    /// <summary>
    /// Gets whether new conversions can be instigated from the given basket (i.e. has a "New Chat" button").
    /// </summary>
    public static bool CanInstigateNew(this BasketKind src)
    {
        return src == BasketKind.Recent || src == BasketKind.Notes;
    }

    /// <summary>
    /// Gets whether user can reply to existing conversions.
    /// </summary>
    public static bool CanReply(this BasketKind src)
    {
        return src != BasketKind.Waste && IsLegal(src);
    }

    /// <summary>
    /// Gets whether the basket may content mixed <see cref="DeckFormat"/> values.
    /// </summary>
    public static bool IsMixedContent(this BasketKind src)
    {
        // Semantic shortcut TBD
        return DefaultDeck(src) == DeckFormat.None;
    }

}