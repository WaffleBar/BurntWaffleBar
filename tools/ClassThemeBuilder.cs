using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

public sealed class ClassThemeDef
{
    public string Id;
    public string Folder;
    public string Label;
    public string ClassName;
    public string Fantasy;
    public float ClockR, ClockG, ClockB;
    public float GlowR, GlowG, GlowB;
    public float AccentR, AccentG, AccentB;
    public float ShadowR, ShadowG, ShadowB;
    public float AccentHue;
    public float ClockHue;
}

public static class ClassThemeBuilder
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

    static readonly ClassThemeDef[] Classes =
    {
        Def("TheWarrior", "The Warrior", "Warrior",
            "Iron and honor — battered plate, crimson war paint, Alliance steel and Horde fury.",
            0.78f, 0.61f, 0.43f, 1.0f, 0.55f, 0.25f, 1.0f, 0.72f, 0.35f, 0.12f, 0.08f, 0.05f, 32f, 28f),
        Def("TheHunter", "The Hunter", "Hunter",
            "Wilderness tracker — leather and bone, emerald marksmanship, beast mastery runes.",
            0.67f, 0.83f, 0.45f, 0.55f, 1.0f, 0.35f, 0.65f, 0.95f, 0.40f, 0.10f, 0.14f, 0.06f, 88f, 95f),
        Def("TheRogue", "The Rogue", "Rogue",
            "Shadow and venom — obsidian daggers, poison vials, guild sigils, yellow-class stealth.",
            1.0f, 0.96f, 0.41f, 1.0f, 0.92f, 0.30f, 0.95f, 0.88f, 0.25f, 0.12f, 0.11f, 0.06f, 52f, 58f),
        Def("ThePriest", "The Priest", "Priest",
            "Holy light and shadow — sanctified silver, divine radiance, void whispers.",
            1.0f, 1.0f, 1.0f, 1.0f, 0.95f, 0.70f, 0.95f, 0.92f, 0.75f, 0.14f, 0.13f, 0.18f, 48f, 280f),
        Def("TheShaman", "The Shaman", "Shaman",
            "Elements unleashed — storm totems, molten lava, tidal blue, ancestral spirits.",
            0.0f, 0.44f, 0.87f, 0.35f, 0.85f, 1.0f, 0.25f, 0.75f, 1.0f, 0.05f, 0.10f, 0.18f, 205f, 210f),
        Def("TheMage", "The Mage", "Mage",
            "Arcane mastery — runic circles, frost crystals, arcane violet-blue energy.",
            0.25f, 0.78f, 0.92f, 0.45f, 0.90f, 1.0f, 0.40f, 0.85f, 1.0f, 0.08f, 0.14f, 0.22f, 195f, 200f),
        Def("TheWarlock", "The Warlock", "Warlock",
            "Fel and shadow — soul shards, demonic contracts, green fel fire, void purple.",
            0.53f, 0.53f, 0.93f, 0.75f, 0.35f, 1.0f, 0.45f, 1.0f, 0.35f, 0.14f, 0.06f, 0.18f, 115f, 265f),
        Def("TheMonk", "The Monk", "Monk",
            "Inner harmony — jade mists, brew barrels, Pandaren scrolls, chi energy.",
            0.0f, 1.0f, 0.59f, 0.35f, 1.0f, 0.70f, 0.20f, 0.95f, 0.65f, 0.06f, 0.14f, 0.10f, 155f, 160f),
        Def("TheDruid", "The Druid", "Druid",
            "Nature's balance — living bark, lunar cycles, feral claws, emerald dream.",
            1.0f, 0.49f, 0.04f, 1.0f, 0.72f, 0.25f, 0.85f, 0.65f, 0.20f, 0.16f, 0.10f, 0.05f, 28f, 35f),
        Def("TheDeathKnight", "The Death Knight", "Death Knight",
            "Unholy frost — runeforged blades, blood runes, Lich King ice, Scourge steel.",
            0.77f, 0.12f, 0.23f, 1.0f, 0.35f, 0.40f, 0.85f, 0.15f, 0.20f, 0.12f, 0.04f, 0.06f, 350f, 355f),
        Def("TheEvoker", "The Evoker", "Evoker",
            "Dracthyr legacy — bronze hourglass, emerald life, azure breath, dragonflight magic.",
            0.20f, 0.58f, 0.50f, 0.40f, 1.0f, 0.85f, 0.35f, 0.90f, 0.75f, 0.06f, 0.12f, 0.11f, 168f, 175f),
    };

    static ClassThemeDef Def(string id, string label, string className, string fantasy,
        float cr, float cg, float cb, float gr, float gg, float gb,
        float ar, float ag, float ab, float sr, float sg, float sb,
        float accentHue, float clockHue)
    {
        return new ClassThemeDef
        {
            Id = id, Folder = id, Label = label, ClassName = className, Fantasy = fantasy,
            ClockR = cr, ClockG = cg, ClockB = cb,
            GlowR = gr, GlowG = gg, GlowB = gb,
            AccentR = ar, AccentG = ag, AccentB = ab,
            ShadowR = sr, ShadowG = sg, ShadowB = sb,
            AccentHue = accentHue, ClockHue = clockHue,
        };
    }

    public static void Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string illidariSource = Path.Combine(root, "Media", "Themes", "TheIllidari", "source");
        if (!Directory.Exists(illidariSource))
        {
            Console.Error.WriteLine("Missing Illidari sources: " + illidariSource);
            Environment.Exit(1);
        }

        foreach (ClassThemeDef theme in Classes)
        {
            Console.WriteLine("=== " + theme.Label + " ===");
            string themeRoot = Path.Combine(root, "Media", "Themes", theme.Folder);
            string sourceDir = Path.Combine(themeRoot, "source");
            Directory.CreateDirectory(sourceDir);

            foreach (string name in Names)
            {
                string illidariPath = Path.Combine(illidariSource, "TheIllidari_" + name + ".png");
                if (!File.Exists(illidariPath))
                {
                    Console.WriteLine("  Missing Illidari: " + name);
                    continue;
                }

                using (var src = new Bitmap(illidariPath))
                using (var remapped = RemapIllidariSource(src, theme))
                {
                    string outSource = Path.Combine(sourceDir, theme.Id + "_" + name + ".png");
                    remapped.Save(outSource, ImageFormat.Png);
                    using (var processed = Process(remapped, theme))
                        processed.Save(Path.Combine(themeRoot, name + ".png"), ImageFormat.Png);
                }
                Console.WriteLine("  " + name);
            }

            WriteThemeDoc(themeRoot, theme);
        }

        WriteIconsLuaFragment(root);
        Console.WriteLine("Done — " + Classes.Length + " class themes built.");
    }

    static void WriteThemeDoc(string themeRoot, ClassThemeDef theme)
    {
        var sb = new StringBuilder();
        sb.AppendLine(theme.Label + " theme");
        sb.AppendLine(new string('=', theme.Label.Length + 6));
        sb.AppendLine();
        sb.AppendLine("Retail WoW " + theme.ClassName + " class identity — " + theme.Fantasy);
        sb.AppendLine();
        sb.AppendLine("Bootstrap: Illidari micro-menu silhouettes recolored to " + theme.ClassName);
        sb.AppendLine("class palette (official UI class color + thematic accent glow).");
        sb.AppendLine();
        sb.AppendLine("Pipeline: tools/ClassThemeBuilder.cs");
        sb.AppendLine("  Raw:  source/" + theme.Id + "_{Name}.png");
        sb.AppendLine("  Out:  this folder / {Name}.png");
        sb.AppendLine();
        sb.AppendLine("Select in /bw → Icon Theme → " + theme.Label + ".");
        File.WriteAllText(Path.Combine(themeRoot, "THEME.txt"), sb.ToString());
    }

    static void WriteIconsLuaFragment(string root)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-- AUTO-GENERATED by ClassThemeBuilder.cs — class theme registry snippet");
        sb.AppendLine();
        foreach (ClassThemeDef t in Classes)
        {
            sb.AppendLine("    " + t.Id + " = MakeClassTheme(");
            sb.AppendFormat("        \"{0}\", \"{1}\",\n", t.Id, t.Label);
            sb.AppendFormat("        {{ {0:0.###}, {1:0.###}, {2:0.###} }},\n", t.ClockR, t.ClockG, t.ClockB);
            sb.AppendFormat("        {{ {0:0.###}, {1:0.###}, {2:0.###} }},\n", t.GlowR, t.GlowG, t.GlowB);
            sb.AppendFormat("        {{ {0:0.###}, {1:0.###}, {2:0.###} }},\n", t.ShadowR * 0.6f, t.ShadowG * 0.6f, t.ShadowB * 0.6f);
            sb.AppendFormat("        {{ {0:0.###}, {1:0.###}, {2:0.###} }}\n", t.ShadowR, t.ShadowG, t.ShadowB);
            sb.AppendLine("    ),");
        }
        File.WriteAllText(Path.Combine(root, "tools", "ClassThemes.generated.lua"), sb.ToString());
    }

    static float Clamp01(float v) { return Math.Max(0f, Math.Min(1f, v)); }
    static byte ClampByte(int v) { return (byte)Math.Max(0, Math.Min(255, v)); }
    static float Lerp(float a, float b, float t) { return a + (b - a) * t; }

    static void RgbToHsl(float r, float g, float b, out float h, out float s, out float l)
    {
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        l = (max + min) * 0.5f;
        if (max <= min + 0.0001f) { h = 0; s = 0; return; }
        float d = max - min;
        s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
        if (max == r) h = ((g - b) / d + (g < b ? 6f : 0f)) / 6f;
        else if (max == g) h = ((b - r) / d + 2f) / 6f;
        else h = ((r - g) / d + 4f) / 6f;
    }

    static void HslToRgb(float h, float s, float l, out float r, out float g, out float b)
    {
        if (s <= 0.0001f) { r = g = b = l; return; }
        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;
        r = HueToRgb(p, q, h + 1f / 3f);
        g = HueToRgb(p, q, h);
        b = HueToRgb(p, q, h - 1f / 3f);
    }

    static float HueToRgb(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    static float HueDist(float a, float b)
    {
        float d = Math.Abs(a - b);
        return Math.Min(d, 1f - d);
    }

    static Bitmap RemapIllidariSource(Bitmap source, ClassThemeDef theme)
    {
        int w = source.Width;
        int h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        float accentH = theme.AccentHue / 360f;
        float clockH = theme.ClockHue / 360f;

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

                float sr = c.R / 255f, sg = c.G / 255f, sb = c.B / 255f;
                float hr, hg, hb;
                RgbToHsl(sr, sg, sb, out hr, out hg, out hb);

                float fel = Math.Max(0f, sg - Math.Max(sr, sb) * 0.82f);
                float purple = Math.Max(0f, sb - sg * 0.88f) + Math.Max(0f, sr - sg * 0.55f) * 0.35f;
                float shadow = Clamp01(1f - hg / 0.38f);
                shadow = shadow * shadow;

                float targetH = hr;
                float targetS = hg;
                if (fel > 0.02f && hg > 0.08f)
                {
                    targetH = accentH;
                    targetS = Math.Min(1f, hg * 1.08f);
                }
                else if (purple > 0.02f && hg > 0.06f)
                {
                    targetH = clockH;
                    targetS = Math.Min(1f, hg * 0.95f);
                }
                else if (shadow > 0.5f)
                {
                    sr = Lerp(sr, theme.ShadowR, shadow * 0.85f);
                    sg = Lerp(sg, theme.ShadowG, shadow * 0.85f);
                    sb = Lerp(sb, theme.ShadowB, shadow * 0.85f);
                    output.SetPixel(x, y, Color.FromArgb(c.A,
                        ClampByte((int)Math.Round(sr * 255f)),
                        ClampByte((int)Math.Round(sg * 255f)),
                        ClampByte((int)Math.Round(sb * 255f))));
                    continue;
                }
                else
                {
                    float blend = Clamp01(fel * 2.5f + purple * 1.8f);
                    if (blend < 0.05f)
                    {
                        float dAccent = HueDist(hr, 120f / 360f);
                        float dPurple = HueDist(hr, 280f / 360f);
                        targetH = dAccent < dPurple ? accentH : clockH;
                        blend = 0.35f;
                    }
                    hr = Lerp(hr, targetH, blend);
                }

                HslToRgb(hr, targetS, hg, out sr, out sg, out sb);

                float accentMix = Clamp01(fel * 3f);
                sr = Lerp(sr, theme.AccentR, accentMix * 0.55f);
                sg = Lerp(sg, theme.AccentG, accentMix * 0.55f);
                sb = Lerp(sb, theme.AccentB, accentMix * 0.55f);

                output.SetPixel(x, y, Color.FromArgb(c.A,
                    ClampByte((int)Math.Round(sr * 255f)),
                    ClampByte((int)Math.Round(sg * 255f)),
                    ClampByte((int)Math.Round(sb * 255f))));
            }
        }
        return output;
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
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color c = bitmap.GetPixel(x, y);
                byte alpha = KeyAlpha(c.R, c.G, c.B);
                if (alpha == 0) bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                else if (alpha < 255) bitmap.SetPixel(x, y, Color.FromArgb(alpha, c.R, c.G, c.B));
            }
    }

    static Rectangle FindContentBounds(Bitmap bitmap, int threshold)
    {
        int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).A <= threshold) continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
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
            float maxDraw = canvasSize * IconScale;
            float scale = Math.Min(maxDraw / source.Width, maxDraw / source.Height);
            g.DrawImage(source, (canvasSize - source.Width * scale) / 2f, (canvasSize - source.Height * scale) / 2f,
                source.Width * scale, source.Height * scale);
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
            g.DrawImage(source, 0, 0, targetSize, targetSize);
        }
        return output;
    }

    static Bitmap AddContactShadow(Bitmap source, ClassThemeDef theme)
    {
        int w = source.Width, h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(output))
        {
            g.Clear(Color.Transparent);
            using (var shadow = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        Color c = source.GetPixel(x, y);
                        if (c.A <= AlphaCutoff) { shadow.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0)); continue; }
                        byte sa = ClampByte((int)Math.Round(c.A * 0.14f));
                        shadow.SetPixel(x, y, Color.FromArgb(sa,
                            ClampByte((int)Math.Round(theme.ShadowR * 255f)),
                            ClampByte((int)Math.Round(theme.ShadowG * 255f)),
                            ClampByte((int)Math.Round(theme.ShadowB * 255f))));
                    }
                g.DrawImage(shadow, 1.2f, 2.0f);
            }
            g.DrawImage(source, 0f, 0f);
        }
        return output;
    }

    static void HarmonizeClock(ref float sr, ref float sg, ref float sb, ClassThemeDef theme, float strength)
    {
        float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
        if (luma < 0.025f) return;
        float clockLuma = theme.ClockR * 0.299f + theme.ClockG * 0.587f + theme.ClockB * 0.114f;
        float scale = Math.Max(0.20f, Math.Min(1.12f, luma / Math.Max(0.001f, clockLuma)));
        float tr = Clamp01(theme.ClockR * scale);
        float tg = Clamp01(theme.ClockG * scale);
        float tb = Clamp01(theme.ClockB * scale);
        float blend = strength * Clamp01(luma * 1.2f);
        sr = Lerp(sr, tr, blend);
        sg = Lerp(sg, tg, blend);
        sb = Lerp(sb, tb, blend);
    }

    static Bitmap EnhanceReadability(Bitmap source, ClassThemeDef theme)
    {
        int w = source.Width, h = source.Height;
        var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c = source.GetPixel(x, y);
                if (c.A <= AlphaCutoff) { output.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0)); continue; }
                float sr = c.R / 255f, sg = c.G / 255f, sb = c.B / 255f;
                float luma = sr * 0.299f + sg * 0.587f + sb * 0.114f;
                float shadowLift = (1f - Math.Min(1f, luma / 0.40f));
                shadowLift *= shadowLift;
                sr = Lerp(sr, theme.ShadowR, shadowLift * 0.5f);
                sg = Lerp(sg, theme.ShadowG, shadowLift * 0.5f);
                sb = Lerp(sb, theme.ShadowB, shadowLift * 0.5f);
                const float gamma = 0.78f;
                sr = (float)Math.Pow(Math.Max(0f, sr), gamma);
                sg = (float)Math.Pow(Math.Max(0f, sg), gamma);
                sb = (float)Math.Pow(Math.Max(0f, sb), gamma);
                float accent = Math.Max(0f, sg - Math.Max(sr, sb) * 0.80f);
                if (accent > 0.015f)
                {
                    float boost = Math.Min(1f, accent * 3f);
                    sr = Lerp(sr, theme.AccentR, boost * 0.25f);
                    sg = Lerp(sg, theme.AccentG, boost * 0.25f);
                    sb = Lerp(sb, theme.AccentB, boost * 0.25f);
                }
                HarmonizeClock(ref sr, ref sg, ref sb, theme, 0.55f);
                output.SetPixel(x, y, Color.FromArgb(c.A,
                    ClampByte((int)Math.Round(sr * 255f)),
                    ClampByte((int)Math.Round(sg * 255f)),
                    ClampByte((int)Math.Round(sb * 255f))));
            }
        return output;
    }

    static Bitmap Process(Bitmap source, ClassThemeDef theme)
    {
        using (var working = RenderScaled(source, WorkSize))
        {
            KeyBackground(working);
            using (var cropped = TightCropRecenter(working, WorkSize))
            using (var withShadow = AddContactShadow(cropped, theme))
            using (var enhanced = EnhanceReadability(withShadow, theme))
                return DownscaleBitmap(enhanced, OutputSize);
        }
    }
}
