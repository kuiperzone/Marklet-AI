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

        var button = NewControl<PixieCard>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.IsCollapsible);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = group.IsCollapsible;
        button.RightButton.Click += IsCollapsibleChangedHandler;

        button = NewControl<PixieCard>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.ChildBackground);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = true;
        button.RightButton.Click += ChildBackgroundChangedHandler;

        button = NewControl<PixieCard>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.ChildCorner);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = true;
        button.RightButton.Click += ChildCornerChangedHandler;

        button = NewControl<PixieCard>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.ChildIndent);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = group.ChildIndent != 0.0;
        button.RightButton.Click += ChildIndentChangedHandler;

        button = NewControl<PixieCard>(group);
        button.RightButton.IsVisible = true;
        button.RightButton.Content = nameof(PixieGroup.IsCapped);
        button.RightButton.CanToggle = true;
        button.RightButton.IsChecked = true;
        button.RightButton.Click += IsPillListChangedHandler;
    }

    private void IsCollapsibleChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.IsCollapsible = btn.IsChecked;
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

    private void IsPillListChangedHandler(object? sender, EventArgs __)
    {
        var btn = (LightButton)sender!;
        _group.IsCapped = btn.IsChecked;
    }

}