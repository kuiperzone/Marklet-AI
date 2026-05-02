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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Carousels;

/// <summary>
/// Generic subclass of <see cref="CarouselPage"/> which as a base class intended to host controls with persistant
/// settings applicable to underlying settings of type T.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control itself. Rather, it is collection of controls to be shown as
/// "pages".
/// </remarks>
public abstract class CarouselPage<T> : CarouselPage where T : SettingsBase, new()
{
    private readonly DispatcherTimer _timer = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    public CarouselPage(T settings)
    {
        Settings = settings;
        _timer.Tick += TimerTickHandler;
    }

    /// <summary>
    /// Gets the settings instance provided on construction.
    /// </summary>
    public T Settings { get; }

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
    /// Update visual control value from given "settings".
    /// </summary>
    /// <remarks>
    /// Exposed for unit testing. Subclass to implement. Implementation to do nothing other than set child control data
    /// values using "settings", rather than <see cref="Settings"/>. It should not update IsEnabled states of controls,
    /// which should be done separately <see cref="EnableControls"/>.
    /// </remarks>
    public abstract void UpdateControls(T settings);

    /// <summary>
    /// Set enabled/disable of child controls separately from <see cref="UpdateControls"/>.
    /// </summary>
    /// <remarks>
    /// The implementation should do nothing other than set IsEnabled of controls based on control values themselves. It
    /// should not modify control values or change anything which may trigger <see cref="OnValueChanged"/>. Base
    /// implementation does nothing.
    /// </remarks>
    public virtual void EnableControls()
    {
    }

    /// <summary>
    /// Update "settings" from control data value.
    /// </summary>
    /// <remarks>
    /// Exposed for unit testing. Subclass to implement.
    /// </remarks>
    public abstract void UpdateSettings(T settings);

    /// <summary>
    /// Resets <see cref="Settings"/>, calls <see cref="UpdateControls"/> and invokes <see
    /// cref="SettingsBase.OnChanged(bool)"/>.
    /// </summary>
    public virtual void Reset()
    {
        const string NSpace = $"{nameof(CarouselPage<>)}.{nameof(Reset)}";
        Diag.WriteLine(NSpace, "Reset: " + Title);

        try
        {
            IsReseting = true;

            if (IsFluid)
            {
                Diag.WriteLine(NSpace, "Fluid");
                Settings.Reset();
                UpdateControls(Settings);
                EnableControls();

                Settings.OnChanged(false);
                WriteSettings(0);
                return;
            }

            Diag.WriteLine(NSpace, "Not fluid");
            UpdateControls(new T());
            EnableControls();
        }
        finally
        {
            IsReseting = false;
        }
    }

    /// <summary>
    /// Calls <see cref="UpdateControls"/> on activation.
    /// </summary>
    public override void Activate(bool fluid)
    {
        const string NSpace = $"{nameof(CarouselPage<>)}.{nameof(Activate)}";

        if (!IsActive)
        {
            // Becoming visible
            Diag.WriteLine(NSpace, "ACTIVATE: " + Title);
            UpdateControls(Settings);
            EnableControls();
        }

        base.Activate(fluid);
    }

    /// <summary>
    /// Ensures <see cref="WriteSettings"/> excutes when <see cref="IsPendingWrite"/> is true.
    /// </summary>
    public override void Deactivate()
    {
        const string NSpace = $"{nameof(CarouselPage<>)}.{nameof(Deactivate)}";
        Diag.WriteLine(NSpace, "DEACTIVATE: " + Title);

        if (IsPendingWrite)
        {
            Diag.WriteLine(NSpace, "Is pending");
            WriteSettings(0);
        }

        base.Deactivate();
    }

    /// <summary>
    /// Ensures <see cref="WriteSettings"/> excutes when <see cref="IsPendingWrite"/> is true.
    /// </summary>
    public override void Apply()
    {
        const string NSpace = $"{nameof(CarouselPage<>)}.{nameof(Apply)}";
        Diag.WriteLine(NSpace, "APPLY: " + IsActive);

        if (IsActive)
        {
            UpdateSettings(Settings);
            Settings.OnChanged(false);
            WriteSettings(0);
        }
    }

    /// <summary>
    /// Creates a new "reset" group with given title.
    /// </summary>
    /// <remarks>
    /// Clicking the button calls <see cref="Reset"/>.
    /// </remarks>
    protected PixieGroup CreateResetGroup(string? title = null)
    {
        var group = new PixieGroup();
        group.TopTitle = "Reset";

        var obj = new PixieCard();
        obj.RightButton.IsVisible = true;
        obj.RightButton.Classes.Add("regular");
        obj.RightButton.Content = "Reset " + Symbols.Replay;
        obj.Title = title ?? $"Reset {Title} Defaults";
        obj.RightButton.Click += (_, __) => Reset();

        group.Children.Add(obj);
        return group;
    }

    /// <summary>
    /// Informs the base class that a child control value has changed, invoking calls to <see
    /// cref="UpdateSettings(T)"/>, <see cref="EnableControls"/> and the <see cref="SettingsBase.OnChanged(bool)"/>
    /// event.
    /// </summary>
    /// <remarks>
    /// The method does nothing where <see cref="CarouselPage.IsActive"/> is false.
    /// </remarks>
    protected void OnValueChanged(bool force = false)
    {
        const string NSpace = $"{nameof(CarouselPage<>)}.{nameof(OnValueChanged)}";
        Diag.WriteLine(NSpace, "CHANGED: " + IsActive);

        if (IsActive)
        {
            if ((force || IsFluid) && !IsReseting)
            {
                Diag.WriteLine(NSpace, "Updating");
                UpdateSettings(Settings);

                Settings.OnChanged(false);
                WriteSettings();
            }

            // Important. This is where UpdateControls() becomes distinct from EnabledControls().
            EnableControls();
        }
    }

    /// <summary>
    /// Calls <see cref="SettingsBase.Write"/> after "delay" milliseconds.
    /// </summary>
    private void WriteSettings(int delay = 3000)
    {
        // Timer intended to prevent multiple
        // needless writes in response to key presses.
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
        const string NSpace = $"{nameof(CarouselPage<>)}.{nameof(TimerTickHandler)}";
        Diag.WriteLine(NSpace, "Timer tick: " + Title);
        WriteSettings(0);
    }

}