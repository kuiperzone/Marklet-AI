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

using System.Text.Json;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Settings;
using KuiperZone.Marklet.PixieChrome.Shared;
using KuiperZone.Marklet.Shared;

namespace KuiperZone.Marklet.Settings;

/// <summary>
/// Serializable "content" settings.
/// </summary>
public sealed class ContentSettings : SettingsBase, IEquatable<SettingsBase>
{
    /// <summary>
    /// Minimum legal <see cref="DefaultScale"/> value.
    /// </summary>
    public const int MinScale = Zoomer.MinScale;

    /// <summary>
    /// Maximum legal <see cref="DefaultScale"/> value.
    /// </summary>
    public const int MaxScale = Zoomer.MaxScale;

    /// <summary>
    /// Gets a global instance loaded on <see cref="App.Initialize"/>.
    /// </summary>
    public static ContentSettings Global { get; } = new();

    /// <summary>
    /// Gets or sets the content width.
    /// </summary>
    public ContentWidth Width { get; set; }

    /// <summary>
    /// Gets or sets the default scale as a percentage.
    /// </summary>
    public int DefaultScale { get; set; } = 100;

    /// <summary>
    /// Gets or sets the body font category.
    /// </summary>
    public FontCategory BodyFont { get; set; }

    /// <summary>
    /// Gets or sets the user background area.
    /// </summary>
    /// <remarks>
    /// A default 0 value will be interpreted as a default value.
    /// </remarks>
    public uint UserBaseColor { get; set; }

    /// <summary>
    /// Gets or sets the heading font category.
    /// </summary>
    public FontCategory HeadingFont { get; set; }

    /// <summary>
    /// Gets or sets the heading color.
    /// </summary>
    /// <remarks>
    /// A default 0 value will be interpreted as a default color.
    /// </remarks>
    public uint HeadingForeground { get; set; }

    /// <summary>
    /// Gets or sets the fenced code foreground color.
    /// </summary>
    /// <remarks>
    /// A default 0 value will be interpreted as a default color.
    /// </remarks>
    public uint FencedForeground { get; set; }

    /// <summary>
    /// Gets or sets the default line wrap state of fenced and indented code.
    /// </summary>
    public bool DefaultFencedWrap { get; set; }

    /// <inheritdoc cref="SettingsBase.Reset"/>
    public override void Reset()
    {
        CopyFrom(new());
    }

    /// <inheritdoc cref="SettingsBase.Deserialize(string)"/>
    public override void Deserialize(string json)
    {
        CopyFrom(JsonSerializer.Deserialize(json, AppSerializer.Default.ContentSettings));
    }

    /// <inheritdoc cref="SettingsBase.Serialize"/>
    public override string? Serialize()
    {
        return JsonSerializer.Serialize(this, AppSerializer.Default.ContentSettings);
    }

    /// <inheritdoc cref="SettingsBase.TrimLegal"/>
    public override void TrimLegal()
    {
        Width = Width.TrimLegal();
        DefaultScale = Math.Clamp(DefaultScale, MinScale, MaxScale);
        BodyFont = BodyFont.TrimLegal();
        HeadingFont = HeadingFont.TrimLegal();
    }

    /// <inheritdoc cref="SettingsBase.Equals(SettingsBase?)"/>
    public override bool Equals(SettingsBase? other)
    {
        return other is ContentSettings s &&
            Width == s.Width &&
            DefaultScale == s.DefaultScale &&
            BodyFont == s.BodyFont &&
            UserBaseColor == s.UserBaseColor &&
            HeadingFont == s.HeadingFont &&
            HeadingForeground == s.HeadingForeground &&
            FencedForeground == s.FencedForeground &&
            DefaultFencedWrap == s.DefaultFencedWrap;
    }

    /// <summary>
    /// Gets the <see cref="UserBaseColor"/> as color with defined default value.
    /// </summary>
    public Color ToUserColor(Color def)
    {
        return ToColor(UserBaseColor, def);
    }

    /// <summary>
    /// Create new background brush for <see cref="UserBaseColor"/>.
    /// </summary>
    public ImmutableSolidColorBrush ToUserBrush(Color def)
    {
        return new(ToUserColor(def), 0.25);
    }

    /// <summary>
    /// Gets the <see cref="HeadingForeground"/> as color with defined default value.
    /// </summary>
    public Color ToHeadingColor()
    {
        return ToColor(HeadingForeground, default);
    }

    /// <summary>
    /// Create new brush <see cref="HeadingForeground"/>.
    /// </summary>
    public ImmutableSolidColorBrush? ToHeadingBrush()
    {
        return ToBrush(HeadingForeground, null);
    }

    /// <summary>
    /// Gets the <see cref="FencedForeground"/> as color with defined default value.
    /// </summary>
    public Color ToFencedColor()
    {
        return ToColor(FencedForeground, default);
    }

    /// <summary>
    /// Create new brush <see cref="FencedForeground"/>.
    /// </summary>
    public ImmutableSolidColorBrush? ToFencedBrush()
    {
        return ToBrush(FencedForeground, null);
    }

    /// <summary>
    /// Copies setting from "other" to "this".
    /// </summary>
    public void CopyFrom(ContentSettings? other)
    {
        if (other != null)
        {
            Width = other.Width;
            DefaultScale = other.DefaultScale;
            BodyFont = other.BodyFont;
            UserBaseColor = other.UserBaseColor;
            HeadingFont = other.HeadingFont;
            HeadingForeground = other.HeadingForeground;
            FencedForeground = other.FencedForeground;
            DefaultFencedWrap = other.DefaultFencedWrap;
        }
    }

}
