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

using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

internal sealed class MarkRun : CrossRun
{
    private readonly MarkView _owner;

    public MarkRun(MarkView owner, BlockKind parent, MarkElement element)
    {
        _owner = owner;
        ParentKind = parent;
        Update(element);
    }

    /// <summary>
    /// Gets the block kind this belongs to.
    /// </summary>
    public BlockKind ParentKind { get; private set; }

    /// <summary>
    /// Gets the styling.
    /// </summary>
    public InlineStyling Styling { get; private set; }

    /// <summary>
    /// Updates with optional new "element" content.
    /// </summary>
    public void Update(MarkElement? element)
    {
        if (element != null)
        {
            Styling = element.Styling;
            Text = Decorate(element);

            if (Uri.TryCreate(element.Link?.Uri, UriKind.Absolute, out Uri? uri))
            {
                Uri = uri;
            }
            else
            {
                Uri = null;
            }
        }

        var s = Styling;
        this.SetFontStyle(s);
        this.SetFontWeight(s);
        this.SetBaseline(s, _owner.ScaledFontSize);
        TextDecorations = s.ToXamlTextDecorations();


        bool hasFamily = false;
        bool hasForeground = false;
        bool hasBackground = false;

        if (s.HasFlag(InlineStyling.Mark) || s.HasFlag(InlineStyling.Keyword))
        {
            if (!hasBackground)
            {
                hasBackground = true;
                this.SetBackground(MarkControl.MarkBackgroundBrush);
            }
        }

        if (s.HasFlag(InlineStyling.Code))
        {
            if (!hasFamily)
            {
                hasFamily = true;
                FontFamily = _owner.MonoFamily;
            }

            if (!hasBackground)
            {
                hasBackground = true;
                this.SetBackground(MarkControl.DefaultCodeBackground);
            }
        }
        else
        if (s.HasFlag(InlineStyling.Grayed))
        {
            // Do not clash with Code background here
            if (!hasForeground)
            {
                hasForeground = true;
                this.SetForeground(MarkControl.NeutralForegroundBrush);
            }
        }

        if (s.HasFlag(InlineStyling.Mono) || s.HasFlag(InlineStyling.Math))
        {
            if (!hasFamily)
            {
                hasFamily = true;
                FontFamily = _owner.MonoFamily;
            }
        }

        if (Uri != null)
        {
            if (!hasForeground)
            {
                hasForeground = true;
                this.SetForeground(_owner.LinkForeground);
            }
        }

        if (!hasFamily)
        {
            ClearValue(FontFamilyProperty);
        }

        if (!hasBackground)
        {
            ClearValue(BackgroundProperty);
        }

        if (!hasForeground)
        {
            ClearValue(ForegroundProperty);
        }
    }

    private static string Decorate(MarkElement e)
    {
        if (e.Styling.HasFlag(InlineStyling.Code))
        {
            // Non-breaky
            // We could use narrow form: U+202F
            // But this makes no difference in monospace
            return string.Concat("\u00A0", e.Text, "\u00A0");
        }

        return e.Text;
    }
}