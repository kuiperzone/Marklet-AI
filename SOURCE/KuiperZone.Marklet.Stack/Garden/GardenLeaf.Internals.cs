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

namespace KuiperZone.Marklet.Stack.Garden;

// INTERNALS (partial)
public sealed partial class GardenLeaf : IComparable<GardenLeaf>, IEquatable<GardenLeaf>
{
    /// <summary>
    /// Reads all message items pertaining to a <see cref="GardenDeck"/>.
    /// </summary>
    /// <exception cref="DbException">Database exception</exception>
    /// <exception cref="Exception">Exceptions indicating invalid data or other failure</exception>
    internal static void ReadDb(DbConnection con, GardenDeck owner, IndexableSet<GardenLeaf> children)
    {
        using var cmd = LeafOps.GetReader(con, owner);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            // The design is such that we wish to assign private fields directly.
            // This prevents us from putting the read fully into TableOps.
            // Moreover, we ensure data is valid before fully reading.
            var id = new Zuid(reader.GetInt64OrThrow(LeafOps.IdField));
            var format = (LeafFormat)reader.GetInt32OrThrow(LeafOps.FormatField);

            // This allows us to add values in future while, if careful,
            // allowing older software to be foreward compatible with
            // newer database. We will ignore illedefined values.
            if (!id.IsEmpty && format.IsLegal())
            {
                var leaf = new GardenLeaf(owner, id, format);

                Diag.ThrowIfNotEqual(id, leaf.Id);
                Diag.ThrowIfNotEqual(format, leaf.Format);
                leaf._assistant = reader.GetStringOrDefault(LeafOps.AssistantField);
                leaf._content = reader.GetStringOrDefault(LeafOps.ContentField);

                // Future values should get with defaults

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

    /// <exception cref="ArgumentException">Id undefined</exception>
    /// <exception cref="DbException">Database exception</exception>
    internal void InsertDb(DbConnection con)
    {
        Diag.ThrowIfNull(Owner); // <- detached
        Diag.ThrowIfNull(Owner?.Garden); // <- not expected
        Diag.ThrowIfTrue(_inserted); // <- already inserted

        if (!IsEphemeral && !_inserted)
        {
            _inserted = true;
            LeafOps.Insert(con, this);
        }

        // Assume written
        _inserted = true;
    }

    /// <summary>
    /// Sets <see cref="Owner"/> to null, meaning that this instance can no longer write to the database.
    /// </summary>
    internal void DetachInternal()
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(DetachInternal)}";
        Diag.WriteLine(NSpace, "Detach leaf");
        Owner = null;
    }

    private void OnModified(LeafMods mods)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(OnModified)}";
        Diag.WriteLine(NSpace, $"Changes: {mods}, {_inserted}");

        if (mods != LeafMods.None && Owner != null)
        {
            if (mods.IsVisual())
            {
                VisualCounter += 1;
                Diag.WriteLine(NSpace, "Visual change");
            }

            if (_inserted && !IsEphemeral && Garden?.Provider != null && (!IsStreaming || mods != LeafMods.Content))
            {
                try
                {
                    Diag.WriteLine(NSpace, "Connecting");
                    using var con = Garden.Provider.Connect();
                    Diag.WriteLine(NSpace, "Update");
                    LeafOps.Update(con, this, mods);

                    Diag.WriteLine(NSpace, "Call parent");
                    Owner.OnModifiedInternal(DeckMods.Leaf, con, true);
                    return;
                }
                catch (Exception e)
                {
                    Diag.WriteLine(NSpace, e);
                    Garden.SetStatus(GardenStatus.Lost);
                    return;
                }
            }

            // Still raise but with no connection
            Diag.WriteLine(NSpace, "Non-persistant change");
            Owner.OnModifiedInternal(DeckMods.Leaf, null, true);
        }
    }

}