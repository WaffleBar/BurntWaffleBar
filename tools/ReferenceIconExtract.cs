using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Text;
using System.Runtime.InteropServices;

public static class ReferenceIconExtract
{
    const int IntermediateSize = 1024;
    const int OutputSize = 256;
    const int RenderScale = 2;
    const int ClockRenderScale = 4;
    const int ClockOutputSize = 512;
    const float Padding = 0.05f;
    const float EdgeSoftness = 1.75f;
    const float ClockEdgeSoftness = 3.0f;
    const float ShadowOffsetX = 1.3f;
    const float ShadowOffsetY = 2.1f;
    const float ShadowStrength = 0.18f;
    const float TargetFill = 0.83f;
    const int AlphaThreshold = 88;
    const int AlphaThresholdThin = 40;

    static readonly Dictionary<string, int[]> ButtonRegions = new Dictionary<string, int[]>
    {
        { "Collections", new[] { 33, 54 } },
        { "AdventureGuide", new[] { 65, 78 } },
        { "Guild", new[] { 86, 107 } },
        { "Housing", new[] { 115, 133 } },
        { "PVP", new[] { 140, 159 } },
        { "QuestTracker", new[] { 165, 187 } },
        { "GroupFinder", new[] { 194, 214 } },
        { "AchievementTracker", new[] { 218, 239 } },
        { "Talents", new[] { 248, 264 } },
        { "Character", new[] { 277, 290 } },
        { "Social", new[] { 296, 319 } },
        { "GameMenu", new[] { 379, 397 } },
    };

    static bool IsGoldNumber(byte r, byte g, byte b)
    {
        return r > 150 && g > 120 && b < 110;
    }

    static bool IsForeground(byte r, byte g, byte b)
    {
        if (IsGoldNumber(r, g, b)) return false;
        int lum = (r + g + b) / 3;
        return lum >= 70;
    }

    static Bitmap ExtractRegion(Bitmap source, int left, int right)
    {
        int w = right - left + 1;
        int h = source.Height;
        var crop = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(crop))
        {
            g.DrawImage(source, 0, 0, new Rectangle(left, 0, w, h), GraphicsUnit.Pixel);
        }
        return crop;
    }

    static Rectangle GetHorizontalBounds(Bitmap crop)
    {
        int minX = crop.Width;
        int maxX = -1;

        for (int y = 0; y < crop.Height; y++)
        {
            for (int x = 0; x < crop.Width; x++)
            {
                Color pixel = crop.GetPixel(x, y);
                if (IsForeground(pixel.R, pixel.G, pixel.B))
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
        }

        if (maxX < 0)
        {
            return new Rectangle(0, 0, crop.Width, crop.Height);
        }

        return new Rectangle(minX, 0, maxX - minX + 1, crop.Height);
    }

    static Bitmap TrimHorizontalOnly(Bitmap crop)
    {
        Rectangle bounds = GetHorizontalBounds(crop);
        var trimmed = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(trimmed))
        {
            g.DrawImage(crop, 0, 0, bounds, GraphicsUnit.Pixel);
        }
        crop.Dispose();
        return trimmed;
    }

    static Bitmap MakeSoftMask(Bitmap crop, bool captureThinStrokes)
    {
        int lumCutoff = captureThinStrokes ? 62 : 88;
        var result = new Bitmap(crop.Width, crop.Height, PixelFormat.Format32bppArgb);
        for (int y = 0; y < crop.Height; y++)
        {
            for (int x = 0; x < crop.Width; x++)
            {
                Color pixel = crop.GetPixel(x, y);
                if (IsGoldNumber(pixel.R, pixel.G, pixel.B))
                {
                    result.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    continue;
                }

                int lum = (pixel.R + pixel.G + pixel.B) / 3;
                if (lum < lumCutoff)
                {
                    result.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    continue;
                }

                int span = Math.Max(1, 255 - lumCutoff);
                int alpha = Math.Min(255, Math.Max(0, (lum - lumCutoff) * 255 / span));
                alpha = (int)Math.Round(Math.Pow(alpha / 255.0, 1.0) * 255.0);
                result.SetPixel(x, y, Color.FromArgb(alpha, 255, 255, 255));
            }
        }
        crop.Dispose();
        return result;
    }

    static Bitmap FitToCanvasByHeight(Bitmap source, int canvasSize)
    {
        var canvas = new Bitmap(canvasSize, canvasSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(canvas))
        {
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            float targetH = canvasSize * TargetFill;
            float scale = targetH / source.Height;
            float drawW = source.Width * scale;
            float drawH = source.Height * scale;
            float drawX = (canvasSize - drawW) / 2f;
            float drawY = (canvasSize - drawH) / 2f;
            g.DrawImage(source, drawX, drawY, drawW, drawH);
        }
        return canvas;
    }

    static bool[,] AlphaToMask(Bitmap bitmap, int threshold)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        var mask = new bool[w, h];
        var data = bitmap.LockBits(
            new Rectangle(0, 0, w, h),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);
        int stride = Math.Abs(data.Stride);
        var raw = new byte[stride * h];
        Marshal.Copy(data.Scan0, raw, 0, raw.Length);
        bitmap.UnlockBits(data);

        for (int y = 0; y < h; y++)
        {
            int row = y * stride;
            for (int x = 0; x < w; x++)
            {
                mask[x, y] = raw[row + x * 4 + 3] >= threshold;
            }
        }

        return mask;
    }

    static bool[,] CropMaskToContent(bool[,] mask, int pad)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        int minX = w;
        int minY = h;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y]) continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0)
        {
            return mask;
        }

        minX = Math.Max(0, minX - pad);
        minY = Math.Max(0, minY - pad);
        maxX = Math.Min(w - 1, maxX + pad);
        maxY = Math.Min(h - 1, maxY + pad);

        int cw = maxX - minX + 1;
        int ch = maxY - minY + 1;
        var cropped = new bool[cw, ch];
        for (int y = 0; y < ch; y++)
        {
            for (int x = 0; x < cw; x++)
            {
                cropped[x, y] = mask[minX + x, minY + y];
            }
        }

        return cropped;
    }

    static bool[,] DilateMask(bool[,] mask)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        var dilated = new bool[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y])
                {
                    continue;
                }

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        dilated[nx, ny] = true;
                    }
                }
            }
        }

        return dilated;
    }

    static bool[,] ErodeMask(bool[,] mask)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        var eroded = new bool[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y])
                {
                    continue;
                }

                bool keep = true;
                for (int dy = -1; dy <= 1 && keep; dy++)
                {
                    for (int dx = -1; dx <= 1 && keep; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h || !mask[nx, ny])
                        {
                            keep = false;
                        }
                    }
                }

                eroded[x, y] = keep;
            }
        }

        return eroded;
    }

    static bool[,] CloseMask(bool[,] mask)
    {
        return ErodeMask(DilateMask(mask));
    }

    static bool[,] RemoveSmallComponents(bool[,] mask, int minPixels)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        var visited = new bool[w, h];
        var kept = new bool[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y] || visited[x, y]) continue;

                var stack = new Stack<KeyValuePair<int, int>>();
                var pixels = new List<KeyValuePair<int, int>>();
                stack.Push(new KeyValuePair<int, int>(x, y));
                visited[x, y] = true;

                while (stack.Count > 0)
                {
                    KeyValuePair<int, int> p = stack.Pop();
                    pixels.Add(p);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = p.Key + dx;
                            int ny = p.Value + dy;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                            if (!mask[nx, ny] || visited[nx, ny]) continue;
                            visited[nx, ny] = true;
                            stack.Push(new KeyValuePair<int, int>(nx, ny));
                        }
                    }
                }

                if (pixels.Count >= minPixels)
                {
                    foreach (KeyValuePair<int, int> p in pixels)
                    {
                        kept[p.Key, p.Value] = true;
                    }
                }
            }
        }

        return kept;
    }

    static bool[,] KeepLargestComponent(bool[,] mask)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        var visited = new bool[w, h];
        int bestSize = 0;
        List<int> bestPixels = null;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y] || visited[x, y]) continue;

                var stack = new Stack<KeyValuePair<int, int>>();
                var pixels = new List<int>();
                stack.Push(new KeyValuePair<int, int>(x, y));
                visited[x, y] = true;

                while (stack.Count > 0)
                {
                    KeyValuePair<int, int> p = stack.Pop();
                    pixels.Add(p.Key);
                    pixels.Add(p.Value);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = p.Key + dx;
                            int ny = p.Value + dy;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                            if (!mask[nx, ny] || visited[nx, ny]) continue;
                            visited[nx, ny] = true;
                            stack.Push(new KeyValuePair<int, int>(nx, ny));
                        }
                    }
                }

                if (pixels.Count / 2 > bestSize)
                {
                    bestSize = pixels.Count / 2;
                    bestPixels = pixels;
                }
            }
        }

        var cleaned = new bool[w, h];
        if (bestPixels == null)
        {
            return mask;
        }

        for (int i = 0; i < bestPixels.Count; i += 2)
        {
            cleaned[bestPixels[i], bestPixels[i + 1]] = true;
        }

        return cleaned;
    }

    static bool IsEdge(bool[,] mask, int x, int y)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        bool value = mask[x, y];

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) return true;
                if (mask[nx, ny] != value) return true;
            }
        }

        return false;
    }

    static float[,] BuildSignedDistanceField(bool[,] mask)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        const float inf = 1e6f;
        var inside = new float[w, h];
        var outside = new float[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (mask[x, y])
                {
                    inside[x, y] = IsEdge(mask, x, y) ? 0f : inf;
                    outside[x, y] = inf;
                }
                else
                {
                    inside[x, y] = inf;
                    outside[x, y] = IsEdge(mask, x, y) ? 0f : inf;
                }
            }
        }

        ChamferPass(inside, w, h, false);
        ChamferPass(inside, w, h, true);
        ChamferPass(inside, w, h, false);
        ChamferPass(inside, w, h, true);
        ChamferPass(outside, w, h, false);
        ChamferPass(outside, w, h, true);
        ChamferPass(outside, w, h, false);
        ChamferPass(outside, w, h, true);

        var sdf = new float[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                sdf[x, y] = mask[x, y] ? inside[x, y] : -outside[x, y];
            }
        }

        return sdf;
    }

    static void ChamferPass(float[,] grid, int w, int h, bool reverse)
    {
        int yStart = reverse ? h - 1 : 0;
        int yEnd = reverse ? -1 : h;
        int yStep = reverse ? -1 : 1;
        int xStart = reverse ? w - 1 : 0;
        int xEnd = reverse ? -1 : w;
        int xStep = reverse ? -1 : 1;

        for (int y = yStart; y != yEnd; y += yStep)
        {
            for (int x = xStart; x != xEnd; x += xStep)
            {
                float value = grid[x, y];
                if (reverse)
                {
                    if (x + 1 < w) value = Math.Min(value, grid[x + 1, y] + 3f);
                    if (y + 1 < h) value = Math.Min(value, grid[x, y + 1] + 3f);
                    if (x + 1 < w && y + 1 < h) value = Math.Min(value, grid[x + 1, y + 1] + 4f);
                    if (x - 1 >= 0 && y + 1 < h) value = Math.Min(value, grid[x - 1, y + 1] + 4f);
                }
                else
                {
                    if (x - 1 >= 0) value = Math.Min(value, grid[x - 1, y] + 3f);
                    if (y - 1 >= 0) value = Math.Min(value, grid[x, y - 1] + 3f);
                    if (x - 1 >= 0 && y - 1 >= 0) value = Math.Min(value, grid[x - 1, y - 1] + 4f);
                    if (x + 1 < w && y - 1 >= 0) value = Math.Min(value, grid[x + 1, y - 1] + 4f);
                }

                grid[x, y] = value;
            }
        }
    }

    static float SampleSdf(float[,] sdf, float x, float y)
    {
        int w = sdf.GetLength(0);
        int h = sdf.GetLength(1);

        if (x <= 0f || y <= 0f || x >= w - 1f || y >= h - 1f)
        {
            return -EdgeSoftness;
        }

        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        float tx = x - x0;
        float ty = y - y0;

        float a = sdf[x0, y0];
        float b = sdf[x0 + 1, y0];
        float c = sdf[x0, y0 + 1];
        float d = sdf[x0 + 1, y0 + 1];
        float ab = a + (b - a) * tx;
        float cd = c + (d - c) * tx;
        return ab + (cd - ab) * ty;
    }

    static float GetShapeInset(string iconId)
    {
        switch (iconId)
        {
            case "GroupFinder":
                return 0.5f;
            case "Collections":
                return 1.3f;
            case "QuestTracker":
            case "GameMenu":
            case "PVP":
            case "Guild":
            case "AchievementTracker":
                return 2.25f;
            default:
                return 1.85f;
        }
    }

    static int GetAlphaThreshold(string iconId)
    {
        switch (iconId)
        {
            case "GroupFinder":
                return AlphaThresholdThin;
            case "Collections":
                return 76;
            case "QuestTracker":
            case "GameMenu":
            case "PVP":
            case "Guild":
            case "AchievementTracker":
                return 92;
            default:
                return AlphaThreshold;
        }
    }

    static int GetMinComponentSize(string iconId)
    {
        return iconId == "Collections" ? 6 : 8;
    }

    static float SmoothStep(float edge0, float edge1, float x)
    {
        if (edge0 >= edge1)
        {
            return x >= edge1 ? 1f : 0f;
        }

        float t = Math.Max(0f, Math.Min(1f, (x - edge0) / (edge1 - edge0)));
        return t * t * (3f - 2f * t);
    }

    static float SmoothAlpha(float dist, float edge)
    {
        return SmoothStep(-edge, edge, dist);
    }

    static float SampleDistAt(
        float[,] sdf,
        int mw,
        int mh,
        float scale,
        float offsetX,
        float offsetY,
        float shapeInset,
        float px,
        float py)
    {
        float sx = (px - offsetX) / scale;
        float sy = (py - offsetY) / scale;
        if (sx < -0.5f || sy < -0.5f || sx > mw - 0.5f || sy > mh - 0.5f)
        {
            return -EdgeSoftness;
        }

        return (SampleSdf(sdf, sx, sy) - shapeInset) * scale;
    }

    static void ClearGlassBody(
        float fillAlpha,
        float distPx,
        float renderScale,
        out byte r,
        out byte g,
        out byte b,
        out byte a)
    {
        float core = SmoothStep(2f * renderScale, 18f * renderScale, distPx);
        float bodyAlpha = 0.34f + core * 0.14f;

        r = 255;
        g = 255;
        b = 255;
        a = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(fillAlpha * bodyAlpha * 255f)));
    }

    static void ClearGlassHighlight(
        float distPx,
        float fillAlpha,
        float nx,
        float ny,
        float renderScale,
        out byte r,
        out byte g,
        out byte b,
        out byte a)
    {
        float edgeLine = 1f - SmoothStep(0f, 2.6f * renderScale, distPx);
        float innerRim = (1f - SmoothStep(1.4f * renderScale, 22f * renderScale, distPx)) * 0.96f;
        float rim = Math.Max(edgeLine, innerRim);

        float specX = (nx - 0.22f) / 0.40f;
        float specY = (ny - 0.68f) / 0.36f;
        float specBroad = Math.Max(0f, 1f - specX * specX - specY * specY);
        specBroad = specBroad * specBroad;

        float specX2 = (nx - 0.24f) / 0.16f;
        float specY2 = (ny - 0.72f) / 0.14f;
        float specCore = Math.Max(0f, 1f - specX2 * specX2 - specY2 * specY2);
        specCore = specCore * specCore * specCore;
        float spec = specBroad * 0.90f + specCore * 0.85f;

        float topGlow = SmoothStep(0.18f, 0.96f, ny) * SmoothStep(1f * renderScale, 26f * renderScale, distPx);

        float highlight = rim;
        highlight = Math.Max(highlight, spec * 0.96f);
        highlight = Math.Max(highlight, topGlow * 0.68f);

        if (highlight <= 0.02f)
        {
            r = g = b = a = 0;
            return;
        }

        float edgeAA = Math.Max(fillAlpha, SmoothStep(0f, 2.2f * renderScale, distPx));
        float alpha = highlight * edgeAA;

        r = 255;
        g = 255;
        b = 255;
        a = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(alpha * 255f)));
    }

    static void CompositeOver(
        byte[] raw,
        int i,
        byte sr,
        byte sg,
        byte sb,
        byte sa)
    {
        if (sa <= 0)
        {
            return;
        }

        byte da = raw[i + 3];
        if (da == 0)
        {
            raw[i + 0] = sr;
            raw[i + 1] = sg;
            raw[i + 2] = sb;
            raw[i + 3] = sa;
            return;
        }

        float srcA = sa / 255f;
        float dstA = da / 255f;
        float outA = srcA + dstA * (1f - srcA);
        if (outA <= 0f)
        {
            return;
        }

        float invOutA = 1f / outA;
        raw[i + 0] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round((sr * srcA + raw[i + 0] * dstA * (1f - srcA)) * invOutA)));
        raw[i + 1] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round((sg * srcA + raw[i + 1] * dstA * (1f - srcA)) * invOutA)));
        raw[i + 2] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round((sb * srcA + raw[i + 2] * dstA * (1f - srcA)) * invOutA)));
        raw[i + 3] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(outA * 255f)));
    }

    static Bitmap DownscaleBitmap(Bitmap source, int targetSize)
    {
        var output = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(output))
        {
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(source, 0, 0, targetSize, targetSize);
        }

        return output;
    }

    static Bitmap RenderFromSdf(float[,] sdf, float shapeInset)
    {
        return RenderFromSdf(sdf, shapeInset, RenderScale, EdgeSoftness, OutputSize, null);
    }

    static float[,] ExtractAlphaGrid(Bitmap bitmap)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        var alpha = new float[w, h];
        var data = bitmap.LockBits(
            new Rectangle(0, 0, w, h),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);
        int stride = Math.Abs(data.Stride);
        var raw = new byte[stride * h];
        Marshal.Copy(data.Scan0, raw, 0, raw.Length);
        bitmap.UnlockBits(data);

        for (int y = 0; y < h; y++)
        {
            int row = y * stride;
            for (int x = 0; x < w; x++)
            {
                alpha[x, y] = raw[row + x * 4 + 3] / 255f;
            }
        }

        return alpha;
    }

    static float SampleAlphaBilinear(float[,] alpha, int w, int h, float x, float y)
    {
        x = Math.Max(0f, Math.Min(w - 1.001f, x));
        y = Math.Max(0f, Math.Min(h - 1.001f, y));
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        float tx = x - x0;
        float ty = y - y0;
        int x1 = Math.Min(x0 + 1, w - 1);
        int y1 = Math.Min(y0 + 1, h - 1);

        float a00 = alpha[x0, y0];
        float a10 = alpha[x1, y0];
        float a01 = alpha[x0, y1];
        float a11 = alpha[x1, y1];
        float a0 = a00 + (a10 - a00) * tx;
        float a1 = a01 + (a11 - a01) * tx;
        return a0 + (a1 - a0) * ty;
    }

    static Bitmap RenderFromSdf(float[,] sdf, float shapeInset, int renderScale, float edgeSoftness, int outputSize, float[,] softAlpha)
    {
        int renderSize = outputSize * renderScale;
        float renderScaleF = renderScale;
        int mw = sdf.GetLength(0);
        int mh = sdf.GetLength(1);
        float targetH = renderSize * TargetFill;
        float targetW = renderSize * TargetFill;
        float scale = targetH / mh;
        if (mw * scale > targetW)
        {
            scale = targetW / mw;
        }
        float drawW = mw * scale;
        float drawH = mh * scale;
        float offsetX = (renderSize - drawW) / 2f;
        float offsetY = (renderSize - drawH) / 2f;
        float edge = edgeSoftness * renderScaleF;
        float shadowOffsetX = ShadowOffsetX * renderScaleF;
        float shadowOffsetY = ShadowOffsetY * renderScaleF;

        var output = new Bitmap(renderSize, renderSize, PixelFormat.Format32bppArgb);
        var data = output.LockBits(
            new Rectangle(0, 0, renderSize, renderSize),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        int stride = Math.Abs(data.Stride);
        var raw = new byte[stride * renderSize];

        for (int y = 0; y < renderSize; y++)
        {
            int row = y * stride;
            for (int x = 0; x < renderSize; x++)
            {
                int i = row + x * 4;
                raw[i + 0] = 0;
                raw[i + 1] = 0;
                raw[i + 2] = 0;
                raw[i + 3] = 0;

                float shadowDist = SampleDistAt(
                    sdf, mw, mh, scale, offsetX, offsetY, shapeInset,
                    x - shadowOffsetX, y - shadowOffsetY);
                float shadowAlpha = shadowDist > 0f ? SmoothAlpha(shadowDist, edge * 1.6f) * ShadowStrength : 0f;
                if (shadowAlpha > 0.001f)
                {
                    byte sa = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(shadowAlpha * 255f)));
                    CompositeOver(raw, i, 12, 18, 32, sa);
                }

                float dist = SampleDistAt(sdf, mw, mh, scale, offsetX, offsetY, shapeInset, x, y);
                float softSample = 1f;
                if (softAlpha != null)
                {
                    float sx = (x - offsetX) / scale;
                    float sy = (y - offsetY) / scale;
                    softSample = SampleAlphaBilinear(softAlpha, mw, mh, sx, sy);
                    if (softSample <= 0.002f)
                    {
                        continue;
                    }
                }

                float fillAlpha = dist > 0f ? SmoothAlpha(dist, edge) : 0f;
                fillAlpha *= Math.Min(1f, softSample * 1.08f);
                if (fillAlpha <= 0.001f)
                {
                    continue;
                }

                float nx = drawW > 0f ? (x - offsetX) / drawW : 0.5f;
                float ny = drawH > 0f ? 1f - (y - offsetY) / drawH : 0.5f;

                byte br;
                byte bg;
                byte bb;
                byte ba;
                ClearGlassBody(fillAlpha, dist, renderScaleF, out br, out bg, out bb, out ba);
                if (ba > 0)
                {
                    CompositeOver(raw, i, br, bg, bb, ba);
                }

                byte hr;
                byte hg;
                byte hb;
                byte ha;
                ClearGlassHighlight(dist, fillAlpha, nx, ny, renderScaleF, out hr, out hg, out hb, out ha);
                if (softAlpha != null && ha > 0)
                {
                    ha = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(ha * Math.Min(1f, softSample * 1.04f))));
                }
                if (ha > 0)
                {
                    CompositeOver(raw, i, hr, hg, hb, ha);
                }
            }
        }

        Marshal.Copy(raw, 0, data.Scan0, raw.Length);
        output.UnlockBits(data);

        var downscaled = DownscaleBitmap(output, outputSize);
        output.Dispose();
        return downscaled;
    }

    static void ComputeGlassLayout(int mw, int mh, int renderSize, out float scale, out float offsetX, out float offsetY, out float drawW, out float drawH)
    {
        float targetH = renderSize * TargetFill;
        float targetW = renderSize * TargetFill;
        scale = targetH / mh;
        if (mw * scale > targetW)
        {
            scale = targetW / mw;
        }

        drawW = mw * scale;
        drawH = mh * scale;
        offsetX = (renderSize - drawW) / 2f;
        offsetY = (renderSize - drawH) / 2f;
    }

    static Bitmap RenderGlassFromSoftAlpha(float[,] alphaGrid, int outputSize)
    {
        int renderScale = ClockRenderScale;
        int renderSize = outputSize * renderScale;
        int mw = alphaGrid.GetLength(0);
        int mh = alphaGrid.GetLength(1);

        float scale;
        float offsetX;
        float offsetY;
        float drawW;
        float drawH;
        ComputeGlassLayout(mw, mh, renderSize, out scale, out offsetX, out offsetY, out drawW, out drawH);

        float shadowOffsetX = ShadowOffsetX * renderScale;
        float shadowOffsetY = ShadowOffsetY * renderScale;

        var output = new Bitmap(renderSize, renderSize, PixelFormat.Format32bppArgb);
        var data = output.LockBits(
            new Rectangle(0, 0, renderSize, renderSize),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        int stride = Math.Abs(data.Stride);
        var raw = new byte[stride * renderSize];

        for (int y = 0; y < renderSize; y++)
        {
            int row = y * stride;
            for (int x = 0; x < renderSize; x++)
            {
                int i = row + x * 4;
                raw[i + 0] = 0;
                raw[i + 1] = 0;
                raw[i + 2] = 0;
                raw[i + 3] = 0;

                float sx = (x - offsetX) / scale;
                float sy = (y - offsetY) / scale;
                float shadowSample = SampleAlphaBilinear(alphaGrid, mw, mh, sx - shadowOffsetX, sy - shadowOffsetY);
                float shadowAlpha = shadowSample * ShadowStrength;
                if (shadowAlpha > 0.001f)
                {
                    byte sa = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(shadowAlpha * 255f)));
                    CompositeOver(raw, i, 12, 18, 32, sa);
                }

                float alpha = SampleAlphaBilinear(alphaGrid, mw, mh, sx, sy);
                if (alpha <= 0.001f)
                {
                    continue;
                }

                float nx = drawW > 0f ? (x - offsetX) / drawW : 0.5f;
                float ny = drawH > 0f ? 1f - (y - offsetY) / drawH : 0.5f;
                float distPx = Math.Max(0.75f, (alpha - 0.38f) * 30f);
                float fillAlpha = Math.Min(1f, alpha * 1.04f);
                float edgeFade = SmoothStep(0.02f, 0.22f, alpha) * SmoothStep(1f, 0.78f, alpha);

                byte br;
                byte bg;
                byte bb;
                byte ba;
                ClearGlassBody(fillAlpha * edgeFade, distPx, 1f, out br, out bg, out bb, out ba);
                if (ba > 0)
                {
                    CompositeOver(raw, i, br, bg, bb, ba);
                }

                byte hr;
                byte hg;
                byte hb;
                byte ha;
                ClearGlassHighlight(distPx, fillAlpha * edgeFade, nx, ny, 1f, out hr, out hg, out hb, out ha);
                if (ha > 0)
                {
                    ha = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(ha * Math.Min(1f, alpha * 1.08f))));
                    CompositeOver(raw, i, hr, hg, hb, ha);
                }
            }
        }

        Marshal.Copy(raw, 0, data.Scan0, raw.Length);
        output.UnlockBits(data);

        var downscaled = DownscaleBitmap(output, outputSize);
        output.Dispose();
        return downscaled;
    }

    static Bitmap ProcessMask(Bitmap barSlice, string iconId)
    {
        using (var soft = MakeSoftMask(barSlice, iconId == "GroupFinder"))
        using (var upscaled = FitToCanvasByHeight(soft, IntermediateSize))
        {
            int threshold = GetAlphaThreshold(iconId);
            bool[,] mask = AlphaToMask(upscaled, threshold);

            mask = RemoveSmallComponents(mask, iconId == "GroupFinder" ? 4 : GetMinComponentSize(iconId));
            mask = CloseMask(mask);
            mask = CropMaskToContent(mask, 4);
            float[,] sdf = BuildSignedDistanceField(mask);
            return RenderFromSdf(sdf, GetShapeInset(iconId));
        }
    }

    public static void ExtractAll(string referencePath, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        using (var source = new Bitmap(referencePath))
        {
            foreach (KeyValuePair<string, int[]> entry in ButtonRegions)
            {
                using (var crop = ExtractRegion(source, entry.Value[0], entry.Value[1]))
                using (var barSlice = TrimHorizontalOnly(crop))
                using (var rendered = ProcessMask(barSlice, entry.Key))
                {
                    rendered.Save(Path.Combine(outputDir, entry.Key + ".png"), ImageFormat.Png);
                }
            }
        }
    }

    static Bitmap RenderTextMask(string text, Font font)
    {
        const int super = 2;
        const int canvasW = 1024;
        const int canvasH = 1024;
        const int pad = 64;
        var canvas = new Bitmap(canvasW * super, canvasH * super, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(canvas))
        {
            g.Clear(Color.Transparent);
            g.ScaleTransform(super, super);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(text, font, brush, pad, pad, StringFormat.GenericTypographic);
            }
        }

        int minX = canvasW;
        int minY = canvasH;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < canvasH; y++)
        {
            for (int x = 0; x < canvasW; x++)
            {
                if (canvas.GetPixel(x * super + super / 2, y * super + super / 2).A <= 8)
                {
                    continue;
                }

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0)
        {
            return canvas;
        }

        minX = Math.Max(0, minX - pad / 2);
        minY = Math.Max(0, minY - pad / 2);
        maxX = Math.Min(canvasW - 1, maxX + pad / 2);
        maxY = Math.Min(canvasH - 1, maxY + pad / 2);

        int cropW = maxX - minX + 1;
        int cropH = maxY - minY + 1;
        var cropped = new Bitmap(cropW, cropH, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cropped))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(
                canvas,
                new Rectangle(0, 0, cropW, cropH),
                minX * super,
                minY * super,
                cropW * super,
                cropH * super,
                GraphicsUnit.Pixel);
        }

        canvas.Dispose();
        return cropped;
    }

    static Bitmap ProcessTextMask(Bitmap textMask)
    {
        using (var upscaled = FitToCanvasByHeight(textMask, IntermediateSize))
        {
            float[,] softAlpha = ExtractAlphaGrid(upscaled);
            bool[,] mask = AlphaToMask(upscaled, 32);
            mask = RemoveSmallComponents(mask, 2);
            float[,] sdf = BuildSignedDistanceField(mask);
            return RenderFromSdf(sdf, 0.15f, ClockRenderScale, ClockEdgeSoftness, ClockOutputSize, softAlpha);
        }
    }

    static float MeasureLayoutAdvance(Bitmap textMask, Font font, string glyph, int outputSize)
    {
        float typographicWidth;
        using (var measureBitmap = new Bitmap(1, 1))
        using (var g = Graphics.FromImage(measureBitmap))
        {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            typographicWidth = g.MeasureString(
                glyph,
                font,
                PointF.Empty,
                StringFormat.GenericTypographic).Width;
        }

        if (textMask.Height <= 0)
        {
            return 0.35f;
        }

        float scale = (outputSize * TargetFill) / textMask.Height;
        return (typographicWidth * scale) / outputSize;
    }

    static void AnalyzeDigitLayout(Bitmap bitmap, Bitmap textMask, Font font, string glyph, out float advance, out float u0, out float u1)
    {
        int minX = bitmap.Width;
        int maxX = -1;

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).A <= 4)
                {
                    continue;
                }

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
            }
        }

        if (maxX < 0)
        {
            advance = 0.35f;
            u0 = 0f;
            u1 = 1f;
            return;
        }

        u0 = Math.Max(0f, (minX - 8f) / bitmap.Width);
        u1 = Math.Min(1f, (maxX + 9f) / bitmap.Width);
        float cropWidth = u1 - u0;
        float typographicAdvance = MeasureLayoutAdvance(textMask, font, glyph, bitmap.Height);
        advance = Math.Max(cropWidth, typographicAdvance);
    }

    static string ClockDigitFileName(string glyph)
    {
        switch (glyph)
        {
            case ":": return "ClockColon";
            case " ": return "ClockSpace";
            case "A": return "ClockA";
            case "M": return "ClockM";
            case "P": return "ClockP";
            default: return "Clock" + glyph;
        }
    }

    static string EscapeLuaString(string value)
    {
        if (value == " ") return "' '";
        if (value == ":") return "':'";
        return "'" + value + "'";
    }

    public static void ExtractClockDigits(string fontPath, string outputDir, string manifestPath, float fontSizePx = 480f)
    {
        Directory.CreateDirectory(outputDir);

        var fontCollection = new PrivateFontCollection();
        fontCollection.AddFontFile(fontPath);
        FontFamily family = fontCollection.Families[0];

        var manifestLines = new List<string>
        {
            "local addonName, ns = ...",
            "",
            "ns.pristineClockDigitLayout = {",
        };

        string glyphs = "0123456789:AMP";
        Font familyFont = null;
        try
        {
            familyFont = new Font(family, fontSizePx, FontStyle.Regular, GraphicsUnit.Pixel);
            foreach (char c in glyphs)
            {
                string glyph = c.ToString();
                using (var textMask = RenderTextMask(glyph, familyFont))
                using (var rendered = ProcessTextMask(textMask))
                {
                    string fileName = ClockDigitFileName(glyph) + ".png";
                    rendered.Save(Path.Combine(outputDir, fileName), ImageFormat.Png);

                    float advance;
                    float u0;
                    float u1;
                    AnalyzeDigitLayout(rendered, textMask, familyFont, glyph, out advance, out u0, out u1);
                    manifestLines.Add(string.Format(
                        "    [{0}] = {{ advance = {1:F4}, u0 = {2:F4}, u1 = {3:F4} }},",
                        EscapeLuaString(glyph),
                        advance,
                        u0,
                        u1));
                }
            }

            using (var textMask = RenderTextMask(" ", familyFont))
            using (var rendered = ProcessTextMask(textMask))
            {
                rendered.Save(Path.Combine(outputDir, ClockDigitFileName(" ") + ".png"), ImageFormat.Png);
                float advance;
                float u0;
                float u1;
                AnalyzeDigitLayout(rendered, textMask, familyFont, " ", out advance, out u0, out u1);
                manifestLines.Add(string.Format(
                    "    [{0}] = {{ advance = {1:F4}, u0 = {2:F4}, u1 = {3:F4} }},",
                    EscapeLuaString(" "),
                    advance,
                    u0,
                    u1));
            }
        }
        finally
        {
            if (familyFont != null)
            {
                familyFont.Dispose();
            }

            fontCollection.Dispose();
        }

        manifestLines.Add("}");
        manifestLines.Add("");
        File.WriteAllLines(manifestPath, manifestLines);
    }
}
