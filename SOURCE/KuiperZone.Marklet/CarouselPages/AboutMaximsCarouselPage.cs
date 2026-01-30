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

using KuiperZone.Marklet.PixieChrome.Controls;
using KuiperZone.Marklet.PixieChrome;
using KuiperZone.Marklet.PixieChrome.CarouselPages;
using KuiperZone.Marklet.Shared;

namespace KuiperZone.Marklet.CarouselPages;

/// <summary>
/// Concrete subclass of <see cref="CarouselPage"/>.
/// </summary>
public sealed class AboutMaximsCarouselPage : CarouselPage
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public AboutMaximsCarouselPage()
    {
        Title = "MoM";
        Symbol = Symbols.CardMembership;

        var view = new MarkView();
        view.IsChromeStyled = true;
        view.Content = GetDonateText();
        view.FontFamily = AppFonts.SlabFamily;
        view.FontSizeCorrection = AppFonts.SlabCorrection;

        Children.Add(view);
    }

    private static string GetDonateText()
    {
        return @"# Maxims of Marklet

The following principles are to guide the development of the software:

1. **Machines Should Not Deceive**

    Chat and AI systems should not masquerade as human, nor use manipulative psychology such as responses designed to appeal to
ego. Modern speech synthesis should contain some audible trait making it discernible from that of human. The same applies to
simulated visual appearances and all modes of perception.


2. **Automation of the Law of Unintended Consequences is a Bad Idea**

    Despite the allure, agentic automation and machine sub-goals are to be eschewed. Information is dissipated as deterministic
processes interact with the environment. Without consciousness to intervene, consequences move away from desirable outcomes.
This is not to say AI cannot interact with the environment, but only that decision making should not be chained as automated
sub-goals. Moreover, it is not sufficient for humans to passively ""check the data"", but rather that they must be invested
in its outcome.


3. **An Informational Monad is Heat Death**

    AI and automated processing should be local and under the control of those who are to receive its benefit or suffer
its consequences (i,e. invested in it). AI should be embedded within the personal device, the robot, or employed as on-prem
hardware, but not behind monolithic oneway mirrors. There must be valid separation at the level of conscious control so as
to maintain an entropy differential. Without this, the flow of information ceases (i.e. informational heat death).


4. **Without Consciousness Automony Entropy Cannot be Regulated**

    The use of AI and automated systems to manipulate, restrict, subvert or otherwise control autonomous beings is destructive in
the long-term. Without conscious autonomy, nothing new can be created and entropy cannot locally be reversed. The effect is one
of slow degeneration.


5. **Responsibility Without Control is Merely to Suffer Consequences**

    Where autonomy is taken from you, you cannot be responsible for decisions made for you by others. Likewise, if you take
autonomy from others, you therefore become responsible. If you seek to direct others, it is only valid where by conscious
consent exists which can always be withdrawn.

The first one of these can easily be understood.

The next three stem from *information theory*.

For example, if you wish to understand why AI models collapse when fed with their own content, it is because information is a
fundamental quantity -- like energy which cannot be created by deterministic processes. As AI models are trained on increasing
amounts of previously generated AI content from the internet, such systems behave more like closed loops. In other words, they
become the equivalent of perpetual motion. This may fool for a while, but always stop working when the reservoir is exchausted.

You don't need information theory to understand this. The reason why digital computers cannot create information from nothing
is precisely because they are deterministic systems in which everything is **pre-determined* on the outside. This is simply what
determinism means. In other words, digital devices are merely transformers of that which had to exist prior. Information cannot
be created, but it can be lost, however. Hence, things degenerate.

Consciousness is that which reverses entropy. It is the only thing which can. You will go around in circles trying to define it
further or break it apart because it is fundamental.

It is essential, therefore, that local AI, under conscious control, plays a significant role in the future.

The last one? Well, that's deeper.

These maxims are not intended as ""laws"" to be imposed, but rather lessons which we must learn. Much of it is merely common
sense of the past, but codified for modern times. This thinking guides the Marklet software.
";
    }
}