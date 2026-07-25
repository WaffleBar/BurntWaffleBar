using System;
using System.IO;

public static class GenerateProfessions
{
    public static void ReprocessClassThemes(string root)
    {
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

    public static void Run(string root)
    {
        ReprocessClassThemes(root);
        Console.WriteLine("Professions icon generation complete.");
    }

    public static void Main(string[] args)
    {
        string root = Directory.GetCurrentDirectory();

        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("-"))
            {
                root = args[i];
            }
        }

        Run(root);
    }
}
