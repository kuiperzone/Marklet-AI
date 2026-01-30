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

using Avalonia.Input;

namespace KuiperZone.Marklet.PixieChrome.Shared;

/// <summary>
/// Controls the zoom "level".
/// </summary>
public sealed class Zoomer
{
    /// <summary>
    /// Min <see cref="Scale"/> value.
    /// </summary>
    public const int MinScale = 50;

    /// <summary>
    /// Max <see cref="Scale"/> value..
    /// </summary>
    public const int MaxScale = 300;

    private const double ZoomDeltaF = 1.10;
    private int _scale = 100;
    private int _default = 100;

    /// <summary>
    /// Occurs when <see cref="Scale"/> changes.
    /// </summary>
    public event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Gets or sets the current zoom scale as a percentage in the range [<see cref="MinScale"/>, <see
    /// cref="MaxScale"/>].
    /// </summary>
    /// <remarks>
    /// The default is 100.
    /// </remarks>
    public int Scale
    {
        get { return _scale; }

        set
        {
            value = Math.Clamp(value, MinScale, MaxScale);

            if (_scale != value)
            {
                _scale = value;
                Fraction = _scale / 100.0;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the default level as a percentage in the range [<see cref="MinScale"/>, <see
    /// cref="MaxScale"/>].
    /// </summary>
    /// <remarks>
    /// The default is 100.
    /// </remarks>
    public int Default
    {
        get { return _default; }

        set
        {
            value = Math.Clamp(value, MinScale, MaxScale);

            if (_default != value)
            {
                // Current
                bool equal = _default == _scale;

                _default = value;

                if (equal)
                {
                    Scale = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets <see cref="Scale"/> value as a fraction value, i.e. 10% = 0.1.
    /// </summary>
    public double Fraction { get; private set; } = 1.0;

    /// <summary>
    /// Gets whether <see cref="Scale"/> equals <see cref="Default"/>.
    /// </summary>
    public bool IsDefault
    {
        get { return _scale == _default; }
    }

    /// <summary>
    /// Gets whether <see cref="Scale"/> equals <see cref="MinScale"/>.
    /// </summary>
    public bool IsMinimum
    {
        get { return _scale == MinScale; }
    }

    /// <summary>
    /// Gets whether <see cref="Scale"/> equals <see cref="MaxScale"/>.
    /// </summary>
    public bool IsMaximum
    {
        get { return _scale == MaxScale; }
    }

    /// <summary>
    /// Increments <see cref="Scale"/> and returns true if changed.
    /// </summary>
    public bool Increment()
    {
        var old = _scale;
        var valueF = Math.Clamp(old * ZoomDeltaF, MinScale, MaxScale);

        if ((old < _default && valueF > _default) || (valueF > _default - 5.0 && valueF < _default + 5.0))
        {
            // Lock on default
            Scale = _default;
            return _scale != old;
        }

        Scale = (int)valueF;
        return _scale != old;
    }

    /// <summary>
    /// Increments <see cref="Scale"/> and returns true if changed.
    /// </summary>
    public bool Decrement()
    {
        var old = _scale;
        var valueF = Math.Clamp(old / ZoomDeltaF, MinScale, MaxScale);

        if ((old > _default && valueF < _default) || (valueF > _default - 5.0 && valueF < _default + 5.0))
        {
            // Lock on default
            Scale = 100;
            return _scale != old;
        }

        Scale = (int)valueF;
        return _scale != old;
    }

    /// <summary>
    /// Sets <see cref="Scale"/> to 100 and returns true if changed.
    /// </summary>
    public bool Reset()
    {
        if (_scale != _default)
        {
            Scale = _default;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Increments (CTRL+OemPlus), decrements (CTRL+OemMinus) or resets (CTRL+Digit0), and returns true if the key was
    /// accepted.
    /// </summary>
    /// <remarks>
    /// The method always returns false where the event is marked as "Handled", and sets the Handled flag where the result is true.
    /// </remarks>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && !e.Handled)
        {
            if (e.Key == Key.OemPlus || e.PhysicalKey == PhysicalKey.NumPadAdd)
            {
                e.Handled = true;
                Increment();
                return true;
            }

            if (e.Key == Key.OemMinus || e.PhysicalKey == PhysicalKey.NumPadSubtract)
            {
                e.Handled = true;
                Decrement();
                return true;
            }

            if (e.PhysicalKey == PhysicalKey.Digit0 || e.PhysicalKey == PhysicalKey.NumPad0)
            {
                e.Handled = true;
                Reset();
                return true;

            }
        }

        return false;
    }
}