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

using System.Data.Common;
using System.Text;
using KuiperZone.Marklet.Stack.Garden.Internal;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Message leaf containing <see cref="Content"/>.
/// </summary>
/// <remarks>
/// The instance is readonly at class level, but properties may change in response to calls on the <see cref="Session"/>
/// <see cref="GardenSession"/> instance.
/// </remarks>
public sealed class GardenLeaf : IComparable<GardenLeaf>
{
    /// <summary>
    /// Gets the underlying table name (do not change).
    /// </summary>
    public const string TableName = "message_leaf";

    /// <summary>
    /// Gets the table version, where 1 is the first release.
    /// </summary>
    public const int Version = 1;

    /// <summary>
    /// Gets the maximum possible <see cref="Content"/> length.
    /// </summary>
    /// <remarks>
    /// We can make this relatively high, but not unbounded.
    /// </remarks>
    public const int MaxContentLength = 16 * 1024 * 1024;

    private string? _model;
    private string _content = "";
    private bool _pendingInsertion;

    private static readonly LeafChanges[] FieldFlags = GetFieldFlags();

    /// <summary>
    /// Insertion constructor.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid kind</exception>
    internal GardenLeaf(GardenSession session, LeafKind kind, bool streaming)
        : this(session, Zuid.New(), kind)
    {
        if (kind == LeafKind.None)
        {
            throw new ArgumentException($"Invalid {nameof(LeafKind)} = {kind}", nameof(kind));
        }

        _pendingInsertion = true;
        _model = kind.HasModel() ? session.Model : null;
        IsStreaming = streaming;
    }

    private GardenLeaf(GardenSession session, Zuid id, LeafKind kind)
    {
        // Read constructor
        ConditionalDebug.ThrowIfTrue(id.IsEmpty);
        ConditionalDebug.ThrowIfEqual(LeafKind.None, kind);
        Session = session;
        Id = id;
        Kind = kind;
        VisualCounter = Random.Shared.NextInt64() + 1;
    }

    /// <summary>
    /// Gets the <see cref="GardenSession"/> to which this message belongs.
    /// </summary>
    public GardenSession Session { get; }

    /// <summary>
    /// Gets whether the instance is currently writable to storage.
    /// </summary>
    /// <remarks>
    /// The result is true if: A. <see cref="LeafKind"/> is persistant, B. the parent <see
    /// cref="GardenSession.IsWritable"/> is true and, C. <see cref="IsStreaming"/> is false.
    /// </remarks>
    public bool IsWritable
    {
        get { return Kind.IsPersistant() && Session.IsWritable && !IsStreaming; }
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
    /// Gets the assistant model name in lowercase.
    /// </summary>
    /// <remarks>
    /// The value is converted to lowercase on assignment. It is expected to be null for human input.
    /// </remarks>
    public string? Model
    {
        get { return _model; }

        set
        {
            value = MemoryGarden.SanitizeName(value)?.ToLowerInvariant();

            if (_model != value)
            {
                _model = value;
                UpsertDb(LeafChanges.Model);
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
            value = MarkDocument.Sanitize(value.Trim(), MaxContentLength);

            if (_content != value)
            {
                _content = value;
                UpsertDb(LeafChanges.Content);
            }
        }
    }

    /// <summary>
    /// Gets whether the message is currently streaming (chunks).
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
            if (Kind == LeafKind.Assistant && IsStreaming)
            {
                return MarkOptions.Presan | MarkOptions.Chunking;
            }

            if (Kind.HasInlineMarkup())
            {
                // Content is sanitized on setting.
                // The markdown parser need not re-do this.
                return MarkOptions.Presan;
            }

            return MarkOptions.Presan | MarkOptions.IgnoreInline;
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
        ConditionalDebug.WriteLine(NSpace, $"APPEND STREAM on: {Session}");
        ConditionalDebug.WriteLine(NSpace, $"Kind: {Kind}");
        ConditionalDebug.WriteLine(NSpace, $"IsStreaming: {IsStreaming}");

        if (IsStreaming && !string.IsNullOrEmpty(chunk))
        {
            chunk = MarkDocument.Sanitize(chunk, MaxContentLength);
            _content = string.Concat(_content, chunk).Truncate(MaxContentLength);

            // Even tho not written, UpsertDb() can still needed (it won't write).
            ConditionalDebug.ThrowIfTrue(IsWritable);
            UpsertDb(LeafChanges.Content);

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
            UpsertDb(LeafChanges.Content);
            return true;
        }

        return false;
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
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Gets a count of messages, or total count if "parentId" is default.
    /// </summary>
    internal static long CountInternal(IMemoryGardener gardener, Zuid parentId = default)
    {
        using var con = gardener.Connect();
        using var cmd = con.CreateCommand();

        if (parentId.IsEmpty)
        {
            const string Sql = $"SELECT COUNT(*) FROM {TableName};";
            cmd.CommandText = Sql;
        }
        else
        {
            const string Sql = $"SELECT COUNT(*) FROM {TableName} WHERE parent_id = @parent_id;";
            cmd.CommandText = Sql;
            cmd.AddParameter("@parent_id", parentId.Value);
        }

        return (long)(cmd.ExecuteScalar() ?? 0);
    }

    /// <summary>
    /// Ensure table.
    /// </summary>
    internal static void CreateTableInternal(DbConnection con, DbTransaction? tran)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(CreateTableInternal)}";

        // VERSION 1
        const string Sql = $@"CREATE TABLE IF NOT EXISTS {TableName} (
id              BIGINT PRIMARY KEY,
parent_id       BIGINT NOT NULL,
kind            INTEGER NOT NULL,
model           VARCHAR(255),
content         TEXT NOT NULL,
CONSTRAINT  fk_parent
    FOREIGN KEY (parent_id)
    REFERENCES {GardenSession.TableName}(id)
    ON DELETE CASCADE
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
    internal static bool UpgradeTableInternal(DbConnection _, DbTransaction? __, int currentVersion)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(UpgradeTableInternal)}";

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
    /// Reads all message items pertaining to a <see cref="GardenSession"/>.
    /// </summary>
    internal static void ReadInternal(DbConnection con, GardenSession item, List<GardenLeaf> children)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(ReadInternal)}";

        const string Sql = $"SELECT * FROM {TableName} WHERE parent_id = @parent_id ORDER BY id";
        ConditionalDebug.WriteLine(NSpace, Sql);

        using var cmd = con.CreateCommand();
        cmd.CommandText = Sql;
        cmd.AddParameter("@parent_id", item.Id.Value);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var zuid = new Zuid(reader.GetInt64("id"));
            var kind = (LeafKind)reader.GetInt32("kind");

            if (!zuid.IsEmpty && kind != LeafKind.None)
            {
                var leaf = new GardenLeaf(item, zuid, kind);
                leaf._model = reader.GetStringOrNull("model");
                leaf._content = reader.GetStringOrEmpty("content");

                // Assert expected properties
                ConditionalDebug.ThrowIfNotEqual(zuid, leaf.Id);
                ConditionalDebug.ThrowIfNotEqual(kind, leaf.Kind);
                ConditionalDebug.WriteLine(NSpace, $"Content length: {leaf.Content.Length}");

                children.Add(leaf);
            }
        }
    }

    internal void InsertDb(DbConnection con)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(InsertDb)}";

        if (IsWritable)
        {
            using var cmd = GetInsertCommand(con);
            ConditionalDebug.WriteLine(NSpace, cmd.CommandText);
            cmd.ExecuteNonQuery();

            IsStreaming = false;
            _pendingInsertion = false;
        }
    }

    private static LeafChanges[] GetFieldFlags()
    {
        var list = new List<LeafChanges>(4);

        foreach (var item in Enum.GetValues<LeafChanges>())
        {
            if (item != LeafChanges.None)
            {
                list.Add(item);
            }
        }

        return list.ToArray();
    }

    private bool UpsertDb(LeafChanges flags)
    {
        const string NSpace = $"{nameof(GardenLeaf)}.{nameof(UpsertDb)}";
        ConditionalDebug.WriteLine(NSpace, $"Changes: {flags}");

        if (flags != LeafChanges.None)
        {
            if (flags.HasFlag(LeafChanges.Model) || flags.HasFlag(LeafChanges.Content))
            {
                // Visual changes only
                VisualCounter += 1;
                ConditionalDebug.WriteLine(NSpace, "Visual change");
            }

            if (IsWritable && Session.Owner != null)
            {
                ConditionalDebug.WriteLine(NSpace, "Connect and get command");
                using var con = Session.Owner.Gardener.Connect();
                using var cmd = _pendingInsertion ? GetInsertCommand(con) : GetUpdateCommand(con, flags);

                ConditionalDebug.WriteLine(NSpace, cmd.CommandText);
                cmd.ExecuteNonQuery();

                ConditionalDebug.WriteLine(NSpace, "Call parent");
                _pendingInsertion = false;
                Session.UpdateDb(con, ModFlags.Leaf, true);
            }
            else
            {
                // Still raise but with no connection
                ConditionalDebug.WriteLine(NSpace, "Non-writable change");
                Session.UpdateDb(null, ModFlags.Leaf, true);
            }

            return true;
        }

        return false;
    }

    private DbCommand GetInsertCommand(DbConnection con)
    {
        const string Sql = $"INSERT INTO {TableName} (id, parent_id, kind, model, content) VALUES (@id, @parent_id, @kind, @model, @content);";

        if (!_pendingInsertion)
        {
            throw new InvalidOperationException($"{nameof(GardenLeaf)} not pending insertion");
        }

        var cmd = con.CreateCommand();
        cmd.CommandText = Sql;

        cmd.AddParameter("@id", Id.Value);
        cmd.AddParameter("@parent_id", Session.Id.Value);
        cmd.AddParameter("@kind", (int)Kind);  // <- enum as int
        cmd.AddParameter("@model", _model);
        cmd.AddParameter("@content", _content);

        return cmd;
    }

    private DbCommand GetUpdateCommand(DbConnection con, LeafChanges flags)
    {
        const string Sql = $"UPDATE {TableName} SET";

        if (flags == LeafChanges.None)
        {
            throw new ArgumentException($"{nameof(LeafChanges)} equal {LeafChanges.None}", nameof(flags));
        }

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
                case LeafChanges.Model:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" model = @model");
                    cmd.AddParameter("@model", _model);
                    i0 = true;
                    continue;
                case LeafChanges.Content:
                    if (i0)
                    {
                        buffer.Append(',');
                    }
                    buffer.Append(" content = @content");
                    cmd.AddParameter("@content", _content);
                    i0 = true;
                    continue;
            }
        }

        ConditionalDebug.ThrowIfFalse(i0);
        buffer.Append(" WHERE id = @id;");
        cmd.CommandText = buffer.ToString();
        return cmd;
    }


    [Flags]
    private enum LeafChanges
    {
        None = 0x0000,
        Model = 0x0001,
        Content = 0x0002,
    }

}