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
using KuiperZone.Marklet.PixieChrome.CarouselPages;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Windows.Internal;

/// <summary>
/// Internal for <see cref="CarouselWindow"/>
/// </summary>
internal sealed class PageEntry : CarouselPage
{
    public PageEntry(CarouselPage other, int index, IEnumerable<string>? classes)
        : base(other)
    {
        // This is now a copy of "other"
        PageIndex = index;
        IndexButton.Title = Title;
        IndexButton.LeftSymbol = Symbol;

        foreach (var item in Children)
        {
            item.Classes.AddRange(classes ?? Array.Empty<string>());
        }

        if (IsSectionStart && index > 0)
        {
            IndexDivider = new();
        }
    }

    public int PageIndex { get; }
    public PixieButton IndexButton { get; } = new();
    public DividerEntry? IndexDivider { get; }

    public void RefreshStyling()
    {
        IndexDivider?.RefreshStyling();
    }
}

internal sealed class DividerEntry : Border
{
    public DividerEntry()
    {
        Height = 1.0;
        Margin = ChromeSizes.RegularPadding;
        RefreshStyling();
    }

    public void RefreshStyling()
    {
        Background = ChromeStyling.Global.BorderBrush;
    }
}
