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

public class GardenBasketTest : TestBase
{
    [Fact]
    public void GetContents_CreationOldestFirst()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);

        var first = obj.FindOnTitle("0");
        Assert.NotNull(first);

        var last = obj.FindOnTitle("4");
        Assert.NotNull(last);

        var basket = obj[BasketKind.Recent];
        var sorted = basket.GetContents(GardenSort.CreationOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetContents_CreationNewestFirst()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);

        var first = obj.FindOnTitle("0");
        Assert.NotNull(first);

        var last = obj.FindOnTitle("4");
        Assert.NotNull(last);

        var basket = obj[BasketKind.Recent];
        var sorted = basket.GetContents(GardenSort.CreationNewestFirst);
        Assert.Same(first, sorted[^1]);
        Assert.Same(last, sorted[0]);
    }

    [Fact]
    public void GetContents_UpdateOldestFirst()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);

        var first = obj.FindOnTitle("0");
        Assert.NotNull(first);

        var child = obj.FindOnTitle("2");
        Assert.NotNull(child);

        child.Append(LeafFormat.UserMessage, "Update");

        var basket = obj[BasketKind.Recent];
        var sorted = basket.GetContents(GardenSort.UpdateOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(child, sorted[^1]);
    }

    [Fact]
    public void GetContents_UpdateNewestFirst()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);

        var last = obj.FindOnTitle("0");
        Assert.NotNull(last);

        var childN = obj.FindOnTitle("2");
        Assert.NotNull(childN);

        childN.Append(LeafFormat.UserMessage, "Update");

        var basket = obj[BasketKind.Recent];
        var sorted = basket.GetContents(GardenSort.UpdateNewestFirst);
        Assert.Same(childN, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetContents_UpdateNewestPinnedFirst()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);

        var last = obj.FindOnTitle("0");
        Assert.NotNull(last);

        var childN = obj.FindOnTitle("2");
        Assert.NotNull(childN);
        childN.Append(LeafFormat.UserMessage, "Update");

        var childP = obj.FindOnTitle("3");
        Assert.NotNull(childP);
        childP.IsPinned = true;

        var basket = obj[BasketKind.Recent];
        var sorted = basket.GetContents(GardenSort.UpdateNewestPinnedFirst);
        Assert.Same(childP, sorted[0]);
        Assert.Same(childN, sorted[1]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetFolderNames_IsOrdered()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        PopulateWithFolder(obj, DeckFormat.Chat, BasketKind.Recent);

        var basket = obj[BasketKind.Recent];
        Assert.Equal(["A", "B"], basket.GetFolderNames());
    }

    [Fact]
    public void RenameFolder_RenamesInvokesEvent()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        PopulateWithFolder(obj, DeckFormat.Chat, BasketKind.Recent);

        var basket = obj[BasketKind.Recent];
        Assert.Equal(["A", "B"], basket.GetFolderNames());

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        Assert.True(basket.RenameFolder("B", "z"));
        Assert.Equal(["A", "z"], basket.GetFolderNames());
        Assert.Equal(1, receiver.ChangedCounter);

        Assert.True(basket.RenameFolder(null, "C"));
        Assert.Equal(["A", "C", "z"], basket.GetFolderNames());
        Assert.Equal(2, receiver.ChangedCounter);
    }

    [Fact]
    public void RenameFolder_SetsNullAndInvokesEvent()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        PopulateWithFolder(obj, DeckFormat.Chat, BasketKind.Recent);

        var basket = obj[BasketKind.Recent];
        Assert.Equal(["A", "B"], basket.GetFolderNames());

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        // Not allowed without merge
        Assert.False(basket.RenameFolder("A", null));

        // Set with merge option
        Assert.True(basket.RenameFolder("A", null, true));
        Assert.Equal(["B"], basket.GetFolderNames());
        Assert.Equal(1, receiver.ChangedCounter);

        Assert.True(basket.RenameFolder("B", null, true));
        Assert.Empty(basket.GetFolderNames());
        Assert.Equal(2, receiver.ChangedCounter);

        foreach (var item in obj[BasketKind.Recent])
        {
            Assert.Null(item.Folder);
        }
    }

    [Fact]
    public void RemoveFolder_Deletes()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        PopulateWithFolder(obj, DeckFormat.Chat, BasketKind.Recent);

        var basket = obj[BasketKind.Recent];
        Assert.Equal(["A", "B"], basket.GetFolderNames());

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        // Does nothing
        Assert.False(basket.DeleteFolder("not exist"));
        Assert.Equal(0, receiver.ChangedCounter);
        Assert.Equal(3, obj.PopulationCount);

        Assert.True(basket.DeleteFolder("B"));
        Assert.Equal(1, receiver.ChangedCounter);
        Assert.Equal(2, obj.PopulationCount);

        // Delete empty - same as null
        Assert.True(basket.DeleteFolder(""));
        Assert.Equal(["A"], basket.GetFolderNames());
        Assert.Equal(1, obj.PopulationCount);

        Assert.Equal(2, receiver.ChangedCounter);
        Assert.Single(basket);
        receiver.Reset();
    }

    [Fact]
    public void Prune_RecentTimeout_MovesToWaste()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        Assert.True(obj.Insert(child));

        child.Append(LeafFormat.UserMessage, "Hello world");
        Assert.Equal(BasketKind.Recent, child.CurrentBasket);

        var basket = obj[BasketKind.Recent];
        Assert.Same(basket, child.GetBasket());
        Assert.True(basket.Contains(child));

        Thread.Sleep(2);

        var opts = new PruneOptions();
        opts.Period = TimeSpan.FromMicroseconds(1);
        Assert.Equal(1, basket.Prune(opts));

        Assert.Equal(BasketKind.Waste, child.CurrentBasket);
        Assert.False(basket.Contains(child));

        Assert.True(obj[BasketKind.Waste].Contains(child));
    }

    [Fact]
    public void Prune_WasteTimeout_DeletesChild()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        Assert.True(obj.Insert(child));

        child.Append(LeafFormat.UserMessage, "Hello world");
        Assert.Equal(BasketKind.Recent, child.CurrentBasket);
        Assert.Equal(EphemeralStatus.Persistant, child.Ephemeral);

        child.Title = "DeletedTitle";
        child.CurrentBasket = BasketKind.Waste;

        var basket = obj[BasketKind.Waste];
        Assert.Same(basket, child.GetBasket());
        Assert.True(basket.Contains(child));

        Thread.Sleep(2);
        var opts = new PruneOptions();
        opts.Period = TimeSpan.FromMicroseconds(1);
        Assert.Equal(1, basket.Prune(opts));

        Assert.Null(child.Garden);
        Assert.Equal(EphemeralStatus.Implicit, child.Ephemeral);
        Assert.False(obj[BasketKind.Waste].Contains(child));

        // Check has gone from database
        obj.Reload();
        Assert.Null(obj.FindOnTitle("DeletedTitle"));
    }

    private static void PopulateWithFolder(MemoryGarden obj, DeckFormat format, BasketKind origin)
    {
        var child = new GardenDeck(format, origin);
        Assert.True(obj.Insert(child));

        child.Title = "0";
        child.Folder = " B ";
        child.Append(LeafFormat.UserMessage, "user1");

        child = new GardenDeck(format, origin);
        Assert.True(obj.Insert(child));

        child.Title = "2";
        child.Append(LeafFormat.AssistantMessage, "assistant");

        child = new GardenDeck(format, origin);
        Assert.True(obj.Insert(child));

        child.Title = "3";
        child.Folder = "A";
        child.Append(LeafFormat.UserMessage, "user2");
    }

}
