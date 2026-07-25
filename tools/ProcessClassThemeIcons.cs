using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public sealed class ClassThemeProcessConfig
{
    public string Prefix;
    public float ClockR, ClockG, ClockB;
    public float AccentR, AccentG, AccentB;
    public float ShadowR, ShadowG, ShadowB;
}

public static class ProcessClassThemeIcons
{
    const int OutputSize = 256;
    const int WorkSize = 512;
    const float IconScale = 0.93f;
    const int ContentPad = 3;
    const int AlphaCutoff = 20;

    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static ClassThemeProcessConfig _cfg;

    static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    static byte ClampByte(int value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }

    static float Clamp01(float value)
    {
        return Math.Max(0f, Math.Min(1f, value));
    }

    static float AccentStrength(float sr, float sg, float sb)
    {
        float ar = _cfg.AccentR, ag = _cfg.AccentG, ab = _cfg.AccentB;
        float len = (float)Math.Sqrt(ar * ar + ag * ag + ab * ab);
        if (len < 0.001f) return 0f;
        ar /= len; ag /= len; ab /= len;
        float dot = sr * ar + sg * ag + sb * ab;
        float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
        return Math.Max(0f, dot - luma * 0.72f);
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

    static Bitmap AddContactShadow(Bitmap source)
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
                int sr = ClampByte((int)Math.Round(_cfg.ShadowR * 255f));
                int sg = ClampByte((int)Math.Round(_cfg.ShadowG * 255f));
                int sb = ClampByte((int)Math.Round(_cfg.ShadowB * 255f));
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
                        byte sa = ClampByte((int)Math.Round(c.A * 0.20f));
                        shadow.SetPixel(x, y, Color.FromArgb(sa, sr, sg, sb));
                    }
                }
                g.DrawImage(shadow, 1.2f, 2.0f);
            }
            g.DrawImage(source, 0f, 0f);
        }
        return output;
    }

    static void HarmonizeClockRimOnly(ref float sr, ref float sg, ref float sb, float rimWeight, float strength)
    {
        if (rimWeight <= 0.02f || AccentStrength(sr, sg, sb) > 0.12f) return;

        float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
        if (luma < 0.08f || luma > 0.92f) return;

        float blend = rimWeight * strength;
        sr = Lerp(sr, Clamp01(_cfg.ClockR * (0.55f + luma * 0.55f)), blend);
        sg = Lerp(sg, Clamp01(_cfg.ClockG * (0.55f + luma * 0.55f)), blend);
        sb = Lerp(sb, Clamp01(_cfg.ClockB * (0.55f + luma * 0.55f)), blend);
    }

    static float Saturation(float sr, float sg, float sb)
    {
        float max = Math.Max(sr, Math.Max(sg, sb));
        float min = Math.Min(sr, Math.Min(sg, sb));
        return max <= 0.001f ? 0f : (max - min) / max;
    }

    static void BoostSaturation(ref float sr, ref float sg, ref float sb, float amount)
    {
        float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
        sr = Clamp01(luma + (sr - luma) * amount);
        sg = Clamp01(luma + (sg - luma) * amount);
        sb = Clamp01(luma + (sb - luma) * amount);
    }

    static Bitmap EnhanceContrastAndDepth(Bitmap source, Rectangle bounds)
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

                float sr = c.R / 255f, sg = c.G / 255f, sb = c.B / 255f;
                float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
                float nx = bounds.Width > 0 ? (x - bounds.X) / (float)bounds.Width : 0.5f;
                float ny = bounds.Height > 0 ? 1f - (y - bounds.Y) / (float)bounds.Height : 0.5f;

                float shadowLift = 1f - Math.Min(1f, luma / 0.28f);
                shadowLift = shadowLift * shadowLift * 0.55f;
                sr = Lerp(sr, sr + 0.04f, shadowLift);
                sg = Lerp(sg, sg + 0.04f, shadowLift);
                sb = Lerp(sb, sb + 0.05f, shadowLift);

                // Anchor deep shadows — wider tonal range = more depth at micro size.
                if (luma < 0.20f)
                {
                    float darken = (0.20f - luma) * 0.40f;
                    sr = Clamp01(sr - darken);
                    sg = Clamp01(sg - darken);
                    sb = Clamp01(sb - darken);
                }

                const float gamma = 0.90f;
                sr = (float)Math.Pow(Math.Max(0f, sr), gamma);
                sg = (float)Math.Pow(Math.Max(0f, sg), gamma);
                sb = (float)Math.Pow(Math.Max(0f, sb), gamma);

                float accent = AccentStrength(sr, sg, sb);
                if (accent > 0.012f)
                {
                    float boost = Math.Min(1f, accent * 4.0f);
                    sr = Clamp01(sr + boost * (_cfg.AccentR - sr) * 0.55f);
                    sg = Clamp01(sg + boost * (_cfg.AccentG - sg) * 0.55f);
                    sb = Clamp01(sb + boost * (_cfg.AccentB - sb) * 0.55f);
                }

                float sat = Saturation(sr, sg, sb);
                float satBoost = 1.18f + (1f - sat) * 0.14f;
                BoostSaturation(ref sr, ref sg, ref sb, satBoost);

                // Preserve warm/cool material separation (wood vs metal vs glow).
                if (sr > sb * 1.08f)
                {
                    sr = Clamp01(sr * 1.04f);
                    sb = Clamp01(sb * 0.97f);
                }
                else if (sb > sr * 1.08f)
                {
                    sb = Clamp01(sb * 1.04f);
                    sr = Clamp01(sr * 0.97f);
                }

                float gleamX = (nx - 0.12f) / 0.55f;
                float gleamY = (ny - 0.58f) / 0.42f;
                float gleam = Math.Max(0f, 1f - gleamX * gleamX - gleamY * gleamY);
                gleam = gleam * gleam * (0.12f + (1f - luma) * 0.14f);
                sr = Clamp01(sr + gleam * 0.28f);
                sg = Clamp01(sg + gleam * 0.24f);
                sb = Clamp01(sb + gleam * 0.18f);

                float mid = luma * 0.50f + 0.25f;
                sr = Clamp01(sr * (0.84f + mid * 0.32f));
                sg = Clamp01(sg * (0.84f + mid * 0.32f));
                sb = Clamp01(sb * (0.84f + mid * 0.32f));

                float rim = Math.Max(0f, ny - 0.62f) * Math.Max(0f, 1f - Math.Abs(nx - 0.5f) * 2.0f);
                HarmonizeClockRimOnly(ref sr, ref sg, ref sb, rim, 0.42f);

                output.SetPixel(x, y, Color.FromArgb(c.A,
                    ClampByte((int)Math.Round(sr * 255f)),
                    ClampByte((int)Math.Round(sg * 255f)),
                    ClampByte((int)Math.Round(sb * 255f))));
            }
        }

        return UnsharpMask(output, 0.48f);
    }

    static Bitmap BoxBlur(Bitmap source, int radius)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        int r = Math.Max(1, radius);
        int diam = r * 2 + 1;
        float inv = 1f / (diam * diam);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float ar = 0f, ag = 0f, ab = 0f, aa = 0f;
                for (int ky = -r; ky <= r; ky++)
                {
                    int sy = Math.Max(0, Math.Min(h - 1, y + ky));
                    for (int kx = -r; kx <= r; kx++)
                    {
                        int sx = Math.Max(0, Math.Min(w - 1, x + kx));
                        Color c = source.GetPixel(sx, sy);
                        ar += c.R; ag += c.G; ab += c.B; aa += c.A;
                    }
                }
                output.SetPixel(x, y, Color.FromArgb(
                    ClampByte((int)Math.Round(aa * inv)),
                    ClampByte((int)Math.Round(ar * inv)),
                    ClampByte((int)Math.Round(ag * inv)),
                    ClampByte((int)Math.Round(ab * inv))));
            }
        }
        return output;
    }

    static Bitmap UnsharpMask(Bitmap source, float amount)
    {
        using (var blurred = BoxBlur(source, 1))
        {
            int w = source.Width;
            int h = source.Height;
            var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color o = source.GetPixel(x, y);
                    Color b = blurred.GetPixel(x, y);
                    if (o.A <= AlphaCutoff)
                    {
                        output.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        continue;
                    }
                    int r = ClampByte((int)Math.Round(o.R + (o.R - b.R) * amount));
                    int g = ClampByte((int)Math.Round(o.G + (o.G - b.G) * amount));
                    int bl = ClampByte((int)Math.Round(o.B + (o.B - b.B) * amount));
                    output.SetPixel(x, y, Color.FromArgb(o.A, r, g, bl));
                }
            }
            return output;
        }
    }

    static Bitmap Process(Bitmap source)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            using (var withShadow = AddContactShadow(cropped))
            {
                Rectangle bounds = FindContentBounds(withShadow, AlphaCutoff);
                using (var enhanced = EnhanceContrastAndDepth(withShadow, bounds))
                    return DownscaleBitmap(enhanced, OutputSize);
            }
        }
    }

    public static void ProcessTheme(ClassThemeProcessConfig cfg, string inputDir, string outputDir)
    {
        _cfg = cfg;
        Directory.CreateDirectory(outputDir);
        foreach (string name in Names)
        {
            string input = Path.Combine(inputDir, cfg.Prefix + "_" + name + ".png");
            if (!File.Exists(input)) { Console.WriteLine("Missing: " + input); continue; }
            using (var src = new Bitmap(input))
            using (var processed = Process(src))
                processed.Save(Path.Combine(outputDir, name + ".png"), ImageFormat.Png);
            Console.WriteLine("Processed " + cfg.Prefix + " / " + name);
        }
    }

    public static ClassThemeProcessConfig ConfigFor(string themeId)
    {
        switch (themeId)
        {
            case "TheWarrior":
                return new ClassThemeProcessConfig { Prefix = "TheWarrior", ClockR = 0.78f, ClockG = 0.61f, ClockB = 0.43f, AccentR = 1f, AccentG = 0.55f, AccentB = 0.25f, ShadowR = 0.12f, ShadowG = 0.08f, ShadowB = 0.05f };
            case "TheHunter":
                return new ClassThemeProcessConfig { Prefix = "TheHunter", ClockR = 0.67f, ClockG = 0.83f, ClockB = 0.45f, AccentR = 0.55f, AccentG = 1f, AccentB = 0.35f, ShadowR = 0.10f, ShadowG = 0.14f, ShadowB = 0.06f };
            case "TheRogue":
                return new ClassThemeProcessConfig { Prefix = "TheRogue", ClockR = 1f, ClockG = 0.96f, ClockB = 0.41f, AccentR = 1f, AccentG = 0.92f, AccentB = 0.30f, ShadowR = 0.12f, ShadowG = 0.11f, ShadowB = 0.06f };
            case "ThePriest":
                return new ClassThemeProcessConfig { Prefix = "ThePriest", ClockR = 1f, ClockG = 1f, ClockB = 1f, AccentR = 1f, AccentG = 0.95f, AccentB = 0.70f, ShadowR = 0.14f, ShadowG = 0.13f, ShadowB = 0.18f };
            case "TheShaman":
                return new ClassThemeProcessConfig { Prefix = "TheShaman", ClockR = 0f, ClockG = 0.44f, ClockB = 0.87f, AccentR = 0.35f, AccentG = 0.85f, AccentB = 1f, ShadowR = 0.05f, ShadowG = 0.10f, ShadowB = 0.18f };
            case "TheMage":
                return new ClassThemeProcessConfig { Prefix = "TheMage", ClockR = 0.25f, ClockG = 0.78f, ClockB = 0.92f, AccentR = 0.45f, AccentG = 0.90f, AccentB = 1f, ShadowR = 0.08f, ShadowG = 0.14f, ShadowB = 0.22f };
            case "TheWarlock":
                return new ClassThemeProcessConfig { Prefix = "TheWarlock", ClockR = 0.53f, ClockG = 0.53f, ClockB = 0.93f, AccentR = 0.75f, AccentG = 0.35f, AccentB = 1f, ShadowR = 0.14f, ShadowG = 0.06f, ShadowB = 0.18f };
            case "TheMonk":
                return new ClassThemeProcessConfig { Prefix = "TheMonk", ClockR = 0f, ClockG = 1f, ClockB = 0.59f, AccentR = 0.35f, AccentG = 1f, AccentB = 0.70f, ShadowR = 0.06f, ShadowG = 0.14f, ShadowB = 0.10f };
            case "TheDruid":
                return new ClassThemeProcessConfig { Prefix = "TheDruid", ClockR = 1f, ClockG = 0.49f, ClockB = 0.04f, AccentR = 1f, AccentG = 0.72f, AccentB = 0.25f, ShadowR = 0.16f, ShadowG = 0.10f, ShadowB = 0.05f };
            case "TheDeathKnight":
                return new ClassThemeProcessConfig { Prefix = "TheDeathKnight", ClockR = 0.77f, ClockG = 0.12f, ClockB = 0.23f, AccentR = 1f, AccentG = 0.35f, AccentB = 0.40f, ShadowR = 0.12f, ShadowG = 0.04f, ShadowB = 0.06f };
            case "TheEvoker":
                return new ClassThemeProcessConfig { Prefix = "TheEvoker", ClockR = 0.20f, ClockG = 0.58f, ClockB = 0.50f, AccentR = 0.40f, AccentG = 1f, AccentB = 0.85f, ShadowR = 0.06f, ShadowG = 0.12f, ShadowB = 0.11f };
            default:
                throw new ArgumentException("Unknown theme: " + themeId);
        }
    }

    public static void ProcessAllThemes(string themesRoot)
    {
        string[] themes =
        {
            "TheWarrior", "TheHunter", "TheRogue", "ThePriest", "TheShaman", "TheMage",
            "TheWarlock", "TheMonk", "TheDruid", "TheDeathKnight", "TheEvoker",
        };
        foreach (string id in themes)
        {
            string dir = Path.Combine(themesRoot, id);
            string src = Path.Combine(dir, "source");
            ProcessTheme(ConfigFor(id), src, dir);
        }
    }
}
