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
public sealed partial class GardenDeck : IReadOnlyList<GardenLeaf>, IComparable<GardenDeck>, IEquatable<GardenDeck>
{
    /// <summary>
    /// Create stub instance used only for search.
    /// </summary>
    internal static GardenDeck CreateStub(Zuid id)
    {
        return new GardenDeck(id);
    }

    /// <summary>
    /// Reads all <see cref="GardenDeck"/> headers.
    /// </summary>
    /// <exception cref="DbException">Database exception</exception>
    /// <exception cref="Exception">Exceptions indicating invalid data or other failure</exception>
    internal static void ReadDb(DbConnection con, MemoryGarden garden, List<GardenDeck> list)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(ReadDb)}";
        Diag.WriteLine(NSpace, "READ DECK");

        using var cmd = DeckOps.GetReader(con);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            // The design is such that we wish to assign private fields directly.
            // This prevents us from putting this fully in TableOps. Moreover,
            // we ensure data is valid before fully readable.
            var id = new Zuid(reader.GetInt64OrThrow(DeckOps.IdField));
            var kind = (DeckFormat)reader.GetInt32OrThrow(DeckOps.FormatField);
            var origin = (BasketKind)reader.GetInt32OrThrow(DeckOps.OriginField);
            var basket = (BasketKind)reader.GetInt32OrThrow(DeckOps.BasketField);
            var updated = new DateTime(reader.GetInt64OrThrow(DeckOps.UpdatedField));

            // This allows us to add values in future while, if careful,
            // allowing older software to be foreward compatible with
            // newer database. We will ignore ill-defined values.
            if (!id.IsEmpty && kind.IsLegal() && origin.IsLegal() && basket.IsLegal() && updated > DateTime.UnixEpoch)
            {
                var obj = new GardenDeck(garden, id, kind, origin);
                obj.Updated = updated;
                obj._currentBasket = basket;
                obj._title = reader.GetStringOrDefault(DeckOps.TitleField);
                obj._model = reader.GetStringOrDefault(DeckOps.ModelField);
                obj._folder = reader.GetStringOrDefault(DeckOps.FolderField);
                obj._flags = (DeckFlags)reader.GetInt32OrThrow(DeckOps.FlagsField);

                // Future values should get with defaults

                Diag.WriteLine(NSpace, $"READ: {obj.Title}");
                list.Add(obj);
            }
        }
    }

    /// <summary>
    /// Inserts <see cref="GardenDeck"/> into the database and assigns <see cref="Garden"/>, returning true if inserted.
    /// </summary>
    /// <remarks>
    /// The <see cref="Garden"/> is assigned here.
    /// </remarks>
    /// <exception cref="ArgumentException">Id undefined</exception>
    internal bool InsertDb(MemoryGarden garden)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(InsertDb)}";
        Diag.ThrowIfNotNull(Garden);

        // Must set BEFORE Ephemeral
        Garden = garden;
        Diag.WriteLine(NSpace, "Garden assigned");
        Diag.WriteLine(NSpace, "Ephemeral: " + Ephemeral);

        if (Ephemeral == EphemeralStatus.Persistant)
        {
            Diag.WriteLine(NSpace, "Connecting");
            Diag.ThrowIfNull(garden.Provider);

            try
            {
                using var con = garden.Provider!.Connect();

                DeckOps.Insert(con, this);
                Diag.WriteLine(NSpace, $"Writing {nameof(GardenLeaf)} children: {_children.Count}");

                foreach (var item in _children)
                {
                    // Unexpected that Deck insert should succeed, but not leaf
                    // children. Have determined it best to do nothing on negative
                    // result, but let the caller handle any exception. If there's
                    // a leaf ID conflict at this point, what do we do about it?
                    // Drop the connection or ignore the leaf? Let's ignore the leaf.
                    item.InsertDb(con);
                }

                // Assumed true even if not writable
                Diag.WriteLine(NSpace, "Written to database OK");
                return true;
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                Garden?.SetStatus(GardenStatus.Lost);
            }
        }

        return false;
    }

    /// <summary>
    /// Delete child leaf returns true on success.
    /// </summary>
    /// <exception cref="DbException">Database exception</exception>
    internal bool DeleteDb(DbConnection? con)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(DeleteDb)}";
        Diag.WriteLine(NSpace, "DELETE DECK: " + ToString());

        bool persitant = Ephemeral == EphemeralStatus.Persistant;
        Diag.WriteLine(NSpace, "Persistant: " + persitant);

        DetachInternal();
        return con != null && persitant && DeckOps.Delete(con, Id);

    }

    /// <summary>
    /// Delete child leaf returns true on success.
    /// </summary>
    /// <remarks>
    /// Only needed when deleting leaves individually as we assume FOREIGN KEY. Does not throw on database error.
    /// </remarks>
    internal bool DeleteLeaf(GardenLeaf leaf, bool raiseBasket)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(DeleteLeaf)}";
        Diag.WriteLine(NSpace, "DELETE LEAF: " + leaf);

        // The necessity to remove from children requires us include
        // DB leaf removal in this class, rather than on GardenLeaf.
        if (leaf.Owner != this || !_children.Remove(leaf))
        {
            Diag.WriteLine(NSpace, "Not member");
            return false;
        }

        if (leaf.IsEphemeral)
        {
            Diag.WriteLine(NSpace, "Not persistant (success)");

            // Must detach after ephemeral check
            leaf.DetachInternal();
            OnModifiedInternal(DeckMods.Leaf, null, raiseBasket);
            return true;
        }

        Diag.WriteLine(NSpace, "Persistant");
        leaf.DetachInternal();

        try
        {
            Diag.WriteLine(NSpace, "Connecting");

            // Provider cannot be null here (leaf ephemeral should guarantee it)
            Diag.ThrowIfNull(Garden?.Provider);
            using var con = Garden!.Provider!.Connect();

            bool result = LeafOps.Delete(con, leaf.Id);
            OnModifiedInternal(DeckMods.Leaf, con, raiseBasket);

            Diag.WriteLine(NSpace, "Done ok: " + result);
            return result;
        }
        catch (Exception e)
        {
            Diag.WriteLine(NSpace, e);
            Garden?.SetStatus(GardenStatus.Lost);
            return false;
        }
    }

    /// <summary>
    /// Sets <see cref="Format"/> and writes the database without rasing a change against the owner.
    /// </summary>
    internal bool SetBasketNoRaise(DbConnection? con, BasketKind basket)
    {
        if (_currentBasket != basket)
        {
            _currentBasket = basket;
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
    /// Sets <see cref="IsFocused"/> to false without rasing a selection change against the owner.
    /// </summary>
    internal void DeselectNoRaise()
    {
        // No callback on parent
        _isFocused = false;
    }

    /// <summary>
    /// Writes changes to the database.
    /// </summary>
    internal void OnModifiedInternal(DeckMods mods, DbConnection? con, bool raiseBasket)
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(OnModifiedInternal)}";
        Diag.WriteLine(NSpace, "Mods: " + mods);

        const DeckMods Updatables = DeckMods.Leaf | DeckMods.Basket;
        const DeckMods Footprints = ~(DeckMods.Basket | DeckMods.Updated | DeckMods.Flags); // <- note ~NOT

        if (mods != DeckMods.None)
        {
            if (mods.HasFlag(DeckMods.Leaf) && _title == null)
            {
                // Generate algorithmic title if there is none
                _title = GetSigTitle();

                if (_title != null)
                {
                    mods |= DeckMods.Flags;

                }
            }

            if ((mods & Footprints) != 0)
            {
                _footprint = 0;
            }

            if ((mods & Updatables) != 0)
            {
                mods |= DeckMods.Updated;
                Updated = DateTime.UtcNow + SpoofOffset;
            }

            if (con != null && Ephemeral == EphemeralStatus.Persistant)
            {
                DeckOps.Update(con, this, mods);
            }

            if (mods.IsVisual())
            {
                VisualCounter += 1;
            }

            Diag.WriteLine(NSpace, "Call garden");
            Garden?.OnUpdated(this, mods, raiseBasket);

            // AFTER OnUpdated()
            if ((_currentBasket == BasketKind.Waste || _currentBasket == BasketKind.Archive) && mods.HasFlag(DeckMods.Basket))
            {
                // Free memory if moved to waste or archive
                Diag.WriteLine(NSpace, "Free memory");
                Close();
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
            IsFocused = false;

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
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(OnModifiedInternal)}";
        Diag.WriteLine(NSpace, "Mods: " + mods);

        if (mods != DeckMods.None && Garden?.Provider != null && Ephemeral == EphemeralStatus.Persistant)
        {
            try
            {
                Diag.WriteLine(NSpace, "Connecting");
                using DbConnection? con = Garden.Provider.Connect();
                OnModifiedInternal(mods, con, true);
                return;
            }
            catch (Exception e)
            {
                Diag.WriteLine(NSpace, e);
                Garden.SetStatus(GardenStatus.Lost);
                return;
            }
        }

        // Inform only
        OnModifiedInternal(mods, null, true);
    }

    private void SilentRotateOnFull()
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(SilentRotateOnFull)}";

        if (_children.Count > MaxLeafCount)
        {
            Diag.WriteLine(NSpace, "DECK FULL");

            // Delete 3rd (index 2), not first
            DeleteLeaf(_children[2], false);
        }
    }

    private string? GetSigTitle()
    {
        const string NSpace = $"{nameof(GardenDeck)}.{nameof(GetSigTitle)}";

        int count = Math.Min(_children.Count, 10);

        for (int n = 0; n < count; n++)
        {
            var item = _children[n];

            if (item.Content != null && item.Format.IsVisible())
            {
                var t = item.Content.SigText();
                Diag.WriteLine(NSpace, "Sigtext: " + t);

                if (t != null)
                {
                    Diag.WriteLine(NSpace, "Accepted");
                    return t;
                }
            }
        }

        return null;
    }

}