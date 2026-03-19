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
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using Avalonia.Layout;
using KuiperZone.Marklet.Shared;
using Avalonia.Controls.Documents;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Controls;

/// <summary>
/// Used to present pre-canned information over <see cref="DeckViewer"/>.
/// </summary>
public sealed class DeckOverlay : Border
{
    private readonly ScrollPanel _panel = new();
    private GardenDeck? _newDeck;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DeckOverlay()
    {
        IsVisible = false;
        base.Child = _panel;

        _panel.VerticalAlignment = VerticalAlignment.Center;
        _panel.ContentMaxWidth = ContentWidth.Narrow.ToPixels();
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
    /// Shows a text prompt.
    /// </summary>
    public bool ShowPrompt(GardenDeck? pending)
    {
        if (_newDeck != pending)
        {
            if (pending != null)
            {
                var kind = pending.Kind;
                bool ephem = pending.IsEphemeral;
                var text = "New " + kind.DisplayName(DisplayKind.Default, ephem);

                if (pending.Kind == DeckKind.Note)
                {
                    text += $"\n\nAssistant does not respond to notes.";
                }

                if (pending.IsEphemeral)
                {
                    text += $"\n\n{kind.DisplayName(DisplayKind.Plural, ephem)} are lost when the application exits.";
                }

                ShowPrompt(kind.MaterialSymbol(ephem), text);
                _newDeck = pending;
                return true;
            }

            _newDeck = null;
        }

        return false;
    }

    /// <summary>
    /// Shows a text prompt.
    /// </summary>
    public void ShowPrompt(string prompt)
    {
        ShowPrompt(null, prompt);
    }

    /// <summary>
    /// Shows a text prompt with large symbol.
    /// </summary>
    public void ShowPrompt(string? symbol, string prompt)
    {
        var obj = NewPrompt(symbol, prompt);

        if (obj != null)
        {
            obj.Margin = new(0.0, ChromeSizes.HugePx, 0.0, 0.0);

            _newDeck = null;
            _panel.Children.Clear();
            _panel.Children.Add(obj);
            IsVisible = true;
        }
    }

    /// <summary>
    /// Shows a markdown prompt.
    /// </summary>
    public void ShowMarkdown(string prompt)
    {
        ShowMarkdown(null, prompt);
    }

    /// <summary>
    /// Shows a markdown prompt with large symbol.
    /// </summary>
    public void ShowMarkdown(string? symbol, string prompt)
    {
        MarkView? mark = null;
        var obj = NewPrompt(symbol, "");

        if (!string.IsNullOrEmpty(prompt))
        {
            mark = new();
            mark.IsChromeStyled = true;
            mark.Content = prompt;
            mark.HorizontalAlignment = HorizontalAlignment.Center;
        }

        if (obj != null || mark != null)
        {
            _newDeck = null;
            _panel.Children.Clear();

            if (obj != null)
            {
                obj.Margin = new(0.0, ChromeSizes.HugePx, 0.0, 0.0);
                _panel.Children.Add(obj);
            }

            if (mark != null)
            {
                if (obj == null)
                {
                    mark.Margin = new(0.0, ChromeSizes.HugePx, 0.0, 0.0);
                }

                _panel.Children.Add(mark);
            }

            IsVisible = true;
        }
    }

    /// <summary>
    /// Clears and hides.
    /// </summary>
    public void Clear()
    {
        IsVisible = false;
        _panel.Children.Clear();
    }

    private static TextBlock? NewPrompt(string? symbol, string? prompt)
    {
        InlineCollection? inline = null;

        if (!string.IsNullOrEmpty(symbol))
        {
            inline = ChromeFonts.GetRun(symbol, 2.0 * ChromeFonts.HugeFontSize);
            inline?.Add("\n\n");
        }

        if (!string.IsNullOrEmpty(prompt))
        {
            inline ??= new();
            inline.AddRange(ChromeFonts.GetRun(prompt)!);
        }

        if (inline != null)
        {
            var obj = new TextBlock();
            obj.Foreground = ChromeStyling.GrayForeground;
            obj.TextAlignment = Avalonia.Media.TextAlignment.Center;

            obj.Inlines = inline;
            return obj;
        }

        return null;
    }

}