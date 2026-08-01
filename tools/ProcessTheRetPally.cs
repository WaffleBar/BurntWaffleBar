using System;
using System.IO;

class ProcessTheRetPally
{
    // Silver/ash plate silhouettes that can muddy against void at bar size.
    static readonly string[] DarkIcons =
    {
        "Character",
        "Housing",
        "Professions",
    };

    static int Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : ".";
        string themes = Path.Combine(root, "Media", "Themes");
        string src = Path.Combine(themes, "TheRetPally", "source");
        string dest = Path.Combine(themes, "TheRetPally");
        string smallDir = Path.Combine(dest, "small");

        // Same freeform pipeline as The Fire Mage: optical fill + strong small tier.
        ProcessPreserveThemeIcons.ProcessTheme("TheRetPally", src, dest, 22, 10, 6, 128, 0.68f, true, true);

        foreach (string name in DarkIcons)
        {
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.34f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.22f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.10f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.38f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.26f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.12f);
        }

        Console.WriteLine("TheRetPally done (optical fill + dark-icon lift on Character/Housing/Professions).");
        return 0;
    }
}
