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
using System.Data.Common;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// A chronological sequence of <see cref="GardenLeaf"/> items.
/// </summary>
/// <remarks>
/// This is typically a "chat session" but it doesn't have to be. It could be sequence of non-interactive notes or meta
/// data. Hence the term "deck".
/// </remarks>
public sealed class GardenDeck : IReadOnlyList<GardenLeaf>, IComparable<GardenDeck>
{
    /// <summary>
    /// Maximum child leaf count.
    /// </summary>
    /// <remarks>
    /// We should not allow this to be infinite. Mega-chats may well reach this limit. We are limited, not so much by
    /// memory and ability of UI to render, but by ability to generate context histories. When limited is reached on
    /// natural insertion, we will remove the oldest to make room.
    /// </remarks>
    public const int MaxLeafCount = 1024;

    // Approx memory used by data (inc public properties and ref itself)
    private const long FootprintOverhead = sizeof(long) + sizeof(long) * 8 + 8 * sizeof(int) + 4 + 8;

    // Loaded in order and not expected to contain duplicates.
    private readonly IndexableSet<GardenLeaf> _children = new(4);

    private string? _title;
    private string? _model;
    private string? _folder;
    private string? _titleCache;
    private long _footprint;
    private BasketKind _basket;
    private bool _isPinned;
    private bool _isCurrent;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <exception cref="ArgumentException">Not legal deck</exception>
    public GardenDeck(DeckKind kind, BasketKind origin, bool ephemeral = false)
        : this(kind, origin, default, ephemeral)
    {
    }

    /// <summary>
    /// Constructor. The "offset" is used to set a negative <see cref="SpoofOffset"/> for test purposes only.
    /// </summary>
    /// <exception cref="ArgumentException">Not legal</exception>
    public GardenDeck(DeckKind kind, BasketKind origin, TimeSpan offset, bool ephemeral = false)
    {
        kind.ThrowIfNotLegal();
        origin.ThrowIfNotLegal();

        Kind = kind;
        Origin = origin;
        _basket = origin;
        IsEphemeral = ephemeral;

        SpoofOffset = offset;
        Id = Zuid.New(offset);
        IsLoaded = true;
        Updated = Id.Timestamp;
        VisualCounter = Random.Shared.NextInt64() + 1; // <- +1 intentional
    }

    /// <summary>
    /// Internal "find" stub constructor only.
    /// </summary>
    internal GardenDeck(Zuid id)
    {
        Id = id;
    }

    /// <summary>
    /// Read constructor.
    /// </summary>
    private GardenDeck(MemoryGarden garden, Zuid id, DeckKind kind, BasketKind origin)
    {
        ConditionalDebug.ThrowIfTrue(id.IsEmpty);
        ConditionalDebug.ThrowIfFalse(kind.IsLegal());
        ConditionalDebug.ThrowIfFalse(origin.IsLegal());
        Garden = garden;

        Id = id;
        Kind = kind;
        Origin = origin;
        Updated = id.Timestamp;
        VisualCounter = Random.Shared.NextInt64();
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/> indexer.
    /// </summary>
    public GardenLeaf this[int index]
    {
        get { return _children[index]; }
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/> and return to the open count of messages.
    /// </summary>
    /// <remarks>
    /// The value is 0 until <see cref="Load()"/> is called.
    /// </remarks>
    public int Count
    {
        get { return _children.Count; }
    }

    /// <summary>
    /// Gets the garden owner.
    /// </summary>
    /// <remarks>
    /// The value is null until the instance is inserted into the database using <see
    /// cref="MemoryGarden.Insert(GardenDeck)"/>.
    /// </remarks>
    public MemoryGarden? Garden { get; private set; }

    /// <summary>
    /// Gets whether the sequence container children are loaded in memory.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Gets whether data will be written to storage.
    /// </summary>
    public bool IsPersistant
    {
        get { return Garden?.IsPersistant == true && !IsEphemeral; }
    }

    /// <summary>
    /// Gets whether the instance is current (selected).
    /// </summary>
    /// <remarks>
    /// The term "current" here refers to that which is the focus of attention in the user interface. Changing <see
    /// cref="IsCurrent"/> from false to true also sets <see cref="IsLoaded"/> to true, and unselects any previously
    /// current item.
    /// </remarks>
    public bool IsCurrent
    {
        get { return _isCurrent; }

        set
        {
            if (_isCurrent != value)
            {
                if (value)
                {
                    Load();
                }

                _isCurrent = value;
                Garden?.OnCurrentChanged(value ? this : null);
            }
        }
    }

    /// <summary>
    /// Gets the unique identifier which also provides creation time.
    /// </summary>
    public Zuid Id { get; private set; }

    /// <summary>
    /// Gets the offset used to spoof historical data in test.
    /// </summary>
    public TimeSpan SpoofOffset { get; private set; }

    /// <summary>
    /// Gets or sets the basket identifier and orthogonal options.
    /// </summary>
    public DeckKind Kind { get; }

    /// <summary>
    /// Gets the basket identifier as first inserted.
    /// </summary>
    public BasketKind Origin { get; }

    /// <summary>
    /// Gets or sets the current basket identifier.
    /// </summary>
    public BasketKind Basket
    {
        get { return _basket; }

        set
        {
            if (_basket != value)
            {
                _basket = value;
                OnModified(DeckMods.Basket);
            }
        }
    }

    /// <summary>
    /// Gets the UTC update time that content was added or modified on child items.
    /// </summary>
    /// <remarks>
    /// The <see cref="Updated"/> is updated by <see cref="Append"/>. The value is not updated in response to
    /// metainformation changes, such as <see cref="Title"/>.
    /// </remarks>
    public DateTime Updated { get; private set; }

    /// <summary>
    /// Gets whether the item is ephemeral.
    /// </summary>
    /// <remarks>
    /// Set on construction only and not stored in database.
    /// </remarks>
    public bool IsEphemeral { get; }

    /// <summary>
    /// Gets or sets whether the item is in the waste basket.
    /// </summary>
    public bool IsPinned
    {
        get { return _isPinned; }
        set
        {
            if (_isPinned != value)
            {
                _isPinned = value;
                OnModified(DeckMods.Pinned);
            }
        }
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <remarks>
    /// Setting an empty string assigns null.
    /// </remarks>
    public string? Title
    {
        get { return _title; }

        set
        {
            value = MemoryGarden.Sanitize(value, MemoryGarden.MaxMetaLength);

            if (_title != value)
            {
                _title = value;
                OnModified(DeckMods.Title);
            }
        }
    }

    /// <summary>
    /// Gets or sets the default assistant model.
    /// </summary>
    public string? Model
    {
        get { return _model; }

        set
        {
            value = MemoryGarden.Sanitize(value, MemoryGarden.MaxMetaLength);

            if (_model != value)
            {
                _model = value;
                OnModified(DeckMods.Model);
            }
        }
    }

    /// <summary>
    /// Gets or sets a folder tag.
    /// </summary>
    /// <remarks>
    /// Setting an empty string sets null.
    /// </remarks>
    public string? Folder
    {
        get { return _folder; }

        set
        {
            value = MemoryGarden.Sanitize(value, MemoryGarden.MaxMetaLength);

            if (_folder != value)
            {
                Garden?.GetBasket(_basket).MoveFolderCache(this, value);

                _folder = value;
                OnModified(DeckMods.Folder);
            }
        }
    }

    /// <summary>
    /// Gets an approximate figure for the memory consumed in bytes by this instance.
    /// </summary>
    /// <remarks>
    /// Intended a means to detect high usage only.
    /// </remarks>
    public long Footprint
    {
        get
        {
            if (_footprint > 0)
            {
                return _footprint;
            }

            long mem = FootprintOverhead + _model?.Length * 2L ?? 0 + _title?.Length * 2L ?? 0 +
                _folder?.Length * 2L ?? 0 + SearchSnippet?.Length * 2L ?? 0;

            foreach (var item in _children)
            {
                mem += item.Footprint;
            }

            _footprint = mem;
            return mem;
        }
    }

    /// <summary>
    /// Gets the id of the <see cref="GardenLeaf"/> containing subtext as a result of the last call to <see
    /// cref="SearchContent"/>.
    /// </summary>
    /// <remarks>
    /// The value will be zero where <see cref="SearchSnippet"/> was found in the title.
    /// </remarks>
    public Zuid SearchLeaf { get; private set; }

    /// <summary>
    /// Gets a subtext snippet as a result of the last call to <see cref="SearchContent"/>.
    /// </summary>
    public string? SearchSnippet { get; private set; }

    /// <summary>
    /// Gets a 64-bit value which changes whenever properties are modified which are expected to be seen by the user.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualCounter"/> is initialized to a positive random value on construction. A changed value
    /// indicates that a visual update is necessary, including child items. The value may not change for properties
    /// which may be considered purely "internal" and not visible to the user.
    /// </remarks>
    public long VisualCounter { get; private set; }

    /// <summary>
    /// Gets or sets a tag value expected to hold a visual component reference.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualComponent"/> is not used by <see cref="GardenDeck"/> but simply carried. The instance type is
    /// undefined here.
    /// </remarks>
    public object? VisualComponent { get; set; }

    /// <summary>
    /// Gets or sets visual flags indicating that the item wants attention..
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualSignals"/> is not used by <see cref="GardenDeck"/> but simply carried.
    /// </remarks>
    public SignalFlags VisualSignals { get; set; }

    /// <summary>
    /// Implements <see cref="IComparable{T}"/>.
    /// </summary>
    /// <remarks>
    /// This sorts newest first, unlike <see cref="GardenLeaf.CompareTo(GardenLeaf?)"/> which sorts newest last.
    /// </remarks>
    public int CompareTo(GardenDeck? other)
    {
        if (other == null)
        {
            return -1;
        }

        return other.Id.CompareTo(Id);
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public IEnumerator<GardenLeaf> GetEnumerator()
    {
        return _children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _children.GetEnumerator();
    }

    /// <summary>
    /// Gets the current <see cref="GardenBasket"/> holding this instance.
    /// </summary>
    /// <remarks>
    /// The return value may change when <see cref="Basket"/> changes. The result is null where <see cref="Garden"/>
    /// is null.
    /// </remarks>
    public GardenBasket? GetBasket()
    {
        return Garden?.GetBasket(_basket);
    }

    /// <summary>
    /// Delete the instance from the database and returns true on success.
    /// </summary>
    /// <remarks>
    /// On success, the <see cref="Garden"/> will be null and, while the instance properties will still be valid,
    /// it may no longer interact with the database.
    /// </remarks>
    public bool Delete()
    {
        return Garden?.Delete(this) == true;
    }

    /// <summary>
    /// Updates <see cref="Updated"/> only.
    /// </summary>
    public void Touch()
    {
        Updated = DateTime.UtcNow + SpoofOffset;
        OnModified(DeckMods.Updated);
    }

    /// <summary>
    /// Populates the sequence container with <see cref="GardenLeaf"/> items from the database and returns the leaf
    /// count on return.
    /// </summary>
    public int Load()
    {
        if (!IsLoaded)
        {
            const string NSpace = $"{nameof(GardenDeck)}.{nameof(Load)}";
            ConditionalDebug.WriteLine(NSpace, $"OPEN on {this}");

            if (Garden?.Gardener != null)
            {
                using var con = Garden.Gardener.Connect();
                GardenLeaf.ReadAllDb(con, this, _children);
            }

            _footprint = 0;
            IsLoaded = true;
        }

        return _children.Count;
    }

    /// <summary>
    /// Where <see cref="IsPersistant"/> true, this temporarily discards leaf data as they can be re-loaded on demand.
    /// </summary>
    /// <remarks>
    /// This frees memory when not in used but relies on the ability to <see cref="Load"/> on demand. It does nothing if
    /// <see cref="IsLoaded"/> is already true.
    /// </remarks>
    public void TryUnload()
    {
        if (IsLoaded)
        {
            // Do first (order important)
            IsCurrent = false;

            if (IsPersistant)
            {
                // We can only unload if persistant
                IsLoaded = false;
                _children.Clear();
                _footprint = FootprintOverhead;
            }
        }
    }

    /// <summary>
    /// Gets <see cref="Title"/> where not null or, otherwise, provides an algorithmic value derived from child
    /// contents.
    /// </summary>
    public string GetTitleOrDefault(string fallback = "New")
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(GetTitleOrDefault)}";

        if (_title != null)
        {
            ConditionalDebug.WriteLine(NSpace, "Has title: " + _title);
            _titleCache = null;
            return _title;
        }

        if (_titleCache != null)
        {
            ConditionalDebug.WriteLine(NSpace, "Cached title");
            return _titleCache;
        }

        int count = Math.Min(_children.Count, 10);
        ConditionalDebug.WriteLine(NSpace, "Use sigtext");

        for (int n = 0; n < count; n++)
        {
            var item = _children[n];

            if (item.IsStreaming)
            {
                break;
            }

            if (!item.Kind.IsShown() || item.Content == null)
            {
                continue;
            }

            var t = item.Content.SigText();
            ConditionalDebug.WriteLine(NSpace, "Sigtext: " + t);

            if (!string.IsNullOrEmpty(t))
            {
                ConditionalDebug.WriteLine(NSpace, "Accepted");
                _titleCache = t;
                return t;
            }
        }

        return fallback;
    }

    /// <summary>
    /// Finds subtext in <see cref="Title"/> then <see cref="GardenLeaf.Content"/>, and returns true if matched.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="SearchSnippet"/> and <see cref="SearchLeaf"/>. On success, <see cref="SearchSnippet"/> will be
    /// non-null, while if the match occurred in a leaf, <see cref="SearchLeaf"/> will identify the leaf. On no match,
    /// both these properties will be default values.
    /// </remarks>
    public bool SearchContent(SearchOptions opts)
    {
        SearchLeaf = default;
        SearchSnippet = null;

        if (opts.Subtext.Length == 0)
        {
            return false;
        }

        var snippet = _title?.PrettySearch(opts.Subtext, opts.MaxSnippet, opts.Flags, opts.ScanLimit);

        if (snippet != null)
        {
            // Found in title
            SearchSnippet = snippet;
            return true;
        }

        bool loaded = IsLoaded;

        if (Load() != 0)
        {
            ConditionalDebug.ThrowIfZero(_children.Count);

            foreach (var item in _children)
            {
                snippet = item.Content.PrettySearch(opts.Subtext, opts.MaxSnippet, opts.Flags, opts.ScanLimit);

                if (snippet != null)
                {
                    SearchLeaf = item.Id;
                    SearchSnippet = snippet;
                    return true;
                }
            }

            if (!_isCurrent && !loaded)
            {
                // Close it not found
                TryUnload();
            }
        }

        return false;
    }

    /// <summary>
    /// Appends a new <see cref="GardenLeaf"/> with the given "content" and returns the new intent on success.
    /// </summary>
    /// <remarks>
    /// The method returns null and does nothing where "content" is null, empty or whitespace. Otherwise, calling this
    /// method will ensure <see cref="IsLoaded"/> to true before appending a new <see cref="GardenLeaf"/> item. On
    /// success, the new item will be contained within the <see cref="GardenDeck"/> sequence.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    public GardenLeaf? Append(LeafKind kind, string? content)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(Append)}";
        ConditionalDebug.WriteLine(NSpace, $"APPEND CONTENT: {kind}");
        ConditionalDebug.WriteLine(NSpace, $"Content: " + content?.Truncate(64));

        if (!string.IsNullOrWhiteSpace(content))
        {
            Load();
            _footprint = 0;

            var leaf = new GardenLeaf(this, SpoofOffset, kind, false);

            if (_children.Insert(leaf) > -1)
            {
                SilentRotateIfFull();

                // Setting the content will call
                // this.OnModified() and perform INSERT
                leaf.Content = content;
                return leaf;
            }
        }

        return null;
    }



    /// <summary>
    /// Appends a new item with <see cref="GardenLeaf.IsStreaming"/> initially set to true.
    /// </summary>
    /// <remarks>
    /// Calling this method will always ensure <see cref="IsLoaded"/> to true. The <see cref="GardenLeaf.IsStreaming"/>
    /// will be true on the returned instance, and <see cref="GardenLeaf.Content"/> will be initialized with "chunk0"
    /// (which may be null). The <see cref="GardenLeaf.AppendStream(string?)"/> and <see cref="GardenLeaf.StopStream"/>
    /// should subsequently be called on the resulting instance. Although the new instance is part of this <see
    /// cref="GardenDeck"/> sequence on return, data is not written to the database until <see
    /// cref="GardenLeaf.StopStream"/> is called. Streaming is typically reserved for the <see
    /// cref="LeafKind.Assistant"/> kind.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    public GardenLeaf AppendStream(LeafKind kind, string? chunk0 = null)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(AppendStream)}";
        ConditionalDebug.WriteLine(NSpace, $"APPEND STREAM: {kind}");
        ConditionalDebug.WriteLine(NSpace, $"Content: " + chunk0?.Truncate(64));

        Load();
        _footprint = 0;

        var leaf = new GardenLeaf(this, SpoofOffset, kind, true);

        if (_children.Insert(leaf) > -1)
        {
            SilentRotateIfFull();

            // Appending final chunk will perform INSERT
            leaf.AppendStream(chunk0);
            return leaf;
        }

        // Not expected
        throw new InvalidOperationException("Failed to insert streaming leaf");
    }

    /// <summary>
    /// Resets <see cref="SpoofOffset"/> to 0.
    /// </summary>
    public void ResetSpoof()
    {
        SpoofOffset = default;
    }

    /// <summary>
    /// Overrides and returns <see cref="Title"/> and <see cref="Id"/> for debug purposes.
    /// </summary>
    public override string ToString()
    {
        return string.Concat(Kind, ", ", Basket, ", ", Folder, ", ", Id.ToString(true),
            " (", Id.ToString(false), "), ", Sanitizer.ToDebugSafe(Title, true, true, 32));
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Reads all <see cref="GardenDeck"/> data into the garden.
    /// </summary>
    internal static void ReadAllDb(DbConnection con, MemoryGarden garden, List<GardenDeck> list)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(ReadAllDb)}";
        ConditionalDebug.WriteLine(NSpace, "READ DECK");

        using var cmd = DeckOps.GetReader(con);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            // The design is such that we wish to assign private fields directly.
            // This prevents us from putting this fully in TableOps. Moreover,
            // we ensure data is valid before fully readable.
            var id = new Zuid(reader.GetInt64(DeckOps.IdField));
            var kind = (DeckKind)reader.GetInt32(DeckOps.KindField);
            var origin = (BasketKind)reader.GetInt32(DeckOps.OriginField);
            var basket = (BasketKind)reader.GetInt32(DeckOps.BasketField);

            // This allows us to add values in future while, if careful,
            // allowing older software to be foreward compatible with
            // newer database. We will ignore ill-defined values.
            if (!id.IsEmpty && kind.IsLegal() && origin.IsLegal() && basket.IsLegal())
            {
                var obj = new GardenDeck(garden, id, kind, origin);
                obj.Updated = new DateTime(reader.GetInt64(DeckOps.UpdatedField));
                obj._basket = basket;
                obj._title = reader.GetStringOrNull(DeckOps.TitleField);
                obj._model = reader.GetStringOrNull(DeckOps.ModelField);
                obj._folder = reader.GetStringOrNull(DeckOps.FolderField);
                obj._isPinned = reader.GetBoolean(DeckOps.PinnedField);

                ConditionalDebug.WriteLine(NSpace, $"READ: {obj.Title}");
                list.Add(obj);
            }
        }
    }

    /// <summary>
    /// Inserts this into the database and assigns <see cref="Garden"/>.
    /// </summary>
    /// <remarks>
    /// No change is raised against the garden or basket.
    /// </remarks>
    internal bool InsertDb(MemoryGarden garden)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(InsertDb)}";
        ConditionalDebug.ThrowIfNotNull(Garden);

        // Must set Owner before IsPersistant
        Garden = garden;
        ConditionalDebug.WriteLine(NSpace, "Garden assigned");

        if (IsPersistant)
        {
            ConditionalDebug.WriteLine(NSpace, "Connecting");

            // Null not expected
            using var con = garden.Gardener!.Connect();

            if (!DeckOps.Insert(con, this))
            {
                // Not expected here
                return false;
            }

            ConditionalDebug.WriteLine(NSpace, $"Writing {nameof(GardenLeaf)} children: {_children.Count}");

            foreach (var item in _children)
            {
                if (!item.InsertLeafDb(con))
                {
                    // Not expected here
                    return false;
                }
            }
        }

        ConditionalDebug.WriteLine(NSpace, "Success OK");
        return true;
    }

    /// <summary>
    /// Delete child leaf returns true on success.
    /// </summary>
    /// <remarks>
    /// No change is raised against the garden or basket.
    /// </remarks>
    internal bool DeleteDb(DbConnection? con)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(DeleteDb)}";
        ConditionalDebug.WriteLine(NSpace, "DELETE DECK: " + ToString());

        try
        {
            if (IsPersistant && con != null)
            {
                DeckOps.Delete(con, Id);
            }

            return DetachInternal();
        }
        catch
        {
            DetachInternal();
            throw;
        }
    }

    /// <summary>
    /// Delete child leaf returns true on success.
    /// </summary>
    internal bool DeleteLeafDb(GardenLeaf leaf, bool raiseBasket)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(DeleteLeafDb)}";
        ConditionalDebug.WriteLine(NSpace, "DELETE LEAF: " + leaf);
        ConditionalDebug.ThrowIfTrue(!IsPersistant && leaf.IsPersistant);

        if (leaf.DeckOwner != this || !_children.Remove(leaf))
        {
            ConditionalDebug.WriteLine(NSpace, "Not member");
            return false;
        }

        ConditionalDebug.WriteLine(NSpace, "Detach leaf");
        leaf.DetachInternal();

        if (!leaf.IsPersistant)
        {
            ConditionalDebug.WriteLine(NSpace, "Not persistant (success)");
            OnModifiedInternal(DeckMods.Leaf, null, raiseBasket);
            return true;
        }

        // Gardner cannot be null here
        ConditionalDebug.WriteLine(NSpace, "Connecting");
        using var con = Garden!.Gardener!.Connect();

        bool result = LeafOps.Delete(con, leaf.Id);
        OnModifiedInternal(DeckMods.Leaf, con, raiseBasket);

        ConditionalDebug.WriteLine(NSpace, "Done ok: " + result);
        return result;
    }

    /// <summary>
    /// Sets <see cref="Kind"/> and writes the database without rasing a change against the owner.
    /// </summary>
    internal bool SetBasketNoRaise(DbConnection? con, BasketKind basket)
    {
        if (_basket != basket)
        {
            _basket = basket;
            OnModifiedInternal(DeckMods.Basket, con, false);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets <see cref="Folder"/> and writes the database without rasing a change against the owner.
    /// </summary>
    internal void SetFolderNoRaise(DbConnection? con, string? name)
    {
        // No sanitization
        _folder = name;
        OnModifiedInternal(DeckMods.Folder, con, false);
    }

    /// <summary>
    /// Sets <see cref="IsCurrent"/> to false without rasing a selection change against the owner.
    /// </summary>
    internal void DeselectNoRaise()
    {
        // No callback on parent
        _isCurrent = false;
    }

    /// <summary>
    /// Writes changes to the database.
    /// </summary>
    internal void OnModifiedInternal(DeckMods mods, DbConnection? con, bool raiseBasket)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(OnModifiedInternal)}";
        ConditionalDebug.WriteLine(NSpace, "Mods: " + mods);

        const DeckMods Updatables = DeckMods.Leaf | DeckMods.Basket;
        const DeckMods Footprints = ~(DeckMods.Basket | DeckMods.Updated | DeckMods.Pinned);

        if (mods != DeckMods.None)
        {
            // Must be careful what we call here
            // Until we call Garden.OnUpdated() we are between states.
            if ((mods & Footprints) != 0)
            {
                _footprint = 0;
            }

            if ((mods & Updatables) != 0)
            {
                mods |= DeckMods.Updated;
                Updated = DateTime.UtcNow + SpoofOffset;
            }

            if (con != null && IsPersistant)
            {
                DeckOps.Update(con, this, mods);
            }

            if (mods.IsVisual())
            {
                VisualCounter += 1;
            }

            ConditionalDebug.WriteLine(NSpace, "Call garden");
            Garden?.OnUpdated(this, mods, raiseBasket);

            // AFTER OnUpdated()
            if ((_basket == BasketKind.Waste || _basket == BasketKind.Archive) && mods.HasFlag(DeckMods.Basket))
            {
                // Free memory if moved to waste or archive
                ConditionalDebug.WriteLine(NSpace, "Free memory");
                TryUnload();
            }

        }
    }

    /// <summary>
    /// Sets <see cref="Garden"/> to null, meaning that this instance can no longer write to the database.
    /// </summary>
    internal bool DetachInternal()
    {
        if (Garden != null)
        {
            // Do this first
            IsCurrent = false;

            // Leave child loaded
            Garden = null;
            VisualComponent = null;
            VisualSignals = SignalFlags.None;
            return true;
        }

        return false;
    }

    private void OnModified(DeckMods mods)
    {
        if (IsPersistant && Garden?.Gardener != null && mods != DeckMods.None)
        {
            using DbConnection? con = Garden.Gardener.Connect();
            OnModifiedInternal(mods, con, true);
            return;
        }

        // Inform only
        OnModifiedInternal(mods, null, true);
    }

    private void SilentRotateIfFull()
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(SilentRotateIfFull)}";

        if (_children.Count > MaxLeafCount)
        {
            ConditionalDebug.WriteLine(NSpace, "DECK FULL");
            DeleteLeafDb(_children[0], false);
        }
    }
}