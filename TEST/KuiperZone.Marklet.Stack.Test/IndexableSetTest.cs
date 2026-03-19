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

using Xunit.Abstractions;

namespace KuiperZone.Marklet.Stack;

public class IndexableSetTest
{
    private readonly ITestOutputHelper _out;

    public IndexableSetTest(ITestOutputHelper helper)
    {
        _out = helper;
    }

    [Fact]
    public void Constructor_Ok()
    {
        var list = GetSequential();
        list.Reverse();

        var obj = new IndexableSet<int>(list);
        Assert.True(obj.SequenceEqual(GetSequential()));
    }

    [Fact]
    public void Insert_Sequential()
    {
        var obj = new IndexableSet<int>();
        var list = GetSequential();

        for (int n = 0; n < list.Count; ++n)
        {
            Assert.Equal(n, obj.Insert(list[n]));
        }

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.True(list.SequenceEqual(obj));
        Assert.Equal(0, obj.IndexOf(0));
        Assert.Equal(8, obj.IndexOf(8));
        Assert.True(obj.IndexOf(-5) < 0);

        // Reverse
        obj.Clear();
        list.Reverse();

        for (int n = 0; n < list.Count; ++n)
        {
            Assert.Equal(0, obj.Insert(list[n]));
        }

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.Equal(3, obj.IndexOf(3));
    }

    [Fact]
    public void Upsert_Sequential()
    {
        var obj = new IndexableSet<int>();
        var list = GetSequential();

        for (int n = 0; n < list.Count; ++n)
        {
            Assert.Equal(list[n], obj.Upsert(list[n]));
        }

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.True(list.SequenceEqual(obj));
        Assert.Equal(0, obj.IndexOf(0));
        Assert.Equal(8, obj.IndexOf(8));
        Assert.True(obj.IndexOf(-5) < 0);

        // Reverse
        obj.Clear();
        list.Reverse();

        for (int n = 0; n < list.Count; ++n)
        {
            Assert.Equal(0, obj.Upsert(list[n]));
        }

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.Equal(3, obj.IndexOf(3));
    }

    [Fact]
    public void Insert_Shuffled()
    {
        var obj = new IndexableSet<int>();
        var list = GetShuffled();

        for (int n = 0; n < list.Count; ++n)
        {
            Assert.True(obj.Insert(list[n]) > -1);
        }

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.True(obj.SequenceEqual(GetSequential()));
    }

    [Fact]
    public void Insert_Random()
    {
        var obj = new IndexableSet<int>();
        var list = GetRandom10();

        for (int n = 0; n < list.Count; ++n)
        {
            obj.Insert(list[n]);
        }

        // Random from [0, 10)
        Assert.Equal(10, obj.Count);
    }

    [Fact]
    public void InsertMany_Random()
    {
        var obj = new IndexableSet<int>();
        var list = GetRandom10();
        Assert.Equal(10, obj.InsertMany(list));

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.Equal(3, obj.IndexOf(3));
        Assert.True(obj.Contains(3));
        Assert.False(obj.Contains(-1));
        Assert.False(obj.Contains(1000));

        // Random from [0, 10)
        Assert.Equal(10, obj.Count);
    }

    [Fact]
    public void Upsert_Shuffled()
    {
        var obj = new IndexableSet<int>();
        var list = GetShuffled();

        for (int n = 0; n < list.Count; ++n)
        {
            Assert.True(obj.Upsert(list[n]) > -1);
        }

        Assert.True(obj.Insert(list[5]) < 0);
        Assert.True(obj.SequenceEqual(GetSequential()));
    }

    [Fact]
    public void Upsert_Random()
    {
        var obj = new IndexableSet<int>();
        var list = GetRandom10();

        for (int n = 0; n < list.Count; ++n)
        {
            obj.Upsert(list[n]);
        }

        // Random from [0, 10)
        Assert.Equal(10, obj.Count);
    }

    [Fact]
    public void UpsertManyRandom()
    {
        var obj = new IndexableSet<int>();
        var list = GetRandom10();
        Assert.Equal(list.Count, obj.UpsertMany(list));


        Assert.True(obj.Insert(list[5]) < 0);
        Assert.Equal(3, obj.IndexOf(3));
        Assert.True(obj.Contains(3));
        Assert.False(obj.Contains(-1));
        Assert.False(obj.Contains(1000));

        // Random from [0, 10)
        Assert.Equal(10, obj.Count);
    }

    [Fact]
    public void Remove_Random()
    {
        var obj = new IndexableSet<int>();
        var list = GetRandom10();
        Assert.Equal(list.Count, obj.UpsertMany(list));

        Assert.True(obj.Remove(list[3]));
        Assert.Equal(9, obj.Count);

        Assert.False(obj.Remove(-1));
        Assert.False(obj.Remove(101));
        Assert.Equal(9, obj.Count);
    }

    [Fact]
    public void RemoveMany_Random()
    {
        var obj = new IndexableSet<int>();
        var list = GetRandom10();
        Assert.Equal(list.Count, obj.UpsertMany(list));

        int count = obj.Count;
        Assert.Equal(count, obj.RemoveMany(list));
        Assert.Empty(obj);
    }

    private static List<int> GetSequential()
    {
        var list = new List<int>();

        for (int n = 0; n < 100; ++n)
        {
            list.Add(n);
        }

        return list;
    }

    private static List<int> GetShuffled()
    {
        var array = GetSequential().ToArray();
        Random.Shared.Shuffle(array);
        return new(array);
    }

    private static List<int> GetRandom10()
    {
        var list = new List<int>();

        for (int n = 0; n < 100; ++n)
        {
            list.Add(Random.Shared.Next(0, 10));
        }

        return list;
    }

}