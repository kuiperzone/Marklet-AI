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
using Avalonia.Collections;
using Avalonia.Media;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Extends <see cref="PixieColorPicker"/> to display pre-defined accent colors.
/// </summary>
public class PixieAccentPicker : PixieColorPicker
{
    private static readonly List<Color> Palette0 = new();
    private static readonly List<Color> Palette1 = new();
    private static readonly List<Color> Palette2 = new();

    private bool _isSecondaryVisible;
    private bool _isTertiaryVisible;

    static PixieAccentPicker()
    {
        Palette0.Add(ChromeBrushes.BlueAccent.Color);
        Palette0.Add(ChromeBrushes.TealAccent.Color);
        Palette0.Add(ChromeBrushes.GreenAccent.Color);
        Palette0.Add(ChromeBrushes.YellowAccent.Color);
        Palette0.Add(ChromeBrushes.OrangeAccent.Color);
        Palette0.Add(ChromeBrushes.RedAccent.Color);
        Palette0.Add(ChromeBrushes.PinkAccent.Color);
        Palette0.Add(ChromeBrushes.PurpleAccent.Color);
        Palette0.Add(ChromeBrushes.SlateAccent.Color);

        Palette1.Add(ChromeBrushes.BlueLightAccent.Color);
        Palette1.Add(ChromeBrushes.TealLightAccent.Color);
        Palette1.Add(ChromeBrushes.GreenLightAccent.Color);
        Palette1.Add(ChromeBrushes.YellowLightAccent.Color);
        Palette1.Add(ChromeBrushes.OrangeLightAccent.Color);
        Palette1.Add(ChromeBrushes.RedLightAccent.Color);
        Palette1.Add(ChromeBrushes.PinkLightAccent.Color);
        Palette1.Add(ChromeBrushes.PurpleLightAccent.Color);
        Palette1.Add(ChromeBrushes.SlateLightAccent.Color);

        Palette2.Add(ChromeBrushes.BlueDarkAccent.Color);
        Palette2.Add(ChromeBrushes.TealDarkAccent.Color);
        Palette2.Add(ChromeBrushes.GreenDarkAccent.Color);
        Palette2.Add(ChromeBrushes.YellowDarkAccent.Color);
        Palette2.Add(ChromeBrushes.OrangeDarkAccent.Color);
        Palette2.Add(ChromeBrushes.RedDarkAccent.Color);
        Palette2.Add(ChromeBrushes.PinkDarkAccent.Color);
        Palette2.Add(ChromeBrushes.PurpleDarkAccent.Color);
        Palette2.Add(ChromeBrushes.SlateDarkAccent.Color);
    }

    /// <summary>
    /// Default construction.
    /// </summary>
    public PixieAccentPicker()
    {
        base.Palette.AddRange(Palette0);
    }

    /// <summary>
    /// Defines the <see cref="IsEditorVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieAccentPicker, bool> IsEditorVisibleProperty =
        AvaloniaProperty.RegisterDirect<PixieAccentPicker, bool>(nameof(IsEditorVisible),
        o => o.IsEditorVisible, (o, v) => o.IsEditorVisible = v, true);

    /// <summary>
    /// Defines the <see cref="IsSecondaryVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieAccentPicker, bool> IsSecondaryVisibleProperty =
        AvaloniaProperty.RegisterDirect<PixieAccentPicker, bool>(nameof(IsSecondaryVisible),
        o => o.IsSecondaryVisible, (o, v) => o.IsSecondaryVisible = v);

    /// <summary>
    /// Defines the <see cref="IsTertiaryVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<PixieAccentPicker, bool> IsTertiaryVisibleProperty =
        AvaloniaProperty.RegisterDirect<PixieAccentPicker, bool>(nameof(IsTertiaryVisible),
        o => o.IsTertiaryVisible, (o, v) => o.IsTertiaryVisible = v);

    /// <summary>
    /// Gets or sets whether the user can edit custom colors.
    /// </summary>
    /// <remarks>
    /// Default is true.
    /// </remarks>
    public bool IsEditorVisible
    {
        get { return IsEditorVisibleInternal; }
        set { IsEditorVisibleInternal = value; }
    }

    /// <summary>
    /// Gets or sets whether the secondary (lighter) palette is visible.
    /// </summary>
    /// <remarks>
    /// Default is false.
    /// </remarks>
    public bool IsSecondaryVisible
    {
        get { return _isSecondaryVisible; }
        set { SetAndRaise(IsSecondaryVisibleProperty, ref _isSecondaryVisible, value); }
    }

    /// <summary>
    /// Gets or sets whether the secondary (lighter) palette is visible.
    /// </summary>
    /// <remarks>
    /// Default is false.
    /// </remarks>
    public bool IsTertiaryVisible
    {
        get { return _isTertiaryVisible; }
        set { SetAndRaise(IsTertiaryVisibleProperty, ref _isTertiaryVisible, value); }
    }

    /// <summary>
    /// Do not use. The palette is fixed.
    /// </summary>
    [Obsolete($"Do not use. The palette is fixed.", true)]
    protected new AvaloniaList<Color> Palette
    {
        get { return base.Palette; }
    }

    /// <summary>
    /// Do not use. The palette is fixed.
    /// </summary>
    [Obsolete($"Do not use. The palette is fixed.", true)]
    protected new AvaloniaList<Color> SecondaryPalette
    {
        get { return base.SecondaryPalette; }
    }

    /// <summary>
    /// Do not use. The palette is fixed.
    /// </summary>
    [Obsolete($"Do not use. The palette is fixed.", true)]
    protected new AvaloniaList<Color> TertiaryPalette
    {
        get { return base.TertiaryPalette; }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var p = change.Property;

        if (p == IsSecondaryVisibleProperty)
        {
            if (change.GetNewValue<bool>())
            {
                base.SecondaryPalette.AddRange(Palette1);
            }
            else
            {
                base.SecondaryPalette.Clear();
            }

            return;
        }

        if (p == IsTertiaryVisibleProperty)
        {
            if (change.GetNewValue<bool>())
            {
                base.TertiaryPalette.AddRange(Palette2);
            }
            else
            {
                base.TertiaryPalette.Clear();
            }

            return;
        }

        base.OnPropertyChanged(change);
    }
}
