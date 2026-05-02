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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Display name form.
/// </summary>
public enum DisplayStyle
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
    public static string DisplayName(this DeckFormat src, DisplayStyle style, EphemeralStatus ephemeral = EphemeralStatus.Persistant)
    {
        // Follow type value for name
        var s = src.ToString();

        if (src == DeckFormat.None)
        {
            s = "Item";
        }

        if (ephemeral == EphemeralStatus.Explicit)
        {
            s = "Ephemeral " + s;
        }

        switch (style)
        {
            case DisplayStyle.Plural:
                return s + "s";
            case DisplayStyle.Lower:
                return s.ToLowerInvariant();
            case DisplayStyle.Upper:
                return s.ToUpperInvariant();
            case DisplayStyle.LowerPlural:
                return (s + "s").ToLowerInvariant();
            default:
                return s;
        }
    }

    /// <summary>
    /// Gets a Material icon symbol for the <see cref="DeckFormat"/> value.
    /// </summary>
    public static string? MaterialSymbol(this DeckFormat src, EphemeralStatus ephemeral)
    {
        if (ephemeral == EphemeralStatus.Explicit)
        {
            return Symbols.HourglassBottom;
        }

        switch (src)
        {
            case DeckFormat.Chat:
                return Symbols.Chat;
            case DeckFormat.Note:
                return Symbols.EditNote;
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets whether the data is loaded from <see cref="DatabaseSettings.DefaultPath"/>.
    /// </summary>
    public static bool IsDefault(this MemoryGarden src)
    {
        return src.Provider?.Source == DatabaseSettings.DefaultPath;
    }

    /// <summary>
    /// Opens (or re-opens) <see cref="MemoryGarden"/> showing an error message dialogue on failure..
    /// </summary>
    /// <remarks>
    /// The result is true if the database is open on return. if "source" is null, setting path is used.
    /// </remarks>
    public static async Task<bool> OpenAsync(this MemoryGarden src, object? visual, string? source = null)
    {
        string? details;
        source ??= DatabaseSettings.Global.GetActualPath();

        try
        {
            if (src.Open(new SqliteProvider(source, ProviderFlags.WalNormal)).IsOpen())
            {
                return true;
            }

            details = src.Status.ToMessage();
            return true;
        }
        catch (Exception e)
        {
            details = e.Message;
        }

        if (visual != null)
        {
            var dialog = CreatePathDialog("Failed to open database", details, src.Provider?.Source ?? source);
            await dialog.ShowDialog(visual);
        }

        return false;
    }

    private static ChromeDialog CreatePathDialog(string? message, string? details, string? source)
    {
        var obj = new ChromeDialog();
        obj.Message = message;
        obj.Details = details;
        obj.Buttons = DialogButtons.Ok;

        if (!string.IsNullOrEmpty(source))
        {
            var text = new PixieSelectableText();
            obj.Children.Add(text);

            text.LeftSymbol = Symbols.Database;
            text.Text = source;

            text.RightButton.IsVisible = true;
            text.RightButton.Content = Symbols.ContentCopy;
            text.RightButton.Click += (_, __) => text.CopyToClipboard(text.Text);

            text.Footer = "Ensure full access to the database location";
        }

        return obj;
    }
}