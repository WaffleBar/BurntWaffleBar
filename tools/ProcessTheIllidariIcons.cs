using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class ProcessTheIllidariIcons
{
    const int OutputSize = 256;
    const int WorkSize = 512;
    const float IconScale = 0.93f;
    const int ContentPad = 3;
    const int AlphaCutoff = 20;

    // Matches Icons.lua TheIllidari clockStyle.color { 0.80, 0.58, 0.96 }
    const float ClockR = 0.80f;
    const float ClockG = 0.58f;
    const float ClockB = 0.96f;
    const float ClockLuma = ClockR * 0.299f + ClockG * 0.587f + ClockB * 0.114f;

    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

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

    static Bitmap AddFelContactShadow(Bitmap source)
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

                        byte sa = ClampByte((int)Math.Round(c.A * 0.14f));
                        shadow.SetPixel(x, y, Color.FromArgb(sa, 10, 16, 8));
                    }
                }

                g.DrawImage(shadow, 1.2f, 2.0f);
            }

            g.DrawImage(source, 0f, 0f);
        }
        return output;
    }

    static void HarmonizeClockPurple(ref float sr, ref float sg, ref float sb, float strength)
    {
        float fel = Math.Max(0f, sg - Math.Max(sr, sb) * 0.88f);
        if (fel > 0.10f) return;

        float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
        if (luma < 0.025f) return;

        float purpleLead = Math.Max(0f, sb - sg * 0.90f) + Math.Max(0f, sr - sg * 0.50f) * 0.30f;
        float purpleWeight = Clamp01(purpleLead * 2.2f);

        if (sb >= sr * 0.68f && sg < sb * 0.85f && luma < 0.80f)
            purpleWeight = Math.Max(purpleWeight, Clamp01(0.50f + (0.58f - luma) * 0.75f));

        if (purpleWeight <= 0.03f) return;

        float scale = luma / ClockLuma;
        scale = Math.Max(0.20f, Math.Min(1.12f, scale));
        float tr = Clamp01(ClockR * scale);
        float tg = Clamp01(ClockG * scale);
        float tb = Clamp01(ClockB * scale);

        float blend = purpleWeight * strength;
        sr = Lerp(sr, tr, blend);
        sg = Lerp(sg, tg, blend);
        sb = Lerp(sb, tb, blend);
    }

    static Bitmap HarmonizeClockPurpleBitmap(Bitmap source)
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
                float sr = c.R / 255f;
                float sg = c.G / 255f;
                float sb = c.B / 255f;
                HarmonizeClockPurple(ref sr, ref sg, ref sb, 0.92f);
                output.SetPixel(x, y, Color.FromArgb(
                    c.A,
                    ClampByte((int)Math.Round(sr * 255f)),
                    ClampByte((int)Math.Round(sg * 255f)),
                    ClampByte((int)Math.Round(sb * 255f))));
            }
        }
        return output;
    }

    static Bitmap EnhanceSmallSizeReadability(Bitmap source)
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

                float sr = c.R / 255f;
                float sg = c.G / 255f;
                float sb = c.B / 255f;
                float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
                float max = Math.Max(sr, Math.Max(sg, sb));
                float min = Math.Min(sr, Math.Min(sg, sb));
                float sat = max <= 0.001f ? 0f : (max - min) / max;

                // Lift crushed shadows into readable clock-purple leather.
                float shadowLift = 1f - Math.Min(1f, luma / 0.40f);
                shadowLift = shadowLift * shadowLift;
                const float shadowScale = 0.40f;
                sr = Lerp(sr, ClockR * shadowScale, shadowLift * 0.78f);
                sg = Lerp(sg, ClockG * shadowScale, shadowLift * 0.62f);
                sb = Lerp(sb, ClockB * shadowScale, shadowLift * 0.82f);

                // Gamma brighten midtones so 49px UI size keeps internal filigree.
                const float gamma = 0.76f;
                sr = (float)Math.Pow(Math.Max(0f, sr), gamma);
                sg = (float)Math.Pow(Math.Max(0f, sg), gamma);
                sb = (float)Math.Pow(Math.Max(0f, sb), gamma);

                // Fel accent pop — preserve green glow readability.
                float fel = Math.Max(0f, sg - Math.Max(sr, sb) * 0.82f);
                if (fel > 0.015f)
                {
                    float boost = Math.Min(1f, fel * 3.2f);
                    sg = Math.Min(1f, sg + boost * 0.20f);
                    sr = Math.Max(0f, sr - boost * 0.03f);
                }

                // Purple filigree → clock purple hue.
                HarmonizeClockPurple(ref sr, ref sg, ref sb, 0.55f);

                // Local midtone contrast — carved detail reads at micro size.
                float mid = luma * 0.55f + 0.22f;
                sr = Clamp01(sr * (0.88f + mid * 0.24f));
                sg = Clamp01(sg * (0.88f + mid * 0.24f));
                sb = Clamp01(sb * (0.88f + mid * 0.24f));

                output.SetPixel(x, y, Color.FromArgb(
                    c.A,
                    ClampByte((int)Math.Round(sr * 255f)),
                    ClampByte((int)Math.Round(sg * 255f)),
                    ClampByte((int)Math.Round(sb * 255f))));
            }
        }

        return output;
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

    static Bitmap ApplyUiPolish(Bitmap source)
    {
        Rectangle bounds = FindContentBounds(source, AlphaCutoff);
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

                float sr = c.R / 255f;
                float sg = c.G / 255f;
                float sb = c.B / 255f;
                float nx = bounds.Width > 0 ? (x - bounds.X) / (float)bounds.Width : 0.5f;
                float ny = bounds.Height > 0 ? 1f - (y - bounds.Y) / (float)bounds.Height : 0.5f;
                float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;

                // Studio key light from upper-left — Paladin-style material gleam.
                float gleamX = (nx - 0.12f) / 0.55f;
                float gleamY = (ny - 0.58f) / 0.42f;
                float gleam = Math.Max(0f, 1f - gleamX * gleamX - gleamY * gleamY);
                gleam = gleam * gleam * (0.14f + (1f - luma) * 0.10f);
                sr = Clamp01(sr + gleam * 0.24f);
                sg = Clamp01(sg + gleam * 0.20f);
                sb = Clamp01(sb + gleam * 0.14f);

                // Material saturation — leather and metal feel richer, not flat.
                float satBoost = 1.08f;
                sr = Clamp01(luma + (sr - luma) * satBoost);
                sg = Clamp01(luma + (sg - luma) * satBoost);
                sb = Clamp01(luma + (sb - luma) * satBoost);

                // Soft clock-purple rim on upper edges for UI cohesion.
                float rim = Math.Max(0f, ny - 0.55f) * Math.Max(0f, 1f - Math.Abs(nx - 0.5f) * 2.2f);
                sr = Clamp01(sr + rim * ClockR * 0.06f);
                sg = Clamp01(sg + rim * ClockG * 0.06f);
                sb = Clamp01(sb + rim * ClockB * 0.06f);

                HarmonizeClockPurple(ref sr, ref sg, ref sb, 0.72f);

                output.SetPixel(x, y, Color.FromArgb(
                    c.A,
                    ClampByte((int)Math.Round(sr * 255f)),
                    ClampByte((int)Math.Round(sg * 255f)),
                    ClampByte((int)Math.Round(sb * 255f))));
            }
        }

        return UnsharpMask(output, 0.42f);
    }

    static float Clamp01(float value)
    {
        return Math.Max(0f, Math.Min(1f, value));
    }

    static Bitmap Process(Bitmap source)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            using (var withShadow = AddFelContactShadow(cropped))
            using (var enhanced = EnhanceSmallSizeReadability(withShadow))
            using (var polished = ApplyUiPolish(enhanced))
            using (var harmonized = HarmonizeClockPurpleBitmap(polished))
            {
                return DownscaleBitmap(harmonized, OutputSize);
            }
        }
    }

    public static void ProcessAll(string inputDir, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        foreach (string name in Names)
        {
            string input = Path.Combine(inputDir, "TheIllidari_" + name + ".png");
            if (!File.Exists(input)) { Console.WriteLine("Missing: " + input); continue; }
            using (var src = new Bitmap(input))
            using (var processed = Process(src))
                processed.Save(Path.Combine(outputDir, name + ".png"), ImageFormat.Png);
            Console.WriteLine("Processed " + name);
        }
    }
}
