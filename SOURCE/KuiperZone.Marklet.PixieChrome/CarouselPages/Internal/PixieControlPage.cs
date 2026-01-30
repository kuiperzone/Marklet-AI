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

using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

internal sealed class PixieControlPage : PixiePageBase
{
    public PixieControlPage()
    {
        Title = nameof(PixieControl);
        Symbol = Symbols.AdsClick;

        var group = NewGroup(nameof(PixieControl));
        group.TopTitle = nameof(PixieGroup.TopTitle);
        group.TopFooter = nameof(PixieGroup.TopFooter);

        var control = NewControl<PixieControl>(group);
        AddAccoutrements(control);

        // Minimal
        NewControl<PixieControl>(group);
    }
}