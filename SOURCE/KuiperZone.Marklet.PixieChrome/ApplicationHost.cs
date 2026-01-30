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

using System.Runtime.InteropServices;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome;

/// <summary>
/// Provides information about the host.
/// </summary>
public sealed class ApplicationHost
{
    private FileStream? _lockHandle;
    private readonly string _lockPath;
    private readonly string _raisePath;

    /// <summary>
    /// Constructor with application reverse domain name identity, i.e. "zone.kuiper.marklet".
    /// </summary>
    /// <exception cref="InvalidOperationException">appId</exception>
    public ApplicationHost(string appId)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId, nameof(appId));
        AppId = appId;

        string? pathId = appId;
        IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        IsBsd = RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        if (IsLinux)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FLATPAK_SANDBOX_DIR")))
            {
                // AppId and FLATPAK_ID are expected to be same!
                // Flatpak already has ID in leading paths
                pathId = null;
                IsFlatpak = true;
            }
            else
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPIMAGE")))
            {
                // APPIMAGE – absolute path of the running AppImage file.
                IsAppImage = true;
            }
        }

        ProcessPath = Path.GetFullPath(Environment.ProcessPath ?? "");
        ProcessDirectory = Path.GetDirectoryName(ProcessPath) ?? Path.GetDirectoryName(AppContext.BaseDirectory) ??
            throw new InvalidOperationException($"Failed to get {nameof(ProcessDirectory)}");

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        ConfigDirectory = GetPathOrDefault("XDG_CONFIG_HOME", pathId, appData);

        var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DataDirectory = GetPathOrDefault("XDG_DATA_HOME", pathId, localData);
        CacheDirectory = GetPathOrDefault("XDG_CACHE_HOME", pathId, Path.GetTempPath());
        RuntimeDirectory = GetPathOrDefault("XDG_RUNTIME_DIR", pathId, Path.GetTempPath());
        DownloadDirectory = GetPathOrDefault("XDG_DOWNLOAD_DIR", null, "~/Downloads");

        // File lock
        _lockPath = Path.Combine(RuntimeDirectory, ".lock");
        _raisePath = Path.Combine(RuntimeDirectory, ".raise");
        HasFileSystemPermission = !IsLinux || Directory.Exists("/proc/sys");

        DebugRuntime();
    }

    /// <summary>
    /// Gets the application reverse domain name identity, i.e. "zone.kuiper.marklet".
    /// </summary>
    public string AppId { get; }

    /// <summary>
    /// Gets whether LINUX.
    /// </summary>
    public bool IsLinux { get; }

    /// <summary>
    /// Gets whether BSD.
    /// </summary>
    public bool IsBsd { get; }

    /// <summary>
    /// Gets whether Windows.
    /// </summary>
    public bool IsWindows { get; }

    /// <summary>
    /// Gets whether Osx.
    /// </summary>
    public bool IsOsx { get; }

    /// <summary>
    /// Gets whether published for AppImage.
    /// </summary>
    public bool IsAppImage { get; }

    /// <summary>
    /// Gets whether running under a flatpak sandbox on Linux.
    /// </summary>
    public bool IsFlatpak { get; }

    /// <summary>
    /// Gets whether <see cref="Initialize"/> has been called.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Gets whether the application holds the exclusive lock handle.
    /// </summary>
    /// <remarks>
    /// The value is set on <see cref="Initialize"/>.
    /// </remarks>
    public bool HasAppLock
    {
        get { return _lockHandle != null; }
    }

    /// <summary>
    /// Gets an indication as to whether app has wider filesystem permissions.
    /// </summary>
    /// <remarks>
    /// The value is always true under non-linux systems. It is true on Linux where the directory "/proc/sys" is
    /// detected.
    /// </remarks>
    public bool HasFileSystemPermission { get; }

    /// <summary>
    /// Gets the main executable path.
    /// </summary>
    public string ProcessPath { get; }

    /// <summary>
    /// Gets the executing assembly directory.
    /// </summary>
    public string ProcessDirectory { get; }

    /// <summary>
    /// Gets the configuration directory.
    /// </summary>
    public string ConfigDirectory { get; }

    /// <summary>
    /// Gets the local data directory.
    /// </summary>
    public string DataDirectory { get; }

    /// <summary>
    /// Gets the local data directory.
    /// </summary>
    public string CacheDirectory { get; }

    /// <summary>
    /// Gets the runtime or temp directory.
    /// </summary>
    public string RuntimeDirectory { get; }

    /// <summary>
    /// Gets the user download directory.
    /// </summary>
    public string DownloadDirectory { get; }

    /// <summary>
    /// Ensure all directories exist and acquires an exclusive lock file.
    /// </summary>
    /// <remarks>
    /// The result is true if the instance holds an exclusive lock file.
    /// </remarks>
    public bool Initialize()
    {
        const string NSpace = $"{nameof(ApplicationHost)}.{nameof(Initialize)}";

        if (_lockHandle == null)
        {
            IsInitialized = true;
            EnsureExists(ConfigDirectory, false);
            EnsureExists(DataDirectory, false);

            EnsureExists(CacheDirectory, true);
            EnsureExists(RuntimeDirectory, true);
            EnsureExists(DownloadDirectory, true);

            // Remove any existing flag
            try { File.Delete(_raisePath); } catch { }

            try
            {
                ConditionalDebug.WriteLine(NSpace, $"Acquire: {_lockPath}");
                _lockHandle = new FileStream(_lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 64, FileOptions.DeleteOnClose);
                ConditionalDebug.WriteLine(NSpace, "Acquired OK");
                return true;
            }
            catch (IOException)
            {
                // Raise flag
                ConditionalDebug.WriteLine(NSpace, "Lock denied");
                try { File.Create(_raisePath).Dispose(); } catch { }
            }
        }

        ConditionalDebug.WriteLine(NSpace, $"Result: {_lockHandle != null}");
        return _lockHandle != null;
    }

    /// <summary>
    /// Returns true if other application attempted to acquire exclusive lock since the last call.
    /// </summary>
    /// <remarks>
    /// The result is cleared when the call return. Application may poll this, say once per second.
    /// </remarks>
    public bool IsLockRequested()
    {
        const string NSpace = $"{nameof(ApplicationHost)}.{nameof(IsLockRequested)}";

        if (File.Exists(_raisePath))
        {
            try
            {
                // Only return true if we can delete it
                ConditionalDebug.WriteLine(NSpace, "Exclusive request detected");
                File.Delete(_raisePath);
                return true;
            }
            catch (Exception e)
            {
                ConditionalDebug.WriteLine(NSpace, e);
            }
        }

        return false;
    }

    /// <summary>
    /// Releases the lock.
    /// </summary>
    public void ReleaseLock()
    {
        _lockHandle?.Dispose();
        _lockHandle = null;
    }

    /// <summary>
    /// Removes pertenant directories as part of a "un-install" process.
    /// </summary>
    /// <remarks>
    /// The <see cref="DataDirectory"/> is removed only if "data" is true. Does nothing where <see cref="IsFlatpak"/> is
    /// true as removal is left to flatpak itself. The <see cref="Initialize"/> method should NOT be called where <see
    /// cref="Uninstall"/> is to be used.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Invalid Initialization</exception>
    public void Uninstall(bool data)
    {
        if (!IsFlatpak)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Invalid Initialization");
            }

            if (_lockHandle != null)
            {
                // Not expected
                throw new InvalidOperationException("Cannot remove while holding exclusive file lock");
            }

            // Expect these to have AppId leaf name
            ConditionalDebug.ThrowIfFalse(ConfigDirectory.Contains(AppId));
            Directory.Delete(ConfigDirectory, true);

            ConditionalDebug.ThrowIfFalse(RuntimeDirectory.Contains(AppId));
            Directory.Delete(RuntimeDirectory, true);

            if (data)
            {
                ConditionalDebug.ThrowIfFalse(DataDirectory.Contains(AppId));
                Directory.Delete(DataDirectory, true);
            }
        }
    }

    private static void EnsureExists(string directory, bool writable)
    {
        const string NSpace = $"{nameof(ApplicationHost)}.{nameof(EnsureExists)}";

        try
        {
            ConditionalDebug.WriteLine(NSpace, "Assert directory: " + directory);
            Directory.CreateDirectory(directory);
        }
        catch (Exception e)
        {
            // Allowed to exist as read-only
            ConditionalDebug.WriteLine(NSpace, e);

            if (writable || !Directory.Exists(directory))
            {
                throw;
            }
        }
    }

    private static string ExpandHome(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (path.StartsWith("~/") || path.StartsWith("~\\") || path == "~")
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, path.Substring(1).TrimStart('/', '\\'));
        }

        return path;
    }

    private string GetPathOrDefault(string? var0, string? id, string def)
    {
        if (var0 != null)
        {
            var value = Environment.GetEnvironmentVariable(var0);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return FinalizePath(id != null ? Path.Combine(value, id) : value);
            }
        }

        return FinalizePath(id != null ? Path.Combine(def, id) : def);
    }

    private string FinalizePath(string path)
    {
#if DEBUG
        return ExpandHome(Path.Combine(path, "debug"));
#else
        // Keep AppImage separate from other installations and avoid conflict.
        return IsAppImage ? ExpandHome(Path.Combine(path, "appimage")) : ExpandHome(path);
#endif
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void DebugRuntime()
    {
        const string NSpace = $"{nameof(ApplicationHost)}.{nameof(DebugRuntime)}";
        ConditionalDebug.WriteLine(NSpace, $"OS: {RuntimeInformation.OSDescription}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(AppId)}: {AppId}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(IsFlatpak)}: {IsFlatpak}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(ProcessDirectory)}: {ProcessDirectory}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(ConfigDirectory)}: {ConfigDirectory}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(DataDirectory)}: {DataDirectory}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(DownloadDirectory)}: {DownloadDirectory}");
        ConditionalDebug.WriteLine(NSpace, $"{nameof(RuntimeDirectory)}: {RuntimeDirectory}");
    }
}