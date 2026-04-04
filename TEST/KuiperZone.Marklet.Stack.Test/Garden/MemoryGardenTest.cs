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

public class MemoryGardenTest : GardenTestBase
{
    [Fact]
    public void Populate_Reload_LoadsBack()
    {
        const int PopCount = 20;
        var obj = OpenNew(true);
        Assert.NotNull(obj.Gardener);
        Assert.True(obj.IsPersistant);

        Populate(obj, DeckKind.Chat, BasketKind.Recent, PopCount);
        Assert.Equal(PopCount, obj.Count);

        obj.Reload();

        Assert.NotNull(obj.Gardener);
        Assert.True(obj.IsPersistant);
        Assert.Equal(PopCount, obj.Count);

        int n = 0;

        foreach (var _ in obj)
        {
            var child = obj.FindTitleExact(n++.ToString());
            Assert.NotNull(child);
            Assert.Equal(DeckKind.Chat, child.Kind);

            Assert.True(child.IsPersistant);
            Assert.False(child.IsLoaded);
            Assert.NotNull(child.Garden);
            Assert.Empty(child);

            child.Load();

            var leaf = child[0];
            Assert.Equal(LeafKind.User, leaf.Kind);
            Assert.Equal("User 0", leaf.Content);

            leaf = child[1];
            Assert.Equal(LeafKind.Assistant, leaf.Kind);
            Assert.Equal("Assistant 1", leaf.Content);

            leaf = child[2];
            Assert.Equal(LeafKind.User, leaf.Kind);
            Assert.Equal("User 2", leaf.Content);

            leaf = child[3];
            Assert.Equal(LeafKind.Assistant, leaf.Kind);
            Assert.Equal("Assistant 3", leaf.Content);

            leaf = child[4];
            Assert.Equal(LeafKind.User, leaf.Kind);
            Assert.Equal("User 4", leaf.Content);

            Assert.True(child.Count > 4);
        }
    }

    [Fact]
    public void Populate_AccessibleWithoutDatabase()
    {
        const int PopCount = 20;

        // Do not use NewObj()
        var obj = new MemoryGarden();

        Populate(obj, DeckKind.Chat, BasketKind.Recent, PopCount);
        Assert.Equal(PopCount, obj.Count);

        Assert.Null(obj.Gardener);
        Assert.False(obj.IsPersistant);
        Assert.Equal(PopCount, obj.Count);

        int n = 0;

        foreach (var _ in obj)
        {
            var child = obj.FindTitleExact(n++.ToString());
            Assert.NotNull(child);
            Assert.Equal(DeckKind.Chat, child.Kind);

            Assert.False(child.IsPersistant);
            Assert.True(child.IsLoaded);
            Assert.NotNull(child.Garden);
            Assert.NotEmpty(child);

            var leaf = child[0];
            Assert.Equal(LeafKind.User, leaf.Kind);
            Assert.Equal("User 0", leaf.Content);

            leaf = child[1];
            Assert.Equal(LeafKind.Assistant, leaf.Kind);
            Assert.Equal("Assistant 1", leaf.Content);

            leaf = child[2];
            Assert.Equal(LeafKind.User, leaf.Kind);
            Assert.Equal("User 2", leaf.Content);

            leaf = child[3];
            Assert.Equal(LeafKind.Assistant, leaf.Kind);
            Assert.Equal("Assistant 3", leaf.Content);

            leaf = child[4];
            Assert.Equal(LeafKind.User, leaf.Kind);
            Assert.Equal("User 4", leaf.Content);

            Assert.True(child.Count > 4);
        }
    }

    [Fact]
    public void Insert_CreatesChildAndInvokesEvent()
    {
        var obj = OpenNew();

        var receiver = new ChangeReceiver();
        obj.GetBasket(BasketKind.Recent).Changed += receiver.BasketHandler;

        // First, we will add one but not insert it.
        var child = obj.Insert(new(DeckKind.Chat, BasketKind.Recent));
        Assert.Equal(1, receiver.BasketUpdatedCounter);

        child.Title = "New title";
        Assert.Equal(2, receiver.BasketUpdatedCounter);

        // Now try to re-read it
        obj.Reload();
        Assert.NotNull(obj.Gardener);
        Assert.True(obj.IsPersistant);
        Assert.NotNull(obj.FindTitleExact("New title"));
    }

    [Fact]
    public void FindOnId_Succeeds()
    {
        var obj = OpenNew();

        var deck = new GardenDeck(DeckKind.Chat, BasketKind.Recent);
        obj.Insert(deck);

        Assert.Same(deck, obj.FindOnId(deck.Id));

        deck = new GardenDeck(DeckKind.Note, BasketKind.Notes);
        obj.Insert(deck);

        Assert.Same(deck, obj.FindOnId(deck.Id));

        Assert.Null(obj.FindOnId(default));
        Assert.Null(obj.FindOnId(Zuid.New()));
    }

    [Fact]
    public void Purge_EmptyTables()
    {
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 100);
        Populate(obj, DeckKind.Note, BasketKind.Notes, 100);

        Assert.Equal(200, obj.Count);

        Assert.True(obj.Purge());
        Assert.True(obj.IsPersistant);
        Assert.Empty(obj);

        // Check has gone from database
        obj.Reload();
        Assert.True(obj.IsPersistant);
        Assert.Empty(obj);
    }


    [Fact]
    public void SetFocused_InvokesEvent()
    {
        // Don't need a separate reader
        var obj = OpenNew();
        Populate(obj, DeckKind.Chat, BasketKind.Recent, 5);

        // Initially null
        Assert.Null(obj.Focused);

        var receiver = new ChangeReceiver();
        obj.FocusChanged += receiver.FocusChangedHandler;
        obj.FocusedUpdated += receiver.FocusedUpdatedHandler;

        var first = obj.FindTitleExact("0");
        var childN = obj.FindTitleExact("1");
        Assert.NotNull(first);
        Assert.NotNull(childN);

        first.IsFocused = true;
        Assert.True(first.IsFocused);
        Assert.Same(first, obj.Focused);
        Assert.NotNull(receiver.FocusChangedEvent);
        Assert.Same(first, receiver.FocusChangedEvent.Current);
        Assert.Null(receiver.FocusChangedEvent.Previous);
        Assert.Null(receiver.FocusedUpdatedEvent); // <- remains null
        receiver.Reset();

        childN.IsFocused = true;
        Assert.False(first.IsFocused);
        Assert.True(childN.IsFocused);
        Assert.Same(childN, obj.Focused);
        Assert.NotNull(receiver.FocusChangedEvent);
        Assert.Same(childN, receiver.FocusChangedEvent.Current);
        Assert.Same(first, receiver.FocusChangedEvent.Previous);
        Assert.Null(receiver.FocusedUpdatedEvent); // <- remains null
        receiver.Reset();

        // Modify selected
        childN.Model = "model change";
        Assert.True(childN.IsFocused);
        Assert.NotNull(receiver.FocusedUpdatedEvent);
        Assert.Same(childN, receiver.FocusedUpdatedEvent.Current);
        receiver.Reset();

        childN.Append(LeafKind.PersistantMessage, "Message");
        Assert.NotNull(receiver.FocusedUpdatedEvent);
        Assert.Same(childN, receiver.FocusedUpdatedEvent.Current);
        receiver.Reset();

        childN.Append(LeafKind.EphemeralMessage, "Message");
        Assert.NotNull(receiver.FocusedUpdatedEvent);
        Assert.Same(childN, receiver.FocusedUpdatedEvent.Current);
        receiver.Reset();

        // Delete the instance and check selected is null
        Assert.True(obj.Delete(childN));
        Assert.Null(childN.Garden);

        Assert.Null(obj.Focused);
        Assert.NotNull(receiver.FocusChangedEvent);
        Assert.Null(receiver.FocusChangedEvent.Current);
        Assert.Same(childN, receiver.FocusChangedEvent.Previous);
        Assert.Null(receiver.FocusedUpdatedEvent); // <- remains null
        receiver.Reset();
    }
}
