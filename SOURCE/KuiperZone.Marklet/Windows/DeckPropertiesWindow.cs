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
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Windows;

/// <summary>
/// Allow user to select or enter a name for <see cref="GardenDeck.Folder"/>.
/// </summary>
public sealed class DeckPropertiesWindow : ChromeDialog
{
    private const DialogButtons AcceptButton = DialogButtons.Ok;
    private readonly GardenDeck _source;
    private readonly PixieCombo _combo = new();
    private readonly PixieCheckBox _pinned = new();

    /// <summary>
    /// Constructor with existing Folders.
    /// </summary>
    public DeckPropertiesWindow(GardenDeck source)
    {
        _source = source;

        var format = source.Format;
        var ephem = source.Ephemeral;
        Title = format.DisplayName(DisplayStyle.Default, ephem) + " Properties";

        MaxWidth = 450;
        _combo.MinSubjectWidth = 250;
        _combo.MaxSubjectWidth = 250;

        _combo.Title = "Folder Name";
        _combo.Footer = $"Select a new folder to move the {format.DisplayName(DisplayStyle.Lower)}.";

        _combo.Items.Add("[None]");
        _combo.SelectedIndex = 0;
        Children.Add(_combo);

        _pinned.Title = "Pinned";
        _pinned.Footer = $"Pinned items may optionally be sorted to the top and pinned items in the {BasketKind.Recent.DisplayName()} bin will not be moved to {BasketKind.Waste.DisplayName()} as a result of housekeeping.";
        Children.Add(_pinned);

        if (ephem != EphemeralStatus.Persistant)
        {
            Details = $"{format.DisplayName(DisplayStyle.Plural)} are lost when application exits.";
        }

        Buttons = AcceptButton | DialogButtons.Cancel;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void Close(DialogButtons button)
    {
        if (button == AcceptButton)
        {
            string? name = _combo.SelectedIndex > 0 ? _combo.Text : null;

            if (_source.Folder != name)
            {
                // Signal change
                _source.VisualSignals |= SignalFlags.ItemAttention | SignalFlags.OpenFolder;
                _source.Folder = name;
            }

            _source.IsPinned = _pinned.IsChecked;
        }

        base.Close(button);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _combo.Focus();
        _pinned.IsChecked = _source.IsPinned;

        var folders = _source.Garden?[_source.CurrentBasket].GetFolderNames();

        if (folders != null)
        {
            foreach (var item in folders)
            {
                _combo.Items.Add(item);
            }

            if (!string.IsNullOrEmpty(_source.Folder))
            {
                int idx = folders.IndexOf(_source.Folder);

                if (idx > -1)
                {
                    _combo.SelectedIndex = idx + 1;
                }
            }
        }
    }
}
