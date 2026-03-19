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
    public static readonly Cursor? None = NewStandard(StandardCursorType.None);

    /// <summary>
    /// Gets an <see cref="StandardCursorType.Arrow"/> instance.
    /// </summary>
    public static readonly Cursor? Arrow = NewStandard(StandardCursorType.Arrow);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.Hand"/> instance.
    /// </summary>
    public static readonly Cursor? Hand = NewStandard(StandardCursorType.Hand);

    /// <summary>
    /// Gets an <see cref="StandardCursorType.Ibeam"/> instance.
    /// </summary>
    public static readonly Cursor? IBeam = NewStandard(StandardCursorType.Ibeam);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.Wait"/> instance.
    /// </summary>
    public static readonly Cursor? Wait = NewStandard(StandardCursorType.Wait);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.No"/> instance.
    /// </summary>
    public static readonly Cursor? No = NewStandard(StandardCursorType.No);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.SizeNorthSouth"/> instance.
    /// </summary>
    public static readonly Cursor? SizeNorthSouth = NewStandard(StandardCursorType.SizeNorthSouth);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.SizeWestEast"/> instance.
    /// </summary>
    public static readonly Cursor? SizeWestEast = NewStandard(StandardCursorType.SizeWestEast);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.TopLeftCorner"/> instance.
    /// </summary>
    public static readonly Cursor? TopLeftCorner = NewStandard(StandardCursorType.TopLeftCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.TopRightCorner"/> instance.
    /// </summary>
    public static readonly Cursor? TopRightCorner = NewStandard(StandardCursorType.TopRightCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.BottomLeftCorner"/> instance.
    /// </summary>
    public static readonly Cursor? BottomLeftCorner = NewStandard(StandardCursorType.BottomLeftCorner);

    /// <summary>
    /// Gets a <see cref="StandardCursorType.BottomRightCorner"/> instance.
    /// </summary>
    public static readonly Cursor? BottomRightCorner = NewStandard(StandardCursorType.BottomRightCorner);

    /// <summary>
    /// Gets a generic "folder" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? FolderLight24 = NewAsset("folder.light.24.png");

    /// <summary>
    /// Gets a generic "folder" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? FolderDark24 = NewAsset("folder.dark.24.png");

    /// <summary>
    /// Gets a generic "folder" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? FolderLight48 = NewAsset("folder.light.48.png");

    /// <summary>
    /// Gets a generic "folder" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? FolderDark48 = NewAsset("folder.dark.48.png");

    /// <summary>
    /// Gets a generic "document" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? DocumentLight24 = NewAsset("document.light.24.png");

    /// <summary>
    /// Gets a generic "document" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? DocumentDark24 = NewAsset("document.dark.24.png");

    /// <summary>
    /// Gets a generic "document" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? DocumentLight48 = NewAsset("document.light.48.png");

    /// <summary>
    /// Gets a generic "document" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? DocumentDark48 = NewAsset("document.dark.48.png");

    /// <summary>
    /// Gets a generic "image" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? ImageLight24 = NewAsset("image.light.24.png");

    /// <summary>
    /// Gets a generic "image" cursor of 24x24px.
    /// </summary>
    public static readonly Cursor? ImageDark24 = NewAsset("image.dark.24.png");

    /// <summary>
    /// Gets a generic "image" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? ImageLight48 = NewAsset("image.light.48.png");

    /// <summary>
    /// Gets a generic "image" cursor of 48x48px.
    /// </summary>
    public static readonly Cursor? ImageDark48 = NewAsset("image.dark.48.png");

    private static Cursor? NewStandard(StandardCursorType type)
    {
        const string NSpace = $"{nameof(ChromeCursors)}.{nameof(NewStandard)}";

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

    private static Cursor? NewAsset(string name)
    {
        return NewAsset(name, new(8, 8));
    }

    private static Cursor? NewAsset(string name, PixelPoint point)
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
            ConditionalDebug.WriteLine(e.Message);
            return null;
        }
    }
}
