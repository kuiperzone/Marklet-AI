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
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using KuiperZone.Marklet.PixieChrome.Internal;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Tooling;
using ReactiveUI;

namespace KuiperZone.Marklet.PixieChrome;
/// <summary>
/// Application level "styling" reactive model.
/// </summary>
/// <remarks>
/// There is only one instance of <see cref="ChromeStyling"/> as accessed through <see cref="Global"/>. This "model"
/// concerns itself with styling rather than function or logic. The class is partial so it may be separated into feature
/// areas.
/// </remarks>
public partial class ChromeStyling : ReactiveObject
{
    // Not possible to support more than a light and dark theme.
    // However, we can switch the dark theme style with this value.
    private bool _preferFluentDark;
    private DispatchCoalescer? _changePoster;
    private ChromeTheme _theme;
    private Application? _app;
    private AppearanceSettings? _settings;

    private ChromeStyling()
    {
    }

    /// <summary>
    /// Singleton and the only way to access an instance.
    /// </summary>
    public static ChromeStyling Global { get; } = new();

    /// <summary>
    /// Occurs on a change to <see cref="Theme"/>, <see cref="AccentBrush"/>, <see cref="Corners"/> or other properties
    /// which pertain to colors.
    /// </summary>
    public event EventHandler<EventArgs>? StylingChanged;

    /// <summary>
    /// Gets or sets the application specific theme variant.
    /// </summary>
    /// <remarks>
    /// Setting sets the underlying <see cref="Application"/> theme variant. The <see cref="StylingChanged"/> event occurs when
    /// <see cref="Application.ActualThemeVariant"/> changes.
    /// </remarks>
    public ChromeTheme Theme
    {
        get { return _theme; }

        set
        {
            const string NSpace = $"{nameof(ChromeStyling)}.{nameof(Theme)} setter";
            ConditionalDebug.WriteLine(NSpace, $"Current: {_theme}, New: {value}");

            _theme = value;
            _app?.RequestedThemeVariant = value.ToVariant();
        }
    }

    /// <summary>
    /// Creates a more traditional focus adorner using a Border and <see cref="AccentBrush"/>.
    /// </summary>
    public static FuncTemplate<Control> NewAdorner()
    {
        return new FuncTemplate<Control>(NewAdornerBorder);
    }

    /// <summary>
    /// To be called during Application.Initialize().
    /// </summary>
    /// <remarks>
    /// When supplied with "settings", styling will update when <see cref="SettingsBase.OnChanged(bool)"/> is called.
    /// Where "settings" is null, <see cref="AppearanceSettings.Global"/> is used.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Already initialized</exception>
    public void Initialize(Application app, AppearanceSettings? settings = null)
    {
        const string NSpace = $"{nameof(ChromeStyling)}.{nameof(Initialize)}";
        ConditionalDebug.WriteLine(NSpace, $"Requested theme: {app.RequestedThemeVariant}");
        ConditionalDebug.WriteLine(NSpace, $"Actual theme: {app.RequestedThemeVariant}");

        if (_app != null)
        {
            throw new InvalidOperationException("Already initialized");
        }

        _app = app;
        var pal = GetFluent(app).Palettes;
        _preferFluentDark = settings?.PreferFluentDark ?? false;

        // Re-initialize light and dark palettes
        pal.Clear();
        pal.Add(ThemeVariant.Light, ThemePalette.Get(ThemeVariant.Light, _preferFluentDark));
        pal.Add(ThemeVariant.Dark, ThemePalette.Get(ThemeVariant.Dark, _preferFluentDark));

        // Important to call these here as they won't get
        // called for light theme as the default actual is light.
        RaiseColorProperties(ThemePalette.Get(ThemeVariant.Light, _preferFluentDark));

        // Order important
        app.ActualThemeVariantChanged += ActualThemeVariantChangedHandler;
        ConditionalDebug.WriteLine(NSpace, $"Requested now: {app.RequestedThemeVariant}");
        ConditionalDebug.WriteLine(NSpace, $"Actual now: {app.ActualThemeVariant}");

        ConditionalDebug.WriteLine(NSpace, "Copy settings");
        _settings = settings ?? AppearanceSettings.Global;

        ApplySettings();
        _settings.Changed += (_, __) => ApplySettings();

        _changePoster = new();
        _changePoster.Posted += (_, __) => StylingChanged?.Invoke(this, EventArgs.Empty);
    }

    private static Border NewAdornerBorder()
    {
        var obj = new Border();
        obj.CornerRadius = Global.SmallCornerRadius;
        obj.BorderThickness = new(1.0);
        obj.BorderBrush = Global.AccentBrush;
        obj.Background = null;
        obj.IsHitTestVisible = false;
        return obj;
    }

    private FluentTheme GetFluent(Application app)
    {
        foreach (var style in app.Styles)
        {
            if (style is FluentTheme fluent)
            {
                return fluent;
            }
        }

        throw new ArgumentException($"{nameof(FluentTheme)} not found in application resources", nameof(app));
    }

    private void ApplySettings()
    {
        if (_settings != null)
        {
            // Call setters (not fields)
            Theme = _settings.Theme;
            AccentColor = _settings.ToAccentColor();
            Corners = _settings.Corners;
        }
    }

    private void OnStylingChanged(bool immediate = false)
    {
        if (_changePoster != null)
        {
            if (immediate)
            {
                StylingChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            _changePoster.Post();
        }
    }

    private void ActualThemeVariantChangedHandler(object? _, EventArgs __)
    {
        const string NSpace = $"{nameof(ChromeStyling)}.{nameof(ActualThemeVariantChangedHandler)}";
        ConditionalDebug.WriteLine(NSpace, $"AppTheme: {Theme}");

        var variant = _app!.ActualThemeVariant;
        ConditionalDebug.WriteLine(NSpace, $"Variant: {variant}");

        // Order important
        IsActualThemeDark = variant == ThemeVariant.Dark;
        RaiseColorProperties(ThemePalette.Get(variant, _preferFluentDark));

        if (_changePoster?.IsPending == false)
        {
            OnStylingChanged(true);
        }
    }
}
