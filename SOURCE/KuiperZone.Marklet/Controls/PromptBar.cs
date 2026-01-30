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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using Avalonia.Input;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Subclass of <see cref="MarkView"/> for use by <see cref="ContentViewer"/>.
/// </summary>
public sealed class PromptBar : Border
{
    private readonly static ChromeStyling Styling = ChromeStyling.Global;
    private readonly Grid _grid = new();
    private readonly LightBar _bar = new();
    private readonly LightButton _submit = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    public PromptBar()
    {
        const int BarColumn = 0;
        // const int AttachColumn = 1;
        const int SubmitColumn = 2;

        Child = _grid;

        _grid.RowDefinitions.Add(new(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new(GridLength.Star));
        _grid.ColumnDefinitions.Add(new(GridLength.Auto));

        Grid.SetColumn(_bar, BarColumn);
        _bar.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _grid.Children.Add(_bar);

        Attach = _bar.Add(Symbols.AttachFile, "Attach");
        Attach.Gesture = new(Key.A, KeyModifiers.Control | KeyModifiers.Shift);

        Copy = _bar.Add(Symbols.ContentCopy, "Copy");
        Copy.Gesture = new(Key.C, KeyModifiers.Control);

        Paste = _bar.Add(Symbols.ContentPaste, "Paste");
        Paste.Gesture = new(Key.V, KeyModifiers.Control);

        SelectAll = _bar.Add(Symbols.SelectAll, "Select All");
        SelectAll.Gesture = new(Key.A, KeyModifiers.Control);

        Monospace = _bar.Add(Symbols.Abc, "Monospace");
        Monospace.Gesture = new(Key.M, KeyModifiers.Control);

        EditWindow = _bar.Add(Symbols.OpenInNew, "Edit in window");
        EditWindow.Gesture = new(Key.F12);

        Submit = _submit;
        Grid.SetColumn(_submit, SubmitColumn);
        _submit.Content = Symbols.Send;
        _submit.Tip = "Submit";
        _submit.Foreground = ChromeBrushes.White;
        _submit.IsChecked = true;
        _submit.Gesture = new(Key.Enter);
        _submit.Focusable = false;
        _submit.MinWidth = ChromeFonts.SymbolFontSize * 3.0;
        _submit.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

        _grid.Children.Add(_submit);

        // Prevent button clicks taking focus for current control.
        foreach (var item in _bar.Buttons)
        {
            item.Focusable = false;
        }
    }

    /// <summary>
    /// Gets the "attach" button.
    /// </summary>
    public readonly LightButton Attach;

    /// <summary>
    /// Gets the "copy" button.
    /// </summary>
    public readonly LightButton Copy;

    /// <summary>
    /// Gets the "attach" button.
    /// </summary>
    public readonly LightButton Paste;

    /// <summary>
    /// Gets the "select all" button.
    /// </summary>
    public readonly LightButton SelectAll;

    /// <summary>
    /// Gets the "monospace" button.
    /// </summary>
    public readonly LightButton Monospace;

    /// <summary>
    /// Gets the "edit window" button.
    /// </summary>
    public readonly LightButton EditWindow;

    /// <summary>
    /// Gets the "submit" button.
    /// </summary>
    public readonly LightButton Submit;

    /// <summary>
    /// Handles button key gestures.
    /// </summary>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        if (_bar.HandleKeyGesture(e))
        {
            return true;
        }

        if (_submit.HandleKeyGesture(e))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StylingChangedHandler(null, EventArgs.Empty);
        Styling.StylingChanged += StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
    }

    private void StylingChangedHandler(object? _, EventArgs __)
    {
        _submit.Background = Styling.AccentBrush;
    }

}