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

using Avalonia.Input;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Cursor instances.
/// </summary>
/// <remarks>
/// Instances are null outside of running Application, i.e. in unit test.
/// </remarks>
public static class ChromeCursors
{
    /// <summary>
    /// Gets a <see cref="StandardCursorType.None"/> instance.
    /// </summary>
    public static readonly Cursor? NoneCursor = New(StandardCursorType.None);

    /// <summary>
    /// Gets an <see cref="StandardCursorType.Arrow"/> instance.
    /// </summary>
    public static readonly Cursor? ArrowCursor = New(StandardCursorType.Arrow);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.Hand"/> instance.
    /// </summary>
    public static readonly Cursor? HandCursor = New(StandardCursorType.Hand);

    /// <summary>
    /// Gets an <see cref="StandardCursorType.Ibeam"/> instance.
    /// </summary>
    public static readonly Cursor? IBeamCursor = New(StandardCursorType.Ibeam);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.Wait"/> instance.
    /// </summary>
    public static readonly Cursor? WaitCursor = New(StandardCursorType.Wait);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.No"/> instance.
    /// </summary>
    public static readonly Cursor? NoCursor = New(StandardCursorType.No);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.SizeNorthSouth"/> instance.
    /// </summary>
    public static readonly Cursor? SizeNorthSouthCursor = New(StandardCursorType.SizeNorthSouth);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.SizeWestEast"/> instance.
    /// </summary>
    public static readonly Cursor? SizeWestEastCursor = New(StandardCursorType.SizeWestEast);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.TopLeftCorner"/> instance.
    /// </summary>
    public static readonly Cursor? TopLeftCornerCursor = New(StandardCursorType.TopLeftCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.TopRightCorner"/> instance.
    /// </summary>
    public static readonly Cursor? TopRightCornerCursor = New(StandardCursorType.TopRightCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.BottomLeftCorner"/> instance.
    /// </summary>
    public static readonly Cursor? BottomLeftCornerCursor = New(StandardCursorType.BottomLeftCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.BottomRightCorner"/> instance.
    /// </summary>
    public static readonly Cursor? BottomRightCornerCursor = New(StandardCursorType.BottomRightCorner);

    private static Cursor? New(StandardCursorType type)
    {
        const string NSpace = $"{nameof(ChromeCursors)}.{nameof(New)}";

        try
        {
            return new(type);
        }
        catch (Exception e)
        {
            // Expected to throw in unit test
            ConditionalDebug.WriteLine(e.Message);
            return null;
        }
    }
}
