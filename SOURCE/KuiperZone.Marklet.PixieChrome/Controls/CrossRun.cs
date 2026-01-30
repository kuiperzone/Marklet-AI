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

using Avalonia.Controls.Documents;

namespace KuiperZone.Marklet.PixieChrome.Controls;

/// <summary>
/// Extends <see cref="Run"/> to provide non-jankey clickable links. Otherwise, where <see cref="Uri"/> is null, it
/// behaves exactly as <see cref="Run"/> for inline text.
/// </summary>
/// <remarks>
/// Where <see cref="Uri"/> is given, the Foreground property is set to an alternate color..
/// </remarks>
public class CrossRun : Run
{
    private Uri? _uri;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CrossRun()
    {
    }

    /// <summary>
    /// Property constructor.
    /// </summary>
    public CrossRun(string? text, Uri? uri = null)
        : base(text)
    {
        Uri = uri;
    }

    /// <summary>
    /// Gets or sets the link URI.
    /// </summary>
    /// <remarks>
    /// If the Foreground property is unset when setting a Uri instance, the Foreground is set to <see
    /// cref="CrossTextBlock.DefaultLinkBrush"/>. If Foreground was already set, it is not changed. When setting <see
    /// cref="Uri"/> from non-null to null, the Foreground property is cleared.
    /// </remarks>
    public Uri? Uri
    {
        get { return _uri; }

        set
        {
            if (value != null)
            {
                _uri = value;

                if (!IsSet(ForegroundProperty))
                {
                    Foreground = CrossTextBlock.DefaultLinkBrush;
                }
            }
            else
            if (_uri != null)
            {
                _uri = null;
                ClearValue(ForegroundProperty);
            }
        }
    }
}