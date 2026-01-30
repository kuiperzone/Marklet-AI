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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// A <see cref="ChromeWindow"/> subclass class primarily intended to display message text, but may also show complex
/// vertically arranged <see cref="Children"/>.
/// </summary>
public class MessageDialog : ChromeWindow
{
    private static readonly DialogButtons[] AllButtons = Enum.GetValues<DialogButtons>();
    private readonly ScrollViewer _scroller = new();
    private readonly Grid _grid = new();
    private readonly PixieGroup _group = new();
    private readonly StackPanel _buttonPanel = new();

    static MessageDialog()
    {
        MinWidthProperty.OverrideDefaultValue<MessageDialog>(400.0);
        MaxWidthProperty.OverrideDefaultValue<MessageDialog>(800.0);
        MinHeightProperty.OverrideDefaultValue<MessageDialog>(190.0);
        MaxHeightProperty.OverrideDefaultValue<MessageDialog>(800.0);
        SizeToContentProperty.OverrideDefaultValue<MessageDialog>(SizeToContent.WidthAndHeight);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public MessageDialog()
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
        _group.Classes.Add("chrome-high");
        _group.Classes.Add("chrome-corner-grouped");
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
    /// Gets or sets the buttons shown.
    /// </summary>
    /// <remarks>
    /// Setting has no effect once the window is shown.
    /// </remarks>
    public DialogButtons Buttons { get; set; } = DialogButtons.Ok;

    /// <summary>
    /// Gets the owning Window from "sender" or throws.
    /// </summary>
    /// <exception cref="ArgumentNullException">sender</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Window GetWindow(object? sender)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));

        if (sender is Window window)
        {
            return window;
        }

        if (sender is Visual visual)
        {
            return GetTopLevel(visual) as Window ?? throw new ArgumentException("Visual has no owning window", nameof(visual));
        }

        throw new ArgumentException("Not instance of Visual", nameof(sender));
    }

    /// <summary>
    /// Shows an instance of <see cref="MessageDialog"/> with the given "message" and application default title.
    /// </summary>
    /// <remarks>
    /// The result comprises a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    public static Task<DialogButtons> ShowDialog(Window owner, string message, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowDialog(owner, message, "", buttons);
    }

    /// <summary>
    /// Variant overload with <see cref="Visual"/> "sender" from which the owning window is determined.
    /// </summary>
    /// <remarks>
    /// The "sender" must be a non-null instance of <see cref="Visual"/> otherwise the call throws.
    /// </remarks>
    /// <exception cref="ArgumentNullException">sender</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? sender, string message, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowDialog(GetWindow(sender), message, buttons);
    }

    /// <summary>
    /// Shows an instance of <see cref="MessageDialog"/> with the given "message" and "title".
    /// </summary>
    /// <remarks>
    /// The result comprises a single <see cref="DialogButtons"/> flag value pertaining to the button clicked.
    /// </remarks>
    public static Task<DialogButtons> ShowDialog(Window owner, string message, string title, DialogButtons buttons = DialogButtons.Ok)
    {
        var window = new MessageDialog();
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

    /// <summary>
    /// Variant overload with <see cref="Visual"/> "sender" from which the owning window is determined.
    /// </summary>
    /// <remarks>
    /// The "sender" must be a non-null instance of <see cref="Visual"/> otherwise the call throws.
    /// </remarks>
    /// <exception cref="ArgumentNullException">sender</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? sender, string message, string title, DialogButtons buttons = DialogButtons.Ok)
    {
        return ShowDialog(GetWindow(sender), message, title, buttons);
    }

    /// <summary>
    /// Shows an instance of <see cref="MessageDialog"/> with exception information.
    /// </summary>
    /// <remarks>
    /// Where "showStack" is true, the full error stack is shown, whereas only the message is shown if false. If null,
    /// the stack is shown only where DEBUG is defined. The result comprises a single <see cref="DialogButtons"/> flag
    /// value pertaining to the button clicked.
    /// </remarks>
    public static Task<DialogButtons> ShowDialog(Window owner, Exception error, bool? showStack = null)
    {
#if DEBUG
        showStack ??= true;
#endif
        var window = new MessageDialog();
        window.Title = ChromeApplication.Current.Name;
        window.Message = error.Message;
        window.Details = showStack == true ? error.ToString() : error.GetType().Name;
        return window.ShowDialog<DialogButtons>(owner);
    }

    /// <summary>
    /// Variant overload with visual item from which the owning window is determined.
    /// </summary>
    /// <remarks>
    /// The "sender" must be a non-null instance of <see cref="Visual"/> otherwise the call throws.
    /// </remarks>
    /// <exception cref="ArgumentNullException">sender</exception>
    /// <exception cref="ArgumentException">Not instance of Visual, or Visual has no owning window</exception>
    public static Task<DialogButtons> ShowDialog(object? sender, Exception error, bool? showStack = null)
    {
        return ShowDialog(GetWindow(sender), error, showStack);
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        const string NSpace = $"{nameof(MessageDialog)}.{nameof(OnKeyDown)}";
        ConditionalDebug.WriteLine(NSpace, $"Physical key: {e.PhysicalKey}");

        if (e.KeyModifiers == KeyModifiers.None)
        {
            var flag = Buttons.GetCloseAction(e.PhysicalKey);

            if (flag != DialogButtons.None)
            {
                ConditionalDebug.WriteLine(NSpace, $"Close result: {flag}");
                e.Handled = true;
                Close(flag);
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

    private void AddButton(DialogButtons flag)
    {
        var btn = new LightButton();
        btn.Classes.Add("dialog-button");
        btn.Content = flag == DialogButtons.Ok ? "OK" : flag.ToString().GetFriendlyNameOf();
        btn.Click += (_, __) => { Close(flag); };

        _buttonPanel.Children.Add(btn);
    }
}