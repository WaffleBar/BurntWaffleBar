using System;
using System.IO;

class ProcessTheSubRogue
{
    // Dark leather / shadow silhouettes that muddy against void at bar size.
    static readonly string[] DarkIcons =
    {
        "Character",
        "Housing",
        "Professions",
        "Guild",
        "GroupFinder",
        "PVP",
        "Social",
        "AdventureGuide",
        "Talents",
        "GameMenu",
        "QuestTracker",
        "Collections",
        "AchievementTracker",
    };

    static int Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : ".";
        string themes = Path.Combine(root, "Media", "Themes");
        string src = Path.Combine(themes, "TheSubRogue", "source");
        string dest = Path.Combine(themes, "TheSubRogue");
        string smallDir = Path.Combine(dest, "small");

        // Same freeform pipeline as The Fire Mage / specialty packs.
        ProcessPreserveThemeIcons.ProcessTheme("TheSubRogue", src, dest, 22, 10, 6, 128, 0.68f, true, true);

        foreach (string name in DarkIcons)
        {
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.34f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.22f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(dest, name + ".png"), 1.10f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.38f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.26f);
            ProcessPreserveThemeIcons.LiftIconFile(Path.Combine(smallDir, name + ".png"), 1.12f);
        }

        Console.WriteLine("TheSubRogue done (optical fill + dark-icon lift on leather/shadow icons).");
        return 0;
    }
}
