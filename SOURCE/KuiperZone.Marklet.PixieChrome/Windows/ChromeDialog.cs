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
/// vertically arranged <see cref="Children"/>.
/// </summary>
public class ChromeDialog : ChromeWindow
{
    private static readonly DialogButtons[] AllButtons = Enum.GetValues<DialogButtons>();
    private readonly ScrollViewer _scroller = new();
    private readonly Grid _grid = new();
    private readonly PixieGroup _group = new();
    private readonly StackPanel _buttonPanel = new();
    private DialogButtons _buttons = DialogButtons.Close;
    private DialogButtons _disabledButtons;

    static ChromeDialog()
    {
        MinWidthProperty.OverrideDefaultValue<ChromeDialog>(400.0);
        MaxWidthProperty.OverrideDefaultValue<ChromeDialog>(800.0);
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
        const double MarginH = 20.0;

        // Expect
        ConditionalDebug.ThrowIfNotEqual(SizeToContent.WidthAndHeight, SizeToContent);
        ConditionalDebug.ThrowIfNotEqual(ScrollBarVisibility.Auto, _scroller.VerticalScrollBarVisibility);
        ConditionalDebug.ThrowIfNotEqual(ScrollBarVisibility.Disabled, _scroller.HorizontalScrollBarVisibility);

        _grid.Margin = new(0.0, MarginH);
        _grid.RowDefinitions.Add(new(GridLength.Star));
        _grid.RowDefinitions.Add(new(GridLength.Auto));

        _group.MinWidth = MinWidth - MarginH * 2.0;
        _group.Margin = new(MarginH, 12.0, MarginH, 0.0);
        _group.Classes.Add("shade-background");
        _group.Classes.Add("pill-list");
        Children = _group.Children;

        _scroller.Content = _group;

        Grid.SetRow(_scroller, 0);
        _grid.Children.Add(_scroller);

        _buttonPanel.Margin = new(MarginH, 36.0, MarginH, 0.0);
        _buttonPanel.Spacing = 8.0;
        _buttonPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
        _buttonPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        _buttonPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

        Grid.SetRow(_buttonPanel, 1);
        _grid.Children.Add(_buttonPanel);

        Content = _grid;
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
    /// Gets additional child controls shown within a <see cref="PixieGroup"/>.
    /// </summary>
    /// <remarks>
    /// It is intended that this be used to display <see cref="PixieControl"/> items.
    /// </remarks>
    public Avalonia.Controls.Controls Children { get; }

    /// <summary>
    /// Gets or sets the buttons shown in the bottom panel.
    /// </summary>
    /// <remarks>
    /// Setting has no effect once the window is open. The value must not contain an illegal combination of multiple
    /// "default" or "cancel" buttons otherwise the setter throws.
    /// </remarks>
    public DialogButtons Buttons
    {
        get { return _buttons; }

        set
        {
            if (_buttons != value && !value.IsCombinedLegal())
            {
                throw new ArgumentException("Illegal button combination", nameof(Buttons));
            }

            _buttons = value;
        }
    }

    /// <summary>
    /// Gets or sets which <see cref="Buttons"/> are disabled.
    /// </summary>
    /// <remarks>
    /// This may be used be subclass do disable selected buttons. The default is <see cref="DialogButtons.None"/> which
    /// implies all buttons are enabled.
    /// </remarks>
    protected DialogButtons DisabledButtons
    {
        get { return _disabledButtons; }

        set
        {
            if (_disabledButtons != value)
            {
                _disabledButtons = value;

                foreach (var item in _buttonPanel.Children)
                {
                    if (item.Tag is DialogButtons flag)
                    {
                        item.IsEnabled = !value.HasFlag(flag);
                    }
                }
            }
        }
    }

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
    /// The "owner" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    /// <exception cref="ArgumentNullException">owner</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? owner, string message, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowDialog(GetWindow(owner), message, buttons);
    }

    /// <summary>
    /// Shows an instance of <see cref="ChromeDialog"/> with the given "message" and window "title".
    /// </summary>
    /// <remarks>
    /// The "owner" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    /// <exception cref="ArgumentNullException">owner</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? owner, string message, string? title, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowDialog(GetWindow(owner), message, title, buttons);
    }

    /// <summary>
    /// Shows Exception information.
    /// </summary>
    /// <remarks>
    /// The "owner" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked. Where
    /// "showStack" is true, the full error stack is shown, whereas only the message is shown if false. If null, the
    /// stack is shown only for DEBUG.
    /// </remarks>
    /// <exception cref="ArgumentNullException">owner</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? owner, Exception error, bool? showStack = null)
    {
        return ShowDialog(GetWindow(owner), error, showStack);
    }

    /// <summary>
    /// Shows the window as a dialog.
    /// </summary>
    /// <remarks>
    /// The "owner" must be a non-null instance of <see cref="Visual"/> with an owning window otherwise the call throws.
    /// The result is a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    /// <exception cref="ArgumentNullException">sender</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public Task<DialogButtons> ShowDialog(object? owner)
    {
        return ShowDialog<DialogButtons>(GetWindow(owner));
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
    /// Gets the display text for the given "button".
    /// </summary>
    /// <remarks>
    /// May be overridden.
    /// </remarks>
    protected virtual string GetButtonText(DialogButtons button)
    {
        return button == DialogButtons.Ok ? "OK" : button.ToString().GetFriendlyNameOf();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(ChromeDialog)}.{nameof(OnKeyDown)}";
        ConditionalDebug.WriteLine(NSpace, $"Physical key: {e.PhysicalKey}");

        if (e.KeyModifiers == KeyModifiers.None)
        {
            var flag = Buttons.GetCloseAction(e.Key);

            if (flag != DialogButtons.None)
            {
                ConditionalDebug.WriteLine(NSpace, $"Close result: {flag}");
                e.Handled = true;
                base.Close(flag);
                return;
            }
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (!HasOpened)
        {
            foreach (var item in AllButtons)
            {
                if (item != DialogButtons.None && Buttons.HasFlag(item))
                {
                    AddButton(item);
                }
            }
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

    private static Task<DialogButtons> ShowDialog(Window owner, string message, string? title, DialogButtons buttons = DialogButtons.Ok)
    {
        var window = new ChromeDialog();
        window.Message = message;
        window.Buttons = buttons;

        if (string.IsNullOrWhiteSpace(title))
        {
            window.Title = ChromeApplication.Current.Name;
        }
        else
        {
            window.Title = title;
        }

        window.ChromeBar.Title = title;
        return window.ShowDialog<DialogButtons>(owner);
    }

    private static Task<DialogButtons> ShowDialog(Window owner, Exception error, bool? showStack = null)
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

    private void AddButton(DialogButtons button)
    {
        var obj = new LightButton();
        obj.Classes.Add("dialog-button");
        obj.Content = GetButtonText(button);
        obj.Tag = button;
        obj.IsEnabled = !_disabledButtons.HasFlag(button);
        obj.Click += (_, __) => { Close(button); };

        if (button.IsCritical())
        {
            obj.Classes.Add("critical-background");
        }
        else
        if (button.IsDefault())
        {
            obj.Classes.Add("accent-background");
        }
        else
        {
            obj.Classes.Add("regular-background");
        }

        _buttonPanel.Children.Add(obj);
    }
}