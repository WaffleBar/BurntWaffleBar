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
        "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static readonly string[] GalleryThemeOrder =
    {
        "ThePaladin", "TheIllidari", "TheWarrior", "TheHunter", "TheRogue", "ThePriest",
        "TheShaman", "TheMage", "TheWarlock", "TheMonk", "TheDruid", "TheDeathKnight", "TheEvoker",
    };

    static readonly Dictionary<string, string> ThemeLabels = new Dictionary<string, string>
    {
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

        CleanGeneratedPreviews(previewsDir);

        string addonIcon = Path.Combine(root, "Media", "AddonIcon.png");
        if (File.Exists(addonIcon))
        {
            File.Copy(addonIcon, Path.Combine(previewsDir, "01-addon-icon.png"), true);
            Console.WriteLine("Wrote 01-addon-icon.png");
            WriteSocialPreview(root, addonIcon);
        }

        for (int i = 0; i < GalleryThemeOrder.Length; i++)
        {
            string themeId = GalleryThemeOrder[i];
            string fileSlug = ThemeLabels[themeId];
            int galleryNumber = i + 2;
            RenderThemeBar(
                themesRoot,
                previewsDir,
                themeId,
                fileSlug,
                string.Format("{0:D2}-{1}.png", galleryNumber, fileSlug));
        }

        Console.WriteLine("Wrote previews to " + previewsDir);
    }

    static void CleanGeneratedPreviews(string previewsDir)
    {
        foreach (string path in Directory.GetFiles(previewsDir, "*.png"))
        {
            File.Delete(path);
        }
    }

    static void WriteSocialPreview(string root, string addonIconPath)
    {
        string githubDir = Path.Combine(root, ".github");
        Directory.CreateDirectory(githubDir);
        string outputPath = Path.Combine(githubDir, "social-preview.png");

        const int width = 1280;
        const int height = 640;

        using (var icon = new Bitmap(addonIconPath))
        using (var banner = new Bitmap(width, height, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(banner))
        {
            g.Clear(Color.Black);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            float iconSize = 320f;
            float scale = Math.Min(iconSize / icon.Width, iconSize / icon.Height);
            float drawWidth = icon.Width * scale;
            float drawHeight = icon.Height * scale;
            float x = (width - drawWidth) / 2f;
            float y = (height - drawHeight) / 2f - 20f;
            g.DrawImage(icon, x, y, drawWidth, drawHeight);

            using (var titleFont = new Font("Segoe UI", 42f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var subtitleFont = new Font("Segoe UI", 22f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var titleBrush = new SolidBrush(Color.FromArgb(255, 245, 245, 245)))
            using (var subtitleBrush = new SolidBrush(Color.FromArgb(255, 180, 180, 180)))
            {
                string title = "BurntWaffleBar";
                string subtitle = "Custom micro menu bar for World of Warcraft";
                SizeF titleSize = g.MeasureString(title, titleFont);
                SizeF subtitleSize = g.MeasureString(subtitle, subtitleFont);
                float titleY = y + drawHeight + 24f;
                g.DrawString(title, titleFont, titleBrush, (width - titleSize.Width) / 2f, titleY);
                g.DrawString(subtitle, subtitleFont, subtitleBrush, (width - subtitleSize.Width) / 2f, titleY + titleSize.Height + 8f);
            }

            banner.Save(outputPath, ImageFormat.Png);
        }

        Console.WriteLine("Wrote " + outputPath);
    }

    static void RenderThemeBar(string themesRoot, string previewsDir, string themeId, string fileSlug, string outputName)
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

            bar.Save(Path.Combine(previewsDir, outputName), ImageFormat.Png);
            Console.WriteLine("Wrote " + outputName + " (" + drawn + " icons)");
        }
    }
}
