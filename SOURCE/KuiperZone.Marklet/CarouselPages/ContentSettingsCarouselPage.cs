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

using KuiperZone.Marklet.Shared;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.CarouselPages;
using Avalonia.Controls;
using Avalonia;

namespace KuiperZone.Marklet.CarouselPages;

/// <summary>
/// Concrete subclass of <see cref="SettingsCarouselPage{T}"/> which hosts <see cref="ContentSettings"/>.
/// </summary>
public sealed class ContentSettingsCarouselPage : SettingsCarouselPage<ContentSettings>
{
    private static bool s_isPreviewOpen = true;

    private readonly PixieCombo _contentWidthCombo = new();
    private readonly PixieNumeric _scaleNumeric = new();

    private readonly PixieGroup _previewGroup = new();
    private readonly PixieControl<Preview> _previewer = new();

    private readonly PixieCombo _bodyFontCombo = new();

    private readonly PixieCombo _headFontCombo = new();
    private readonly PixieAccentPicker _headColor = new();

    private readonly PixieSwitch _fencedWrapSwitch = new();
    private readonly PixieAccentPicker _fencedColor = new();

    private readonly PixieAccentPicker _userPicker = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ContentSettingsCarouselPage()
        : base(ContentSettings.Global)
    {
        Title = "Content";
        Symbol = Symbols.Chat;

        // MAIN
        var group = new PixieGroup();
        group.TopTitle = "Content View";
        Children.Add(group);

        _scaleNumeric.Title = "Default Scale";
        _scaleNumeric.LeftSymbol = Symbols.TextIncrease;
        _scaleNumeric.Footer = "Keys CTRL+, CTRL- and CTRL-0 can be used to adjust in main window";
        _scaleNumeric.MinValue = ContentSettings.MinScale;
        _scaleNumeric.MaxValue = ContentSettings.MaxScale;
        _scaleNumeric.Default = 100;
        _scaleNumeric.Increment = 5;
        _scaleNumeric.AcceptFractionInput = false;
        _scaleNumeric.Units = "%";
        group.Children.Add(_scaleNumeric);

        _contentWidthCombo.Title = "Content Width";
        _contentWidthCombo.LeftSymbol = Symbols.FitPageWidth;
        _contentWidthCombo.Footer = "Content display width";
        _contentWidthCombo.SetItemsAs<ContentWidth>();
        group.Children.Add(_contentWidthCombo);


        // PREVIEW
        group = _previewGroup;
        group.TopTitle = "Preview";
        group.IsCollapsable = true;
        group.CollapseTitleWeight = FontWeight.Bold;
        group.IsOpen = s_isPreviewOpen;
        Children.Add(group);

        group.Children.Add(_previewer);


        // BODY
        group = new PixieGroup();
        group.TopTitle = "Body Text";
        Children.Add(group);

        _bodyFontCombo.Title = "Default Font";
        _bodyFontCombo.LeftSymbol = Symbols.FontDownload;
        _bodyFontCombo.Footer = "Font style for default content text";
        _bodyFontCombo.SetItemsAs<FontCategory>();
        group.Children.Add(_bodyFontCombo);


        // HEADINGS
        group = new PixieGroup();
        group.TopTitle = "Headings";
        Children.Add(group);

        _headFontCombo.Title = "Heading Font";
        _headFontCombo.LeftSymbol = Symbols.FormatH1;
        _headFontCombo.Footer = "Font style for headings";
        _headFontCombo.SetItemsAs<FontCategory>();
        group.Children.Add(_headFontCombo);

        _headColor.Header = "Foreground Color";
        _headColor.IsSecondaryVisible = true;
        _headColor.IsDefaultColorVisible = true;
        group.Children.Add(_headColor);


        // CODE
        group = new PixieGroup();
        group.TopTitle = "Fenced Code";
        Children.Add(group);

        _fencedColor.Header = "Foreground Color";
        _fencedColor.Footer = "Default foreground color fenced code";
        _fencedColor.IsSecondaryVisible = true;
        _fencedColor.IsDefaultColorVisible = true;
        group.Children.Add(_fencedColor);

        _fencedWrapSwitch.Title = "Default Code Wrap";
        _fencedWrapSwitch.LeftSymbol = Symbols.WrapText;
        _fencedWrapSwitch.Footer = "Fenced and indented code line wraps by default when checked";
        group.Children.Add(_fencedWrapSwitch);


        // USER
        group = new PixieGroup();
        group.TopTitle = "User Background";
        Children.Add(group);

        _userPicker.Header = "Base Color";
        _userPicker.Footer = "Base color for user message background. Follows accent color by default.";
        _userPicker.IsDefaultColorVisible = true;
        _userPicker.DefaultColorLabel = "Accent";
        group.Children.Add(_userPicker);


        // RESET
        Children.Add(NewResetGroup($"Reset {Title} defaults"));

        // Update in construction not OpOpened().
        // Possible that some ChangedHandlers may be delayed.
        UpdateControls(Settings);
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateControls(ContentSettings settings)
    {
        _contentWidthCombo.SelectedIndex = (int)settings.Width;
        _scaleNumeric.Value = settings.DefaultScale;

        _bodyFontCombo.SelectedIndex = (int)settings.BodyFont;

        _headFontCombo.SelectedIndex = (int)settings.HeadingFont;
        _headColor.ChosenColor = settings.ToHeadingColor();

        _fencedWrapSwitch.IsChecked = settings.DefaultFencedWrap;
        _fencedColor.ChosenColor = settings.ToFencedColor();

        _userPicker.ChosenColor = settings.ToUserColor(default);

        _previewer.ChildControl.Refresh(settings);

        UpdateControlEnabledStates();
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override void UpdateSettings(ContentSettings settings)
    {
        settings.Width = _contentWidthCombo.GetSelectedIndexAs<ContentWidth>();
        settings.DefaultScale = (int)_scaleNumeric.Value;

        settings.BodyFont = _bodyFontCombo.GetSelectedIndexAs<FontCategory>();

        settings.HeadingFont = _headFontCombo.GetSelectedIndexAs<FontCategory>();
        settings.HeadingForeground = _headColor.ChosenColor.ToUInt32();

        settings.DefaultFencedWrap = _fencedWrapSwitch.IsChecked;
        settings.FencedForeground = _fencedColor.ChosenColor.ToUInt32();

        settings.UserBaseColor = _userPicker.ChosenColor.ToUInt32();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void OnOpened()
    {
        base.OnOpened();
        _contentWidthCombo.ValueChanged += ControlValueChangedHandler;
        _scaleNumeric.ValueChanged += ControlValueChangedHandler;

        _bodyFontCombo.ValueChanged += ControlValueChangedHandler;

        _headFontCombo.ValueChanged += ControlValueChangedHandler;
        _headColor.ValueChanged += ControlValueChangedHandler;

        _fencedWrapSwitch.ValueChanged += ControlValueChangedHandler;
        _fencedColor.ValueChanged += ControlValueChangedHandler;

        _userPicker.ValueChanged += ControlValueChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void OnClosed()
    {
        base.OnClosed();
        s_isPreviewOpen = _previewGroup.IsOpen;
    }

    private sealed class Preview : Border
    {
        private readonly StackPanel _stack = new();
        private readonly TextBlock _block = new();

        private readonly MarkView _user;
        private readonly MarkView _assistant;

        public Preview()
        {
            var large = ChromeSizes.LargePadding;
            var tracker = new CrossTracker();

            _stack.Margin = new(large.Left, large.Top * 2.0, large.Right, 0.0);
            ClipToBounds = true;

            _user = new(tracker);
            _user.Content = "Lorem ipsum dolor sit amet?";
            _user.Padding = ChromeSizes.RegularPadding;
            _user.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            _stack.Children.Add(_user);

            _block.Foreground = ChromeStyling.ForegroundGray;
            _block.Text = "Assistant";
            _stack.Children.Add(_block);

            _assistant = new(tracker);
            _assistant.Content = GetAssistantContent();
            _stack.Children.Add(_assistant);

            Child = _stack;
        }

        public void Refresh(ContentSettings settings)
        {
            CornerRadius = Styling.SmallCornerRadius;

            // Not using
            // Background = Styling.Background;

            double s = settings.DefaultScale / 100.0;
            _block.FontSize = ChromeFonts.DefaultFontSize * s;
            _block.Margin = new(0.0, ChromeSizes.LargePadding.Top * s, 0.0, 0.0);
            _block.Foreground = ChromeStyling.ForegroundGray;

            RefreshMark(_user, settings, settings.ToUserBrush(Styling.AccentColor));
            RefreshMark(_assistant, settings);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            Refresh(ContentSettings.Global);
            Styling.StylingChanged += ChangedHandler;
            ContentSettings.Global.Changed += ChangedHandler;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            Styling.StylingChanged -= ChangedHandler;
            ContentSettings.Global.Changed -= ChangedHandler;
        }

        private static void RefreshMark(MarkControl mark, ContentSettings settings, IBrush? background = null)
        {
            mark.Background = background;
            mark.CornerRadius = Styling.SmallCornerRadius;
            mark.FencedBackground = Styling.BackgroundLow;
            mark.FencedCornerRadius = Styling.SmallCornerRadius;

            mark.FontFamily = settings.BodyFont.ToFamily();
            mark.FontSizeCorrection = settings.BodyFont.ToCorrection();
            mark.HeadingFamily = settings.HeadingFont.ToFamily();
            mark.HeadingForeground = settings.ToHeadingBrush();
            mark.FencedForeground = settings.ToFencedBrush();

            mark.Zoom.Scale = settings.DefaultScale;
        }

        private static string GetAssistantContent()
        {
            return @"# Lorem Ipsum
Proin porttitor, orci nec nonummy molestie, enim est eleifend mi, non fermentum diam nisl sit amet erat.
```bash
#!/usr/bin/env bash
echo 'Listing...'
ls -la /tmp```";
        }

        private void ChangedHandler(object? _, EventArgs __)
        {
            Refresh(ContentSettings.Global);
        }
    }
}