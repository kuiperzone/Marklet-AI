// -----------------------------------------------------------------------------
// SPDX-FileNotice: KuiperZone.Marklet - Local AI Client
// SPDX-License-Identifier: AGPL-3.0-only
// SPDX-FileCopyrightText: © 2025-2026 Andrew Thomas <kuiperzone@users.noreply.github.com>
// SPDX-ProjectHomePage: https://kuiper.zone/marklet-ai/
// SPDX-FileType: Source
// SPDX-FileComment: This is NOT AI generated source code but was created with human thinking and effort.
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
/// Button option flags for <see cref="ChromeDialog"/>.
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
    None = 0x00000000,

    // CRITICAL

    /// <summary>
    /// Proceed (critical, positive)
    /// </summary>
    Proceed = 0x00000001,

    /// <summary>
    /// Delete (critical, positive).
    /// </summary>
    Delete = 0x00000002,

    /// <summary>
    /// Delete all (critical, positive).
    /// </summary>
    DeleteAll = 0x00000004,

    /// <summary>
    /// Yes all (critical, positive).
    /// </summary>
    YesAll = 0x00000020,

    // DEFAULT

    /// <summary>
    /// Retry (default, positive).
    /// </summary>
    Retry = 0x00000100,

    /// <summary>
    /// Continue (default, positive).
    /// </summary>
    Continue = 0x00000200,

    /// <summary>
    /// Save (default, positive).
    /// </summary>
    Save = 0x00000400,

    /// <summary>
    /// Yes (default, positive).
    /// </summary>
    Yes = 0x00000800,

    /// <summary>
    /// OK (default, positive).
    /// </summary>
    Ok = 0x00001000,

    /// <summary>
    /// Close (default).
    /// </summary>
    Close = 0x00002000,

    // NEUTRAL

    /// <summary>
    /// Apply changes (neutral, positive).
    /// </summary>
    Apply = 0x00010000,

    /// <summary>
    /// Apply all (neutral, positive).
    /// </summary>
    ApplyAll = 0x00020000,

    /// <summary>
    /// Ignore (neutral).
    /// </summary>
    Ignore = 0x00040000,

    /// <summary>
    /// Ignore all (neutral).
    /// </summary>
    IgnoreAll = 0x00080000,

    /// <summary>
    /// Save As (neutral, positive).
    /// </summary>
    SaveAs = 0x00100000,

    /// <summary>
    /// Save all (neutral, positive).
    /// </summary>
    SaveAll = 0x00200000,

    // CANCEL

    /// <summary>
    /// No (cancel).
    /// </summary>
    No = 0x01000000,

    /// <summary>
    /// No all (cancel).
    /// </summary>
    NoAll = 0x02000000,

    /// <summary>
    /// Cancel (cancel).
    /// </summary>
    Cancel = 0x04000000,

    /// <summary>
    /// Abort (cancel).
    /// </summary>
    Abort = 0x08000000,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    private const DialogButtons CriticalBlock = (DialogButtons)0x000000FF;
    private const DialogButtons DefaultBlock = (DialogButtons)0x0000FF00;
    private const DialogButtons NeutralBlock = (DialogButtons)0x00FF0000;
    private const DialogButtons CancelBlock = (DialogButtons)0x4F000000;
    private static readonly DialogButtons[] LegalButtons = Enum.GetValues<DialogButtons>();

    /// <summary>
    /// Returns true if the value is one of the <see cref="DialogButtons"/> values including <see
    /// cref="DialogButtons.None"/>. It returns false for a combined flag value.
    /// </summary>
    public static bool IsSingleLegal(this DialogButtons src)
    {
        return LegalButtons.Contains(src);
    }

    /// <summary>
    /// Returns true if the given combined flag value is legal.
    /// </summary>
    /// <remarks>
    /// It returns false where the combination contains multiple "default" or "cancel" buttons, i.e. where more than one
    /// may respond to the ENTER or ESCAPE key. It returns true for the <see cref="DialogButtons.Ok"/>, <see
    /// cref="DialogButtons.Cancel"/> combination, but false for <see cref="DialogButtons.Cancel"/> plus <see
    /// cref="DialogButtons.Abort"/>.
    /// </remarks>
    public static bool IsCombinedLegal(this DialogButtons src)
    {
        bool def = false;
        bool cancel = false;

        foreach (var item in LegalButtons)
        {
            if (src.HasFlag(item))
            {
                if (IsDefault(item))
                {
                    if (def)
                    {
                        return false;
                    }

                    def = true;
                    continue;
                }

                if (IsCancel(item))
                {
                    if (cancel)
                    {
                        return false;
                    }

                    cancel = true;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets whether the button "src" contains a flag considered to be "critical".
    /// </summary>
    /// <remarks>
    /// This includes <see cref="DialogButtons.Yes"/> and <see cref="DialogButtons.YesAll"/>.
    /// </remarks>
    public static bool IsCritical(this DialogButtons src)
    {
        return (src & CriticalBlock) != 0;
    }

    /// <summary>
    /// Gets whether the button "src" contains a flag considered to be "default", i.e. responds to ENTER key.
    /// </summary>
    /// <remarks>
    /// This excludes <see cref="DialogButtons.YesAll"/>.
    /// </remarks>
    public static bool IsDefault(this DialogButtons src)
    {
        return (src & DefaultBlock) != 0;
    }

    /// <summary>
    /// Gets whether the button "src" contains a flag considered to be "cancel", i.e. responds to ESCAPE key.
    /// </summary>
    public static bool IsCancel(this DialogButtons src)
    {
        return (src & CancelBlock) != 0;
    }

    /// <summary>
    /// Gets whether the button "src" contains a flag considered to be a "positive action", i.e. go ahead and do the
    /// thing rather than cancel or abort.
    /// </summary>
    /// <remarks>
    /// This does not distinguish between, for example, <see cref="DialogButtons.Ok"/> and <see
    /// cref="DialogButtons.DeleteAll"/>.
    /// </remarks>
    public static bool IsPositiveResult(this DialogButtons src)
    {
        return (src & (CriticalBlock | DefaultBlock | NeutralBlock)) != 0 &&
            (src & (DialogButtons.Close | DialogButtons.Ignore | DialogButtons.IgnoreAll)) == 0;
    }

    /// <summary>
    /// Returns single <see cref="DialogButtons"/> flag pertaining to Enter or Escpace key.
    /// </summary>
    public static DialogButtons GetCloseAction(this DialogButtons src, Key key)
    {
        if (key == Key.Escape)
        {
            foreach (var item in LegalButtons)
            {
                if (item != DialogButtons.None && src.HasFlag(item) && item.IsCancel())
                {
                    return item;
                }
            }

            return DialogButtons.None;
        }

        if (key == Key.Enter)
        {
            foreach (var item in LegalButtons)
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