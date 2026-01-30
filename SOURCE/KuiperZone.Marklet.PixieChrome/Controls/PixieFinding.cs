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

using Avalonia.Controls;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Specialist subclass of <see cref="PixieButton"/> instended only to display search results.
/// </summary>
public sealed class PixieFinding : PixieButton
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public PixieFinding(PixieControl source)
    {
        Source = source;
        Footer = source.Footer;
        LeftSymbol = source.LeftSymbol;

        if (!string.IsNullOrEmpty(source.Title))
        {
            Title = source.Title;
            return;
        }

        Title = source.Header;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public PixieFinding(PixieGroup source)
    {
        Source = source;

        if (!string.IsNullOrEmpty(source.TopTitle))
        {
            Title = source.TopTitle;
            Footer = source.TopFooter;
            return;
        }

        if (source.IsCollapsable)
        {
            if (!string.IsNullOrEmpty(source.CollapseTitle))
            {
                LeftSymbol = source.CollapseSymbol;
                Title = source.CollapseTitle;
                Footer = source.CollapseFooter;
                return;
            }

            if (!string.IsNullOrEmpty(source.CollapseHeader))
            {
                LeftSymbol = source.CollapseSymbol;
                Title = source.CollapseHeader;
                Footer = source.CollapseFooter;
                return;
            }

            if (!string.IsNullOrEmpty(source.CollapseFooter))
            {
                LeftSymbol = source.CollapseSymbol;
                Title = source.CollapseFooter;
                return;
            }
        }

        Title = source.TopFooter;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public PixieFinding(TextBlock source)
    {
        Source = source;
        Title = source.Text;
    }

    /// <summary>
    /// Gets the source instance, either a <see cref="PixieControl"/> or <see cref="PixieGroup"/> instance.
    /// </summary>
    public Control? Source { get; }
}
