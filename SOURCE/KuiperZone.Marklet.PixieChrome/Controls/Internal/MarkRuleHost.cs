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
using Avalonia.Controls.Shapes;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Rule block.
/// </summary>
internal sealed class MarkRuleHost : MarkBlockHost
{
    private readonly Rectangle _rule = new();

    public MarkRuleHost(MarkView owner, IReadOnlyMarkBlock source)
        : base(owner, source)
    {
        ConditionalDebug.ThrowIfNotEqual(Kind, BlockKind.Rule);
        Control = _rule;
    }

    public override void RefreshLook(bool isFirst, bool isLast)
    {
        ConditionalDebug.ThrowIfTrue(IsPending);
        SetChildMargin(isFirst, isLast);
        RefreshInternal();
    }

    /// <summary>
    /// Extends.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder(base.ToString());

        Append(buffer, "Rule.Fill", _rule.Fill);
        Append(buffer, "Rule.Height", _rule.Height);

        return buffer.ToString();
    }

    /// <summary>
    /// Overrides.
    /// </summary>
    public override MarkConsumed ConsumeUpdates(IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        var rslt = base.ConsumeUpdates(sequence, ref index);

        if (rslt == MarkConsumed.Changed)
        {
            RefreshInternal();
        }

        return rslt;
    }

    protected override void SetChildMargin(bool isFirst, bool isLast)
    {
        Control.Margin = default;
    }

    private void RefreshInternal()
    {
        var fill = Owner.RuleLine;
        _rule.Fill = fill;

        if (fill != null)
        {
            _rule.Height = Owner.RulePixels;
            return;
        }

        _rule.Height = 0;
    }
}