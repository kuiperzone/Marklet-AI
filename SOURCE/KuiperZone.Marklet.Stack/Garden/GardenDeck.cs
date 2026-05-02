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
public sealed partial class GardenDeck : IReadOnlyList<GardenLeaf>, IComparable<GardenDeck>, IEquatable<GardenDeck>
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
    private const long FootOverhead = sizeof(long) + sizeof(long) * 9 + 8 * sizeof(byte);

    // Loaded in order and not expected to contain duplicates.
    private readonly IndexableSet<GardenLeaf> _children = new(4);

    private string? _title;
    private string? _model;
    private string? _folder;
    private BasketKind _currentBasket;
    private DeckFlags _flags;

    private long _footprint;
    private bool _isFocused;
    private readonly bool _isExplicitEphemeral;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <exception cref="ArgumentException">Not legal kind or origin</exception>
    public GardenDeck(DeckFormat format, BasketKind origin, bool ephemeral = false)
        : this(format, origin, default, ephemeral)
    {
    }

    /// <summary>
    /// Constructor. The "offset" is used to set <see cref="SpoofOffset"/> for test purposes only.
    /// </summary>
    /// <exception cref="ArgumentException">Not legal kind or origin</exception>
    public GardenDeck(DeckFormat format, BasketKind origin, TimeSpan offset, bool ephemeral = false)
    {
        format.ThrowIfNotLegal();
        origin.ThrowIfNotLegal();

        Format = format;
        OriginBasket = origin;
        _currentBasket = origin;
        _isExplicitEphemeral = ephemeral;

        SpoofOffset = offset;
        Id = Zuid.New(offset);
        Updated = Id.Timestamp;

        IsOpen = true;
    }

    /// <summary>
    /// Internal "find" stub constructor only.
    /// </summary>
    private GardenDeck(Zuid id)
    {
        Id = id;
    }

    /// <summary>
    /// Clone constructor.
    /// </summary>
    internal GardenDeck(MemoryGarden? garden, GardenDeck other, GardenLeaf? branch = null)
    {
        Garden = garden;
        Format = other.Format;
        OriginBasket = other.OriginBasket;
        _currentBasket = other._currentBasket;

        _title = other._title;
        _model = other._model;
        _folder = other._folder;
        _footprint = other._footprint;
        _isExplicitEphemeral = other._isExplicitEphemeral;

        bool isBranch;

        if (branch != null)
        {
            isBranch = true;
            Diag.ThrowIfNotSame(this, branch.Owner);

            Id = other.Id.CloneUnique();
            Updated = DateTime.UtcNow;

            // Don't copy Pinned
            _flags = DeckFlags.Branch;
        }
        else
        {
            isBranch = false;
            Id = other.Id;
            Updated = other.Updated;
            _flags = other._flags;
        }

        Diag.ThrowIfTrue(!other.IsOpen && other._children.Count > 0);
        IsOpen = other.IsOpen;

        foreach (var item in other._children)
        {
            _children.Insert(new GardenLeaf(this, item, isBranch));

            if (item == branch)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Read constructor.
    /// </summary>
    private GardenDeck(MemoryGarden garden, Zuid id, DeckFormat format, BasketKind origin)
    {
        Diag.ThrowIfTrue(id.IsEmpty);
        Diag.ThrowIfFalse(format.IsLegal());
        Diag.ThrowIfFalse(origin.IsLegal());

        // IsOpen to be false here
        Diag.ThrowIfTrue(IsOpen);

        Garden = garden;
        Id = id;
        Format = format;
        OriginBasket = origin;
        Updated = id.Timestamp;
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
    /// The result is always 0 if <see cref="IsOpen"/> is false.
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
    /// Gets whether the instance is the focus of user attention.
    /// </summary>
    /// <remarks>
    /// The term "focus" is not to be confused with Control.Focus. Changing <see cref="IsFocused"/> from false to true
    /// also sets <see cref="IsOpen"/> to true, and unfocused any previously focused item.
    /// </remarks>
    public bool IsFocused
    {
        get { return _isFocused; }

        set
        {
            if (_isFocused != value)
            {
                if (value)
                {
                    Open();
                }

                _isFocused = value;
                Garden?.OnFocusChanged(value ? this : null);
            }
        }
    }

    /// <summary>
    /// Gets the unique identifier which also provides creation time.
    /// </summary>
    public Zuid Id { get; private set; }

    /// <summary>
    /// Gets the UTC update time that content was added or modified on child items.
    /// </summary>
    /// <remarks>
    /// The <see cref="Updated"/> is updated by <see cref="Append"/>. The value is not updated in response to
    /// metainformation changes, such as <see cref="Title"/>.
    /// </remarks>
    public DateTime Updated { get; private set; }

    /// <summary>
    /// Gets the offset used to spoof historical data in test.
    /// </summary>
    public TimeSpan SpoofOffset { get; private set; }

    /// <summary>
    /// Gets the basket identifier as first inserted.
    /// </summary>
    public BasketKind OriginBasket { get; }

    /// <summary>
    /// Gets or sets the current basket identifier.
    /// </summary>
    public BasketKind CurrentBasket
    {
        get { return _currentBasket; }

        set
        {
            if (_currentBasket != value)
            {
                _currentBasket = value;
                OnModified(DeckMods.Basket);
            }
        }
    }


    /// <summary>
    /// Gets or sets the format.
    /// </summary>
    public DeckFormat Format { get; }

    /// <summary>
    /// Gets whether the ephemeral status.
    /// </summary>
    public EphemeralStatus Ephemeral
    {
        get
        {
            if (_isExplicitEphemeral)
            {
                return EphemeralStatus.Explicit;
            }

            if (Garden?.Provider?.IsReadOnly == false)
            {
                return EphemeralStatus.Persistant;
            }

            return EphemeralStatus.Implicit;
        }
    }

    /// <summary>
    /// Gets whether <see cref="GardenLeaf"/> children are cached in memory.
    /// </summary>
    /// <remarks>
    /// When <see cref="IsOpen"/> is false, children are loaded on demand by <see cref="Open()"/>. The default is
    /// true (open). Where <see cref="CanClose"/> is true, <see cref="IsOpen"/> becomes false by calling <see
    /// cref="Close"/>,
    /// </remarks>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets whether <see cref="GardenLeaf"/> children can be freed by calling <see cref="Close()"/>.
    /// </summary>
    public bool CanClose
    {
        get
        {
            var e = Ephemeral;
            return e == EphemeralStatus.Persistant || (e == EphemeralStatus.Implicit && Garden?.Provider != null);
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
                Garden?[_currentBasket].MoveFolderCache(this, value);

                _folder = value;
                OnModified(DeckMods.Folder);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the item is in the waste basket.
    /// </summary>
    /// <remarks>
    /// This translates to change of <see cref="Flags"/> pertaining to <see cref="DeckFlags.Pinned"/>.
    /// </remarks>
    public bool IsPinned
    {
        get { return _flags.HasFlag(DeckFlags.Pinned); }

        set
        {
            if (value)
            {
                Flags |= DeckFlags.Pinned;
            }
            else
            {
                Flags &= ~DeckFlags.Pinned;
            }
        }
    }

    /// <summary>
    /// Gets flags.
    /// </summary>
    public DeckFlags Flags
    {
        get { return _flags; }

        private set
        {
            if (_flags != value)
            {
                // Set privately with write on change
                _flags = value;
                OnModified(DeckMods.Flags);
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

            long mem = FootOverhead + _model?.Length * 2L ?? 0 + _title?.Length * 2L ?? 0 +
                _folder?.Length * 2L ?? 0 + KeywordSnippet?.Length * 2L ?? 0;

            foreach (var item in _children)
            {
                mem += item.Footprint;
            }

            _footprint = mem;
            return mem;
        }
    }

    /// <summary>
    /// Gets the id of the <see cref="GardenLeaf"/> containing the keyword as a result of the last call to <see
    /// cref="SearchInContent"/>.
    /// </summary>
    /// <remarks>
    /// The value will be zero where <see cref="KeywordSnippet"/> was found in the title.
    /// </remarks>
    public Zuid KeywordLeaf { get; private set; }

    /// <summary>
    /// Gets a keyword snippet as a result of the last call to <see cref="SearchInContent"/>.
    /// </summary>
    public string? KeywordSnippet { get; private set; }

    /// <summary>
    /// Gets a 64-bit value which changes whenever properties are modified which are expected to be seen by the user.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualCounter"/> is initialized to a positive random value on construction. A changed value
    /// indicates that a visual update is necessary, including child items. The value may not change for properties
    /// which may be considered purely "internal" and not visible to the user.
    /// </remarks>
    public long VisualCounter { get; private set; } = Random.Shared.NextInt64(long.MaxValue - 1) + 1;

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



}