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
public sealed partial class GardenDeck : IReadOnlyList<GardenLeaf>, IComparable<GardenDeck>, IEquatable<GardenDeck>
{
    /// <summary>
    /// Gets the current <see cref="GardenBasket"/> holding this instance.
    /// </summary>
    /// <remarks>
    /// The return value may change when <see cref="CurrentBasket"/> changes. The result is null where <see cref="Garden"/>
    /// is null.
    /// </remarks>
    public GardenBasket? GetBasket()
    {
        return Garden?[_currentBasket];
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
    /// <remarks>
    /// The call does nothing where <see cref="IsOpen"/> is already true.
    /// </remarks>
    public int Open()
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(Open)}";
        Diag.WriteLine(NSpace, $"OPEN on {this}");
        Diag.WriteLine(NSpace, $"Current open state: {IsOpen}");

        if (!IsOpen)
        {
            Diag.WriteLine(NSpace, "Opening");

            if (Garden?.Provider != null)
            {
                try
                {
                    using var con = Garden.Provider.Connect();
                    GardenLeaf.ReadDb(con, this, _children);
                }
                catch (Exception e)
                {
                    Diag.WriteLine(NSpace, e);
                    Garden.SetStatus(GardenStatus.Lost);
                    return 0;
                }
            }

            IsOpen = true;
            _footprint = 0;
        }

        Diag.WriteLine(NSpace, $"Count: {_children.Count}");
        return _children.Count;
    }

    /// <summary>
    /// Temporarily frees leaf data so that they they can be re-loaded on demand and returns true on success.
    /// </summary>
    /// <remarks>
    /// It does nothing if is already <see cref="IsOpen"/> is false. Children are not freed and the <see cref="IsOpen"/>
    /// is not set to false where <see cref="CanClose"/> is false.
    /// </remarks>
    public bool Close()
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(Close)}";
        Diag.WriteLine(NSpace, $"Close on {this}");
        Diag.WriteLine(NSpace, $"Current state: {IsOpen}, {CanClose}");

        if (IsOpen)
        {
            // Do first (order important)
            IsFocused = false;

            if (CanClose)
            {
                IsOpen = false;
                _children.Clear();
                _footprint = FootOverhead;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds a keyword in <see cref="Title"/> then <see cref="GardenLeaf.Content"/>, and returns true if matched.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="KeywordSnippet"/> and <see cref="KeywordLeaf"/>. On success, <see cref="KeywordSnippet"/> will
    /// be non-null, while if the match occurred in a leaf, <see cref="KeywordLeaf"/> will identify the leaf. On no
    /// match, both these properties will be default values.
    /// </remarks>
    public bool SearchInContent(SearchOptions opts)
    {
        KeywordLeaf = default;
        KeywordSnippet = null;

        if (opts.Keyword == null)
        {
            return false;
        }

        var snippet = _title?.PrettySearch(opts.Keyword, opts.MaxSnippet, opts.Flags, opts.ScanLimit);

        if (snippet != null)
        {
            // Found in title
            KeywordSnippet = snippet;
            return true;
        }

        using var c = new DeckTransientOpener(this);

        if (Count != 0)
        {
            foreach (var item in _children)
            {
                snippet = item.Content?.PrettySearch(opts.Keyword, opts.MaxSnippet, opts.Flags, opts.ScanLimit);

                if (snippet != null)
                {
                    KeywordLeaf = item.Id;
                    KeywordSnippet = snippet;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Appends a new <see cref="GardenLeaf"/> with the given "content" and returns the new intent on success.
    /// </summary>
    /// <remarks>
    /// The method returns null and does nothing where "content" is null, empty or whitespace. Otherwise, calling this
    /// method will ensure <see cref="IsOpen"/> is true before appending a new <see cref="GardenLeaf"/> item. On
    /// success, the new item will be contained within the <see cref="GardenDeck"/> sequence. On database error, the
    /// call will not throw, but invokes <see cref="MemoryGarden.Changed"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    public GardenLeaf Append(LeafFormat format, string? content, LeafFlags flags = LeafFlags.None)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(Append)}";
        Diag.WriteLine(NSpace, $"APPEND CONTENT: {format}, {flags}");
        Diag.WriteLine(NSpace, $"Content: " + content?.Truncate(64));

        Open();
        _footprint = 0;

        var id = Zuid.New(SpoofOffset);
        var leaf = new GardenLeaf(this, id, format, content, flags);

        if (_children.Insert(leaf) > -1)
        {
            SilentRotateOnFull();

            if (!leaf.IsEphemeral && Garden?.Provider != null)
            {
                try
                {
                    Diag.ThrowIfNotEqual(EphemeralStatus.Persistant, Ephemeral);

                    Diag.WriteLine(NSpace, "Connect");
                    using var con = Garden.Provider.Connect();

                    Diag.WriteLine(NSpace, "Insert");
                    leaf.InsertDb(con);

                    Diag.WriteLine(NSpace, "Call OnModified");
                    OnModifiedInternal(DeckMods.Leaf, con, true);
                    return leaf;
                }
                catch (Exception e)
                {
                    Diag.WriteLine(NSpace, e);
                    Garden.SetStatus(GardenStatus.Lost);
                    return leaf;
                }
            }

            Diag.WriteLine(NSpace, "Call OnModified without connector");
            OnModifiedInternal(DeckMods.Leaf, null, true);
        }

        return leaf;
    }

    /// <summary>
    /// Creates a clone of this.
    /// </summary>
    /// <remarks>
    /// The new instance can be inserted into another <see cref="MemoryGarden"/>, but the same reference as the current
    /// <see cref="Garden"/> as the two will share the same <see cref="Id"/>. Child <see cref="GardenLeaf"/> data is
    /// cloned only if <see cref="IsOpen"/> is true when called.
    /// </remarks>
    public GardenDeck Clone()
    {
        return new(null, this);
    }

    /// <summary>
    /// Resets <see cref="SpoofOffset"/> to 0.
    /// </summary>
    public void ResetSpoof()
    {
        SpoofOffset = default;
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
    /// Implements <see cref="IEquatable{T}"/> for content value equality.
    /// </summary>
    /// <remarks>
    /// It does not call <see cref="Open"/> and does not compare child leaves.
    /// </remarks>
    public bool Equals([NotNullWhen(true)] GardenDeck? other)
    {
        if (other == this)
        {
            return true;
        }

        return other != null &&
            Id == other.Id &&
            Format == other.Format &&
            OriginBasket == other.OriginBasket &&
            Updated == other.Updated &&
            _model == other._model &&
            _title == other._title &&
            _folder == other._folder &&
            _flags == other._flags &&
            _isExplicitEphemeral == other._isExplicitEphemeral;
    }

    /// <summary>
    /// Returns whether both instances in storage contain same data.
    /// </summary>
    /// <remarks>
    /// Unlike Equals(), this method ensures <see cref="Open"/> on both instances and compares child leaves. It may be
    /// expensive in comparison to Equals().
    /// </remarks>
    public bool IsConcordant([NotNullWhen(true)] GardenDeck? other)
    {
        if (other == this)
        {
            return true;
        }

        if (Equals(other))
        {
            using var c0 = new DeckTransientOpener(this);
            using var c1 = new DeckTransientOpener(other);

            int count = _children.Count;

            if (other._children.Count != count)
            {
                return false;
            }

            for (int n = 0; n < count; ++n)
            {
                if (!_children[n].Equals(other._children[n]))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return Equals(obj as GardenDeck);
    }

    /// <summary>
    /// Overrides and returns <see cref="Title"/> and <see cref="Id"/> for debug purposes.
    /// </summary>
    public override string ToString()
    {
        return string.Concat(Format, ", ", CurrentBasket, ", ", Folder, ", ", Id.ToString(true),
            " (", Id.ToString(false), "), ", Sanitizer.ToDebugSafe(Title, true, true, 32));
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}