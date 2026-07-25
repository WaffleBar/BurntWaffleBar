using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class ProcessBurntIcons
{
    const int OutputSize = 256;
    const float IconScale = 0.90f;

    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static byte KeyAlpha(int r, int g, int b)
    {
        int max = Math.Max(r, Math.Max(g, b));
        int min = Math.Min(r, Math.Min(g, b));
        int chroma = max - min;

        if (r <= 10 && g <= 10 && b <= 10)
        {
            return 0;
        }

        if (max <= 24 && chroma <= 5)
        {
            return (byte)Math.Max(0, Math.Min(255, (max - 8) * 16));
        }

        return 255;
    }

    static Bitmap Process(Bitmap source)
    {
        var target = new Bitmap(OutputSize, OutputSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(target))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            float maxDraw = OutputSize * IconScale;
            float scale = Math.Min(maxDraw / source.Width, maxDraw / source.Height);
            float drawW = source.Width * scale;
            float drawH = source.Height * scale;
            float x = (OutputSize - drawW) / 2f;
            float y = (OutputSize - drawH) / 2f;
            g.DrawImage(source, x, y, drawW, drawH);
        }

        for (int py = 0; py < OutputSize; py++)
        {
            for (int px = 0; px < OutputSize; px++)
            {
                Color c = target.GetPixel(px, py);
                byte alpha = KeyAlpha(c.R, c.G, c.B);
                if (alpha == 0)
                {
                    target.SetPixel(px, py, Color.FromArgb(0, 0, 0, 0));
                }
                else if (alpha < 255)
                {
                    target.SetPixel(px, py, Color.FromArgb(alpha, c.R, c.G, c.B));
                }
            }
        }

        source.Dispose();
        return target;
    }

    public static void ProcessAll(string inputDir, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        foreach (string name in Names)
        {
            string input = Path.Combine(inputDir, "BurntWaffle_" + name + ".png");
            if (!File.Exists(input))
            {
                Console.WriteLine("Missing: " + input);
                continue;
            }

            using (var src = new Bitmap(input))
            using (var processed = Process(new Bitmap(src)))
            {
                processed.Save(Path.Combine(outputDir, name + ".png"), ImageFormat.Png);
            }

            Console.WriteLine("Processed " + name);
        }
    }
}
