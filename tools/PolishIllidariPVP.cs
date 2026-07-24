using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

/// <summary>
/// Adds 3D fel-metal polish to the flat warglaive silhouette without changing its shape.
/// </summary>
public static class PolishIllidariPVP
{
    static bool IsShapePixel(int r, int g, int b, int a)
    {
        if (a < 20) return false;
        return g > r + 20 && g > 80;
    }

    static bool IsDiscPixel(int r, int g, int b, int a)
    {
        if (a < 20) return false;
        return r > 80 && b > 120 && g < b - 10;
    }

    static byte Clamp(int v)
    {
        return (byte)Math.Max(0, Math.Min(255, v));
    }

    static Color ShadeFel(int x, int y, int cx, int cy, float nx, float ny)
    {
        float dx = (x - cx) / 280f;
        float dy = (y - cy) / 280f;
        float light = 1f - Math.Max(0f, dx * -0.55f + dy * -0.45f);
        float rim = Math.Min(1f, Math.Abs(nx) + Math.Abs(ny));

        int bright = Clamp((int)(210 + light * 45 - rim * 30));
        int mid = Clamp((int)(90 + light * 80));
        int deep = Clamp((int)(35 + light * 25));

        float t = Math.Max(0f, Math.Min(1f, 0.35f + dx * 0.25f + dy * 0.15f));
        int r = Clamp((int)(deep + (mid - deep) * t));
        int g = Clamp((int)(bright - t * 20));
        int b = Clamp((int)(deep + (mid - deep) * t * 0.8f + 40));
        return Color.FromArgb(255, r, g, b);
    }

    static Color ShadeDisc(int x, int y, int cx, int cy, float dist)
    {
        float t = Math.Max(0f, Math.Min(1f, dist / 360f));
        float light = 1f - ((x - cx) * -0.004f + (y - cy) * -0.003f);
        int r = Clamp((int)(130 - t * 70 + light * 25));
        int g = Clamp((int)(70 - t * 35 + light * 12));
        int b = Clamp((int)(165 - t * 85 + light * 30));
        return Color.FromArgb(255, r, g, b);
    }

    static bool IsEdge(bool[,] mask, int x, int y, int w, int h)
    {
        if (!mask[x, y]) return false;
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                if (ox == 0 && oy == 0) continue;
                int nx = x + ox, ny = y + oy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || !mask[nx, ny])
                    return true;
            }
        }
        return false;
    }

    public static Bitmap RenderFromFlat(string flatPath)
    {
        using (var src = new Bitmap(flatPath))
        {
            int w = src.Width, h = src.Height;
            var shape = new bool[w, h];
            var disc = new bool[w, h];
            int cx = 0, cy = 0, shapeCount = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = src.GetPixel(x, y);
                    if (IsShapePixel(c.R, c.G, c.B, c.A))
                    {
                        shape[x, y] = true;
                        cx += x;
                        cy += y;
                        shapeCount++;
                    }
                    else if (IsDiscPixel(c.R, c.G, c.B, c.A))
                    {
                        disc[x, y] = true;
                    }
                }
            }

            if (shapeCount > 0)
            {
                cx /= shapeCount;
                cy /= shapeCount;
            }
            else
            {
                cx = w / 2;
                cy = h / 2;
            }

            var outBmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            float discCx = w * 0.5f;
            float discCy = h * 0.52f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (disc[x, y])
                    {
                        float dist = (float)Math.Sqrt((x - discCx) * (x - discCx) + (y - discCy) * (y - discCy));
                        Color baseColor = ShadeDisc(x, y, (int)discCx, (int)discCy, dist);
                        if (dist > 330 && dist < 380)
                        {
                            baseColor = Color.FromArgb(255, 204, 148, 245);
                        }
                        outBmp.SetPixel(x, y, baseColor);
                    }
                    else
                    {
                        outBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }

            // Contact shadow under warglaives
            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++)
                {
                    int sx = x + 8, sy = y + 10;
                    if (sx >= w || sy >= h || !shape[x, y]) continue;
                    if (!disc[sx, sy] && !shape[sx, sy])
                        continue;
                    Color existing = outBmp.GetPixel(sx, sy);
                    if (existing.A == 0) continue;
                    outBmp.SetPixel(sx, sy, Color.FromArgb(
                        existing.A,
                        existing.R * 7 / 10,
                        existing.G * 7 / 10,
                        existing.B * 7 / 10));
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!shape[x, y]) continue;

                    float nx = (x - cx) / 260f;
                    float ny = (y - cy) / 260f;
                    Color col = ShadeFel(x, y, cx, cy, nx, ny);

                    if (IsEdge(shape, x, y, w, h))
                    {
                        col = Color.FromArgb(255,
                            Clamp(col.R + 35),
                            Clamp(col.G + 45),
                            Clamp(col.B + 30));
                    }

                    // Fel glow halo on edge pixels
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        for (int ox = -2; ox <= 2; ox++)
                        {
                            int gx = x + ox, gy = y + oy;
                            if (gx < 0 || gy < 0 || gx >= w || gy >= h) continue;
                            if (shape[gx, gy] || disc[gx, gy]) continue;
                            Color gcol = outBmp.GetPixel(gx, gy);
                            int glowA = Clamp(40 - (Math.Abs(ox) + Math.Abs(gy)) * 10);
                            outBmp.SetPixel(gx, gy, Color.FromArgb(
                                Math.Max(gcol.A, glowA),
                                Clamp(gcol.R + glowA / 3),
                                Clamp(gcol.G + glowA),
                                Clamp(gcol.B + glowA / 2)));
                        }
                    }

                    outBmp.SetPixel(x, y, col);
                }
            }

            // Center grip highlight
            int hubR = 18;
            for (int y = cy - hubR; y <= cy + hubR; y++)
            {
                for (int x = cx - hubR; x <= cx + hubR; x++)
                {
                    if (x < 0 || y < 0 || x >= w || y >= h || !shape[x, y]) continue;
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d > hubR) continue;
                    outBmp.SetPixel(x, y, Color.FromArgb(255, 210, 255, 225));
                }
            }

            return outBmp;
        }
    }

    public static void Save(string flatPath, string outPath)
    {
        string dir = Path.GetDirectoryName(outPath);
        if (!String.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        using (var bmp = RenderFromFlat(flatPath))
            bmp.Save(outPath, ImageFormat.Png);
    }

    public static void Main(string[] args)
    {
        string flat = args.Length > 0 ? args[0]
            : @"C:\Users\arkti\.cursor\projects\c-Users-arkti-cursor-projects\assets\TheIllidari_PVP_final.png";
        string output = args.Length > 1 ? args[1]
            : @"c:\Users\arkti\.cursor\projects\BurntWaffleBar\Media\Themes\TheIllidari\source\TheIllidari_PVP.png";
        Save(flat, output);
        Console.WriteLine("Saved " + output);
    }
}
