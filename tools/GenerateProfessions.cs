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
        root = Path.GetFullPath(root);
        string themesRoot = Path.Combine(root, "Media", "Themes");

        Console.WriteLine("Generating Professions icons...");

        MinimalIconGen.GenerateProfessionsAssets(
            Path.Combine(themesRoot, "BurntWaffle"),
            Path.Combine(themesRoot, "BurntWaffle", "source"),
            "BurntWaffle");
        ProcessBurntIcons.ProcessAll(
            Path.Combine(themesRoot, "BurntWaffle", "source"),
            Path.Combine(themesRoot, "BurntWaffle"));

        MinimalWhiteGen.GenerateProfessions(Path.Combine(themesRoot, "Pristine"));
        FrozenWaffleGen.GenerateProfessions(Path.Combine(themesRoot, "FrozenWaffle"));

        ReprocessClassThemes(root);

        foreach (string themeId in new[] { "BurntWaffle", "FrozenWaffle", "Pristine" })
        {
            GenerateSmallIconTiers.GenerateForTheme(Path.Combine(themesRoot, themeId));
        }

        Console.WriteLine("Professions icon generation complete.");
    }

    public static void Main(string[] args)
    {
        string root = Directory.GetCurrentDirectory();
        bool classOnly = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--class-only")
            {
                classOnly = true;
            }
            else if (!args[i].StartsWith("-"))
            {
                root = args[i];
            }
        }

        if (classOnly)
        {
            ReprocessClassThemes(root);
            return;
        }

        Run(root);
    }
}
