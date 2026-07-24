using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class ProcessSpookyIcons
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

    static byte KeyAlpha(int r, int g, int b)
    {
        int max = Math.Max(r, Math.Max(g, b));
        int min = Math.Min(r, Math.Min(g, b));
        int chroma = max - min;

        if (r <= 12 && g <= 12 && b <= 12)
        {
            return 0;
        }

        if (max <= 32 && max - min <= 10)
        {
            return 0;
        }

        if (max <= 42 && max - min <= 12)
        {
            return (byte)Math.Max(0, Math.Min(255, (max - 14) * 9));
        }

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
                if (alpha == 0)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
                else if (alpha < 255)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(alpha, c.R, c.G, c.B));
                }
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
        {
            g.DrawImage(source, 0, 0, bounds, GraphicsUnit.Pixel);
        }

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

    static Bitmap AddSpookyContactShadow(Bitmap source)
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

                        byte sa = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(c.A * 0.20f)));
                        shadow.SetPixel(x, y, Color.FromArgb(sa, 10, 4, 18));
                    }
                }

                g.DrawImage(shadow, 1.0f, 1.8f);
            }

            g.DrawImage(source, 0f, 0f);
        }

        return output;
    }

    static Bitmap Process(Bitmap source)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            using (var withShadow = AddSpookyContactShadow(cropped))
            {
                return DownscaleBitmap(withShadow, OutputSize);
            }
        }
    }

    public static void ProcessAll(string inputDir, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        foreach (string name in Names)
        {
            string input = Path.Combine(inputDir, "SpookyWaffle_" + name + ".png");
            if (!File.Exists(input))
            {
                Console.WriteLine("Missing: " + input);
                continue;
            }

            using (var src = new Bitmap(input))
            using (var processed = Process(src))
            {
                processed.Save(Path.Combine(outputDir, name + ".png"), ImageFormat.Png);
            }

            Console.WriteLine("Processed " + name);
        }
    }
}
