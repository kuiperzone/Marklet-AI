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

using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Display name form.
/// </summary>
public enum DisplayKind
{
    /// <summary>
    /// Default singular title case.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Plural title case.
    /// </summary>
    Plural,

    /// <summary>
    /// Singular lowercase.
    /// </summary>
    Lower,

    /// <summary>
    /// Singular uppercase.
    /// </summary>
    Upper,

    /// <summary>
    /// Lower case plural.
    /// </summary>
    LowerPlural,
}

/// <summary>
/// Extension methods for application.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets the display name string.
    /// </summary>
    public static string DisplayName(this BasketKind src)
    {
        if (src == BasketKind.None)
        {
            return "Search";
        }

        // Follow type value for name
        return src.ToString();
    }

    /// <summary>
    /// Gets a Material icon symbol for the <see cref="BasketKind"/> value.
    /// </summary>
    public static string? MaterialSymbol(this BasketKind src)
    {
        switch (src)
        {
            case BasketKind.Recent:
                return Symbols.Home;
            case BasketKind.Notes:
                return Symbols.EditNote;
            case BasketKind.Archive:
                return Symbols.Archive;
            case BasketKind.Waste:
                return Symbols.Delete;
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the display name string.
    /// </summary>
    public static string DisplayName(this DeckKind src, DisplayKind kind, bool ephemeral = false)
    {
        // Follow type value for name
        var s = src.ToString();

        if (src == DeckKind.None)
        {
            s = "Item";
        }

        if (ephemeral)
        {
            s = "Ephemeral " + s;
        }

        switch (kind)
        {
            case DisplayKind.Plural:
                return s + "s";
            case DisplayKind.Lower:
                return s.ToLowerInvariant();
            case DisplayKind.Upper:
                return s.ToUpperInvariant();
            case DisplayKind.LowerPlural:
                return (s + "s").ToLowerInvariant();
            default:
                return s;
        }
    }

    /// <summary>
    /// Gets a Material icon symbol for the <see cref="DeckKind"/> value.
    /// </summary>
    public static string? MaterialSymbol(this DeckKind src, bool ephemeral)
    {
        if (ephemeral)
        {
            return Symbols.HourglassBottom;
        }
        switch (src)
        {
            case DeckKind.Chat:
                return Symbols.Chat;
            case DeckKind.Note:
                return Symbols.EditNote;
            default:
                return null;
        }
    }

}