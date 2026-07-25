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
        string themeBarPath = Path.Combine(root, "Media", "Themes", "ThePaladin");

        const int size = 1280;

        using (var banner = new Bitmap(size, size, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(banner))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var bg = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(255, 8, 10, 14),
                Color.FromArgb(255, 18, 14, 10),
                90f))
            {
                g.FillRectangle(bg, 0, 0, size, size);
            }

            using (var glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(size * 0.12f, size * 0.38f, size * 0.76f, size * 0.34f);
                using (var glowBrush = new PathGradientBrush(glowPath))
                {
                    glowBrush.CenterColor = Color.FromArgb(48, 255, 145, 64);
                    glowBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 145, 64) };
                    g.FillPath(glowBrush, glowPath);
                }
            }

            using (var icon = new Bitmap(addonIconPath))
            {
                float iconSize = 220f;
                float scale = Math.Min(iconSize / icon.Width, iconSize / icon.Height);
                float drawWidth = icon.Width * scale;
                float drawHeight = icon.Height * scale;
                float x = (size - drawWidth) / 2f;
                float y = 150f;
                g.DrawImage(icon, x, y, drawWidth, drawHeight);
            }

            DrawThemeBar(g, themeBarPath, size, 430f);

            using (var titleFont = new Font("Segoe UI", 92f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var subtitleFont = new Font("Segoe UI", 34f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var detailFont = new Font("Segoe UI", 24f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var creditFont = new Font("Segoe UI", 20f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var titleBrush = new SolidBrush(Color.FromArgb(255, 248, 244, 238)))
            using (var subtitleBrush = new SolidBrush(Color.FromArgb(255, 255, 168, 96)))
            using (var detailBrush = new SolidBrush(Color.FromArgb(255, 170, 156, 140)))
            {
                DrawCentered(g, "BurntWaffleBar", titleFont, titleBrush, size, 700f);
                DrawCentered(g, "Custom micro menu bar", subtitleFont, subtitleBrush, size, 810f);
                DrawCentered(g, "Class themes  ·  Edit Mode  ·  Queue eye", detailFont, detailBrush, size, size - 72f);
                DrawCentered(g, "By Waffle", creditFont, detailBrush, size, size - 40f);
            }

            banner.Save(outputPath, ImageFormat.Png);
        }

        Console.WriteLine("Wrote " + outputPath);
    }

    static void DrawCentered(Graphics g, string text, Font font, Brush brush, int width, float y)
    {
        SizeF size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, (width - size.Width) / 2f, y);
    }

    static void DrawThemeBar(Graphics g, string themeDir, int canvasWidth, float y)
    {
        const int iconSize = 72;
        const int padding = 10;
        int iconsToDraw = Math.Min(IconOrder.Length, 10);
        int barWidth = iconsToDraw * iconSize + (iconsToDraw + 1) * padding;
        float x = (canvasWidth - barWidth) / 2f;
        var barRect = new RectangleF(x - 16f, y - 16f, barWidth + 32f, iconSize + padding * 2 + 32f);

        using (var barPath = RoundedRect(barRect, 18f))
        using (var barBrush = new SolidBrush(Color.FromArgb(210, 24, 20, 18)))
        using (var barBorder = new Pen(Color.FromArgb(255, 88, 62, 42), 2f))
        {
            g.FillPath(barBrush, barPath);
            g.DrawPath(barBorder, barPath);
        }

        g.SetClip(barRect);
        for (int i = 0; i < iconsToDraw; i++)
        {
            string path = Path.Combine(themeDir, IconOrder[i] + ".png");
            if (!File.Exists(path))
                continue;

            using (var icon = new Bitmap(path))
            {
                float drawX = x + padding + i * (iconSize + padding);
                g.DrawImage(icon, drawX, y + padding, iconSize, iconSize);
            }
        }

        g.ResetClip();
    }

    static GraphicsPath RoundedRect(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2f;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
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
