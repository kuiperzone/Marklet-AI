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

using Avalonia.Input;

namespace KuiperZone.Marklet.PixieChrome.Windows;

/// <summary>
/// Button option flags for <see cref="MessageDialog"/>.
/// </summary>
/// <remarks>
/// Flag values are ordered in the order of presentation.
/// </remarks>
[Flags]
public enum DialogButtons
{
    /// <summary>
    /// No buttons.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// OK.
    /// </summary>
    Ok = 0x0001,

    /// <summary>
    /// Yes.
    /// </summary>
    Yes = 0x0002,

    /// <summary>
    /// Yes all.
    /// </summary>
    YesAll = 0x0004,

    /// <summary>
    /// No.
    /// </summary>
    No = 0x0008,

    /// <summary>
    /// No all.
    /// </summary>
    NoAll = 0x0010,

    /// <summary>
    /// Cancel.
    /// </summary>
    Cancel = 0x0020,

    /// <summary>
    /// Abort.
    /// </summary>
    Abort = 0x0040,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    private const DialogButtons DefaultFlags = DialogButtons.Ok | DialogButtons.Yes;
    private const DialogButtons CancelFlags = DialogButtons.No | DialogButtons.NoAll | DialogButtons.Cancel | DialogButtons.Abort;
    private static readonly DialogButtons[] AllButtons = Enum.GetValues<DialogButtons>();

    /// <summary>
    /// Gets whether the button "src" contains a flag considered to be "default", i.e. responds to ENTER key.
    /// </summary>
    /// <remarks>
    /// This excludes <see cref="DialogButtons.YesAll"/>.
    /// </remarks>
    public static bool IsDefault(this DialogButtons src)
    {
        return (src & DefaultFlags) != 0;
    }

    /// <summary>
    /// Gets whether the button "src" contains a flag considered to be "cancel", i.e. responds to ESCAPE key.
    /// </summary>
    public static bool IsCancel(this DialogButtons src)
    {
        return (src & CancelFlags) != 0;
    }

    /// <summary>
    /// Where "src" may contain multiple flag values, returns a single <see cref="DialogButtons"/> flag according to the
    /// given key press.
    /// </summary>
    public static DialogButtons GetCloseAction(this DialogButtons src, PhysicalKey key)
    {
        if (key == PhysicalKey.Escape)
        {
            // Prioritize
            if (src.HasFlag(DialogButtons.Abort))
            {
                return DialogButtons.Abort;
            }

            if (src.HasFlag(DialogButtons.NoAll))
            {
                return DialogButtons.NoAll;
            }

            foreach (var item in AllButtons)
            {
                if (item != DialogButtons.None && src.HasFlag(item) && item.IsCancel())
                {
                    return item;
                }
            }

            return DialogButtons.None;
        }

        if (key == PhysicalKey.Enter)
        {
            // Prioritize
            if (src.HasFlag(DialogButtons.Yes))
            {
                return DialogButtons.Yes;
            }

            foreach (var item in AllButtons)
            {
                if (item != DialogButtons.None && src.HasFlag(item) && item.IsDefault())
                {
                    return item;
                }
            }

            return DialogButtons.None;
        }

        return DialogButtons.None;
    }

}