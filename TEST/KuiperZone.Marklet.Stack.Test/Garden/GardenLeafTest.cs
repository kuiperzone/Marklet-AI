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

public class GardenLeafTest : GardenTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Delete_Garden_DeletesAndInvokesEvent(bool database)
    {
        var obj = OpenNew(database);
        var child = obj.Insert(new(DeckKind.Chat, BasketKind.Recent));

        var receiver = new ChangeReceiver();
        obj.GetBasket(child.Basket).Changed += receiver.BasketHandler;

        // Insert leaf
        var leaf = child.Append(LeafKind.User, "Hello world");
        Assert.NotNull(leaf);
        Assert.Equal(1, receiver.BasketUpdatedCounter);

        // Insert stream
        var stream = child.AppendStream(LeafKind.Assistant, "Hello world");

        // No event as object not persistant until stream stops
        Assert.Equal(1, receiver.BasketUpdatedCounter);

        // Remove leaf
        Assert.True(leaf.Delete());
        Assert.Equal(2, receiver.BasketUpdatedCounter);
        Assert.Single(child);
        Assert.Null(leaf.Garden);
        Assert.False(leaf.IsPersistant);

        // Does nothing
        Assert.False(leaf.Delete());

        // Remove stream
        Assert.True(stream.Delete());
        Assert.Equal(3, receiver.BasketUpdatedCounter);
        Assert.Empty(child);
        Assert.Null(stream.Garden);
        Assert.False(stream.IsPersistant);
    }

    [Fact]
    public void Delete_Standalone_Deletes()
    {
        var child = new GardenDeck(DeckKind.Chat, BasketKind.Recent);

        // Insert leaf
        var leaf = child.Append(LeafKind.User, "Hello world");
        Assert.NotNull(leaf);

        // Insert stream
        var stream = child.AppendStream(LeafKind.Assistant, "Hello world");

        // Remove leaf
        Assert.True(leaf.Delete());
        Assert.Single(child);
        Assert.Null(leaf.Garden);
        Assert.False(leaf.IsPersistant);

        // Does nothing
        Assert.False(leaf.Delete());

        // Remove stream
        Assert.True(stream.Delete());
        Assert.Empty(child);
        Assert.Null(stream.Garden);
        Assert.False(stream.IsPersistant);
    }

}
