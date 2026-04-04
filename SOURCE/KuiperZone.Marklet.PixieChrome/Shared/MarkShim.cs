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
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Controls.Internal;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Shared;

/// <summary>
/// Helper class used with <see cref="MarkView"/> and related classes.
/// </summary>
public sealed class MarkShim
{
    // Intended for textual display rather than Control content.
    private const double LineHeightF = 1.8;
    private const double DefaultLetterSpacing = 1.3;

    /// <summary>
    /// Constructor with owner and children instance.
    /// </summary>
    /// <remarks>
    /// The instance will managed "children".
    /// </remarks>
    public MarkShim(MarkControl owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public readonly MarkControl Owner;

    /// <summary>
    /// Gets or sets a foreground brush override.
    /// </summary>
    public IBrush? ForegroundOverride { get; set; }

    /// <summary>
    /// Gets the actual foreground to use.
    /// </summary>
    /// <remarks>
    /// The value is <see cref="ForegroundOverride"/> where <see cref="ForegroundOverride"/> is not null, otherwise the
    /// owner Foreground.
    /// </remarks>
    public IBrush? ActualForeground
    {
        get { return ForegroundOverride ?? Owner.Foreground; }
    }

    /// <summary>
    /// Gets a scaled font size.
    /// </summary>
    public double FontSize
    {
        get { return Owner.FontSize * Owner.Zoom.Fraction; }
    }

    /// <summary>
    /// Gets a scaled line-height.
    /// </summary>
    public double LineHeight
    {
        get { return Owner.FontSize * Owner.Zoom.Fraction * LineHeightF; }
    }

    /// <summary>
    /// Gets a scaled pixel width very approx equal to one character width.
    /// </summary>
    public double OneCh
    {
        get { return Owner.FontSize * Owner.Zoom.Fraction * 0.5; }
    }

    /// <summary>
    /// Gets an indentation width roughly equivalent to four scaled spaces.
    /// </summary>
    public double TabPx
    {
        get { return Owner.FontSize * Owner.Zoom.Fraction * 0.5 * 4.0; }
    }

    /// <summary>
    /// Gets a scaled letter-spacing.
    /// </summary>
    public double LetterSpacing
    {
        get { return DefaultLetterSpacing * Owner.Zoom.Fraction; }
    }

    /// <summary>
    /// Gets a scaled line pixel width which is equal to 1.0 at 100% scale.
    /// </summary>
    public double LinePixels
    {
        get { return Math.Max(Owner.Zoom.Fraction, 0.8); }
    }

    /// <summary>
    /// Gets a scaled width of a "rule" line.
    /// </summary>
    public double RulePixels
    {
        get { return Math.Max(Owner.Zoom.Fraction * 1.5, 1.0); }
    }

}