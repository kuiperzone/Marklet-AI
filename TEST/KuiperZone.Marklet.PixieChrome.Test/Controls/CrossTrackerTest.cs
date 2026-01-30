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

using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class CrossTrackerTest
{
    [Fact]
    public void AddInternal_AddsToChildren()
    {
        var obj = new CrossTracker();
        var b0 = new CrossTextStub();
        Assert.Null(b0.Tracker);
        Assert.Equal(0U, b0.TrackKey);

        // Calls Tracker.AddInternal()
        b0.Tracker = obj;

        Assert.Single(obj.Children);
        Assert.Same(obj, b0.Tracker);
        Assert.NotEqual(0U, b0.TrackKey);
        Assert.Empty(obj.Selecting);

        var b1 = new CrossTextStub();
        b1.Tracker = obj;

        Assert.Equal(2, obj.Count);
        Assert.Same(obj, b0.Tracker);
        Assert.NotEqual(0U, b0.TrackKey);
        Assert.Empty(obj.Selecting);

        Assert.True(obj.Contains(b0));
        Assert.True(obj.Contains(b1));

        // Throws
        Assert.Throws<ArgumentException>(() => obj.AddInternal(b1));
    }

    [Fact]
    public void RemoveInternal_RemovesFromChildren()
    {
        var obj = new CrossTracker();
        var b0 = new CrossTextStub();
        b0.Tracker = obj;
        var b1 = new CrossTextStub();
        b1.Tracker = obj;

        Assert.Equal(2, obj.Count);

        // Calls RemoveInternal()
        b0.Tracker = null;
        Assert.Null(b0.Tracker);
        Assert.Equal(0U, b0.TrackKey);
        Assert.Single(obj.Children);

        b1.Tracker = null;
        Assert.Null(b1.Tracker);
        Assert.Equal(0U, b1.TrackKey);
        Assert.Empty(obj.Children);
    }

    [Fact]
    public void SelectNone_ClearsSelected()
    {
        var obj = new CrossTracker();

        var b0 = new CrossTextStub();
        b0.Text = "12345";
        b0.Tracker = obj;

        var b1 = new CrossTextStub();
        b1.Text = "1";
        b1.Tracker = obj;

        Assert.True(obj.SelectAll());
        Assert.True(obj.SelectNone());

        // Second time
        Assert.False(obj.SelectNone());
    }

    [Fact]
    public void Select_AddsToSelectedAndSetsRange()
    {
        var obj = new CrossTracker();

        var b0 = new CrossTextStub();
        b0.Text = "0";
        b0.Tracker = obj;

        var b1 = new CrossTextStub();
        b1.Text = "1";
        b1.Tracker = obj;

        var b2 = new CrossTextStub();
        b2.Text = "2";
        b2.Tracker = obj;

        var b3 = new CrossTextStub();
        b3.Text = "3";
        b3.Tracker = obj;

        // Ensure clears
        obj.SelectAll();

        Assert.Equal(2, obj.Select(b1.TrackKey, b2.TrackKey));
        Assert.False(b0.HasSelection);
        Assert.True(b1.HasSelection);
        Assert.True(b2.HasSelection);
        Assert.False(b3.HasSelection);


        Assert.Equal(1, obj.Select(b0.TrackKey));
        Assert.True(b0.HasSelection);
        Assert.False(b1.HasSelection);
        Assert.False(b2.HasSelection);
        Assert.False(b3.HasSelection);

        Assert.Equal(1, obj.Select(b3.TrackKey));
        Assert.False(b0.HasSelection);
        Assert.False(b1.HasSelection);
        Assert.False(b2.HasSelection);
        Assert.True(b3.HasSelection);
    }

    [Fact]
    public void SelectAll_AddsToSelectedAndSetsRange()
    {
        var obj = new CrossTracker();

        var b0 = new CrossTextStub();
        b0.Text = "12345";
        b0.Tracker = obj;

        var b1 = new CrossTextStub();
        b1.Text = "1";
        b1.Tracker = obj;

        Assert.True(obj.SelectAll());
        Assert.Equal(2, obj.Count);
        Assert.Equal(2, obj.SelectingCount);

        Assert.True(b0.HasSelection);
        Assert.Equal(0, b0.SelectionStart);
        Assert.Equal(5, b0.SelectionEnd);

        Assert.True(b1.HasSelection);
        Assert.Equal(0, b1.SelectionStart);
        Assert.Equal(1, b1.SelectionEnd);
    }

    [Fact]
    public void SelectAll_RemovesFromSelectedAndClearsRange()
    {
        var obj = new CrossTracker();

        var b0 = new CrossTextStub();
        b0.Text = "12345";
        b0.Tracker = obj;

        var b1 = new CrossTextStub();
        b1.Text = "1";
        b1.Tracker = obj;

        Assert.True(obj.SelectAll());
        Assert.Equal(2, obj.SelectingCount);

        obj.SelectNone();
        Assert.Equal(2, obj.Count);
        Assert.Empty(obj.Selecting);
        Assert.False(b0.HasSelection);
        Assert.False(b1.HasSelection);
    }

}
