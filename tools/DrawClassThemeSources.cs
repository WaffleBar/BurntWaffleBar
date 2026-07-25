using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class DrawClassThemeSources
{
    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    public static void DrawAll(string root)
    {
        foreach (ClassPalette pal in ClassPalette.All())
        {
            string sourceDir = Path.Combine(root, "Media", "Themes", pal.Id, "source");
            Directory.CreateDirectory(sourceDir);
            Console.WriteLine("Drawing " + pal.Label + "...");

            foreach (string name in Names)
            {
                using (var bmp = ClassIconRenderer.Render(pal, name))
                {
                    string path = Path.Combine(sourceDir, pal.Id + "_" + name + ".png");
                    bmp.Save(path, ImageFormat.Png);
                }
            }
        }

        Console.WriteLine("Done — drew sources for " + ClassPalette.All().Length + " classes.");
    }

    public static void DrawProfessions(string root)
    {
        throw new InvalidOperationException(
            "Professions class sources must be bespoke AI art in source/{ThemeId}_Professions.png. " +
            "Do not regenerate procedurally.");
    }
}