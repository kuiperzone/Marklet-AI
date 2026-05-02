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

public class GardenLeafTest : TestBase
{
    [Fact]
    public void Delete_DeletesAndInvokesEvent()
    {
        var obj = new MemoryGarden(CreateMemoryProvider());

        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);
        Assert.True(obj.Insert(child));

        var receiver = new ChangeReceiver();
        obj.Changed += receiver.GardenChangedHandler;

        // Insert leaf
        var leaf = child.Append(LeafFormat.UserMessage, "Hello world");
        Assert.False(leaf.IsEphemeral);
        Assert.Equal(1, receiver.ChangedCounter);

        // Insert stream
        var stream = child.Append(LeafFormat.AssistantMessage, "Hello world", LeafFlags.Streaming);
        Assert.True(stream.IsStreaming);
        Assert.False(stream.IsEphemeral);
        Assert.Equal(2, receiver.ChangedCounter);

        // Remove leaf
        Assert.True(leaf.Delete());
        Assert.Equal(3, receiver.ChangedCounter);
        Assert.Single(child);
        Assert.Null(leaf.Garden);
        Assert.True(leaf.IsEphemeral);

        // Does nothing
        Assert.False(leaf.Delete());

        // Remove stream
        Assert.True(stream.Delete());
        Assert.Equal(4, receiver.ChangedCounter);
        Assert.Empty(child);
        Assert.Null(stream.Garden);
        Assert.True(stream.IsEphemeral);
    }

    [Fact]
    public void Delete_Standalone_Deletes()
    {
        var child = new GardenDeck(DeckFormat.Chat, BasketKind.Recent);

        // Insert leaf
        var leaf = child.Append(LeafFormat.UserMessage, "Hello world");
        Assert.True(leaf.IsEphemeral);

        // Insert stream
        var stream = child.Append(LeafFormat.AssistantMessage, "Hello world", LeafFlags.Streaming);
        Assert.True(stream.IsStreaming);
        Assert.True(stream.IsEphemeral);

        // Remove leaf
        Assert.True(leaf.Delete());
        Assert.Single(child);
        Assert.Null(leaf.Garden);
        Assert.True(leaf.IsEphemeral);

        // Does nothing
        Assert.False(leaf.Delete());

        // Remove stream
        Assert.True(stream.Delete());
        Assert.Empty(child);
        Assert.Null(stream.Garden);
        Assert.True(stream.IsEphemeral);
    }

}
