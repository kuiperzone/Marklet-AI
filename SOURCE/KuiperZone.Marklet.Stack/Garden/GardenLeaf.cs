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

using System.Data.Common;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Message leaf containing <see cref="Content"/>.
/// </summary>
/// <remarks>
/// The instance is readonly at class level, but properties may change in response to calls on the <see cref="DeckOwner"/>
/// <see cref="GardenDeck"/> instance.
/// </remarks>
public sealed class GardenLeaf : IComparable<GardenLeaf>
{
    private const SanFlags BasicSan = SanFlags.NormC | SanFlags.SubControl;

    // Approx memory used by data (inc public properties and ref itself)
    private const long FootprintOverhead = sizeof(long) + sizeof(long) * 5 + sizeof(byte) * 4;
    private string? _assistant;
    private string _content = "";
    private bool _pendingInsertion;

    /// <summary>
    /// Insertion constructor.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    internal GardenLeaf(GardenDeck deck, TimeSpan offset, LeafKind kind, bool streaming)
        : this(deck, Zuid.New(offset), kind)
    {
        if (kind == LeafKind.None)
        {
            throw new ArgumentException($"Invalid {nameof(LeafKind)} = {kind}", nameof(kind));
        }

        _pendingInsertion = true;
        _assistant = kind.HasModel() ? deck.Model : null;
        IsStreaming = streaming;
    }

    private GardenLeaf(GardenDeck deck, Zuid id, LeafKind kind)
    {
        // Read constructor
        ConditionalDebug.ThrowIfTrue(id.IsEmpty);
        ConditionalDebug.ThrowIfEqual(LeafKind.None, kind);
        DeckOwner = deck;
        Id = id;
        Kind = kind;
        VisualCounter = Random.Shared.NextInt64() + 1;
    }

    /// <summary>
    /// Gets the <see cref="GardenDeck"/> to which this message belongs.
    /// </summary>
    public GardenDeck? DeckOwner { get; private set; }

    /// <summary>
    /// Gets the <see cref="MemoryGarden"/> in which this message is stored.
    /// </summary>
    public MemoryGarden? Garden
    {
        get { return DeckOwner?.Garden; }
    }

    /// <summary>
    /// Gets whether the instance is writable to storage.
    /// </summary>
    /// <remarks>
    /// The result is true if: A. <see cref="LeafKind"/> is persistant, B. the parent <see
    /// cref="GardenDeck.IsPersistant"/> is true and, C. <see cref="IsStreaming"/> is false.
    /// </remarks>
    public bool IsPersistant
    {
        get { return !IsStreaming && Kind.IsPersistant() && DeckOwner?.IsPersistant == true; }
    }

    /// <summary>
    /// Gets the unique identifier which also provides creation time.
    /// </summary>
    public Zuid Id { get; }

    /// <summary>
    /// Gets the <see cref="LeafKind"/> associated with this message.
    /// </summary>
    public LeafKind Kind { get; }

    /// <summary>
    /// Gets the assistant name.
    /// </summary>
    /// <remarks>
    /// It is expected to be null for human input.
    /// </remarks>
    public string? Assistant
    {
        get { return _assistant; }

        set
        {
            value = MemoryGarden.Sanitize(value, MemoryGarden.MaxMetaLength);

            if (_assistant != value)
            {
                _assistant = value;
                OnModified(LeafMods.Assistant);
            }
        }
    }

    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    /// <remarks>
    /// Directly setting <see cref="Content"/> will set <see cref="IsStreaming"/> to false.
    /// </remarks>
    public string Content
    {
        get { return _content; }

        set
        {
            IsStreaming = false;
            value = Sanitizer.Sanitize(value, BasicSan | SanFlags.Trim, MemoryGarden.MaxContentLength);

            if (_content != value)
            {
                _content = value;
                OnModified(LeafMods.Content);
            }
        }
    }

    /// <summary>
    /// Gets an approximate figure for the memory consumed in bytes by this instance.
    /// </summary>
    public long Footprint
    {
        get { return FootprintOverhead + _content.Length * 2L + _assistant?.Length * 2L ?? 0; }
    }

    /// <summary>
    /// Gets whether the message is currently streaming.
    /// </summary>
    public bool IsStreaming { get; private set; }

    /// <summary>
    /// Gets a 64-bit value which changes whenever properties are modified which are expected to be seen by the user.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualCounter"/> is initialized to a positive random value on construction. A changed value
    /// indicates that re-rendering needs to be performed for this item. The value may not change for properties which
    /// may be considered purely "internal" and not visible to the user.
    /// </remarks>
    public long VisualCounter { get; private set; }

    /// <summary>
    /// Gets <see cref="MarkOptions"/> to use when parsing <see cref="Content"/>.
    /// </summary>
    public MarkOptions ParseOptions
    {
        get
        {
            // No Sanitize (already done)
            MarkOptions opts = MarkOptions.Blocks | MarkOptions.PlainLinks | MarkOptions.Coalesce;

            if (Kind.InlineMarkup())
            {
                opts |= MarkOptions.Inlines;
            }

            if (Kind == LeafKind.Assistant && IsStreaming)
            {
                return opts | MarkOptions.Chunking;
            }

            return opts;
        }
    }

    /// <summary>
    /// Appends streaming a chunk to <see cref="Content"/> and returns true on success.
    /// </summary>
    /// <remarks>
    /// The result is true if <see cref="IsStreaming"/> is true and "chunk" not null or empty, otherwise the call does
    /// nothing. When streaming, the "chunk" is appended to <see cref="Content"/>. Data is written only when
    /// terminated with a call to <see cref="StopStream"/>.
    /// </remarks>
    public bool AppendStream(string? chunk)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(AppendStream)}";
        ConditionalDebug.WriteLine(NSpace, $"APPEND STREAM on: {DeckOwner}");
        ConditionalDebug.WriteLine(NSpace, $"Kind: {Kind}");
        ConditionalDebug.WriteLine(NSpace, $"IsStreaming: {IsStreaming}");

        if (IsStreaming && !string.IsNullOrEmpty(chunk))
        {
            ConditionalDebug.ThrowIfTrue(IsPersistant);

            // No Trim here
            chunk = Sanitizer.Sanitize(chunk, BasicSan, MemoryGarden.MaxContentLength);
            _content = string.Concat(_content, chunk).Truncate(MemoryGarden.MaxContentLength);
            OnModified(LeafMods.Content);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Writes streamed <see cref="Content"/> to the database, sets <see cref="IsStreaming"/> from high to low, and
    /// returns true on success.
    /// </summary>
    /// <remarks>
    /// Where <see cref="IsStreaming"/> is already false when called, the method does nothing and returns false.
    /// </remarks>
    public bool StopStream()
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(StopStream)}";

        if (IsStreaming)
        {
            ConditionalDebug.WriteLine(NSpace, "Stop streaming and commit");
            IsStreaming = false;
            _content = _content.Trim();
            OnModified(LeafMods.Content);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Delete the instance from the database and returns true on success.
    /// </summary>
    /// <remarks>
    /// On success, the <see cref="DeckOwner"/> and <see cref="Garden"/> will be null and, while the instance properties
    /// will still be valid, it may no longer interact with the database.
    /// </remarks>
    public bool Delete()
    {
        return DeckOwner?.DeleteLeafDb(this, true) == true;
    }

    /// <summary>
    /// Implements <see cref="IComparable{T}"/>.
    /// </summary>
    public int CompareTo(GardenLeaf? other)
    {
        if (other == null)
        {
            return -1;
        }

        return Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Overrides and returns <see cref="Content"/> and <see cref="Id"/> for debug purposes.
    /// </summary>
    public override string ToString()
    {
        return string.Concat(Sanitizer.ToDebugSafe(Content, true, true, 32), ", ", Id.ToString(true), " (", Id.ToString(false), ")");
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Reads all message items pertaining to a <see cref="GardenDeck"/>.
    /// </summary>
    internal static void ReadAllDb(DbConnection con, GardenDeck parent, IndexableSet<GardenLeaf> children)
    {
        using var cmd = LeafOps.GetReader(con, parent);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            // The design is such that we wish to assign private fields directly.
            // This prevents us from putting the read fully into TableOps.
            // Moreover, we ensure data is valid before fully reading.
            var id = new Zuid(reader.GetInt64(LeafOps.IdField));
            var kind = (LeafKind)reader.GetInt32(LeafOps.KindField);

            // This allows us to add values in future while, if careful,
            // allowing older software to be foreward compatible with
            // newer database. We will ignore illedefined values.
            if (!id.IsEmpty && kind.IsLegal())
            {
                var leaf = new GardenLeaf(parent, id, kind);
                leaf._assistant = reader.GetStringOrNull(LeafOps.AssistantField);
                leaf._content = reader.GetStringOrEmpty(LeafOps.ContentField);

                // Assert expected properties
                ConditionalDebug.ThrowIfNotEqual(id, leaf.Id);
                ConditionalDebug.ThrowIfNotEqual(kind, leaf.Kind);
                children.Insert(leaf);

                if (children.Count > GardenDeck.MaxLeafCount)
                {
                    // Not expected unless we lower
                    // max limit on live systems
                    children.RemoveAt(0);
                }
            }
        }
    }

    internal bool InsertLeafDb(DbConnection con)
    {
        if (!IsPersistant)
        {
            // Assume written
            return true;
        }

        IsStreaming = false;
        _pendingInsertion = false;
        return LeafOps.Insert(con, this);
    }

    /// <summary>
    /// Sets <see cref="DeckOwner"/> to null, meaning that this instance can no longer write to the database.
    /// </summary>
    internal void DetachInternal()
    {
        DeckOwner = null;
        IsStreaming = false;
    }

    private bool OnModified(LeafMods mods)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(OnModified)}";
        ConditionalDebug.WriteLine(NSpace, $"Changes: {mods}");

        if (mods != LeafMods.None && DeckOwner != null)
        {
            if (mods.IsVisual())
            {
                VisualCounter += 1;
                ConditionalDebug.WriteLine(NSpace, "Visual change");
            }

            if (IsPersistant && Garden?.Gardener != null)
            {
                // Not persistant while streaming
                ConditionalDebug.ThrowIfTrue(IsStreaming);

                ConditionalDebug.WriteLine(NSpace, "Connecting");
                using var con = Garden.Gardener.Connect();

                if (_pendingInsertion)
                {
                    ConditionalDebug.WriteLine(NSpace, "Inserting");
                    _pendingInsertion = false;
                    LeafOps.Insert(con, this);
                }
                else
                {
                    ConditionalDebug.WriteLine(NSpace, "Update");
                    LeafOps.Update(con, this, mods);
                }

                ConditionalDebug.WriteLine(NSpace, "Call parent");
                DeckOwner.OnModifiedInternal(DeckMods.Leaf, con, true);
            }
            else
            {
                // Still raise but with no connection
                ConditionalDebug.WriteLine(NSpace, "Non-persistant change");
                DeckOwner.OnModifiedInternal(DeckMods.Leaf, null, !IsStreaming);
            }

            return true;
        }

        return false;
    }

}