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

using System.Globalization;
using System.Text;
using Avalonia.Controls;
using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.PixieChrome.Controls.Internal;

/// <summary>
/// Hosts controls displaying one or more <see cref="IReadOnlyMarkBlock"/> instances.
/// </summary>
/// <remarks>
/// Hosts may be cached and updated. Note <see cref="ConsumeUpdates"/> must be called immediately after construction.
/// </remarks>
internal abstract class MarkVisualHost
{
    private Control? _control;

    protected MarkVisualHost(MarkView owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public MarkView Owner { get; }

    /// <summary>
    /// Gets the quote level (subclass must set).
    /// </summary>
    public int QuoteLevel { get; protected set; }

    /// <summary>
    /// Gets the list level (subclass must set).
    /// </summary>
    public int ListLevel { get; protected set; }

    /// <summary>
    /// Gets the first <see cref="ICrossTrackable"/> instance.
    /// </summary>
    /// <remarks>
    /// The value is null where the host does not contain selectable text (i.e. a "rule").
    /// </remarks>
    public ICrossTrackable? Track0 { get; protected set; }

    /// <summary>
    /// Gets the last <see cref="ICrossTrackable"/> instance.
    /// </summary>
    public ICrossTrackable? Track1 { get; protected set; }

    /// <summary>
    /// Gets the child. Subclass must set on construction.
    /// </summary>
    public Control Control
    {
        get { return _control ?? throw new InvalidOperationException("Child not set"); }
        protected set { _control = value; }
    }

    /// <summary>
    /// Create appropriate instance of <see cref="MarkBlockHost"/> for given source.
    /// </summary>
    /// <remarks>
    /// Always consumes exactly one block.
    /// </remarks>
    public static MarkVisualHost New(MarkView owner, IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index)
    {
        var source = sequence[index];

        if (source.QuoteLevel > 0 || source.ListLevel > 0)
        {
            var host = new MarkLevelHost(owner, source);

            if (host.ConsumeUpdates(sequence, ref index) != MarkConsumed.Changed)
            {
                throw new InvalidOperationException($"{host.GetType().Name} failed to consume initial blocks");
            }

            return host;
        }

        return MarkBlockHost.New(owner, sequence, ref index);
    }

    /// <summary>
    /// Refreshes colors and sizes, but not content.
    /// </summary>
    public abstract void Refresh(bool isFirst, bool isLast);

    /// <summary>
    /// Consumes updates from "source" sequence and increments "index" on success.
    /// </summary>
    public abstract MarkConsumed ConsumeUpdates(IReadOnlyList<IReadOnlyMarkBlock> sequence, ref int index);

    /// <summary>
    /// To be extended for debug, unit test and inspection.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder();
        Append(buffer, nameof(Control), _control?.GetType().Name);
        Append(buffer, nameof(QuoteLevel), QuoteLevel);
        Append(buffer, nameof(ListLevel), ListLevel);
        return buffer.ToString();
    }

    /// <summary>
    /// For use by subclass.
    /// </summary>
    protected void Append(StringBuilder buffer, string name, object? value)
    {
        if (buffer.Length != 0)
        {
            buffer.Append(", ");
        }

        buffer.Append(name);
        buffer.Append('=');
        buffer.Append(value?.ToString() ?? "NULL");
    }

    /// <summary>
    /// For use by subclass.
    /// </summary>
    protected void Append(StringBuilder buffer, string name, double value)
    {
        Append(buffer, name, value.ToString(CultureInfo.InvariantCulture));
    }

}