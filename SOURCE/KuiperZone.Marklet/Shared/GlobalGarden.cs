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
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Provides a global instance of <see cref="MemoryGarden"/>
/// </summary>
/// <remarks>
/// Provides static search state needed for intercommunication between visual components.
/// </remarks>
public static class GlobalGarden
{
    // Short delay
    private static readonly DispatcherTimer _timer = new(TimeSpan.FromMilliseconds(200), DispatcherPriority.Default, TickHandler);
    private static SearchOptions? _options;
    private static bool _isMissionSearch;

    /// <summary>
    /// Occurs when <see cref="Search"/> change.
    /// </summary>
    /// <remarks>
    /// Note that the handler is global. The sender is null.
    /// </remarks>
    public static event EventHandler<EventArgs>? SearchChanged;

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
    /// The <see cref="SearchChanged"/> is invoked on value equality change.
    /// </remarks>
    public static SearchOptions? Search
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
        SearchChanged?.Invoke(null, EventArgs.Empty);
    }
}