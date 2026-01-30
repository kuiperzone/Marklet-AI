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

using Avalonia.Threading;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages;

/// <summary>
/// Generic subclass of <see cref="CarouselPage"/> which itself serves as a base class intended host setting controls
/// applicable to underlying settings of type T.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control. Rather instances of this type are shown in <see
/// cref="CarouselWindow"/>.
/// </remarks>
public abstract class SettingsCarouselPage<T> : CarouselPage where T : SettingsBase
{
    private readonly DispatcherTimer _timer = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    public SettingsCarouselPage(T settings)
    {
        Settings = settings;
        _timer.Tick += TimerTickHandler;
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    protected SettingsCarouselPage(T settings, CarouselPage other)
        : base(other)
    {
        Settings = settings;
        _timer.Tick += TimerTickHandler;
    }

    /// <summary>
    /// Gets the settings instance provided on construction.
    /// </summary>
    public T Settings { get; }

    /// <summary>
    /// Gets whether changes to the controls are written to file (may be set false for unit test).
    /// </summary>
    public bool IsChangeWritable { get; set; } = true;

    /// <summary>
    /// Gets whether <see cref="WriteSettings"/> was called with a delay that is waiting to be written.
    /// </summary>
    public bool IsPendingWrite
    {
        get { return _timer.IsEnabled; }
    }

    /// <summary>
    /// Gets flag which is raised during the <see cref="Reset"/> operation.
    /// </summary>
    protected bool IsReseting { get; private set; }

    /// <summary>
    /// Updates visual control value from given "settings".
    /// </summary>
    /// <remarks>
    /// Exposed for unit testing. Subclass to implement.
    /// </remarks>
    public abstract void UpdateControls(T settings);

    /// <summary>
    /// Updates "settings" from control states.
    /// </summary>
    /// <remarks>
    /// Exposed for unit testing. Subclass to implement.
    /// </remarks>
    public abstract void UpdateSettings(T settings);

    /// <summary>
    /// Resets <see cref="Settings"/>, calls <see cref="UpdateControls"/> and invokes <see
    /// cref="SettingsBase.OnChanged(bool)"/>.
    /// </summary>
    public void Reset()
    {
        try
        {
            IsReseting = true;
            Settings.Reset();
            UpdateControls(Settings);
            Settings.OnChanged(false);
            WriteSettings(0);
        }
        finally
        {
            IsReseting = false;
        }
    }

    /// <summary>
    /// Ensures <see cref="WriteSettings"/> excutes when <see cref="IsPendingWrite"/> is true.
    /// </summary>
    public override void OnClosed()
    {
        if (IsPendingWrite)
        {
            WriteSettings(0);
        }
    }

    /// <summary>
    /// Update enabled/disable control states separately from <see cref="UpdateControls"/>.
    /// </summary>
    /// <remarks>
    /// Base method does nothing.
    /// </remarks>
    protected virtual void UpdateControlEnabledStates()
    {
    }

    /// <summary>
    /// Creates a new "reset" group with given title.
    /// </summary>
    protected PixieGroup NewResetGroup(string title)
    {
        var group = new PixieGroup();
        group.TopTitle = "Reset";

        var obj = new PixieButton();
        obj.RightButton.IsVisible = true;
        obj.RightButton.Classes.Add("regular-background");
        obj.RightButton.Content = "Reset";
        obj.Title = title;
        obj.RightButton.Click += (_, __) => Reset();

        group.Children.Add(obj);
        return group;
    }

    /// <summary>
    /// Handles control value changes, calling <see cref="UpdateSettings(T)"/>, <see cref="UpdateControlEnabledStates"/>
    /// and invoking <see cref="SettingsBase.OnChanged(bool)"/>.
    /// </summary>
    protected void ControlValueChangedHandler(object? _, EventArgs __)
    {
        if (!IsReseting)
        {
            UpdateSettings(Settings);
            UpdateControlEnabledStates();
            Settings.OnChanged(false);
            WriteSettings();
        }
    }

    /// <summary>
    /// Calls <see cref="SettingsBase.Write"/> after "delay" milliseconds.
    /// </summary>
    private void WriteSettings(int delay = 3000)
    {
        // This is intended to prevent multiple needless writes in response to key presses.
        _timer.Stop();

        if (delay <= 0)
        {
            Settings.Write();
            return;
        }

        _timer.Interval = TimeSpan.FromMilliseconds(delay);
        _timer.Start();
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        _timer.Stop();
        WriteSettings();
    }

}