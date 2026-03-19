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

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Event arguments for <see cref="CrossTextBlock"/> link click.
/// </summary>
public sealed class LinkClickEventArgs : EventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public LinkClickEventArgs(Uri link)
    {
        Uri = link;
    }

    /// <summary>
    /// Gets the link Uri.
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Gets whether the <see cref="Uri"/> scheme equals "app".
    /// </summary>
    /// <remarks>
    /// This scheme must be handled using the <see cref="CrossTracker.LinkClick"/> event.
    /// </remarks>
    public bool IsAppLink
    {
        get { return Uri.Scheme == "app"; }
    }

    /// <summary>
    /// Gets or sets whether the URI was handled by the event subscriber.
    /// </summary>
    public bool Handled { get; set; }
}
