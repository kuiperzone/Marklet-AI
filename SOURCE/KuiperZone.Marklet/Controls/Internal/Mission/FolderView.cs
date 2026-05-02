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
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Media;
using KuiperZone.Marklet.Shared;
using Avalonia.LogicalTree;

namespace KuiperZone.Marklet.Controls.Internal.Mission;

/// <summary>
/// Container of <see cref="PixieGroup"/> intended to hold instances of <see cref="DeckCard"/> for use with <see
/// cref="BasketView"/>.
/// </summary>
internal sealed class FolderView : Border, IDeckDrop
{
    private static readonly MemoryGarden Garden = GlobalGarden.Global;
    private readonly ChromeStyling Styling = ChromeStyling.Global;
    private readonly PixieGroup _group = new();
    private List<GardenDeck>? _source;
    private IDeckDrop? _dropTarget;
    private bool _isSearching;
    private bool _isNewlyCreated = true;

    /// <summary>
    /// Constructor.
    /// </summary>
    public FolderView(BasketView owner, bool isRoot)
    {
        Owner = owner;
        IsRootFolder = isRoot;
        Updated = Created;
        Background = DefaultBackground;

        Child = _group;

        if (isRoot)
        {
            MinHeight = ChromeSizes.GiantPx * 1.5;
        }


        _group.IsCollapseHoverButton = true;
        _group.ChildRenaming += ChildRenamingHandler;
    }

    /// <summary>
    /// Child folder constructor.
    /// </summary>
    public FolderView(BasketView owner, string folderName)
        : this(owner, false)
    {
        const string NSpace = $"{nameof(FolderView)}.constructor";
        Diag.WriteLine(NSpace, folderName);

        _group.CollapseTitle = folderName;
        _group.IsCollapsible = true;
        _group.IsOpen = false;
        _group.ChildIndent = ChromeSizes.OneCh * 3.0;
        _group.CollapseDropMenu = FolderMenu.Get(owner.Kind);
        _group.CollapseRenaming += GroupRenamingHandler;
        _group.PropertyChanged += GroupPropertyChangedHandler;
    }

    /// <summary>
    /// Gets the default background.
    /// </summary>
    public static readonly IBrush DefaultBackground = ChromeBrushes.Transparent;

    /// <summary>
    /// Timestamp this visual item was created.
    /// </summary>
    public readonly DateTime Created = DateTime.UtcNow;

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public BasketView Owner { get; }

    /// <summary>
    /// Gets whether this is the untagged root.
    /// </summary>
    public bool IsRootFolder { get; }

    /// <summary>
    /// Gets the most recent <see cref="GardenDeck.Updated"/> value.
    /// </summary>
    public DateTime Updated { get; private set; }

    /// <summary>
    /// Gets or sets the folder title.
    /// </summary>
    /// <remarks>
    /// Expected to be null where <see cref="IsRootFolder"/> equals true.
    /// </remarks>
    public string? FolderName
    {
        get { return _group.CollapseTitle; }
        set { _group.CollapseTitle = value; }
    }

    /// <summary>
    /// Gets or sets whether the folder is open.
    /// </summary>
    /// <remarks>
    /// Expected to be true where <see cref="IsRootFolder"/> equals true.
    /// </remarks>
    public bool IsOpen
    {
        get { return _group.IsOpen; }

        set
        {
            Diag.ThrowIfTrue(IsRootFolder && !value);
            _group.IsOpen = value;
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(PixieGroup);

    /// <inheritdoc cref="IDeckDrop.CanDrop"/>
    public bool CanDrop(object obj)
    {
        return GetDropDeck(obj) != null;
    }

    /// <inheritdoc cref="IDeckDrop.CancelDrop"/>
    public void CancelDrop()
    {
        Background = DefaultBackground;
    }

    /// <inheritdoc cref="IDeckDrop.StartDrop"/>
    public void StartDrop(object obj)
    {
        if (GetDropDeck(obj) != null)
        {
            Background = ChromeBrushes.Highlight;
        }
    }

    /// <inheritdoc cref="IDeckDrop.AcceptDrop"/>
    public bool AcceptDrop(object obj)
    {
        CancelDrop();
        var deck = GetDropDeck(obj);

        if (deck != null)
        {
            // That's it. The event handlers will take care of themselves.
            deck.Folder = FolderName;
            deck.VisualSignals = SignalFlags.OpenFolder;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clear visual children.
    /// </summary>
    public void Clear()
    {
        CancelDrop();
        _group.Children.Clear();
    }

    /// <summary>
    /// Returns true if the folder contains the given <see cref="DeckCard"/> instance.
    /// </summary>
    public bool Contains(DeckCard? obj)
    {
        Diag.ThrowIfTrue(obj?.Group == _group && !_group.Children.Contains(obj));
        Diag.ThrowIfTrue(obj != null && obj.Group == null && _group.Children.Contains(obj));
        return obj?.Group == _group;
    }

    /// <summary>
    /// Start visual renaming.
    /// </summary>
    public bool StartRename()
    {
        Diag.ThrowIfTrue(IsRootFolder);
        return _group.StartRename(MemoryGarden.MaxMetaLength);
    }

    /// <summary>
    /// Start rebuild using supplied sorted list.
    /// </summary>
    public void StartRebuild(List<GardenDeck> source, bool isSearching)
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(StartRebuild)}";
        Diag.WriteLine(NSpace, $"ROOT FOLDER: {IsRootFolder}");
        Diag.WriteLine(NSpace, $"Source count: {source.Count}");

        _source = null;
        _isSearching = isSearching;

        if (source.Count != 0)
        {
            _source = source;
            _isNewlyCreated = false;

            if (source[0].Updated > Created)
            {
                Updated = source[0].Updated;
            }

        }
    }

    /// <summary>
    /// Complete rebuild.
    /// </summary>
    public GardenDeck? FinishRebuild()
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(FinishRebuild)}";
        Diag.WriteLine(NSpace, $"ROOT FOLDER: {IsRootFolder}");

        var source = _source;
        _source = null;

        DeckCard? newCard = null;
        var folderFlags = SignalFlags.None;

        GardenDeck? result = null;
        GardenDeck? recent = Owner.Basket.RecentFocused;

        var count = source?.Count ?? 0;
        var buffer = new List<Control>(count);

        for(int n = 0; n < count; ++n)
        {
            var item = source![n];
            Diag.WriteLine(NSpace, $"CARD TITLE: {item.Title}");

            // If Owner is null, it means we have stale copy that
            // has been removed from the database. It shouldn't happen.
            var garden = item.Garden;
            Diag.ThrowIfNull(garden);

            if (garden == null)
            {
                continue;
            }

            // Get these first
            // They are reset by RefreshVisual()
            folderFlags |= item.VisualSignals;

            // See if it holding a visual object which is our child.
            // This saves us looking it up. If not, we create a new one below.
            if (item.VisualComponent is DeckCard exist && exist.Group == _group)
            {
                // This is already ours.
                Diag.WriteLine(NSpace, $"Refresh: {item.Title}");
                buffer.Add(exist);
                exist.RefreshVisual(_isSearching);

                if (item == recent)
                {
                    result = item;
                }
                else
                {
                    exist.IsChecked = false;
                }

                continue;
            }

            Diag.WriteLine(NSpace, "NEW CARD");
            newCard = new DeckCard(item, IsRootFolder);
            newCard.RefreshVisual(_isSearching);
            buffer.Add(newCard);

            if (item == recent)
            {
                result = item;
            }
        }

        // This should be efficient where references are unchanged.
        // It will avoid pulling things in or out of the tree where possible.
        _group.Children.Replace(buffer);
        Diag.WriteLine(NSpace, $"Buffer count: {buffer.Count}");
        Diag.WriteLine(NSpace, $"Children count: {_group.Children.Count}");
        Diag.ThrowIfNotEqual(_group.Children.Count, buffer.Count);

        if (!IsRootFolder)
        {
            if (folderFlags.HasFlag(SignalFlags.OpenFolder))
            {
                _group.IsOpen = true;
            }

            if (_isNewlyCreated || folderFlags.HasFlag(SignalFlags.FolderAttention))
            {
                _group.Attention(ChromeStyling.PixieHover);
            }
        }

        RefreshHeader();
        _isNewlyCreated = false;

        // Recent
        return result;
    }

    /// <summary>
    /// Refresh existing <see cref="DeckCard"/> instances only.
    /// </summary>
    public void RefreshVisual(bool isSearch)
    {
        RefreshGroup();

        foreach (var item in _group.Children)
        {
            if (item is DeckCard card)
            {
                card.RefreshVisual(isSearch);
            }
        }
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Styling.StylingChanged += StylingChangedHandler;

        RefreshGroup();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Styling.StylingChanged -= StylingChangedHandler;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);

        if (!e.Handled && point.Properties.IsRightButtonPressed)
        {
            // Right click
            var v = this.GetVisualAt(point.Position);

            if (CardMenu.OpenAt(v?.FindLogicalAncestorOfType<DeckCard>(true)))
            {
                return;
            }

            if (!IsRootFolder)
            {
                var folder = v?.FindLogicalAncestorOfType<FolderView>(true);

                if (folder != null && FolderMenu.OpenAt(folder.Owner.Kind, this))
                {
                    return;
                }
            }
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(OnPointerExited)}";
        base.OnPointerExited(e);

        if (!IsRootFolder)
        {
            var info = e.GetCurrentPoint(this);
            var props = info.Properties;

            if (props.IsLeftButtonPressed && e.Pointer.Captured is not DeckCard)
            {
                Diag.WriteLine(NSpace, "Capture");
                e.Pointer.Capture(this);
                Cursor = Styling.IsActualThemeDark ? ChromeCursors.FolderDark48 : ChromeCursors.FolderLight48;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(OnPointerMoved)}";
        base.OnPointerMoved(e);

        var captured = e.Pointer.Captured;

        if (captured == this)
        {
            Diag.WriteLine(NSpace, "Is captured");
            var target = GetTarget(e);

            // Ensure foreign folder and not self
            if (target != _dropTarget)
            {
                Diag.WriteLine(NSpace, "Drag target");
                CancelTarget();
                _dropTarget = target;
                target?.StartDrop(this);
            }

            return;
        }

        // Reset for edge cases
        CancelTarget();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(OnPointerExited)}";
        base.OnPointerReleased(e);

        var target = CancelTarget();

        if (e.Pointer.Captured == this)
        {
            Diag.WriteLine(NSpace, "Capture released");
            e.Handled = true;
            e.Pointer.Capture(null);
            target?.AcceptDrop(this);
        }
    }

    private GardenDeck? GetDropDeck(object obj)
    {
        if (obj is GardenDeck deck)
        {
            if (deck.Folder != FolderName && deck.CurrentBasket == Owner.Kind)
            {
                return deck;
            }

            return null;
        }

        if (obj is DeckCard card)
        {
            deck = card.Source;

            if (deck.Folder != FolderName && deck.CurrentBasket == Owner.Kind)
            {
                return deck;
            }
        }

        return null;
    }

    private IDeckDrop? CancelTarget()
    {
        var target = _dropTarget;

        _dropTarget = null;
        target?.CancelDrop();

        if (target != null)
        {
            Cursor = null;
        }

        return target;
    }

    private IDeckDrop? GetTarget(PointerEventArgs e)
    {
        const string NSpace = $"{nameof(DeckCard)}.{nameof(GetTarget)}";
        var top = TopLevel.GetTopLevel(this);

        if (top != null && top.InputHitTest(e.GetPosition(top)) is Control control)
        {
            Diag.WriteLine(NSpace, "Control: " + control);
            return control.FindLogicalAncestorOfType<IDeckDrop>(true);
        }

        return null;
    }

    private void RefreshHeader()
    {
        if (!IsRootFolder)
        {
            if (IsOpen)
            {
                _group.CollapseSymbol = Symbols.FolderOpen;
                _group.CollapseFooter = _group.Children.Count > 0 ? null : "Empty";
                return;
            }

            _group.CollapseFooter = null;
            _group.CollapseSymbol = Symbols.Folder;
        }
    }

    private void RefreshGroup()
    {
        CornerRadius = Styling.SmallCornerRadius;
        _group.ChildCorner = Styling.SmallCornerRadius;

        if (!IsRootFolder)
        {
            _group.IsCapped = true;
            _group.ChildBackground = ChromeStyling.GroupShade;
        }
    }

    private void StylingChangedHandler(object? sender, EventArgs __)
    {
        RefreshVisual(_isSearching);
    }

    private void GroupPropertyChangedHandler(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        var p = e.Property;

        if (p == PixieGroup.IsOpenProperty)
        {
            RefreshHeader();

            // No focused when closed
            if (!IsOpen && Garden.Focused?.VisualComponent is DeckCard card && Contains(card))
            {
                Garden.Focused.IsFocused = false;
            }

            return;
        }
    }

    private void ChildRenamingHandler(object? _, GroupChildRenamingEventArgs e)
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(ChildRenamingHandler)}";
        Diag.WriteLine(NSpace, "Old name: " + e.StartText);
        Diag.WriteLine(NSpace, "New text: " + e.CurrentText);
        var src = ((DeckCard)e.Card!).Source;
        src.Title = e.CurrentText;
    }

    private void GroupRenamingHandler(object? _, GroupChildRenamingEventArgs e)
    {
        const string NSpace = $"{nameof(FolderView)}.{nameof(GroupRenamingHandler)}";
        Diag.WriteLine(NSpace, "Old name: " + e.StartText);
        Diag.WriteLine(NSpace, "New text: " + e.CurrentText);

        if (Owner is not BasketView view || e.StartText == e.CurrentText)
        {
            // Either inside Search (not allowed or possible), or same as old
            Diag.WriteLine(NSpace, "Do nothing");
            return;
        }

        GardenDeck? firstOpen = null;

        if (IsOpen)
        {
            // If we are open, get the first visible item
            foreach (var item in _group.Children)
            {
                if (item is DeckCard card)
                {
                    firstOpen = card.Source;
                    Diag.WriteLine(NSpace, "A child was found");

                    // Only one needed
                    break;
                }
            }
        }

        // Rename
        if (view.Basket.RenameFolder(e.StartText, e.CurrentText))
        {
            Diag.WriteLine(NSpace, "Rename accepted");

            if (IsOpen)
            {
                // We needed this to cause the new folder to be open when
                //  shown, otherwise an open folder when renamed, will close.
                firstOpen?.VisualSignals |= SignalFlags.OpenFolder;
            }
        }
        else
        {
            e.Handled = true;
            e.IsRejected = true;
            Diag.WriteLine(NSpace, "RENAME REJECTED");
        }
    }
}
