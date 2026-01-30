// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// AUTHOR    : Andrew Thomas
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// LICENSE   : AGPL-3.0-only
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
using System.Text;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// A chat session containing a chronological sequence of <see cref="GardenLeaf"/> items.
/// </summary>
public sealed class GardenSession : IReadOnlyList<GardenLeaf>, IComparable<GardenSession>
{
    /// <summary>
    /// Gets the underlying table name (do not change).
    /// </summary>
    public const string TableName = "garden_session";

    /// <summary>
    /// Gets the table version, where 1 is the first release.
    /// </summary>
    public const int Version = 1;

    private static readonly ModFlags[] FieldFlags = GetFieldFlags();

    // Loaded in order and not expected to contain duplicates.
    private readonly List<GardenLeaf> _children = new(4);

    private BinKind _homeBin;
    private bool _isWaste;
    private string? _title;
    private string? _model;
    private string? _topic;
    private long _totalLength;
    private bool _isSelected;

    /// <summary>
    /// Constructor.
    /// </summary>
    public GardenSession(BinKind bin = BinKind.Home)
    {
        Id = Zuid.New();

        _homeBin = bin;
        IsOpen = true;
        UpdateTime = Id.Timestamp;
        AccessTime = Id.Timestamp;
        VisualCounter = Random.Shared.NextInt64() + 1;
    }

    /// <summary>
    /// Read constructor.
    /// </summary>
    private GardenSession(MemoryGarden garden, Zuid id, DateTime access)
    {
        Owner = garden;
        ConditionalDebug.ThrowIfTrue(id.IsEmpty);

        Id = id;
        UpdateTime = id.Timestamp;
        AccessTime = access;
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
    /// The value is 0 until <see cref="Open()"/> is called.
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
    /// cref="MemoryGarden.Insert(GardenSession)"/>.
    /// </remarks>
    public MemoryGarden? Owner { get; private set; }

    /// <summary>
    /// Gets whether data will be written to storage.
    /// </summary>
    public bool IsWritable
    {
        get { return Owner?.IsOpen == true; }
    }

    /// <summary>
    /// Gets whether the sequence container contents are loaded in memory.
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets whether the instance is selected.
    /// </summary>
    /// <remarks>
    /// The term "selected" here refers to that which is the focus of attention in the user interface. Changing <see
    /// cref="IsSelected"/> from false to true will also set <see cref="IsOpen"/> to true, and unselected any previously
    /// selected item of the <see cref="Owner"/>.
    /// </remarks>
    public bool IsSelected
    {
        get { return _isSelected; }

        set
        {
            if (_isSelected != value)
            {
                if (value)
                {
                    Open();
                    AccessTime = DateTime.UtcNow;
                }

                _isSelected = value;
                Owner?.OnSelected(value ? this : null);
            }
        }
    }

    /// <summary>
    /// Gets the unique identifier which also provides creation time.
    /// </summary>
    public Zuid Id { get; }

    /// <summary>
    /// Gets the UTC update time that content was added or modified on child items.
    /// </summary>
    /// <remarks>
    /// The <see cref="UpdateTime"/> is updated by <see cref="Append"/>. The value is not updated in response to
    /// metainformation changes, such as <see cref="Title"/>.
    /// </remarks>
    public DateTime UpdateTime { get; private set; }

    /// <summary>
    /// Gets the last access time as UTC.
    /// </summary>
    /// <remarks>
    /// The value is updated automatically when <see cref="IsSelected"/> changes or properties are modified.
    /// </remarks>
    public DateTime AccessTime { get; private set; }

    /// <summary>
    /// Gets or sets bin kind where the session lives (unless it is in the waste bin).
    /// </summary>
    /// <remarks>
    /// Changing <see cref="HomeBin"/> will move the item from its current designated <see cref="GardenBin"/> to another
    /// within <see cref="MemoryGarden"/>.
    /// </remarks>
    public BinKind HomeBin
    {
        get { return _homeBin; }

        set
        {
            if (_homeBin != value)
            {
                _homeBin = value;
                UpdateDb(ModFlags.HomeBin);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the item is designated for the waste bin.
    /// </summary>
    /// <remarks>
    /// Changing <see cref="IsWaste"/> will move the item between the <see cref="GardenBin"/> designated by <see
    /// cref="HomeBin"/> and <see cref="MemoryGarden.WasteBin"/>.
    /// </remarks>
    public bool IsWaste
    {
        get { return _isWaste; }

        set
        {
            if (_isWaste != value)
            {
                _isWaste = value;
                UpdateDb(ModFlags.IsWaste);
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
            value = MemoryGarden.SanitizeName(value);

            if (_title != value)
            {
                _title = value;
                UpdateDb(ModFlags.Title);
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
            value = MemoryGarden.SanitizeName(value);

            if (_model != value)
            {
                _model = value;
                UpdateDb(ModFlags.Model);
            }
        }
    }

    /// <summary>
    /// Gets or sets a topic tag.
    /// </summary>
    /// <remarks>
    /// Setting an empty string sets null.
    /// </remarks>
    public string? Topic
    {
        get { return _topic; }

        set
        {
            value = MemoryGarden.SanitizeName(value);

            if (_topic != value)
            {
                _topic = value;
                UpdateDb(ModFlags.Topic);
            }
        }
    }

    /// <summary>
    /// Gets the total <see cref="GardenLeaf.Content"/> length of all open messages.
    /// </summary>
    /// <remarks>
    /// The result is number of characters, not tokens. Always 0 when <see cref="IsOpen"/> is false.
    /// </remarks>
    public long TotalOpenLength
    {
        get
        {
            if (IsOpen)
            {
                if (_totalLength > -1)
                {
                    return _totalLength;
                }

                long count = 0;

                foreach (var item in _children)
                {
                    count += item.Content.Length;
                }

                return _totalLength = count;
            }

            return 0;
        }
    }

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
    /// Gets or sets a tag value expected to hold a visual component reference.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualTag"/> is not used by <see cref="GardenSession"/> but simply held.
    /// </remarks>
    public object? VisualTag { get; set; }

    /// <summary>
    /// Gets or sets a double value expected to hold a scroll position.
    /// </summary>
    /// <remarks>
    /// The <see cref="VisualScrollPos"/> is not used by <see cref="GardenSession"/> but simply held.
    /// </remarks>
    public double VisualScrollPos { get; set; }

    /// <summary>
    /// Implements <see cref="IComparable{T}"/>.
    /// </summary>
    public int CompareTo(GardenSession? other)
    {
        if (other == null)
        {
            return -1;
        }

        return Id.CompareTo(other.Id);
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
    /// Populates the sequence container with <see cref="GardenLeaf"/> items from the database and returns the open
    /// message count.
    /// </summary>
    public int Open()
    {
        if (!IsOpen)
        {
            const string NSpace = $"{nameof(GardenSession)}.{nameof(Open)}";
            ConditionalDebug.WriteLine(NSpace, $"OPEN on {this}");

            if (Owner != null)
            {
                using var con = Owner.Gardener.Connect();
                GardenLeaf.ReadInternal(con, this, _children);
            }

            IsOpen = true;
            _totalLength = -1;
        }

        return _children.Count;
    }

    /// <summary>
    /// Closes leaf items and sets <see cref="IsOpen"/> to false and <see cref="Count"/> to 0.
    /// </summary>
    /// <remarks>
    /// This frees memory when not in used, sets <see cref="IsSelected"/> to false and <see cref="Count"/> to 0. It does
    /// nothing if <see cref="IsOpen"/> is already false.
    /// </remarks>
    public void Close()
    {
        const string NSpace = $"{nameof(GardenSession)}.{nameof(Close)}";

        if (IsOpen)
        {
            ConditionalDebug.WriteLine(NSpace, $"CLOSE on {this}");
            IsOpen = false;
            IsSelected = false;
            _children.Clear();
        }
    }

    /// <summary>
    /// Appends a new <see cref="GardenLeaf"/> with the given "content" and returns the new intent on success.
    /// </summary>
    /// <remarks>
    /// The method returns null and does nothing where "content" is null, empty or whitespace. Otherwise, calling this
    /// method will ensure <see cref="IsOpen"/> to true before appending a new <see cref="GardenLeaf"/> item. On
    /// success, the new item will be contained within the <see cref="GardenSession"/> sequence.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    public GardenLeaf? Append(LeafKind kind, string? content)
    {
        const string NSpace = $"{nameof(GardenSession)}.{nameof(Append)}";
        ConditionalDebug.WriteLine(NSpace, $"APPEND CONTENT: {kind}");
        ConditionalDebug.WriteLine(NSpace, $"Content: " + content?.Truncate(64));

        if (!string.IsNullOrWhiteSpace(content))
        {
            Open();
            _totalLength = -1;

            var leaf = new GardenLeaf(this, kind, false);
            _children.Add(leaf);
            leaf.Content = content;
            return leaf;
        }

        return null;
    }

    /// <summary>
    /// Appends a new item with <see cref="GardenLeaf.IsStreaming"/> initially set to true.
    /// </summary>
    /// <remarks>
    /// Calling this method will always ensure <see cref="IsOpen"/> to true. The <see cref="GardenLeaf.IsStreaming"/>
    /// will be true on the returned instance, and <see cref="GardenLeaf.Content"/> will be initialized with "chunk0"
    /// (which may be null). The <see cref="GardenLeaf.AppendStream(string?)"/> and <see cref="GardenLeaf.StopStream"/>
    /// should subsequently be called on the resulting instance. Although the new instance is part of this <see
    /// cref="GardenSession"/> sequence on return, data is not written to the database until <see
    /// cref="GardenLeaf.StopStream"/> is called. Streaming is typically reserved for the <see
    /// cref="LeafKind.Assistant"/> kind.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    public GardenLeaf AppendStream(LeafKind kind, string? chunk0 = null)
    {
        const string NSpace = $"{nameof(GardenSession)}.{nameof(AppendStream)}";
        ConditionalDebug.WriteLine(NSpace, $"APPEND STREAM: {kind}");
        ConditionalDebug.WriteLine(NSpace, $"Content: " + chunk0?.Truncate(64));

        Open();
        _totalLength = -1;

        var leaf = new GardenLeaf(this, kind, true);
        _children.Add(leaf);
        leaf.AppendStream(chunk0);
        return leaf;
    }

    /// <summary>
    /// Deletes the instance from the database, including its message leaves, and removes it from <see cref="Owner"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Owner"/> is set to null on return, and the instance should be discarded. It does nothing if
    /// called multiple times.
    /// </remarks>
    public bool Delete()
    {
        if (Owner != null)
        {
            using var con = Owner.Gardener.Connect();
            return DeleteDb(con, true);
        }

        return false;
    }

    /// <summary>
    /// Overrides and returns <see cref="Title"/> and <see cref="Id"/> for debug purposes.
    /// </summary>
    public override string ToString()
    {
        return string.Concat(Sanitizer.ToDebugSafe(Title, true, true, 32), ", ", Id.ToString(true), " (", Id.ToString(false), ")");
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Ensure table.
    /// </summary>
    internal static void CreateTable(DbConnection con, DbTransaction? tran)
    {
        const string NSpace = $"{nameof(GardenSession)}.{nameof(CreateTable)}";

        // VERSION 1
        const string Sql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
id              BIGINT PRIMARY KEY,
update_time     BIGINT NOT NULL,
home_bin        INTEGER NOT NULL,
is_waste        BOOLEAN,
title           VARCHAR(255),
model           VARCHAR(255),
topic           VARCHAR(255)
);";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Future expansion.
    /// </summary>
    internal static bool UpgradeTable(DbConnection _, DbTransaction? __, int currentVersion)
    {
        const string NSpace = $"{nameof(GardenSession)}.{nameof(UpgradeTable)}";

        if (Version > currentVersion)
        {
            // FOR FUTURE
            ConditionalDebug.WriteLine(NSpace, $"ALTER TABLE: {TableName}");
            ConditionalDebug.Fail("Not implemented");

            // I.e. ALTER TABLE message ADD COLUMN metadata TEXT DEFAULT '';
            // return true;
        }

        return false;
    }

    /// <summary>
    /// Reads all sessions into the garden.
    /// </summary>
    internal static void Read(DbConnection con, MemoryGarden owner, List<GardenSession> garden)
    {
        const string NSpace = $"{nameof(GardenSession)}.{nameof(Read)}";

        const string Sql = $"SELECT * FROM {TableName} ORDER BY id";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;

        var now = DateTime.UtcNow;
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var obj = new GardenSession(owner, new Zuid(reader.GetInt64("id")), now);
            obj.UpdateTime = new DateTime(reader.GetInt64("update_time"));

            obj._homeBin = (BinKind)reader.GetInt32("home_bin");
            obj._isWaste = reader.GetBoolean("is_waste");
            obj._title = reader.GetStringOrNull("title");
            obj._model = reader.GetStringOrNull("model");
            obj._topic = reader.GetStringOrNull("topic");

            ConditionalDebug.WriteLine(NSpace, $"READ: {obj}, model: {obj.Model}");
            garden.Add(obj);
        }
    }

    /// <summary>
    /// Inserts new item into the database and assigns <see cref="Owner"/>.
    /// </summary>
    /// <remarks>
    /// No change is raised against the owner.
    /// </remarks>
    /// <exception cref="ArgumentException">Already inserted</exception>
    internal void InsertDbNoRaise(MemoryGarden garden)
    {
        if (Owner != null)
        {
            throw new ArgumentException("Already inserted");
        }

        // Must set Owner before IsWritable check
        ConditionalDebug.ThrowIfFalse(garden.IsOpen);
        Owner = garden;

        if (IsWritable)
        {
            using var con = garden.Gardener.Connect();
            using var cmd = GetInsertCommand(con);
            cmd.ExecuteNonQuery();

            foreach (var item in _children)
            {
                item.InsertDb(con);
            }
        }
    }

    /// <summary>
    /// Writes changes to the database.
    /// </summary>
    internal void UpdateDb(DbConnection? con, ModFlags changes, bool raiseBin)
    {
        if (changes != ModFlags.None)
        {
            AccessTime = DateTime.UtcNow;

            if (changes.HasFlag(ModFlags.Leaf))
            {
                _totalLength = -1;
                UpdateTime = AccessTime;
                changes |= ModFlags.Time;
            }

            if (con != null)
            {
                using var cmd = GetUpdateCommand(con, changes);
                cmd.ExecuteNonQuery();
            }

            if (changes.HasVisual())
            {
                VisualCounter += 1;
            }

            Owner?.OnChildChanged(this, changes, raiseBin);
        }
    }

    /// <summary>
    /// Deletes this item and its leaves in the database and calls <see cref="Discard"/>.
    /// </summary>
    internal bool DeleteDb(DbConnection con, bool raiseBin)
    {
        // Must hold copies before Discard()
        var owner = Owner;
        bool writable = IsWritable;

        if (owner != null)
        {
            if (writable)
            {
                const string Sql = $"DELETE FROM {TableName} WHERE id = @id;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = Sql;
                cmd.AddParameter("@id", Id.Value);
                cmd.ExecuteNonQuery();

                Discard();

                owner.OnChildDeleted(this, raiseBin);
                ConditionalDebug.ThrowIfTrue(_isSelected);
                return true;
            }

            Discard();
        }

        return false;
    }

    /// <summary>
    /// Sets <see cref="Topic"/> and writes the database without rasing a change against the owner.
    /// </summary>
    internal void SetTopicNoRaise(DbConnection con, string? topic)
    {
        // No sanitization
        _topic = topic;
        UpdateDb(con, ModFlags.Topic, false);
    }

    /// <summary>
    /// Sets <see cref="IsWaste"/> and writes the database without rasing a change against the owner.
    /// </summary>
    internal void SetIsWasteNoRaise(DbConnection con, bool value)
    {
        if (_isWaste != value)
        {
            _isWaste = value;
            UpdateDb(con, ModFlags.IsWaste, false);
        }
    }

    /// <summary>
    /// Sets <see cref="IsSelected"/> to false without rasing a selection change against the owner.
    /// </summary>
    internal void DeselectNoRaise()
    {
        // No callback on parent
        if (_isSelected)
        {
            _isSelected = false;
            AccessTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Calls <see cref="Close"/> then sets <see cref="Owner"/> to null, meaning that this instance can no longer write
    /// to the database.
    /// </summary>
    internal void Discard()
    {
        if (Owner != null)
        {
            try
            {
                Close();
                VisualTag = null;
                ConditionalDebug.ThrowIfTrue(_isSelected);
            }
            finally
            {
                Owner = null;
            }
        }
    }

    private static ModFlags[] GetFieldFlags()
    {
        var list = new List<ModFlags>(8);

        foreach (var item in Enum.GetValues<ModFlags>())
        {
            if (item != ModFlags.None && item != ModFlags.Leaf)
            {
                list.Add(item);
            }
        }

        return list.ToArray();
    }

    private bool UpdateDb(ModFlags changes)
    {
        if (changes != ModFlags.None)
        {
            if (IsWritable && Owner != null)
            {
                using var con = Owner.Gardener.Connect();
                UpdateDb(con, changes, true);
            }

            return true;
        }

        return false;
    }

    private DbCommand GetInsertCommand(DbConnection con)
    {
        const string Sql = $"INSERT INTO {TableName} (id, update_time, home_bin, is_waste, title, model, topic) VALUES (@id, @update_time, @home_bin, @is_waste, @title, @model, @topic);";

        var cmd = con.CreateCommand();
        cmd.CommandText = Sql;

        cmd.AddParameter("@id", Id.Value);
        cmd.AddParameter("@update_time", UpdateTime.Ticks);
        cmd.AddParameter("@home_bin", (int)_homeBin);
        cmd.AddParameter("@is_waste", _isWaste);
        cmd.AddParameter("@title", _title);
        cmd.AddParameter("@model", _model);
        cmd.AddParameter("@topic", _topic);

        return cmd;
    }

    private DbCommand GetUpdateCommand(DbConnection con, ModFlags flags)
    {
        const string Sql = $"UPDATE {TableName} SET";
        var buffer = new StringBuilder(Sql);

        var cmd = con.CreateCommand();
        cmd.AddParameter("@id", Id.Value);

        bool i0 = false;

        foreach (var item in FieldFlags)
        {
            if (!flags.HasFlag(item))
            {
                continue;
            }

            switch (item)
            {
                case ModFlags.HomeBin:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" home_bin = @home_bin");
                    cmd.AddParameter("@home_bin", _homeBin);
                    i0 = true;
                    continue;
                case ModFlags.IsWaste:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" is_waste = @is_waste");
                    cmd.AddParameter("@is_waste", _isWaste);
                    i0 = true;
                    continue;
                case ModFlags.Time:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" update_time = @update_time");
                    cmd.AddParameter("@update_time", UpdateTime.Ticks);
                    i0 = true;
                    continue;
                case ModFlags.Title:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" title = @title");
                    cmd.AddParameter("@title", _title);
                    i0 = true;
                    continue;
                case ModFlags.Model:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" model = @model");
                    cmd.AddParameter("@model", _model);
                    i0 = true;
                    continue;
                case ModFlags.Topic:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" topic = @topic");
                    cmd.AddParameter("@topic", _topic);
                    i0 = true;
                    continue;
            }
        }

        ConditionalDebug.ThrowIfFalse(i0);
        buffer.Append(" WHERE id = @id;");
        cmd.CommandText = buffer.ToString();
        return cmd;
    }

}