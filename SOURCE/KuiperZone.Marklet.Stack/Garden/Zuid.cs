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
using System.Globalization;
using System.Security.Cryptography;

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// Provides a 64-bit timestamped, sortable, local system unique identifier.
/// </summary>
/// <remarks>
/// The <see cref="Timestamp"/> is UTC with millisecond precision. The class utilizes an internal global counter to
/// ensure uniqueness of values with same timestamp value. The "Z" first letter originates from an earlier project to be
/// called "ZEN".
/// </remarks>
public readonly struct Zuid : IComparable<Zuid>, IEquatable<Zuid>
{
    // We loose a bit to avoid negative values.
    private const int SignBits = 1;
    private const int TimeBits = 45;  // <- 18 bits of counter
    private const int TimeShift = 64 - SignBits - TimeBits;
    private const long SequenceMask = (long)(ulong.MaxValue >> (64 - TimeShift));
    private const long MaxMilliseconds = (long)(ulong.MaxValue >> (64 - TimeBits));
    private static long s_sequence = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));

    /// <summary>
    /// Gets the number sequence bits for information purposes.
    /// </summary>
    public const int SequenceBits = TimeShift;

    /// <summary>
    /// Constructor with underlying 64-bit integer value.
    /// </summary>
    /// <remarks>
    /// Negative values are clamped to 0.
    /// </remarks>
    public Zuid(long value)
    {
        Value = Math.Max(value, 0U);
    }

    private Zuid(DateTime utc)
    {
        long t = (long)ClampTime(utc).Subtract(ZeroTime).TotalMilliseconds << TimeShift;
        Value = t | (Interlocked.Increment(ref s_sequence) & SequenceMask);
    }

    /// <summary>
    /// Gets the minimum possible time value.
    /// </summary>
    public static readonly DateTime ZeroTime = new(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets the maximum possible time value.
    /// </summary>
    public static readonly DateTime MaxTime = ZeroTime.AddMilliseconds(MaxMilliseconds);

    /// <summary>
    /// Zero or default empty value.
    /// </summary>
    public static readonly Zuid Empty = default;

    /// <summary>
    /// Gets the underlying 64-bit integer value.
    /// </summary>
    /// <remarks>
    /// The default instance of <see cref="Zuid"/> has a value of 0 and is considered invalid.
    /// </remarks>
    public readonly long Value;

    /// <summary>
    /// Gets the timestamp as UTC with millisecond precision.
    /// </summary>
    /// <remarks>
    /// The default instance of <see cref="Zuid"/> has a <see cref="Timestamp"/> value of <see
    /// cref="ZeroTime"/>.
    /// </remarks>
    public DateTime Timestamp
    {
        get { return ZeroTime.AddMilliseconds(Value >> TimeShift); }
    }

    /// <summary>
    /// Returns true if this is the undefined <see cref="Zuid"/> default value.
    /// </summary>
    public bool IsEmpty
    {
        get { return Value <= 0L; }
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Zuid left, Zuid right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Zuid left, Zuid right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Generates a new timestamped value from the system clock.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Clock out of range</exception>
    public static Zuid New()
    {
        return new(DateTime.UtcNow);
    }

    /// <summary>
    /// Generates a new timestamped <see cref="Zuid"/> value using the time value provided.
    /// </summary>
    public static Zuid New(DateTime time)
    {
        return new(time.ToUniversalTime());
    }

    /// <summary>
    /// Gets whether system clock is within valid range.
    /// </summary>
    public static bool IsClockValid()
    {
        // This should not fail.
        // Range was chosen to accommodate systems with seriously erroneous clocks.
        var now = DateTime.UtcNow;
        return now > ZeroTime && now < MaxTime;
    }

    /// <summary>
    /// Throws if <see cref="IsEmpty"/> is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">Invalid</exception>
    public void ThrowIfEmpty()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException($"Invalid {nameof(Zuid)}");
        }
    }

    /// <summary>
    /// Implements <see cref="IComparable{T}"/>.
    /// </summary>
    public int CompareTo(Zuid other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(Zuid other)
    {
        return Value == other.Value;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Zuid zuid && Equals(zuid);
    }

    /// <summary>
    /// Returns <see cref="Value"/> as a hexadecimal string with separators.
    /// </summary>
    public override string ToString()
    {
        return Value.ToString("X16", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Where "timestamp" is true, returns <see cref="Timestamp"/> in the form "yyyy'-'MM'-'dd HH':'mm':'ss.fff'Z'".
    /// Otherwise, same as default ToString().
    /// </summary>
    public string ToString(bool timestamp)
    {
        if (timestamp)
        {
            return Timestamp.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture);
        }

        return ToString();
    }

    /// <summary>
    /// Overrides and returns the hash-code of <see cref="Value"/>.
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    private static DateTime ClampTime(DateTime utc)
    {
        // Out of range unexpected, but we will still function.
        // We will be relying on counter bits for uniqueness.
        if (utc < ZeroTime)
        {
            return ZeroTime;
        }

        if (utc > MaxTime)
        {
            return MaxTime;
        }

        return utc;
    }
}
