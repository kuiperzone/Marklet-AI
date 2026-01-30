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

using System.Text;
using Avalonia.Controls;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Provides visual for a single <see cref="IReadOnlyMarkBlock"/>.
/// </summary>
internal abstract class MarkBlockHost : MarkVisualHost
{
    protected MarkBlockHost(MarkView owner, IReadOnlyMarkBlock source)
        : base(owner)
    {
        Source = source;
        Kind = source.Kind;
        ListLevel = source.ListLevel;
        QuoteLevel = source.QuoteLevel;
    }

    /// <summary>
    /// Provides short to the <see cref="BlockKind"/> associated with this block.
    /// </summary>
    public readonly BlockKind Kind;

    /// <summary>
    /// Gets the source block data.
    /// </summary>
    /// <remarks>
    /// The reference may change with <see cref="ConsumeUpdates"/>, but the <see cref="IReadOnlyMarkBlock.Kind"/> does not.
    /// </remarks>
    public IReadOnlyMarkBlock Source { get; private set; }

    /// <summary>
    /// Gets whether the instance is pending its first update (set false by <see cref="ConsumeUpdates"/>).
    /// </summary>
    public bool IsPending { get; private set; } = true;

    /// <summary>
    /// Create appropriate instance of <see cref="MarkBlockHost"/> for given source.
    /// </summary>
    /// <remarks>
    /// Always consumes exactly one block.
    /// </remarks>
    public new static MarkBlockHost New(MarkView owner, IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        int orig = index;
        var source = sequence[index];
        MarkBlockHost host;

        if (source.Kind.IsCode())
        {
            host = new MarkCodeHost(owner, source);

            if (host.ConsumeUpdates(sequence, ref index) != MarkConsumed.Changed)
            {
                throw new InvalidOperationException($"{host.GetType().Name} failed to consume initial block");
            }

            // Not expected but important to catch in development
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(index, orig);
            return host;
        }

        if (source.Kind == BlockKind.Rule)
        {
            host = new MarkRuleHost(owner, source);

            if (host.ConsumeUpdates(sequence, ref index) != MarkConsumed.Changed)
            {
                throw new InvalidOperationException($"{host.GetType().Name} failed to consume initial block");
            }

            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(index, orig);
            return host;
        }

        host = new MarkTextHost(owner, source);

        if (host.ConsumeUpdates(sequence, ref index) != MarkConsumed.Changed)
        {
            throw new InvalidOperationException($"{host.GetType().Name} failed to consume initial block");
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(index, orig);
        return host;
    }

    /// <summary>
    /// Gets whether "source" is compatible with this "cached" visual block.
    /// </summary>
    public bool IsCompatible(IReadOnlyMarkBlock source)
    {
        return Kind == source.Kind && QuoteLevel == source.QuoteLevel && ListLevel == source.ListLevel;
    }

    /// <summary>
    /// Overrides and consumes a single block from the source "sequence".
    /// </summary>
    /// <remarks>
    /// To be overridden by subclass.
    /// </remarks>
    public override MarkConsumed ConsumeUpdates(IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        var source = sequence[index];

        if (IsCompatible(source))
        {
            if (IsPending || !Source.Equals(source))
            {
                // Order important
                Source = source;
                SetChildMargin(index == 0, index == sequence.Count - 1);

                index += 1;
                IsPending = false;
                return MarkConsumed.Changed;
            }

            Source = source;

            index += 1;
            IsPending = false;
            return MarkConsumed.NoChange;
        }

        // Don't increment index
        return MarkConsumed.Incompatible;
    }

    /// <summary>
    /// Extends.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder(base.ToString());
        Append(buffer, nameof(Kind), Kind);
        Append(buffer, $"{nameof(Control)}.Margin", Control.Margin);
        return buffer.ToString();
    }

    /// <summary>
    /// Updates the margin on <see cref="MarkVisualHost.Control"/>.
    /// </summary>
    protected virtual void SetChildMargin(bool isFirst, bool isLast)
    {
        double topPx = 0.0;
        double fsize = Owner.ScaledFontSize;

        // Key of font size
        double spacePx = fsize * 0.5;

        if (Source.ListLevel > 0)
        {
            spacePx = fsize * 0.25;
        }

        if (!isFirst)
        {
            topPx = spacePx;

            if (Kind.IsHeading())
            {
                topPx += Kind.ToXamlFontScale() * fsize;
            }
        }

        Control.Margin = new(0.0, topPx, 0.0, isLast ? 0.0 : spacePx);
    }
}