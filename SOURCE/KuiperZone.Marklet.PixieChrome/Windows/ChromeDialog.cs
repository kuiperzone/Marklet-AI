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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// A <see cref="ChromeWindow"/> subclass class primarily intended to display message text, but may also show complex
/// vertically arranged <see cref="Children"/> within a single <see cref="PixieGroup"/> control.
/// </summary>
public class ChromeDialog : ChromeWindow
{
    private readonly ScrollViewer _scroller = new();
    private readonly Grid _grid = new();
    private readonly PixieGroup _group = new();
    private readonly DialogControls _controls = new();

    static ChromeDialog()
    {
        MinWidthProperty.OverrideDefaultValue<ChromeDialog>(400.0);
        MaxWidthProperty.OverrideDefaultValue<ChromeDialog>(600.0);
        MinHeightProperty.OverrideDefaultValue<ChromeDialog>(190.0);
        MaxHeightProperty.OverrideDefaultValue<ChromeDialog>(800.0);
        SizeToContentProperty.OverrideDefaultValue<ChromeDialog>(SizeToContent.WidthAndHeight);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ChromeDialog()
        : base(true)
    {
        // Expect
        Diag.ThrowIfNotEqual(SizeToContent.WidthAndHeight, SizeToContent);
        Diag.ThrowIfNotEqual(ScrollBarVisibility.Auto, _scroller.VerticalScrollBarVisibility);
        Diag.ThrowIfNotEqual(ScrollBarVisibility.Disabled, _scroller.HorizontalScrollBarVisibility);

        const double UniPad = DialogControls.UniformPadding;
        _grid.Margin = new(0.0, UniPad, 0.0, 0.0);
        _grid.RowDefinitions.Add(new(GridLength.Star));
        _grid.RowDefinitions.Add(new(GridLength.Auto));

        _group.MinWidth = MinWidth - UniPad * 2.0;
        _group.Margin = new(UniPad, 12.0, UniPad, 0.0);
        Children = _group.Children;

        _scroller.Content = _group;

        Grid.SetRow(_scroller, 0);
        _grid.Children.Add(_scroller);

        Grid.SetRow(_controls, 1);
        _grid.Children.Add(_controls);
        _controls.Click += (_, e) => Close(e.Button);

        Content = _grid;
        ButtonText = _controls.ButtonText;

        GroupClasses.Add("pill-list");
        GroupClasses.Add("shade-background");
    }

    /// <summary>
    /// Gets or sets the primary message text.
    /// </summary>
    public string? Message
    {
        get { return _group.TopTitle; }
        set { _group.TopTitle = value; }
    }

    /// <summary>
    /// Gets or sets a tagline under <see cref="Message"/>.
    /// </summary>
    public string? Details
    {
        get { return _group.TopFooter; }
        set { _group.TopFooter = value; }
    }

    /// <summary>
    /// Gets child controls shown within a <see cref="PixieGroup"/>.
    /// </summary>
    /// <remarks>
    /// A non-static caller may populate this. Children will be displayed within an <see cref="PixieGroup"/> container,
    /// and it is expected, but not enforced, that children be <see cref="PixieControl"/> items.
    /// </remarks>
    public Avalonia.Controls.Controls Children { get; }

    /// <summary>
    /// Gets a list of XAML style class names to add to the <see cref="PixieGroup"/> container of <see
    /// cref="Children"/>.
    /// </summary>
    /// <remarks>
    /// The sequence should be populated prior to the window becoming visible. The sequence should be clear to display
    /// with the default background.
    /// </remarks>
    public List<string> GroupClasses { get; } = new();

    /// <summary>
    /// Gets or sets the buttons shown in the bottom panel.
    /// </summary>
    /// <remarks>
    /// Setting has no effect once the window is open. The value must not contain an illegal combination of multiple
    /// "default" or "cancel" buttons otherwise the setter throws. The default is <see cref="DialogButtons.Close"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">Illegal combined button flag</exception>
    public DialogButtons Buttons
    {
        get { return _controls.Buttons; }
        set { _controls.Buttons = value; }
    }

    /// <summary>
    /// Gets or sets which <see cref="Buttons"/> are disabled.
    /// </summary>
    /// <remarks>
    /// This may be used be subclass do disable selected buttons. The default is <see cref="DialogButtons.None"/> which
    /// implies all buttons are enabled.
    /// </remarks>
    public DialogButtons DisabledButtons
    {
        get { return _controls.DisabledButtons; }
        set { _controls.DisabledButtons = value; }
    }

    /// <summary>
    /// Gets a mutable button text dictionary.
    /// </summary>
    /// <remarks>
    /// The window may use this to change default button text before the control is displayed. Default is empty.
    /// </remarks>
    public Dictionary<DialogButtons, string> ButtonText { get; }

    /// <summary>
    /// Gets the result of the window.
    /// </summary>
    /// <remarks>
    /// The window closes when the value is set to any value other than <see cref="DialogButtons.None"/> and is set
    /// automatically when the user clicks one of <see cref="Buttons"/>. The result will comprise a single <see
    /// cref="DialogButtons"/> bit flag pertaining to the button clicked. It will be <see cref="DialogButtons.None"/>
    /// where the window system close button is clicked or the user presses the escape key.
    /// </remarks>
    public DialogButtons ModalResult { get; private set; }

    /// <summary>
    /// Gets whether <see cref="ModalResult"/> is a "positive action" value, i.e. indicates to go ahead and do the thing
    /// rather than cancel or abort.
    /// </summary>
    /// <remarks>
    /// This does not distinguish between, for example, <see cref="DialogButtons.Ok"/> and <see
    /// cref="DialogButtons.DeleteAll"/>. It is not suitable where the window displays complex buttons.
    /// </remarks>
    public bool IsPositiveResult
    {
        get { return ModalResult.IsPositiveResult(); }
    }

    /// <summary>
    /// Shows an instance of <see cref="ChromeDialog"/> with the given "message" and application default title.
    /// </summary>
    /// <remarks>
    /// The "visual" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    /// <exception cref="ArgumentNullException">visual</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? visual, string message, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowInternal(GetWindow(visual), message, null, null, buttons);
    }

    /// <summary>
    /// Shows an instance of <see cref="ChromeDialog"/> with the given "message" and window "title".
    /// </summary>
    /// <remarks>
    /// The "visual" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    /// <exception cref="ArgumentNullException">visual</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? visual, string message, string? title, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowInternal(GetWindow(visual), message, title, null, buttons);
    }

    /// <summary>
    /// Shows Exception information.
    /// </summary>
    /// <remarks>
    /// The "visual" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked. Where
    /// "showStack" is true, the full error stack is shown, whereas only the message is shown if false. If null, the
    /// stack is shown only for DEBUG.
    /// </remarks>
    /// <exception cref="ArgumentNullException">visual</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? visual, Exception error, bool? showStack = null)
    {
        return ShowInternal(GetWindow(visual), error, showStack);
    }

    /// <summary>
    /// Shows the window as a dialog.
    /// </summary>
    /// <remarks>
    /// The "visual" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    /// <exception cref="ArgumentNullException">visual</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public Task<DialogButtons> ShowDialog(object? visual)
    {
        return ShowDialog<DialogButtons>(GetWindow(visual));
    }

    /// <summary>
    /// Sets <see cref="ModalResult"/> and closes the window.
    /// </summary>
    /// <remarks>
    /// The value must be a single <see cref="DialogButtons"/> value rather than combined flags. Subclass should call
    /// this rather then the subclass Close() method as it sets <see cref="ModalResult"/>. It may be overridden to
    /// intercept "button" before closing.
    /// </remarks>
    /// <exception cref="ArgumentException">Illegal combined button flag</exception>
    protected virtual void Close(DialogButtons button)
    {
        if (!button.IsSingleLegal())
        {
            throw new ArgumentException("Illegal combined button flag", nameof(button));
        }

        ModalResult = button;
        base.Close(button);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        _group.Classes.AddRange(GroupClasses);
        base.OnOpened(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!_controls.HandleKeyGesture(e))
        {
            base.OnKeyDown(e);
        }
    }

    private static Window GetWindow(object? sender)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));

        if (sender is Window window)
        {
            return window;
        }

        if (sender is Visual visual)
        {
            return GetTopLevel(visual) as Window ??
                throw new ArgumentException("Visual has no owning window", nameof(sender));
        }

        throw new ArgumentException("Not instance of Visual", nameof(sender));
    }

    private static Task<DialogButtons> ShowInternal(Window owner, string message, string? title, string? details, DialogButtons buttons)
    {
        var window = new ChromeDialog();
        window.Message = message;
        window.Details = details;
        window.Buttons = buttons;

        window.Title = string.IsNullOrWhiteSpace(title) ? ChromeApplication.Current.Name : title;
        window.ChromeBar.Title = title;

        return window.ShowDialog<DialogButtons>(owner);
    }

    private static Task<DialogButtons> ShowInternal(Window owner, Exception error, bool? showStack = null)
    {
#if DEBUG
        showStack ??= true;
#endif
        var window = new ChromeDialog();
        window.Title = ChromeApplication.Current.Name;
        window.Message = error.Message;
        window.Details = showStack == true ? error.ToString() : error.GetType().Name;
        return window.ShowDialog<DialogButtons>(owner);
    }
}