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
/// Markdown block, i.e. a text paragraph or table.
/// </summary>
public interface IReadOnlyMarkBlock : IReadOnlyStyledContainer,
    IEquatable<IReadOnlyMarkBlock>, IEquatable<IReadOnlyStyledContainer>
{
    /// <summary>
    /// Gets (or sets) the block kind.
    /// </summary>
    BlockKind Kind { get; }

    /// <summary>
    /// Gets (or sets) the zero indexed quote indentation level.
    /// </summary>
    /// <remarks>
    /// The value is clamped between 0 and <see cref="MarkBlock.MaxQuoteLevel"/>.
    /// </remarks>
    int QuoteLevel { get; }

    /// <summary>
    /// Gets (or sets) the list indentation level.
    /// </summary>
    /// <remarks>
    /// A block with a positive non-zero value is considered part of a "list". A list must be headed by a block with a
    /// non-zero <see cref="ListOrder"/> or <see cref="ListBullet"/>. The <see cref="ListLevel"/> value is clamped
    /// between 0 and <see cref="MarkBlock.MaxListLevel"/>.
    /// </remarks>
    int ListLevel { get; }

    /// <summary>
    /// Gets (or sets) the numeric prefix for an ordered list.
    /// </summary>
    /// <remarks>
    /// If the value is greater than 0, the list block heads an ordered list. The <see cref="ListOrder"/> applies only
    /// where <see cref="ListLevel"/> is greater than 0. If <see cref="ListLevel"/> is non-zero, and both <see
    /// cref="ListOrder"/> and <see cref="ListBullet"/> are 0, then the block is considered a continuation of an
    /// existing list.
    /// </remarks>
    int ListOrder { get; }

    /// <summary>
    /// Gets (or sets) the markdown bullet prefix for an unordered list, i.e. '*', '+' or '-'.
    /// </summary>
    /// <remarks>
    /// Setting any value other than a recognised markdown bullet sets '\0'. If the value is not '\0', the list block
    /// heads an unordered list. The <see cref="ListBullet"/> applies only where <see cref="ListLevel"/> is greater than
    /// 0. If <see cref="ListLevel"/> is non-zero, and both <see cref="ListOrder"/> and <see cref="ListBullet"/> are 0,
    /// then the block is considered a continuation of an existing list.
    /// </remarks>
    char ListBullet { get; }

    /// <summary>
    /// Gets (or sets) the code language string, i.e. "csharp".
    /// </summary>
    /// <remarks>
    /// The value is ignored where <see cref="Kind"/> does not equal <see cref="BlockKind.FencedCode"/>. The value shall
    /// not contain spaces. Setting an empty value sets null.
    /// </remarks>
    string? Lang { get; }

    /// <summary>
    /// Gets (or sets) the table accoutrement.
    /// </summary>
    /// <remarks>
    /// When <see cref="Table"/> is not null, <see cref="Kind"/> is expected to equal <see cref="BlockKind.TableCode"/>.
    /// Where <see cref="Table"/> was created by <see cref="MarkDocument.Update"/>, <see
    /// cref="StyledContainer.Elements"/> will also be populated with a plain text variant of the table which should be
    /// displayed in monospace. The <see cref="Table"/> instance is not considered in equality or hash codes.
    /// </remarks>
    MarkTable? Table { get; }

    /// <summary>
    /// Returns a quote block prefix string according the value of <see cref="QuoteLevel"/> value.
    /// </summary>
    /// <remarks>
    /// With a <see cref="QuoteLevel"/> value of 2, the string "> > " is returned. The result is null where <see
    /// cref="QuoteLevel"/> is 0.
    /// </remarks>
    string? GetQuotePrefix();

    /// <summary>
    /// Returns the list kind, as defined by <see cref="ListLevel"/>, <see cref="ListOrder"/> and <see cref="ListBullet"/>.
    /// </summary>
    ListKind GetListKind();

    /// <summary>
    /// Returns a list prefix string or character appropriate for display in given "format".
    /// </summary>
    /// <remarks>
    /// It returns null where <see cref="ListLevel"/> is 0, or where "format" is <see cref="TextFormat.Html"/>.
    /// </remarks>
    string? GetListPrefix(TextFormat format = TextFormat.Unicode);

    /// <summary>
    /// Combines elements of matching styles and returns a new block if modification is made.
    /// </summary>
    /// <remarks>
    /// If no change to "this" instance is necessary, the result is null. Where a new block is returned, accoutrements
    /// such as <see cref="Table"/> are NOT copied. The operation is sensitive to the value of <see cref="Kind"/> which
    /// should be set before calling.
    /// </remarks>
    public MarkBlock? Coalesce();
}
