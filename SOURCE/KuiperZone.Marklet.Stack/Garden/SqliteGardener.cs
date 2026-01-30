// -----------------------------------------------------------------------------
// PROJECT   : RigelBlue.Zen
// COPYRIGHT : Rigel Blue © 2023-25
// AUTHOR    : Andrew Thomas
// LICENSE   : LicenseRef-Proprietary
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
    private static int s_gate;
    private static DbConnection? s_mem;
    private readonly string _connection;

    /// <summary>
    /// Default constructor (in-memory only).
    /// </summary>
    /// <remarks>
    /// A single connection will be held permanently open on the first time this constructor is called, thus lasting
    /// the lifetime of the application. Other calls to this constructor are intended for test purposes only and will be
    /// assigned a random internal name, so that data written with a connection from one gardener cannot be read with a
    /// connection from another.
    /// </remarks>
    public SqliteGardener()
    {
        var name = "mem-" + Random.Shared.Next().ToString();
        _connection = $"Data Source=file:{name}?mode=memory&cache=shared;";

        if (Interlocked.Increment(ref s_gate) == 1)
        {
            s_mem = new SqliteConnection(_connection);
            s_mem.Open();
        }
    }

    /// <summary>
    /// Constructor with directory and filename.
    /// </summary>
    /// <remarks>
    /// The "directory" must exist. The "filename" will be created as needed.
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Directory not found</exception>
    public SqliteGardener(string directory, string? filename = null)
    {
        const string NSpace = $"{nameof(SqliteGardener)}.Constructor";
        ConditionalDebug.WriteLine(NSpace, $"Directory: {directory}");

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found {directory}");
        }

        // For SQLite variant (not Sqlite), include: Version=3;
        var path = Path.Combine(directory, filename ?? "marklet.db");
        _connection = $"Data Source={path};";
    }

    /// <inherit cref="IMemoryGardener.Connect()"/>
    public DbConnection Connect()
    {
        const string NSpace = $"{nameof(SqliteGardener)}.{nameof(Connect)}";
        var con = new SqliteConnection(_connection);

        try
        {
            con.Open();

            // Foreign keys important
            using var pragma = con.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
            return con;
        }
        catch (Exception e)
        {
            ConditionalDebug.WriteLine(NSpace, e);
            con.Dispose();
            throw;
        }
    }
}
