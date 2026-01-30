// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// AUTHOR    : Andrew Thomas
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

using KuiperZone.Marklet.Tooling.Markdown;

namespace KuiperZone.Marklet.Tooling.Test;

public class BaseTest
{
    protected static readonly string Indent = new(' ', 8);

    protected static void WriteIndentedDebug(string? s = null)
    {
#if DEBUG
        // Prevent ignoring empty
        s ??= "//";
        s = s?.Replace("\\", "\\\\").Replace("\0", "\\0").Replace("\n", "\\n").Replace("\t", "\\t");
        Console.WriteLine(Indent + s);
#endif
    }

    protected static void WriteAssertEqual(bool value, string propName)
    {
        if (value)
        {
            WriteIndentedDebug($"Assert.True({propName});");
        }
        else
        {
            WriteIndentedDebug($"Assert.False({propName});");
        }
    }

    protected static void WriteAssertEqual<T>(T value, string propName) where T : struct
    {
        var t = value.GetType();

        if (t.IsEnum)
        {
            string qual = t.Name + ".";
            string flags = qual + value.ToString()!.Replace(", ", " | " + qual);
            WriteIndentedDebug($"Assert.Equal({flags}, {propName});");
            return;
        }

        WriteIndentedDebug($"Assert.Equal({value}, {propName});");
    }

    protected static void WriteAssertEqual(char value, string propName)
    {
        WriteIndentedDebug($"Assert.Equal('{value}', {propName});");
    }

    protected static void WriteAssertEqual(string? value, string propName)
    {
        if (value == null)
        {
            WriteIndentedDebug($"Assert.Null({propName});");
        }
        else
        {
            WriteIndentedDebug($"Assert.Equal(\"{value}\", {propName});");
        }
    }

    protected static void WriteAssertEqual(object? value, string propName)
    {
        if (value == null)
        {
            WriteIndentedDebug($"Assert.Null({propName});");
        }
        else
        {
            WriteIndentedDebug($"Assert.Equal({value}, {propName});");
        }
    }

    protected static void WriteAssertCount<T>(IReadOnlyCollection<T> value, string propName)
    {
        if (value.Count == 0)
        {
            WriteIndentedDebug($"Assert.Empty({propName});");
        }
        else
                if (value.Count == 1)
        {
            WriteIndentedDebug($"Assert.Single({propName});");
        }
        else
        {
            WriteIndentedDebug($"Assert.Equal({value.Count}, {propName}.Count);");
        }
    }

    protected static void WriteAssertLength<T>(T[] value, string propName)
    {
        if (value.Length == 0)
        {
            WriteIndentedDebug($"Assert.Empty({propName});");
        }
        else
        if (value.Length == 1)
        {
            WriteIndentedDebug($"Assert.Single({propName});");
        }
        else
        {
            WriteIndentedDebug($"Assert.Equal({value.Length}, {propName}.Length);");
        }
    }

    protected static void WriteAssertNull(string propName)
    {
        // We cheat here - value is either: "True(" or "False("
        WriteIndentedDebug($"Assert.Null({propName});");
    }

    protected static void WriteAssertNotNull(string propName)
    {
        // We cheat here - value is either: "True(" or "False("
        WriteIndentedDebug($"Assert.NotNull({propName});");
    }

    protected static void WriteAssertEmpty(string propName)
    {
        // We cheat here - value is either: "True(" or "False("
        WriteIndentedDebug($"Assert.Empty({propName});");
    }

    protected static string ExpandStyling(InlineStyling flags)
    {
        return nameof(InlineStyling) + "." + flags.ToString().Replace(", ", " | " + nameof(InlineStyling) + ".");
    }

    protected static MarkDocument UpdateWriteOut(string? content, MarkOptions opts = default)
    {
        var obj = new MarkDocument();
        obj.Update(content, opts);
        WriteTestCode(obj);
        return obj;
    }

    protected static void WriteTestCode(MarkDocument obj)
    {
        const string ObjName = "obj";
        string blocksProp = $"{ObjName}";

        WriteIndentedDebug();
        WriteIndentedDebug();

        for (int bN = 0; bN < obj.Count; ++bN)
        {
            var blockN = obj[bN];
            var blockName = $"{blocksProp}[{bN}]";

            WriteIndentedDebug();
            WriteIndentedDebug($"// " + blockN.ToString(TextFormat.Markdown));

            WriteAssertEqual(blockN.Kind, $"{blockName}.{nameof(blockN.Kind)}");
            WriteAssertEqual(blockN.QuoteLevel, $"{blockName}.{nameof(blockN.QuoteLevel)}");
            WriteAssertEqual(blockN.ListLevel, $"{blockName}.{nameof(blockN.ListLevel)}");
            WriteAssertEqual(blockN.ListOrder, $"{blockName}.{nameof(blockN.ListOrder)}");
            WriteAssertEqual(blockN.ListBullet, $"{blockName}.{nameof(blockN.ListBullet)}");

            if (blockN.Lang != null)
            {
                WriteAssertEqual(blockN.Lang, $"{blockName}.{nameof(blockN.Lang)}");
            }

            var tab = blockN.Table;

            if (tab != null)
            {
                // Don't assert everything here
                // We only want to confirm structure
                string tabName = $"{blockName}.{nameof(blockN.Table)}";
                WriteAssertNotNull(tabName);
                WriteAssertEqual(tab.RowCount, $"{tabName}?.{nameof(tab.RowCount)}");
                WriteAssertEqual(tab.ColCount, $"{tabName}?.{nameof(tab.ColCount)}");
            }

            var elems = blockN.Elements;

            for (int iN = 0; iN < elems.Count; ++iN)
            {
                var elemN = elems[iN];
                string elemName = $"{blockName}.{nameof(blockN.Elements)}[{iN}]";

                WriteAssertEqual(elemN.Styling, $"{elemName}.{nameof(elemN.Styling)}");
                WriteAssertEqual(elemN.Text, $"{elemName}.{nameof(elemN.Text)}");

                var link = elems[iN].Link;

                if (link != null)
                {
                    string linkName = $"{elemName}.{nameof(elemN.Link)}";
                    WriteAssertEqual(link.IsImage, $"{linkName}?.{nameof(link.IsImage)}");
                    WriteAssertEqual(link.ToString(), $"{linkName}?.ToString()");
                    WriteAssertEqual(link.Title, $"{linkName}?.{nameof(link.Title)}");
                }
            }
        }

        // Write count at end
        WriteIndentedDebug();
        WriteAssertCount(obj, blocksProp);
        WriteIndentedDebug();
    }


}