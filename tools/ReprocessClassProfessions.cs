using System;
using System.IO;

public static class ReprocessClassProfessions
{
    public static void Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        root = Path.GetFullPath(root);
        string themesRoot = Path.Combine(root, "Media", "Themes");

        Console.WriteLine("Reprocessing class Professions icons from AI sources...");

        ProcessPreserveThemeIcons.ProcessAllClassThemes(themesRoot);

        ProcessThePaladinIcons.ProcessAll(
            Path.Combine(themesRoot, "ThePaladin", "source"),
            Path.Combine(themesRoot, "ThePaladin"));

        ProcessTheIllidariIcons.ProcessAll(
            Path.Combine(themesRoot, "TheIllidari", "source"),
            Path.Combine(themesRoot, "TheIllidari"));

        foreach (string themeId in new[]
        {
            "TheWarrior", "TheHunter", "TheRogue", "ThePriest", "TheShaman", "TheMage",
            "TheWarlock", "TheMonk", "TheDruid", "TheDeathKnight", "TheEvoker",
            "ThePaladin", "TheIllidari",
        })
        {
            GenerateSmallIconTiers.GenerateForTheme(Path.Combine(themesRoot, themeId));
        }

        Console.WriteLine("Class Professions reprocessing complete.");
    }
}
