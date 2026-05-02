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

using System.Diagnostics.CodeAnalysis;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

// METHODS (partial)
public sealed partial class GardenLeaf : IComparable<GardenLeaf>, IEquatable<GardenLeaf>
{
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
        Diag.WriteLine(NSpace, $"APPEND STREAM on: {Owner}");
        Diag.WriteLine(NSpace, $"Kind: {Format}, {IsStreaming}");

        if (IsStreaming && !string.IsNullOrEmpty(chunk))
        {
            // No Trim here
            // ConditionalDebug.WriteLine(NSpace, $"Chunk: {chunk}");
            chunk = MemoryGarden.Sanitize(chunk, MemoryGarden.MaxContentLength, false);
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
            Diag.WriteLine(NSpace, "Stop streaming and commit");
            _content = _content?.Trim();
            _flags &= ~LeafFlags.Streaming;
            OnModified(LeafMods.Content);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a branch (unique clone) of the <see cref="Owner"/> with clones of children up to and including this
    /// leaf.
    /// </summary>
    /// <remarks>
    /// Note that binary attachments are NOT copied over into the branch.
    /// </remarks>
    /// <exception cref="InvalidOperationException">No Owner (detached)</exception>
    public GardenDeck Branch()
    {
        if (Owner == null)
        {
            throw new InvalidOperationException($"No {nameof(Owner)} (detached)");
        }

        return new GardenDeck(null, Owner, this);
    }

    /// <summary>
    /// Delete the instance from the database and returns true on success.
    /// </summary>
    /// <remarks>
    /// On success, the <see cref="Owner"/> and <see cref="Garden"/> will be null and, while the instance properties
    /// will still be valid, it may no longer interact with the database.
    /// </remarks>
    public bool Delete()
    {
        return Owner?.DeleteLeaf(this, true) == true;
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
    /// Implements <see cref="IEquatable{T}"/> for content equality.
    /// </summary>
    public bool Equals([NotNullWhen(true)] GardenLeaf? other)
    {
        if (other == this)
        {
            return true;
        }

        return other != null &&
            Id.Equals(other.Id) &&
            Format == other.Format &&
            _assistant == other._assistant &&
            _content == other._content;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return Equals(obj as GardenLeaf);
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
}