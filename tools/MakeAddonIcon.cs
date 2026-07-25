using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class MakeAddonIcon
{
    const int OutputSize = 256;
    const float IconScale = 0.90f;

    public static void Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string sourcePath = Path.Combine(root, "Media", "Themes", "ThePaladin", "GameMenu.png");
        string outputPath = Path.Combine(root, "Media", "AddonIcon.png");

        if (!File.Exists(sourcePath))
        {
            Console.Error.WriteLine("Missing source: " + sourcePath);
            Environment.Exit(1);
        }

        using (var source = new Bitmap(sourcePath))
        using (var target = new Bitmap(OutputSize, OutputSize, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(target))
        {
            g.Clear(Color.Black);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            float maxDraw = OutputSize * IconScale;
            float scale = Math.Min(maxDraw / source.Width, maxDraw / source.Height);
            float w = source.Width * scale;
            float h = source.Height * scale;
            g.DrawImage(source, (OutputSize - w) / 2f, (OutputSize - h) / 2f, w, h);
            target.Save(outputPath, ImageFormat.Png);
        }

        Console.WriteLine("Wrote " + outputPath);
    }
}
