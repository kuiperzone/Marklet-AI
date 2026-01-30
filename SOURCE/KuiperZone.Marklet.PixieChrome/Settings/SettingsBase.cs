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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Settings;

/// <summary>
/// Base settings class which can be written and read as JSON.
/// </summary>
/// <remarks>
/// Data is expected to be POCO with a <see cref="Changed"/> event which must be called programmatically using <see
/// cref="OnChanged"/>. The subclass should implement <see cref="Equals(SettingsBase?)"/> as this is used for testing
/// and provides the ability to detect changes.
/// </remarks>
public abstract class SettingsBase : IEquatable<SettingsBase>
{
    /// <summary>
    /// Occurs when properties change.
    /// </summary>
    /// <remarks>
    /// Expected to be manually invoked by a call to <see cref="OnChanged"/>.
    /// </remarks>
    public event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Gets whether an initial file did NOT exist when <see cref="Read"/> was called.
    /// </summary>
    /// <remarks>
    /// The value is false until <see cref="Read"/> is called.
    /// </remarks>
    [JsonIgnore]
    public bool IsFirstRead { get; private set; }

    /// <summary>
    /// Gets the file path successfully read by the last call to <see cref="Read"/>.
    /// </summary>
    /// <remarks>
    /// The value is null until <see cref="Read"/> is called.
    /// </remarks>
    [JsonIgnore]
    public string? SettingsPath { get; private set; }

    /// <summary>
    /// Resets all default values.
    /// </summary>
    /// <remarks>
    /// The method should not call <see cref="OnChanged"/>.
    /// </remarks>
    public abstract void Reset();

    /// <summary>
    /// Deserializes from a json string and modifies this class instance.
    /// </summary>
    /// <remarks>
    /// The method should not call <see cref="OnChanged"/>. The <see cref="Read"/> method returns false if <see
    /// cref="Deserialize(string)"/> throws.
    /// </remarks>
    public abstract void Deserialize(string json);

    /// <summary>
    /// Serializes the instance to JSON.
    /// </summary>
    /// <remarks>
    /// The method is called by <see cref="Write"/> which does nothing if <see cref="Serialize"/> returns null. The <see
    /// cref="Write"/> method returns false if <see cref="Serialize"/> throws.
    /// </remarks>
    public abstract string? Serialize();

    /// <summary>
    /// Ensures all values are legal.
    /// </summary>
    /// <remarks>
    /// The <see cref="TrimLegal"/> method is called by <see cref="Read"/>. The method should not call <see
    /// cref="OnChanged"/>. The base method does nothing.
    /// </remarks>
    public virtual void TrimLegal()
    {
    }

    /// <summary>
    /// Reads settings from a file and returns true on success.
    /// </summary>
    /// <remarks>
    /// The call does not throw unless passed an empty "path". The result is true even if the file does not exist, but
    /// the "path" directory does. The result is false if parsing fails, or the "path" directory is invalid.
    /// </remarks>
    /// <exception cref="ArgumentException">Path empty or whitespace</exception>
    public bool Read(string path, bool init = true)
    {
        const string NSpace = $"{nameof(SettingsBase)}.{nameof(Read)}";

        ConditionalDebug.WriteLine(NSpace, $"Path: {path}");
        ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));

        if (File.Exists(path))
        {
            try
            {
                Deserialize(File.ReadAllText(path));
                TrimLegal();

                SettingsPath = path;
                return true;
            }
            catch (Exception e)
            {
                // None critical
                ConditionalDebug.WriteLine(NSpace, e);
            }
        }

        ConditionalDebug.WriteLine(NSpace, "Not exist");

        if (Directory.Exists(Path.GetDirectoryName(path)))
        {
            // Consider success if directory exists
            IsFirstRead = true;
            SettingsPath = path;

            if (init)
            {
                return Write(path);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Writes settings to a file and returns true on success.
    /// </summary>
    /// <remarks>
    /// Where "path" is null, <see cref="SettingsPath"/> is used, i.e. that used by the last successful call to <see
    /// cref="Read"/>. If "path" is null and <see cref="SettingsPath"/> is also null (i.e. the class was never read from
    /// file), then <see cref="Write"/> does nothing and returns false.
    /// </remarks>
    public bool Write(string? path = null)
    {
        const string NSpace = $"{nameof(SettingsBase)}.{nameof(Write)}";

        path ??= SettingsPath;
        ConditionalDebug.WriteLine(NSpace, $"Path: {path}");

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                var s = Serialize();

                if (s != null)
                {
                    File.WriteAllText(path, s);
                    return true;
                }
            }
            catch (Exception e)
            {
                ConditionalDebug.WriteLine(NSpace, e);
            }
        }

        return false;
    }

    /// <summary>
    /// Must be called to invoke <see cref="Changed"/>.
    /// </summary>
    /// <remarks>
    /// Where "write" is true, <see cref="Write"/> is called after <see cref="Changed"/> is invoked.
    /// </remarks>
    public void OnChanged(bool write)
    {
        Changed?.Invoke(this, EventArgs.Empty);

        if (write)
        {
            Write();
        }
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public abstract bool Equals(SettingsBase? other);

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as SettingsBase);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        // Don't need this
        return base.GetHashCode();
    }

    /// <summary>
    /// Static helper which returns the unsigned integer "color" as a solid brush, or "def" where "color" is 0.
    /// </summary>
    /// <remarks>
    /// Where "alpha" is false, the resulting color will alway have an alpha value of 255.
    /// </remarks>
    [return: NotNullIfNotNull(nameof(def))]
    protected static ImmutableSolidColorBrush? ToBrush(uint color, ImmutableSolidColorBrush? def, bool alpha = false)
    {
        if (color == 0)
        {
            return def;
        }

        if (alpha)
        {
            return new(color);
        }

        return new(color | 0xFF000000);
    }

    /// <summary>
    /// Static helper which returns the unsigned integer "color" as a <see cref="Color"/> value, or "def" where "color"
    /// is 0.
    /// </summary>
    /// <remarks>
    /// Where "alpha" is false, the resulting color will alway have an alpha value of 255.
    /// </remarks>
    protected static Color ToColor(uint color, Color def, bool alpha = false)
    {
        if (color == 0)
        {
            return def;
        }

        if (alpha)
        {
            return Color.FromUInt32(color);
        }

        return Color.FromUInt32(color | 0xFF000000);
    }
}
