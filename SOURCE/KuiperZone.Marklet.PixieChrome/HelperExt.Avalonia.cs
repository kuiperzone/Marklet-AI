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

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Extension methods related to Avalonia classes.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Gets first parent of type T, or null.
    /// </summary>
    public static T? GetParent<T>(this StyledElement src) where T : class
    {
        // First parent is Popup surrounding menu
        var parent = src?.Parent;

        while (parent != null)
        {
            if (parent is T t)
            {
                return t;
            }

            parent = parent.Parent;
        }

        return null;
    }

    /// <summary>
    /// Restart timer.
    /// </summary>
    public static void Restart(this DispatcherTimer src)
    {
        src.Stop();
        src.Start();
    }

    /// <summary>
    /// Gets the sum of Left + Right values.
    /// </summary>
    public static double Width(this Thickness src)
    {
        return src.Left + src.Right;
    }

    /// <summary>
    /// Gets the sum of Top + Bottom values.
    /// </summary>
    public static double Height(this Thickness src)
    {
        return src.Top + src.Bottom;
    }

    /// <summary>
    /// Returns true if the color is considered “dark”.
    /// </summary>
    public static bool IsDark(this Color src)
    {
        // Convert to linear luminance (sRGB standard)
        double r = src.R / 255.0;
        double g = src.G / 255.0;
        double b = src.B / 255.0;

        // Perceptual luminance approximation
        double luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;

        return luminance < 0.55;
    }

    /// <summary>
    /// Returns true if the color is considered “light”.
    /// </summary>
    public static bool IsLight(this Color src)
    {
        return !IsDark(src);
    }

    /// <summary>
    /// Blends the "src" color with "background" with the given "opacity" in the range [0, 1.0].
    /// </summary>
    /// <remarks>
    /// Here, "opacity" refers to the opacity of the "src" color, rather than "background". On success, the RGB channels
    /// are merged in proportion, with the new alpha channel taking on the <see cref="Color.A"/> value of "background".
    /// The result is always "src" here the alpha of "background" is 0 or "opacity" equals 1.0.
    /// </remarks>
    public static Color Blend(this Color src, double opacity, Color background)
    {
        if (opacity >= 1.0 || background.A == 0)
        {
            return src;
        }

        if (opacity <= 0.0)
        {
            return background;
        }

        var o1 = 1.0 - opacity;
        var r = src.R * opacity + background.R * o1;
        var g = src.G * opacity + background.G * o1;
        var b = src.B * opacity + background.B * o1;
        return new(background.A, (byte)r, (byte)g, (byte)b);
    }

    /// <summary>
    /// Overloads IndexOf() with start position.
    /// </summary>
    public static int IndexOf<T1, T2>(this AvaloniaList<T1> src, T2 item, int startPos)
        where T1 : class
        where T2 : T1
    {
        for (int n = startPos; n < src.Count; ++n)
        {
            if (ReferenceEquals(src[n], item))
            {
                return n;
            }
        }

        return -1;
    }

    /// <summary>
    /// Efficient replacement of Avalonia list items. Returns true if modified.
    /// </summary>
    public static bool Replace<T1, T2>(this AvaloniaList<T1> src, IReadOnlyCollection<T2> other)
        where T1 : class
        where T2 : T1
    {
        if (src == other)
        {
            return false;
        }

        int n = -1;
        bool rslt = false;
        List<T1>? temp = null;

        foreach (var item in other)
        {
            if (src.Count > ++n)
            {
                if (ReferenceEquals(src[n], item))
                {
                    continue;
                }

                rslt = true;
                src.RemoveRange(n, src.Count - n);
            }

            temp ??= new(other.Count - n);
            temp.Add(item);
        }

        if (src.Count > other.Count)
        {
            rslt = true;
            src.RemoveRange(other.Count, src.Count - other.Count);
        }

        if (temp != null)
        {
            src.AddRange(temp);
            return true;
        }

        return rslt;
    }

    /// <summary>
    /// Finds the first focusable control in "src".
    /// </summary>
    /// <remarks>
    /// Result may not respect tab order.
    /// </remarks>
    public static Control? FirstFocusableChild(this Visual src)
    {
        foreach (var item in src.GetVisualDescendants())
        {
            if (item is Control c && c.IsEffectivelyVisible && c.IsEffectivelyEnabled && c.Focusable)
            {
                return c;
            }
        }

        return null;
    }

    /// <summary>
    /// Copies text to clipboard and returns true on success.
    /// </summary>
    public static bool CopyToClipboard(this Visual? visual, string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;

            if (clipboard != null)
            {
                clipboard.SetTextAsync(text);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Collapses separators in menu and sub-menus.
    /// </summary>
    /// <remarks>
    /// It returns true if the menu contains at least one visible menu item after normalization. The result is false if
    /// "src" is empty, or contains only separators or hidden items.
    /// </remarks>
    public static bool Normalize<T>(this T src) where T : MenuBase
    {
        return NormalizeMenuItems(src.Items);
    }

    private static bool NormalizeMenuItems(IEnumerable<object?> items)
    {
        bool notEmpty = false;
        bool lastSeparator = true;
        Separator? trailingSeparator = null;

        foreach (var item in items.OfType<Control>())
        {
            if (item is Separator s)
            {
                s.IsVisible = !lastSeparator;

                if (!lastSeparator)
                {
                    trailingSeparator = s;
                }

                lastSeparator = true;
            }
            else
            if (item.IsVisible)
            {
                notEmpty = true;
                lastSeparator = false;
                trailingSeparator = null;

                if (item is MenuItem m)
                {
                    NormalizeMenuItems(m.Items);
                }
            }
        }

        if (trailingSeparator != null)
        {
            trailingSeparator.IsVisible = false;
        }

        return notEmpty;
    }
}