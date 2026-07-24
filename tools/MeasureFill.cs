using System;
using System.Drawing;
using System.IO;

public static class MeasureFill
{
    public static void Main(string[] args)
    {
        string dir = args.Length > 0 ? args[0] : ".";
        foreach (string file in Directory.GetFiles(dir, "*.png"))
        {
            using (var b = new Bitmap(file))
            {
                int minX = b.Width, minY = b.Height, maxX = -1, maxY = -1;
                for (int y = 0; y < b.Height; y++)
                    for (int x = 0; x < b.Width; x++)
                        if (b.GetPixel(x, y).A > 20)
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                if (maxX < 0) { Console.WriteLine(Path.GetFileName(file) + ": empty"); continue; }
                float fw = (maxX - minX + 1) * 100f / b.Width;
                float fh = (maxY - minY + 1) * 100f / b.Height;
                Console.WriteLine(Path.GetFileName(file) + ": " + fw.ToString("0.0") + "% x " + fh.ToString("0.0") + "%");
            }
        }
    }
}
