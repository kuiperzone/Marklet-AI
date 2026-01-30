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

using System.Text.Json;
using Avalonia.Controls;

namespace KuiperZone.Marklet.PixieChrome.Settings;

/// <summary>
/// Serializable window state intended to pertain to an application MainWindow.
/// </summary>
/// <remarks>
/// It is intended that this data be applied to MainWindow only, although in principle it can be used with any. It
/// stores size and state, but does not store position by design.
/// </remarks>
public sealed class WindowPersistence : SettingsBase, IEquatable<SettingsBase>
{
    private bool _isSizingWindow;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <remarks>
    /// The default 0 value is ignored by <see cref="SetWindow(Window)"/>.
    /// </remarks>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <remarks>
    /// The default 0 value is ignored by <see cref="SetWindow(Window)"/>.
    /// </remarks>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets whether maximimized.
    /// </summary>
    public bool IsMaximized { get; set; }

    /// <inheritdoc cref="SettingsBase.Reset"/>
    public override void Reset()
    {
        CopyFrom(new WindowPersistence());
    }

    /// <inheritdoc cref="SettingsBase.Deserialize(string)"/>
    public override void Deserialize(string json)
    {
        CopyFrom(JsonSerializer.Deserialize(json, SettingsSerializer.Default.WindowPersistence));
    }

    /// <inheritdoc cref="SettingsBase.Serialize"/>
    public override string? Serialize()
    {
        if (!_isSizingWindow)
        {
            return JsonSerializer.Serialize(this, SettingsSerializer.Default.WindowPersistence);
        }

        // Prevents Write() if CopyTo(Window) has event driven write handler.
        return null;
    }

    /// <inheritdoc cref="SettingsBase.TrimLegal"/>
    public override void TrimLegal()
    {
        Width = Math.Clamp(Width, 0, short.MaxValue);
        Height = Math.Clamp(Height, 0, short.MaxValue);
    }


    /// <inheritdoc cref="SettingsBase.Equals(SettingsBase?)"/>
    public override bool Equals(SettingsBase? other)
    {
        return other is WindowPersistence s &&
            Width == s.Width &&
            Height == s.Height &&
            IsMaximized == s.IsMaximized;
    }

    /// <summary>
    /// Copies setting from "other" to "this".
    /// </summary>
    public void CopyFrom(WindowPersistence? other)
    {
        if (other != null)
        {
            Width = other.Width;
            Height = other.Height;
            IsMaximized = other.IsMaximized;
        }
    }

    /// <summary>
    /// Copies "window" size to "this" and returns true if changed.
    /// </summary>
    public bool CopyFrom(Window window)
    {
        var state = window.WindowState;

        if (state == WindowState.Maximized || state == WindowState.FullScreen)
        {
            if (!IsMaximized)
            {
                IsMaximized = true;
                return true;
            }

            return false;
        }

        double w = window.Width;
        double h = window.Height;

        if (state == WindowState.Normal && w > 0 && h > 0 && (Width != w || Height != h))
        {
            IsMaximized = false;
            Width = (int)window.Width;
            Height = (int)window.Height;
            return true;
        }

        if (IsMaximized)
        {
            IsMaximized = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Copies state to "window" and returns true on success.
    /// </summary>
    /// <remarks>
    /// A default instance of <see cref="WindowPersistence"/> with <see cref="Width"/> and <see cref="Height"/> equal to
    /// zero does nothing and the result is false.
    /// </remarks>
    public bool SetWindow(Window window)
    {
        try
        {
            if (Width > 0 && Height > 0)
            {
                // Prevent Write() if window has SizeChanged event handler
                _isSizingWindow = true;

                window.Width = Width;
                window.Height = Height;

                if (window.WindowState != WindowState.Minimized)
                {
                    window.WindowState = IsMaximized && window.CanMaximize ? WindowState.Maximized : WindowState.Normal;
                }

                return true;
            }

            return false;
        }
        finally
        {
            _isSizingWindow = false;
        }
    }

}
