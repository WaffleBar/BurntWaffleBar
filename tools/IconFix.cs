using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;

public static class BurntWafflesIconFix
{
    static bool IsBackground(byte r, byte g, byte b, int threshold)
    {
        if (r >= threshold && g >= threshold && b >= threshold) return true;
        if (r <= 25 && g <= 25 && b <= 25) return true;
        return false;
    }

    static void FloodFillTransparent(Bitmap bitmap, int threshold)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        bool[] visited = new bool[width * height];
        Queue<Point> queue = new Queue<Point>();

        Action<int, int> tryEnqueue = (x, y) =>
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return;
            int index = y * width + x;
            if (visited[index]) return;

            Color pixel = bitmap.GetPixel(x, y);
            if (IsBackground(pixel.R, pixel.G, pixel.B, threshold))
            {
                visited[index] = true;
                queue.Enqueue(new Point(x, y));
            }
        };

        for (int x = 0; x < width; x++)
        {
            tryEnqueue(x, 0);
            tryEnqueue(x, height - 1);
        }
        for (int y = 0; y < height; y++)
        {
            tryEnqueue(0, y);
            tryEnqueue(width - 1, y);
        }

        while (queue.Count > 0)
        {
            Point p = queue.Dequeue();
            bitmap.SetPixel(p.X, p.Y, Color.FromArgb(0, 0, 0, 0));

            tryEnqueue(p.X - 1, p.Y);
            tryEnqueue(p.X + 1, p.Y);
            tryEnqueue(p.X, p.Y - 1);
            tryEnqueue(p.X, p.Y + 1);
        }
    }

    static bool IsLightBackground(byte r, byte g, byte b, int threshold)
    {
        if (r >= threshold && g >= threshold && b >= threshold) return true;

        int maxDiff = Math.Max(Math.Abs(r - g), Math.Max(Math.Abs(g - b), Math.Abs(r - b)));
        int average = (r + g + b) / 3;
        return average >= threshold && maxDiff <= 35;
    }

    static void RemoveNearWhite(Bitmap bitmap, int threshold)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.A == 0) continue;

                if (IsLightBackground(pixel.R, pixel.G, pixel.B, threshold))
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
        }
    }

    static void SaveTga(Bitmap bitmap, string path)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        byte[] header = new byte[18];
        header[2] = 2;
        header[12] = (byte)(width & 0xFF);
        header[13] = (byte)((width >> 8) & 0xFF);
        header[14] = (byte)(height & 0xFF);
        header[15] = (byte)((height >> 8) & 0xFF);
        header[16] = 32;
        header[17] = 0x28;

        byte[] pixels = new byte[width * height * 4];
        int offset = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = bitmap.GetPixel(x, y);
                pixels[offset++] = c.B;
                pixels[offset++] = c.G;
                pixels[offset++] = c.R;
                pixels[offset++] = c.A;
            }
        }

        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            fs.Write(header, 0, header.Length);
            fs.Write(pixels, 0, pixels.Length);
        }
    }

    public static void Process(string inputPath, string pngPath, string tgaPath, int threshold)
    {
        string tempPng = pngPath + ".tmp.png";

        using (Bitmap source = new Bitmap(inputPath))
        {
            FloodFillTransparent(source, threshold);

            using (Bitmap cleaned = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(cleaned))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(source, 0, 0, source.Width, source.Height);
                }

                FloodFillTransparent(cleaned, threshold - 10);
                RemoveNearWhite(cleaned, 180);
                RemoveNearWhite(cleaned, 180);
                cleaned.Save(tempPng, ImageFormat.Png);
                SaveTga(cleaned, tgaPath);
            }
        }

        if (File.Exists(tempPng))
        {
            File.Copy(tempPng, pngPath, true);
            File.Delete(tempPng);
        }
    }
}
