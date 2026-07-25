using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

/// <summary>
/// Builds sharpened 96px small/ tiers from existing full-size theme icons.
/// Used for themes not processed by ProcessPreserveThemeIcons.
/// </summary>
public static class GenerateSmallIconTiers
{
    const int SmallOutputSize = 96;

    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static byte ClampByte(int value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }

    static Bitmap DownscaleBitmap(Bitmap source, int targetSize)
    {
        var output = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(output))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(source, 0, 0, targetSize, targetSize);
        }
        return output;
    }

    static Color SamplePixel(Bitmap source, int x, int y)
    {
        x = Math.Max(0, Math.Min(source.Width - 1, x));
        y = Math.Max(0, Math.Min(source.Height - 1, y));
        return source.GetPixel(x, y);
    }

    static Bitmap SharpenForSmallDisplay(Bitmap source, float amount)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color center = SamplePixel(source, x, y);
                if (center.A == 0)
                {
                    output.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    continue;
                }

                int blurA = center.A;
                int blurR = center.R;
                int blurG = center.G;
                int blurB = center.B;
                int samples = 1;
                for (int i = 0; i < dx.Length; i++)
                {
                    Color neighbor = SamplePixel(source, x + dx[i], y + dy[i]);
                    if (neighbor.A == 0)
                        continue;

                    blurA += neighbor.A;
                    blurR += neighbor.R;
                    blurG += neighbor.G;
                    blurB += neighbor.B;
                    samples++;
                }

                Color blur = Color.FromArgb(
                    blurA / samples,
                    blurR / samples,
                    blurG / samples,
                    blurB / samples);

                byte alpha = ClampByte((int)Math.Round(center.A + amount * (center.A - blur.A)));
                byte red = ClampByte((int)Math.Round(center.R + amount * (center.R - blur.R)));
                byte green = ClampByte((int)Math.Round(center.G + amount * (center.G - blur.G)));
                byte blue = ClampByte((int)Math.Round(center.B + amount * (center.B - blur.B)));
                output.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));
            }
        }

        return output;
    }

    public static void GenerateForTheme(string themeDir)
    {
        if (!Directory.Exists(themeDir))
        {
            Console.WriteLine("Missing theme dir: " + themeDir);
            return;
        }

        string smallDir = Path.Combine(themeDir, "small");
        Directory.CreateDirectory(smallDir);
        int count = 0;

        foreach (string name in Names)
        {
            string input = Path.Combine(themeDir, name + ".png");
            if (!File.Exists(input))
                continue;

            using (var src = new Bitmap(input))
            using (var baseSmall = DownscaleBitmap(src, SmallOutputSize))
            using (var small = SharpenForSmallDisplay(baseSmall, 0.42f))
            {
                small.Save(Path.Combine(smallDir, name + ".png"), ImageFormat.Png);
                count++;
            }
        }

        Console.WriteLine("Generated " + count + " small icons in " + smallDir);
    }

    public static void GenerateAll(string themesRoot, params string[] themeIds)
    {
        foreach (string themeId in themeIds)
            GenerateForTheme(Path.Combine(themesRoot, themeId));
    }
}
