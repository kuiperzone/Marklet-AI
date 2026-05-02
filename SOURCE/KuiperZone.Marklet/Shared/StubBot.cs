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

using System.Text;
using Avalonia.Threading;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// A temporary class to generate stub messages.
/// </summary>
public sealed class StubBot
{
    private const int SkipN = 10;
    private const string Model = "stub-bot";
    private static readonly TimeSpan StartDelay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ChunkDelay = TimeSpan.FromMilliseconds(20);

    private readonly DispatcherTimer _timer = new();
    private int _posN;
    private string? _reply;

    /// <summary>
    /// Constructor.
    /// </summary>
    public StubBot()
    {
        _timer.Tick += TimerTickHandler;
    }

    /// <summary>
    /// Occurs when next stub chunk received.
    /// </summary>
    public event EventHandler<EventArgs>? ChunkReceived;

    /// <summary>
    /// Gets the working chunk.
    /// </summary>
    public string? Chunk { get; private set; }

    /// <summary>
    /// Starts output of a reply.
    /// </summary>
    public void StartReply(string? msg)
    {
        Stop();
        _reply = GetReply(msg);

        _timer.Interval = StartDelay;
        _timer.Start();
    }

    /// <summary>
    /// Stops output.
    /// </summary>
    public void Stop()
    {
        _posN = 0;
        _reply = null;
        Chunk = null;

        if (_timer.IsEnabled)
        {
            _timer.Stop();
            ChunkReceived?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Insert population of stub messages to exercise <see cref="MemoryGarden"/>.
    /// </summary>
    public static void InsertTest(MemoryGarden garden)
    {
        // HOME
        Insert(garden, BasketKind.Recent, "Show paragraph", RandOffset(0.9), 100);
        Insert(garden, BasketKind.Recent, "Show code", RandOffset(0.9), 100);
        Insert(garden, BasketKind.Recent, "Show table", RandOffset(0.9), 100);
        Insert(garden, BasketKind.Recent, "Show list", RandOffset(0.9), 100);
        Insert(garden, BasketKind.Recent, "Show indented", RandOffset(0.9), 100);
        Insert(garden, BasketKind.Recent, "Show Mega Code", RandOffset(30.0), GardenDeck.MaxLeafCount / 2);
        Insert(garden, BasketKind.Recent, "Show Mega Table", RandOffset(30.0), GardenDeck.MaxLeafCount / 2);

        var obj = Insert(garden, BasketKind.Recent, "Explain quantum entanglement like I'm 12 years old.", RandOffset(30.0), 10);
        obj.Folder = "Science";

        obj = Insert(garden, BasketKind.Recent, "Tell be about the double slit experiment.", RandOffset(30.0), 10);
        obj.Folder = "Science";

        obj = Insert(garden, BasketKind.Recent, "What are the best budget mechanical keyboards available in the UK right now?", RandOffset(30.0), 10);
        obj.Folder = "Computers";

        obj = Insert(garden, BasketKind.Recent, "Can you debug this C# code for me?", RandOffset(30.0), 10);
        obj.Folder = "Computers";

        obj = Insert(garden, BasketKind.Recent, "What are the pros and cons of moving from Windows to GhostBSD?", RandOffset(30.0), 10);
        obj.Folder = "Computers";

        obj = Insert(garden, BasketKind.Recent, "Compare the current top local LLMs for running on a 16GB VRAM GPU.", RandOffset(30.0), 10);
        obj.Folder = "Computers";

        obj = Insert(garden, BasketKind.Recent, "What's the difference between Avalonia and WPF?", RandOffset(30.0), 10);
        obj.Folder = "Computers";

        obj = Insert(garden, BasketKind.Recent, "I need a 7-day meal plan for someone who hates cooking but wants to eat healthy.", RandOffset(30.0), 10);
        obj.Folder = "Personal";

        obj = Insert(garden, BasketKind.Recent, "Can you write a short sci-fi story set on a generation ship?", RandOffset(30.0), 10);
        obj.Folder = "Personal";

        obj = Insert(garden, BasketKind.Recent, "I want to start running. Create a beginner 5K training schedule.", RandOffset(30.0), 10);
        obj.Folder = "Personal";

        obj = Insert(garden, BasketKind.Recent, "Help me name my new project.", RandOffset(30.0), 10);
        obj.Folder = "Personal";

        obj = Insert(garden, BasketKind.Recent, "Hi, can you help me write a polite email declining a job offer?", RandOffset(30.0), 10);
        obj.Folder = "Personal";

        // NOTES
        Insert(garden, BasketKind.Notes, NoteContent(), RandOffset(0.9), 1);

        // ARCHIVE
        Insert(garden, BasketKind.Recent, BasketKind.Archive, "Show quoted", RandOffset(0.9), 100);
        Insert(garden, BasketKind.Recent, BasketKind.Archive, "Translate this paragraph into natural French, keeping the same tone.", RandOffset(30.0), 10);
        Insert(garden, BasketKind.Recent, BasketKind.Archive, "Write a short, professional bio for my LinkedIn profile.", RandOffset(30.0), 10);

        // Waste
        for (int n = 1; n < 100; ++n)
        {
            Insert(garden, BasketKind.Recent, BasketKind.Waste, "Discussion " + n, RandOffset(365), 50);
        }

    }

    private static string NoteContent()
    {
        return @"Technical Notes - Local AI Setup & Testing
Last updated: 2026-05-01
Ollama + Marklet Integration Testing
I'm currently testing Marklet's connection to local models via Ollama. Key findings so far:

Gemma4:26b (Q4_K_M) runs well on the RX 9070 with Vulkan + Flash Attention. Context up to 64k is stable with `OLLAMA_KV_CACHE_TYPE=q8_0`. Beyond that it starts swapping layers and becomes noticeably slower.
gpt-oss:20b feels snappier for general use and handles 96k-128k context better, but its reasoning depth is slightly behind Gemma4 on complex tasks.
Important: Always set OLLAMA_KEEP_ALIVE=0 when using Marklet so the model stays loaded between chats.

Useful Environment Variables (for ollama.service)

```
BashEnvironment=""OLLAMA_VULKAN=1""
Environment=""OLLAMA_FLASH_ATTENTION=1""
Environment=""OLLAMA_KV_CACHE_TYPE=q8_0""
Environment=""OLLAMA_CONTEXT_LENGTH=65536""
Environment=""OLLAMA_KEEP_ALIVE=0""
```
Todo / Open Issues

Test branching with attachments (currently drops them — expected behaviour)
Verify custom title bar button positioning on GNOME (left-side preference)
Add schema migration test for new ResourceKind enum values
Measure cold-start time when model is not already loaded

These notes are purely for internal testing. Everything is running 100% locally. No cloud services involved.";
    }

    private static GardenDeck Insert(MemoryGarden garden, BasketKind origin, string msg0, TimeSpan offset, int total = 1)
    {
        return Insert(garden, origin, origin, msg0, offset, total);
    }

    private static GardenDeck Insert(MemoryGarden garden, BasketKind origin, BasketKind basket, string msg0, TimeSpan offset, int total = 1)
    {
        var obj = new GardenDeck(origin.DefaultDeck(), origin, -offset);
        obj.Model = Model;
        obj.CurrentBasket = basket;

        if (origin == BasketKind.Notes)
        {
            obj.Append(LeafFormat.UserNote, msg0);
            msg0 = "User question about notes?";
        }

        obj.Append(LeafFormat.UserMessage, msg0);
        obj.Append(LeafFormat.AssistantMessage, GetReply(msg0));

        for (int n = 1; n < total; ++n)
        {
            obj.Append(LeafFormat.UserMessage, msg0 + $" ({n})");
            obj.Append(LeafFormat.AssistantMessage, GetReply(msg0));
        }

        garden.Insert(obj);
        obj.ResetSpoof();

        return obj;
    }

    private static TimeSpan RandOffset(double days)
    {
        if (days < 1.0)
        {
            return TimeSpan.FromDays(Random.Shared.NextDouble());
        }

        return TimeSpan.FromDays(1.0 + Random.Shared.NextDouble() * days);
    }

    private static string GetReply(string? msg)
    {
        // Will include test-card output if DEBUG or asked using simple algorithm rule.
        const StringComparison Comp = StringComparison.OrdinalIgnoreCase;

        msg ??= "";
        var show = msg.IndexOf("show ", Comp);

        if (show > -1)
        {
            if (show < msg.IndexOf(" para", Comp) || show < msg.IndexOf(" text", Comp))
            {
                return GetParaReply();
            }

            if (show < msg.IndexOf(" fence", Comp) || show < msg.IndexOf(" code", Comp))
            {
                return GetFencedReply();
            }

            if (show < msg.IndexOf(" indent", Comp))
            {
                return GetIndentedReply();
            }

            if (show < msg.IndexOf(" table", Comp))
            {
                return GetTableReply();
            }

            if (show < msg.IndexOf(" list", Comp))
            {
                return GetListReply();
            }

            if (show < msg.IndexOf(" quote", Comp) || show < msg.IndexOf(" quotation", Comp))
            {
                return GetQuotedReply();
            }

            if (show < msg.IndexOf(" html", Comp))
            {
                return GetHtmlReply();
            }
        }

        return GetStandardReply();
    }

    private static string GetHello()
    {
        return $"Hello, This is {Model}.\n";
    }

    private static string GetStandardReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("**No AI model connected.** You can use the following commands to test response output:");

        sb.AppendLine();
        sb.AppendLine("    Show para, Show code, Show indent, Show table, Show list, Show quote, Show list");

        sb.AppendLine();
        sb.AppendLine("Ready");

        return sb.ToString();
    }

    private static string GetParaReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here are paragraphs, headings and links:");

        sb.AppendLine();
        sb.AppendLine("# Heading 1");
        sb.AppendLine(@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut
labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo
consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");

        sb.AppendLine();
        sb.AppendLine("## Heading 2");
        sb.AppendLine(@"Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis, id tincidunt sapien risus a quam.
Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi. Duis sapien sem, aliquet nec, commodo eget,
consequat quis, neque.");

        sb.AppendLine();
        sb.AppendLine("### Heading 3");
        sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");

        return sb.ToString();
    }

    private static string GetFencedReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here is a fenced code block:");

        sb.AppendLine();
        sb.AppendLine(@"```Bash
#!/usr/bin/env bash

set -euo pipefail

SOURCE_DIR=""/home/user/documents""
BACKUP_DIR=""/mnt/backup/$(date +%Y-%m-%d)""
LOGFILE=""/var/log/backup_$(date +%Y%m%d).log""

mkdir -p ""$BACKUP_DIR""

echo ""Starting backup at $(date)"" >> ""$LOGFILE""

rsync -aHv --delete \
--exclude={'*.tmp','cache/','node_modules/'} \
""$SOURCE_DIR/"" ""$BACKUP_DIR/"" >> ""$LOGFILE"" 2>&1

if [ $? -eq 0 ]; then
echo ""Backup completed successfully"" >> ""$LOGFILE""
else
echo ""Backup failed!"" >&2
exit 1
fi

echo ""Done at $(date)"" >> ""$LOGFILE""
```");

        sb.AppendLine();
        sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");

        return sb.ToString();
    }

    private static string GetIndentedReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here is an indented block:");

        sb.AppendLine();
        sb.AppendLine("    #!/bin/bash");
        sb.AppendLine("    ");
        sb.AppendLine("    echo \"┌──────────────────── System Snapshot ────────────────────┐\"");
        sb.AppendLine("    echo \"│  Date:         $(date '+%Y-%m-%d %H:%M:%S')             │\"");
        sb.AppendLine("    echo \"│  Hostname:     $(hostname)                              │\"");
        sb.AppendLine("    echo \"│  Uptime:       $(uptime -p)                             │\"");
        sb.AppendLine("    echo \"│  Load average: $(uptime | awk -F'load average:' '{print $2}')│\"");
        sb.AppendLine("    echo \"└─────────────────────────────────────────────────────────┘\"");

        sb.AppendLine();
        sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");

        return sb.ToString();
    }

    private static string GetTableReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here is a table:");

        sb.AppendLine();
        sb.AppendLine("| ID | Name        | Status   |");
        sb.AppendLine("|----|-------------|----------|");
        sb.AppendLine("| 1  | Alpha       | Active   |");
        sb.AppendLine("| 2  | Beta        | Pending  |");
        sb.AppendLine("| 3  | Gamma       | Disabled |");

        sb.AppendLine();
        sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");

        return sb.ToString();
    }

    private static string GetListReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here are some list blocks:");

        sb.AppendLine();
        sb.AppendLine("Here is an ordered list:");
        sb.AppendLine();
        sb.AppendLine("1. Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
        sb.AppendLine();
        sb.AppendLine("2. Sed do eiusmod tempor incididunt.");
        sb.AppendLine();
        sb.AppendLine("3. Curabitur pretium tincidunt lacus.");

        sb.AppendLine();
        sb.AppendLine("Here is an unordered list:");
        sb.AppendLine();
        sb.AppendLine("* Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
        sb.AppendLine();
        sb.AppendLine("* Sed do eiusmod tempor incididunt.");
        sb.AppendLine();
        sb.AppendLine("* Curabitur pretium tincidunt lacus.");

        sb.AppendLine();
        sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");

        return sb.ToString();
    }

    private static string GetQuotedReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here are some quoted blocks:");

        sb.AppendLine();
        sb.AppendLine("> Here are heading and paragraphs:");
        sb.AppendLine(">");
        sb.AppendLine("> ## Heading");
        sb.AppendLine(@"> Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis, id tincidunt sapien risus a quam.
Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi. Duis sapien sem, aliquet nec, commodo eget,
consequat quis, neque.");

        sb.AppendLine();
        sb.AppendLine("> Here is an ordered list:");
        sb.AppendLine(">");
        sb.AppendLine("> 1. Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
        sb.AppendLine(">");
        sb.AppendLine("> 2. Sed do eiusmod tempor incididunt.");
        sb.AppendLine(">");
        sb.AppendLine("> 3. Curabitur pretium tincidunt lacus.");

        sb.AppendLine();
        sb.AppendLine("> This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");

        return sb.ToString();
    }

    private static string GetHtmlReply()
    {
        var sb = new StringBuilder(GetHello());

        sb.AppendLine();
        sb.AppendLine("Here are some html blocks which are rendered as indented text:");

        sb.AppendLine();
        sb.AppendLine("<div>");
        sb.AppendLine("This is div 1.");
        sb.AppendLine("</div>");
        sb.AppendLine("");
        sb.AppendLine("<div>This is div 1.</div>");
        sb.AppendLine("");
        sb.AppendLine("<p>This is div 1.</p>");

        sb.AppendLine();
        sb.AppendLine("This is a <b><i>bold italic</i></b> using inline html, and this is an <unknown>unknown tag</unknown> which is shown as plain text.");

        return sb.ToString();
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        _timer.Interval = ChunkDelay;

        if (!string.IsNullOrEmpty(_reply) && _posN < _reply.Length)
        {
            int n0 = _posN;
            int count = Math.Min(SkipN, _reply.Length - n0);
            _posN += SkipN;

            Chunk = _reply.Substring(n0, count);
            ChunkReceived?.Invoke(this, EventArgs.Empty);
            return;
        }

        Stop();
    }
}
