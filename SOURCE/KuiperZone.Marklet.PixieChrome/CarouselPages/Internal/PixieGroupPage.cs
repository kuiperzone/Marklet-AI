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

internal sealed class PixieGroupPage : PixiePageBase
{
    private readonly PixieGroup _group;

    public PixieGroupPage()
    {
        Title = nameof(PixieGroup);
        Symbol = Symbols.TabGroup;

        // GROUP
        _group = NewGroup(nameof(PixieGroup));

        var group = _group;
        group.TopTitle = nameof(PixieGroup.TopTitle);
        group.TopFooter = nameof(PixieGroup.TopFooter);

        var button = NewControl<PixieButton>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.IsCollapsable);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = group.IsCollapsable;
        button.RightButton.Click += IsCollapsableChangedHandler;

        button = NewControl<PixieButton>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.ChildBackground);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = true;
        button.RightButton.Click += ChildBackgroundChangedHandler;

        button = NewControl<PixieButton>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.ChildCorner);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = true;
        button.RightButton.Click += ChildCornerChangedHandler;

        button = NewControl<PixieButton>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.ChildIndent);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = group.ChildIndent != 0.0;
        button.RightButton.Click += ChildIndentChangedHandler;

        button = NewControl<PixieButton>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.IsCornerGrouped);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = true;
        button.RightButton.Click += IsCornerGroupedChangedHandler;
    }

    private void IsCollapsableChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.IsCollapsable = btn.IsChecked;
    }

    private void ChildBackgroundChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.ChildBackground = btn.IsChecked ? Styling.BackgroundHigh : ChromeBrushes.Transparent;
    }

    private void ChildCornerChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.ChildCorner = btn.IsChecked ? Styling.LargeCornerRadius : default;
    }

    private void ChildIndentChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.ChildIndent = btn.IsChecked ? ChromeSizes.TabPx : 0.0;
    }

    private void IsCornerGroupedChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.IsCornerGrouped = btn.IsChecked;
    }

}