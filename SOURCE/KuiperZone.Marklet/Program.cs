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

using Avalonia;
using KuiperZone.Marklet.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.Tooling;

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
        ConditionalDebug.EnableNamespace(nameof(ContentViewer));
        ConditionalDebug.EnableNamespace(nameof(ContentLeaf));
        ConditionalDebug.EnableNamespace(nameof(GardenBinView));
        ConditionalDebug.EnableNamespace(nameof(SessionControl));
        ConditionalDebug.EnableNamespace(nameof(TopicControl));

        ConditionalDebug.EnableNamespace("ChromeWindow");
        // ConditionalDebug.EnableNamespace("CrossTracker");
        // ConditionalDebug.EnableNamespace("CrossTextBlock");
        // ConditionalDebug.EnableNamespace("DispatchCoalescer");
        // ConditionalDebug.EnableNamespace("StyledContainer");
        // ConditionalDebug.EnableNamespace("MarkDocument");
        // ConditionalDebug.EnableNamespace("MarkTextHost");
        // ConditionalDebug.EnableNamespace("MarkView");

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
