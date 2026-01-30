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

using Avalonia.Interactivity;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Event arguments for <see cref="CrossTextBlock"/> link click.
/// </summary>
public sealed class LinkClickEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public LinkClickEventArgs(Uri link)
    {
        Uri = link;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public LinkClickEventArgs(RoutedEvent? routedEvent, object? source, Uri link)
        : base(routedEvent, source)
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
    public bool IsAppLink
    {
        get { return Uri.Scheme == "app"; }
    }

}
