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

public abstract class GardenTestBase
{
    protected static void Populate(MemoryGarden obj, BinKind kind, int count)
    {
        for (int n = 0; n < count; ++n)
        {
            var child = new GardenSession(kind);

            // Assign title before insertion (test)
            child.Title = n.ToString();

            obj.Insert(child);

            // Assign model after insertion
            child.Model = "model";

            child.Append(LeafKind.User, "User 0");
            child.Append(LeafKind.Assistant, "Assistant 1");
            child.Append(LeafKind.User, "User 2");
            child.Append(LeafKind.Assistant, "Assistant 3");
            child.Append(LeafKind.User, "User 4");
        }
    }

    protected static MemoryGarden OpenNew()
    {
        var obj = new MemoryGarden(new SqliteGardener());
        obj.Open();
        return obj;
    }

    protected class ChangeReceiver
    {
        public int BinUpdatedCounter;
        public SelectedChangedEventArgs? SelectedChangedEvent;
        public SelectedUpdatedEventArgs? SelectedUpdatedEvent;

        public void Reset()
        {
            BinUpdatedCounter = 0;
            SelectedChangedEvent = null;
            SelectedUpdatedEvent = null;
        }

        public void BinHandler(object? _, EventArgs __)
        {
            BinUpdatedCounter += 1;
        }

        public void SelectedChangedHandler(object? _, SelectedChangedEventArgs e)
        {
            SelectedChangedEvent = e;
        }

        public void SelectedUpdatedHandler(object? _, SelectedUpdatedEventArgs e)
        {
            SelectedUpdatedEvent = e;
        }
    }
}
