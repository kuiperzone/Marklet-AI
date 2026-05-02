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
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Windows;
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
        Diag.EnableNamespace("");
        Diag.EnableNamespace(nameof(App));
        Diag.EnableNamespace(nameof(ChromeApplication));
        Diag.EnableNamespace(nameof(MainWindow));

        // Diag.EnableNamespace("MetaOps");
        // Diag.EnableNamespace("DeckOps");
        // Diag.EnableNamespace("LeafOps");
        // Diag.EnableNamespace(nameof(MemoryGarden));
        // Diag.EnableNamespace(nameof(SqliteProvider));
        // Diag.EnableNamespace(nameof(GardenDeck));
        // Diag.EnableNamespace(nameof(GardenLeaf));
        // Diag.EnableNamespace(nameof(GardenBasket));

        //ConditionalDebug.EnableNamespace(nameof(MainMission));
        // Diag.EnableNamespace(nameof(CarouselControl));
        // Diag.EnableNamespace(nameof(SettingsCarousel<>));
        // Diag.EnableNamespace(nameof(DatabaseSettingsCarousel));
        // Diag.EnableNamespace(nameof(DeckViewer));
        // Diag.EnableNamespace(nameof(BasketView));
        // Diag.EnableNamespace(nameof(DeckCard));
        // Diag.EnableNamespace(nameof(FolderView));
        // Diag.EnableNamespace(nameof(CardMenu));
        // Diag.EnableNamespace("GroupRenamer");

        // Diag.EnableNamespace(nameof(PixieCombo));
        // Diag.EnableNamespace(nameof(LightButton));
        // Diag.EnableNamespace(nameof(CardMenu));
        // Diag.EnableNamespace(nameof(PruneWindow));
        // Diag.EnableNamespace(nameof(SessionLeaf));
        // Diag.EnableNamespace(nameof(ChromeWindow));

        // Diag.EnableNamespace("CrossTracker");
        // Diag.EnableNamespace("CrossTextBlock");
        // Diag.EnableNamespace("MarkView");
        // Diag.EnableNamespace("MarkTextHost");
        // Diag.EnableNamespace("MarkDocument");
        // Diag.EnableNamespace("StyledContainer");

        // Diag.EnableNamespace("DispatchCoalescer");

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
