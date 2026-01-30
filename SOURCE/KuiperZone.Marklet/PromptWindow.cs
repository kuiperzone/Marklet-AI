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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using KuiperZone.Marklet.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet;

/// <summary>
/// MessageBox class with static "show dialog" methods.
/// </summary>
public sealed class PromptWindow : ChromeWindow
{
    /// <summary>
    /// Gets the maximum <see cref="Text"/> length.
    /// </summary>
    public const int MaxTextLength = 64 * 1024;

    private readonly Grid _grid = new();
    private readonly PromptBar _bar = new();
    private readonly TextEditor _editor = new();

    private readonly ContextMenu _context = new();
    private readonly MenuItem _incrementMenu = new();
    private readonly MenuItem _decrementMenu = new();
    private readonly MenuItem _resetMenu = new();
    private readonly MenuItem _monoMenu = new();
    private readonly MenuItem _clearMenu = new();
    private readonly MenuItem _attachMenu = new();
    private readonly MenuItem _submitMenu = new();

    private readonly DispatcherTimer _timer = new();
    private const bool _isAttachButtonVisible = false; // TBD
    private bool _isMonospace;

    /// <summary>
    /// Constructor.
    /// </summary>
    public PromptWindow()
        : base(true)
    {
        // Sanity
        ConditionalDebug.ThrowIfGreaterThan(MaxTextLength, GardenLeaf.MaxContentLength);

        Title = "Prompt";
        ChromeBar.Title = "";
        IsChromeAlwaysVisible = false;

        Width = 600;
        MinWidth = 400;
        Height = 600;
        MinHeight = 200;
        CanResize = true;

        _grid.RowDefinitions.Add(new(GridLength.Star));
        _grid.RowDefinitions.Add(new(GridLength.Auto));
        Content = _grid;

        Grid.SetRow(_editor, 0);
        _editor.CornerRadius = default;
        _editor.BorderThickness = default;
        _editor.Classes.Add("fixed-background");
        _editor.AcceptsTab = true;
        _editor.AcceptsReturn = true;
        _editor.MaxLength = MaxTextLength;
        _editor.TextWrapping = TextWrapping.Wrap;
        _editor.Submitted += SubmitHandler;
        _grid.Children.Add(_editor);

        Grid.SetRow(_bar, 1);
        _bar.Margin = new(ChromeSizes.TwoCh);

        _bar.Attach.IsVisible = _isAttachButtonVisible;
        // _bar.Attach.Click TBD
        _bar.Copy.Click += CopyClickHandler;
        _bar.Paste.Click += PasteClickHandler;
        _bar.SelectAll.Click += SelectAllClickHandler;
        _bar.Monospace.Click += MonospaceClickHandler;
        _bar.EditWindow.IsVisible = false;

        _bar.Submit.Content = Symbols.KeyboardArrowUp  + Symbols.KeyboardReturn;
        _bar.Submit.Gesture = new(Key.Enter, KeyModifiers.Control);
        _bar.Submit.Click += SubmitHandler;
        _grid.Children.Add(_bar);

        var menu = ChromeBar.LeftGroup.Add(Symbols.Menu,  MakeContext(), "Menu");
        menu.Gesture = new(Key.F, KeyModifiers.Alt);

        _timer.Interval = TimeSpan.FromMilliseconds(1000);
        _timer.Tick += TimerTickHandler;

        Activated += ActivatedHandler;
    }

    /// <summary>
    /// Gets or sets whether the editor uses monospace.
    /// </summary>
    public string? Text
    {
        get { return _editor.Text; }
        set { _editor.Text = value; }
    }

    /// <summary>
    /// Gets or sets whether the editor uses monospace.
    /// </summary>
    public bool IsMonospace
    {
        get { return _isMonospace; }

        set
        {
            if (_isMonospace != value)
            {
                _isMonospace = value;
                _bar.Monospace.IsChecked = value;
                _editor.FontFamily = value ? AppFonts.MonospaceFamily : AppFonts.DefaultFamily;
            }
        }
    }

    /// <summary>
    /// Gets whether the editor contains non-whitespace text.
    /// </summary>
    public bool HasContent
    {
        get { return !string.IsNullOrWhiteSpace(_editor.Text); }
    }

    /// <summary>
    /// Gets the zoomer.
    /// </summary>
    public Zoomer Zoom { get; } = new();

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        ZoomChangedHandler(null, EventArgs.Empty);
        Zoom.Changed += ZoomChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnStylingChanged(bool init)
    {
        base.OnStylingChanged(init);
        _editor.Background = Styling.FocusedBox;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Zoom.HandleKeyGesture(e);
        _bar.HandleKeyGesture(e);
    }

    private ContextMenu MakeContext()
    {
        _context.Cursor = ChromeCursors.ArrowCursor;
        _context.Opened += ContextOpeningHandler;
        _context.Closed += ContextClosedHandler;

        _incrementMenu.Header = ChromeFonts.NewTextBlock(Symbols.AddCircle + " Increment Scale");
        _incrementMenu.InputGesture = new(Key.Add, KeyModifiers.Control);
        _incrementMenu.Click += IncrementClickHandler;
        _context.Items.Add(_incrementMenu);

        _decrementMenu.Header = ChromeFonts.NewTextBlock(Symbols.DoNotDisturbOn + " Decrement Scale");
        _decrementMenu.InputGesture = new(Key.Subtract, KeyModifiers.Control);
        _decrementMenu.Click += DecrementClickHandler;
        _context.Items.Add(_decrementMenu);

        _resetMenu.Header = ChromeFonts.NewTextBlock(Symbols.ViewRealSize + " Reset Scale");
        _resetMenu.InputGesture = new(Key.D0, KeyModifiers.Control);
        _resetMenu.Click += ResetClickHandler;
        _context.Items.Add(_resetMenu);

        _context.Items.Add(new Separator());

        _monoMenu.Header = ChromeFonts.NewTextBlock("Monospace");
        _monoMenu.ToggleType = MenuItemToggleType.CheckBox;
        _monoMenu.InputGesture = _bar.Monospace.Gesture;
        _monoMenu.Click += MonospaceClickHandler;
        _context.Items.Add(_monoMenu);

        _context.Items.Add(new Separator());

        _clearMenu.Header = ChromeFonts.NewTextBlock("Clear");
        _clearMenu.Click += ClearClickHandler;
        _context.Items.Add(_clearMenu);

        _attachMenu.Header = ChromeFonts.NewTextBlock("Add Attachment");
        _attachMenu.IsEnabled = false;
        _attachMenu.InputGesture = _bar.Attach.Gesture;
        _attachMenu.Click += AttachClickHandler;
        _context.Items.Add(_attachMenu);

        _submitMenu.Header = ChromeFonts.NewTextBlock("Submit");
        _submitMenu.InputGesture = _bar.Submit.Gesture;
        _submitMenu.Click += SubmitHandler;
        _context.Items.Add(_submitMenu);

        return _context;
    }

    private void ContextOpeningHandler(object? _, EventArgs __)
    {
        var editor = _editor;
        bool empty = string.IsNullOrEmpty(editor.Text);

        _resetMenu.IsEnabled = !Zoom.IsDefault;
        _incrementMenu.IsEnabled = !Zoom.IsMaximum;
        _decrementMenu.IsEnabled = !Zoom.IsMinimum;
        _clearMenu.IsEnabled = !empty;
        _submitMenu.IsEnabled = HasContent;

        _monoMenu.IsChecked = IsMonospace;

    }

    private void ContextClosedHandler(object? _, EventArgs __)
    {
        _editor.Focus();
    }

    private void ZoomChangedHandler(object? sender, EventArgs __)
    {
        _editor.FontSize = Zoom.Fraction * ChromeFonts.DefaultFontSize;

        if (sender == Zoom)
        {
            ChromeBar.Title = Zoom.Scale + "%";
            _timer.Restart();
        }
    }

    private void ActivatedHandler(object? _, EventArgs __)
    {
        _editor.Focus();
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        _timer!.Stop();
        ChromeBar.Title = "";
    }

    private void CopyClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.CopySelectedOrAll();
    }

    private void PasteClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.Paste();
    }

    private void SelectAllClickHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;
        _editor.SelectAll();
    }

    private void MonospaceClickHandler(object? _, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            IsMonospace = !IsMonospace;
        }
    }

    private void ResetClickHandler(object? _, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            Zoom.Reset();
        }
    }

    private void IncrementClickHandler(object? _, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            Zoom.Increment();
        }
    }

    private void DecrementClickHandler(object? _, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            Zoom.Decrement();
        }
    }

    private void ClearClickHandler(object? _, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            _editor.Clear();
        }
    }

    private void AttachClickHandler(object? _, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
        }
    }

    private void SubmitHandler(object? _, RoutedEventArgs e)
    {
        e.Handled = true;

        // Return true if contains text,
        // or false if empty or whitespace.
        Close(HasContent);
    }

}
