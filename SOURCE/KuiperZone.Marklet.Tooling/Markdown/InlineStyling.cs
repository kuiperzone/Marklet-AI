// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// AUTHOR    : Andrew Thomas
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
/// Inline styling flags.
/// </summary>
[Flags]
public enum InlineStyling
{
    /// <summary>
    /// Default Inline text.
    /// </summary>
    /// <remarks>
    /// Styling is only that defined by the parent <see cref="BlockKind"/> value.
    /// </remarks>
    Default = 0x0000,

    /// <summary>
    /// Inline emphasis or italic. Markdown "*", "_", or HTML: {em}, {i}
    /// </summary>
    Emphasis = 0x00000001,

    /// <summary>
    /// Inline strong or bold. Markdown "**", "__", or HTML: {strong}, {b}
    /// </summary>
    Strong = 0x00000002,

    /// <summary>
    /// Inline code. Markdown "`" or HTML: {code}.
    /// </summary>
    Code = 0x00000004,

    /// <summary>
    /// Monospace. HTML only: {samp}, {kbd}, {var} or {tt}.
    /// </summary>
    Mono = 0x00000008,

    /// <summary>
    /// Inline underlined. HTML only: {u}.
    /// </summary>
    Underline = 0x00000010,

    /// <summary>
    /// Inline strike-through. HTML only: {del}, {s} or {strike}.
    /// </summary>
    Strike = 0x00000020,

    /// <summary>
    /// Inline subscript. HTML only: {sub}.
    /// </summary>
    Sub = 0x00000040,

    /// <summary>
    /// Inline superscript. HTML only: {sup}.
    /// </summary>
    Sup = 0x00000080,

    /// <summary>
    /// Inline marked. HTML only: {mark}.
    /// </summary>
    Mark = 0x00000100,

    /// <summary>
    /// Inline math. Markdown "$". Exports to HTML as: {samp}.
    /// </summary>
    Math = 0x00000200,

    /// <summary>
    /// Behaves as <see cref="Mark"/>, but treated as disctinct and no way to specify in text.
    /// </summary>
    /// <remarks>
    /// Has no markdown equivalent and does not export to HTML.
    /// </remarks>
    Keyword = 0x00000400,

    /// <summary>
    /// Text is grayed out. Does not export to HTML.
    /// </summary>
    /// <remarks>
    /// Has no markdown equivalent and does not export to HTML.
    /// </remarks>
    Grayed = 0x00000800,
}

/// <summary>
/// Markdown extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Returns whether style has <see cref="InlineStyling.Code"/>, <see cref="InlineStyling.Mono"/> or <see cref="InlineStyling.Math"/>.
    /// </summary>
    public static bool IsMono(this InlineStyling src)
    {
        const InlineStyling AnyCode = InlineStyling.Code | InlineStyling.Mono | InlineStyling.Math;
        return (src & AnyCode) != 0;
    }

    /// <summary>
    /// Returns the markdown tag for a single flag value, or null if none.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid value</exception>
    public static string? ToMark(this InlineStyling src)
    {
        switch (src)
        {
            case InlineStyling.Default: return null;
            case InlineStyling.Emphasis: return "*";
            case InlineStyling.Strong: return "**";
            case InlineStyling.Code: return "`";
            case InlineStyling.Mono:
            case InlineStyling.Strike:
            case InlineStyling.Underline:
            case InlineStyling.Sub:
            case InlineStyling.Sup:
            case InlineStyling.Mark: return null;
            case InlineStyling.Math: return "$";
            case InlineStyling.Keyword: return null;
            case InlineStyling.Grayed: return null;
            default: throw new ArgumentException($"Invalid {nameof(InlineStyling)} = {src}");
        }
    }

    /// <summary>
    /// Returns the default equivalent HTML element name for a single flag value, or null if none.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid value</exception>
    public static string? ToHtml(this InlineStyling src)
    {
        switch (src)
        {
            case InlineStyling.Default: return null;
            case InlineStyling.Emphasis: return "em";
            case InlineStyling.Strong: return "strong";
            case InlineStyling.Code: return "code";
            case InlineStyling.Mono: return "samp";
            case InlineStyling.Underline: return "ins"; // <- "u" is deprecated
            case InlineStyling.Strike: return "del"; // <- "strike" is deprecated
            case InlineStyling.Sub: return "sub";
            case InlineStyling.Sup: return "sup";
            case InlineStyling.Mark: return "mark";
            case InlineStyling.Math: return "samp";
            case InlineStyling.Keyword: return null;
            case InlineStyling.Grayed: return null;
            default: throw new ArgumentException($"Invalid {nameof(InlineStyling)} = {src}");
        }
    }
}
