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
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Shared;

/// <summary>
/// Non-generic variant of <see cref="DispatchCoalescer{T}"/> where the generic base type is <see cref="EventArgs"/>.
/// </summary>
public sealed class DispatchCoalescer : DispatchCoalescer<EventArgs>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public DispatchCoalescer()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DispatchCoalescer(DispatcherPriority priority)
        : base(priority)
    {
    }

    /// <summary>
    /// Overloads the base <see cref="DispatchCoalescer{T}.Post"/> with <see cref="EventArgs.Empty"/>.
    /// </summary>
    /// <returns></returns>
    public bool Post()
    {
        return Post(EventArgs.Empty);
    }
}

/// <summary>
/// Coalesces multiple calls to <see cref="Post"/> into a single event invocation in the UI thread.
/// </summary>
/// <remarks>
/// The class is thread-safe and may be used for inter-thread communication from a background thread to the UI (but not
/// the other way around).
/// </remarks>
public class DispatchCoalescer<T> where T : EventArgs
{
    private readonly Lock _syncObj = new();
    private T? _args;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DispatchCoalescer()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DispatchCoalescer(DispatcherPriority priority)
    {
        Priority = priority;
    }

    /// <summary>
    /// Gets the priority.
    /// </summary>
    public readonly DispatcherPriority Priority = DispatcherPriority.Normal;

    /// <summary>
    /// Occurs in respose to <see cref="Post"/>.
    /// </summary>
    public event EventHandler<T>? Posted;

    /// <summary>
    /// Gets whether <see cref="Posted"/> is waiting to be invoked.
    /// </summary>
    public bool IsPending
    {
        get { lock (_syncObj) { return _args != null; } }
    }

    /// <summary>
    /// Causes the <see cref="Posted"/> to be invoked with the given <see cref="EventArgs"/> instance.
    /// </summary>
    /// <remarks>
    /// Multiple calls to <see cref="Post"/> will cause <see cref="Posted"/> to be invoked once in the UI thread with
    /// the last instance of "e" supplied. When called, <see cref="IsPending"/> becomes true until <see cref="Posted"/>
    /// is invoked.
    /// </remarks>
    public bool Post(T e)
    {
        const string NSpace = $"{nameof(DispatchCoalescer)}.{nameof(Post)}";

        if (Swap(e) == null)
        {
            ConditionalDebug.WriteLine(NSpace, "Posting");
            Dispatcher.UIThread.Post(PostReceiver, Priority);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Cancels <see cref="IsPending"/> and returns true if cancelled.
    /// </summary>
    public bool Cancel()
    {
        return Swap(null) != null;
    }

    private void PostReceiver()
    {
        const string NSpace = $"{nameof(DispatchCoalescer)}.{nameof(PostReceiver)}";
        var args = Swap(null);

        if (args != null)
        {
            try
            {
                ConditionalDebug.WriteLine(NSpace, $"INVOKING");
                Posted?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                ConditionalDebug.WriteLine(NSpace, "EXCEPTION IN UI THREAD HANDLER");
                ConditionalDebug.WriteLine(NSpace, ex);
            }
        }
    }

    private T? Swap(T? e)
    {
        lock (_syncObj)
        {
            var rslt = _args;
            _args = e;
            return rslt;
        }

    }
}
