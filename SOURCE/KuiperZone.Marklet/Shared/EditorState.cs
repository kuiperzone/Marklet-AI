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
using Avalonia.Controls;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// Holds pertenant state of <see cref="PromptWindow"/>.
/// </summary>
public sealed class EditorState
{
    private readonly Window _parent;

    /// <summary>
    /// Constructor.
    /// </summary>
    public EditorState(Window parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Gets or sets the last window width;
    /// </summary>
    public Rect Bounds { get; set; }

    /// <summary>
    /// Gets or sets the monospace state;
    /// </summary>
    public bool IsMonospace { get; set; }

    /// <summary>
    /// Gets the zoom level;
    /// </summary>
    public int ZoomScale { get; set; } = 100;

    /// <summary>
    /// Gets or sets the editor text;
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Updates this instance from given editor window.
    /// </summary>
    public void CopyFrom(PromptWindow editor)
    {
        // Bounds should give last size after window closed.
        Bounds = editor.GetRelativeRect(_parent);
        IsMonospace = editor.IsMonospace;
        ZoomScale = editor.Zoom.Scale;
        Text = editor.Text;
    }

    /// <summary>
    /// Apply this instance properties to editor window.
    /// </summary>
    public void CopyTo(PromptWindow editor)
    {
        editor.RestoreRelativeRect(_parent, Bounds);
        editor.IsMonospace = IsMonospace;
        editor.Zoom.Scale = ZoomScale;
        editor.Text = Text;
    }
}
