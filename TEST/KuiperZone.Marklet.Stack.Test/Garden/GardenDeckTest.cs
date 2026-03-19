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

public class GardenDeckTest : GardenTestBase
{
    private const string DefaultModelName = "def-model";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Insert_AppendStream_Assistant_StreamsAndStops(bool backing)
    {
        var obj = OpenNew(backing);

        var child = obj.Insert(new(DeckKind.Chat, BasketKind.Recent));
        child.Title = "Stream test";
        Assert.Empty(child);
        Assert.True(child.IsLoaded);
        var updated = child.Updated;


        // Streaming with empty message allowed
        var leaf = child.AppendStream(LeafKind.Assistant);
        Assert.Same(leaf, child[0]);
        Assert.True(leaf.IsStreaming);
        Assert.False(leaf.IsPersistant);
        Assert.Equal("", leaf.Content);

        // First chunk
        leaf.AppendStream("This is a ");
        Assert.True(leaf.IsStreaming);
        Assert.False(leaf.IsPersistant);
        Assert.Equal("This is a ", leaf.Content);

        // Next chunk
        leaf.AppendStream("stream message.");
        Assert.Equal("This is a stream message.", leaf.Content);

        // Stop
        Assert.True(leaf.StopStream());
        Assert.False(leaf.IsStreaming);

        // Not persistant if no database
        Assert.Equal(backing, leaf.IsPersistant);

        if (backing)
        {
            // RELOAD
            // Ensure we can read this from empty if we have database

            obj.Reload();
            child = obj.FindTitleExact("Stream test");
            Assert.NotNull(child);
            Assert.False(child.IsLoaded);

            // Must open first
            Assert.Equal(1, child.Load());
            Assert.True(child.IsLoaded);
            Assert.Single(child);

            leaf = child[0];
            Assert.Equal("This is a stream message.", leaf.Content);
            Assert.Equal(LeafKind.Assistant, leaf.Kind);

            // No assistant was assigned to this
            Assert.Null(leaf.Assistant);

            // Check updated by insert
            Assert.True(child.Updated > updated);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Insert_Append_User_InvokesEvent(bool backing)
    {
        var obj = OpenNew(backing);
        var child = obj.Insert(new(DeckKind.Chat, BasketKind.Recent));

        child.Model = DefaultModelName;
        Assert.Empty(child);
        Assert.True(child.IsLoaded);
        var updated = child.Updated;

        var receiver = new ChangeReceiver();
        obj.GetBasket(child.Basket).Changed += receiver.BasketHandler;

        var leaf = child.Append(LeafKind.User, "Hello world");
        Assert.NotNull(leaf);
        Assert.Single(child);
        Assert.Contains(leaf, child);

        Assert.Equal(1, receiver.BasketUpdatedCounter);

        // No assistant on user leaf
        Assert.Null(child[0].Assistant);

        // Check updated by insert
        Assert.True(child.Updated > updated);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Append_Insert_User_ReloadsAndInvokesEvent(bool backing)
    {
        var obj = OpenNew(backing);

        var child = new GardenDeck(DeckKind.Chat, BasketKind.Recent);
        child.Model = "model";
        child.Title = "child0";

        // Insert 2 leaves
        Assert.NotNull(child.Append(LeafKind.User, "User message"));
        Assert.Single(child);

        Assert.NotNull(child.Append(LeafKind.Assistant, "Assistant message"));
        Assert.Equal(2, child.Count);

        // Select this instance outside of the garden
        child.IsCurrent = true;
        Assert.True(child.IsCurrent);

        var receiver = new ChangeReceiver();
        obj.CurrentChanged += receiver.CurrentChangedHandler;
        obj.GetBasket(child.Basket).Changed += receiver.BasketHandler;

        obj.Insert(child);
        Assert.True(child.IsCurrent);
        Assert.Same(child, obj.Current);
        Assert.Same(child, obj.FindTitleExact("child0"));

        // Evented
        Assert.Equal(1, receiver.BasketUpdatedCounter);
        Assert.NotNull(receiver.CurrentChangedEvent);
        Assert.Same(child, receiver.CurrentChangedEvent.Current);

        if (backing)
        {
            // RELOAD
            var gardener = obj.Gardener;
            obj.CloseDatabase();
            Assert.Null(obj.Current);
            Assert.Equal(2, receiver.BasketUpdatedCounter);
            Assert.Null(receiver.CurrentChangedEvent.Current);

            obj.OpenDatabase(gardener!);
            Assert.Null(obj.Current);
            Assert.Equal(3, receiver.BasketUpdatedCounter);

            child = obj.FindTitleExact("child0");
            Assert.NotNull(child);
            Assert.Empty(child);

            Assert.Equal(2, child.Load());
            Assert.Equal(LeafKind.User, child[0].Kind);
            Assert.Null(child[0].Assistant); // <- no assistant with user message
            Assert.Equal("User message", child[0].Content);

            Assert.Equal(LeafKind.Assistant, child[1].Kind);
            Assert.Equal("model", child[1].Assistant); // <- holds model
            Assert.Equal("Assistant message", child[1].Content);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Basket_FromHomeToArchiveToWasteAndRestore(bool backing)
    {
        var obj = OpenNew(backing);
        var recent = obj.GetBasket(BasketKind.Recent);
        var archive = obj.GetBasket(BasketKind.Archive);
        var waste = obj.GetBasket(BasketKind.Waste);

        var recentReceiver = new ChangeReceiver();
        recent.Changed += recentReceiver.BasketHandler;

        var archiveReceiver = new ChangeReceiver();
        archive.Changed += archiveReceiver.BasketHandler;

        var wasteReceiver = new ChangeReceiver();
        waste.Changed += wasteReceiver.BasketHandler;


        var child = new GardenDeck(DeckKind.Chat, BasketKind.Recent);
        child.IsCurrent = true;
        child.Append(LeafKind.User, "User message");

        obj.Insert(child);
        Assert.Same(recent, child.GetBasket());
        Assert.Same(child, recent.Find(child.Id));
        Assert.Equal(1, recentReceiver.BasketUpdatedCounter);
        Assert.Equal(0, archiveReceiver.BasketUpdatedCounter);

        // Sets selected
        Assert.Same(child, obj.Current);

        // Move to Archive
        child.Basket = BasketKind.Archive;
        Assert.Null(recent.Find(child.Id));
        Assert.Same(archive, child.GetBasket());
        Assert.Same(child, archive.Find(child.Id));
        Assert.Equal(2, recentReceiver.BasketUpdatedCounter);
        Assert.Equal(1, archiveReceiver.BasketUpdatedCounter);

        // Set waste
        child.Basket = BasketKind.Waste;
        Assert.Null(archive.Find(child.Id));
        Assert.Same(waste, child.GetBasket());
        Assert.Same(child, waste.Find(child.Id));
        Assert.Equal(2, recentReceiver.BasketUpdatedCounter);
        Assert.Equal(2, archiveReceiver.BasketUpdatedCounter);
        Assert.Equal(1, wasteReceiver.BasketUpdatedCounter);

        // Null when moving to waste
        // This might be brittle
        // We now Unload() which deselects Current
        Assert.Null(obj.Current);

        // Restore
        child.Basket = BasketKind.Recent;
        Assert.Null(waste.Find(child.Id));
        Assert.Same(recent, child.GetBasket());
        Assert.Same(child, recent.Find(child.Id));
        Assert.Equal(3, recentReceiver.BasketUpdatedCounter);
        Assert.Equal(2, archiveReceiver.BasketUpdatedCounter);
        Assert.Equal(2, wasteReceiver.BasketUpdatedCounter);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Delete_RemovesFromDatabaseAndInvokesEvent(bool backing)
    {
        var obj = OpenNew(backing);
        Populate(obj, DeckKind.Note, BasketKind.Notes, 5);

        var child = obj.FindTitleExact("1");
        Assert.NotNull(child);

        child.IsCurrent = true;
        Assert.Same(child, obj.Current);

        // Set selected and ensure event
        var receiver = new ChangeReceiver();
        obj.CurrentChanged += receiver.CurrentChangedHandler;
        obj.GetBasket(child.Basket).Changed += receiver.BasketHandler;

        Assert.True(obj.Delete(child));
        Assert.Null(child.Garden);
        Assert.Null(obj.Current);

        Assert.Equal(1, receiver.BasketUpdatedCounter);
        Assert.NotNull(receiver.CurrentChangedEvent);
        Assert.Null(receiver.CurrentChangedEvent.Current);
        Assert.Same(child, receiver.CurrentChangedEvent.Previous);

        if (backing)
        {
            // RELOAD
            // Ensure we can read this from empty
            obj.Reload();
            Assert.True(obj.Count > 1);
            Assert.NotNull(obj.FindTitleExact("0"));
            Assert.Null(obj.FindTitleExact("1"));
        }
    }

}
