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

using Avalonia.Threading;
using KuiperZone.Marklet.Stack.Garden;

namespace KuiperZone.Marklet.Shared;

/// <summary>
/// A temporary class to generate stub messages.
/// </summary>
sealed class StubMessages
{
    private const string Model = "gpt-oss";
    private static readonly TimeSpan StartDelay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ChunkDelay = TimeSpan.FromMilliseconds(10);

    private readonly DispatcherTimer _timer = new();
    private MessageKind _next;
    private int _posN;
    private string? _message;

    /// <summary>
    /// Stub message kinds.
    /// </summary>
    public enum MessageKind { Para, Fenced, Indented, Table, QuoteList }

    public StubMessages()
    {
        _timer.Tick += TimerTickHandler;
    }

    public event EventHandler<EventArgs>? ChunkReceived;

    public string? Chunk { get; private set; }

    public void StartNext()
    {
        Stop();

        _message = GetMessage(_next);
        _next = Increment(_next);

        _timer.Interval = StartDelay;
        _timer.Start();
    }

    public void Stop()
    {
        _posN = 0;
        _message = null;
        Chunk = null;

        if (_timer.IsEnabled)
        {
            _timer.Stop();
            ChunkReceived?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Initial population of stub messages.
    /// </summary>
    public static void Populate(MemoryGarden garden)
    {
        // HOME
        garden.Insert(NewRecentMessage("Lorem ipsum dolor sit amet", TimeSpan.FromDays(-29.99), MessageKind.Para, 100));
        garden.Insert(NewRecentMessage("Duis sapien sem", TimeSpan.FromDays(-2.8), MessageKind.Table));
        garden.Insert(NewRecentMessage("Etiam tempor", TimeSpan.FromDays(-0.4), MessageKind.QuoteList));

        // ARCHIVED

        // Grouped
        var child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, TimeSpan.FromDays(-35.2));
        child.Basket = BasketKind.Archive;
        child.Model = Model;
        child.Title = "Boring Latin";
        child.Folder = "Archived A";
        child.Append(LeafKind.User, "Say something in a language everyone one understands.");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.Para));
        garden.Insert(child);
        child.ResetSpoof();

        child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, TimeSpan.FromDays(-65.76));
        child.Basket = BasketKind.Archive;
        child.Model = Model;
        child.Title = "Some data";
        child.Folder = "Archived A";
        child.IsPinned = true;
        child.Append(LeafKind.User, "Show me some data.");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.Table));
        garden.Insert(child);
        child.ResetSpoof();

        child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, TimeSpan.FromDays(-8.32));
        child.Basket = BasketKind.Archive;
        child.Model = Model;
        child.Title = "Stuff";
        child.Folder = "Archived B";
        child.Append(LeafKind.User, "Say something else.");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.QuoteList));
        garden.Insert(child);
        child.ResetSpoof();

        child = new GardenDeck(DeckKind.Note, BasketKind.Notes, TimeSpan.FromDays(-1.63));
        child.Basket = BasketKind.Archive;
        child.Model = Model;
        child.Title = "More stuff";
        child.Folder = "Archived B";
        child.Append(LeafKind.User, "Say more.");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.Indented));
        garden.Insert(child);
        child.ResetSpoof();

        // Floating
        child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, TimeSpan.FromDays(-29.99));
        child.Basket = BasketKind.Archive;
        child.Model = Model;
        child.Title = "Mend it. Fix it.";
        child.Folder = null;
        child.Append(LeafKind.User, "I have some code. It no work. Make it work. Mend it. Fix it!");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.Fenced));
        garden.Insert(child);
        child.ResetSpoof();

        // WASTE
        child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, TimeSpan.FromDays(-29.99));
        child.Basket = BasketKind.Waste;
        child.Title = "Indented text";
        child.Model = Model;
        child.Folder = null;
        child.Append(LeafKind.User, "Show me some indented text");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.Indented));
        garden.Insert(child);
        child.ResetSpoof();

        child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, TimeSpan.FromDays(-32.9));
        child.Basket = BasketKind.Waste;
        child.Model = Model;
        child.Title = "Yet more Latin";
        child.Folder = null;
        child.Append(LeafKind.User, "Show me more latin");
        child.Append(LeafKind.Assistant, GetMessage(MessageKind.Para));
        garden.Insert(child);
        child.ResetSpoof();
    }

    public static string GetMessage(MessageKind kind)
    {
        switch (kind)
        {
            case MessageKind.Para:
                return @"This is stub message containing paragraphs.

# Heading 1
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.

Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis,
id tincidunt sapien risus a quam. Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi.
Duis sapien sem, aliquet nec, commodo eget, consequat quis, neque.

## Heading 2
Curabitur pretium tincidunt lacus. Nulla gravida orci a odio. Nullam varius, turpis et commodo pharetra, est eros bibendum elit,
nec luctus magna felis sollicitudin mauris. Integer in mauris eu nibh euismod gravida. Duis ac tellus et risus vulputate
vehicula. Donec lobortis risus a elit. Etiam tempor.

### Heading 3
This is [link text](http://example.com) to http://example.com.";

            case MessageKind.Fenced:
                return @"This is stub message containing fenced code.

```Bash
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
```
Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            case MessageKind.Indented:
                return @"This is stub message containing indented text.

    #!/bin/bash

    echo ""┌──────────────────── System Snapshot ────────────────────┐""
    echo ""│  Date:         $(date '+%Y-%m-%d %H:%M:%S')             │""
    echo ""│  Hostname:     $(hostname)                              │""
    echo ""│  Uptime:       $(uptime -p)                             │""
    echo ""│  Load average: $(uptime | awk -F'load average:' '{print $2}')│""
    echo ""└─────────────────────────────────────────────────────────┘""

Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

            case MessageKind.Table:
                return @"This is stub message containing a table.

| ID | Name        | Status   |
|----|-------------|----------|
| 1  | Alpha       | Active   |
| 2  | Beta        | Pending  |
| 3  | Gamma       | Disabled |

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            case MessageKind.QuoteList:
                return @"This is stub message containing quotations and lists.

You said:

> Say something.

Here is an ordered list:

1. Lorem ipsum dolor sit amet, consectetur adipiscing elit.
2. Sed do eiusmod tempor incididunt.
3. Curabitur pretium tincidunt lacus.

And here is an unordered list:

* Lorem ipsum dolor sit amet, consectetur adipiscing elit.
* Sed do eiusmod tempor incididunt.
* Curabitur pretium tincidunt lacus.

Here is a quote with a table:

> | ID | Name        | Status   |
> |----|-------------|----------|
> | 1  | Alpha       | Active   |
> | 2  | Beta        | Pending  |
> | 3  | Gamma       | Disabled |

And here is quote with fenced code:

> ```bash
> #!/usr/bin/env bash
> BACKUP_DIR=""/mnt/backup/$(date +%Y-%m-%d)""
> mkdir -p ""$BACKUP_DIR""
>```

Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            default:
                throw new ArgumentException("Invalid message kind");
        }

    }

    private static MessageKind Increment(MessageKind kind)
    {
        kind += 1;

        if (!Enum.IsDefined(kind))
        {
            return MessageKind.Para;
        }

        return kind;
    }

    private static GardenDeck NewRecentMessage(string title, TimeSpan offset, MessageKind start, int iter = 1)
    {
        var child = new GardenDeck(DeckKind.Chat, BasketKind.Recent, offset);
        child.Model = Model;
        child.Title = title;

        for (int n = 0; n < iter; ++n)
        {
            child.Append(LeafKind.User, "Say something.");
            child.Append(LeafKind.Assistant, GetMessage(start));

            start = Increment(start);
            child.Append(LeafKind.User, "Give me some bash script.");
            child.Append(LeafKind.Assistant, GetMessage(start));

            start = Increment(start);
            child.Append(LeafKind.User, "Give me some code as indented text.");
            child.Append(LeafKind.Assistant, GetMessage(start));

            start = Increment(start);
            child.Append(LeafKind.User, "Show me a table.");
            child.Append(LeafKind.Assistant, GetMessage(start));

            start = Increment(start);
            child.Append(LeafKind.User, "Show me formatting of quotes and lists.");
            child.Append(LeafKind.Assistant, GetMessage(start));
        }

        child.ResetSpoof();
        return child;
    }

    private void TimerTickHandler(object? _, EventArgs __)
    {
        _timer.Interval = ChunkDelay;

        if (!string.IsNullOrEmpty(_message) && _posN < _message.Length)
        {
            Chunk = _message[_posN++].ToString();
            ChunkReceived?.Invoke(this, EventArgs.Empty);
            return;
        }

        Stop();
    }
}
