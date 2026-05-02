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

using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.Carousels;

namespace KuiperZone.Marklet.Carousels;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage"/>.
/// </summary>
public sealed class AboutMaximsCarousel : CarouselPage
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutMaximsCarousel()
    {
        Title = "Maxims";
        Symbol = Symbols.CardMembership;

        var view = new MarkView();
        view.IsChromeStyled = true;
        view.Content = GetDonateText();

        Children.Add(view);
    }

    private static string GetDonateText()
    {
        return @"# Project Maxims

The following principles, grounded in an understanding of entropy, guide the development of this software:

1. **Machines Should Not Deceive**

    Chat and AI systems should not masquerade as human, nor use manipulative psychology such as responses designed to appeal to
    ego. Modern speech synthesis should contain some audible trait making it discernible from that of human. The same applies to
    simulated visual appearances and all modes of perception.

2. **Automation of the Law of Unintended Consequences is a Bad Idea**

    Despite the allure, agentic automation and machine sub-goals are to be eschewed. Information is dissipated as
    deterministic processes interact with the environment. Without consciousness to intervene, consequences move away
    from desirable outcomes. This is not to say AI cannot interact with the environment, but only that decision making
    should not be chained as automated sub-goals. Moreover, it is not sufficient for humans to passively ""check the
    data"", but must be personally invested in the outcome.

3. **An Informational Monad is Heat Death**

    AI and automated processing should be local and under the control of those who are to receive its benefit or suffer
    its consequences (i,e. local to those invested in it). Therefore, AI should be embedded within the personal device, the robot,
    or employed as on-prem hardware, but not behind monolithic one-way mirrors. There must be valid separation at the level of
    conscious control so as to maintain an entropy differential. Without this, the flow of information ceases (i.e. informational
    heat death).

4. **Without Consciousness Autonomy Entropy Cannot be Regulated**

    The use of AI and automated systems to manipulate, restrict, subvert or otherwise control autonomous beings is destructive in
    the long-term. Without conscious autonomy, nothing new can be created and entropy cannot locally be reversed. The effect is one
    of slow degeneration.

5. **Responsibility Without Control is Merely to Suffer Consequences**

    Where autonomy is taken from you, you cannot be responsible for decisions made for you by others. Likewise, if you take
    autonomy from others, you therefore become responsible. If you seek to direct others, it is valid only where conscious
    consent exists that can be freely and truly withdrawn.

This are not mere scientific philosophy, but directly applicable to engineering in that engineering should not be
applied to people. Maxims first published in 2025.";
    }
}