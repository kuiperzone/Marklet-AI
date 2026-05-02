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

using KuiperZone.Marklet.Shared;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Settings;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Carousels;
using Avalonia.Controls;
using Avalonia;

namespace KuiperZone.Marklet.Carousels;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage{T}"/> which hosts <see cref="ContentSettings"/>.
/// </summary>
public sealed class ContentCarousel : CarouselPage<ContentSettings>
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
    public ContentCarousel()
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
        _scaleNumeric.RepeatInterval = 500;
        _scaleNumeric.Increment = 5;
        _scaleNumeric.AcceptFractionInput = false;
        _scaleNumeric.Units = "%";
        _scaleNumeric.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_scaleNumeric);

        _contentWidthCombo.Title = "Content Width";
        _contentWidthCombo.IsTranslateFriendly = true;
        _contentWidthCombo.LeftSymbol = Symbols.FitPageWidth;
        _contentWidthCombo.Footer = "Content display width";
        _contentWidthCombo.SetItemsAs<ContentWidth>();
        _contentWidthCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_contentWidthCombo);


        // PREVIEW
        group = _previewGroup;
        group.TopTitle = "Preview";
        group.IsCollapsible = true;
        group.CollapseTitleWeight = FontWeight.Bold;
        group.IsOpen = s_isPreviewOpen;
        Children.Add(group);

        group.Children.Add(_previewer);


        // BODY
        group = new PixieGroup();
        group.TopTitle = "Body Text";
        Children.Add(group);

        _bodyFontCombo.Title = "Default Font";
        _bodyFontCombo.IsTranslateFriendly = true;
        _bodyFontCombo.LeftSymbol = Symbols.FontDownload;
        _bodyFontCombo.Footer = "Font style for default content text";
        _bodyFontCombo.SetItemsAs<FontCategory>();
        _bodyFontCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_bodyFontCombo);


        // HEADINGS
        group = new PixieGroup();
        group.TopTitle = "Headings";
        Children.Add(group);

        _headFontCombo.Title = "Heading Font";
        _headFontCombo.IsTranslateFriendly = true;
        _headFontCombo.LeftSymbol = Symbols.FormatH1;
        _headFontCombo.Footer = "Font style for headings";
        _headFontCombo.SetItemsAs<FontCategory>();
        _headFontCombo.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_headFontCombo);

        _headColor.Header = "Foreground Color";
        _headColor.IsSecondaryVisible = true;
        _headColor.IsDefaultColorVisible = true;
        _headColor.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_headColor);


        // CODE
        group = new PixieGroup();
        group.TopTitle = "Fenced Code";
        Children.Add(group);

        _fencedColor.Header = "Foreground Color";
        _fencedColor.Footer = "Default foreground color fenced code";
        _fencedColor.IsSecondaryVisible = true;
        _fencedColor.IsDefaultColorVisible = true;
        _fencedColor.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_fencedColor);

        _fencedWrapSwitch.Title = "Default Code Wrap";
        _fencedWrapSwitch.LeftSymbol = Symbols.WrapText;
        _fencedWrapSwitch.Footer = "Fenced and indented code line wraps by default when checked";
        _fencedWrapSwitch.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_fencedWrapSwitch);


        // USER
        group = new PixieGroup();
        group.TopTitle = "User Background";
        Children.Add(group);

        _userPicker.Header = "Base Color";
        _userPicker.Footer = "Base color for user message background. Follows accent color by default.";
        _userPicker.IsDefaultColorVisible = true;
        _userPicker.DefaultColorLabel = "Accent";
        _userPicker.ValueChanged += (_, __) => OnValueChanged();
        group.Children.Add(_userPicker);


        // RESET
        Children.Add(CreateResetGroup());
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

        _previewer.Subject.Refresh(settings);
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
    public override void Deactivate()
    {
        base.Deactivate();
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
            var tracker = new CrossTracker(_stack);

            _stack.Margin = new(large.Left, large.Top * 2.0, large.Right, 0.0);
            ClipToBounds = true;

            _user = new(tracker);
            _user.Content = "Lorem ipsum dolor sit amet?";
            _user.Padding = ChromeSizes.StandardPadding;
            _user.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            _stack.Children.Add(_user);

            _block.Foreground = ChromeStyling.GrayForeground;
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
            _block.Foreground = ChromeStyling.GrayForeground;

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
            mark.CornerRadius = Styling.SmallCornerBottom;
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