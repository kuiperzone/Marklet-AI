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
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Test;

public abstract class GardenTestBase
{
    static GardenTestBase()
    {
        // If we need logging
        ConditionalDebug.EnableNamespace(nameof(MemoryGarden));
        ConditionalDebug.EnableNamespace(nameof(GardenDeck));
        ConditionalDebug.EnableNamespace(nameof(GardenLeaf));
        ConditionalDebug.EnableNamespace(nameof(MetaOps));
        ConditionalDebug.EnableNamespace(nameof(DeckOps));
        ConditionalDebug.EnableNamespace(nameof(LeafOps));
    }

    protected static void Populate(MemoryGarden obj, DeckKind kind, BasketKind origin, int count)
    {
        for (int n = 0; n < count; ++n)
        {
            var child = new GardenDeck(kind, origin);

            // Assign title before insertion (test)
            // We just assign a counter starting at 0
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

    protected static MemoryGarden OpenNew(bool backing = true)
    {
        var obj = new MemoryGarden();

        if (backing)
        {
            obj.OpenDatabase(SqliteGardener.NewMemory());
        }

        return obj;
    }

    protected class ChangeReceiver
    {
        public int BasketUpdatedCounter;
        public CurrentDeckChangedEventArgs? CurrentChangedEvent;
        public CurrentDeckUpdatedEventArgs? CurrentUpdatedEvent;

        public void Reset()
        {
            BasketUpdatedCounter = 0;
            CurrentChangedEvent = null;
            CurrentUpdatedEvent = null;
        }

        public void BasketHandler(object? _, EventArgs __)
        {
            BasketUpdatedCounter += 1;
        }

        public void CurrentChangedHandler(object? _, CurrentDeckChangedEventArgs e)
        {
            CurrentChangedEvent = e;
        }

        public void CurrentUpdatedHandler(object? _, CurrentDeckUpdatedEventArgs e)
        {
            CurrentUpdatedEvent = e;
        }
    }
}
