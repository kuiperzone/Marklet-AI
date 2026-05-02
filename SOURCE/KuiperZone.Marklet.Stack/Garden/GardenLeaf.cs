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
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Message leaf containing <see cref="Content"/>.
/// </summary>
/// <remarks>
/// The instance is readonly at class level, but properties may change in response to calls on the <see cref="Owner"/>
/// <see cref="GardenDeck"/> instance.
/// </remarks>
public sealed partial class GardenLeaf : IComparable<GardenLeaf>, IEquatable<GardenLeaf>
{
    // Approx memory used by data (inc public properties and ref itself)
    private const long FootprintOverhead = sizeof(long) + sizeof(long) * 4 + sizeof(byte) * 4;
    private string? _assistant;
    private string? _content;
    private LeafFlags _flags;
    private bool _inserted;

    /// <summary>
    /// Constructor.
    /// </summary>
    internal GardenLeaf(GardenDeck owner, Zuid id, LeafFormat format, string? content, LeafFlags flags)
    {
        Diag.ThrowIfTrue(id.IsEmpty);
        Diag.ThrowIfEqual(LeafFormat.None, format);

        Owner = owner;
        Id = id;
        Format = format;

        _assistant = Format.IsAssistant() ? owner.Model : null;
        _content = MemoryGarden.Sanitize(content, MemoryGarden.MaxContentLength, !flags.HasFlag(LeafFlags.Streaming));
        _flags = flags;
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    internal GardenLeaf(GardenDeck owner, GardenLeaf other, bool branch)
    {
        Owner = owner;
        Id = branch ? other.Id.CloneUnique() : other.Id;
        Format = other.Format;

        _assistant = other._assistant;
        _content = other.Content;

        if (other._flags.HasFlag(LeafFlags.Ephemeral))
        {
            _flags = LeafFlags.Ephemeral;
        }
    }

    /// <summary>
    /// Read constructor.
    /// </summary>
    private GardenLeaf(GardenDeck owner, Zuid id, LeafFormat format)
    {
        Diag.ThrowIfTrue(id.IsEmpty);
        Diag.ThrowIfEqual(LeafFormat.None, format);

        Owner = owner;
        Id = id;
        Format = format;

        _inserted = true;
    }

    /// <summary>
    /// Gets the <see cref="GardenDeck"/> to which this message belongs.
    /// </summary>
    public GardenDeck? Owner { get; private set; }

    /// <summary>
    /// Gets the <see cref="MemoryGarden"/> in which this message is stored.
    /// </summary>
    public MemoryGarden? Garden
    {
        get { return Owner?.Garden; }
    }

    /// <summary>
    /// Gets the unique identifier which also provides creation time.
    /// </summary>
    public Zuid Id { get; }

    /// <summary>
    /// Gets the format.
    /// </summary>
    public LeafFormat Format { get; }

    /// <summary>
    /// Gets whether the leaf is ephemeral.
    /// </summary>
    public bool IsEphemeral
    {
        get { return _flags.HasFlag(LeafFlags.Ephemeral) || Owner?.Ephemeral != EphemeralStatus.Persistant; }
    }

    /// <summary>
    /// Gets whether the message is currently streaming.
    /// </summary>
    public bool IsStreaming
    {
        get { return _flags.HasFlag(LeafFlags.Streaming); }
    }

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
    public string? Content
    {
        get { return _content; }

        set
        {
            value = MemoryGarden.Sanitize(value, MemoryGarden.MaxContentLength);

            if (_content != value)
            {
                _content = value;
                _flags &= ~LeafFlags.Streaming;
                OnModified(LeafMods.Content);
            }
        }
    }

    /// <summary>
    /// Gets an approximate figure for the memory consumed in bytes by this instance.
    /// </summary>
    public long Footprint
    {
        get { return FootprintOverhead + (_content?.Length * 2L ?? 0) + (_assistant?.Length * 2L ?? 0); }
    }

    /// <summary>
    /// Gets a 64-bit value which changes whenever properties are modified which are expected to be seen by the user.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualCounter"/> is initialized to a positive random value on construction. A changed value
    /// indicates that re-rendering needs to be performed for this item. The value may not change for properties which
    /// may be considered purely "internal" and not visible to the user.
    /// </remarks>
    public long VisualCounter { get; private set; } = Random.Shared.NextInt64(long.MaxValue - 1) + 1;

    /// <summary>
    /// Gets <see cref="MarkOptions"/> to use when parsing <see cref="Content"/>.
    /// </summary>
    public MarkOptions ParsingsOptions
    {
        get
        {
            // No Sanitize (already done)
            const MarkOptions Options = MarkOptions.Blocks | MarkOptions.PlainLinks | MarkOptions.Coalesce;

            if (Format.IsDefaultInline())
            {
                return Options | MarkOptions.Inlines;
            }

            return Options;
        }
    }
}