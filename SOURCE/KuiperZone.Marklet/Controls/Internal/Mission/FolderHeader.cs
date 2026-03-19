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

using Avalonia;
using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Folder header for <see cref="BasketView"/>.
/// </summary>
internal sealed class FolderHeader : Border
{
    private static readonly ChromeStyling Styling = ChromeStyling.Global;
    private readonly string? _title;
    private readonly DockPanel _panel = new();
    private readonly TextBlock _label = new();
    private int _count;

    /// <summary>
    /// Constructor.
    /// </summary>
    public FolderHeader(string? title, bool isRoot)
    {
        _title = title;
        Margin = new(0.0, ChromeSizes.HugePx, 0.0, ChromeSizes.LargePx);
        Padding = new(0.0, 0.0, 0.0, ChromeSizes.SmallPx);
        BorderThickness = new(0.0, 0.0, 0.0, 1.0);
        Child = _panel;

        _panel.HorizontalSpacing = ChromeSizes.StandardPx;
        _panel.Children.Add(_label);
        DockPanel.SetDock(_label, Dock.Left);

        _label.FontSize = ChromeFonts.SmallFontSize;
        _label.Foreground = ChromeStyling.GrayForeground;
        _label.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _label.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

        if (!isRoot)
        {
            NewFolder = new LightButton();
            _panel.Children.Add(NewFolder);
            DockPanel.SetDock(NewFolder, Dock.Right);

            NewFolder.Content = Symbols.CreateNewFolder;
            NewFolder.Tip = "New folder";
            NewFolder.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            NewFolder.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        }

        _count = -1;
        Count = 0;
    }

    /// <summary>
    /// Gets or sets the number of history items or number of folders.
    /// </summary>
    public int Count
    {
        get { return _count; }

        set
        {
            if (_count != value)
            {
                _count = value;

                if (value > 0)
                {
                    _label.Text = _title + " " + value;
                }
                else
                {
                    _label.Text = _title;
                }
            }
        }
    }

    /// <summary>
    /// Gets the new folder button.
    /// </summary>
    public LightButton? NewFolder { get; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        BorderBrush = Styling.BorderBrush;
        Styling.StylingChanged += StylingChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChanged;
    }

    private void StylingChanged(object? _, EventArgs __)
    {
        BorderBrush = Styling.BorderBrush;
    }
}