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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Custom GridSplitter.
/// </summary>
/// <remarks>
/// The class shares its StyleKey with <see cref="GridSplitter"/>.
/// </remarks>
public sealed class HighSplitter : GridSplitter
{
    private readonly DispatcherTimer _timer = new();
    private IBrush? _originalBackground;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public HighSplitter()
    {
        // Set (not static defaults)
        FocusAdorner = null;
        Background = Brushes.Transparent;

        _timer.Interval = TimeSpan.FromMilliseconds(250);
        _timer.Tick += TimerHandler;
    }

    /// <summary>
    /// Custom property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HighlightProperty =
        AvaloniaProperty.Register<HighSplitter, IBrush?>(nameof(Highlight), ChromeBrushes.BlueAccent);

    /// <summary>
    /// Gets or sets the highlight brush.
    /// </summary>
    public IBrush? Highlight
    {
        get { return GetValue(HighlightProperty); }
        set { SetValue(HighlightProperty, value); }
    }

    /// <summary>
    /// Gets whether currently highlighting.
    /// </summary>
    public bool IsHigh { get; private set; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(GridSplitter);

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (SetHighlight(true))
        {
            _timer.Start();
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        SetHighlight(true);

    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        if (!_timer.IsEnabled)
        {
            SetHighlight(false);
        }
    }

    private bool SetHighlight(bool value)
    {
        if (IsHigh != value)
        {
            IsHigh = value;

            if (value)
            {
                _originalBackground = Background;
                SetCurrentValue(BackgroundProperty, Highlight);
                return true;
            }

            SetCurrentValue(BackgroundProperty, _originalBackground);
            _originalBackground = null;
            return true;
        }

        return false;
    }

    private void TimerHandler(object? _, EventArgs __)
    {
        if (!IsPointerOver && _timer.IsEnabled)
        {
            SetHighlight(false);
            _timer.Stop();
        }
    }
}