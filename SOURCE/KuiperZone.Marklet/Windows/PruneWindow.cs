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
using Avalonia.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome.Windows;
using KuiperZone.Marklet.Shared;
using KuiperZone.Marklet.Stack.Garden;
using KuiperZone.Marklet.Tooling;

namespace KuiperZone.Marklet.Windows;

/// <summary>
/// Clean out old messages window.
/// </summary>
public sealed class PruneWindow : ChromeDialog
{
    private const DialogButtons AcceptButton = DialogButtons.DeleteAll;
    private const TimePeriod DefaultRetention = TimePeriod.ThirtyDays;
    private static readonly SortedList<int, TimePeriod> Periods = new();
    private static int s_defaultIndex;

    private readonly GardenBasket _source;
    private readonly PixieCombo _periodCombo = new();
    private readonly PixieCheckBox _pinnedCheck = new();
    private readonly PixieCheckBox _folderCheck = new();
    private readonly PixieCheckBox _projectCheck = new();
    private readonly PixieRadio _wasteRadio = new();
    private readonly PixieRadio _deleteRadio = new();
    private readonly TextBlock _countBlock = new();

    static PruneWindow()
    {
        int n = 0;

        foreach (var item in Enum.GetValues<TimePeriod>())
        {
            if (item != TimePeriod.Never)
            {
                if (item == DefaultRetention)
                {
                    s_defaultIndex = n;
                }

                Periods.Add(n++, item);
            }
        }
    }

    /// <summary>
    /// Constructor with existing Folders.
    /// </summary>
    public PruneWindow(GardenBasket source)
    {
        Kind = source.Kind;
        _source = source;

        var plural = Kind.DefaultDeck().DisplayName(DisplayStyle.Plural);
        var pluralLow = Kind.DefaultDeck().DisplayName(DisplayStyle.LowerPlural);

        MinWidth = ChromeSizes.OneCh * 60;
        MaxWidth = MinWidth;
        CanResize = false;

        Title = "Prune " + Kind.DisplayName();
        Message = "Removes Abandoned " + plural;

        foreach (var item in Periods.Values)
        {
            _periodCombo.Items.Add(item.DisplayName());
        }

        _periodCombo.Title = "Retention";
        _periodCombo.LeftSymbol = Symbols.HourglassBottom;
        _periodCombo.MinSubjectWidth = ChromeSizes.OneCh * 20;
        _periodCombo.Footer = $"Remove {pluralLow} which have not been updated within this time. Selecting \"{TimePeriod.None.DisplayName()}\" will potentially empty everything.";

        var topMargin = new Thickness(0.0, ChromeSizes.HugePx, 0.0, 0.0);

        // PRIMARY
        _pinnedCheck.LeftSymbol = Symbols.Keep;
        _pinnedCheck.Title = "Remove Pinned";
        _pinnedCheck.Footer = $"Expired pinned {pluralLow} may be removed. If unchecked, pinned items never expire.";

        _folderCheck.LeftSymbol = Symbols.Folder;
        _folderCheck.Title = $"Remove {plural} in Folders";
        _folderCheck.Footer = $"May remove {pluralLow} in folders. If unchecked, items within folders never expire.";

        _projectCheck.LeftSymbol = Symbols.Settings;
        _projectCheck.Title = "Remove Empty Project Folders";
        _projectCheck.Footer = "Project folders may be removed when they are both empty and last updated prior to retention period. " +
            "If unchecked, empty folders with projects never expire.";

        // REMOVE ACTIONS
        var child0 = new PixieGroup();
        child0.Classes.Add("chrome-high");
        child0.Classes.Add("pill-list");
        child0.ChildIndent = ChromeSizes.OneCh * 3.0;
        child0.Margin = topMargin;

        if (Kind != BasketKind.Waste)
        {
            _wasteRadio.Title = "Move to " + BasketKind.Waste.DisplayName();
            _deleteRadio.Title = "Permanently Delete";
            _deleteRadio.Footer = "This action cannot be undone.";
        }

        child0.TopTitle = "Remove Action";
        child0.Children.Add(_wasteRadio);
        child0.Children.Add(_deleteRadio);

        // Message count
        _countBlock.Margin = topMargin;
        _countBlock.FontSize = ChromeFonts.SmallFontSize;
        _countBlock.Foreground = ChromeStyling.GrayForeground;


        // Final
        Children.Add(_periodCombo);
        Children.Add(_pinnedCheck);
        Children.Add(_folderCheck);
        Children.Add(_projectCheck);
        Children.Add(child0);
        Children.Add(_countBlock);

        Closing += ClosingHandler;
        Buttons = AcceptButton | DialogButtons.Cancel;
        ButtonText.Add(AcceptButton, "Prune Now");
    }

    /// <summary>
    /// Gets the basket kind.
    /// </summary>
    public BasketKind Kind { get; }

    /// <summary>
    /// Gets options.
    /// </summary>
    public PruneOptions Options { get; } = new();

    /// <summary>
    /// Overrides.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Maintain state
        _periodCombo.SelectedIndex = s_defaultIndex;
        _pinnedCheck.IsChecked = Options.RemovePinned;
        _folderCheck.IsChecked = Options.RemoveFolderItems;
        _projectCheck.IsChecked = Options.RemoveEmptyProjects;

        if (Kind != BasketKind.Waste)
        {
            _wasteRadio.IsChecked = !Options.AlwaysDelete;
            _deleteRadio.IsChecked = Options.AlwaysDelete;
        }

        _periodCombo.ValueChanged += ValueChangedHandler;
        _pinnedCheck.ValueChanged += ValueChangedHandler;
        _folderCheck.ValueChanged += ValueChangedHandler;
        _projectCheck.ValueChanged += ValueChangedHandler;

        ValueChangedHandler(null, EventArgs.Empty);
    }

    private void SetOptions(PruneOptions opts)
    {
        const string NSpace = $"{nameof(PruneWindow)}.{nameof(SetOptions)}";

        opts.Period = Periods[_periodCombo.SelectedIndex].ToSpan();
        Diag.WriteLine(NSpace, "Index: " + _periodCombo.SelectedIndex);
        Diag.WriteLine(NSpace, "Value: " + opts.Period);

        opts.AlwaysDelete = _deleteRadio.IsChecked;
        opts.RemovePinned = _pinnedCheck.IsChecked;
        opts.RemoveFolderItems = _folderCheck.IsChecked;
        opts.RemoveEmptyProjects = _projectCheck.IsChecked;
    }

    private void ValueChangedHandler(object? _, EventArgs __)
    {
        _projectCheck.IsEnabled = _folderCheck.IsChecked;
        _projectCheck.IsChecked &= _folderCheck.IsChecked;

        var opts = new PruneOptions();
        SetOptions(opts);

        int count = _source.PruneCount(opts);
        _countBlock.Text = "Removes " + count;
        DisabledButtons = count > 0 ? DialogButtons.None : AcceptButton;
    }

    private void ClosingHandler(object? _, EventArgs __)
    {
        if (ModalResult == AcceptButton)
        {
            SetOptions(Options);
            s_defaultIndex = _periodCombo.SelectedIndex;
        }
    }

}
