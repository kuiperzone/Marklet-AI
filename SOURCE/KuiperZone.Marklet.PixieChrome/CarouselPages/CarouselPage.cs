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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages;

/// <summary>
/// Base class for pages to be shown in <see cref="CarouselWindow"/>.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control. Rather instances of this type are shown in <see
/// cref="CarouselWindow"/>.
/// </remarks>
public class CarouselPage
{
    /// <summary>
    /// Convenient access to <see cref="ChromeStyling.Global"/>.
    /// </summary>
    protected static readonly ChromeStyling Styling = ChromeStyling.Global;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CarouselPage()
    {
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    protected CarouselPage(CarouselPage other)
    {
        Title = other.Title;
        Footer = other.Footer;
        Symbol = other.Symbol;
        IsSectionStart = other.IsSectionStart;
        Children.AddRange(other.Children);
    }

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    /// <remarks>
    /// The page will be ignored if null or empty.
    /// </remarks>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional symbol character in Material Symbol private range.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Gets or sets optional title footer text.
    /// </summary>
    public string? Footer { get; set; }

    /// <summary>
    /// Gets or sets whether this page starts a new section in a sequence.
    /// </summary>
    /// <remarks>
    /// Where true, a dividing line is drawn in the left-hand panel of <see cref="CarouselWindow"/> provided the page is
    /// not the first.
    /// </remarks>
    public bool IsSectionStart { get; set; }

    /// <summary>
    /// Gest a list of controls displayed vertically in a scroll panel.
    /// </summary>
    /// <remarks>
    /// The subclass or class should populate this. It is anticipated that these be instances of <see
    /// cref="PixieGroup"/>, where each itself contains instances of <see cref="PixieControl"/>. Controls will be
    /// visibly separated.
    /// </remarks>
    public List<Control> Children { get; } = new();

    /// <summary>
    /// Called by <see cref="CarouselWindow"/> when the window opens.
    /// </summary>
    /// <remarks>
    /// Base method does nothing but may be overridden.
    /// </remarks>
    public virtual void OnOpened()
    {
    }

    /// <summary>
    /// Called by <see cref="CarouselWindow"/> when the window closes.
    /// </summary>
    /// <remarks>
    /// Base method does nothing but may be overridden.
    /// </remarks>
    public virtual void OnClosed()
    {
    }
}