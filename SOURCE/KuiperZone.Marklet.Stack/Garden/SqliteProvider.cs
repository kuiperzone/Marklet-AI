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
/// SQLite implementation of <see cref="IServiceProvider"/>
/// </summary>
public sealed class SqliteProvider : IServiceProvider
{
    private readonly DbConnection? _mem;

    /// <summary>
    /// Constructor with source filepath.
    /// </summary>
    /// <exception cref="ArgumentException">Empty source path</exception>
    /// <exception cref="DirectoryNotFoundException">Directory not found</exception>
    /// <exception cref="SqliteException">Sqlite error</exception>
    public SqliteProvider(string source, ProviderFlags flags = ProviderFlags.None)
    {
        const string NSpace = $"{nameof(SqliteProvider)}.Constructor";
        Diag.WriteLine(NSpace, $"Path: {source}");

        Diag.WriteLine(NSpace, $"Flags: {flags}");
        Flags = flags;

        if (flags.HasFlag(ProviderFlags.Memory))
        {
            Diag.WriteLine(NSpace, "TEST ONLY");

            // Cannot set ReadOnly here, as must create tables everything.
            // We use a unique mem reference which will likely last for app lifetime.
            var name = "mem-" + Random.Shared.Next().ToString();

            Source = "MEMORY";
            Connection = $"Data Source=file:{name}?mode=memory&cache=shared;Pooling=true;";

            // Not readonly
            Flags &= ~ProviderFlags.ReadOnly;
            Diag.ThrowIfTrue(IsReadOnly);

            // Need to keep this open
            _mem = new SqliteConnection(Connection);
            _mem.Open();
            return;
        }

        Source = source.Trim();

        if (string.IsNullOrEmpty(Source))
        {
            throw new ArgumentException("Empty source path", nameof(source));
        }

        var directory = Path.GetDirectoryName(Source);

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException("Directory not found " + directory);
        }

        // For SQLite variant (not MS Sqlite), include: Version=3;
        IsReadOnly = flags.HasFlag(ProviderFlags.ReadOnly);

        if (!IsReadOnly)
        {
            Connection = $"Data Source={Source};Pooling=true;Mode=ReadWriteCreate;";
            Diag.WriteLine(NSpace, "READ-WRITE: " + Connection);

            try
            {
                using var con = Connect();
                ExecuteWriteTest(con);

                // Success
                return;
            }
            catch (SqliteException e)
            {
                Diag.WriteLine(NSpace, e);

                if (e.SqliteErrorCode != 6 && e.SqliteErrorCode != 8 && e.SqliteErrorCode != 14)
                {
                    // SQLITE_LOCKED (6)
                    // SQLITE_READONLY (8)
                    // SQLITE_CANTOPEN (14)
                    // Otherwise fall-thru to try read-only
                    throw;
                }
            }
        }

        // Assumed readonly but will catch error later
        IsReadOnly = true;
        Flags &= ~ProviderFlags.WalNormal;
        Flags &= ~ProviderFlags.SchemaInit1;
        Connection = ToReadOnlyConnection(Source);
        Diag.WriteLine(NSpace, "READONLY: " + Connection);
    }

    private SqliteProvider(string source, string connection, ProviderFlags flags)
    {
        Source = source;
        Connection = connection;
        Flags = flags;
        IsReadOnly = flags.HasFlag(ProviderFlags.ReadOnly);
    }

    /// <inherit cref="IServiceProvider.Source"/>
    public string Source { get; }

    /// <inherit cref="IServiceProvider.Connection"/>
    public string Connection { get; }

    /// <inherit cref="IServiceProvider.IsReadOnly"/>
    public bool IsReadOnly { get; }

    /// <inherit cref="IServiceProvider.Flags"/>
    public ProviderFlags Flags { get; }

    /// <summary>
    /// Gets the size on disk of a Sqlite database in bytes, including expected "journal" and "wal" files.
    /// </summary>
    /// <remarks>
    /// On failure, the result is 0.
    /// </remarks>
    public static long SizeOnDisk(string? source)
    {
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
        {
            return 0;
        }

        long sum = new FileInfo(source).Length;

        var info = new FileInfo(source + "-wal");

        if (info.Exists)
        {
            sum += info.Length;
        }

        info = new FileInfo(source + "-journal");

        if (info.Exists)
        {
            sum += info.Length;
        }

        return sum;
    }

    /// <inherit cref="IServiceProvider.Connect()"/>
    public DbConnection Connect()
    {
        const string NSpace = $"{nameof(SqliteProvider)}.{nameof(Connect)}";
        Diag.WriteLine(NSpace, Connection);

        var con = new SqliteConnection(Connection);
        con.Open();

        if (!IsReadOnly)
        {
            // Foreign keys important
            // Must be set on every connection
            ExecuteNonQuery(con, null, "PRAGMA foreign_keys = ON;");

            if (Flags.HasFlag(ProviderFlags.WalNormal))
            {
                // Default is FULL
                // NORMAL to be used only with WAL
                // Must be set on every connection
                ExecuteNonQuery(con, null, "PRAGMA synchronous = NORMAL;");
            }
        }

        return con;
    }

    /// <inherit cref="IServiceProvider.GetSize"/>
    public long GetSize()
    {
        if (!Flags.HasFlag(ProviderFlags.Memory))
        {
            return SizeOnDisk(Source);
        }

        return 0;
    }

    /// <inherit cref="IServiceProvider.CloneReadOnly"/>
    public IServiceProvider CloneReadOnly()
    {
        if (Flags.HasFlag(ProviderFlags.Memory))
        {
            return new SqliteProvider(Source, Connection, Flags | ProviderFlags.ReadOnly);
        }

        return new SqliteProvider(Source, ToReadOnlyConnection(Source), Flags | ProviderFlags.ReadOnly);
    }

    private static string ToReadOnlyConnection(string source)
    {
        if (!source.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            source = "file:" + source;
        }

        // Need URI and Mode=ro here
        return $"Data Source={source}?Mode=ro&immutable=1;pooling=true;";
    }

    private void ExecuteWriteTest(DbConnection con)
    {
        const string NSpace = $"{nameof(SqliteProvider)}.{nameof(ExecuteWriteTest)}";
        Diag.WriteLine(NSpace, "Write test");

        using var tran = con.BeginTransaction();
        ExecuteNonQuery(con, tran, "CREATE TABLE __rwtest(id INTEGER);");
    }

    private static void ExecuteNonQuery(DbConnection con, DbTransaction? tran, string? sql)
    {
        const string NSpace = $"{nameof(SqliteProvider)}.{nameof(ExecuteNonQuery)}";

        if (!string.IsNullOrEmpty(sql))
        {
            using var cmd = con.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = sql;

            Diag.WriteLine(NSpace, cmd.CommandText);
            cmd.ExecuteNonQuery();
        }
    }
}
