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

using System.Collections;
using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome.CarouselPages;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.Windows.Internal;


internal sealed class IndexClickEventArgs : EventArgs
{
    public IndexClickEventArgs(PageEntry page)
    {
        Page = page;
    }

    public PageEntry Page { get; }
}

/// <summary>
/// Internal for <see cref="CarouselWindow"/>
/// </summary>
internal sealed class CarouselEntries : IReadOnlyList<PageEntry>
{
    private readonly List<PageEntry> _list = new(4);

    /// <summary>
    /// Occurs when index <see cref="PageEntry.IndexButton"/> is clicked.
    /// </summary>
    public event EventHandler<IndexClickEventArgs>? IndexClicked;

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public PageEntry this[int index]
    {
        get { return _list[index]; }
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public int Count
    {
        get { return _list.Count; }
    }

    /// <summary>
    /// Gets whether <see cref="IsInitialized"/> called.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public IEnumerator<PageEntry> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <summary>
    /// Implements <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <summary>
    /// To be called on Window.OnOpen().
    /// </summary>
    public void Initialize(IEnumerable<CarouselPage> pages, IEnumerable<string>? classes)
    {
        IsInitialized = true;

        _list.Clear();
        int index = 0;

        foreach (var item in pages)
        {
            if (!string.IsNullOrWhiteSpace(item.Title))
            {
                var entry = new PageEntry(item, index++, classes);

                entry.IndexButton.Tag = entry; // <- connect
                entry.IndexButton.BackgroundClick += HeaderClickHandler;

                _list.Add(entry);
            }
        }
    }

    /// <summary>
    /// Safely gets <see cref="PageEntry"/> using "index", or return a default, or null.
    /// </summary>
    public PageEntry? GetPageOrDefault(int index, int defaultIndex = -1)
    {
        if (index < 0 || index >= _list.Count)
        {
            index = defaultIndex;
        }

        if (index > -1 && index < _list.Count)
        {
            return _list[index];
        }

        return null;
    }

    /// <summary>
    /// Refresh styling where <see cref="PixieControl"/> cannot do this.
    /// </summary>
    public void RefreshStyling()
    {
        foreach (var item in _list)
        {
            item.RefreshStyling();
        }
    }

    /// <summary>
    /// Locates "keyword" and appends to "findings".
    /// </summary>
    public bool Find(string? keyword, List<PixieFinding> findings)
    {
        keyword = keyword?.Trim();

        if (string.IsNullOrEmpty(keyword))
        {
            return false;
        }

        int count = findings.Count;

        foreach (var entry in _list)
        {
            foreach (var item in entry.Children)
            {
                if (item is PixieGroup group)
                {
                    group.Find(keyword, findings);
                    continue;
                }

                if (item is PixieControl control)
                {
                    control.Find(keyword, findings);
                    continue;
                }

                if (item is TextBlock block)
                {
                    if (block.Text?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        findings.Add(new(block));
                    }

                    continue;
                }
            }
        }

        return findings.Count != count;
    }

    /// <summary>
    /// Locates <see cref="PageEntry"/> given a <see cref="PixieFinding"/>.
    /// </summary>
    public PageEntry? GetFindEntry(PixieFinding? finding)
    {
        if (finding?.Source != null)
        {
            var source = finding.Source;

            // Page
            foreach (var entry in _list)
            {
                // Children are nominally PixieGroup
                foreach (var item in entry.Children)
                {
                    if (item == source)
                    {
                        return entry;
                    }

                    if (item is PixieGroup group && group.Children.Contains(source))
                    {
                        return entry;
                    }
                }
            }
        }

        return null;
    }

    private void HeaderClickHandler(object? sender, EventArgs __)
    {
        if (sender is PixieButton btn && btn.Tag is PageEntry page)
        {
            IndexClicked?.Invoke(sender, new(page));
        }
    }
}
