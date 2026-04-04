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
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// A sequence container for <see cref="GardenDeck"/> items backed by application logic, and a database implementation
/// provided by the <see cref="IMemoryGardener"/> implementation.
/// </summary>
/// <remarks>
/// This and related class are not thread safe. While backed by a database, it does not need one to operate. However, in
/// this case, all data is held in memory and lost on exit. Thanks to Molly Rocket for the helpful metaphor.
/// </remarks>
public sealed class MemoryGarden : IReadOnlyCollection<GardenDeck>
{
    private const long MinBasketAlloc = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Gets the maximum character length of meta and name strings.
    /// </summary>
    /// <remarks>
    /// Strings truncated where exceeded.
    /// </remarks>
    public const int MaxMetaLength = 48;

    /// <summary>
    /// Gets the maximum character length of "summary" text.
    /// </summary>
    /// <remarks>
    /// Strings truncated where exceeded.
    /// </remarks>
    public const int MaxSummaryLength = 16 * 1024; // 16KB

    /// <summary>
    /// Gets the maximum character length of <see cref="GardenLeaf.Content"/>.
    /// </summary>
    /// <remarks>
    /// Strings truncated where exceeded.
    /// </remarks>
    public const int MaxContentLength = 16 * 1024 * 1024; // 16MB

    /// <summary>
    /// Gets maximum length for non-cached binary data.
    /// </summary>
    public const int MaxBinaryLength = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Gets the maximum number of <see cref="GardenLeaf"/> items in <see cref="GardenDeck"/>.
    /// </summary>
    /// <remarks>
    /// Oldest items are to be removed when this limit is reached.
    /// </remarks>
    public const int MaxDeckCount = 1000;

    private readonly IReadOnlyList<GardenDeck> Empty = new List<GardenDeck>();
    private readonly List<GardenBasket> _baskets = new(4);
    private double _allocFactor = 0.025; // <- 2.5% of 16GB, = 410MB

    /// <summary>
    /// Static constructor.
    /// </summary>
    static MemoryGarden()
    {
        var list = new List<BasketKind>(4);
        LegalBaskets = list;

        foreach (var item in Enum.GetValues<BasketKind>())
        {
            if (item.IsLegal() && item != BasketKind.Waste)
            {
                list.Add(item);
            }
        }

        // Waste last
        list.Add(BasketKind.Waste);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MemoryGarden()
    {
        const string NSpace = $"{nameof(MemoryGarden)}.constructor";
        ConditionalDebug.WriteLine(NSpace, "System memory: " + SystemMemory);
        ConditionalDebug.ThrowIfNegativeOrZero(SystemMemory);

        Baskets = _baskets;

        foreach (var item in LegalBaskets)
        {
            if (item.IsLegal())
            {
                _baskets.Add(new(this, item));
            }
        }
    }

    /// <summary>
    /// Gets a sequence of legal <see cref="BasketKind"/> values for use with <see cref="GetBasket"/>.
    /// </summary>
    public static readonly IReadOnlyList<BasketKind> LegalBaskets;

    /// <summary>
    /// Gets total system memory in bytes.
    /// </summary>
    public static readonly long SystemMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

    /// <summary>
    /// Occurs when the <see cref="Focused"/> reference changes, including being set to null.
    /// </summary>
    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

    /// <summary>
    /// Occurs when the properties of <see cref="Focused"/> are modified.
    /// </summary>
    /// <remarks>
    /// The event is invoked only for the instance given by <see cref="Focused"/>. It is not invoked when items are
    /// added or removed. It does occur when <see cref="Focused"/> is null
    /// </remarks>
    public event EventHandler<FocusedUpdatedEventArgs>? FocusedUpdated;

    /// <summary>
    /// Gets a sequence of baskets belonging to this <see cref="MemoryGarden"/> instance.
    /// </summary>
    public IReadOnlyList<GardenBasket> Baskets { get; }

    /// <summary>
    /// Gets the datbase backing "gardener" instance.
    /// </summary>
    public IMemoryGardener? Gardener { get; private set; }

    /// <summary>
    /// Gets whether data added to the garden is persistant.
    /// </summary>
    /// <remarks>
    /// The value is false where <see cref="Gardener"/> is null or where <see cref="IMemoryGardener.IsReadOnly"/> gives
    /// true.
    /// </remarks>
    public bool IsPersistant
    {
        get
        {
            if (Gardener != null)
            {
                return !Gardener.IsReadOnly;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets a value in bytes above which cached <see cref="GardenLeaf"/> data will be restricted on a per basket basis.
    /// </summary>
    /// <remarks>
    /// Once read and accessed, data is cached in memory. While not intended to be perfect, this provides a means to
    /// free memory not in use.
    /// </remarks>
    public long BasketAlloc
    {
        get
        {
            if (double.IsNaN(_allocFactor))
            {
                return SystemMemory;
            }

            return Math.Max((long)(_allocFactor * SystemMemory), MinBasketAlloc);
        }
    }

    /// <summary>
    /// Gets or sets a value expected to be in the range [0.01, 1.0] which permits memory usage on a per-basket basis up
    /// to (very approximately) <see cref="AllocFactor"/> multiplied by <see cref="SystemMemory"/>.
    /// </summary>
    /// <remarks>
    /// This exists only for edge cases where a user runs many mega-chats over an extended period. A value of NaN
    /// implies no limit. Default is 0.025.
    /// </remarks>
    public double AllocFactor
    {
        get { return _allocFactor; }
        set { _allocFactor = Math.Clamp(value, 0.01, 1.0); }
    }

    /// <summary>
    /// Gets whether the data is empty.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            for (int n = 0; n < _baskets.Count; ++n)
            {
                if (!_baskets[n].IsEmpty)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyCollection{T}.Count"/> and gets the total open count.
    /// </summary>
    /// <remarks>
    /// Always returns 0 if <see cref="Gardener"/> is null.
    /// </remarks>
    public int Count
    {
        get
        {
            int count = 0;

            for (int n = 0; n < _baskets.Count; ++n)
            {
                count += _baskets[n].Count;
            }

            return count;
        }
    }

    /// <summary>
    /// Gets the currently selected <see cref="GardenDeck"/> child.
    /// </summary>
    /// <remarks>
    /// The term "focused" means that the item is focus of attention. Initial value is null (none).
    /// </remarks>
    public GardenDeck? Focused { get; private set; }

    /// <summary>
    /// Common sanitization method.
    /// </summary>
    /// <remarks>
    /// The result is null if given an empty of whitespace string.
    /// </remarks>
    public static string? Sanitize(string? text, int maxLength)
    {
        const SanFlags SanFlags = SanFlags.Trim | SanFlags.NormC | SanFlags.SubControl;
        text = Sanitizer.Sanitize(text, SanFlags, maxLength);
        return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <summary>
    /// Determines whether <see cref="OpenDatabase"/> will upgrade the database schema when called.
    /// </summary>
    public static bool IsUpgradeRequired(IMemoryGardener gardener)
    {
        using var con = gardener.Connect();
        var version = MetaOps.ReadSchema(con);
        return version != 0 && version < MetaOps.SchemaVersion;
    }

    /// <summary>
    /// Returns the corresponding <see cref="GardenBasket"/> instance given a <see cref="GardenDeck.Basket"/> value.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid BasketKind</exception>
    public GardenBasket GetBasket(BasketKind kind)
    {
        for(int n = 0; n < _baskets.Count; ++n)
        {
            var item = _baskets[n];

            if (item.Kind == kind)
            {
                return item;
            }
        }

        throw new ArgumentException($"Invalid {nameof(BasketKind)} {kind}", nameof(kind));
    }

    /// <summary>
    /// Returns true if the garden contains the given item.
    /// </summary>
    public bool Contains(GardenDeck obj)
    {
        return Gardener != null && obj.Garden == this;
    }

    /// <summary>
    /// Opens the database, loads <see cref="GardenDeck"/> headers, and sets <see cref="Gardener"/> to the instance
    /// supplied.
    /// </summary>
    /// <remarks>
    /// The result is true on success. It does nothing and returns false if <see cref="Gardener"/> equals the supplied
    /// "gardener" instance. If the garden is already open, <see cref="CloseDatabase"/> if first called to clear all
    /// cached data.
    /// </remarks>
    public bool OpenDatabase(IMemoryGardener gardener)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(OpenDatabase)}";
        ConditionalDebug.WriteLine(NSpace, "OPEN GARDEN");

        if (Gardener == gardener)
        {
            ConditionalDebug.WriteLine(NSpace, "Already open");
            return false;
        }

        ConditionalDebug.WriteLine(NSpace, "Getting connection");
        using var con = gardener.Connect();
        MetaOps.Init(gardener);

        ConditionalDebug.WriteLine(NSpace, "Close existing");
        CloseDatabase();

        ConditionalDebug.ThrowIfNotNull(Gardener);

        ConditionalDebug.WriteLine(NSpace, "Read headers");
        var list = new List<GardenDeck>(64);
        GardenDeck.ReadAllDb(con, this, list);

        foreach (var item in list)
        {
            // Do not invoke event for every item
            GetBasket(item.Basket).InsertCache(item, false);
        }

        Gardener = gardener;
        ConditionalDebug.WriteLine(NSpace, "Garden formally open");

        foreach (var item in _baskets)
        {
            if (item.Count != 0)
            {
                // Single event for multiple insertions
                item.OnChangedInternal(true);
            }
        }

        return true;
    }

    /// <summary>
    /// Sets <see cref="Gardener"/> to null and discards all cached data.
    /// </summary>
    public void CloseDatabase()
    {
        if (Gardener == null && IsEmpty)
        {
            return;
        }

        Gardener = null;

        foreach (var item in _baskets)
        {
            item.ClearCache(true);
        }

        // Discard should clear this
        ConditionalDebug.ThrowIfNotNull(Focused);
    }

    /// <summary>
    /// Discards all data and re-loads from <see cref="Gardener"/>.
    /// </summary>
    /// <remarks>
    /// If <see cref="Gardener"/> is null, it does nothing and returns false. Otherwise, it calls <see
    /// cref="CloseDatabase"/> followed by <see cref="OpenDatabase"/> with same <see cref="Gardener"/>.
    /// </remarks>
    public bool Reload()
    {
        if (Gardener == null)
        {
            return false;
        }

        var g = Gardener;
        CloseDatabase();
        OpenDatabase(g);
        return true;
    }

    /// <summary>
    /// Finds the child or returns null.
    /// </summary>
    /// <remarks>
    /// Matching is simple and case sensitive. The <see cref="GardenDeck.Title"/> value does not have to be unique and
    /// only the most recent in terms of creation is returned. The call always returns null if <see cref="Gardener"/> is
    /// null.
    /// </remarks>
    public GardenDeck? FindOnId(Zuid id)
    {
        if (Gardener != null)
        {
            var stub = new GardenDeck(id);

            foreach (var item in _baskets)
            {
                var obj = item.FindInternal(stub);

                if (obj != null)
                {
                    return obj;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a recent child instance with matching "title", or returns null.
    /// </summary>
    /// <remarks>
    /// The <see cref="GardenDeck.Title"/> value does not have to be unique and only the most recent in terms of
    /// creation time is returned. Simple match is used. The call always returns null if "title" is null or empty.
    /// </remarks>
    public GardenDeck? FindTitleExact(string? title, StringComparison comparison = StringComparison.Ordinal)
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
    /// Inserts new <see cref="GardenDeck"/> instance and returns instance give.
    /// </summary>
    /// <exception cref="ArgumentException">Already member in garden</exception>
    public GardenDeck Insert(GardenDeck obj)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Insert)}";
        ConditionalDebug.WriteLine(NSpace, "NEW INSTANCE");
        ConditionalDebug.WriteLine(NSpace, $"Kind: {obj.Kind}, {obj.Basket}");

        if (obj.Garden != null)
        {
            // This should reliably determine whether already
            // exists or even a member of other Garden database
            throw new ArgumentException("Already member in garden");
        }

        if (!obj.InsertDb(this) || !GetBasket(obj.Basket).InsertCache(obj, true))
        {
            // Unexpected failure here
            throw new InvalidOperationException($"Unexpected failure insert {nameof(GardenDeck)}");
        }

        ConditionalDebug.ThrowIfNull(obj.Garden);

        if (obj.IsFocused)
        {
            OnFocusChanged(obj);
        }

        ConditionalDebug.WriteLine(NSpace, $"Success OK");
        return obj;
    }

    /// <summary>
    /// Deletes the given instance and returns true on success.
    /// </summary>
    public bool Delete(GardenDeck obj)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Delete)}";
        ConditionalDebug.WriteLine(NSpace, "DELETE");
        ConditionalDebug.WriteLine(NSpace, $"Kind: {obj.Kind}, {obj.Basket}");

        if (GetBasket(obj.Basket).RemoveCache(obj, true))
        {
            using var con = Gardener?.Connect();
            return obj.DeleteDb(con);
        }

        return false;
    }

    /// <summary>
    /// Deletes all message data from the database, including all messages, and sets <see cref="Count"/> to 0.
    /// </summary>
    /// <remarks>
    /// Where <see cref="Gardener"/> is null, <see cref="Purge"/> merely discard cache. The result is true if <see
    /// cref="IsEmpty"/> was false when called.
    /// </remarks>
    public bool Purge()
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Purge)}";
        ConditionalDebug.WriteLine(NSpace, "PURGE");

        bool result = !IsEmpty;

        var g = Gardener;
        CloseDatabase();
        ConditionalDebug.ThrowIfNotNull(Focused);

        if (g != null)
        {
            using var con = g.Connect();
            MetaOps.Purge(con);

            OpenDatabase(g);
        }

        return result;
    }

    /// <summary>
    /// Gets a count of the number of items to be pruned.
    /// </summary>
    public int GetCount(PruneOptions options)
    {
        int count = 0;

        foreach (var item in LegalBaskets)
        {
            count += GetBasket(item).PruneCount(options);
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
        int count = GetBasket(BasketKind.Waste).Prune(options);

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
    /// Implements <see cref="IEnumerable{T}.GetEnumerator()"/>.
    /// </summary>
    /// <remarks>
    /// The order of items should be assumed to be arbitrary. Always returns an empty sequence if <see cref="Gardener"/>
    /// is null.
    /// </remarks>
    public IEnumerator<GardenDeck> GetEnumerator()
    {
        return GetEnumerable()?.GetEnumerator() ?? Empty.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Sets <see cref="Focused"/> on the given child and returns true if selection changed.
    /// </summary>
    /// <remarks>
    /// Does nothing if <see cref="Gardener"/> is null.
    /// </remarks>
    internal void OnFocusChanged(GardenDeck? obj)
    {
        if (Focused != obj)
        {
            ConditionalDebug.ThrowIfTrue(obj?.IsFocused == false);

            // Silent
            var old = Focused;
            old?.DeselectNoRaise();

            Focused = obj;
            obj?.GetBasket()?.SetRecentInternal(obj);
            FocusChanged?.Invoke(this, new(obj, old));
        }
    }


    /// <summary>
    /// Called when properties of "obj" have changed.
    /// </summary>
    internal void OnUpdated(GardenDeck obj, DeckMods mods, bool raiseBasket)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(OnUpdated)}";
        ConditionalDebug.WriteLine(NSpace, "Deck: " + obj.ToString());
        ConditionalDebug.WriteLine(NSpace, "Mods: " + mods);

        if (mods != DeckMods.None)
        {
            try
            {
                if (mods.HasFlag(DeckMods.Basket))
                {
                    foreach (var b in _baskets)
                    {
                        if (b.RemoveCache(obj, raiseBasket))
                        {
                            break;
                        }
                    }

                    GetBasket(obj.Basket).InsertCache(obj, raiseBasket);
                    ConditionalDebug.ThrowIfNotSame(obj.Garden, this);
                    return;
                }

                GetBasket(obj.Basket).OnChangedInternal(raiseBasket);
                ConditionalDebug.ThrowIfNotSame(obj.Garden, this);
            }
            finally
            {
                if (Focused == obj)
                {
                    FocusedUpdated?.Invoke(this, new(obj));
                }
            }
        }
    }

    private List<GardenDeck>? GetEnumerable()
    {
        int count = Count;

        if (count != 0)
        {
            var list = new List<GardenDeck>(count);

            for (int n = 0; n < _baskets.Count; ++n)
            {
                list.AddRange(_baskets[n]);
            }

            return list;
        }

        return null;
    }

}