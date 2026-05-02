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

using KuiperZone.Marklet.PixieChrome.Carousels;
using KuiperZone.Marklet.PixieChrome.Settings;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Settings window base class.
/// </summary>
/// <remarks>
/// This class is concrete but expects instances of <see cref="CarouselPage{T}"/> to be added to <see
/// cref="CarouselDialog.Pages"/> in the subclass constructor. The class holds the last selected page index statically
/// so that new instances are shown in the same location. Subclass to extend.
/// </remarks>
public class SettingsWindow : CarouselDialog
{
    private static int s_globalIndex;
    private static readonly WindowPersistence s_persistence = new();


    /// <summary>
    /// Constructor with flag which determines whether settings are fluid (true), or whether the window has OK and
    /// Cancel buttons (false).
    /// </summary>
    public SettingsWindow(bool fluid = true)
        : base(GetButtons(fluid))
    {
        Title = "Settings";
        IsFluid = fluid;

        Width = 1000;
        MinWidth = 700;

        Height = 700;
        MinHeight = 400;

        IsSearchVisible = true;
    }

    /// <summary>
    /// Gets whether settings are fluid.
    /// </summary>
    public bool IsFluid { get; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        // Order important
        base.OnOpened(e);
        PageIndex = s_globalIndex;
        s_persistence.SetWindow(this);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        s_globalIndex = PageIndex;
        s_persistence.CopyFrom(this);
        base.OnClosed(e);
    }

    private static DialogButtons GetButtons(bool fluid)
    {
        if (fluid)
        {
            return DialogButtons.None;
        }

        return DialogButtons.Apply | DialogButtons.Ok | DialogButtons.Cancel;
    }
}
