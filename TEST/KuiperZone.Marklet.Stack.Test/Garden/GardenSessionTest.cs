// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// AUTHOR    : Andrew Thomas
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// LICENSE   : AGPL-3.0-only
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

public class GardenSessionTest : GardenTestBase
{
    private const string DefaultModelName = "def-model";

    [Fact]
    public void Insert_AppendStream_Assistant_StreamsAndStops()
    {
        var obj = OpenNew();

        var child = obj.Insert(BinKind.Home);
        child.Title = "Stream test";
        Assert.Empty(child);
        Assert.True(child.IsOpen);

        // Streaming with empty message allowed
        var leaf = child.AppendStream(LeafKind.Assistant);
        Assert.Same(leaf, child[0]);
        Assert.True(leaf.IsStreaming);
        Assert.False(leaf.IsWritable);
        Assert.Equal("", leaf.Content);

        // First chunk
        leaf.AppendStream("This is a ");
        Assert.True(leaf.IsStreaming);
        Assert.False(leaf.IsWritable);
        Assert.Equal("This is a ", leaf.Content);

        // Next chunk
        leaf.AppendStream("stream message.");
        Assert.Equal("This is a stream message.", leaf.Content);

        // Stop
        Assert.True(leaf.StopStream());
        Assert.False(leaf.IsStreaming);
        Assert.True(leaf.IsWritable);

        // RELOAD
        // Ensure we can read this from empty
        Assert.Equal(OpenInit.Open, obj.Reload());
        child = obj.FindFirstOnTitle("Stream test");
        Assert.NotNull(child);
        Assert.False(child.IsOpen);

        // Must open first
        Assert.Equal(1, child.Open());
        Assert.True(child.IsOpen);
        Assert.Single(child);

        leaf = child[0];
        Assert.Equal("This is a stream message.", leaf.Content);
        Assert.Equal(LeafKind.Assistant, leaf.Kind);

        // No model was assigned to this
        Assert.Null(leaf.Model);
    }

    [Fact]
    public void Insert_Append_User_InvokesEvent()
    {
        var kind = BinKind.Home;

        var obj = OpenNew();
        var child = obj.Insert(kind);
        child.Model = DefaultModelName;
        Assert.Empty(child);
        Assert.True(child.IsOpen);

        var receiver = new ChangeReceiver();
        obj.GetBin(kind).Updated += receiver.BinHandler;

        var leaf = child.Append(LeafKind.User, "Hello world");
        Assert.NotNull(leaf);
        Assert.Single(child);
        Assert.Contains(leaf, child);

        Assert.Equal(1, receiver.BinUpdatedCounter);

        // No model on user leaf
        Assert.Null(child[0].Model);
    }

    [Fact]
    public void Append_Insert_User_ReloadsAndInvokesEvent()
    {
        var obj = OpenNew();

        // Make child outside garden
        var child = new GardenSession();
        child.Model = "model";
        child.Title = "child0";
        child.Append(LeafKind.User, "User message");
        child.Append(LeafKind.Assistant, "Assistant message");

        // Select this instance outside of the garden
        child.IsSelected = true;
        Assert.True(child.IsSelected);

        var receiver = new ChangeReceiver();
        obj.SelectedChanged += receiver.SelectedChangedHandler;
        obj.GetBin(child.HomeBin).Updated += receiver.BinHandler;

        obj.Insert(child);
        Assert.Same(child, obj.First());
        Assert.True(child.IsSelected);
        Assert.Same(child, obj.Selected);

        // Evented
        Assert.Equal(1, receiver.BinUpdatedCounter);
        Assert.NotNull(receiver.SelectedChangedEvent);
        Assert.Same(child, receiver.SelectedChangedEvent.Selected);

        // RELOAD
        obj.Close();
        Assert.Null(obj.Selected);
        Assert.Equal(2, receiver.BinUpdatedCounter);
        Assert.Null(receiver.SelectedChangedEvent.Selected);

        obj.Open();
        Assert.Null(obj.Selected);
        Assert.Equal(3, receiver.BinUpdatedCounter);

        child = obj.FindFirstOnTitle("child0");
        Assert.NotNull(child);
        Assert.Empty(child);

        Assert.Equal(2, child.Open());
        Assert.Equal(LeafKind.User, child[0].Kind);
        Assert.Null(child[0].Model); // <- no model with user message
        Assert.Equal("User message", child[0].Content);

        Assert.Equal(LeafKind.Assistant, child[1].Kind);
        Assert.Equal("model", child[1].Model); // <- holds model
        Assert.Equal("Assistant message", child[1].Content);
    }

    [Fact]
    public void Delete_RemovesFromDatabaseAndInvokesEvent()
    {
        var kind = BinKind.Archive;

        var obj = OpenNew();
        Populate(obj, kind, 5);

        var child = obj.FindFirstOnTitle("1");
        Assert.NotNull(child);
        child.IsSelected = true;
        Assert.Same(child, obj.Selected);

        // Set selected and ensure event
        var receiver = new ChangeReceiver();
        obj.SelectedChanged += receiver.SelectedChangedHandler;
        obj.GetBin(kind).Updated += receiver.BinHandler;

        Assert.True(child.Delete());
        Assert.Null(child.Owner);
        Assert.Null(obj.Selected);

        Assert.Equal(1, receiver.BinUpdatedCounter);
        Assert.NotNull(receiver.SelectedChangedEvent);
        Assert.Null(receiver.SelectedChangedEvent.Selected);
        Assert.Same(child, receiver.SelectedChangedEvent.Previous);

        // RELOAD
        // Ensure we can read this from empty
        Assert.Equal(OpenInit.Open, obj.Reload());
        Assert.True(obj.Count > 1);
        Assert.NotNull(obj.FindFirstOnTitle("0"));
        Assert.Null(obj.FindFirstOnTitle("1"));
    }

}
