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

public class MemoryGardenTest : GardenTestBase
{
    [Theory]
    [InlineData(BinKind.Home)]
    [InlineData(BinKind.Archive)]
    public void Populate_WritesAndReadsBack(BinKind kind)
    {
        const int PopCount = 20;
        var obj = OpenNew();

        Populate(obj, kind, PopCount);
        Assert.Equal(PopCount, obj.Count);

        obj.Close();
        Assert.False(obj.IsOpen);
        Assert.Empty(obj);

        obj.Reload();
        Assert.Equal(PopCount, obj.Count);

        int n = 0;

        foreach (var _ in obj)
        {
            var child = obj.FindFirstOnTitle(n++.ToString());
            Assert.NotNull(child);
            Assert.Equal(kind, child.HomeBin);

            Assert.False(child.IsOpen);
            Assert.NotNull(child.Owner);
            Assert.Empty(child);

            child.Open();

            var leaf = child[0];
            Assert.Equal(LeafKind.User, leaf.Kind);
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

    [Theory]
    [InlineData(BinKind.Home)]
    [InlineData(BinKind.Archive)]
    public void Insert_CreatesChildAndInvokesEvent(BinKind bin)
    {
        var obj = OpenNew();

        var receiver = new ChangeReceiver();
        obj.GetBin(bin).Updated += receiver.BinHandler;

        // First, we will add one but not insert it.
        var child = obj.Insert(bin);
        Assert.Equal(1, receiver.BinUpdatedCounter);

        child.Title = "New title";
        Assert.Equal(2, receiver.BinUpdatedCounter);

        // Now try to re-read it
        obj.Reload();
        Assert.NotNull(obj.FindFirstOnTitle("New title"));
    }

    [Fact]
    public void Prune_RecentTimeout_MovesToWaste()
    {
        var obj = OpenNew();

        var child = obj.Insert(BinKind.Home);
        child.Append(LeafKind.User, "Hello world");
        Assert.False(child.IsWaste);
        Assert.True(obj.HomeBin.Contains(child));

        obj.HomeBin.Timeout = TimeSpan.FromMicroseconds(1);

        Thread.Sleep(2);
        Assert.True(obj.Prune());
        Assert.True(child.IsWaste);
        Assert.False(obj.HomeBin.Contains(child));
        Assert.True(obj.WasteBin.Contains(child));
    }

    [Fact]
    public void Prune_WasteTimeout_DeletesChild()
    {
        var obj = OpenNew();

        var child = obj.Insert(BinKind.Home);
        child.Append(LeafKind.User, "Hello world");

        child.Title = "DeletedTitle";
        child.IsWaste = true;
        Assert.True(obj.WasteBin.Contains(child));

        obj.WasteBin.Timeout = TimeSpan.FromMicroseconds(1);

        Thread.Sleep(2);
        Assert.True(obj.Prune());
        Assert.Null(child.Owner);
        Assert.False(child.IsWritable);
        Assert.False(obj.WasteBin.Contains(child));

        // Check has gone from database
        obj.Reload();
        Assert.Null(obj.FindFirstOnTitle("DeletedTitle"));
    }

    [Fact]
    public void GetSorted_CreationOldestFirst()
    {
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        var first = obj.FindFirstOnTitle("0");
        var last = obj.FindFirstOnTitle("4");
        Assert.NotNull(first);
        Assert.NotNull(last);

        var sorted = obj.HomeBin.GetSortedSessions(GardenSort.CreationOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetSorted_CreationNewestFirst()
    {
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        var first = obj.FindFirstOnTitle("0");
        var last = obj.FindFirstOnTitle("4");
        Assert.NotNull(first);
        Assert.NotNull(last);

        var sorted = obj.HomeBin.GetSortedSessions(GardenSort.CreationNewestFirst);
        Assert.Same(first, sorted[^1]);
        Assert.Same(last, sorted[0]);
    }

    [Fact]
    public void GetSorted_UpdateOldestFirst()
    {
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        var first = obj.FindFirstOnTitle("0");
        var child = obj.FindFirstOnTitle("2");
        Assert.NotNull(first);
        Assert.NotNull(child);

        child.Append(LeafKind.User, "Update");

        var sorted = obj.HomeBin.GetSortedSessions(GardenSort.UpdateOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(child, sorted[^1]);
    }

    [Fact]
    public void GetSorted_UpdateNewestFirst()
    {
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        var last = obj.FindFirstOnTitle("0");
        var childN = obj.FindFirstOnTitle("2");
        Assert.NotNull(last);
        Assert.NotNull(childN);

        childN.Append(LeafKind.User, "Update");

        var sorted = obj.HomeBin.GetSortedSessions(GardenSort.UpdateNewestFirst);
        Assert.Same(childN, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void GetSorted_AccessOldestFirst()
    {
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        var first = obj.FindFirstOnTitle("0");
        var childN = obj.FindFirstOnTitle("2");
        Assert.NotNull(first);
        Assert.NotNull(childN);

        childN.IsSelected = true;

        var sorted = obj.HomeBin.GetSortedSessions(GardenSort.AccessOldestFirst);
        Assert.Same(first, sorted[0]);
        Assert.Same(childN, sorted[^1]);
    }

    [Fact]
    public void GetSorted_AccessNewestFirst()
    {
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        var last = obj.FindFirstOnTitle("0");
        var childN = obj.FindFirstOnTitle("2");
        Assert.NotNull(last);
        Assert.NotNull(childN);

        childN.IsSelected = true;

        var sorted = obj.HomeBin.GetSortedSessions(GardenSort.AccessNewestFirst);
        Assert.Same(childN, sorted[0]);
        Assert.Same(last, sorted[^1]);
    }

    [Fact]
    public void SetSelected_InvokesEvent()
    {
        // Don't need a separate reader
        var obj = OpenNew();
        Populate(obj, BinKind.Home, 5);

        // Initially null
        Assert.Null(obj.Selected);

        var receiver = new ChangeReceiver();
        obj.SelectedChanged += receiver.SelectedChangedHandler;
        obj.SelectedUpdated += receiver.SelectedUpdatedHandler;

        var first = obj.FindFirstOnTitle("0");
        var childN = obj.FindFirstOnTitle("1");
        Assert.NotNull(first);
        Assert.NotNull(childN);

        first.IsSelected = true;
        Assert.True(first.IsSelected);
        Assert.Same(first, obj.Selected);
        Assert.NotNull(receiver.SelectedChangedEvent);
        Assert.Same(first, receiver.SelectedChangedEvent.Selected);
        Assert.Null(receiver.SelectedChangedEvent.Previous);
        Assert.Null(receiver.SelectedUpdatedEvent); // <- remains null
        receiver.Reset();


        childN.IsSelected = true;
        Assert.False(first.IsSelected);
        Assert.True(childN.IsSelected);
        Assert.Same(childN, obj.Selected);
        Assert.NotNull(receiver.SelectedChangedEvent);
        Assert.Same(childN, receiver.SelectedChangedEvent.Selected);
        Assert.Same(first, receiver.SelectedChangedEvent.Previous);
        Assert.Null(receiver.SelectedUpdatedEvent); // <- remains null
        receiver.Reset();

        // Modify selected
        childN.Model = "model change";
        Assert.True(childN.IsSelected);
        Assert.NotNull(receiver.SelectedUpdatedEvent);
        Assert.Same(childN, receiver.SelectedUpdatedEvent.Selected);
        receiver.Reset();

        childN.Append(LeafKind.DisplayMessage, "Message");
        Assert.NotNull(receiver.SelectedUpdatedEvent);
        Assert.Same(childN, receiver.SelectedUpdatedEvent.Selected);
        receiver.Reset();

        // Delete the instance and check selected is null
        Assert.True(childN.Delete());
        Assert.Null(childN.Owner);

        Assert.Null(obj.Selected);
        Assert.NotNull(receiver.SelectedChangedEvent);
        Assert.Null(receiver.SelectedChangedEvent.Selected);
        Assert.Same(childN, receiver.SelectedChangedEvent.Previous);
        Assert.Null(receiver.SelectedUpdatedEvent); // <- remains null
        receiver.Reset();
    }

    [Fact]
    public void GroupNames_CaseInSensitive()
    {
        var obj = OpenNew();
        PopulateWithGroups(obj, BinKind.Archive);
        Assert.Equal([null, "A", "B"], obj.ArchiveBin.GetTopics());
    }

    [Fact]
    public void RenameGroup_RenamesInvokesEvent()
    {
        var obj = OpenNew();
        PopulateWithGroups(obj, BinKind.Archive);
        Assert.Equal([null, "A", "B"], obj.ArchiveBin.GetTopics());

        var receiver = new ChangeReceiver();
        obj.ArchiveBin.Updated += receiver.BinHandler;

        Assert.True(obj.ArchiveBin.RenameTopic("B", "z"));
        Assert.Equal([null, "A", "z"], obj.ArchiveBin.GetTopics());
        Assert.Equal(1, receiver.BinUpdatedCounter);

        Assert.True(obj.ArchiveBin.RenameTopic(null, "C"));
        Assert.Equal(["A", "C", "z"], obj.ArchiveBin.GetTopics());
        Assert.Equal(2, receiver.BinUpdatedCounter);
    }

    [Fact]
    public void RenameGroup_SetsNullAndInvokesEvent()
    {
        var obj = OpenNew();
        PopulateWithGroups(obj, BinKind.Archive);
        Assert.Equal([null, "A", "B"], obj.ArchiveBin.GetTopics());

        var receiver = new ChangeReceiver();
        obj.ArchiveBin.Updated += receiver.BinHandler;

        Assert.True(obj.ArchiveBin.RenameTopic("A", null));
        Assert.Equal([null, "B"], obj.ArchiveBin.GetTopics());
        Assert.Equal(1, receiver.BinUpdatedCounter);

        Assert.True(obj.ArchiveBin.RenameTopic("B", null));
        Assert.Equal([null], obj.ArchiveBin.GetTopics());
        Assert.Equal(2, receiver.BinUpdatedCounter);

        foreach (var item in obj)
        {
            Assert.Null(item.Topic);
            Assert.False(item.IsWaste);
        }
    }

    [Fact]
    public void DeleteGroup_Deletes()
    {
        var obj = OpenNew();
        PopulateWithGroups(obj, BinKind.Archive);
        Assert.Equal([null, "A", "B"], obj.ArchiveBin.GetTopics());

        var receiver = new ChangeReceiver();
        obj.ArchiveBin.Updated += receiver.BinHandler;

        // Does nothing
        Assert.False(obj.ArchiveBin.DeleteTopic("not exist"));
        Assert.Equal(0, receiver.BinUpdatedCounter);
        Assert.Equal(4, obj.Count);

        Assert.True(obj.ArchiveBin.DeleteTopic("b"));
        Assert.Equal(1, receiver.BinUpdatedCounter);
        Assert.Equal(2, obj.Count);

        // Delete empty - same as null
        Assert.True(obj.ArchiveBin.DeleteTopic(""));
        Assert.Equal(["A"], obj.ArchiveBin.GetTopics());
        Assert.Single(obj);

        Assert.Equal(2, receiver.BinUpdatedCounter);
        Assert.Single(obj.ArchiveBin);
        receiver.Reset();
    }

    private static void PopulateWithGroups(MemoryGarden obj, BinKind kind)
    {
        var child = obj.Insert(kind);
        child.Title = "0";
        child.Topic = " B ";
        child.Append(LeafKind.User, "user1");

        child = obj.Insert(kind);
        child.Title = "1";
        child.Topic = "b";
        child.Append(LeafKind.User, "user2");

        child = obj.Insert(kind);
        child.Title = "2";
        child.Append(LeafKind.Assistant, "assistant");

        child = obj.Insert(kind);
        child.Title = "3";
        child.Topic = "A";
        child.Append(LeafKind.User, "user3");
    }

}
