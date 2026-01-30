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
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.CarouselPages;

namespace KuiperZone.Marklet.CarouselPages;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage"/>.
/// </summary>
public sealed class AboutDonateCarouselPage : CarouselPage
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutDonateCarouselPage()
    {
        Title = "Donate";
        Symbol = Symbols.CardMembership;

        var view = new MarkView();
        view.IsChromeStyled = true;
        view.Content = GetDonateText();
        Children.Add(view);
    }

    private static string GetDonateText()
    {
        return @"# Not Necessary

Marklet is free open source software. It is not necessary or expected to provide payment. It is suggested that the same
attitude be applied to any forks or derivatives.

This software was created with non-financial motivations, which may be summarized by three quotations:

> *Consciousness cannot be accounted for in physical terms. For consciousness is absolutely fundamental. It cannot be accounted for in terms of anything else.*
> **Erwin Schrödinger, 1931**

> *I point out to you... a lesson learned from past over-machined societies which you appear not to have learned. The devices themselves condition the users to employ each other the way they employ machines.*
> **Leto II, God Emperor of Dune, Frank Herbert**

> *Any state, any entity, any ideology that fails to recognise the worth, the dignity, the rights of man... that state is obsolete.*
> **The Obsolete Man, The Twilight Zone, E65**

The author believes that we should have control over the software that runs on our own devices and that our ""information"" belongs to us.
Never has this been more important than today.

If you wish to express thanks in a small, voluntary way, a donation link may appear here in the future. However, the project is not
intended as a business. There will never be in-app nagging, reminders, prompts or pressure — you would have to seek out
the link yourself.

In return, there would simply be gratitude. This is the stuff that the universe actually runs on.";
    }
}
