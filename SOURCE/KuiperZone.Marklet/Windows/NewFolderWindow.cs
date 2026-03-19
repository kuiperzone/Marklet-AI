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

using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Windows;

/// <summary>
/// Allows user to enter a new folder name.
/// </summary>
public sealed class NewFolderWindow : ChromeDialog
{
    private const int ControlWidth = 250;
    private const DialogButtons AcceptButton = DialogButtons.Ok;
    private readonly PixieEditor _edit = new();
    private readonly TextEditor _subject = new();
    private readonly GardenBasket _basket;
    private List<string>? _folders;

    /// <summary>
    /// Constructor with existing Folders.
    /// </summary>
    public NewFolderWindow(GardenBasket basket)
    {
        _basket = basket;
        Title = "New Folder";

        _edit.Title = "Folder Name";
        _edit.Subject.Width = ControlWidth;
        _edit.Subject.MaxLength = MemoryGarden.MaxMetaLength;

        _subject = _edit.Subject;
        _subject.Submitted += (_, __) => Close(AcceptButton);
        _subject.TextChanged += TextChangedHandler;

        Children.Add(_edit);
        Buttons = AcceptButton | DialogButtons.Cancel;
        DisabledButtons = AcceptButton;
    }

    /// <summary>
    /// Gets or set folder name. Updated when user clicks OK.
    /// </summary>
    public string? FolderName { get; private set; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _edit.Focus();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void Close(DialogButtons button)
    {
        if (button == AcceptButton)
        {
            FolderName = _subject.Text?.Trim();
        }

        base.Close(button);
    }

    private void TextChangedHandler(object? _, EventArgs e)
    {
        bool legal = false;
        var text = _subject.Text?.Trim();


        if (!string.IsNullOrEmpty(text))
        {
            _folders ??= _basket.GetFolderNames();
            legal = !_folders.Contains(text);
        }

        DisabledButtons = legal ? DialogButtons.None : AcceptButton;

        if (legal || string.IsNullOrEmpty(text))
        {
            _edit.Footer = null;
            _subject.ClearValue(ForegroundProperty);
            return;
        }

        _edit.Footer = "Folder name already exists";
        _subject.Foreground = ChromeBrushes.WarningBrush;
    }
}
