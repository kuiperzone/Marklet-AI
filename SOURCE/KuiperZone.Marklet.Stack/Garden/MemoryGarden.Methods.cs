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

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

// METHODS (partial)
public sealed partial class MemoryGarden : IReadOnlyCollection<GardenBasket>
{
    /// <summary>
    /// Common sanitization method.
    /// </summary>
    /// <remarks>
    /// The result is null if given an empty of whitespace string.
    /// </remarks>
    public static string? Sanitize(string? text, int maxLength, bool trim = true)
    {
        const SanFlags SanFlags = SanFlags.NormC | SanFlags.SubControl;
        text = Sanitizer.Sanitize(text, trim ? SanFlags | SanFlags.Trim : SanFlags, maxLength);
        return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <summary>
    /// Deletes all data, rebuilds the database schema using the given "provider", and returns true on success.
    /// </summary>
    /// <remarks>
    /// Caller must ensure any database instance with same <see cref="IServiceProvider.Source"/> is first closed before
    /// calling. It does nothing and returns false if "provider" is null, or if <see
    /// cref="IServiceProvider.IsReadOnly"/> is true.
    /// </remarks>
    public static bool Purge(IServiceProvider? provider)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Purge)}";
        Diag.WriteLine(NSpace, "PURGE");

        if (provider?.IsReadOnly != false)
        {
            return false;
        }

        return MetaOps.Purge(provider) == GardenStatus.CreatedOk;
    }

    /// <summary>
    /// Tries to open the provider without modification, and reads the schema version of <see cref="IServiceProvider.Source"/>.
    /// </summary>
    /// <summary>
    /// The return is the expected result of calling <see cref="Open"/>. Valid schema versions start at 1 and increment.
    /// </summary>
    public static GardenStatus TryOpen(IServiceProvider provider, out int version)
    {
        return MetaOps.TryOpen(provider, out version);
    }

    /// <summary>
    /// Opens the database, loads <see cref="GardenDeck"/> headers, and sets <see cref="Provider"/> to the instance
    /// supplied.
    /// </summary>
    /// <remarks>
    /// The result is true on success. It does nothing and returns false if "provider" is null or already equals <see
    /// cref="Provider"/>. Otherwise, If the garden is open when called, <see cref="Close()"/> if first called to clear
    /// all cached data.
    /// </remarks>
    public GardenStatus Open(IServiceProvider provider)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Open)}";
        Diag.WriteLine(NSpace, "OPEN GARDEN");
        Diag.WriteLine(NSpace, "Provider: " + provider?.Source);
        Diag.WriteLine(NSpace, "Current status: " + Status);

        if (Provider == provider || provider == null)
        {
            Diag.WriteLine(NSpace, "Do nothing");
            return Status;
        }

        Diag.WriteLine(NSpace, "Initialize");
        using var con = MetaOps.Init(provider, out int _, out GardenStatus status);

        Diag.WriteLine(NSpace, "Close existing");
        SetStatus(status);

        if (con == null)
        {
            return status;
        }

        try
        {
            Diag.ThrowIfFalse(status.IsOpen());

            Diag.WriteLine(NSpace, "Read headers");
            var list = new List<GardenDeck>(64);
            GardenDeck.ReadDb(con, this, list);

            foreach (var item in list)
            {
                // Do not invoke event for every item
                this[item.CurrentBasket].InsertCache(item);
            }

            Provider = provider;
            _name = MetaOps.GetName(con);
        }
        catch (Exception e)
        {
            Diag.WriteLine(NSpace, e);
            SetStatus(GardenStatus.OpenError);
            return GardenStatus.OpenError;
        }

        // Outside catch
        Diag.WriteLine(NSpace, "Garden now open");
        OnChanged(BasketKind.None);

        // Single event per basket for multiple insertions
        foreach (var item in _baskets)
        {
            if (item.Count != 0)
            {
                OnChanged(item.Kind);
            }
        }

        return status;
    }

    /// <summary>
    /// Sets <see cref="Provider"/> to null and discards all cached data.
    /// </summary>
    public void Close()
    {
        SetStatus(GardenStatus.Disconnected);
    }

    /// <summary>
    /// Returns a readonly clone.
    /// </summary>
    /// <remarks>
    /// The clone contains a value copy of the source <see cref="MemoryGarden"/> at the time it is called. It does not
    /// initially read data from <see cref="Provider"/> and is intended to be fast and memory efficient to create. It
    /// can, however, be used to call <see cref="GardenDeck.Open"/> on children and so has access to all <see
    /// cref="GardenLeaf"/> content from a different thread.
    /// </remarks>
    public MemoryGarden CloneReadOnly()
    {
        return new(this);
    }

    /// <summary>
    /// Returns whether both instances in storage contain same data.
    /// </summary>
    /// <remarks>
    /// This method calls <see cref="GardenBasket.IsConcordant(GardenBasket?)"/> on all data instances and returns true
    /// if other contains exactly the same content as this instance. The <see cref="IServiceProvider"/> instance is not
    /// relevant.
    /// </remarks>
    public bool IsConcordant([NotNullWhen(true)] MemoryGarden? other)
    {
        if (other == this)
        {
            return true;
        }

        if (other != null && _name == other._name)
        {
            // We only need to check master here
            for (int n = 0; n < _baskets.Count; ++n)
            {
                if (!_baskets[n].IsConcordant(other._baskets[n]))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Discards all data and re-loads from <see cref="Provider"/>.
    /// </summary>
    /// <remarks>
    /// If <see cref="Provider"/> is null, it does nothing and returns false. Otherwise, it calls <see
    /// cref="Close()"/> followed by <see cref="Open"/> with same <see cref="Provider"/>.
    /// </remarks>
    public bool Reload()
    {
        if (Provider != null)
        {
            var g = Provider;
            Close();
            Open(g);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the garden contains the given item.
    /// </summary>
    public bool Contains(GardenDeck obj)
    {
        return obj.Garden == this;
    }

    /// <summary>
    /// Finds based on the <see cref="GardenDeck.Id"/> or returns null.
    /// </summary>
    public GardenDeck? FindOnId(Zuid id)
    {
        foreach (var item in _baskets)
        {
            var obj = item.FindOnId(id);

            if (obj != null)
            {
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a child instance with matching "title", or returns null.
    /// </summary>
    /// <remarks>
    /// The <see cref="GardenDeck.Title"/> value does not have to be unique and only the most recent in terms of
    /// creation time is returned. Simple match is used. The call always returns null if "title" is null or empty.
    /// </remarks>
    public GardenDeck? FindOnTitle(string? title, StringComparison comparison = StringComparison.Ordinal)
    {
        title = Sanitize(title, MaxMetaLength);

        if (title == null)
        {
            return null;
        }

        foreach (var basket in _baskets)
        {
            // Items otherwise ordered creation first
            foreach (var item in basket)
            {
                if (title.Equals(item.Title, comparison))
                {
                    return item;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Inserts new <see cref="GardenDeck"/> instance and returns true on success.
    /// </summary>
    /// <remarks>
    /// The result is false where the instance is already a member of this or another <see cref="MemoryGarden"/>. The result is true even if <see cref="IsEphemeral"/> is true.
    /// </remarks>
    public bool Insert(GardenDeck obj)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Insert)}";
        Diag.WriteLine(NSpace, "NEW INSTANCE");
        Diag.WriteLine(NSpace, $"Kind: {obj.Format}, {obj.CurrentBasket}");

        if (obj.Garden != null || !this[obj.CurrentBasket].InsertCache(obj))
        {
            // This should reliably determine whether already
            // exists or even a member of other Garden database
            return false;
        }

        obj.InsertDb(this);

        // Insert to assign Garden
        Diag.ThrowIfNull(obj.Garden);
        OnChanged(obj.CurrentBasket, true);

        if (obj.IsFocused)
        {
            OnFocusChanged(obj);
        }

        Diag.WriteLine(NSpace, $"Finished");
        return true;
    }

    /// <summary>
    /// Deletes the given instance and returns true on success.
    /// </summary>
    public bool Delete(GardenDeck item)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Delete)}";
        Diag.WriteLine(NSpace, "DELETE");
        Diag.WriteLine(NSpace, $"Kind: {item.Format}, {item.CurrentBasket}");

        if (!Contains(item))
        {
            Diag.WriteLine(NSpace, "Does not contain");
            return false;
        }

        if (this[item.CurrentBasket].RemoveCache(item))
        {
            OnChanged(item.CurrentBasket, true);

            try
            {
                using var con = Provider?.Connect();
                item.DeleteDb(con);

                // Assume true here
                return true;
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                SetStatus(GardenStatus.Lost);
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a count of the number of items to be pruned.
    /// </summary>
    public int GetPrunableCount(PruneOptions options)
    {
        int count = 0;

        foreach (var item in _baskets)
        {
            count += item.PruneCount(options);
        }

        return count;
    }

    /// <summary>
    /// Prunes items in all baskets and returns removal count.
    /// </summary>
    /// <remarks>
    /// Where "options" is null, the method may be used to reduce excessive memory use.
    /// </remarks>
    public int Prune(PruneOptions? options)
    {
        // Do waste first
        int count = this[BasketKind.Waste].Prune(options);

        foreach (var item in _baskets)
        {
            if (item.Kind != BasketKind.Waste)
            {
                count += item.Prune(options);
            }
        }

        return count;
    }

    /// <summary>
    /// Implements <see cref="IEnumerable{T}.GetEnumerator"/>
    /// </summary>
    public IEnumerator<GardenBasket> GetEnumerator()
    {
        return _baskets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _baskets.GetEnumerator();
    }
}