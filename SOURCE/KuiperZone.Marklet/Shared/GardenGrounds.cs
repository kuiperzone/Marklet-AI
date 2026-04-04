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

using Avalonia.Threading;
using KuiperZone.Marklet.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Provides a global instance of <see cref="MemoryGarden"/>
/// </summary>
public static class GardenGrounds
{
    // Short delay
    private static readonly DispatcherTimer _timer = new(TimeSpan.FromMilliseconds(200), DispatcherPriority.Default, TickHandler);
    private static FindOptions? _options;
    private static bool _isMissionSearch;

    /// <summary>
    /// Occurs when <see cref="Find"/> change.
    /// </summary>
    /// <remarks>
    /// Note that the handler is global. The sender is null.
    /// </remarks>
    public static event EventHandler<EventArgs>? FindChanged;

    /// <summary>
    /// Occurs when <see cref="IsMissionSearch"/> changes.
    /// </summary>
    /// <remarks>
    /// Note that the handler is global. The sender is null.
    /// </remarks>
    public static event EventHandler<EventArgs>? MissionChanged;

    /// <summary>
    /// Global instance of <see cref="MemoryGarden"/>.
    /// </summary>
    public static readonly MemoryGarden Global = new();

    /// <summary>
    /// Gets or sets the find options.
    /// </summary>
    /// <remarks>
    /// The <see cref="FindChanged"/> is invoked on value equality change.
    /// </remarks>
    public static FindOptions? Find
    {
        get { return _options; }

        set
        {
            if (!Equals(_options, value))
            {
                _options = value;
                _timer.Restart();
            }
        }
    }

    /// <summary>
    /// Gets or sets searching is under <see cref="MainMission"/> control.
    /// </summary>
    public static bool IsMissionSearch
    {
        get { return _isMissionSearch; }

        set
        {
            if (_isMissionSearch != value)
            {
                _isMissionSearch = value;

                _timer.Stop();
                _options = null;
                MissionChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    private static void TickHandler(object? _, EventArgs __)
    {
        _timer.Stop();
        FindChanged?.Invoke(null, EventArgs.Empty);
    }
}