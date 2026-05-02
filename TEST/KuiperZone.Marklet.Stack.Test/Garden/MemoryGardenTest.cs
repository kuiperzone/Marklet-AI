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

public class MemoryGardenTest : TestBase
{
    [Theory]
    [InlineData(ProviderFlags.Memory)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal | ProviderFlags.SchemaInit1)]
    public void Populate_Reloads(ProviderFlags flags)
    {
        const int PopCount = 20;
        var obj = new MemoryGarden(CreateMemoryProvider(flags));
        Assert.False(obj.IsEphemeral); // will fail for flags = None

        var provider = obj.Provider;
        Assert.NotNull(provider);

        Populate(obj, DeckFormat.Chat, BasketKind.Recent, PopCount);
        Assert.Equal(PopCount, obj.PopulationCount);

        obj.Reload();

        Assert.Same(provider, obj.Provider);
        Assert.False(obj.IsEphemeral);
        Assert.Equal(PopCount, obj.PopulationCount);

        for(int n = 0; n < PopCount; ++n)
        {
            var child = obj.FindOnTitle(n++.ToString());
            Assert.NotNull(child);
            Assert.Equal(DeckFormat.Chat, child.Format);
            Assert.Equal(BasketKind.Recent, child.CurrentBasket);

            var expEphem = flags.HasFlag(ProviderFlags.Memory) ? EphemeralStatus.Persistant : EphemeralStatus.Implicit;
            Assert.Equal(expEphem, child.Ephemeral);
            Assert.False(child.IsOpen);
            Assert.NotNull(child.Garden);
            Assert.Empty(child);

            child.Open();

            var leaf = child[0];
            Assert.Equal(LeafFormat.UserMessage, leaf.Format);
            Assert.Equal("User 0", leaf.Content);

            leaf = child[1];
            Assert.Equal(LeafFormat.AssistantMessage, leaf.Format);
            Assert.Equal("Assistant 1", leaf.Content);

            leaf = child[2];
            Assert.Equal(LeafFormat.UserMessage, leaf.Format);
            Assert.Equal("User 2", leaf.Content);

            leaf = child[3];
            Assert.Equal(LeafFormat.AssistantMessage, leaf.Format);
            Assert.Equal("Assistant 3", leaf.Content);

            leaf = child[4];
            Assert.Equal(LeafFormat.UserMessage, leaf.Format);
            Assert.Equal("User 4", leaf.Content);

            Assert.True(child.Count > 4);
        }
    }

    [Fact]
    public void Populate_AccessibleWithoutProvider()
    {
        const int PopCount = 20;

        var obj = new MemoryGarden();
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, PopCount);
        Assert.Equal(PopCount, obj.PopulationCount);

        Assert.Null(obj.Provider);
        Assert.True(obj.IsEphemeral);
        Assert.Equal(PopCount, obj.PopulationCount);

        int n = 0;

        foreach (var _ in obj[BasketKind.Recent])
        {
            var child = obj.FindOnTitle(n++.ToString());
            Assert.NotNull(child);
            Assert.Equal(DeckFormat.Chat, child.Format);
            Assert.Equal(BasketKind.Recent, child.CurrentBasket);

            Assert.Equal(EphemeralStatus.Implicit, child.Ephemeral);
            Assert.True(child.IsOpen);
            Assert.NotNull(child.Garden);
            Assert.NotEmpty(child);

            var leaf = child[0];
            Assert.Equal(LeafFormat.UserMessage, leaf.Format);
            Assert.Equal("User 0", leaf.Content);

            leaf = child[1];
            Assert.Equal(LeafFormat.AssistantMessage, leaf.Format);
            Assert.Equal("Assistant 1", leaf.Content);

            leaf = child[2];
            Assert.Equal(LeafFormat.UserMessage, leaf.Format);
            Assert.Equal("User 2", leaf.Content);

            leaf = child[3];
            Assert.Equal(LeafFormat.AssistantMessage, leaf.Format);
            Assert.Equal("Assistant 3", leaf.Content);

            leaf = child[4];
            Assert.Equal(LeafFormat.UserMessage, leaf.Format);
            Assert.Equal("User 4", leaf.Content);

            Assert.True(child.Count > 4);
        }
    }

    [Fact]
    public void Populate__Open_IsReadOnlyConcordant()
    {
        var obj0 = new MemoryGarden(CreateProvider());
        Assert.False(obj0.IsEphemeral);

        var provider = obj0.Provider;
        Assert.NotNull(provider);

        Populate(obj0, DeckFormat.Chat, BasketKind.Recent, 5);
        Populate(obj0, DeckFormat.Note, BasketKind.Recent, 5);
        Populate(obj0, DeckFormat.Note, BasketKind.Notes, 5);
        Populate(obj0, DeckFormat.Chat, BasketKind.Archive, 5);
        Populate(obj0, DeckFormat.Chat, BasketKind.Waste, 5);

        var obj1 = new MemoryGarden();
        obj1.Open(new SqliteProvider(provider.Source, ProviderFlags.ReadOnly));
        Assert.True(obj1.IsEphemeral);

        Assert.True(obj0.IsConcordant(obj1));
    }

    [Fact]
    public void Insert_CreatesChildAndInvokesEvent()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        // First, we will add one but not insert it.
        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        Assert.True(obj.Insert(child));
        Assert.Equal(1, receiver.ChangedCounter);

        child.Title = "New title";
        Assert.Equal(2, receiver.ChangedCounter);

        // Now try to re-read it
        obj.Reload();
        Assert.NotNull(obj.Provider);
        Assert.False(obj.IsEphemeral);
        Assert.NotNull(obj.FindOnTitle("New title"));
    }

    [Fact]
    public void FindOnId_Succeeds()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());

        var deck = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        obj.Insert(deck);

        Assert.Same(deck, obj.FindOnId(deck.Id));

        deck = new GardenDeck(DeckFormat.Note, BasketKind.Notes);
        obj.Insert(deck);

        Assert.Same(deck, obj.FindOnId(deck.Id));

        Assert.Null(obj.FindOnId(default));
        Assert.Null(obj.FindOnId(Zuid.New()));
    }

    [Fact]
    public void Purge_Succeeds()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());
        var provider = obj.Provider;
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 100);
        Populate(obj, DeckFormat.Note, BasketKind.Notes, 100);

        Assert.Equal(200, obj.PopulationCount);

        obj.Close();

        Assert.True(MemoryGarden.Purge(provider));

        Assert.NotNull(provider);
        Assert.Equal(GardenStatus.OpenOk, obj.Open(provider));
        Assert.False(obj.IsEphemeral);
        Assert.True(obj.IsEmpty);
    }

    [Fact]
    public void CloneReadOnly_IsConcordant()
    {
        var obj0 = new MemoryGarden(CreateMemoryProvider());
        obj0.Name = "Name0";

        Populate(obj0, DeckFormat.Chat, BasketKind.Recent, 10);
        Populate(obj0, DeckFormat.Note, BasketKind.Notes, 10);

        Assert.Equal(20, obj0.PopulationCount);

        var obj1 = obj0.CloneReadOnly();
        Assert.True(obj0.IsConcordant(obj1));

        foreach(var basket in obj0)
        {
            foreach(var item in basket)
            {
                Assert.True(item.Open() > 0);
            }
        }

        Assert.True(obj0.IsConcordant(obj1));

        var child0 = obj0.FindOnTitle("1");
        Assert.NotNull(child0);
    }

    [Fact]
    public void SetFocused_InvokesEvent()
    {
        // Don't need a separate reader
        var obj = new MemoryGarden();
        Populate(obj, DeckFormat.Chat, BasketKind.Recent, 5);

        // Initially null
        Assert.Null(obj.Focused);

        var receiver = new ChangeReceiver();
        obj.FocusChanged += receiver.FocusChangedHandler;
        obj.FocusedUpdated += receiver.FocusedUpdatedHandler;

        var first = obj.FindOnTitle("0");
        var childN = obj.FindOnTitle("1");
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

        childN.Append(LeafFormat.Notification, "Message");
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
