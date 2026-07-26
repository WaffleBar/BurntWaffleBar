using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

/// <summary>
/// Minimal color-preserving pipeline (same as ProcessDeathKnightIcons / ProcessThePaladinIcons).
/// Used for all class themes — no clock harmonization wash.
/// </summary>
public static class ProcessPreserveThemeIcons
{
    const int OutputSize = 256;
    const int SmallOutputSize = 96;
    const int WorkSize = 512;
    const float IconScale = 0.93f;
    const int ContentPad = 3;
    const int AlphaCutoff = 20;

    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static byte ClampByte(int value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }

    static byte KeyAlpha(int r, int g, int b)
    {
        int max = Math.Max(r, Math.Max(g, b));
        int min = Math.Min(r, Math.Min(g, b));
        if (r <= 12 && g <= 12 && b <= 12) return 0;
        if (max <= 32 && max - min <= 10) return 0;
        if (max <= 42 && max - min <= 12)
            return (byte)Math.Max(0, Math.Min(255, (max - 14) * 9));
        return 255;
    }

    static void KeyBackground(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color c = bitmap.GetPixel(x, y);
                byte alpha = KeyAlpha(c.R, c.G, c.B);
                if (alpha == 0) bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                else if (alpha < 255) bitmap.SetPixel(x, y, Color.FromArgb(alpha, c.R, c.G, c.B));
            }
        }
    }

    static Rectangle FindContentBounds(Bitmap bitmap, int threshold)
    {
        int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).A <= threshold) continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0) return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        minX = Math.Max(0, minX - ContentPad);
        minY = Math.Max(0, minY - ContentPad);
        maxX = Math.Min(bitmap.Width - 1, maxX + ContentPad);
        maxY = Math.Min(bitmap.Height - 1, maxY + ContentPad);
        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    static Bitmap RenderScaled(Bitmap source, int canvasSize)
    {
        var target = new Bitmap(canvasSize, canvasSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(target))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            float maxDraw = canvasSize * IconScale;
            float scale = Math.Min(maxDraw / source.Width, maxDraw / source.Height);
            float drawW = source.Width * scale;
            float drawH = source.Height * scale;
            g.DrawImage(source, (canvasSize - drawW) / 2f, (canvasSize - drawH) / 2f, drawW, drawH);
        }
        return target;
    }

    static Bitmap TightCropRecenter(Bitmap source, int canvasSize)
    {
        Rectangle bounds = FindContentBounds(source, AlphaCutoff);
        var cropped = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cropped))
            g.DrawImage(source, 0, 0, bounds, GraphicsUnit.Pixel);

        var target = RenderScaled(cropped, canvasSize);
        cropped.Dispose();
        return target;
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

    static Bitmap AddContactShadow(Bitmap source, int shadowR, int shadowG, int shadowB)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(output))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingMode = CompositingMode.SourceOver;

            using (var shadow = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color c = source.GetPixel(x, y);
                        if (c.A <= AlphaCutoff)
                        {
                            shadow.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                            continue;
                        }
                        byte sa = ClampByte((int)Math.Round(c.A * 0.22f));
                        shadow.SetPixel(x, y, Color.FromArgb(sa, shadowR, shadowG, shadowB));
                    }
                }
                g.DrawImage(shadow, 1.2f, 2.0f);
            }
            g.DrawImage(source, 0f, 0f);
        }
        return output;
    }

    static void ProcessBoth(Bitmap source, int shadowR, int shadowG, int shadowB, out Bitmap full, out Bitmap small)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            using (var withShadow = AddContactShadow(cropped, shadowR, shadowG, shadowB))
            {
                full = DownscaleBitmap(withShadow, OutputSize);
                using (var smallBase = DownscaleBitmap(withShadow, SmallOutputSize))
                    small = SharpenForSmallDisplay(smallBase, 0.42f);
            }
        }
    }

    public static void ProcessTheme(string prefix, string inputDir, string outputDir, int shadowR, int shadowG, int shadowB)
    {
        Directory.CreateDirectory(outputDir);
        string smallDir = Path.Combine(outputDir, "small");
        Directory.CreateDirectory(smallDir);

        foreach (string name in Names)
        {
            string input = Path.Combine(inputDir, prefix + "_" + name + ".png");
            if (!File.Exists(input)) { Console.WriteLine("Missing: " + input); continue; }
            using (var src = new Bitmap(input))
            {
                Bitmap full;
                Bitmap small;
                ProcessBoth(src, shadowR, shadowG, shadowB, out full, out small);
                using (full)
                using (small)
                {
                    full.Save(Path.Combine(outputDir, name + ".png"), ImageFormat.Png);
                    small.Save(Path.Combine(smallDir, name + ".png"), ImageFormat.Png);
                }
            }
            Console.WriteLine("Processed " + prefix + " / " + name);
        }
    }

    public static void ProcessAllClassThemes(string themesRoot)
    {
        ProcessTheme("TheWarrior", Path.Combine(themesRoot, "TheWarrior", "source"), Path.Combine(themesRoot, "TheWarrior"), 18, 12, 8);
        ProcessTheme("TheHunter", Path.Combine(themesRoot, "TheHunter", "source"), Path.Combine(themesRoot, "TheHunter"), 16, 20, 10);
        ProcessTheme("TheRogue", Path.Combine(themesRoot, "TheRogue", "source"), Path.Combine(themesRoot, "TheRogue"), 14, 12, 18);
        ProcessTheme("ThePriest", Path.Combine(themesRoot, "ThePriest", "source"), Path.Combine(themesRoot, "ThePriest"), 20, 18, 28);
        ProcessTheme("TheShaman", Path.Combine(themesRoot, "TheShaman", "source"), Path.Combine(themesRoot, "TheShaman"), 8, 14, 24);
        ProcessTheme("TheMage", Path.Combine(themesRoot, "TheMage", "source"), Path.Combine(themesRoot, "TheMage"), 10, 16, 28);
        ProcessTheme("TheFireMage", Path.Combine(themesRoot, "TheFireMage", "source"), Path.Combine(themesRoot, "TheFireMage"), 22, 10, 6);
        ProcessTheme("TheWarlock", Path.Combine(themesRoot, "TheWarlock", "source"), Path.Combine(themesRoot, "TheWarlock"), 16, 8, 24);
        ProcessTheme("TheMonk", Path.Combine(themesRoot, "TheMonk", "source"), Path.Combine(themesRoot, "TheMonk"), 10, 20, 14);
        ProcessTheme("TheDruid", Path.Combine(themesRoot, "TheDruid", "source"), Path.Combine(themesRoot, "TheDruid"), 20, 14, 8);
        ProcessTheme("TheEvoker", Path.Combine(themesRoot, "TheEvoker", "source"), Path.Combine(themesRoot, "TheEvoker"), 10, 18, 16);
        ProcessTheme("TheDeathKnight", Path.Combine(themesRoot, "TheDeathKnight", "source"), Path.Combine(themesRoot, "TheDeathKnight"), 8, 10, 18);
    }
}
