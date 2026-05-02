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

public class GardenDeckTest : TestBase
{
    private const string DefaultModelName = "def-model";

    [Theory]
    [InlineData(ProviderFlags.None)]
    [InlineData(ProviderFlags.Memory)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal)]
    public void Insert_AppendStream_Assistant_StreamsAndStops(ProviderFlags flags)
    {
        var obj = new MemoryGarden(CreateMemoryProvider(flags));
        Assert.Equal(!flags.HasFlag(ProviderFlags.Memory), obj.IsEphemeral);

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        Assert.True(obj.Insert(child));

        child.Title = "Stream test";
        Assert.Empty(child);
        Assert.True(child.IsOpen);

        var expEphem = flags.HasFlag(ProviderFlags.Memory) ? EphemeralStatus.Persistant : EphemeralStatus.Implicit;
        Assert.Equal(expEphem, child.Ephemeral);
        var updated = child.Updated;


        // Streaming with empty message allowed
        var leaf = child.Append(LeafFormat.AssistantMessage, null, LeafFlags.Streaming);
        Assert.Same(leaf, child[0]);
        Assert.True(leaf.IsStreaming);
        Assert.Null(leaf.Content);

        // First chunk
        leaf.AppendStream("This is a ");
        Assert.True(leaf.IsStreaming);
        Assert.Equal("This is a ", leaf.Content);

        // Next chunk
        leaf.AppendStream("stream message.");
        Assert.Equal("This is a stream message.", leaf.Content);

        // Stop
        Assert.True(leaf.StopStream());
        Assert.False(leaf.IsStreaming);

        if (!obj.IsEphemeral)
        {
            // RELOAD
            // Ensure we can read this from empty if we have database
            obj.Reload();
            Assert.False(obj.IsEphemeral);

            child = obj.FindOnTitle("Stream test");
            Assert.NotNull(child);

            // Closed after reload
            Assert.False(child.IsOpen);

            // Must open
            Assert.Equal(1, child.Open());
            Assert.True(child.IsOpen);
            Assert.Single(child);

            leaf = child[0];
            Assert.Equal("This is a stream message.", leaf.Content);
            Assert.Equal(LeafFormat.AssistantMessage, leaf.Format);

            // No assistant was assigned to this
            Assert.Null(leaf.Assistant);

            // Check updated by insert
            Assert.True(child.Updated > updated);
        }
    }

    [Theory]
    [InlineData(ProviderFlags.None)]
    [InlineData(ProviderFlags.Memory)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal)]
    public void Insert_Append_User_InvokesEvent(ProviderFlags flags)
    {
        var obj = new MemoryGarden(CreateMemoryProvider(flags));

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        Assert.True(obj.Insert(child));

        child.Model = DefaultModelName;
        Assert.Empty(child);
        Assert.True(child.IsOpen);
        var updated = child.Updated;

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        var leaf = child.Append(LeafFormat.UserMessage, "Hello world");
        Assert.NotNull(leaf);
        Assert.Single(child);
        Assert.Contains(leaf, child);

        Assert.Equal(1, receiver.ChangedCounter);
        Assert.Equal(BasketKind.Recent, receiver.LastBasket);

        // No assistant on user leaf
        Assert.Null(child[0].Assistant);

        // Check updated by insert
        Assert.True(child.Updated > updated);
    }

    [Theory]
    [InlineData(ProviderFlags.None)]
    [InlineData(ProviderFlags.Memory)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal)]
    public void Append_Insert_User_ReloadsAndInvokesEvent(ProviderFlags flags)
    {
        var obj = new MemoryGarden(CreateMemoryProvider(flags));
        Assert.Equal(!flags.HasFlag(ProviderFlags.Memory), obj.IsEphemeral);

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        child.Model = "model";
        child.Title = "child0";

        // Insert 2 leaves
        Assert.NotNull(child.Append(LeafFormat.UserMessage, "User message"));
        Assert.Single(child);

        Assert.NotNull(child.Append(LeafFormat.AssistantMessage, "Assistant message"));
        Assert.Equal(2, child.Count);
        Assert.True(child.IsOpen);

        // This ephemeral until we insert it
        Assert.Equal(EphemeralStatus.Implicit, child.Ephemeral);

        // Select this instance outside of the garden
        child.IsFocused = true;
        Assert.True(child.IsFocused);

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;
        obj.FocusChanged += receiver.FocusChangedHandler;

        obj.Insert(child);
        Assert.True(child.IsFocused);
        Assert.Same(child, obj.Focused);
        Assert.Same(child, obj.FindOnTitle("child0"));

        // Now not ephemeral
        var expEphem = flags.HasFlag(ProviderFlags.Memory) ? EphemeralStatus.Persistant : EphemeralStatus.Implicit;
        Assert.Equal(expEphem, child.Ephemeral);

        // Evented
        Assert.Equal(BasketKind.Recent, receiver.LastBasket);
        Assert.NotNull(receiver.FocusChangedEvent);
        Assert.Same(child, receiver.FocusChangedEvent.Current);

        // One change on insert
        // Change counts can be brittle but provides insight correct operation
        Assert.Equal(1, receiver.ChangedCounter);

        if (!obj.IsEphemeral)
        {
            // RELOAD
            var provider = obj.Provider;
            obj.Close();
            Assert.Null(obj.Focused);

            // Plus close and basket clear change count
            Assert.Equal(3, receiver.ChangedCounter);
            Assert.Null(receiver.FocusChangedEvent.Current);

            receiver.Reset();
            obj.Open(provider!);
            Assert.Null(obj.Focused);
            Assert.False(obj.IsEphemeral);

            // Close, Open, + basket insert
            Assert.Equal(3, receiver.ChangedCounter);

            child = obj.FindOnTitle("child0");
            Assert.NotNull(child);
            Assert.Empty(child);

            Assert.Equal(2, child.Open());
            Assert.Equal(LeafFormat.UserMessage, child[0].Format);
            Assert.Null(child[0].Assistant); // <- no assistant with user message
            Assert.Equal("User message", child[0].Content);

            Assert.Equal(LeafFormat.AssistantMessage, child[1].Format);
            Assert.Equal("model", child[1].Assistant); // <- holds model
            Assert.Equal("Assistant message", child[1].Content);
        }
    }

    [Theory]
    [InlineData(ProviderFlags.None)]
    [InlineData(ProviderFlags.Memory)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal)]
    public void Basket_FromHomeToArchiveToWasteAndRestore(ProviderFlags flags)
    {
        var obj = new MemoryGarden(CreateMemoryProvider(flags));

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        var recent = obj[BasketKind.Recent];
        var archive = obj[BasketKind.Archive];
        var waste = obj[BasketKind.Waste];

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        child.IsFocused = true;
        child.Append(LeafFormat.UserMessage, "User message");

        receiver.Reset();
        obj.Insert(child);
        Assert.Same(recent, child.GetBasket());
        Assert.Same(child, recent.FindOnId(child.Id));
        Assert.Equal(1, receiver.ChangedCounter);

        // Sets selected
        Assert.Same(child, obj.Focused);


        // Move to Archive
        receiver.Reset();
        child.CurrentBasket = BasketKind.Archive;
        Assert.Null(recent.FindOnId(child.Id));
        Assert.Same(archive, child.GetBasket());
        Assert.Same(child, archive.FindOnId(child.Id));
        Assert.Equal(2, receiver.ChangedCounter);


        // Set waste
        receiver.Reset();
        child.CurrentBasket = BasketKind.Waste;
        Assert.Null(archive.FindOnId(child.Id));
        Assert.Same(waste, child.GetBasket());
        Assert.Same(child, waste.FindOnId(child.Id));
        Assert.Equal(2, receiver.ChangedCounter);

        // Null when moving to waste
        // This might be brittle
        // We now Unload() which deselects focus
        Assert.Null(obj.Focused);


        // Restore
        receiver.Reset();
        child.CurrentBasket = BasketKind.Recent;
        Assert.Null(waste.FindOnId(child.Id));
        Assert.Same(recent, child.GetBasket());
        Assert.Same(child, recent.FindOnId(child.Id));
        Assert.Equal(2, receiver.ChangedCounter);
    }

    [Theory]
    [InlineData(ProviderFlags.None)]
    [InlineData(ProviderFlags.Memory)]
    [InlineData(ProviderFlags.Memory | ProviderFlags.WalNormal)]
    public void Delete_RemovesFromDatabaseAndInvokesEvent(ProviderFlags flags)
    {
        var obj = new MemoryGarden(CreateMemoryProvider(flags));
        Populate(obj, DeckFormat.Note, BasketKind.Notes, 5);

        var child = obj.FindOnTitle("1");
        Assert.NotNull(child);

        child.IsFocused = true;
        Assert.Same(child, obj.Focused);

        // Set selected and ensure event
        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;
        obj.FocusChanged += receiver.FocusChangedHandler;

        // DELETE
        Assert.True(obj.Delete(child));
        Assert.Null(child.Garden);
        Assert.Null(obj.Focused);

        Assert.Equal(1, receiver.ChangedCounter);
        Assert.NotNull(receiver.FocusChangedEvent);
        Assert.Equal(BasketKind.Notes, receiver.LastBasket);
        Assert.Null(receiver.FocusChangedEvent.Current);
        Assert.Same(child, receiver.FocusChangedEvent.Previous);

        if (!obj.IsEphemeral)
        {
            // RELOAD
            // Ensure we can read this from empty
            obj.Reload();
            Assert.True(obj.PopulationCount > 1);
            Assert.NotNull(obj.FindOnTitle("0"));
            Assert.Null(obj.FindOnTitle("1"));
        }
    }

    [Fact]
    public void Delete_SeparateReaderCannotNotReopen()
    {
        var obj0 = new MemoryGarden(CreateMemoryProvider());
        Populate(obj0, DeckFormat.Note, BasketKind.Notes, 5);

        var child0 = obj0.FindOnTitle("1");
        Assert.NotNull(child0);
        Assert.True(child0.Open() > 0);

        var obj1 = new MemoryGarden(obj0.Provider!.CloneReadOnly());
        Assert.Equal(GardenStatus.Readonly, obj1.Status);

        var child1 = obj1.FindOnTitle("1");
        Assert.NotNull(child1);
        Assert.True(child1.Open() > 0);

        // TRY CLOSE
        Assert.True(child1.Close());
        Assert.Empty(child1);

        // DELETE ON WRITER
        Assert.True(child0.Delete());

        // Now open on child2 fails
        Assert.Equal(0, child1.Open());
    }

    [Fact]
    public void Clone_Equals()
    {
        var obj0 = new GardenDeck(DeckFormat.Chat, BasketKind.Notes, true);
        obj0.Append(LeafFormat.UserMessage, "Hello world", LeafFlags.Streaming);

        // Sanity
        var leaf0 = obj0[0];
        Assert.True(leaf0.IsStreaming);
        Assert.True(leaf0.IsEphemeral);

        var obj1 = obj0.Clone();
        Assert.Equal(obj0, obj1);
    }
}
