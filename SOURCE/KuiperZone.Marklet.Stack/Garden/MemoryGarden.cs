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
/// A sequence container for <see cref="GardenDeck"/> items backed by application logic, and a database implementation
/// provided by the <see cref="IServiceProvider"/> implementation.
/// </summary>
/// <remarks>
/// This and related class are not thread safe. While backed by a database, it does not need one to operate. However, in
/// this case, all data is held in memory and lost on exit. Thanks to Molly Rocket for the helpful metaphor.
/// </remarks>
public sealed partial class MemoryGarden : IReadOnlyCollection<GardenBasket>
{
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

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    public const int SchemaVersion = MetaOps.SchemaVersion;

    private readonly List<GardenBasket> _baskets = new(4);
    private string? _name;

    /// <summary>
    /// Constructor which calls <see cref="Open"/> if provider is not null.
    /// </summary>
    public MemoryGarden(IServiceProvider? provider = null)
    {
        const string NSpace = $"{nameof(MemoryGarden)}.constructor";

        foreach (var item in Enum.GetValues<BasketKind>())
        {
            if (item.IsLegal())
            {
                _baskets.Add(new(this, item));
            }
        }

        Count = _baskets.Count;
        Baskets = _baskets;

        if (provider != null)
        {
            Diag.WriteLine(NSpace, "Call open");
            Open(provider);
        }
    }

    private MemoryGarden(MemoryGarden other)
    {
        Baskets = _baskets;
        _name = other._name;

        foreach(var item in other)
        {
            _baskets.Add(new GardenBasket(this, item));
        }

        if (other.Provider != null)
        {
            Status = GardenStatus.Readonly;
            Provider = other.Provider?.CloneReadOnly();
        }

        Diag.ThrowIfFalse(IsEphemeral);
    }

    /// <summary>
    /// Occurs on change when: A. one or more items are modified, added or removed, B. <see cref="Provider"/> changes
    /// indicating that the garden was opened or closed and, C. <see cref="Name"/> or other meta changes.
    /// </summary>
    /// <remarks>
    /// The <see cref="GardenChangedEventArgs"/> does not indicate which <see cref="GardenDeck"/> instance may have
    /// changed, as it may occur once on multiple such changes. However, it does indicate which instance <see
    /// cref="Baskets"/> has received the change. The <see cref="FocusedUpdated"/>event can be used to received detailed
    /// item change information, but only for the item in "focus". Where <see cref="GardenChangedEventArgs.Basket"/>
    /// equals <see cref="BasketKind.None"/>, it implies a change to <see cref="MemoryGarden"/> itself, such as opening
    /// or closing of <see cref="Provider"/>.
    /// </remarks>
    public event EventHandler<GardenChangedEventArgs>? Changed;

    /// <summary>
    /// Occurs when the <see cref="Focused"/> reference changes (including being set to null).
    /// </summary>
    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

    /// <summary>
    /// Occurs when the properties of <see cref="Focused"/> are modified.
    /// </summary>
    /// <remarks>
    /// The event is invoked only for the instance given by <see cref="Focused"/>. It is not invoked when items are
    /// added or removed. It does not occur when <see cref="Focused"/> is null.
    /// </remarks>
    public event EventHandler<FocusedUpdatedEventArgs>? FocusedUpdated;

    /// <summary>
    /// Gets a sequence of baskets belonging to this <see cref="MemoryGarden"/> instance.
    /// </summary>
    public IReadOnlyList<GardenBasket> Baskets { get; }

    /// <summary>
    /// Gets the database provider.
    /// </summary>
    /// <remarks>
    /// IMPORTANT. Where <see cref="Provider"/> is null, or <see cref="IServiceProvider.IsReadOnly"/> is true, instance
    /// will still allow <see cref="Insert"/> and modification, but changes will not persist (ephemeral).
    /// </remarks>
    public IServiceProvider? Provider { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public GardenStatus Status { get; set; }

    /// <summary>
    /// Gets whether new or modified content is ephemeral.
    /// </summary>
    /// <remarks>
    /// The value is true where <see cref="Provider"/> is null or where <see cref="IServiceProvider.IsReadOnly"/> is
    /// true. A closed or readonly instance will still allow <see cref="Insert"/> and modification, but changes will not
    /// persist (ephemeral).
    /// </remarks>
    public bool IsEphemeral
    {
        get { return Provider == null || Provider.IsReadOnly; }
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
    /// Gets or sets the name.
    /// </summary>
    /// <remarks>
    /// Changes are not persistant unless <see cref="Provider"/> is persistant.
    /// </remarks>
    public string? Name
    {
        get { return _name; }

        set
        {
            value = Sanitize(value, MaxMetaLength);

            if (_name != value)
            {
                _name = value;

                if (Provider?.IsReadOnly == false)
                {
                    try
                    {
                        using var con = Provider.Connect();
                        MetaOps.UpdateName(con, value);
                    }
                    catch
                    {
                        SetStatus(GardenStatus.Lost);
                    }
                }

                OnChanged(BasketKind.None);
            }
        }
    }

    /// <summary>
    /// Gets the currently selected <see cref="GardenDeck"/> child.
    /// </summary>
    /// <remarks>
    /// The term "focused", here, means that the item is the focus of attention (i.e. being viewed). Initial value is
    /// null (none).
    /// </remarks>
    public GardenDeck? Focused { get; private set; }

    /// <summary>
    /// Gets the total count of all <see cref="GardenDeck"/> instances in all baskets.
    /// </summary>
    public int PopulationCount
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
    /// Implements <see cref="IReadOnlyCollection{T}.Count"/>
    /// </summary>
    /// <remarks>
    /// Note that is the count of <see cref="GardenBasket"/> instance. See also <see cref="PopulationCount"/>.
    /// </remarks>
    public int Count { get; }

    /// <summary>
    /// Gets the corresponding <see cref="GardenBasket"/> instance for the given <see cref="BasketKind"/> identifier.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid BasketKind</exception>
    public GardenBasket this[BasketKind basket]
    {
        get
        {
            for (int n = 0; n < Count; ++n)
            {
                var item = _baskets[n];

                if (item.Kind == basket)
                {
                    return item;
                }
            }

            throw new ArgumentException($"Invalid {nameof(BasketKind)} {basket}");
        }
    }
}