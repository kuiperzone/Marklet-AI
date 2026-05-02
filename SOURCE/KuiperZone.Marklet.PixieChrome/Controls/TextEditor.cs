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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Extended <see cref="TextBox"/> which provides sophisticated keyboard handling.
/// </summary>
/// <remarks>
/// The class shares its StyleKey with <see cref="TextBox"/>. The instance has a single pixel border by default and adds
/// the "fixed-border" to <see cref="StyledElement.Classes"/>. This means that its border thickness does not change with
/// focus or pointer-over events. Removing it will make it behave as <see cref="TextBox"/>. The "fixed-background" class
/// may be added so that the control keeps its background when focused and pointer-over.
/// </remarks>
public class TextEditor : TextBox
{
    private const int TabSize = 4;
    private readonly StackPanel _stack = new();
    private readonly LightButton _backButton;
    private LightButton? _revealButton;
    private LightButton? _copyButton;
    private LightButton? _caseButton;
    private LightButton? _wordButton;

    private bool _silentInnerChanging;
    private bool _hasCustomInnertRight;
    private int _pageOffset = 1;
    private string? _oldText;

    private Style? _fxFixedBackground;
    private Style? _fxDisabledBackground;
    private Style? _fxBorder;

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

        _backButton = CreateButton(Symbols.Backspace, DeleteClickHandler, false);
        UpdateButtons();

        PastingFromClipboard += PastingFromClipboardHandler;

        SetFixedBackground(BackgroundProperty.GetDefaultValue(this));
        SetFixedBorder(BorderThicknessProperty.GetDefaultValue(this));

        // Add by default
        Classes.Add("fixed-border");
    }

    /// <summary>
    /// Defines the <see cref="Submitted"/> event.
    /// </summary>
    public static readonly RoutedEvent<SubmittedEventArgs> SubmittedEvent =
        RoutedEvent.Register<TextEditor, SubmittedEventArgs>(nameof(Submitted), RoutingStrategies.Direct);

    /// <summary>
    /// Defines the <see cref="MinLength"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MinLengthProperty =
        AvaloniaProperty.Register<TextEditor, int>(nameof(MinLength));

    /// <summary>
    /// Defines the <see cref="HasRevealButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasRevealButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasRevealButton));

    /// <summary>
    /// Defines the <see cref="HasCopyButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasCopyButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasCopyButton));

    /// <summary>
    /// Defines the <see cref="HasMatchCaseButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasMatchCaseButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasMatchCaseButton));

    /// <summary>
    /// Defines the <see cref="HasMatchWordButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasMatchWordButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasMatchWordButton));

    /// <summary>
    /// Defines the <see cref="HasBackButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasBackButtonProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(HasBackButton), true);

    /// <summary>
    /// Defines the <see cref="AlwaysAcceptReturnOnPaste"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AlwaysAcceptReturnOnPasteProperty =
        AvaloniaProperty.Register<TextEditor, bool>(nameof(AlwaysAcceptReturnOnPaste));

    /// <summary>
    /// Defines the <see cref="DisabledBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> DisabledBackgroundProperty =
        AvaloniaProperty.Register<TextEditor, IBrush?>(nameof(DisabledBackground));

    /// <summary>
    /// Occurs when the user hits ENTER when <see cref="TextBox.AcceptsReturn"/> is false, or "CTRL+ENTER" when <see
    /// cref="TextBox.AcceptsReturn"/> is true.
    /// </summary>
    public event EventHandler<SubmittedEventArgs> Submitted
    {
        add { AddHandler(SubmittedEvent, value); }
        remove { RemoveHandler(SubmittedEvent, value); }
    }

    /// <summary>
    /// Gets or sets the minimum text length.
    /// </summary>
    /// <remarks>
    /// The <see cref="Submitted"/> event will not fire unless <see cref="TextLength"/> equals or exceeds this value.
    /// The value should be equal or less than <see cref="TextBox.MaxLength"/>. The default is 0.
    /// </remarks>
    public int MinLength
    {
        get { return GetValue(MinLengthProperty); }
        set { SetValue(MinLengthProperty, value); }
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
    /// Gets or sets whether to show a checkable "match case" button.
    /// </summary>
    /// <remarks>
    /// Default is false. Note that clicking the "case" button invokes the <see cref="SubmittedEvent"/>.
    /// </remarks>
    public bool HasMatchCaseButton
    {
        get { return GetValue(HasMatchCaseButtonProperty); }
        set { SetValue(HasMatchCaseButtonProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether to show a checkable "match word" button.
    /// </summary>
    /// <remarks>
    /// Default is false. Note that clicking the "case" button invokes the <see cref="SubmittedEvent"/>.
    /// </remarks>
    public bool HasMatchWordButton
    {
        get { return GetValue(HasMatchWordButtonProperty); }
        set { SetValue(HasMatchWordButtonProperty, value); }
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
    /// Gets or sets the background editor when disabled.
    /// </summary>
    /// <remarks>
    /// The default is null which implies to use the theme color.
    /// </remarks>
    public IBrush? DisabledBackground
    {
        get { return GetValue(DisabledBackgroundProperty); }
        set { SetValue(DisabledBackgroundProperty, value); }
    }

    /// <summary>
    /// Gets the text length.
    /// </summary>
    public int TextLength
    {
        get { return Text?.Length ?? 0; }
    }

    /// <summary>
    /// Gets whether the "match case" button is checked.
    /// </summary>
    /// <remarks>
    /// The <see cref="HasMatchCaseButton"/> property must be true otherwise the result of this is always false. Setting
    /// when <see cref="HasMatchCaseButton"/> is false does nothing. Setting programmatically does not trigger an event.
    /// </remarks>
    public bool IsMatchCaseChecked
    {
        get { return _caseButton?.IsChecked == true; }
        set { _caseButton?.IsChecked = value; }
    }

    /// <summary>
    /// Gets whether the "match word" button is checked.
    /// </summary>
    /// <remarks>
    /// The <see cref="HasMatchWordButton"/> property must be true otherwise the result of this is always false. Setting
    /// when <see cref="HasMatchWordButton"/> is false does nothing. Setting programmatically does not trigger an event.
    /// </remarks>
    public bool IsMatchWordChecked
    {
        get { return _wordButton?.IsChecked == true; }
        set { _wordButton?.IsChecked = value; }
    }

    /// <summary>
    /// Follow <see cref="TextBox"/> style.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(TextBox);

    /// <summary>
    /// Raises the <see cref="Submitted"/> event and returns the event result.
    /// </summary>
    /// <remarks>
    /// The call does nothing if <see cref="TextLength"/> is less than <see cref="MinLength"/> and returns null.
    /// </remarks>
    public SubmittedEventArgs? OnSubmitted()
    {
        if (TextLength >= MinLength)
        {
            var e = new SubmittedEventArgs(SubmittedEvent, this, _oldText, Text);
            RaiseEvent(e);
            return e;
        }

        return null;
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
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _oldText = Text;
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

        if (p == DisabledBackgroundProperty)
        {
            SetDisabledBackground(change.GetNewValue<IBrush?>());
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
            _revealButton?.IsChecked = change.GetNewValue<bool>();
            return;
        }

        if (p == AcceptsReturnProperty ||
            p == HasRevealButtonProperty ||
            p == HasCopyButtonProperty ||
            p == HasMatchCaseButtonProperty ||
            p == HasMatchWordButtonProperty ||
            p == HasBackButtonProperty)
        {
            UpdateButtons();
            return;
        }

        if (p == IsReadOnlyProperty)
        {
            var enabled = !change.GetNewValue<bool>();
            _backButton.IsEnabled = enabled;
            _wordButton?.IsEnabled = enabled;
            _caseButton?.IsEnabled = enabled;
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
                    if (!AcceptsReturn || (AcceptsReturn && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
                    {
                        var es = OnSubmitted();

                        if (es != null)
                        {
                            e.Handled |= es.Handled;

                            if (es.IsRejected)
                            {
                                SetCurrentValue(ForegroundProperty, ChromeBrushes.WarningBrush);
                                DispatcherTimer.RunOnce(() => ClearValue(ForegroundProperty), TimeSpan.FromMilliseconds(1000));
                            }
                            else
                            {
                                ClearValue(ForegroundProperty);
                            }
                        }

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

                        if (_revealButton != null)
                        {
                            _revealButton.IsChecked = !_revealButton.IsChecked;
                        }
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
        Diag.WriteLine(NSpace, $"Index: {index}, wordStop: {wordStop}");

        int x = -1;

        for (int n = index; n > -1; --n)
        {
            if (wordStop && n < index && !text.IsSpaceOrTabAt(n, true))
            {
                x = n;
            }

            if (IsStartOfLine(text, n))
            {
                Diag.WriteLine(NSpace, $"Start of line at n: {n}");
                Diag.WriteLine(NSpace, $"WordStop at: {x}");
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
        Diag.WriteLine(NSpace, $"Initial s0, s1: {s0}, {s1}");

        if (s0 != s1)
        {
            if (s0 > s1)
            {
                (s1, s0) = (s0, s1);
            }

            Diag.WriteLine(NSpace, $"Normalized s0, s1: {s0}, {s1}");
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

    private static LightButton CreateButton(string content, EventHandler<RoutedEventArgs> click, bool canToggle, bool isChecked = false)
    {
        var b = new LightButton();
        b.Classes.Add("accent-checked");
        b.Focusable = false;
        b.Content = content;
        b.ContentPadding = default;
        b.FontSize = ChromeFonts.DefaultFontSize;
        b.MinWidth = 14.0 * 1.6;
        b.MaxWidth = 14.0 * 1.6;
        b.MinHeight = 0.0;
        b.MaxHeight = 14.0 * 1.6;
        b.Margin = new(0.0, 0.0, ChromeSizes.SmallPx, 0.0);
        b.CornerRadius = new(4.0);

        b.CanToggle = canToggle;
        b.IsChecked = isChecked;
        b.Click += click;
        return b;
    }

    private void SetFixedBackground(IBrush? background, bool reorder = true)
    {
        if (_fxFixedBackground != null)
        {
            Styles.Remove(_fxFixedBackground);
        }

        _fxFixedBackground = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-background")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());

        _fxFixedBackground.Setters.Add(new Setter(Border.BackgroundProperty, background));
        Styles.Add(_fxFixedBackground);

        if (reorder)
        {
            SetDisabledBackground(DisabledBackground, false);
        }
    }

    private void SetDisabledBackground(IBrush? background, bool reorder = true)
    {
        if (_fxDisabledBackground != null)
        {
            Styles.Remove(_fxDisabledBackground);
        }

        if (background != null)
        {
            if (reorder)
            {
                SetFixedBackground(Background, false);
            }

            _fxDisabledBackground = new Style(s =>
                s.OfType<TextBox>()
                .Class(":disabled")
                .Template()
                .Name("PART_BorderElement")
                .OfType<Border>());

            _fxDisabledBackground.Setters.Add(new Setter(Border.BackgroundProperty, background));
            Styles.Add(_fxDisabledBackground);
        }
    }

    private void SetFixedBorder(Thickness thickness)
    {
        if (_fxBorder != null)
        {
            Styles.Remove(_fxBorder);
        }

        _fxBorder = new Style(s =>
            s.OfType<TextBox>()
            .Class("fixed-border")
            .Template()
            .Name("PART_BorderElement")
            .OfType<Border>());

        _fxBorder.Setters.Add(new Setter(Border.BorderThicknessProperty, thickness));
        Styles.Add(_fxBorder);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void PosDebug(KeyEventArgs? e)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(PosDebug)}";

        if (e != null)
        {
            Diag.WriteLine(NSpace, $"START: {e.Key} + {e.KeyModifiers}");
            Diag.WriteLine(NSpace, $"SelStart: {SelectionStart}, SelEnd: {SelectionEnd}, Caret: {CaretIndex}");
            return;
        }

        Diag.WriteLine(NSpace, $"END: SelStart: {SelectionStart}, SelEnd: {SelectionEnd}, Caret: {CaretIndex}");
    }

    private int ShiftUp(int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(ShiftUp)}";
        Diag.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IterateLineUp(Text, cx, ref offset);
        Diag.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

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
        Diag.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IterateLineDown(Text, cx, ref offset);
        Diag.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

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
        Diag.WriteLine(NSpace, $"shift: {shift}");
        Diag.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IteratePageUp(Text, cx, ref offset);
        Diag.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

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
        Diag.WriteLine(NSpace, $"shift: {shift}");
        Diag.WriteLine(NSpace, $"offset: {offset}");

        if (string.IsNullOrEmpty(Text))
        {
            return -1;
        }

        var cx = CaretIndex;
        int index = IteratePageDown(Text, cx, ref offset);
        Diag.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

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
        Diag.WriteLine(NSpace, $"shift: {shift}");

        var cx = CaretIndex;
        int index = GetStartOfLine(Text ?? "", cx, true);
        Diag.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

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
        Diag.WriteLine(NSpace, $"shift: {shift}");

        var cx = CaretIndex;
        int index = GetEndOfLine(Text ?? "", cx);
        Diag.WriteLine(NSpace, $"Caret: {cx}, index: {index}");

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
            Diag.WriteLine(NSpace, $"index0, index1: {index0}, {index1}");

            var t0 = text.Substring(0, index0);
            var t1 = Unindent(text.Substring(index0, index1 - index0), out int d0, out int d1);
            var t2 = text.Substring(index1);
            Diag.WriteLine(NSpace, $"d0: {d0}");
            Diag.WriteLine(NSpace, $"d1: {d1}");

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
            Diag.WriteLine(NSpace, $"index0, index1: {index0}, {index1}");

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
                Diag.WriteLine(NSpace, $"delta: {delta}");

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
        Diag.WriteLine(NSpace, $"startIndex: {startIndex}");

        return IterateUp(text, startIndex, 2, ref offset);
    }

    private int IteratePageUp(string text, int startIndex, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IteratePageUp)}";
        Diag.WriteLine(NSpace, $"startIndex: {startIndex}");

        // Plus 1 deliberate
        return IterateUp(text, startIndex, GetPageHeight() + 1, ref offset);
    }

    private int IterateUp(string text, int startIndex, int height, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IterateUp)}";
        Diag.WriteLine(NSpace, $"Height: {height}");
        Diag.ThrowIfNegativeOrZero(height);

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
        Diag.WriteLine(NSpace, $"startIndex: {startIndex}");

        // Just 1 here
        return IterateDown(text, startIndex, 1, ref offset);
    }

    private int IteratePageDown(string text, int startIndex, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IteratePageDown)}";
        Diag.WriteLine(NSpace, $"startIndex: {startIndex}");

        // Plus 1 deliberate
        return IterateDown(text, startIndex, GetPageHeight() + 1, ref offset);
    }

    private int IterateDown(string text, int startIndex, int height, ref int offset)
    {
        const string NSpace = $"{nameof(TextEditor)}.{nameof(IterateDown)}";
        Diag.WriteLine(NSpace, $"Height: {height}");
        Diag.ThrowIfNegativeOrZero(height);

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

        var childs = new List<LightButton>(4);

        // 1. REVEAL
        if (HasRevealButton)
        {
            _revealButton ??= CreateButton(Symbols.Visibility, RevealClickHandler, true, RevealPassword);
            _revealButton.Tip = "Reveal";
            childs.Add(_revealButton);
        }
        else
        {
            _revealButton = null;
        }

        // 2. COPY
        if (HasCopyButton)
        {
            _copyButton ??= CreateButton(Symbols.ContentCopy, CopyClickHandler, false);
            _copyButton.Tip = "Copy";
            childs.Add(_copyButton);
        }
        else
        {
            _copyButton = null;
        }

        // 3. CASE
        if (HasMatchCaseButton)
        {
            _caseButton ??= CreateButton(Symbols.MatchCase, CaseWordClickHandler, true);
            _caseButton.Tip = "Match case";
            childs.Add(_caseButton);
        }
        else
        {
            _caseButton = null;
        }

        // 4. WORD
        if (HasMatchWordButton)
        {
            _wordButton ??= CreateButton(Symbols.MatchWord, CaseWordClickHandler, true);
            _wordButton.Tip = "Match words";
            childs.Add(_wordButton);
        }
        else
        {
            _wordButton = null;
        }

        // 3. BACK DELETE
        if (HasBackButton)
        {
            childs.Add(_backButton);
        }

        // STACK
        _stack.Children.Replace(childs);
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
        if (_revealButton != null)
        {
            RevealPassword = _revealButton.IsChecked;
        }

        Focus();
    }

    private void CopyClickHandler(object? _, EventArgs __)
    {
        CopySelectedOrAll();
    }

    private void CaseWordClickHandler(object? _, EventArgs __)
    {
        OnSubmitted();
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