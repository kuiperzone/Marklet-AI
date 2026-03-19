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

public class GardenBasketTest : GardenTestBase
{
    [Fact]
    public void GetSorted_CreationOldestFirst()
    {
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 5);

        var first = obj.FindTitleExact("0");
        Assert.NotNull(first);

        var last = obj.FindTitleExact("4");
        Assert.NotNull(last);

        var basket = obj.GetBasket(BasketKind.Recent);
        var sorted = basket.GetContents(GardenSort.CreationOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetSorted_CreationNewestFirst()
    {
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 5);

        var first = obj.FindTitleExact("0");
        Assert.NotNull(first);

        var last = obj.FindTitleExact("4");
        Assert.NotNull(last);

        var basket = obj.GetBasket(BasketKind.Recent);
        var sorted = basket.GetContents(GardenSort.CreationNewestFirst);
        Assert.Same(first, sorted[^1]);
        Assert.Same(last, sorted[0]);
    }

    [Fact]
    public void GetSorted_UpdateOldestFirst()
    {
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 5);

        var first = obj.FindTitleExact("0");
        Assert.NotNull(first);

        var child = obj.FindTitleExact("2");
        Assert.NotNull(child);

        child.Append(LeafKind.User, "Update");

        var basket = obj.GetBasket(BasketKind.Recent);
        var sorted = basket.GetContents(GardenSort.UpdateOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(child, sorted[^1]);
    }

    [Fact]
    public void GetSorted_UpdateNewestFirst()
    {
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 5);

        var last = obj.FindTitleExact("0");
        Assert.NotNull(last);

        var childN = obj.FindTitleExact("2");
        Assert.NotNull(childN);

        childN.Append(LeafKind.User, "Update");

        var basket = obj.GetBasket(BasketKind.Recent);
        var sorted = basket.GetContents(GardenSort.UpdateNewestFirst);
        Assert.Same(childN, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetSorted_UpdateNewestPinnedFirst()
    {
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 5);

        var last = obj.FindTitleExact("0");
        Assert.NotNull(last);

        var childN = obj.FindTitleExact("2");
        Assert.NotNull(childN);
        childN.Append(LeafKind.User, "Update");

        var childP = obj.FindTitleExact("3");
        Assert.NotNull(childP);
        childP.IsPinned = true;

        var basket = obj.GetBasket(BasketKind.Recent);
        var sorted = basket.GetContents(GardenSort.UpdateNewestPinnedFirst);
        Assert.Same(childP, sorted[0]);
        Assert.Same(childN, sorted[1]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetFolder_IsOrdered()
    {
        var obj = OpenNew();
        PopulateWithFolder(obj, DeckKind.Chat, BasketKind.Recent);

        var basket = obj.GetBasket(BasketKind.Recent);
        Assert.Equal(["A", "B"], basket.GetFolderNames());
    }

    [Fact]
    public void RenameFolder_RenamesInvokesEvent()
    {
        var obj = OpenNew();
        PopulateWithFolder(obj, DeckKind.Chat, BasketKind.Recent);

        var basket = obj.GetBasket(BasketKind.Recent);
        Assert.Equal(["A", "B"], basket.GetFolderNames());

        var receiver = new ChangeReceiver();
        basket.Changed += receiver.BasketHandler;

        Assert.True(basket.RenameFolders("B", "z"));
        Assert.Equal(["A", "z"], basket.GetFolderNames());
        Assert.Equal(1, receiver.BasketUpdatedCounter);

        Assert.True(basket.RenameFolders(null, "C"));
        Assert.Equal(["A", "C", "z"], basket.GetFolderNames());
        Assert.Equal(2, receiver.BasketUpdatedCounter);
    }

    [Fact]
    public void RenameFolder_SetsNullAndInvokesEvent()
    {
        var obj = OpenNew();
        PopulateWithFolder(obj, DeckKind.Chat, BasketKind.Recent);

        var basket = obj.GetBasket(BasketKind.Recent);
        Assert.Equal(["A", "B"], basket.GetFolderNames());

        var receiver = new ChangeReceiver();
        basket.Changed += receiver.BasketHandler;

        // Not allowed without merge
        Assert.False(basket.RenameFolders("A", null));

        // Set with merge option
        Assert.True(basket.RenameFolders("A", null, true));
        Assert.Equal(["B"], basket.GetFolderNames());
        Assert.Equal(1, receiver.BasketUpdatedCounter);

        Assert.True(basket.RenameFolders("B", null, true));
        Assert.Empty(basket.GetFolderNames());
        Assert.Equal(2, receiver.BasketUpdatedCounter);

        foreach (var item in obj)
        {
            Assert.Null(item.Folder);
        }
    }

    [Fact]
    public void RemoveFolders_Deletes()
    {
        var obj = OpenNew();
        PopulateWithFolder(obj, DeckKind.Chat, BasketKind.Recent);

        var basket = obj.GetBasket(BasketKind.Recent);
        Assert.Equal(["A", "B"], basket.GetFolderNames());

        var receiver = new ChangeReceiver();
        basket.Changed += receiver.BasketHandler;

        // Does nothing
        Assert.False(basket.DeleteFolder("not exist"));
        Assert.Equal(0, receiver.BasketUpdatedCounter);
        Assert.Equal(3, obj.Count);

        Assert.True(basket.DeleteFolder("B"));
        Assert.Equal(1, receiver.BasketUpdatedCounter);
        Assert.Equal(2, obj.Count);

        // Delete empty - same as null
        Assert.True(basket.DeleteFolder(""));
        Assert.Equal(["A"], basket.GetFolderNames());
        Assert.Single(obj);

        Assert.Equal(2, receiver.BasketUpdatedCounter);
        Assert.Single(basket);
        receiver.Reset();
    }

    [Fact]
    public void Prune_RecentTimeout_MovesToWaste()
    {
        var obj = OpenNew();

        var child = obj.Insert(new(DeckKind.Chat, BasketKind.Recent));
        child.Append(LeafKind.User, "Hello world");
        Assert.Equal(BasketKind.Recent, child.Basket);

        var basket = obj.GetBasket(BasketKind.Recent);
        Assert.Same(basket, child.GetBasket());
        Assert.True(basket.Contains(child));

        Thread.Sleep(2);

        var opts = new PruneOptions();
        opts.Period = TimeSpan.FromMicroseconds(1);
        Assert.Equal(1, basket.Prune(opts));

        Assert.Equal(BasketKind.Waste, child.Basket);
        Assert.False(basket.Contains(child));

        Assert.True(obj.GetBasket(BasketKind.Waste).Contains(child));
    }

    [Fact]
    public void Prune_WasteTimeout_DeletesChild()
    {
        var obj = OpenNew();

        var child = obj.Insert(new(DeckKind.Chat, BasketKind.Recent));
        child.Append(LeafKind.User, "Hello world");
        Assert.Equal(BasketKind.Recent, child.Basket);
        Assert.True(child.IsPersistant);

        child.Title = "DeletedTitle";
        child.Basket = BasketKind.Waste;

        var basket = obj.GetBasket(BasketKind.Waste);
        Assert.Same(basket, child.GetBasket());
        Assert.True(basket.Contains(child));

        Thread.Sleep(2);
        var opts = new PruneOptions();
        opts.Period = TimeSpan.FromMicroseconds(1);
        Assert.Equal(1, basket.Prune(opts));

        Assert.Null(child.Garden);
        Assert.False(child.IsPersistant);
        Assert.False(obj.GetBasket(BasketKind.Waste).Contains(child));

        // Check has gone from database
        obj.Reload();
        Assert.Null(obj.FindTitleExact("DeletedTitle"));
    }

    private static void PopulateWithFolder(MemoryGarden obj, DeckKind kind, BasketKind origin)
    {
        var child = obj.Insert(new(kind, origin));
        child.Title = "0";
        child.Folder = " B ";
        child.Append(LeafKind.User, "user1");

        child = obj.Insert(new(kind, origin));
        child.Title = "2";
        child.Append(LeafKind.Assistant, "assistant");

        child = obj.Insert(new(kind, origin));
        child.Title = "3";
        child.Folder = "A";
        child.Append(LeafKind.User, "user2");
    }

}
