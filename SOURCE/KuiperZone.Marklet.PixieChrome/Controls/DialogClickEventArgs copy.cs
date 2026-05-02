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

using Avalonia.Interactivity;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Event arguments for <see cref="DialogControls"/>.
/// </summary>
public class DialogClickEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public DialogClickEventArgs(DialogButtons button)
    {
        Button = button;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DialogClickEventArgs(RoutedEvent routedEvent, object? source, DialogButtons button)
        : base(routedEvent, source)
    {
        Button = button;
    }

    /// <summary>
    /// Gets button kind what was clicked.
    /// </summary>
    /// <remarks>
    /// The value will comprise a single <see cref="DialogButtons"/> bit flag pertaining to the button clicked.
    /// </remarks>
    public DialogButtons Button { get; }
}
