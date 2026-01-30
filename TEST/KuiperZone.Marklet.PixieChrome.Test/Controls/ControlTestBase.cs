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
using Avalonia.Controls;

namespace KuiperZone.Marklet.PixieChrome.Test;

public class ControlTestBase
{
    /// <summary>
    /// Assert property value equals "expectDefault" and can be changed "change".
    /// </summary>
    protected void AssertDirect<T>(Control obj, AvaloniaProperty prop, T expectDefault, T change)
    {
        Assert.True(prop.IsDirect);

        Assert.Equal(expectDefault, obj.GetValue(prop));

        obj.SetValue(prop, change);
        Assert.Equal(change, obj.GetValue(prop));
    }

    /// <summary>
    /// Assert property value equals "expectDefault" and can be changed "change", and can be cleared back to
    /// "expectDefault".
    /// </summary>
    protected void AssertStyled<T>(Control obj, AvaloniaProperty prop, T expectDefault, T change)
    {
        Assert.False(prop.IsDirect);

        Assert.Equal(expectDefault, obj.GetValue(prop));

        obj.SetValue(prop, change);
        Assert.Equal(change, obj.GetValue(prop));

        obj.ClearValue(prop);
        Assert.Equal(expectDefault, obj.GetValue(prop));
    }

}
