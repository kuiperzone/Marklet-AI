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
using Avalonia.Styling;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Extended <see cref="TextBox"/> which provides sophisticated keyboard handling.
/// </summary>
/// <remarks>
/// The class shares its StyleKey with <see cref="TextBox"/>. The instance as a single pixel border by default and adds
/// the "fixed-border" to <see cref="StyledElement.Classes"/>. This means that its border thickness does not change with
/// focus or pointer-over events. Removing it will make it behave as <see cref="TextBox"/>. The "fixed-background" class
/// may be added so that the control keeps its background when focused and pointer-over.
/// </remarks>
public sealed class TextEditor : TextBox
{
    private const int TabSize = 4;
    private readonly StackPanel _stack = new();
    private readonly LightButton _revealButton;
    private readonly LightButton _copyButton;
    private readonly LightButton _deleteButton;

    private bool _silentInnerChanging;
    private bool _hasCustomInnertRight;
    private int _pageOffset = 1;

    private readonly Style _fxBackgroundPointOver;
    private readonly Style _fxBackgroundFocus;
    private readonly Style _fxBorderPointOver;
    private readonly Style _fxBorderFocus;
    private readonly Style _fxBorderDisabled;

    static TextEditor()
    {
        BorderThicknessProperty.OverrideDefaultValue<TextEditor>(new(1.0));
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TextEditor()
    {
        _stack.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _stack.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        _stack.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

        _revealButton = NewButton(Symbols.Visibility);
        _revealButton.CanToggle = true;
        _revealButton.IsChecked = RevealPassword;
        _revealButton.Click += RevealClickHandler;

        _copyButton = NewButton(Symbols.ContentCopy);
        _copyButton.Click += CopyClickHandler;

        _deleteButton = NewButton(Symbols.Backspace);
        _deleteButton.Click += DeleteClickHandler;
        UpdateButtons();

        PastingFromClipboard += PastingFromClipboardHandler;

        // BACKGROUND
        _fxBackgroundPointOver = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-background")
            .Class(":pointerover")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());
        Styles.Add(_fxBackgroundPointOver);

        _fxBackgroundFocus = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-background")
            .Class(":focus")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());
        Styles.Add(_fxBackgroundFocus);

        SetFixedBackground(BackgroundProperty.GetDefaultValue(this));


        // BORDER
        _fxBorderPointOver = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-border")
            .Class(":pointerover")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());
        Styles.Add(_fxBorderPointOver);

        _fxBorderFocus = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-border")
            .Class(":focus")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());
        Styles.Add(_fxBorderFocus);

        _fxBorderDisabled = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-border")
            .Class(":disabled")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());
        Styles.Add(_fxBorderDisabled);

        SetFixedBorder(BorderThicknessProperty.GetDefaultValue(this));

        // Add by default
        Classes.Add("fixed-border");
    }

    /// <summary>
    /// Defines the <see cref="Submitted"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> SubmittedEvent =
        RoutedEvent.Register<TextEditor, RoutedEventArgs>(nameof(Submitted), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="HasBackButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasBackButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasBackButton), true);

    /// <summary>
    /// Defines the <see cref="HasCopyButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasCopyButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasCopyButton));

    /// <summary>
    /// Defines the <see cref="HasRevealButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasRevealButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasRevealButton));

    /// <summary>
    /// Defines the <see cref="AlwaysAcceptReturnOnPaste"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AlwaysAcceptReturnOnPasteProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(AlwaysAcceptReturnOnPaste));

    /// <summary>
    /// Occurs when the user hits ENTER when <see cref="TextBox.AcceptsReturn"/> is false, or "CTRL+ENTER" when <see
    /// cref="TextBox.AcceptsReturn"/> is true.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Submitted
    {
        add { AddHandler(SubmittedEvent, value); }
        remove { RemoveHandler(SubmittedEvent, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a backspace (delete all) button.
    /// </summary>
    /// <remarks>
    /// The <see cref="HasBackButton"/> is shown only when <see cref="TextBox.AcceptsReturn"/> is false. Default is
    /// true.
    /// </remarks>
    public bool HasBackButton
    {
        get { return GetValue(HasBackButtonProperty); }
        set { SetValue(HasBackButtonProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a "copy" button.
    /// </summary>
    /// <remarks>
    /// The <see cref="HasCopyButton"/> is shown only when <see cref="TextBox.AcceptsReturn"/> is false. Default is false.
    /// </remarks>
    public bool HasCopyButton
    {
        get { return GetValue(HasCopyButtonProperty); }
        set { SetValue(HasCopyButtonProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a "reveal password" button.
    /// </summary>
    /// <remarks>
    /// The <see cref="HasRevealButton"/> is shown only when <see cref="TextBox.AcceptsReturn"/> is false. Default is
    /// false.
    /// </remarks>
    public bool HasRevealButton
    {
        get { return GetValue(HasRevealButtonProperty); }
        set { SetValue(HasRevealButtonProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether to allow pasting of text with newlines where <see cref="TextBox.AcceptsReturn"/> is false.
    /// </summary>
    /// <remarks>
    /// The default for this value is false.
    /// </remarks>
    public bool AlwaysAcceptReturnOnPaste
    {
        get { return GetValue(AlwaysAcceptReturnOnPasteProperty); }
        set { SetValue(AlwaysAcceptReturnOnPasteProperty, value); }
    }

    /// <summary>
    /// Follow <see cref="TextBox"/> style.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(TextBox);

    /// <summary>
    /// Raises the <see cref="Submitted"/> event.
    /// </summary>
    public void OnSubmitted()
    {
        RaiseEvent(new RoutedEventArgs(SubmittedEvent, this));
    }

    /// <summary>
    /// Copies selected text to clipboard or, if selection is empty, copies all text instead.
    /// </summary>
    public async void CopySelectedOrAll()
    {
        var clippy = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clippy != null)
        {
            var text = SelectedText;

            if (string.IsNullOrEmpty(text))
            {
                text = Text;
            }

            if (!string.IsNullOrEmpty(text))
            {
                var e = new RoutedEventArgs(CopyingToClipboardEvent);
                RaiseEvent(e);

                if (!e.Handled)
                {
                    await clippy.SetTextAsync(text);
                }
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var p = change.Property;

        if (p == BackgroundProperty)
        {
            SetFixedBackground(change.GetNewValue<IBrush?>());
            return;
        }

        if (p == BorderThicknessProperty)
        {
            SetFixedBorder(change.GetNewValue<Thickness>());
            return;
        }

        if (p == InnerRightContentProperty)
        {
            if (!_silentInnerChanging)
            {
                _hasCustomInnertRight = change.NewValue != null;
                UpdateButtons();
            }

            return;
        }

        if (p == RevealPasswordProperty)
        {
            _revealButton.IsChecked = change.GetNewValue<bool>();
            return;
        }

        if (p == AcceptsReturnProperty ||
            p == HasRevealButtonProperty ||
            p == HasCopyButtonProperty ||
            p == HasBackButtonProperty)
        {
            UpdateButtons();
            return;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsFocused)
        {
            base.OnKeyDown(e);
            return;
        }

        try
        {
            PosDebug(e);

            // Reset but hold
            int offset = _pageOffset;
            _pageOffset = -1;

            switch (e.Key)
            {
                case Key.Enter:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && AcceptsReturn)
                    {
                        e.Handled = true;
                        OnSubmitted();
                        return;
                    }

                    if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) && !AcceptsReturn)
                    {
                        e.Handled = true;
                        OnSubmitted();
                        return;
                    }
                    break;
                case Key.Tab:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) && AcceptsTab)
                    {
                        if (ShiftTabLeft())
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) && AcceptsTab)
                    {
                        if (ShiftTabRight())
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    break;
                case Key.Home:
                    Home(e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                    e.Handled = true;
                    return;
                case Key.End:
                    End(e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                    e.Handled = true;
                    return;
                case Key.Up:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        _pageOffset = ShiftUp(offset);
                        e.Handled = true;
                        return;
                    }
                    break;
                case Key.Down:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        _pageOffset = ShiftDown(offset);
                        e.Handled = true;
                        return;
                    }
                    break;
                case Key.PageUp:
                    if (AcceptsReturn)
                    {
                        _pageOffset = PageUp(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset);
                        e.Handled = true;
                        return;
                    }
                    break;
                case Key.PageDown:
                    if (AcceptsReturn)
                    {
                        _pageOffset = PageDown(e.KeyModifiers.HasFlag(KeyModifiers.Shift), offset);
                        e.Handled = true;
                        return;
                    }
                    break;
                case Key.Escape:
                    if (SelectionStart != SelectionEnd && Text != null)
                    {
                        e.Handled = true;
                        int length = Text.Length;
                        SelectionStart = length;
                        SelectionEnd = length;
                        CaretIndex = length;
                        return;
                    }
                    break;
                case Key.Q:
                    if (e.KeyModifiers == KeyModifiers.Alt && HasRevealButton)
                    {
                        e.Handled = true;
                        _revealButton.IsChecked = !_revealButton.IsChecked;
                        return;
                    }
                    break;
            }

            base.OnKeyDown(e);
        }
        finally
        {
            PosDebug(null);
        }
    }

    private static bool IsStartOfLine(string text, int index)
    {
        if (index > text.Length)
        {
            return false;
        }

        if (index <= 0)
        {
            return true;
        }

        var c = text[index - 1];

        if (c == '\r')
        {
            return text[index] != '\n';
        }

        return c == '\n';
    }

    private static bool IsEndOfLine(string text, int index)
    {
        if (index >= text.Length)
        {
            return true;
        }

        if (index <= 0)
        {
            return false;
        }

        var c = text[index];

        if (c == '\n')
        {
            return text[index - 1] != '\r';
        }

        return c == '\r';
    }

    private static int GetStartOfLine(string text, int index, bool wordStop)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(GetStartOfLine)}";
        ConditionalDebug.WriteLine(NSpace, $"Index: {index}, wordStop: {wordStop}");

        int x = -1;

        for (int n = index; n > -1; --n)
        {
            if (wordStop && n < index && !text.IsSpaceOrTabAt(n, true))
            {
                x = n;
            }

            if (IsStartOfLine(text, n))
            {
                ConditionalDebug.WriteLine(NSpace, $"Start of line at n: {n}");
                ConditionalDebug.WriteLine(NSpace, $"WordStop at: {x}");
                return x > -1 ? x : n;
            }
        }

        return x > -1 ? x : 0;
    }

    private static int GetEndOfLine(string text, int index)
    {
        for (int n = index; n < text.Length; ++n)
        {
            if (IsEndOfLine(text, n))
            {
                return n;
            }
        }

        return text.Length;
    }

    private static bool Normalize(ref int s0, ref int s1)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(Normalize)}";
        ConditionalDebug.WriteLine(NSpace, $"Initial s0, s1: {s0}, {s1}");

        if (s0 != s1)
        {
            if (s0 > s1)
            {
                (s1, s0) = (s0, s1);
            }

            ConditionalDebug.WriteLine(NSpace, $"Normalized s0, s1: {s0}, {s1}");
            return true;
        }

        return false;
    }

    private static bool IsMultipleLinesBetween(string text, int index0, int index1)
    {
        for (int n = index0; n < index1; ++n)
        {
            if (text[n] == '\n')
            {
                return true;
            }
        }

        return false;
    }

    private static string Unindent(string text, out int d0, out int d1)
    {
        d0 = 0;
        int l0 = text.Length;

        if (l0 != 0)
        {
            if (text.StartsWith('\t'))
            {
                text = text.Substring(1);
            }
            else
            {
                for (int n = 0; n < TabSize; ++n)
                {
                    if (text.StartsWith(' '))
                    {
                        text = text.Substring(1);
                        continue;
                    }

                    break;
                }
            }

            d0 = text.Length - l0;
            text = text.Replace("\n\t", "\n");

            int l1 = l0;

            for (int n = 0; n < TabSize; ++n)
            {
                text = text.Replace("\n ", "\n");

                if (text.Length != l1)
                {
                    l1 = text.Length;
                    continue;
                }
            }
        }

        // Expect negative
        d1 = text.Length - l0;
        return text;
    }

    private static LightButton NewButton(string content)
    {
        var button = new LightButton();
        button.Focusable = false;
        button.Content = content;
        button.ContentPadding = default;
        button.FontSize = 14.0;
        button.MinWidth = 14.0 * 1.6;
        button.MaxWidth = 14.0 * 1.6;
        button.MinHeight = 14.0 * 1.6;
        button.MaxHeight = 14.0 * 1.6;
        button.Margin = new(0.0, 0.0, 2.0, 0.0);
        button.CornerRadius = new(4.0);
        return button;
    }

    private void SetFixedBorder(Thickness thickness)
    {
        var t = new Setter(Border.BorderThicknessProperty, thickness);

        _fxBorderPointOver.Setters.Clear();
        _fxBorderPointOver.Setters.Add(t);

        _fxBorderFocus.Setters.Clear();
        _fxBorderFocus.Setters.Add(t);

        _fxBorderDisabled.Setters.Clear();
        _fxBorderDisabled.Setters.Add(t);
    }

    private void SetFixedBackground(IBrush? background)
    {
        var setter = new Setter(Border.BackgroundProperty, background);

        _fxBackgroundPointOver.Setters.Clear();
        _fxBackgroundPointOver.Setters.Add(setter);

        _fxBackgroundFocus.Setters.Clear();
        _fxBackgroundFocus.Setters.Add(setter);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void PosDebug(KeyEventArgs? e)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(PosDebug)}";

        if (e != null)
        {
            ConditionalDebug.WriteLine(NSpace, $"START: {e.Key} + {e.KeyModifiers}");
            ConditionalDebug.WriteLine(NSpace, $"SelStart: {SelectionStart}, SelEnd: {SelectionEnd}, Caret: {CaretIndex}");
            return;
        }

        ConditionalDebug.WriteLine(NSpace, $"END: SelStart: {SelectionStart}, SelEnd: {SelectionEnd}, Caret: {CaretIndex}");
    }

    private int ShiftUp(int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(ShiftUp)}";
        ConditionalDebug.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IterateLineUp(Text, cx, ref offset);
        ConditionalDebug.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

        int anchor = (SelectionStart != SelectionEnd)
            ? ((cx == SelectionStart) ? SelectionEnd : SelectionStart) : cx;

        // Caret first, then set selection bounds
        CaretIndex = index;
        SelectionStart = Math.Min(anchor, index);
        SelectionEnd = Math.Max(anchor, index);

        if (SelectionEnd > SelectionStart && IsStartOfLine(Text, SelectionEnd))
        {
            SelectionEnd -= 1;
        }

        return offset;
    }

    private int ShiftDown(int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(ShiftDown)}";
        ConditionalDebug.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IterateLineDown(Text, cx, ref offset);
        ConditionalDebug.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

        var anchor = (SelectionStart != SelectionEnd)
            ? ((cx == SelectionStart) ? SelectionEnd : SelectionStart) : cx;

        // Caret first, then set selection bounds
        CaretIndex = index;
        SelectionStart = Math.Min(anchor, index);
        SelectionEnd = Math.Max(anchor, index);

        return offset;
    }

    private int PageUp(bool shift, int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(PageUp)}";
        ConditionalDebug.WriteLine(NSpace, $"shift: {shift}");
        ConditionalDebug.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IteratePageUp(Text, cx, ref offset);
        ConditionalDebug.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

        if (shift)
        {
            int anchor = (SelectionStart != SelectionEnd)
                ? ((cx == SelectionStart) ? SelectionEnd : SelectionStart) : cx;

            // Caret first, then set selection bounds
            CaretIndex = index;
            SelectionStart = Math.Min(anchor, index);
            SelectionEnd = Math.Max(anchor, index);

            if (SelectionEnd > SelectionStart && IsStartOfLine(Text, SelectionEnd))
            {
                SelectionEnd -= 1;
            }

            return offset;
        }

        SetNoShift(index);
        return offset;
    }

    private int PageDown(bool shift, int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(PageDown)}";
        ConditionalDebug.WriteLine(NSpace, $"shift: {shift}");
        ConditionalDebug.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IteratePageDown(Text, cx, ref offset);
        ConditionalDebug.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

        if (shift)
        {
            var anchor = (SelectionStart != SelectionEnd)
                ? ((cx == SelectionStart) ? SelectionEnd : SelectionStart) : cx;

            // Caret first, then set selection bounds
            CaretIndex = index;
            SelectionStart = Math.Min(anchor, index);
            SelectionEnd = Math.Max(anchor, index);
            return offset;
        }

        SetNoShift(index);
        return offset;
    }


    private void Home(bool shift)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(Home)}";
        ConditionalDebug.WriteLine(NSpace, $"shift: {shift}");

        var cx = CaretIndex;
        int index = GetStartOfLine(Text ?? "", cx, true);
        ConditionalDebug.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

        if (shift)
        {
            int anchor = (SelectionStart != SelectionEnd)
                ? ((cx == SelectionStart) ? SelectionEnd : SelectionStart) : cx;

            // Caret first, then set selection bounds
            CaretIndex = index;
            SelectionStart = Math.Min(anchor, index);
            SelectionEnd = Math.Max(anchor, index);
            return;
        }

        SetNoShift(index);
    }

    private void End(bool shift)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(End)}";
        ConditionalDebug.WriteLine(NSpace, $"shift: {shift}");

        var cx = CaretIndex;
        int index = GetEndOfLine(Text ?? "", cx);
        ConditionalDebug.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

        if (shift)
        {
            var anchor = (SelectionStart != SelectionEnd)
                ? ((cx == SelectionStart) ? SelectionEnd : SelectionStart) : cx;

            // Caret first, then set selection bounds
            CaretIndex = index;
            SelectionStart = Math.Min(anchor, index);
            SelectionEnd = Math.Max(anchor, index);
            return;
        }

        SetNoShift(index);
    }

    private bool ShiftTabLeft()
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(ShiftTabLeft)}";
        var text = Text;
        int s0 = SelectionStart;
        int s1 = SelectionEnd;

        if (!string.IsNullOrEmpty(text))
        {
            Normalize(ref s0, ref s1);
            int index0 = GetStartOfLine(text, s0, false);
            int index1 = GetEndOfLine(text, s1);
            ConditionalDebug.WriteLine(NSpace, $"index0, index1: {index0}, {index1}");

            var t0 = text.Substring(0, index0);
            var t1 = Unindent(text.Substring(index0, index1 - index0), out int d0, out int d1);
            var t2 = text.Substring(index1);
            ConditionalDebug.WriteLine(NSpace, $"d0: {d0}");
            ConditionalDebug.WriteLine(NSpace, $"d1: {d1}");

            Text = string.Concat(t0, t1, t2);

            SelectionStart = Math.Max(s0 + d0, index0);
            SelectionEnd = s1 + d1;
            return true;
        }

        return false;
    }

    private bool ShiftTabRight()
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(ShiftTabRight)}";

        var text = Text;
        int s0 = SelectionStart;
        int s1 = SelectionEnd;

        if (!string.IsNullOrEmpty(text) && Normalize(ref s0, ref s1))
        {
            int index0 = GetStartOfLine(text, s0, false);
            int index1 = GetEndOfLine(text, s1);
            ConditionalDebug.WriteLine(NSpace, $"index0, index1: {index0}, {index1}");

            if (index1 <= index0)
            {
                return false;
            }

            if ((index0 == s0 && index1 == s1) || IsMultipleLinesBetween(text, index0, index1))
            {
                var t0 = text.Substring(0, index0);
                var t1 = text.Substring(index0, index1 - index0);
                var t2 = text.Substring(index1);

                int l0 = text.Length;
                text = string.Concat(t0, "\t", t1.Replace("\n", "\n\t"), t2);

                int delta = text.Length - l0;
                ConditionalDebug.WriteLine(NSpace, $"delta: {delta}");

                Text = text;
                SelectionStart = index0 == s0 ? s0 : s0 + 1;
                SelectionEnd = s1 + delta;
                return true;
            }
        }

        return false;
    }

    private int IterateLineUp(string text, int startIndex, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IterateLineUp)}";
        ConditionalDebug.WriteLine(NSpace, $"startIndex: {startIndex}");

        return IterateUp(text, startIndex, 2, ref offset);
    }

    private int IteratePageUp(string text, int startIndex, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IteratePageUp)}";
        ConditionalDebug.WriteLine(NSpace, $"startIndex: {startIndex}");

        // Plus 1 deliberate
        return IterateUp(text, startIndex, GetPageHeight() + 1, ref offset);
    }

    private int IterateUp(string text, int startIndex, int height, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IterateUp)}";
        ConditionalDebug.WriteLine(NSpace, $"Height: {height}");
        ConditionalDebug.ThrowIfNegativeOrZero(height);

        startIndex = Math.Min(startIndex, text.Length - 1);

        for (int n = startIndex; n > -1; --n)
        {
            if (IsStartOfLine(text, n) && (--height == 0 || n == 0))
            {
                if (n == 0 && height > 0)
                {
                    offset = 0;
                    return 0;
                }

                if (offset < 0)
                {
                    offset = startIndex - GetStartOfLine(text, startIndex, false);
                }

                return Math.Min(n + offset, GetEndOfLine(text, n));
            }
        }

        if (offset < 0)
        {
            offset = 0;
        }

        return 0;
    }

    private int IterateLineDown(string text, int startIndex, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IterateLineDown)}";
        ConditionalDebug.WriteLine(NSpace, $"startIndex: {startIndex}");

        // Just 1 here
        return IterateDown(text, startIndex, 1, ref offset);
    }

    private int IteratePageDown(string text, int startIndex, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IteratePageDown)}";
        ConditionalDebug.WriteLine(NSpace, $"startIndex: {startIndex}");

        // Plus 1 deliberate
        return IterateDown(text, startIndex, GetPageHeight() + 1, ref offset);
    }

    private int IterateDown(string text, int startIndex, int height, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IterateDown)}";
        ConditionalDebug.WriteLine(NSpace, $"Height: {height}");
        ConditionalDebug.ThrowIfNegativeOrZero(height);

        for (int n = startIndex; n < text.Length; ++n)
        {
            bool eol = IsEndOfLine(text, n);
            bool last = n == text.Length - 1;

            // Skip first eol if we start on newline
            if (eol && (n != startIndex || last) && (--height == 0 || last))
            {
                if (n == text.Length - 1 && height > 0)
                {
                    offset = text.Length;
                    return text.Length;
                }

                if (offset < 0)
                {
                    offset = startIndex - GetStartOfLine(text, startIndex, false);
                }

                return Math.Min(n + offset, GetEndOfLine(text, n));
            }
        }

        if (offset < 0)
        {
            offset = text.Length;
        }

        return text.Length;
    }

    private int GetPageHeight()
    {
        // Constant will work for most fonts
        const double MultF = 1.2;

        double lineHeight = LineHeight;

        if (!double.IsFinite(lineHeight) || LineHeight <= 0)
        {
            lineHeight = FontSize * MultF;

            if (!double.IsFinite(lineHeight) || LineHeight <= 0)
            {
                lineHeight = FontSizeProperty.GetDefaultValue(this) * MultF;
            }
        }

        double pad = Padding.Top + Padding.Bottom;
        return (int)Math.Max(1.0, (Bounds.Height - pad) / lineHeight);
    }

    private void UpdateButtons()
    {
        if (_hasCustomInnertRight)
        {
            return;
        }

        if (AcceptsReturn)
        {
            SetSilentInnerRight(null);
            return;
        }

        var childs = _stack.Children;

        // 1. REVEAL
        if (HasRevealButton)
        {
            if (!childs.Contains(_revealButton))
            {
                childs.Insert(0, _revealButton);
            }
        }
        else
        {
            childs.Remove(_revealButton);
        }

        // 2. COPY
        if (HasCopyButton)
        {
            if (!childs.Contains(_copyButton))
            {
                if (childs.Count == 0)
                {
                    childs.Add(_copyButton);
                }
                else
                {
                    childs.Insert(1, _copyButton);
                }
            }
        }
        else
        {
            childs.Remove(_copyButton);
        }

        // 3. DELETE
        if (HasBackButton)
        {
            if (!childs.Contains(_deleteButton))
            {
                childs.Add(_deleteButton);
            }
        }
        else
        {
            childs.Remove(_deleteButton);
        }

        // STACK
        SetSilentInnerRight(childs.Count != 0 ? _stack : null);
    }

    private void SetNoShift(int index)
    {
        CaretIndex = index;
        SelectionStart = index;
        SelectionEnd = index;
    }

    private void SetSilentInnerRight(object? obj)
    {
        try
        {
            _silentInnerChanging = true;
            InnerRightContent = obj;
        }
        finally
        {
            _silentInnerChanging = false;
        }
    }

    private void RevealClickHandler(object? _, EventArgs __)
    {
        RevealPassword = _revealButton.IsChecked;
        Focus();
    }

    private void CopyClickHandler(object? _, EventArgs __)
    {
        CopySelectedOrAll();
    }

    private void DeleteClickHandler(object? _, EventArgs __)
    {
        Clear();
        Focus();
    }

    private async void PastingFromClipboardHandler(object? _, RoutedEventArgs e)
    {
        if (!AcceptsReturn && AlwaysAcceptReturnOnPaste)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

            if (clipboard == null)
            {
                return;
            }

            // Must set handled first due to async
            e.Handled = true;
            var data = await clipboard.TryGetDataAsync();

            if (data != null)
            {
                var clipText = await data.TryGetTextAsync();

                if (!string.IsNullOrEmpty(clipText))
                {
                    var text = Text ?? "";
                    int startPos = Math.Clamp(SelectionStart, 0, text.Length);
                    int endPos = Math.Clamp(SelectionEnd, 0, text.Length);

                    bool reversed = false;

                    if (startPos > endPos)
                    {
                        reversed = true;
                        (startPos, endPos) = (endPos, startPos);
                    }

                    // It appears CoerceText() takes an undo snapshot but does nothing else
                    Text = CoerceText(string.Concat(text.Substring(0, startPos), clipText, text.Substring(endPos)));

                    if (reversed)
                    {
                        SelectionEnd = startPos;
                        SelectionStart = startPos + clipText.Length;
                    }
                    else
                    {
                        SelectionStart = startPos;
                        SelectionEnd = startPos + clipText.Length;
                    }
                }
            }
        }
    }

}