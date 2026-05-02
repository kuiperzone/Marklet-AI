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

using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
    public static readonly Cursor? None = GetStandardCursor(StandardCursorType.None);

    /// <summary>
    /// Gets an <see cref="StandardCursorType.Arrow"/> instance.
    /// </summary>
    public static readonly Cursor? Arrow = GetStandardCursor(StandardCursorType.Arrow);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.Hand"/> instance.
    /// </summary>
    public static readonly Cursor? Hand = GetStandardCursor(StandardCursorType.Hand);

    /// <summary>
    /// Gets an <see cref="StandardCursorType.Ibeam"/> instance.
    /// </summary>
    public static readonly Cursor? IBeam = GetStandardCursor(StandardCursorType.Ibeam);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.Wait"/> instance.
    /// </summary>
    public static readonly Cursor? Wait = GetStandardCursor(StandardCursorType.Wait);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.No"/> instance.
    /// </summary>
    public static readonly Cursor? No = GetStandardCursor(StandardCursorType.No);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.SizeNorthSouth"/> instance.
    /// </summary>
    public static readonly Cursor? SizeNorthSouth = GetStandardCursor(StandardCursorType.SizeNorthSouth);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.SizeWestEast"/> instance.
    /// </summary>
    public static readonly Cursor? SizeWestEast = GetStandardCursor(StandardCursorType.SizeWestEast);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.TopLeftCorner"/> instance.
    /// </summary>
    public static readonly Cursor? TopLeftCorner = GetStandardCursor(StandardCursorType.TopLeftCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.TopRightCorner"/> instance.
    /// </summary>
    public static readonly Cursor? TopRightCorner = GetStandardCursor(StandardCursorType.TopRightCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.BottomLeftCorner"/> instance.
    /// </summary>
    public static readonly Cursor? BottomLeftCorner = GetStandardCursor(StandardCursorType.BottomLeftCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.BottomRightCorner"/> instance.
    /// </summary>
    public static readonly Cursor? BottomRightCorner = GetStandardCursor(StandardCursorType.BottomRightCorner);

    /// <summary>
    /// Gets a generic "folder" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? FolderLight24 = GetAssetCursor("folder.light.24.png");

    /// <summary>
    /// Gets a generic "folder" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? FolderDark24 = GetAssetCursor("folder.dark.24.png");

    /// <summary>
    /// Gets a generic "folder" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? FolderLight48 = GetAssetCursor("folder.light.48.png");

    /// <summary>
    /// Gets a generic "folder" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? FolderDark48 = GetAssetCursor("folder.dark.48.png");

    /// <summary>
    /// Gets a generic "document" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? DocumentLight24 = GetAssetCursor("document.light.24.png");

    /// <summary>
    /// Gets a generic "document" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? DocumentDark24 = GetAssetCursor("document.dark.24.png");

    /// <summary>
    /// Gets a generic "document" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? DocumentLight48 = GetAssetCursor("document.light.48.png");

    /// <summary>
    /// Gets a generic "document" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? DocumentDark48 = GetAssetCursor("document.dark.48.png");

    /// <summary>
    /// Gets a generic "image" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? ImageLight24 = GetAssetCursor("image.light.24.png");

    /// <summary>
    /// Gets a generic "image" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? ImageDark24 = GetAssetCursor("image.dark.24.png");

    /// <summary>
    /// Gets a generic "image" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? ImageLight48 = GetAssetCursor("image.light.48.png");

    /// <summary>
    /// Gets a generic "image" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? ImageDark48 = GetAssetCursor("image.dark.48.png");

    private static Cursor? GetStandardCursor(StandardCursorType type)
    {
        const string NSpace = $"{nameof(ChromeCursors)}.{nameof(GetStandardCursor)}";

        try
        {
            return new(type);
        }
        catch (Exception e)
        {
            // Expected to throw in unit test
            Diag.WriteLine(e.Message);
            return null;
        }
    }

    private static Cursor? GetAssetCursor(string name)
    {
        return GetAssetCursor(name, new(8, 8));
    }

    private static Cursor? GetAssetCursor(string name, PixelPoint point)
    {
        try
        {
            var uri = new Uri("avares://KuiperZone.Marklet.PixieChrome/Assets/Cursors/" + name);
            var bitmap = new Bitmap(AssetLoader.Open(uri));
            return new Cursor(bitmap, point);
        }
        catch (Exception e)
        {
            // Expected to throw in unit test
            Diag.WriteLine(e.Message);
            return null;
        }
    }
}
