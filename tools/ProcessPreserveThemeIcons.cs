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
    const float DefaultSmallSharpen = 0.42f;
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

    /// <summary>
    /// Fit freeform silhouettes to a consistent optical footprint so bar spacing
    /// looks even. Scales to cover minFill on BOTH axes and center-crops overflow
    /// (tall/skinny icons grow until width catches up instead of leaving gutters).
    /// </summary>
    static Bitmap OpticalRecenter(Bitmap source, int canvasSize, float minFill, float maxFill)
    {
        Rectangle bounds = FindContentBounds(source, AlphaCutoff);
        if (bounds.Width < 2 || bounds.Height < 2)
            return RenderScaled(source, canvasSize);

        // Cover: both axes reach at least minFill (overflow is clipped by the canvas).
        float scale = Math.Max(
            (canvasSize * minFill) / bounds.Width,
            (canvasSize * minFill) / bounds.Height);

        // Only shrink if BOTH axes would exceed maxFill (keeps skinny icons from
        // collapsing back to a narrow footprint).
        float scaledW = bounds.Width * scale;
        float scaledH = bounds.Height * scale;
        if (scaledW > canvasSize * maxFill && scaledH > canvasSize * maxFill)
        {
            scale = Math.Min(
                (canvasSize * maxFill) / bounds.Width,
                (canvasSize * maxFill) / bounds.Height);
            scaledW = bounds.Width * scale;
            scaledH = bounds.Height * scale;
        }

        var target = new Bitmap(canvasSize, canvasSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(target))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(
                source,
                new RectangleF((canvasSize - scaledW) / 2f, (canvasSize - scaledH) / 2f, scaledW, scaledH),
                bounds,
                GraphicsUnit.Pixel);
        }
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

    /// <summary>
    /// Slightly dilate opaque coverage so thin freeform strokes (blades, shafts, filigree)
    /// survive aggressive downscale into the small tier.
    /// </summary>
    static Bitmap ThickenAlphaForSmall(Bitmap source)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color center = SamplePixel(source, x, y);
                int bestA = center.A;
                int bestR = center.R;
                int bestG = center.G;
                int bestB = center.B;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        Color n = SamplePixel(source, x + dx, y + dy);
                        if (n.A > bestA)
                        {
                            bestA = n.A;
                            bestR = n.R;
                            bestG = n.G;
                            bestB = n.B;
                        }
                    }
                }

                if (center.A >= AlphaCutoff)
                    output.SetPixel(x, y, center);
                else if (bestA >= AlphaCutoff)
                    output.SetPixel(x, y, Color.FromArgb(ClampByte((int)Math.Round(bestA * 0.85)), bestR, bestG, bestB));
                else
                    output.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
            }
        }
        return output;
    }

    static void ProcessBoth(
        Bitmap source,
        int shadowR,
        int shadowG,
        int shadowB,
        out Bitmap full,
        out Bitmap small,
        int smallOutputSize = SmallOutputSize,
        float smallSharpen = DefaultSmallSharpen,
        bool thickenSmall = false,
        bool opticalNormalize = false)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = opticalNormalize
                ? OpticalRecenter(working, WorkSize, 0.84f, 0.96f)
                : TightCropRecenter(working, WorkSize))
            using (var withShadow = AddContactShadow(cropped, shadowR, shadowG, shadowB))
            {
                full = DownscaleBitmap(withShadow, OutputSize);

                Bitmap smallSource = withShadow;
                Bitmap thickened = null;
                if (thickenSmall)
                {
                    thickened = ThickenAlphaForSmall(withShadow);
                    smallSource = thickened;
                }

                using (var smallBase = DownscaleBitmap(smallSource, smallOutputSize))
                    small = SharpenForSmallDisplay(smallBase, smallSharpen);

                if (thickened != null)
                    thickened.Dispose();
            }
        }
    }

    public static void ProcessTheme(
        string prefix,
        string inputDir,
        string outputDir,
        int shadowR,
        int shadowG,
        int shadowB,
        int smallOutputSize = SmallOutputSize,
        float smallSharpen = DefaultSmallSharpen,
        bool thickenSmall = false,
        bool opticalNormalize = false)
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
                ProcessBoth(src, shadowR, shadowG, shadowB, out full, out small, smallOutputSize, smallSharpen, thickenSmall, opticalNormalize);
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

    /// <summary>
    /// Lift dark midtones so charcoal metal/stone detail reads at bar size, without
    /// blowing out already-bright ember/glow pixels.
    /// </summary>
    public static Bitmap LiftDarkDetails(Bitmap source, float lift = 1.32f)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = source.GetPixel(x, y);
                if (c.A <= AlphaCutoff)
                {
                    output.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    continue;
                }

                float peak = Math.Max(c.R, Math.Max(c.G, c.B)) / 255f;
                // Full lift in deep shadows; ease off as pixels approach glow range.
                float protect = peak <= 0.42f ? 0f : Math.Min(1f, (peak - 0.42f) / 0.40f);
                float factor = lift * (1f - protect) + 1f * protect;

                // Extra gentle floor so near-black charcoal separates from the void bg.
                float floorBoost = peak < 0.18f ? (0.18f - peak) * 0.55f : 0f;

                byte r = ClampByte((int)Math.Round(c.R * factor + floorBoost * 255f));
                byte g = ClampByte((int)Math.Round(c.G * factor + floorBoost * 220f));
                byte b = ClampByte((int)Math.Round(c.B * factor + floorBoost * 180f));
                output.SetPixel(x, y, Color.FromArgb(c.A, r, g, b));
            }
        }
        return output;
    }

    public static void LiftIconFile(string path, float lift = 1.32f)
    {
        if (!File.Exists(path))
            return;

        string tempPath = path + ".lift.tmp.png";
        using (var src = new Bitmap(path))
        using (var lifted = LiftDarkDetails(src, lift))
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            lifted.Save(tempPath, ImageFormat.Png);
        }
        File.Delete(path);
        File.Move(tempPath, path);
        Console.WriteLine("Lifted dark detail: " + path);
    }

    public static void ProcessAllClassThemes(string themesRoot)
    {
        ProcessTheme("TheWarrior", Path.Combine(themesRoot, "TheWarrior", "source"), Path.Combine(themesRoot, "TheWarrior"), 18, 12, 8);
        ProcessTheme("TheHunter", Path.Combine(themesRoot, "TheHunter", "source"), Path.Combine(themesRoot, "TheHunter"), 16, 20, 10);
        ProcessTheme("TheRogue", Path.Combine(themesRoot, "TheRogue", "source"), Path.Combine(themesRoot, "TheRogue"), 14, 12, 18);
        ProcessTheme("ThePriest", Path.Combine(themesRoot, "ThePriest", "source"), Path.Combine(themesRoot, "ThePriest"), 20, 18, 28);
        ProcessTheme("TheShaman", Path.Combine(themesRoot, "TheShaman", "source"), Path.Combine(themesRoot, "TheShaman"), 8, 14, 24);
        ProcessTheme("TheMage", Path.Combine(themesRoot, "TheMage", "source"), Path.Combine(themesRoot, "TheMage"), 10, 16, 28);
        // Freeform Fire Mage: stronger small tier + optical fill so skinny icons don't leave uneven gaps.
        ProcessTheme("TheFireMage", Path.Combine(themesRoot, "TheFireMage", "source"), Path.Combine(themesRoot, "TheFireMage"), 22, 10, 6, 128, 0.68f, true, true);
        // Freeform Ret Pally: same pipeline as Fire Mage (holy gold / silver plate silhouettes).
        ProcessTheme("TheRetPally", Path.Combine(themesRoot, "TheRetPally", "source"), Path.Combine(themesRoot, "TheRetPally"), 22, 10, 6, 128, 0.68f, true, true);
        // Freeform Resto Shammy: same pipeline (seafoam / totem / Healing Rain silhouettes).
        ProcessTheme("TheRestoShammy", Path.Combine(themesRoot, "TheRestoShammy", "source"), Path.Combine(themesRoot, "TheRestoShammy"), 22, 10, 6, 128, 0.68f, true, true);
        // Freeform BDK: same pipeline (crimson blood / bone / blackened steel silhouettes).
        ProcessTheme("TheBDK", Path.Combine(themesRoot, "TheBDK", "source"), Path.Combine(themesRoot, "TheBDK"), 22, 10, 6, 128, 0.68f, true, true);
        // Freeform Sub Rogue: same pipeline (violet Shadow Dance / cold steel silhouettes).
        ProcessTheme("TheSubRogue", Path.Combine(themesRoot, "TheSubRogue", "source"), Path.Combine(themesRoot, "TheSubRogue"), 22, 10, 6, 128, 0.68f, true, true);
        ProcessTheme("TheWarlock", Path.Combine(themesRoot, "TheWarlock", "source"), Path.Combine(themesRoot, "TheWarlock"), 16, 8, 24);
        ProcessTheme("TheMonk", Path.Combine(themesRoot, "TheMonk", "source"), Path.Combine(themesRoot, "TheMonk"), 10, 20, 14);
        ProcessTheme("TheDruid", Path.Combine(themesRoot, "TheDruid", "source"), Path.Combine(themesRoot, "TheDruid"), 20, 14, 8);
        ProcessTheme("TheEvoker", Path.Combine(themesRoot, "TheEvoker", "source"), Path.Combine(themesRoot, "TheEvoker"), 10, 18, 16);
        ProcessTheme("TheDeathKnight", Path.Combine(themesRoot, "TheDeathKnight", "source"), Path.Combine(themesRoot, "TheDeathKnight"), 8, 10, 18);
    }
}
