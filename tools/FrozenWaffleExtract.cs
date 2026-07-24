using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public static class FrozenWaffleExtract
{
    const int IntermediateSize = 512;
    const int OutputSize = 256;
    const float Padding = 0.05f;
    const float EdgeSoftness = 3.5f;
    const float StrokeWidth = 1.8f;
    const float ShadowOffsetX = 1.4f;
    const float ShadowOffsetY = 2.2f;
    const float ShadowStrength = 0.38f;
    const float TargetFill = 0.82f;
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

                int alpha = Math.Min(255, (lum - lumCutoff + 4) * 255 / 150);
                alpha = (int)Math.Round(Math.Pow(alpha / 255.0, 1.32) * 255.0);
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

    static bool NeedsErodePass(string iconId)
    {
        return iconId == "QuestTracker"
            || iconId == "GameMenu"
            || iconId == "PVP"
            || iconId == "Guild"
            || iconId == "AchievementTracker";
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

    struct FrostTint
    {
        public float HueR;
        public float HueG;
        public float HueB;
    }

    static FrostTint GetFrostTint(string iconId)
    {
        switch (iconId)
        {
            case "PVP":
                return new FrostTint { HueR = 0.92f, HueG = 0.78f, HueB = 1.00f };
            case "AdventureGuide":
                return new FrostTint { HueR = 0.72f, HueG = 0.96f, HueB = 1.00f };
            case "Housing":
                return new FrostTint { HueR = 0.80f, HueG = 0.88f, HueB = 1.00f };
            default:
                return new FrostTint { HueR = 0.78f, HueG = 0.90f, HueB = 1.00f };
        }
    }

    static Bitmap RenderFromSdf(float[,] sdf, float shapeInset, string iconId)
    {
        int mw = sdf.GetLength(0);
        int mh = sdf.GetLength(1);
        float targetH = OutputSize * TargetFill;
        float targetW = OutputSize * TargetFill;
        float scale = targetH / mh;
        if (mw * scale > targetW)
        {
            scale = targetW / mw;
        }
        float drawW = mw * scale;
        float drawH = mh * scale;
        float offsetX = (OutputSize - drawW) / 2f;
        float offsetY = (OutputSize - drawH) / 2f;
        float edge = EdgeSoftness;
        FrostTint tint = GetFrostTint(iconId);

        var output = new Bitmap(OutputSize, OutputSize, PixelFormat.Format32bppArgb);
        var data = output.LockBits(
            new Rectangle(0, 0, OutputSize, OutputSize),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        int stride = Math.Abs(data.Stride);
        var raw = new byte[stride * OutputSize];

        for (int y = 0; y < OutputSize; y++)
        {
            int row = y * stride;
            for (int x = 0; x < OutputSize; x++)
            {
                int i = row + x * 4;
                raw[i + 0] = 0;
                raw[i + 1] = 0;
                raw[i + 2] = 0;
                raw[i + 3] = 0;

                float sx = (x - offsetX) / scale;
                float sy = (y - offsetY) / scale;
                if (sx < -0.5f || sy < -0.5f || sx > mw - 0.5f || sy > mh - 0.5f)
                {
                    continue;
                }

                float dist = (SampleSdf(sdf, sx, sy) - shapeInset) * scale;
                float fillAlpha = dist > 0f ? SmoothAlpha(dist, edge) : 0f;
                float strokeAlpha = 0f;

                if (dist <= 0f && dist > -(StrokeWidth + edge))
                {
                    float outerAlpha = SmoothAlpha(dist + StrokeWidth, edge);
                    float innerAlpha = SmoothAlpha(dist, edge);
                    strokeAlpha = outerAlpha * (1f - innerAlpha);
                }

                float shadowDist = (SampleSdf(sdf, sx - ShadowOffsetX / scale, sy - ShadowOffsetY / scale) - shapeInset) * scale;
                float shadowAlpha = SmoothAlpha(shadowDist, edge * 1.25f) * ShadowStrength;

                float nx = drawW > 0f ? (x - offsetX) / drawW : 0.5f;
                float ny = drawH > 0f ? 1f - (y - offsetY) / drawH : 0.5f;

                float outR = 0f;
                float outG = 0f;
                float outB = 0f;
                float outA = 0f;

                if (shadowAlpha > 0.001f)
                {
                    outR = 12f;
                    outG = 28f;
                    outB = 52f;
                    outA = shadowAlpha;
                }

                if (strokeAlpha > 0.001f)
                {
                    if (strokeAlpha >= outA)
                    {
                        outR = 18f;
                        outG = 48f;
                        outB = 82f;
                        outA = strokeAlpha;
                    }
                    else
                    {
                        float inv = 1f - outA;
                        float combined = strokeAlpha + outA * inv;
                        outR = (18f * strokeAlpha + outR * outA * inv) / combined;
                        outG = (48f * strokeAlpha + outG * outA * inv) / combined;
                        outB = (82f * strokeAlpha + outB * outA * inv) / combined;
                        outA = combined;
                    }
                }

                if (fillAlpha > 0.001f)
                {
                    float baseTone = 0.68f + 0.32f * ny;
                    float bevel = 0.55f + 0.45f * SmoothStep(0f, 1f, Math.Min(1f, dist / (edge * 2.8f)));
                    float specular = SmoothStep(0.18f, 0.90f, (1f - nx) * ny)
                        * SmoothStep(edge, edge * 6f, dist)
                        * 0.42f;
                    float satin = SmoothStep(0.40f, 0.58f, ny) * 0.08f;
                    float ambient = 1f - SmoothStep(0f, 0.45f, 1f - ny) * 0.12f;
                    float lum = Math.Min(1f, (baseTone * bevel + specular + satin) * ambient);

                    float hi = 0.55f + 0.45f * lum;
                    float mid = 0.35f + 0.50f * lum;
                    float fr = (220f * hi + 95f * mid) * tint.HueR;
                    float fg = (235f * hi + 145f * mid) * tint.HueG;
                    float fb = (255f * hi + 195f * mid) * tint.HueB;

                    if (fillAlpha >= outA)
                    {
                        outR = fr;
                        outG = fg;
                        outB = fb;
                        outA = fillAlpha;
                    }
                    else
                    {
                        float inv = 1f - outA;
                        float combined = fillAlpha + outA * inv;
                        outR = (fr * fillAlpha + outR * outA * inv) / combined;
                        outG = (fg * fillAlpha + outG * outA * inv) / combined;
                        outB = (fb * fillAlpha + outB * outA * inv) / combined;
                        outA = combined;
                    }
                }

                if (outA <= 0.001f)
                {
                    continue;
                }

                raw[i + 0] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(outB)));
                raw[i + 1] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(outG)));
                raw[i + 2] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(outR)));
                raw[i + 3] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(outA * 255f)));
            }
        }

        Marshal.Copy(raw, 0, data.Scan0, raw.Length);
        output.UnlockBits(data);
        return output;
    }

    static Bitmap ProcessMask(Bitmap barSlice, string iconId)
    {
        using (var soft = MakeSoftMask(barSlice, iconId == "GroupFinder"))
        using (var upscaled = FitToCanvasByHeight(soft, IntermediateSize))
        {
            int threshold = GetAlphaThreshold(iconId);
            bool[,] mask = AlphaToMask(upscaled, threshold);
            upscaled.Dispose();

            if (iconId == "GroupFinder")
            {
                mask = RemoveSmallComponents(mask, 4);
                mask = CloseMask(mask);
            }
            else
            {
                mask = RemoveSmallComponents(mask, GetMinComponentSize(iconId));
                if (NeedsErodePass(iconId))
                {
                    mask = ErodeMask(mask);
                }
            }
            mask = CropMaskToContent(mask, 3);
            float[,] sdf = BuildSignedDistanceField(mask);
            return RenderFromSdf(sdf, GetShapeInset(iconId), iconId);
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
}
