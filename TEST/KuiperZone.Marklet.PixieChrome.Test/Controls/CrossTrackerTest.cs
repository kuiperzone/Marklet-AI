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

using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class CrossTrackerTest
{
    [Fact]
    public void AddInternal_AddsToChildren()
    {
        var obj = new CrossTracker(new Border());
        var b0 = new CrossTextStub();
        Assert.Null(b0.Tracker);

        // Calls Tracker.AddInternal()
        b0.Tracker = obj;

        Assert.Single(obj.Children);
        Assert.Same(obj, b0.Tracker);
        Assert.Equal(0, obj.SelectionCount);

        var b1 = new CrossTextStub();
        b1.Tracker = obj;

        Assert.Equal(2, obj.Children.Count);
        Assert.Same(obj, b0.Tracker);
        Assert.Equal(0, obj.SelectionCount);

        Assert.True(obj.Contains(b0));
        Assert.True(obj.Contains(b1));

        // Already exists
        Assert.Throws<ArgumentException>(() => obj.AddInternal(b1));
    }

    [Fact]
    public void RemoveInternal_RemovesFromChildren()
    {
        var obj = new CrossTracker(new Border());
        var b0 = new CrossTextStub();
        b0.Tracker = obj;
        var b1 = new CrossTextStub();
        b1.Tracker = obj;

        Assert.Equal(2, obj.Children.Count);

        // Calls RemoveInternal()
        b0.Tracker = null;
        Assert.Null(b0.Tracker);
        Assert.Single(obj.Children);

        b1.Tracker = null;
        Assert.Null(b1.Tracker);
        Assert.Empty(obj.Children);
    }

    [Fact]
    public void SelectAll_SelectNone_AddsToSelectionSetsRangeAndClears()
    {
        var obj = new CrossTracker(new Border());

        var b0 = new CrossTextStub();
        b0.Text = "12345";
        b0.Tracker = obj;

        // Empty
        var b1 = new CrossTextStub();
        b1.Tracker = obj;

        var b2 = new CrossTextStub();
        b2.Text = "1";
        b2.Tracker = obj;

        // SELECT
        Assert.True(obj.SelectAll());
        Assert.Equal(3, obj.Children.Count);
        Assert.Equal(3, obj.SelectionCount);
        Assert.Same(b0, obj.FirstSelected);
        Assert.Same(b2, obj.LastSelected);

        Assert.True(b0.HasSelection);
        Assert.True(obj.IsSelected(b0));
        Assert.Equal(0, b0.SelectionStart);
        Assert.Equal(5, b0.SelectionEnd);

        // False as empty (but in range)
        Assert.False(b1.HasSelection);
        Assert.True(obj.IsSelected(b1));

        Assert.True(b2.HasSelection);
        Assert.True(obj.IsSelected(b2));
        Assert.Equal(0, b2.SelectionStart);
        Assert.Equal(1, b2.SelectionEnd);


        // Again - returns false
        Assert.False(obj.SelectAll());
        Assert.Equal(3, obj.SelectionCount);


        // CLEAR
        Assert.True(obj.SelectNone());
        Assert.Equal(3, obj.Children.Count);
        Assert.Equal(0, obj.SelectionCount);
        Assert.Null(obj.FirstSelected);
        Assert.Null(obj.LastSelected);

        Assert.False(b0.HasSelection);
        Assert.False(obj.IsSelected(b0));

        Assert.False(b1.HasSelection);
        Assert.False(obj.IsSelected(b1));

        Assert.False(b2.HasSelection);
        Assert.False(obj.IsSelected(b2));

        // Does nothing
        Assert.False(obj.SelectNone());
    }

    [Fact]
    public void SelectRange_AddsToSelectedAndSetsRange()
    {
        var obj = new CrossTracker(new Border());

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

        Assert.Equal(2, obj.SelectRange(b1, b2));
        Assert.Equal(2, obj.SelectionCount);
        Assert.False(b0.HasSelection);
        Assert.True(b1.HasSelection);
        Assert.True(b2.HasSelection);
        Assert.False(b3.HasSelection);

        Assert.Equal(1, obj.SelectRange(b0));
        Assert.Equal(1, obj.SelectionCount);
        Assert.True(b0.HasSelection);
        Assert.False(b1.HasSelection);
        Assert.False(b2.HasSelection);
        Assert.False(b3.HasSelection);

        // Does nothing with foreign instance
        Assert.Equal(0, obj.SelectRange(b1, new CrossTextStub()));
        Assert.Equal(1, obj.SelectionCount);
    }

    [Fact]
    public void SelectSingle_AddsToSelectedAndSetsRange()
    {
        var obj = new CrossTracker(new Border());

        var b0 = new CrossTextStub();
        b0.Text = "0";
        b0.Tracker = obj;

        var b1 = new CrossTextStub();
        b1.Text = "0123456789";
        b1.Tracker = obj;

        var b2 = new CrossTextStub();
        b2.Text = "0";
        b2.Tracker = obj;

        // Ensure clears
        obj.SelectAll();

        Assert.True(obj.SelectSingle(b1, 1, 4));
        Assert.Equal(1, obj.SelectionCount);
        Assert.False(b0.HasSelection);
        Assert.False(b2.HasSelection);
        Assert.True(b1.HasSelection);
        Assert.Equal(1, b1.SelectionStart);
        Assert.Equal(4, b1.SelectionEnd);

        // Again - does nothing
        Assert.False(obj.SelectSingle(b1, 1, 4));
        Assert.Equal(1, obj.SelectionCount);

        // Reverse
        Assert.True(obj.SelectSingle(b1, 4, 1));
        Assert.Equal(1, obj.SelectionCount);
        Assert.False(b0.HasSelection);
        Assert.False(b2.HasSelection);
        Assert.True(b1.HasSelection);
        Assert.Equal(4, b1.SelectionStart);
        Assert.Equal(1, b1.SelectionEnd);
    }

}
