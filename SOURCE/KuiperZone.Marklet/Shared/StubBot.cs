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
    private const string Model = "stub-bot";
    private static readonly bool IsDebug;
    private static readonly TimeSpan StartDelay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ChunkDelay = TimeSpan.FromMilliseconds(20);

    private readonly DispatcherTimer _timer = new();
    private int _posN;
    private string? _reply;

    static StubBot()
    {
#if DEBUG
        IsDebug = true;
#else
        IsDebug = false;
#endif
    }

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
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            ChunkReceived?.Invoke(this, EventArgs.Empty);
        }

        _posN = 0;
        _reply = null;
        Chunk = null;
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
        Insert(garden, BasketKind.Recent, "Mega chat 1", RandOffset(30.0), GardenDeck.MaxLeafCount / 2);
        Insert(garden, BasketKind.Recent, "Mega chat 2", RandOffset(30.0), GardenDeck.MaxLeafCount / 2);

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
        Insert(garden, BasketKind.Notes, "Notes", RandOffset(0.9), 10);

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

    private static GardenDeck Insert(MemoryGarden garden, BasketKind origin, string msg0, TimeSpan offset, int total = 1)
    {
        return Insert(garden, origin, origin, msg0, offset, total);
    }

    private static GardenDeck Insert(MemoryGarden garden, BasketKind origin, BasketKind basket, string msg0, TimeSpan offset, int total = 1)
    {
        var obj = new GardenDeck(origin.DefaultDeck(), origin, -offset);
        obj.Model = Model;
        obj.Basket = basket;
        obj.Append(LeafKind.User, msg0);
        obj.Append(LeafKind.Assistant, GetReply(msg0));

        for (int n = 1; n < total; ++n)
        {
            obj.Append(LeafKind.User, "User message " + n);
            obj.Append(LeafKind.Assistant, GetReply());
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

    private static string GetReply(string? msg = null)
    {
        // Will include test-card output if DEBUG or asked using simple algorithm rule.
        const StringComparison Comp = StringComparison.OrdinalIgnoreCase;

        msg ??= "";
        var show = msg.IndexOf("show ", Comp);

        if (show > -1)
        {
            if (show < msg.IndexOf(" para", Comp) || show < msg.IndexOf(" text", Comp))
            {
                return GetParaReply(true);
            }

            if (show < msg.IndexOf(" fence", Comp) || show < msg.IndexOf(" code", Comp))
            {
                return GetFencedReply(true);
            }

            if (show < msg.IndexOf(" indent", Comp))
            {
                return GetIndentedReply(true);
            }

            if (show < msg.IndexOf(" table", Comp))
            {
                return GetTableReply(true);
            }

            if (show < msg.IndexOf(" list", Comp))
            {
                return GetListReply(true);
            }

            if (show < msg.IndexOf(" quote", Comp))
            {
                return GetQuotedReply(true);
            }
        }

        switch (Random.Shared.Next(6))
        {
            case 0: return GetParaReply(false);
            case 1: return GetFencedReply(false);
            case 2: return GetIndentedReply(false);
            case 3: return GetTableReply(false);
            case 4: return GetListReply(false);
            case 5: return GetQuotedReply(false);
            default:
                throw new InvalidOperationException("Invalid random");
        }
    }

    private static string GetParaReply(bool force)
    {
        var sb = new StringBuilder(GetLeader());

        if (IsDebug || force)
        {
            sb.AppendLine();
            sb.AppendLine("What follows is a test-card for text, headings and links.");

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
        }

        return sb.ToString();
    }

    private static string GetFencedReply(bool force)
    {
        var sb = new StringBuilder(GetLeader());

        if (IsDebug || force)
        {
            sb.AppendLine();
            sb.AppendLine("What follows is a test-card for a fenced code block.");

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
        }

        return sb.ToString();
    }

    private static string GetIndentedReply(bool force)
    {
        var sb = new StringBuilder(GetLeader());

        if (IsDebug || force)
        {
            sb.AppendLine();
            sb.AppendLine("What follows is a test-card for an indented block.");

            sb.AppendLine();
            sb.AppendLine(@"    #!/bin/bash

    echo ""┌──────────────────── System Snapshot ────────────────────┐""
    echo ""│  Date:         $(date '+%Y-%m-%d %H:%M:%S')             │""
    echo ""│  Hostname:     $(hostname)                              │""
    echo ""│  Uptime:       $(uptime -p)                             │""
    echo ""│  Load average: $(uptime | awk -F'load average:' '{print $2}')│""
    echo ""└─────────────────────────────────────────────────────────┘""");

            sb.AppendLine();
            sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");
        }

        return sb.ToString();
    }

    private static string GetTableReply(bool force)
    {
        var sb = new StringBuilder(GetLeader());

        if (IsDebug || force)
        {
            sb.AppendLine();
            sb.AppendLine("What follows is a test-card for a block containing table data.");

            sb.AppendLine();
            sb.AppendLine(@"| ID | Name        | Status   |
|----|-------------|----------|
| 1  | Alpha       | Active   |
| 2  | Beta        | Pending  |
| 3  | Gamma       | Disabled |");

            sb.AppendLine();
            sb.AppendLine("This is [link text](http://example.com) to http://example.com. This is an example of `inline code`.");
        }

        return sb.ToString();
    }

    private static string GetListReply(bool force)
    {
        var sb = new StringBuilder(GetLeader());

        if (IsDebug || force)
        {
            sb.AppendLine();
            sb.AppendLine("What follows is a test-card for list blocks.");

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
        }

        return sb.ToString();
    }

    private static string GetQuotedReply(bool force)
    {
        var sb = new StringBuilder(GetLeader());

        if (IsDebug || force)
        {
            sb.AppendLine();
            sb.AppendLine("What follows is a test-card for various quoted blocks.");

            sb.AppendLine();
            sb.AppendLine("> Here are heading and paragraphs:");
            sb.AppendLine(">");
            sb.AppendLine("> ## Heading");
            sb.AppendLine(@"> Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis, id tincidunt sapien risus a quam.
Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi. Duis sapien sem, aliquet nec, commodo eget,
consequat quis, neque.");

            sb.AppendLine();
            sb.AppendLine(@"> | ID | Name        | Status   |
> |----|-------------|----------|
> | 1  | Alpha       | Active   |
> | 2  | Beta        | Pending  |
> | 3  | Gamma       | Disabled |");

            sb.AppendLine();
            sb.AppendLine("> Here is fenced code:");
            sb.AppendLine(@"> ```bash
> #!/usr/bin/env bash
> BACKUP_DIR=""/mnt/backup/$(date +%Y-%m-%d)""
> mkdir -p ""$BACKUP_DIR""
>```");


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
        }

        return sb.ToString();
    }

    private static string GetLeader()
    {
        return $"Hello, this is {Model}.\n";
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        _timer.Interval = ChunkDelay;

        if (!string.IsNullOrEmpty(_reply) && _posN < _reply.Length)
        {
            Chunk = _reply[_posN++].ToString();
            ChunkReceived?.Invoke(this, EventArgs.Empty);
            return;
        }

        Stop();
    }
}
