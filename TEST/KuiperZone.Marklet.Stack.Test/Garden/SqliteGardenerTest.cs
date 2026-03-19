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
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Stack.Test;

public class SqliteGardenerTest : GardenTestBase
{
    [Fact]
    public void Connect_Create_CreatesFile()
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        var obj = new SqliteGardener(path);
        Assert.False(obj.IsReadOnly);
        Assert.NotNull(obj.Location);

        obj.Connect().Dispose();

        Assert.True(File.Exists(Path.Combine(dir, file)));
    }

    [Fact]
    public void Connect_Exists_OpensFile()
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        File.Open(path, FileMode.OpenOrCreate).Dispose();

        var obj = new SqliteGardener(path);
        Assert.False(obj.IsReadOnly);
        Assert.NotNull(obj.Location);

        obj.Connect().Dispose();
    }

    [Fact]
    public void Connect_Exists_ReadOnly()
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var f = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

            var obj0 = new SqliteGardener(path);
            Assert.True(obj0.IsReadOnly);
            Assert.NotNull(obj0.Location);

            obj0.Connect().Dispose();
            return;
        }

        File.WriteAllText(path, "");
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);

        var obj1 = new SqliteGardener(path);
        Assert.True(obj1.IsReadOnly);
        Assert.NotNull(obj1.Location);

        obj1.Connect().Dispose();
    }

}
