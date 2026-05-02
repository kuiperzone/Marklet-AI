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
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Shared;
using Avalonia.Threading;
using KuiperZone.Marklet.Controls.Internal.Mission;
using KuiperZone.Marklet.Shared;
using Avalonia.Input;
using Avalonia;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Presents and manages the collection of <see cref="GardenBasket"/> instances in conjunction with <see cref="BufferBar"/>.
/// </summary>
public sealed class MainMission : Border
{
    private static readonly MemoryGarden Garden = GlobalGarden.Global;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);
    private readonly Panel _panel = new();
    private readonly List<BasketView> _views = new(4);
    private readonly DispatchCoalescer _coalescer = new(DispatcherPriority.Render);
    private readonly DispatcherTimer _refreshTimer = new();

    private BasketKind _basket;
    private GardenDeck? _pending;
    private BasketView? _currentView;
    private bool _newClicked;

    private bool _isSearchButtonVisible;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MainMission()
    {
        base.Child = _panel;
        _panel.Margin = new(0.0, ChromeSizes.TwoCh, 0.0, 0.0);

        _basket = BasketKind.Recent;


        foreach (var item in Enum.GetValues<BasketKind>())
        {
            if (item.IsLegal())
            {
                var view = new BasketView(this, item);

                _views.Add(view);
                _panel.Children.Add(view);
                view.IsVisible = item == _basket;
                view.Changed += (_, __) => _coalescer?.Post();

                if (item == _basket)
                {
                    _currentView = view;
                }
            }
        }

        _refreshTimer.Interval = RefreshInterval;
        _refreshTimer.Tick += (_, __) => _currentView?.RefreshVisual();
        _refreshTimer.Start();

        Garden.FocusChanged += (_, __) => _coalescer?.Post();
        _coalescer.Posted += ChangePostedHandler;
    }

    /// <summary>
    /// Defines the <see cref="IsSearchButtonVisible"/> property.
    /// </summary>
    public static readonly DirectProperty<MainMission, bool> IsSearchButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<MainMission, bool>(nameof(IsSearchButtonVisible),
        o => o.IsSearchButtonVisible, (o, v) => o.IsSearchButtonVisible = v);

    /// <summary>
    /// Occurs when view is updated, including a change to <see cref="Basket"/>.
    /// </summary>
    public event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Occurs when the user clicks "new".
    /// </summary>
    /// <remarks>
    /// Always occurs after <see cref="Changed"/> event.
    /// </remarks>
    public event EventHandler<EventArgs>? NewClicked;

    /// <summary>
    /// Gets or sets the currently selected basket.
    /// </summary>
    public BasketKind Basket
    {
        get { return _basket; }

        set
        {
            if (_basket != value)
            {
                _refreshTimer?.Restart();
                _basket = value;
                _currentView = null;

                _pending = null;
                _newClicked = false;

                foreach (var item in _views)
                {
                    if (item.Kind == value)
                    {
                        _currentView = item;
                        continue;
                    }

                    item.IsVisible = false;
                }

                _currentView?.IsVisible = true;
                _coalescer.Post();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether individual search buttons are visible.
    /// </summary>
    public bool IsSearchButtonVisible
    {
        get { return _isSearchButtonVisible; }
        set { SetAndRaise(IsSearchButtonVisibleProperty, ref _isSearchButtonVisible, value); }
    }

    /// <summary>
    /// Gets or sets whether current view is searching.
    /// </summary>
    public bool IsSearching
    {
        get { return _currentView?.IsSearching == true; }
        set { _currentView?.IsSearching = value; }
    }

    /// <summary>
    /// Gets whether the prompter should be active.
    /// </summary>
    public bool CanReply
    {
        get { return _basket.CanReply() && (GetNew(false) != null || Garden[_basket].Contains(Garden.Focused)); }
    }

    /// <summary>
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Gets the pending new instance created, but not inserted, after the user clicks "new".
    /// </summary>
    public GardenDeck? GetNew(bool accept)
    {
        if (_basket.CanInstigateNew() && Garden.Focused == null && _currentView != null && !_currentView.IsSearching)
        {
            var p = _pending;

            if (accept)
            {
                _pending = null;
            }

            return p;
        }

        _pending = null;
        return null;
    }

    /// <summary>
    /// Handles button key gestures.
    /// </summary>
    public bool HandleKeyGesture(KeyEventArgs e)
    {
        return _currentView?.HandleKeyGesture(e) ?? false;
    }

    /// <summary>
    /// Instigates a new chat or note on the current <see cref="Basket"/>.
    /// </summary>
    public bool OnNewClick(bool ephemeral = false)
    {
        if (_basket.CanInstigateNew() && _currentView != null)
        {
            _pending = new(_basket.DefaultDeck(), _basket, ephemeral);
            _pending.IsFocused = true;

            _currentView.IsSearching = false;
            Garden.Focused?.IsFocused = false;

            _newClicked = true;
            _coalescer.Post();
            return true;
        }

        return false;
    }

    private void ChangePostedHandler(object? _, EventArgs __)
    {
        GlobalGarden.IsMissionSearch = IsSearching;

        Changed?.Invoke(this, EventArgs.Empty);

        if (_newClicked)
        {
            _newClicked = false;
            NewClicked?.Invoke(this, EventArgs.Empty);
        }
    }

}