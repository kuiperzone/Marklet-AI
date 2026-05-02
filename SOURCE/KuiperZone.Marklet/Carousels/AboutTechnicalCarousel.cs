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

using System.Runtime.InteropServices;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Carousels;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Carousels;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage"/>.
/// </summary>
public sealed class AboutTechnicalCarousel : CarouselPage
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutTechnicalCarousel()
    {
        Title = "Technical";
        Symbol = Symbols.Handyman;

        var host = ChromeApplication.Current.Host;

        // SYSTEM
        var group = new PixieGroup();
        Children.Add(group);

        // Os
        var item = new PixieSelectableText();
        item.Header = "OS";
        item.Text = RuntimeInformation.OSDescription;
        item.Footer = OsConfirmation();
        group.Children.Add(item);

        item = new PixieSelectableText();
        item.Header = "File System Permissions";
        item.Text = host.HasFileSystemPermission.ToString();
        item.Footer = "Indicates application is running in sandbox with resticted permissions when false";
        group.Children.Add(item);


        // PATHS
        group = new PixieGroup();
        Children.Add(group);

        // ProcessDirectory
        item = new PixieSelectableText();
        group.Children.Add(item);
        item.Header = nameof(ApplicationHost.ProcessDirectory).GetFriendlyNameOf();
        item.Text = host.ProcessDirectory;

        // ConfigDirectory
        item = new PixieSelectableText();
        group.Children.Add(item);
        item.Header = nameof(ApplicationHost.ConfigDirectory).GetFriendlyNameOf();
        item.Text = host.ConfigDirectory;

        // ConfigDirectory
        item = new PixieSelectableText();
        group.Children.Add(item);
        item.Header = nameof(ApplicationHost.DataDirectory).GetFriendlyNameOf();
        item.Text = host.DataDirectory;


        // DATABASE
        group = new PixieGroup();
        Children.Add(group);

        item = new PixieSelectableText();
        item.Header = "Current Database";
        item.Text = DatabaseSettings.Global.GetActualPath();
        item.Footer = "Database location may be changed in Settings";
        group.Children.Add(item);

        item = new PixieSelectableText();
        item.Header = "Size on Disk";
        item.Text = Magverter.ToFriendlyBytes(SqliteProvider.SizeOnDisk(DatabaseSettings.Global.GetActualPath()));
        group.Children.Add(item);
    }

    private static string OsConfirmation()
    {
        var host = ChromeApplication.Current.Host;

        if (host.IsFlatpak)
        {
            return "Linux Flatpak";
        }

        if (host.IsLinux)
        {
            return "Linux";
        }

        if (host.IsBsd)
        {
            return "BSD";
        }

        if (host.IsWindows)
        {
            return "Windows";
        }

        if (host.IsOsx)
        {
            return "OSX";
        }

        return "Unknown";

    }
}