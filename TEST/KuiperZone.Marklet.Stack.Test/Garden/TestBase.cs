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

using System.Runtime.InteropServices;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Stack.Test;

public abstract class TestBase
{
    static TestBase()
    {
        // If we need logging
        //Diag.EnableNamespace(nameof(SqliteProvider));
        //Diag.EnableNamespace(nameof(MemoryGarden));
        //Diag.EnableNamespace(nameof(GardenDeck));
        //Diag.EnableNamespace(nameof(GardenLeaf));
        //Diag.EnableNamespace(nameof(GardenBasket));
        //Diag.EnableNamespace(nameof(MetaOps));
        //Diag.EnableNamespace(nameof(DeckOps));
        //Diag.EnableNamespace(nameof(LeafOps));
    }

    protected static void Populate(MemoryGarden obj, DeckFormat format, BasketKind origin, int count)
    {
        for (int n = 0; n < count; ++n)
        {
            var child = new GardenDeck(format, origin);

            // Assign title before insertion (test)
            // We just assign a counter starting at 0
            child.Title = n.ToString();

            child.Append(LeafFormat.UserMessage, "User 0");
            child.Append(LeafFormat.AssistantMessage, "Assistant 1");

            obj.Insert(child);

            // Assign model after insertion
            child.Model = "model";

            child.Append(LeafFormat.UserMessage, "User 2");
            child.Append(LeafFormat.AssistantMessage, "Assistant 3");
            child.Append(LeafFormat.UserMessage, "User 4");
        }
    }

    protected static SqliteProvider CreateProvider(ProviderFlags flags = ProviderFlags.None)
    {
        return new SqliteProvider(GetTempProviderPath(), flags);
    }

    protected static SqliteProvider? CreateMemoryProvider(ProviderFlags flags = ProviderFlags.Memory | ProviderFlags.WalNormal)
    {
        if (flags.HasFlag(ProviderFlags.Memory))
        {
            return new SqliteProvider("", flags);
        }

        // Cache
        return null;
    }

    protected static string GetTempProviderPath(bool create = false)
    {
        var dir = Path.GetTempPath();
        var file = Path.GetRandomFileName();
        var path = Path.Combine(dir, file);

        if (create)
        {
            File.WriteAllText(path, "");
        }

        return path;
    }

    protected static FileStream? LockPath(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "");
        }

        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
        return null;
    }

    protected static void FreePath(string path)
    {
        const UnixFileMode DefaultMode = UnixFileMode.UserRead  | UnixFileMode.UserWrite |
            UnixFileMode.GroupRead | UnixFileMode.GroupWrite |
            UnixFileMode.OtherRead;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.SetUnixFileMode(path, DefaultMode);
        }
    }

    protected class ChangeReceiver
    {
        public FocusChangedEventArgs? FocusChangedEvent;
        public FocusedUpdatedEventArgs? FocusedUpdatedEvent;
        public int ChangedCounter;
        public BasketKind LastBasket;

        public void Reset()
        {
            ChangedCounter = 0;
            LastBasket = BasketKind.None;
            FocusChangedEvent = null;
            FocusedUpdatedEvent = null;
        }

        public void GardenChangedHandler(object? _, GardenChangedEventArgs e)
        {
            ChangedCounter += 1;
            LastBasket = e.Basket;
        }

        public void FocusChangedHandler(object? _, FocusChangedEventArgs e)
        {
            FocusChangedEvent = e;
        }

        public void FocusedUpdatedHandler(object? _, FocusedUpdatedEventArgs e)
        {
            FocusedUpdatedEvent = e;
        }
    }
}
