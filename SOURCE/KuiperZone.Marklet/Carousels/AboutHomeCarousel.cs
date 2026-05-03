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

using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Carousels;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Windows;

namespace KuiperZone.Marklet.Carousels;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage"/>.
/// </summary>
public sealed class AboutHomeCarousel : CarouselPage
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutHomeCarousel(AboutWindow owner)
    {
        Title = "Home";
        Symbol = Symbols.Home;

        Children.Add(new PixieControl<Logo>());

        var group = new PixieGroup();
        Children.Add(group);

        var button = new PixieCard();
        button.Title = "Website";
        button.TitleWeight = FontWeight.Bold;
        button.RightSymbol = Symbols.OpenInNew;
        button.Click += (_, __) => owner.OnLink(App.WebUrl);
        group.Children.Add(button);

        button = new PixieCard();
        button.Title = "Public Repository";
        button.RightSymbol = Symbols.OpenInNew;
        button.Click += (_, __) => owner.OnLink(App.RepoUrl);
        group.Children.Add(button);

        button = new PixieCard();
        button.Title = "License: AGPL-3.0";
        button.RightSymbol = Symbols.OpenInNew;
        button.Click += (_, __) => owner.OnLink(new("app://license"));
        group.Children.Add(button);

        group = new PixieGroup();
        group.CollapseTitle = "Provisions & Acknowledgements";
        group.IsCollapsible = true;
        group.IsOpen = false;
        Children.Add(group);

        var provisions = new PixieMarkView();
        provisions.Subject.IsChromeStyled = true;
        provisions.Subject.Content = GetProvisionText();
        provisions.Subject.Tracker.LinkClick += owner.LinkClickHandler;
        group.Children.Add(provisions);
    }

    /// <summary>
    /// Occurs when license link clicked.
    /// </summary>
    public event EventHandler<EventArgs>? LicenseClick;

    private static string GetProvisionText()
    {
        return @"# Provisions
This application is free software that comes with absolutely no warranty. See the
[GNU Affero General Public License](app://license) for details.

## Native Desktop

**Marklet** is an open source alt-AI client primarily intended for use with local models (i.e. in conjunction with Ollama or a
similar runner). Its primary purpose is to do this simply and to do it well.

Marklet is a native desktop application and provides a near out-of-box experience, comprising everything except the
models and runner. It is clean, principled and designed for those who do not want bloat or web-UIs.

It prioritises the individual, and is *personal* in the way a PC was once personal.

## Security

Security is provided by virtue that, by default, all data is stored locally with the software intended for use against local models.
The software does not make special cryptographic provisions to protect local data. You must assume that someone with full access
to your device can read such data as they could with other desktop application data. Likewise, if you connect the application to a
remotely hosted service, you must assume that all ""chat messages"", attachments and all associated chat data will be read by such
services at some point.

Marklet is not the place to store sensitive information that requires protection beyond that afforded by a secure, local
environment under your own control. It may not be suitable for use in enterprise environments in which the individual does
not have personal autonomy.

## Image Generation

Marklet does not support image or video generation capabilities and is not designed to do so. Any text-to-speech
capability will be for accessibility only and not to mimic real human voices.

## Acknowledgements
The software leverages the following third-party libraries with thanks:

* Avalonia, AvaloniaUI OÜ
* Markdig, Alexandre Mutel
* Sqlite
";
    }

    private void LicenseClickHandler(object? _, EventArgs __)
    {
        LicenseClick?.Invoke(this, EventArgs.Empty);
    }

    private sealed class Logo : Grid
    {
        private readonly Image _image = new();
        private readonly TextBlock _name = new();
        private readonly TextBlock _version = new();
        private readonly TextBlock _copyright = new();
        private readonly Uri _logoUri = new($"avares://{typeof(Logo).Assembly.GetName().Name}/Assets/owl-marklet.png");

        public Logo()
        {
            RowDefinitions.Add(new(GridLength.Auto));
            RowDefinitions.Add(new(GridLength.Auto));

            _image.Source = new Bitmap(AssetLoader.Open(_logoUri));
            _image.Stretch = Stretch.Uniform;
            _image.Margin = new(0.0, 0.0, 125.0, 0.0);
            _image.MaxWidth = 500;
            SetRow(_image, 0);
            Children.Add(_image);

            var panel = new StackPanel();
            panel.Margin = new(0.0, 24.0, 0.0, 0.0);
            panel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            panel.Orientation = Avalonia.Layout.Orientation.Vertical;

            _name.Text = App.DisplayTitle.ToLowerInvariant();
            _name.FontFamily = AppFonts.VintageFamily;
            _name.FontSize = 38.0;
            _name.LetterSpacing = 1.6;
            panel.Children.Add(_name);

            _version.Text = App.DisplayVersion;
            _version.FontSize = ChromeFonts.LargeFontSize;
            _version.FontWeight = FontWeight.Bold;
            _version.TextAlignment = TextAlignment.Center;
            panel.Children.Add(_version);

            _copyright.Text = App.Copyright;
            _copyright.TextAlignment = TextAlignment.Center;
            _copyright.Margin = new(0.0, 6.0);
            panel.Children.Add(_copyright);

            SetRow(panel, 1);
            Children.Add(panel);
        }
    }
}