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

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

/// <summary>
/// Base class.
/// </summary>
internal class PixiePageBase : CarouselPage
{
    protected PixieGroup NewGroup(string? title = null)
    {
        var g = new PixieGroup();
        g.CollapseTitle = title;
        g.IsCollapsable = title != null;
        g.CollapseFooter = $"{nameof(PixieGroup)}.{nameof(PixieGroup.CollapseFooter)}";
        Children.Add(g);
        return g;
    }

    protected static T NewControl<T>(PixieGroup? group, string? title = null)
        where T : PixieControl, new()
    {
        return NewControl<T>(group, true, title);
    }

    protected static T NewControl<T>(PixieGroup? group, bool enabled, string? title = null)
        where T : PixieControl, new()
    {
        var c = new T();

        c.Header = c.GetType().Name;
        c.Title = title ?? c.Header;
        c.Footer = c.Header;

        if (!enabled)
        {
            c.IsEnabled = false;
            c.Footer += " Disabled";
        }

        group?.Children.Add(c);
        return c;
    }

    protected static void AddAccoutrements(PixieControl control)
    {
        // Dummy
        var context = new ContextMenu();
        context.Items.Add(new MenuItem() { Header = "Option 1" });
        context.Items.Add(new MenuItem() { Header = "Option 2" });
        context.Items.Add(new MenuItem() { Header = "Option 3" });

        control.LeftSymbol = Symbols.ArrowBack;
        control.LeftButton.IsVisible = true;
        control.LeftButton.DropMenu = context;
        control.RightSymbol = Symbols.ArrowForward;
        control.RightButton.IsVisible = true;
        control.RightButton.DropMenu = context;
    }
}