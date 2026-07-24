using System;
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

    public static void Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string themeDir = Path.Combine(root, "Media", "Themes", "BurntWaffle");
        string previewsDir = Path.Combine(root, ".previews");
        Directory.CreateDirectory(previewsDir);

        string addonIcon = Path.Combine(root, "Media", "AddonIcon.png");
        if (File.Exists(addonIcon))
        {
            File.Copy(addonIcon, Path.Combine(previewsDir, "addon-icon.png"), true);
        }

        const int iconSize = 64;
        const int padding = 8;
        int barWidth = IconOrder.Length * iconSize + (IconOrder.Length + 1) * padding;
        int barHeight = iconSize + padding * 2;

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
                {
                    Console.WriteLine("Skip missing: " + path);
                    continue;
                }

                using (var icon = new Bitmap(path))
                {
                    int x = padding + i * (iconSize + padding);
                    int y = padding;
                    g.DrawImage(icon, x, y, iconSize, iconSize);
                }
            }

            bar.Save(Path.Combine(previewsDir, "bar-burnt-waffle.png"), ImageFormat.Png);
        }

        Console.WriteLine("Wrote previews to " + previewsDir);
    }
}
