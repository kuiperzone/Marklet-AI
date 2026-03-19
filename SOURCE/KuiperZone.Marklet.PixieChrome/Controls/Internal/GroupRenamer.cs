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

using Avalonia;
using Avalonia.Input;
using KuiperZone.Marklet.Tooling;
using Avalonia.Threading;
using Avalonia.Controls;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Allow UI driven renaming of <see cref="PixieCard"/> within <see cref="PixieGroup"/>.
/// </summary>
internal sealed class GroupRenamer : PixieEditor
{
    private IInputElement? _focused;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">minLength less than 0, or less than max</exception>
    public GroupRenamer(PixieCard source, int maxLength, int minLength = 1)
    {
        const string NSpace = $"{nameof(GroupRenamer)}.constructor";

        ConditionalDebug.WriteLine(NSpace, source.Title);
        ArgumentNullException.ThrowIfNull(source.Group, nameof(source));
        ArgumentOutOfRangeException.ThrowIfNegative(minLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minLength, maxLength);

        Source = source;
        Header = source.Header;
        Footer = source.Footer;
        LeftSymbol = source.LeftSymbol;
        RightSymbol = source.RightSymbol;
        CornerRadius = source.CornerRadius;
        MinHeight = source.Bounds.Height;

        if (source.IsChecked)
        {
            Background = PixieCard.CheckedBrush;
        }

        Subject.Text = source.Title;
        Subject.MinLength = minLength;
        Subject.MaxLength = maxLength;

        Subject.MinWidth = 0.0;
        Subject.MaxWidth = double.PositiveInfinity;
        Subject.Width = double.NaN;
        Subject.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        Subject.SelectAll();

        Subject.LostFocus += (_, __) => Discard();
        Subject.Submitted += SubmittedHandler;
        Subject.KeyDown += KeyDownHandler;
    }

    /// <summary>
    /// Occurs when user hits enter.
    /// </summary>
    /// <remarks>
    /// The sender is the "source" given on construction and not <see cref="GroupRenamer"/>.
    /// </remarks>
    public event EventHandler<SubmittedEventArgs>? Renaming;

    /// <summary>
    /// Gets the source instance.
    /// </summary>
    public PixieCard? Source { get; private set; }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();

        Source?.IsVisible = false;

        // Must be delayed
        Dispatcher.UIThread.Post(() => { Subject.Focus(); });
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Discard();
    }

    private void Discard()
    {
        const string NSpace = $"{nameof(GroupRenamer)}.{nameof(Discard)}";

        if (Source != null)
        {
            ConditionalDebug.WriteLine(NSpace, "Discarding");
            Source.IsVisible = true;
            Source.Group?.Remove(this);
            Source = null;

            _focused?.Focus();
        }
    }

    private void SubmittedHandler(object? _, SubmittedEventArgs e)
    {
        const string NSpace = $"{nameof(GroupRenamer)}.{nameof(SubmittedHandler)}";


        if (Source != null)
        {
            var t = Source;
            Renaming?.Invoke(t, e);
            ConditionalDebug.WriteLine(NSpace, "Handled: " + e.Handled);
            ConditionalDebug.WriteLine(NSpace, "Rejected: " + e.IsRejected);

            if (!e.IsRejected)
            {
                t.Title = e.NewText;
                Discard();
            }
        }
    }

    private void KeyDownHandler(object? _, KeyEventArgs e)
    {
        const string NSpace = $"{nameof(GroupRenamer)}.{nameof(KeyDownHandler)}";

        if (e.Key == Key.Escape)
        {
            ConditionalDebug.WriteLine(NSpace, "Escape key");
            Discard();
        }
    }

}