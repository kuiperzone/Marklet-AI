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

namespace KuiperZone.Marklet.Tooling.Markdown;

/// <summary>
/// Immutable link information comprising a URI and title (not text).
/// </summary>
public sealed class LinkInfo : IEquatable<LinkInfo>
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public LinkInfo()
    {
    }

    /// <summary>
    /// Constructor where <see cref="IsImage"/> is set to false.
    /// </summary>
    public LinkInfo(string? uri, string? title = null)
    {
        Uri = uri;

        if (!string.IsNullOrEmpty(title))
        {
            Title = title;
        }
    }

    /// <summary>
    /// Full value constructor.
    /// </summary>
    public LinkInfo(string? uri, bool isImage, string? title = null)
        : this(uri, title)
    {
        IsImage = isImage;
    }

    /// <summary>
    /// Static equals.
    /// </summary>
    public static bool Equals(LinkInfo? link0, LinkInfo? link1)
    {
        if (link0 == link1)
        {
            return true;
        }

        if (link0 != null)
        {
            return link0.Equals(link1);
        }

        // Cannot be null
        return link1!.Equals(link0);
    }

    /// <summary>
    /// Gets the URI.
    /// </summary>
    /// <remarks>
    /// Both <see cref="Uri"/> and <see cref="Title"/> can be null according to CommonMark specification.
    /// </remarks>
    public string? Uri { get; }

    /// <summary>
    /// Gets whether the link represents an image.
    /// </summary>
    public bool IsImage { get; }

    /// <summary>
    /// Gets the title. The value is HTML encoded.
    /// </summary>
    /// <remarks>
    /// Both <see cref="Uri"/> and <see cref="Title"/> can be null according to CommonMark specification.
    /// </remarks>
    public string? Title { get; }

    /// <summary>
    /// Returns true if <see cref="Uri"/> is null.
    /// </summary>
    public bool IsEmpty
    {
        get { return Uri == null; }
    }

    /// <summary>
    /// Implements <see cref="IEquatable{T}"/>.
    /// </summary>
    public bool Equals(LinkInfo? other)
    {
        if (other == this)
        {
            return true;
        }

        return other != null &&
            Uri == other.Uri &&
            IsImage == other.IsImage &&
            Title == other.Title;
    }

    /// <summary>
    /// Overides.
    /// </summary>
    public sealed override bool Equals(object? obj)
    {
        return Equals(obj as LinkInfo);
    }

    /// <summary>
    /// Returns the <see cref="Uri"/> only.
    /// </summary>
    public sealed override string? ToString()
    {
        return Uri;
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Uri, Title);
    }

}
