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

using Avalonia.Controls;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using Avalonia.Input;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Provides the main input prompt along with related buttons and message attachment.
/// </summary>
public sealed class Prompter : Panel
{
    /// <summary>
    /// Gets the maximum <see cref="Text"/> length.
    /// </summary>
    public static readonly int MaxTextLength = Math.Min(64 * 1024, MemoryGarden.MaxContentLength);

    private readonly TextEditor _editor = new();
    private readonly PromptBar _bar = new();

    // Backing fields
    private bool _isAttachButtonVisible;
    private bool _isSelectAllButtonVisible;
    private IBrush? _disabledEditBackground;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Prompter()
    {
        Children.Add(_editor);

        _bar.ZIndex = int.MaxValue;
        Children.Add(_bar);

        _editor.FontSize = ChromeFonts.LargeFontSize;
        _editor.Background = Brushes.Transparent;
        _editor.MaxLength = MaxTextLength;
        _editor.TextWrapping = TextWrapping.Wrap;
        _editor.HasBackButton = false;
        _editor.HasCopyButton = false;
        _editor.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        _editor.AcceptsReturn = false;
        _editor.AlwaysAcceptReturnOnPaste = true;
        _editor.Submitted += SubmitClickHandler;
        _editor.TextChanged += EditorTextChangedHandler;

        _bar.Attach.IsVisible = _isAttachButtonVisible;

        _bar.SelectAll.IsVisible = _isSelectAllButtonVisible;
        _bar.SelectAll.Click += SelectAllClickHandler;
        _bar.Monospace.IsVisible = false;

        _bar.Copy.Click += CopyClickHandler;
        _bar.Paste.Click += PasteClickHandler;
        _bar.EditWindow.Click += EditWindowClickHandler;
        _bar.Submit.Click += SubmitClickHandler;

        _bar.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

        // Align values
        _disabledEditBackground = _editor.DisabledBackground;
    }

    /// <summary>
    /// Defines common key gesture.
    /// </summary>
    public static readonly KeyGesture AttachGesture = new(Key.A, KeyModifiers.Control | KeyModifiers.Shift);

    /// <summary>
    /// Defines common key gesture.
    /// </summary>
    public static readonly KeyGesture MonoGesture = new(Key.M, KeyModifiers.Control);

    /// <summary>
    /// Defines common key gesture.
    /// </summary>
    public static readonly KeyGesture EditWindowGesture = new(Key.F12);

    /// <summary>
    /// Defines the <see cref="SubmitClick"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> SubmitClickEvent =
        RoutedEvent.Register<Prompter, RoutedEventArgs>(nameof(SubmitClick), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="SubmitClick"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> TextChangedEvent =
        RoutedEvent.Register<Prompter, RoutedEventArgs>(nameof(TextChanged), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="EditWindowClick"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> EditWindowClickEvent =
        RoutedEvent.Register<Prompter, RoutedEventArgs>(nameof(EditWindowClick), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="IsAttachButtonVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<Prompter, bool> IsAttachButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<Prompter, bool>(nameof(IsAttachButtonVisible),
        o => o.IsAttachButtonVisible, (o, v) => o.IsAttachButtonVisible = v);

    /// <summary>
    /// Defines the <see cref="IsSelectAllButtonVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<Prompter, bool> IsSelectAllButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<Prompter, bool>(nameof(IsSelectAllButtonVisible),
        o => o.IsSelectAllButtonVisible, (o, v) => o.IsSelectAllButtonVisible = v);

    /// <summary>
    /// Defines the <see cref="DisabledEditBackground"/> property.
    /// </summary>
    public static readonly DirectProperty<Prompter, IBrush?> DisabledEditBackgroundProperty =
        AvaloniaProperty.RegisterDirect<Prompter, IBrush?>(nameof(DisabledEditBackground),
        o => o.DisabledEditBackground, (o, v) => o.DisabledEditBackground = v);

    /// <summary>
    /// Occurs when the user clicks the "Submit" or hits ENTER in the edit box.
    /// </summary>
    public event EventHandler<RoutedEventArgs> SubmitClick
    {
        add { AddHandler(SubmitClickEvent, value); }
        remove { RemoveHandler(SubmitClickEvent, value); }
    }

    /// <summary>
    /// Occurs when <see cref="Text"/> changes.
    /// </summary>
    public event EventHandler<RoutedEventArgs> TextChanged
    {
        add { AddHandler(TextChangedEvent, value); }
        remove { RemoveHandler(TextChangedEvent, value); }
    }

    /// <summary>
    /// Occurs when the user clicks the "edit window" button.
    /// </summary>
    public event EventHandler<RoutedEventArgs> EditWindowClick
    {
        add { AddHandler(EditWindowClickEvent, value); }
        remove { RemoveHandler(EditWindowClickEvent, value); }
    }

    /// <summary>
    /// Gets or sets whether the attachment(s) and attachment button is visible.
    /// </summary>
    public bool IsAttachButtonVisible
    {
        get { return _isAttachButtonVisible; }
        set { SetAndRaise(IsAttachButtonVisibleProperty, ref _isAttachButtonVisible, value); }
    }

    /// <summary>
    /// Gets or sets whether the select-all button is visible.
    /// </summary>
    /// <remarks>
    /// The default is false.
    /// </remarks>
    public bool IsSelectAllButtonVisible
    {
        get { return _isSelectAllButtonVisible; }
        set { SetAndRaise(IsSelectAllButtonVisibleProperty, ref _isSelectAllButtonVisible, value); }
    }

    /// <summary>
    /// Gets or sets the background editor when disabled.
    /// </summary>
    /// <remarks>
    /// The default is null.
    /// </remarks>
    public IBrush? DisabledEditBackground
    {
        get { return _disabledEditBackground; }
        set { SetAndRaise(DisabledEditBackgroundProperty, ref _disabledEditBackground, value); }
    }

    /// <summary>
    /// Gets or sets the editor text.
    /// </summary>
    public string? Text
    {
        get { return _editor.Text; }

        set
        {
            if (_editor.Text != value)
            {
                _editor.Text = value;
            }
        }
    }

    /// <summary>
    /// Calls focus on the editor.
    /// </summary>
    public void FocusEditor()
    {
        _editor.Focus();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;
        base.OnPropertyChanged(change);

        if (p == IsAttachButtonVisibleProperty)
        {
            _bar.Attach.IsVisible = change.GetNewValue<bool>();
            return;
        }

        if (p == IsSelectAllButtonVisibleProperty)
        {
            _bar.SelectAll.IsVisible = change.GetNewValue<bool>();
            return;
        }

        if (p == DisabledEditBackgroundProperty)
        {
            _editor.DisabledBackground = change.GetNewValue<IBrush?>();
            return;
        }

        if (p == IsEffectivelyEnabledProperty)
        {
            // Clear on disable
            if (!change.GetNewValue<bool>())
            {
                _editor.Clear();
            }

            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        // Key off symbol font size so if we
        // change this spacing will also change here.
        const double fs = ChromeFonts.SymbolFontSize;

        const double p0 = 2.0 * fs / 3.0;
        const double p1 = 8;
        _bar.Margin = new Thickness(p1, 0.0, p1, p1);

        // We also need rendered bar-height.
        var bh = _bar.Bounds.Height + p1;
        var ep = new Thickness(p0, p0, p0, bh + p0);

        // Buttons sit on top of Padding bottom area
        _editor.Padding = ep;

        // A little taller than one line
        _editor.MinHeight = ChromeFonts.LargeLineHeight + ep.Height();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(Prompter)}.{nameof(OnKeyDown)}";
        Diag.WriteLine(NSpace, $"Key: {e.Key}, {e.KeyModifiers}");

        base.OnKeyDown(e);
        _bar.HandleKeyGesture(e);
    }

    private void CopyClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.CopySelectedOrAll();
        _editor.Focus();
    }

    private void PasteClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.Paste();
        _editor.Focus();
    }

    private void SelectAllClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.SelectAll();
        _editor.Focus();
    }

    private void EditWindowClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        RaiseEvent(new RoutedEventArgs(EditWindowClickEvent, this));
        _editor.Focus();
    }

    private void SubmitClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.Focus();

        if (!string.IsNullOrWhiteSpace(Text))
        {
            RaiseEvent(new RoutedEventArgs(SubmitClickEvent, this));
        }
    }

    private void EditorTextChangedHandler(object? _, RoutedEventArgs __)
    {
        RaiseEvent(new RoutedEventArgs(TextChangedEvent, this));
    }

    private void BarBackgroundClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.Focus();
    }

}