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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Lightweight "sort key" used by <see cref="CrossTracker"/> and <see cref="ICrossTrackable"/> to define a sortable screen position.
/// </summary>
/// <remarks>
/// Although for most controls, the value will be unique, it is not guaranteed to be so.
/// </remarks>
public readonly struct CrossKey : IComparable<CrossKey>, IEquatable<CrossKey>
{
    private const double SlackY = 5.0;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// Note that "y" is first by design (we prioritize the y-position).
    /// </remarks>
    public CrossKey(double y, double x = 0.0)
    {
        Y = y;
        X = x;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public CrossKey(Point p)
    {
        Y = p.Y;
        X = p.X;
    }

    /// <summary>
    /// Gets an invalid empty instance.
    /// </summary>
    public static readonly CrossKey Empty = new(double.NaN, double.NaN);

    /// <summary>
    /// Gets the Y position.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Gets the X position.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets whether the value is considered valid.
    /// </summary>
    public bool IsValid
    {
        get { return double.IsFinite(Y); }
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(CrossKey left, CrossKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Equality not operator.
    /// </summary>
    public static bool operator !=(CrossKey left, CrossKey right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Given a Visual instance and "root", returns the current <see cref="CrossKey"/> value.
    /// </summary>
    /// <remarks>
    /// The value will need re-calculating if the visual control is moved. If "root" is null, the result will use the
    /// top level control.
    /// </remarks>
    public static CrossKey GetKeyPoint(Visual visual, Visual? root)
    {
        var r = visual.Bounds;

        if (r.Width > 0.0 && r.Height > 0.0)
        {
            double y = 0.0;
            double x = 0.0;

            while (visual.Parent != root)
            {
                // Endless loop otherwise
                // Can only occur if given a non-parent root
                ConditionalDebug.ThrowIfNull(visual.Parent);

                y += r.TopLeft.Y;
                x += r.TopLeft.X;

                if (visual.Parent is Visual parent)
                {
                    visual = parent;
                    r = visual.Bounds;
                    continue;
                }

                break;
            }

            return new(y, x);
        }

        return Empty;
    }

    /// <summary>
    /// Returns true if "other" has an equal or nearly equal (to within a few pixels) vertical <see cref="Y"/> position.
    /// </summary>
    public bool IsHorizontalWith(CrossKey other)
    {
        return Y >= other.Y - SlackY && Y <= other.Y + SlackY;
    }

    /// <summary>
    /// Get as a point.
    /// </summary>
    public Point AsPoint()
    {
        return new(X, Y);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Y, X);
    }

    /// <summary>
    /// Implements <see cref="IComparable{T}"/>.
    /// </summary>
    public int CompareTo(CrossKey other)
    {
        // Prioritize Y allowing a little slack
        var oy = other.Y;

        if (Y < oy - SlackY)
        {
            return -1;
        }

        if (Y > oy + SlackY)
        {
            return 1;
        }

        // Few will get here
        int xr = X.CompareTo(other.X);

        if (xr != 0)
        {
            return xr;
        }

        // Fall back
        return Y.CompareTo(oy);
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(CrossKey other)
    {
        return Y == other.Y && X == other.X;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is CrossKey k && Equals(k);
    }

    /// <summary>
    /// Returns as string.
    /// </summary>
    public override string ToString()
    {
        return string.Concat("(", Y.ToString(), ", ", X.ToString(), ")");
    }
}