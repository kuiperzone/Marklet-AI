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
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;

namespace KuiperZone.Marklet.PixieChrome.Carousels;

/// <summary>
/// Base class for control sequences (pages) to be shown in <see cref="CarouselDialog"/> or <see
/// cref="CarouselControl"/>.
/// </summary>
/// <remarks>
/// A <see cref="CarouselPage"/> is not a window or control itself. Rather, it is collection of controls to be
/// shown as "pages".
/// </remarks>
public class CarouselPage
{
    /// <summary>
    /// Convenient access to <see cref="ChromeStyling.Global"/>.
    /// </summary>
    protected static readonly ChromeStyling Styling = ChromeStyling.Global;

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional symbol character in Material Symbol private range.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Gets or sets optional title footer text.
    /// </summary>
    public string? Footer { get; set; }

    /// <summary>
    /// Gets or sets whether this page starts a new section in a sequence.
    /// </summary>
    /// <remarks>
    /// Where true, a dividing line is drawn in the left-hand panel of <see cref="CarouselDialog"/> provided the page is
    /// not the first.
    /// </remarks>
    public bool IsSectionStart { get; set; }

    /// <summary>
    /// Gest a lits of controls displayed vertically in a scroll panel.
    /// </summary>
    /// <remarks>
    /// The subclass or class should populate this. It is anticipated that these be instances of <see
    /// cref="PixieGroup"/>, where each itself contains instances of <see cref="PixieControl"/>. Controls will be
    /// visibly separated.
    /// </remarks>
    public List<Control> Children { get; } = new();

    /// <summary>
    /// Gets whether <see cref="Activate"/> has been called.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indicates that controls should update underlying settings immediately on change.
    /// </summary>
    public bool IsFluid { get; private set; }

    /// <summary>
    /// Internal use.
    /// </summary>
    internal event EventHandler<EventArgs>? IndexClick;

    /// <summary>
    /// Internal use.
    /// </summary>
    internal int PageIndex { get; private set; }

    /// <summary>
    /// Internal use.
    /// </summary>
    internal PixieCard? IndexCard { get;private set;  }

    /// <summary>
    /// Internal use.
    /// </summary>
    internal Border? IndexDivider { get; private set; }

    /// <summary>
    /// Sets <see cref="IsActive"/> to true and <see cref="IsFluid"/> to the value supplied, informing the subclass
    /// that page controls are to be operational.
    /// </summary>
    /// <remarks>
    /// The method is called by <see cref="CarouselDialog"/>, for example, when the window opens. This is no need update
    /// the IsVisible property of individual controls. There is to be a corresponding call to <see cref="Deactivate"/> for
    /// each <see cref="Activate"/>. Subclass to override, as applicable, and call base method.
    /// </remarks>
    public virtual void Activate(bool fluid)
    {
        IsActive = true;
        IsFluid = fluid;
    }

    /// <summary>
    /// Sets <see cref="IsActive"/> false and indicates that page controls are dormant.
    /// </summary>
    /// <remarks>
    /// The method is called by <see cref="CarouselDialog"/>, for example, when the window closes. There is to be a
    /// corresponding call to <see cref="Deactivate"/> for each <see cref="Activate"/>. Subclass to override, as
    /// applicable, and call base method.
    /// </remarks>
    public virtual void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Informs the subclass to apply settings in response to a possible "Apply" button click.
    /// </summary>
    /// <remarks>
    /// The base method does nothing, but subclass may override. Typically, no action would be taken where <see
    /// cref="IsFluid"/> is true, as "applies" are to happen directly on control value change.
    /// </remarks>
    public virtual void Apply()
    {
    }

    /// <summary>
    /// Internal use. Returns true on first call.
    /// </summary>
    internal bool Init(int index, IEnumerable<string>? classes)
    {
        bool result = false;
        PageIndex = index;

        if (IndexCard == null)
        {
            result = true;
            IndexCard = new();
            IndexCard.Click += (_, __) => IndexClick?.Invoke(this, EventArgs.Empty);

            foreach (var c in Children)
            {
                if (c is PixieGroup group && classes != null)
                {
                    group.Classes.AddRange(classes);
                }
            }
        }

        IndexCard.Title = Title;
        IndexCard.LeftSymbol = Symbol;
        IndexDivider = (IsSectionStart && index > 0) ? new DividerEntry() : null;
        return result;
    }

    private sealed class DividerEntry : Border
    {
        public DividerEntry()
        {
            Height = 1.0;
            Margin = ChromeSizes.StandardPadding;
            Background = Styling.BorderBrush;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ChromeStyling.Global.StylingChanged += StylingChangedHandler;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            ChromeStyling.Global.StylingChanged -= StylingChangedHandler;
        }

        private void StylingChangedHandler(object? _, EventArgs __)
        {
            Background = Styling.BorderBrush;
        }
    }
}
