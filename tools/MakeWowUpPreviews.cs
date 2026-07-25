using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class MakeWowUpPreviews
{
    static readonly string[] IconOrder =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static readonly Dictionary<string, string> ThemeLabels = new Dictionary<string, string>
    {
        { "BurntWaffle", "burnt-waffle" },
        { "Pristine", "pristine" },
        { "FrozenWaffle", "frozen-waffle" },
        { "ThePaladin", "the-paladin" },
        { "TheIllidari", "the-illidari" },
        { "TheWarrior", "the-warrior" },
        { "TheHunter", "the-hunter" },
        { "TheRogue", "the-rogue" },
        { "ThePriest", "the-priest" },
        { "TheShaman", "the-shaman" },
        { "TheMage", "the-mage" },
        { "TheWarlock", "the-warlock" },
        { "TheMonk", "the-monk" },
        { "TheDruid", "the-druid" },
        { "TheDeathKnight", "the-death-knight" },
        { "TheEvoker", "the-evoker" },
    };

    public static void Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string themesRoot = Path.Combine(root, "Media", "Themes");
        string previewsDir = Path.Combine(root, ".previews");
        Directory.CreateDirectory(previewsDir);

        string addonIcon = Path.Combine(root, "Media", "AddonIcon.png");
        if (File.Exists(addonIcon))
        {
            File.Copy(addonIcon, Path.Combine(previewsDir, "addon-icon.png"), true);
            Console.WriteLine("Copied addon-icon.png");
        }

        foreach (KeyValuePair<string, string> entry in ThemeLabels)
        {
            RenderThemeBar(themesRoot, previewsDir, entry.Key, entry.Value);
        }

        Console.WriteLine("Wrote previews to " + previewsDir);
    }

    static void RenderThemeBar(string themesRoot, string previewsDir, string themeId, string fileSlug)
    {
        string themeDir = Path.Combine(themesRoot, themeId);
        const int iconSize = 64;
        const int padding = 8;
        int barWidth = IconOrder.Length * iconSize + (IconOrder.Length + 1) * padding;
        int barHeight = iconSize + padding * 2;
        int drawn = 0;

        using (var bar = new Bitmap(barWidth, barHeight, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(bar))
        {
            g.Clear(Color.FromArgb(255, 18, 18, 18));
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            for (int i = 0; i < IconOrder.Length; i++)
            {
                string path = Path.Combine(themeDir, IconOrder[i] + ".png");
                if (!File.Exists(path))
                    continue;

                using (var icon = new Bitmap(path))
                {
                    int x = padding + i * (iconSize + padding);
                    int y = padding;
                    g.DrawImage(icon, x, y, iconSize, iconSize);
                    drawn++;
                }
            }

            if (drawn == 0)
            {
                Console.WriteLine("Skip empty theme: " + themeId);
                return;
            }

            bar.Save(Path.Combine(previewsDir, "bar-" + fileSlug + ".png"), ImageFormat.Png);
            Console.WriteLine("Wrote bar-" + fileSlug + ".png (" + drawn + " icons)");
        }
    }
}
