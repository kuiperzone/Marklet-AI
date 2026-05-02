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

namespace KuiperZone.Marklet.Stack.Garden;

/// <summary>
/// The <see cref="GardenLeaf"/> form or kind.
/// </summary>
/// <remarks>
/// Existing values are written to storage and must not change.
/// </remarks>
public enum LeafFormat : byte
{
    /// <summary>
    /// None (invalid).
    /// </summary>
    None = 0,

    /// <summary>
    /// User message.
    /// </summary>
    UserMessage = 1,

    /// <summary>
    /// Assistant message.
    /// </summary>
    AssistantMessage = 2,

    /// <summary>
    /// User note.
    /// </summary>
    UserNote = 3,

    /// <summary>
    /// Free floating user attachment.
    /// </summary>
    UserAttachment = 4,

    /// <summary>
    /// Free floating assistant attachment.
    /// </summary>
    AssistantAttachment = 5,

    /// <summary>
    /// Display information only.
    /// </summary>
    Notification = 200,
}

/// <summary>
/// Extension methods.
/// </summary>
public static partial class HelperExt
{
    /// <summary>
    /// Ensure underlying value is legal.
    /// </summary>
    public static bool IsLegal(this LeafFormat src)
    {
        return (src > LeafFormat.None && src <= LeafFormat.AssistantAttachment) || src == LeafFormat.Notification;
    }

    /// <summary>
    /// Returns whether the value pertains to the user.
    /// </summary>
    public static bool IsUser(this LeafFormat src)
    {
        return src == LeafFormat.UserMessage || src == LeafFormat.UserNote || src == LeafFormat.UserAttachment;
    }

    /// <summary>
    /// Returns whether the value pertains to the assistant.
    /// </summary>
    public static bool IsAssistant(this LeafFormat src)
    {
        return src == LeafFormat.AssistantMessage || src == LeafFormat.AssistantAttachment;
    }

    /// <summary>
    /// Returns whether the value pertains to an attachment.
    /// </summary>
    public static bool IsAttachment(this LeafFormat src)
    {
        return src == LeafFormat.UserAttachment || src == LeafFormat.AssistantAttachment;
    }

    /// <summary>
    /// Returns a default name such as "User", "Assistant" or "Attachment".
    /// </summary>
    public static string? ToName(this LeafFormat src)
    {
        switch (src)
        {
            case LeafFormat.UserMessage:
                return "User";
            case LeafFormat.AssistantMessage:
                return "Assistant";
            case LeafFormat.UserNote:
                return "Note";
            case LeafFormat.UserAttachment:
            case LeafFormat.AssistantAttachment:
                return "Attachment";
            case LeafFormat.Notification:
                return "Notification";
            default:
                return null;
        }
    }

    /// <summary>
    /// Returns whether the leaf content is nominally show to the user.
    /// </summary>
    public static bool IsVisible(this LeafFormat src)
    {
        return src != LeafFormat.None;
    }

    /// <summary>
    /// Returns whether display fully parses inline markup.
    /// </summary>
    public static bool IsDefaultInline(this LeafFormat src)
    {
        return src == LeafFormat.AssistantMessage || src == LeafFormat.UserNote || src == LeafFormat.Notification;
    }
}