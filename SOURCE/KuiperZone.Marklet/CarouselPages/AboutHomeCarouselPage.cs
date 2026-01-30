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

using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.CarouselPages;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using KuiperZone.Marklet.Shared;

namespace KuiperZone.Marklet.CarouselPages;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage"/>.
/// </summary>
public sealed class AboutHomeCarouselPage : CarouselPage
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutHomeCarouselPage(AboutWindow owner)
    {
        Title = "Home";
        Symbol = Symbols.Home;

        Children.Add(new PixieControl<Logo>());

        var group = new PixieGroup();
        Children.Add(group);

        var button = new PixieButton();
        button.Title = "Website";
        button.TitleWeight = FontWeight.Bold;
        button.RightSymbol = Symbols.OpenInNew;
        button.BackgroundClick += (_, __) => owner.OnLink(App.WebUrl);
        group.Children.Add(button);

        button = new PixieButton();
        button.Title = "Public Repository";
        button.RightSymbol = Symbols.OpenInNew;
        button.BackgroundClick += (_, __) => owner.OnLink(App.RepoUrl);
        group.Children.Add(button);

        button = new PixieButton();
        button.Title = "X/Twitter";
        button.RightSymbol = Symbols.OpenInNew;
        button.BackgroundClick += (_, __) => owner.OnLink(App.XUrl);
        group.Children.Add(button);

        button = new PixieButton();
        button.Title = "License: AGPL-3.0";
        button.RightSymbol = Symbols.OpenInNew;
        button.BackgroundClick += (_, __) => owner.OnLink(new("app://license"));
        group.Children.Add(button);

        group = new PixieGroup();
        group.CollapseTitle = "Provisions & Acknowledgements";
        group.IsCollapsable = true;
        group.IsOpen = false;
        Children.Add(group);

        var provisions = new PixieMarkView();
        provisions.ChildControl.IsChromeStyled = true;
        provisions.ChildControl.Content = GetProvisionText();
        provisions.ChildControl.LinkClick += owner.LinkClickHandler;
        group.Children.Add(provisions);
    }

    /// <summary>
    /// Occurs when license link clicked.
    /// </summary>
    public event EventHandler<EventArgs>? LicenseClick;

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void OnOpened()
    {
        base.OnOpened();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override void OnClosed()
    {
        base.OnClosed();
    }

    private static string GetProvisionText()
    {
        return @"# Provisions
This application is free software that comes with absolutely no warranty. See the
[GNU Affero General Public License](app://license) for details.

## Native desktop

Marklet is a traditional desktop application rather than a web or browser application. Its primary purpose is to provide a
native desktop interface (a viewer) to local AI models, while doing it simply and doing it well. It does not generate AI responses
itself.

While it may facilitate connection to externally hosted services, the design of the software will always prioritise a single
end-user employing local models and local data. It is unlikely to be suitable for use in enterprise environments.

## Security

Security is provided by virtue that, by default, all data is stored locally with the software intended for use against local models.
The software does not make special cryptographic provisions to protect local data. You must assume that someone with full access
to your device can read such data as they could with other desktop application data. Likewise, if you connect the application to a
remotely hosted service, you must assume that all ""chat messages"", attachments and all associated chat data will be read by such
remote services at some point. Any technical measure or option that may be provided to limit or restrict this will be
""best efforts"" only and likely to be imperfect.

Marklet is not the place to store sensitive information that requires protection beyond that afforded by a local environment
under your own control.

## Image Generation

Marklet does not support generative image or generative video capabilities and was never designed to do so. Any text-to-speech
capability will be for accessibility only and not to mimic real human voices.

## Hallucinations & Romantic Attachments
It is well known that AI makes mistakes and ""hallucinates"" output. You must assess for yourself its accuracy and all consequences
of use. If you do not feel competent to use AI applications, you should not do so. If you seek romantic attachment to AI, that
is entirely up to you, but it is suggested that you should not.

## Acknowledgements
The software leverages the following third-party libraries with thanks:

* Avalonia, AvaloniaUI OÜ
* Markdig, Alexandre Mutel
* Sqlite

The Marklet code base is an original work that was created using native human thought. It has not been ""vibe-coded"".";
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

        public Logo()
        {
            RowDefinitions.Add(new(GridLength.Auto));
            RowDefinitions.Add(new(GridLength.Auto));

            var uri = new Uri("avares://KuiperZone.Marklet/Assets/owl-marklet.png");
            _image.Source = new Bitmap(AssetLoader.Open(uri));
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

            _version.Text = App.Version.ToString(App.VersionParts);
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