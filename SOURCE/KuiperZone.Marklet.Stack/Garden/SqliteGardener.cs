// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
// -----------------------------------------------------------------------------

using System.Data.Common;
using KuiperZone.Marklet.Tooling;
using Microsoft.Data.Sqlite;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// SQLite implementation of <see cref="IMemoryGardener"/>
/// </summary>
public sealed class SqliteGardener : IMemoryGardener
{
    /// <summary>
    /// File extension.
    /// </summary>
    public const string Extension = ".db";

    private readonly DbConnection? _mem;
    private readonly string _connection;

    /// <summary>
    /// Constructor with path.
    /// </summary>
    /// <remarks>
    /// The "path" maybe a directory, in which case, a default filename is used. The file is created if needed. The
    /// directory must exist.
    /// </remarks>
    /// <exception cref="FileNotFoundException">File not found in read-only location</exception>
    /// <exception cref="DirectoryNotFoundException">Directory not found</exception>
    /// <exception cref="UnauthorizedAccessException">Access denied</exception>
    public SqliteGardener(string path)
    {
        const string NSpace = $"{nameof(SqliteGardener)}.Constructor";
        ConditionalDebug.WriteLine(NSpace, $"Directory: {path}");

        if (path.Length == 0)
        {
            throw new ArgumentException("Path is empty", nameof(path));
        }

        var directory = path;
        Location = "default" + Extension;

        if (Directory.Exists(path))
        {
            path = Path.Combine(path, Location);
        }
        else
        {
            Location = Path.GetFileName(path);
            directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory not found {directory}");
            }
        }

        try
        {
            // For SQLite variant (not Sqlite), include: Version=3;
            _connection = $"Data Source={path};Pooling=true;Mode=ReadWriteCreate;";
            using var con = new SqliteConnection(_connection);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS __rwtest(id INTEGER);";
            cmd.ExecuteNonQuery();
        }
        catch (SqliteException e)
        {
            ConditionalDebug.WriteLine(NSpace, e);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File {Location} not found in read-only location: {directory}");
            }

            IsReadOnly = true;
            _connection = $"Data Source={path};Pooling=true;Mode=ReadOnly;";
        }
    }

    private SqliteGardener(bool readOnly)
    {
        var name = "mem-" + Random.Shared.Next().ToString();

        // Do not set "mode=readonly;" - we still need to create the table
        _connection = $"Data Source=file:{name}?mode=memory&cache=shared;pooling=true;";

        Location = "MEMORY";
        IsReadOnly = readOnly;

        // Need to keep this open
        _mem = new SqliteConnection(_connection);
        _mem.Open();
    }

    /// <inherit cref="IMemoryGardener.Location"/>
    public string? Location { get; }

    /// <inherit cref="IMemoryGardener.IsReadOnly"/>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Create in-memory instance.
    /// </summary>
    /// <remarks>
    /// Intended for test purposes only. Contents are lost when the gardener is garbage collected.
    /// </remarks>
    public static SqliteGardener NewMemory(bool readOnly = false)
    {
        return new(readOnly);
    }

    /// <inherit cref="IMemoryGardener.Connect()"/>
    public DbConnection Connect()
    {
        const string NSpace = $"{nameof(SqliteGardener)}.{nameof(Connect)}";
        ConditionalDebug.WriteLine(NSpace, _connection);

        var con = new SqliteConnection(_connection);

        con.Open();

        // Foreign keys important
        using var pragma = con.CreateCommand();
        pragma.CommandText = IsReadOnly ? "PRAGMA foreign_keys = ON;" : "PRAGMA foreign_keys = ON;PRAGMA journal_mode=WAL;";

        ConditionalDebug.WriteLine(NSpace, "Executing: " + pragma.CommandText);
        pragma.ExecuteNonQuery();
        return con;
    }

}
