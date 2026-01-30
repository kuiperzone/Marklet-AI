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

using Xunit.Abstractions;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class AppHostTest
{
    private readonly ITestOutputHelper _out;

    public AppHostTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void DirectoriesDefined()
    {
        _out.WriteLine("SpecialFolder.ApplicationData:");
        _out.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        _out.WriteLine("SpecialFolder.LocalApplicationData:");
        _out.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        // We can't fully test here,
        // but we can see output on demand and ensure no throw.
        var host = new ApplicationHost("zone.marklet.kuiper");
        _out.WriteLine(nameof(host.ConfigDirectory) + ":");
        _out.WriteLine(host.ConfigDirectory);
        Assert.NotEmpty(host.ConfigDirectory);

        _out.WriteLine(nameof(host.DataDirectory) + ":");
        _out.WriteLine(host.DataDirectory);
        Assert.NotEmpty(host.DataDirectory);

        _out.WriteLine(nameof(host.DownloadDirectory) + ":");
        _out.WriteLine(host.DownloadDirectory);
        Assert.NotEmpty(host.DownloadDirectory);

        _out.WriteLine(nameof(host.RuntimeDirectory) + ":");
        _out.WriteLine(host.RuntimeDirectory);
        Assert.NotEmpty(host.RuntimeDirectory);

        //Assert.Fail();
    }

    [Fact]
    public void IsFlatpak_IsFalse()
    {
        var host = new ApplicationHost("zone.marklet.kuiper");
        Assert.False(host.IsFlatpak);
    }

    [Fact]
    public void HasFileSystemPermissions_IsTrue()
    {
        var host = new ApplicationHost("zone.marklet.kuiper");
        Assert.True(host.HasFileSystemPermission);
    }

}
