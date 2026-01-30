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

using Avalonia.Layout;
using KuiperZone.Marklet.PixieChrome.Controls;

namespace KuiperZone.Marklet.PixieChrome.CarouselPages.Internal;

internal sealed class PixieMarkViewPage : PixiePageBase
{
    public PixieMarkViewPage()
    {
        Title = nameof(PixieMarkView);
        Symbol = Symbols.Markdown;

        // SWITCH
        var group = NewGroup(nameof(PixieMarkView));

        var control = NewControl<PixieMarkView>(group);
        control.ChildControl.Content = GetAssistantMessage();
        control.ChildControl.IsChromeStyled = true;
        control.Footer = "ASSISTANT STRETCH";

        control = NewControl<PixieMarkView>(group);
        control.ChildControl.Content = GetUserMessage();
        control.ChildControl.IsChromeStyled = true;
        control.ChildControl.HorizontalAlignment = HorizontalAlignment.Right;
        control.Footer = "USER RIGHT";

        control = NewControl<PixieMarkView>(group);
        control.ChildControl.Content = GetShortUserCode();
        control.ChildControl.IsChromeStyled = true;
        control.ChildControl.HorizontalAlignment = HorizontalAlignment.Right;
        control.Footer = "USER FENCE";
    }

    private static string GetUserMessage()
    {
        return @"User Message

Lorem ipsum dolor sit amet.

Fenced code:
```bash
ls -la /tmp
```

Indented text:

    THIS IS INDENTED

Link: [link text](http://example.com).
";
    }

    private static string GetShortUserCode()
    {
        return @"```
123
```";
    }

    private static string GetAssistantMessage()
    {
        return @"Assistant Message

# Heading 1
Proin porttitor, orci nec nonummy molestie, enim est eleifend mi, non fermentum diam nisl sit amet erat. Duis semper. Duis arcu massa, scelerisque vitae, consequat in, pretium a, enim. Pellentesque congue. Ut in risus volutpat libero pharetra tempor.

## Heading 2
Fenced code:
```bash
#!/usr/bin/env bash
echo 'Starting process...'
ls -la /tmp
```

Fenced code with wrapping:
```csharp
// Fence code:
child = _memoryGarden.Insert(BinKind.Archive);child = _memoryGarden.Insert(BinKind.Archive);child = _memoryGarden.Insert(BinKind.Archive);
```

### Heading 3
The following is indented:

    // Indented code
    child = _memoryGarden.Insert(BinKind.Archive);child = _memoryGarden.Insert(BinKind.Archive);child = _memoryGarden.Insert(BinKind.Archive);child = _memoryGarden.Insert(BinKind.Archive);


#### Heading 4
The following is a table:

| ID | Name        | Status   |
|----|-------------|----------|
| 1  | Alpha       | Active   |
| 2  | Beta        | Pending  |
| 3  | Gamma       | Disabled |

##### Heading 5
The following is an ordered list:

1. List item1
2. List item2

    3. Sub itemA
    4. Sub itemB

The following is an unordered list:

* List item1
* List item2
    * Sub itemA

###### Heading 6
The following is quote:

> This is quote
>> This is a reply

The following is rule:

***

Text containing link: [link text](http://example.com) which points to http://example.com.
";
    }

}