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

using Avalonia.Controls;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Shared;
using Avalonia.Input;
using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Keep input gestures in one place.
/// </summary>
internal static class BasketKeys
{
    // BasketMenu
    public static readonly KeyGesture NewGesture = new(Key.N, KeyModifiers.Alt);
    public static readonly KeyGesture EphemeralGesture = new(Key.Q, KeyModifiers.Alt);
    public static readonly KeyGesture SearchGesture = new(Key.F, KeyModifiers.Shift | KeyModifiers.Control);
    public static readonly KeyGesture PinTopGesture = new(Key.T, KeyModifiers.Shift | KeyModifiers.Control);
    public static readonly KeyGesture FolderGesture = new(Key.N, KeyModifiers.Shift | KeyModifiers.Control);
    public static readonly KeyGesture ExpandGesture = new(Key.E, KeyModifiers.Alt);
    public static readonly KeyGesture CloseGesture = new(Key.C, KeyModifiers.Alt);

    // CardMenu
    public static readonly KeyGesture PinnedGesture = new(Key.P, KeyModifiers.Alt);
    public static readonly KeyGesture TouchGesture = new(Key.T, KeyModifiers.Alt);
    public static readonly KeyGesture RenameGesture = new(Key.F2);
    public static readonly KeyGesture PropertiesGesture = new(Key.Enter, KeyModifiers.Alt);
    public static readonly KeyGesture ArchiveGesture = new(Key.A, KeyModifiers.Alt);
    public static readonly KeyGesture WasteGesture = new(Key.Delete);
    public static readonly KeyGesture RestoreGesture = new(Key.R, KeyModifiers.Alt);
    public static readonly KeyGesture DeleteGesture = new(Key.Delete, KeyModifiers.Shift);

    // Folders
    // Not currently possible as no way to select a folder
}
