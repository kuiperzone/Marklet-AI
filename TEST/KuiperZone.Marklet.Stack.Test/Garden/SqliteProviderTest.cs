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

using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Stack.Test;

public class SqliteProviderTest : TestBase
{
    [Fact]
    public void Connect_Create_CreatesFile()
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        var obj = new SqliteProvider(path);
        Assert.False(obj.IsReadOnly);
        Assert.NotNull(obj.Source);

        obj.Connect().Dispose();

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Connect_Exists_OpensFile()
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        File.Open(path, FileMode.OpenOrCreate).Dispose();

        var obj = new SqliteProvider(path);
        Assert.False(obj.IsReadOnly);
        Assert.NotNull(obj.Source);

        obj.Connect().Dispose();
    }

    [Fact]
    public void Connect_Exists_ReadOnly()
    {
        var path = GetTempProviderPath(true);
        using var f = LockPath(path);

        try
        {
            var obj1 = new SqliteProvider(path);
            Assert.True(obj1.IsReadOnly);
            Assert.NotNull(obj1.Source);
            obj1.Connect().Dispose();
        }
        finally
        {
            FreePath(path);
        }
    }

    [Fact]
    public void SizeOnDisk_Succeeds()
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        var obj = new MemoryGarden();
        obj.Open(new SqliteProvider(path));
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);
        obj.Close();

        Assert.True(SqliteProvider.SizeOnDisk(path) > 0.0);
    }
}
