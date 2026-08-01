using System;
using System.IO;

class ProcessTheFireMage
{
    static readonly string[] DarkIcons =
    {
        "Character",   // #1 hood — charcoal armor loses detail at bar size
        "Housing",     // #5 house — stone body too close to void bg
        "Professions", // #9 anvil — hammer/anvil silhouette muddies in shadow
    };

    static int Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : ".";
        string themes = Path.Combine(root, "Media", "Themes");
        string src = Path.Combine(themes, "TheFireMage", "source");
        string dest = Path.Combine(themes, "TheFireMage");
        string smallDir = Path.Combine(dest, "small");

        // Freeform silhouettes: stronger small tier + optical fill normalization for even bar spacing.
        ProcessPreserveThemeIcons.ProcessTheme("TheFireMage", src, dest, 22, 10, 6, 128, 0.68f, true, true);

        foreach (string name in DarkIcons)
        {
            // Charcoal bodies: strong lift, then a gentle finishing nudge (don't wash out).
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.34f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.22f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.10f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.38f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.26f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.12f);
        }

        Console.WriteLine("TheFireMage done (optical fill + dark-icon lift on Character/Housing/Professions).");
        return 0;
    }
}
