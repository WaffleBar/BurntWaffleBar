using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public static class ProcessThePaladinIcons
{
    const int OutputSize = 256;
    const int WorkSize = 512;
    const int RenderScale = 2;
    const float IconScale = 0.93f;
    const float EdgeSoftness = 2.2f;
    const float ShadowOffsetX = 1.4f;
    const float ShadowOffsetY = 2.2f;
    const float ShadowStrength = 0.28f;
    const float TargetFill = 0.88f;
    const int ContentPad = 3;
    const int AlphaCutoff = 20;

    static readonly string[] Names =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static float Clamp01(float value)
    {
        return Math.Max(0f, Math.Min(1f, value));
    }

    static byte ClampByte(int value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }

    static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    static float SmoothStep(float edge0, float edge1, float x)
    {
        if (edge0 >= edge1) return x >= edge1 ? 1f : 0f;
        float t = Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

    static float SmoothAlpha(float dist, float edge)
    {
        return SmoothStep(-edge, edge, dist);
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

    static float[,] ExtractAlphaGrid(Bitmap bitmap)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        var grid = new float[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                grid[x, y] = bitmap.GetPixel(x, y).A / 255f;
        }
        return grid;
    }

    static float[,] ExtractLumaGrid(Bitmap bitmap)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        var grid = new float[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = bitmap.GetPixel(x, y);
                if (c.A <= AlphaCutoff)
                {
                    grid[x, y] = 0f;
                    continue;
                }

                grid[x, y] = (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 255f;
            }
        }
        return grid;
    }

    static float[,] ExtractGoldGrid(Bitmap bitmap)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        var grid = new float[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = bitmap.GetPixel(x, y);
                if (c.A <= AlphaCutoff) { grid[x, y] = 0f; continue; }
                float luma = (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 255f;
                float max = Math.Max(c.R, Math.Max(c.G, c.B)) / 255f;
                float min = Math.Min(c.R, Math.Min(c.G, c.B)) / 255f;
                float sat = max <= 0.001f ? 0f : (max - min) / max;
                if (sat > 0.08f && c.R >= c.G * 0.88f && luma > 0.26f)
                    grid[x, y] = Clamp01(sat * 1.25f + (luma - 0.22f) * 0.55f);
                else if (luma > 0.58f && c.R > c.B)
                    grid[x, y] = Clamp01((luma - 0.45f) * 1.4f);
                else
                    grid[x, y] = 0f;
            }
        }
        return grid;
    }

    static float SampleBilinear(float[,] grid, int w, int h, float x, float y)
    {
        if (x <= 0f || y <= 0f || x >= w - 1f || y >= h - 1f) return 0f;
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        float tx = x - x0;
        float ty = y - y0;
        float a00 = grid[x0, y0];
        float a10 = grid[x0 + 1, y0];
        float a01 = grid[x0, y0 + 1];
        float a11 = grid[x0 + 1, y0 + 1];
        float a0 = a00 + (a10 - a00) * tx;
        float a1 = a01 + (a11 - a01) * tx;
        return a0 + (a1 - a0) * ty;
    }

    static bool[,] AlphaToMask(Bitmap bitmap, int threshold)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        var mask = new bool[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                mask[x, y] = bitmap.GetPixel(x, y).A >= threshold;
        }
        return mask;
    }

    static bool IsEdge(bool[,] mask, int x, int y)
    {
        int w = mask.GetLength(0);
        int h = mask.GetLength(1);
        bool value = mask[x, y];
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                if (ox == 0 && oy == 0) continue;
                int nx = x + ox;
                int ny = y + oy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) return true;
                if (mask[nx, ny] != value) return true;
            }
        }
        return false;
    }

    static void ChamferPass(float[,] grid, int w, int h, bool reverse)
    {
        int yStart = reverse ? h - 1 : 0, yEnd = reverse ? -1 : h, yStep = reverse ? -1 : 1;
        int xStart = reverse ? w - 1 : 0, xEnd = reverse ? -1 : w, xStep = reverse ? -1 : 1;
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

        for (int i = 0; i < 4; i++)
        {
            ChamferPass(inside, w, h, false);
            ChamferPass(inside, w, h, true);
            ChamferPass(outside, w, h, false);
            ChamferPass(outside, w, h, true);
        }

        var sdf = new float[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                sdf[x, y] = mask[x, y] ? inside[x, y] : -outside[x, y];
        return sdf;
    }

    static float SampleSdf(float[,] sdf, float x, float y)
    {
        int w = sdf.GetLength(0);
        int h = sdf.GetLength(1);
        if (x <= 0f || y <= 0f || x >= w - 1f || y >= h - 1f) return -EdgeSoftness;
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        float tx = x - x0;
        float ty = y - y0;
        float ab = sdf[x0, y0] + (sdf[x0 + 1, y0] - sdf[x0, y0]) * tx;
        float cd = sdf[x0, y0 + 1] + (sdf[x0 + 1, y0 + 1] - sdf[x0, y0 + 1]) * tx;
        return ab + (cd - ab) * ty;
    }

    static float SampleDistAt(float[,] sdf, int mw, int mh, float scale, float offsetX, float offsetY, float inset, float px, float py)
    {
        float sx = (px - offsetX) / scale;
        float sy = (py - offsetY) / scale;
        if (sx < -0.5f || sy < -0.5f || sx > mw - 0.5f || sy > mh - 0.5f) return -EdgeSoftness;
        return (SampleSdf(sdf, sx, sy) - inset) * scale;
    }

    static void CompositeOver(byte[] raw, int i, byte sr, byte sg, byte sb, byte sa)
    {
        if (sa <= 0) return;
        byte da = raw[i + 3];
        if (da == 0)
        {
            raw[i + 0] = sr; raw[i + 1] = sg; raw[i + 2] = sb; raw[i + 3] = sa;
            return;
        }
        float srcA = sa / 255f;
        float dstA = da / 255f;
        float outA = srcA + dstA * (1f - srcA);
        if (outA <= 0f) return;
        raw[i + 0] = ClampByte((int)Math.Round((sr * srcA + raw[i + 0] * dstA * (1f - srcA)) / outA));
        raw[i + 1] = ClampByte((int)Math.Round((sg * srcA + raw[i + 1] * dstA * (1f - srcA)) / outA));
        raw[i + 2] = ClampByte((int)Math.Round((sb * srcA + raw[i + 2] * dstA * (1f - srcA)) / outA));
        raw[i + 3] = ClampByte((int)Math.Round(outA * 255f));
    }

    static void ExtractRgbGrids(Bitmap bitmap, out float[,] rGrid, out float[,] gGrid, out float[,] bGrid)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        rGrid = new float[w, h];
        gGrid = new float[w, h];
        bGrid = new float[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = bitmap.GetPixel(x, y);
                if (c.A <= AlphaCutoff)
                {
                    rGrid[x, y] = gGrid[x, y] = bGrid[x, y] = 0f;
                    continue;
                }
                rGrid[x, y] = c.R / 255f;
                gGrid[x, y] = c.G / 255f;
                bGrid[x, y] = c.B / 255f;
            }
        }
    }

    static void WarmShiftSourceColor(ref float sr, ref float sg, ref float sb, float goldMix)
    {
        if (sb > sr * 0.92f && sb > sg * 0.88f)
        {
            float blueLead = sb - Math.Max(sr, sg);
            float fix = Clamp01(blueLead * 4.2f);
            sr = Lerp(sr, Math.Min(1f, sr + blueLead * 1.05f), fix);
            sg = Lerp(sg, Math.Min(1f, sg + blueLead * 0.42f), fix);
            sb = Lerp(sb, Math.Max(0.10f, sb * 0.40f), fix);
        }

        float coolBias = sb - (sr * 0.72f + sg * 0.18f);
        if (coolBias > 0.03f)
        {
            float fix = Clamp01(coolBias * 3.0f);
            sr = Lerp(sr, Math.Min(1f, sr + 0.20f), fix);
            sg = Lerp(sg, Math.Min(1f, sg + 0.08f), fix * 0.70f);
            sb = Lerp(sb, Math.Max(0.10f, sb * 0.50f), fix);
        }

        float gold = Clamp01(goldMix * 1.45f + Math.Max(0f, sr - sb) * 0.35f);
        sr = Lerp(sr, Math.Min(1f, sr * 1.10f + 0.06f), gold);
        sg = Lerp(sg, Math.Min(1f, sg * 1.04f + 0.03f), gold);
        sb = Lerp(sb, sb * 0.68f, gold);
    }

    static void PaladinColorBody(float fillAlpha, float distPx, float renderScale, float ny, float goldMix, float lumaDetail, float sr, float sg, float sb, out byte r, out byte g, out byte b, out byte a)
    {
        WarmShiftSourceColor(ref sr, ref sg, ref sb, goldMix);

        float light = 0.84f + (1f - ny) * 0.14f;
        light *= 0.80f + lumaDetail * 0.36f;

        float ao = distPx > 0f
            ? 1f - SmoothStep(0f, 5f * renderScale, distPx) * 0.10f
            : 1f;

        float alpha = fillAlpha * ao;
        r = ClampByte((int)Math.Round(sr * light * alpha * 255f));
        g = ClampByte((int)Math.Round(sg * light * alpha * 255f));
        b = ClampByte((int)Math.Round(sb * light * alpha * 255f));
        a = ClampByte((int)Math.Round(alpha * 255f));
    }

    static void PaladinEdgeRim(float distPx, float fillAlpha, float renderScale, float goldMix, out byte r, out byte g, out byte b, out byte a)
    {
        float edgeLine = 1f - SmoothStep(0f, 1.6f * renderScale, distPx);
        if (edgeLine <= 0.04f || distPx <= 0f) { r = g = b = a = 0; return; }

        float gold = Clamp01(goldMix * 1.25f);
        float alpha = edgeLine * fillAlpha * Lerp(0.28f, 0.42f, gold);
        float hr = Lerp(244f, 255f, gold);
        float hg = Lerp(140f, 220f, gold);
        float hb = Lerp(186f, 120f, gold);
        r = ClampByte((int)Math.Round(hr * alpha));
        g = ClampByte((int)Math.Round(hg * alpha));
        b = ClampByte((int)Math.Round(hb * alpha));
        a = ClampByte((int)Math.Round(alpha * 255f));
    }

    static void PaladinMetalHighlight(float distPx, float fillAlpha, float nx, float ny, float renderScale, float goldMix, float lumaDetail, out byte r, out byte g, out byte b, out byte a)
    {
        PaladinEdgeRim(distPx, fillAlpha, renderScale, goldMix, out r, out g, out b, out a);
    }

    static Bitmap RenderPaladinIcon(float[,] sdf, float[,] softAlpha, float[,] goldGrid, float[,] lumaGrid, float[,] rGrid, float[,] gGrid, float[,] bGrid, int workSize)
    {
        int renderSize = workSize * RenderScale;
        float renderScaleF = RenderScale;
        int mw = sdf.GetLength(0);
        int mh = sdf.GetLength(1);
        float targetH = renderSize * TargetFill;
        float scale = targetH / mh;
        if (mw * scale > renderSize * TargetFill)
            scale = renderSize * TargetFill / mw;
        float drawW = mw * scale;
        float drawH = mh * scale;
        float offsetX = (renderSize - drawW) / 2f;
        float offsetY = (renderSize - drawH) / 2f;
        float edge = EdgeSoftness * renderScaleF;
        float shadowOx = ShadowOffsetX * renderScaleF;
        float shadowOy = ShadowOffsetY * renderScaleF;
        const float shapeInset = 0.15f;

        var output = new Bitmap(renderSize, renderSize, PixelFormat.Format32bppArgb);
        var data = output.LockBits(new Rectangle(0, 0, renderSize, renderSize), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        int stride = Math.Abs(data.Stride);
        var raw = new byte[stride * renderSize];

        for (int y = 0; y < renderSize; y++)
        {
            int row = y * stride;
            for (int x = 0; x < renderSize; x++)
            {
                int i = row + x * 4;
                raw[i + 0] = raw[i + 1] = raw[i + 2] = raw[i + 3] = 0;

                float shadowDist = SampleDistAt(sdf, mw, mh, scale, offsetX, offsetY, shapeInset, x - shadowOx, y - shadowOy);
                float shadowAlpha = shadowDist > 0f ? SmoothAlpha(shadowDist, edge * 1.5f) * ShadowStrength : 0f;
                if (shadowAlpha > 0.001f)
                {
                    byte sa = ClampByte((int)Math.Round(shadowAlpha * 255f));
                    CompositeOver(raw, i, 20, 10, 16, sa);
                }

                float dist = SampleDistAt(sdf, mw, mh, scale, offsetX, offsetY, shapeInset, x, y);
                float sx = (x - offsetX) / scale;
                float sy = (y - offsetY) / scale;
                float softSample = SampleBilinear(softAlpha, mw, mh, sx, sy);
                if (softSample <= 0.004f) continue;

                float sdfAlpha = dist > 0f ? SmoothAlpha(dist, edge) : 0f;
                float fillAlpha = softSample * sdfAlpha;
                if (fillAlpha <= 0.001f) continue;

                float goldMix = SampleBilinear(goldGrid, mw, mh, sx, sy);
                float lumaDetail = SampleBilinear(lumaGrid, mw, mh, sx, sy);
                float sr = SampleBilinear(rGrid, mw, mh, sx, sy);
                float sg = SampleBilinear(gGrid, mw, mh, sx, sy);
                float sb = SampleBilinear(bGrid, mw, mh, sx, sy);
                float nx = drawW > 0f ? (x - offsetX) / drawW : 0.5f;
                float ny = drawH > 0f ? 1f - (y - offsetY) / drawH : 0.5f;

                byte br, bg, bb, ba;
                PaladinColorBody(fillAlpha, dist, renderScaleF, ny, goldMix, lumaDetail, sr, sg, sb, out br, out bg, out bb, out ba);
                if (ba > 0) CompositeOver(raw, i, br, bg, bb, ba);

                byte hr, hg, hb, ha;
                PaladinMetalHighlight(dist, fillAlpha, nx, ny, renderScaleF, goldMix, lumaDetail, out hr, out hg, out hb, out ha);
                if (ha > 0)
                {
                    ha = ClampByte((int)Math.Round(ha * Math.Min(1f, softSample * 1.02f)));
                    CompositeOver(raw, i, hr, hg, hb, ha);
                }
            }
        }

        Marshal.Copy(raw, 0, data.Scan0, raw.Length);
        output.UnlockBits(data);
        return output;
    }

    static Bitmap Process(Bitmap source)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            using (var withShadow = AddWarmContactShadow(cropped))
            {
                return DownscaleBitmap(withShadow, OutputSize);
            }
        }
    }

    static Bitmap AddWarmContactShadow(Bitmap source)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(output))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingMode = CompositingMode.SourceOver;

            // Warm plum shadow, offset down — keeps source colors intact.
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

                        byte sa = ClampByte((int)Math.Round(c.A * 0.22f));
                        shadow.SetPixel(x, y, Color.FromArgb(sa, 24, 12, 18));
                    }
                }

                g.DrawImage(shadow, 1.2f, 2.0f);
            }

            g.DrawImage(source, 0f, 0f);
        }
        return output;
    }

    static Bitmap ProcessWithSdfBake_UNUSED(Bitmap source)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            {
                float[,] softAlpha = ExtractAlphaGrid(cropped);
                float[,] goldGrid = ExtractGoldGrid(cropped);
                float[,] lumaGrid = ExtractLumaGrid(cropped);
                float[,] rGrid, gGrid, bGrid;
                ExtractRgbGrids(cropped, out rGrid, out gGrid, out bGrid);
                bool[,] mask = AlphaToMask(cropped, 30);
                float[,] sdf = BuildSignedDistanceField(mask);
                using (var rendered = RenderPaladinIcon(sdf, softAlpha, goldGrid, lumaGrid, rGrid, gGrid, bGrid, WorkSize))
                {
                    return DownscaleBitmap(rendered, OutputSize);
                }
            }
        }
    }

    public static void ProcessAll(string inputDir, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        foreach (string name in Names)
        {
            string input = Path.Combine(inputDir, "ThePaladin_" + name + ".png");
            if (!File.Exists(input)) { Console.WriteLine("Missing: " + input); continue; }
            using (var src = new Bitmap(input))
            using (var processed = Process(src))
                processed.Save(Path.Combine(outputDir, name + ".png"), ImageFormat.Png);
            Console.WriteLine("Processed " + name);
        }
    }
}
