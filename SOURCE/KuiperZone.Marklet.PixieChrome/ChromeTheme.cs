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

using Avalonia.Styling;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Application theme identifier.
/// </summary>
/// <remarks>
/// Synonymous with ThemeVariant, but allows for extension. Integer values may be written to disk and should not be
/// changed.
/// </remarks>
public enum ChromeTheme
{
    /// <summary>
    /// Default.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Regular light theme.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Regular dark theme.
    /// </summary>
    Dark = 2,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns true if dark theme.
    /// </summary>
    public static bool IsDark(this ChromeTheme src)
    {
        return src == ChromeTheme.Dark;
    }

    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static ChromeTheme TrimLegal(this ChromeTheme src)
    {
        return Enum.IsDefined(src) ? src : ChromeTheme.Default;
    }

    /// <summary>
    /// Returns underlying theme variant.
    /// </summary>
    public static ThemeVariant ToVariant(this ChromeTheme src)
    {
        switch (src)
        {
            case ChromeTheme.Light:
                return ThemeVariant.Light;
            case ChromeTheme.Dark:
                return ThemeVariant.Dark;
            default:
                return ThemeVariant.Default;
        }
    }
}