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

using Avalonia;
using KuiperZone.Marklet.Controls;
using KuiperZone.Marklet.Controls.Internal;
using KuiperZone.Marklet.Controls.Internal.Mission;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;
using KuiperZone.Marklet.Windows;

namespace KuiperZone.Marklet;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Selectively enable logging namespace
        // otherwise there is just too much!
        ConditionalDebug.EnableNamespace("");
        ConditionalDebug.EnableNamespace(nameof(App));
        ConditionalDebug.EnableNamespace(nameof(ChromeApplication));
        ConditionalDebug.EnableNamespace(nameof(MainWindow));
        ConditionalDebug.EnableNamespace(nameof(MainMission));
        // ConditionalDebug.EnableNamespace(nameof(ChatSessionViewer));
        ConditionalDebug.EnableNamespace(nameof(BasketView));
        ConditionalDebug.EnableNamespace(nameof(DeckCard));
        ConditionalDebug.EnableNamespace(nameof(FolderView));
        ConditionalDebug.EnableNamespace(nameof(CardMenu));
        ConditionalDebug.EnableNamespace(nameof(MemoryGarden));
        ConditionalDebug.EnableNamespace(nameof(GardenDeck));
        ConditionalDebug.EnableNamespace(nameof(GardenLeaf));
        ConditionalDebug.EnableNamespace(nameof(GardenBasket));
        // ConditionalDebug.EnableNamespace("GroupRenamer");
        ConditionalDebug.EnableNamespace(nameof(PixieCombo));
        ConditionalDebug.EnableNamespace(nameof(LightButton));
        ConditionalDebug.EnableNamespace(nameof(CardMenu));
        ConditionalDebug.EnableNamespace(nameof(PruneWindow));
        // ConditionalDebug.EnableNamespace(nameof(SessionLeaf));
        // ConditionalDebug.EnableNamespace(nameof(ChromeWindow));
        // ConditionalDebug.EnableNamespace("CrossTracker");
        // ConditionalDebug.EnableNamespace("CrossTextBlock");
        // ConditionalDebug.EnableNamespace("MarkView");
        // ConditionalDebug.EnableNamespace("MarkTextHost");
        // ConditionalDebug.EnableNamespace("MarkDocument");
        // ConditionalDebug.EnableNamespace("DispatchCoalescer");
        // ConditionalDebug.EnableNamespace("StyledContainer");

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            // Necessary? But good to do
            ChromeApplication.Current.Host.ReleaseLock();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        // No LogToTrace() - Window resizing seems faster without it
        return AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont();
    }
}
