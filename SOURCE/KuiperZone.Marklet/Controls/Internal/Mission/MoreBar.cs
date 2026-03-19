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

using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using Avalonia.Threading;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Contains "See more" and "goto top" for <see cref="BasketView"/>.
/// </summary>
internal sealed class MoreBar : DockPanel
{
    private const string SeeMoreText = "See more\u2026";
    private readonly LightButton _moreButton = new();
    private readonly LightButton? _alphaButton;
    private readonly LightButton? _topButton;
    private bool _isMoreChecked;

    /// <summary>
    /// Constructor.
    /// </summary>
    public MoreBar(bool isRootHistory)
    {
        IsFolderHistory = !isRootHistory;
        IsRootHistory = isRootHistory;
        Margin = ChromeSizes.LargeTopBottom;

        _moreButton.IsVisible = false;

        _moreButton.MinHeight = 0;
        _moreButton.FontSize = ChromeFonts.SmallFontSize;
        _moreButton.Content = SeeMoreText;
        _moreButton.IsChecked = true;
        _moreButton.Classes.Add("accent-checked");
        _moreButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        _moreButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

        Children.Add(_moreButton);
        SetDock(_moreButton, Dock.Left);

        _moreButton.Click += (_, __) => IsMoreChecked = !IsMoreChecked;

        if (isRootHistory)
        {
            _topButton = new();
            _topButton.Content = Symbols.ArrowWarmUp;
            _topButton.Tip = "Got to top";
            _topButton.IsVisible = false;
            _topButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            _topButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            Children.Add(_topButton);
            SetDock(_topButton, Dock.Right);

            _topButton.Click += (_, __) => GotoTopClick?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _alphaButton = new();
            _alphaButton.Content = Symbols.SortByAlpha;
            _alphaButton.Tip = "Sort folders alphabetically";
            _alphaButton.IsVisible = false;
            _alphaButton.Classes.Add("accent-checked");
            _alphaButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            _alphaButton.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            Children.Add(_alphaButton);
            SetDock(_alphaButton, Dock.Right);

            _alphaButton.Click += (_, __) => SetAlphaChecked(!IsAlphaFolderSort, true);
        }
    }

    /// <summary>
    /// Occurs when user clicks "goto top".
    /// </summary>
    public event EventHandler<EventArgs>? GotoTopClick;

    /// <summary>
    /// Occurs when the owner needs to update the view.
    /// </summary>
    public event EventHandler<EventArgs>? ViewChangeRequired;

    /// <summary>
    /// Gets whether related to folder history.
    /// </summary>
    public bool IsFolderHistory { get; }

    /// <summary>
    /// Gets whether related to root history.
    /// </summary>
    public bool IsRootHistory { get; }

    /// <summary>
    /// Gets or sets whether the "See all/more" button is visible.
    /// </summary>
    public bool IsMoreVisible
    {
        get { return _moreButton.IsVisible; }

        set
        {
            if (_moreButton.IsVisible != value)
            {
                // Looses state when changed
                _moreButton.IsVisible = value;
                IsMoreChecked = false;

                if (_alphaButton != null)
                {
                    _alphaButton.IsVisible = false;
                    _alphaButton.IsChecked = false;
                }
            }
        }
    }

    /// <summary>
    /// Gets whether "show more" is checked.
    /// </summary>
    public bool IsMoreChecked
    {
        get { return _isMoreChecked; }

        set
        {
            if (_isMoreChecked != value)
            {
                _isMoreChecked = value;

                if (!value)
                {
                    IsAlphaFolderSort = false;
                }
                DispatcherTimer.RunOnce(() => SetDelayedMore(value), BasketCase.ShortInterval);
                ViewChangeRequired?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets whether folder alpha sort is checked where <see cref="IsFolderHistory"/> is true.
    /// </summary>
    public bool IsAlphaFolderSort { get; private set; }

    /// <summary>
    /// Gets whether folder alpha sort is visible.
    /// </summary>
    public bool IsAlphaVisible
    {
        get { return _alphaButton?.IsVisible == true; }
    }

    /// <summary>
    /// Gets or sets whether "goto top" is visible (does nothing for folder sort).
    /// </summary>
    public bool IsGotoTopVisible
    {
        get { return _topButton?.IsVisible == true; }
        set { _topButton?.IsVisible = value; }
    }

    /// <summary>
    /// Sets <see cref="IsAlphaFolderSort"/> (does nothing unless <see cref="IsFolderHistory"/> is true).
    /// </summary>
    private bool SetAlphaChecked(bool value, bool raise)
    {
        if (IsAlphaFolderSort != value && _alphaButton?.IsVisible == true)
        {
            IsAlphaFolderSort = value;
            _alphaButton.IsChecked = value;

            if (raise)
            {
                ViewChangeRequired?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }

        return false;
    }

    private void SetDelayedMore(bool more)
    {
        if (more)
        {
            // Button checked state actually reverse of public property
            _moreButton.IsChecked = false;
            _moreButton.Content = "See less";
        }
        else
        {
            _moreButton.IsChecked = true;
            _moreButton.Content = SeeMoreText;
        }

        if (_alphaButton != null)
        {
            _alphaButton.IsVisible = more;

            if (!more)
            {
                // Silent off
                _alphaButton.IsChecked = false;
            }

        }
    }
}