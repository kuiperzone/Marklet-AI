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

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Presents and manages the collection of <see cref="GardenBasket"/> instances in conjunction with <see cref="BufferBar"/>.
/// </summary>
public sealed class MainMission : Border
{
    private static readonly MemoryGarden Garden = GardenGrounds.Global;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);
    private readonly Panel _panel = new();
    private readonly List<BasketView> _views = new(4);
    private readonly DispatchCoalescer _changeCoalescer = new(DispatcherPriority.Render);
    private readonly DispatcherTimer _refreshTimer = new();

    private BasketKind _basket;
    private GardenDeck? _pending;
    private BasketView? _currentView;
    private bool _newClicked;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MainMission()
    {
        base.Child = _panel;
        _panel.Margin = new(0.0, ChromeSizes.TwoCh, 0.0, 0.0);


        _basket = BasketKind.Recent;

        foreach (var item in MemoryGarden.LegalBaskets)
        {
            var view = new BasketView(this, item);
            view.IsVisible = item == _basket;

            _views.Add(view);
            _panel.Children.Add(view);
            view.ViewChanged += (_, __) => _changeCoalescer?.Post();

            if (item == _basket)
            {
                _currentView = view;
            }
        }

        _refreshTimer.Interval = RefreshInterval;
        _refreshTimer.Tick += (_, __) => _currentView?.RefreshVisual();
        _refreshTimer.Start();

        Garden.FocusChanged += (_, __) => _changeCoalescer?.Post();
        _changeCoalescer.Posted += PostedHandler;
    }

    /// <summary>
    /// Occurs immediately when <see cref="Basket"/> changes.
    /// </summary>
    public event EventHandler<BasketChangedEventArgs>? BasketChanged;

    /// <summary>
    /// Occurs when view is updated, including a change in selection.
    /// </summary>
    /// <remarks>
    /// This occurs only after any <see cref="GardenBasket.Changed"/> or <see cref="BasketChanged"/> event.
    /// </remarks>
    public event EventHandler<EventArgs>? ViewChanged;

    /// <summary>
    /// Occurs when the user clicks "new".
    /// </summary>
    /// <remarks>
    /// Always occurs after <see cref="ViewChanged"/> event.
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
                    }
                    else
                    {
                        item.IsVisible = false;
                    }
                }

                // Order important
                BasketChanged?.Invoke(this, new BasketChangedEventArgs(value));

                _currentView?.IsVisible = true;
            }
        }
    }

    /// <summary>
    /// Gets whether the prompter should be active.
    /// </summary>
    public bool CanReply
    {
        get { return _basket.CanReply() && (GetNew(false) != null || Garden.GetBasket(_basket).Contains(Garden.Focused)); }
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
    /// Does not use.
    /// </summary>
    [Obsolete($"Do not use.", true)]
    public new Control? Child
    {
        get { return base.Child; }
        set { base.Child = value; }
    }

    /// <summary>
    /// Instigates a new chat or note on the current <see cref="Basket"/>.
    /// </summary>
    public bool OnNewClicked(bool ephemeral = false)
    {
        if (_basket.CanInstigateNew() && _currentView != null)
        {
            _pending = new(_basket.DefaultDeck(), _basket, ephemeral);
            _pending.IsFocused = true;

            _currentView.IsSearching = false;
            Garden.Focused?.IsFocused = false;

            _newClicked = true;
            _changeCoalescer.Post();
            return true;
        }

        return false;
    }

    private void PostedHandler(object? _, EventArgs __)
    {
        GardenGrounds.IsMissionSearch = _currentView?.IsSearching == true;

        ViewChanged?.Invoke(this, EventArgs.Empty);

        if (_newClicked)
        {
            _newClicked = false;
            NewClicked?.Invoke(this, EventArgs.Empty);
        }
    }

}