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

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// Option class for <see cref="Textual.SigText(string, int, SigOptions)"/>.
/// </summary>
public sealed class SigOptions
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public SigOptions()
    {
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public SigOptions(int sigLength, TruncStyle truncStyle = TruncStyle.EndEllipses)
    {
        SigLength = sigLength;
        TruncStyle = truncStyle;
    }

    /// <summary>
    /// Gets or sets the significant length value where higher values mean the algorithm is more likely to prioritise
    /// longer fragments.
    /// </summary>
    /// <remarks>
    /// Working values in the range [12, 64].
    /// </remarks>
    public int SigLength { get; set; } = 52;

    /// <summary>
    /// Gets the truncation style to apply to the final text if it exceeds the desired minimum length.
    /// </summary>
    public TruncStyle TruncStyle { get; set; } = TruncStyle.EndEllipses;

    /// <summary>
    /// Ensures that the first letter of the text fragment is capitalized.
    /// </summary>
    public bool CapitalizeFirst { get; set; } = true;

    /// <summary>
    /// Preserves newlines in the source text.
    /// </summary>
    public bool PreserveLines { get; set; }

}
