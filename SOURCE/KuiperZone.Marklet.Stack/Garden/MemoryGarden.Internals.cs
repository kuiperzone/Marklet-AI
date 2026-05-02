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

// INTERNALS (partial)
public sealed partial class MemoryGarden : IReadOnlyCollection<GardenBasket>
{
    /// <summary>
    /// Sets <see cref="Focused"/> on the given child and returns true if selection changed.
    /// </summary>
    /// <remarks>
    /// Does nothing if <see cref="Provider"/> is null.
    /// </remarks>
    internal void OnFocusChanged(GardenDeck? obj)
    {
        if (Focused != obj)
        {
            Diag.ThrowIfTrue(obj?.IsFocused == false);

            // Silent
            var old = Focused;
            old?.DeselectNoRaise();

            Focused = obj;
            obj?.GetBasket()?.SetRecentInternal(obj);
            FocusChanged?.Invoke(this, new(obj, old));
        }
    }

    /// <summary>
    /// Invokes <see cref="Changed"/> with supplied basket. Does nothing if "raise" is false (convenience).
    /// </summary>
    internal void OnChanged(BasketKind basket, bool raise = true)
    {
        if (raise)
        {
            Changed?.Invoke(this, new(basket));
        }
    }

    /// <summary>
    /// Called when properties of "obj" have changed.
    /// </summary>
    internal void OnUpdated(GardenDeck obj, DeckMods mods, bool raiseBasket)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(OnUpdated)}";

        Diag.ThrowIfNotSame(obj.Garden, this);
        Diag.WriteLine(NSpace, "Deck: " + obj.ToString());
        Diag.WriteLine(NSpace, "Mods: " + mods);

        if (mods == DeckMods.None)
        {
            return;
        }

        if (mods.HasFlag(DeckMods.Basket))
        {
            foreach (var b in _baskets)
            {
                if (b.RemoveCache(obj))
                {
                    OnChanged(b.Kind, raiseBasket);

                    break;
                }
            }

            this[obj.CurrentBasket].InsertCache(obj);
        }

        OnChanged(obj.CurrentBasket, raiseBasket);

        if (Focused == obj)
        {
            FocusedUpdated?.Invoke(this, new(obj));
        }
    }

    /// <summary>
    /// Sets <see cref="Status"/>, clearing all content unless "status" equals <see cref="GardenStatus.Lost"/>.
    /// </summary>
    internal void SetStatus(GardenStatus status)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.{nameof(Close)}";

        var old = Status;
        Diag.WriteLine(NSpace, $"Status: {old} -> {status}");

        Status = status;
        Provider = null;

        // If lost, leave the cache.
        // It will be cleared on successful re-open.
        if (status != GardenStatus.Lost)
        {
            _name = null;

            foreach (var item in _baskets)
            {
                if (item.ClearCache())
                {
                    OnChanged(item.Kind);
                }
            }

            // Discards should clear this
            Diag.ThrowIfNotNull(Focused);
            Focused = null;
        }

        if (status != old)
        {
            Diag.WriteLine(NSpace, "Call OnChange");
            OnChanged(BasketKind.None);
        }
    }

}