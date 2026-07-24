using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public struct ClassPalette
{
    public string Id;
    public string Label;
    public Color Accent;
    public Color Glow;
    public Color MetalLight;
    public Color MetalMid;
    public Color MetalDark;
    public Color Shadow;
    public Color Clock;

    public static ClassPalette Warrior() { return P("TheWarrior", "Warrior", 199, 156, 110, 255, 180, 80, 210, 195, 175, 120, 100, 80, 40, 25, 15, 204, 148, 120); }
    public static ClassPalette Hunter() { return P("TheHunter", "Hunter", 170, 189, 38, 140, 255, 80, 195, 210, 170, 130, 115, 90, 25, 35, 12, 180, 210, 120); }
    public static ClassPalette Rogue() { return P("TheRogue", "Rogue", 255, 245, 105, 255, 220, 50, 200, 195, 175, 130, 120, 100, 35, 30, 15, 255, 240, 140); }
    public static ClassPalette Priest() { return P("ThePriest", "Priest", 255, 255, 255, 255, 240, 180, 235, 230, 220, 170, 160, 150, 30, 28, 40, 255, 250, 245); }
    public static ClassPalette Shaman() { return P("TheShaman", "Shaman", 0, 112, 222, 80, 200, 255, 180, 195, 210, 100, 120, 150, 15, 25, 45, 100, 180, 255); }
    public static ClassPalette Mage() { return P("TheMage", "Mage", 63, 199, 235, 120, 220, 255, 200, 215, 230, 110, 130, 160, 20, 35, 55, 140, 210, 255); }
    public static ClassPalette Warlock() { return P("TheWarlock", "Warlock", 135, 135, 237, 180, 80, 255, 170, 155, 190, 90, 70, 110, 35, 15, 45, 180, 160, 240); }
    public static ClassPalette Monk() { return P("TheMonk", "Monk", 0, 255, 150, 100, 255, 180, 195, 210, 185, 120, 140, 110, 15, 40, 30, 120, 255, 190); }
    public static ClassPalette Druid() { return P("TheDruid", "Druid", 255, 125, 10, 255, 200, 80, 190, 170, 140, 110, 90, 70, 45, 30, 12, 255, 180, 100); }
    public static ClassPalette DeathKnight() { return P("TheDeathKnight", "Death Knight", 196, 31, 59, 255, 80, 100, 170, 175, 185, 100, 110, 130, 25, 8, 15, 220, 100, 120); }
    public static ClassPalette Evoker() { return P("TheEvoker", "Evoker", 51, 147, 127, 80, 255, 200, 185, 200, 195, 110, 130, 125, 15, 35, 35, 100, 220, 190); }

    static ClassPalette P(string id, string label,
        int ar, int ag, int ab, int gr, int gg, int gb,
        int ml, int mm, int md, int mk, int mj, int mn,
        int sr, int sg, int sb, int cr, int cg, int cb)
    {
        return new ClassPalette
        {
            Id = id, Label = label,
            Accent = C(ar, ag, ab), Glow = C(gr, gg, gb),
            MetalLight = C(ml, mm, md), MetalMid = C(mk, mj, mn), MetalDark = C(mk - 20, mj - 20, mn - 20),
            Shadow = C(sr, sg, sb), Clock = C(cr, cg, cb),
        };
    }

    static Color C(int r, int g, int b) { return Color.FromArgb(255, Clamp(r), Clamp(g), Clamp(b)); }
    static int Clamp(int v) { return Math.Max(0, Math.Min(255, v)); }

    public static ClassPalette[] All()
    {
        return new[]
        {
            Warrior(), Hunter(), Rogue(), Priest(), Shaman(), Mage(),
            Warlock(), Monk(), Druid(), DeathKnight(), Evoker(),
        };
    }
}

public static class ClassIconCore
{
    public const int Canvas = 1024;
    const float C = Canvas * 0.5f;

    public static void SetupGraphics(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.Clear(Color.Black);
    }

    public static GraphicsPath Circle(float cx, float cy, float r)
    {
        var p = new GraphicsPath();
        p.AddEllipse(cx - r, cy - r, r * 2, r * 2);
        return p;
    }

    public static GraphicsPath RoundRect(float x, float y, float w, float h, float rad)
    {
        var p = new GraphicsPath();
        float d = Math.Min(rad * 2, Math.Min(w, h));
        p.AddArc(x, y, d, d, 180, 90);
        p.AddArc(x + w - d, y, d, d, 270, 90);
        p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
        p.AddArc(x, y + h - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static void FillMetal(Graphics g, GraphicsPath path, ClassPalette pal, float lx, float ly)
    {
        RectangleF b = path.GetBounds();
        using (var sh = (GraphicsPath)path.Clone())
        using (var m = new Matrix())
        {
            m.Translate(8, 12);
            sh.Transform(m);
            using (var br = new SolidBrush(Color.FromArgb(100, pal.Shadow)))
                g.FillPath(br, sh);
        }

        using (var glow = new Pen(Color.FromArgb(90, pal.Glow), 14f) { LineJoin = LineJoin.Round })
            g.DrawPath(glow, path);

        using (var body = new PathGradientBrush(path))
        {
            body.CenterPoint = new PointF(b.Left + b.Width * lx, b.Top + b.Height * ly);
            body.CenterColor = pal.MetalLight;
            body.SurroundColors = new[] { pal.MetalDark };
            body.FocusScales = new PointF(0.4f, 0.4f);
            g.FillPath(body, path);
        }

        using (var rim = new Pen(Color.FromArgb(200, pal.MetalMid), 4f) { LineJoin = LineJoin.Round })
            g.DrawPath(rim, path);
    }

    public static void FillAccent(Graphics g, GraphicsPath path, ClassPalette pal)
    {
        RectangleF b = path.GetBounds();
        using (var glow = new Pen(Color.FromArgb(120, pal.Glow), 10f) { LineJoin = LineJoin.Round })
            g.DrawPath(glow, path);
        using (var body = new PathGradientBrush(path))
        {
            body.CenterPoint = new PointF(b.X + b.Width * 0.35f, b.Y + b.Height * 0.3f);
            body.CenterColor = Color.FromArgb(255, Lerp(pal.Accent, Color.White, 0.45f));
            body.SurroundColors = new[] { pal.Accent };
            body.FocusScales = new PointF(0.35f, 0.35f);
            g.FillPath(body, path);
        }
    }

    public static void DrawGleam(Graphics g, float cx, float cy, float rx, float ry, ClassPalette pal)
    {
        using (var p = Circle(cx, cy, Math.Max(rx, ry)))
        using (var br = new PathGradientBrush(p))
        {
            br.CenterPoint = new PointF(cx - rx * 0.3f, cy - ry * 0.3f);
            br.CenterColor = Color.FromArgb(180, 255, 255, 255);
            br.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
            br.FocusScales = new PointF(0.2f, 0.15f);
            g.FillEllipse(br, cx - rx, cy - ry, rx * 2, ry * 2);
        }
    }

    public static GraphicsPath Horseshoe(float cx, float cy, float outerR, float innerR)
    {
        var p = new GraphicsPath();
        p.AddArc(cx - outerR, cy - outerR, outerR * 2, outerR * 2, 200, 140);
        var inner = new GraphicsPath();
        inner.AddArc(cx - innerR, cy - innerR, innerR * 2, innerR * 2, 200, 140);
        p.AddPath(inner, false);
        return p;
    }

    public static void DrawShieldBack(Graphics g, float cx, float cy, float w, float h, ClassPalette pal)
    {
        var p = new GraphicsPath();
        p.AddBezier(new PointF(cx, cy - h), new PointF(cx + w, cy - h * 0.55f), new PointF(cx + w * 0.85f, cy + h * 0.7f), new PointF(cx, cy + h));
        p.AddBezier(new PointF(cx, cy + h), new PointF(cx - w * 0.85f, cy + h * 0.7f), new PointF(cx - w, cy - h * 0.55f), new PointF(cx, cy - h));
        p.CloseFigure();
        FillMetal(g, p, pal, 0.3f, 0.25f);
        p.Dispose();
    }

    public static void DrawSword(Graphics g, float cx, float cy, float len, float angle, ClassPalette pal)
    {
        var state = g.Save();
        g.TranslateTransform(cx, cy);
        g.RotateTransform(angle);
        var blade = new GraphicsPath();
        blade.AddPolygon(new[]
        {
            new PointF(-len * 0.08f, -len * 0.5f), new PointF(0, -len * 0.52f), new PointF(len * 0.08f, -len * 0.5f),
            new PointF(len * 0.05f, len * 0.35f), new PointF(0, len * 0.42f), new PointF(-len * 0.05f, len * 0.35f),
        });
        FillMetal(g, blade, pal, 0.35f, 0.2f);
        using (var guard = RoundRect(-len * 0.18f, len * 0.28f, len * 0.36f, len * 0.08f, len * 0.03f))
            FillAccent(g, guard, pal);
        using (var grip = RoundRect(-len * 0.05f, len * 0.36f, len * 0.10f, len * 0.18f, len * 0.03f))
            FillMetal(g, grip, pal, 0.4f, 0.3f);
        blade.Dispose();
        g.Restore(state);
    }

    public static void DrawDagger(Graphics g, float cx, float cy, float len, float angle, ClassPalette pal)
    {
        var state = g.Save();
        g.TranslateTransform(cx, cy);
        g.RotateTransform(angle);
        var blade = new GraphicsPath();
        blade.AddPolygon(new[]
        {
            new PointF(0, -len * 0.55f), new PointF(len * 0.1f, len * 0.15f), new PointF(0, len * 0.22f), new PointF(-len * 0.1f, len * 0.15f),
        });
        FillMetal(g, blade, pal, 0.35f, 0.2f);
        using (var hilt = Circle(0, len * 0.28f, len * 0.08f))
            FillAccent(g, hilt, pal);
        blade.Dispose();
        g.Restore(state);
    }

    public static void DrawBow(Graphics g, float cx, float cy, float h, ClassPalette pal)
    {
        var bow = new GraphicsPath();
        bow.AddBezier(new PointF(cx, cy - h), new PointF(cx + h * 0.55f, cy - h * 0.1f), new PointF(cx + h * 0.55f, cy + h * 0.1f), new PointF(cx, cy + h));
        using (var pen = new Pen(pal.MetalLight, 12f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            g.DrawPath(pen, bow);
        using (var str = new Pen(Color.FromArgb(200, 220, 220, 220), 2f))
            g.DrawLine(str, cx, cy - h * 0.92f, cx, cy + h * 0.92f);
        var arrow = new GraphicsPath();
        arrow.AddLine(cx, cy - h * 0.85f, cx + h * 0.75f, cy);
        arrow.AddLine(cx + h * 0.75f, cy, cx, cy + h * 0.15f);
        using (var ap = new Pen(pal.Accent, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            g.DrawPath(ap, arrow);
        bow.Dispose();
    }

    public static void DrawStaff(Graphics g, float cx, float cy, float h, ClassPalette pal, bool orb)
    {
        using (var shaft = RoundRect(cx - h * 0.04f, cy - h * 0.45f, h * 0.08f, h * 0.95f, h * 0.02f))
            FillMetal(g, shaft, pal, 0.35f, 0.2f);
        if (orb)
        {
            using (var o = Circle(cx, cy - h * 0.52f, h * 0.14f))
                FillAccent(g, o, pal);
            DrawGleam(g, cx - h * 0.04f, cy - h * 0.55f, h * 0.08f, h * 0.06f, pal);
        }
        else
        {
            using (var top = Circle(cx, cy - h * 0.48f, h * 0.1f))
                FillAccent(g, top, pal);
        }
    }

    public static void DrawTotem(Graphics g, float cx, float cy, float h, ClassPalette pal)
    {
        using (var baseP = RoundRect(cx - h * 0.22f, cy + h * 0.15f, h * 0.44f, h * 0.18f, h * 0.04f))
            FillMetal(g, baseP, pal, 0.3f, 0.25f);
        using (var pole = RoundRect(cx - h * 0.08f, cy - h * 0.35f, h * 0.16f, h * 0.55f, h * 0.03f))
            FillAccent(g, pole, pal);
        using (var head = Circle(cx, cy - h * 0.42f, h * 0.16f))
            FillAccent(g, head, pal);
        // lightning
        var bolt = new GraphicsPath();
        bolt.AddPolygon(new[]
        {
            new PointF(cx + h * 0.05f, cy - h * 0.55f), new PointF(cx + h * 0.22f, cy - h * 0.2f),
            new PointF(cx + h * 0.08f, cy - h * 0.22f), new PointF(cx + h * 0.18f, cy + h * 0.05f),
            new PointF(cx - h * 0.02f, cy - h * 0.15f), new PointF(cx + h * 0.06f, cy - h * 0.18f),
        });
        FillAccent(g, bolt, pal);
        bolt.Dispose();
    }

    public static void DrawHolyCross(Graphics g, float cx, float cy, float s, ClassPalette pal)
    {
        using (var v = RoundRect(cx - s * 0.12f, cy - s * 0.5f, s * 0.24f, s, s * 0.06f))
            FillAccent(g, v, pal);
        using (var h = RoundRect(cx - s * 0.38f, cy - s * 0.28f, s * 0.76f, s * 0.24f, s * 0.06f))
            FillAccent(g, h, pal);
        DrawGleam(g, cx - s * 0.15f, cy - s * 0.35f, s * 0.2f, s * 0.15f, pal);
    }

    public static void DrawSkull(Graphics g, float cx, float cy, float r, ClassPalette pal)
    {
        using (var skull = Circle(cx, cy, r))
            FillMetal(g, skull, pal, 0.35f, 0.25f);
        using (var eyeL = Circle(cx - r * 0.35f, cy - r * 0.1f, r * 0.22f))
        using (var eyeR = Circle(cx + r * 0.35f, cy - r * 0.1f, r * 0.22f))
        using (var br = new SolidBrush(pal.Glow))
        {
            g.FillEllipse(br, cx - r * 0.55f, cy - r * 0.28f, r * 0.44f, r * 0.44f);
            g.FillEllipse(br, cx + r * 0.11f, cy - r * 0.28f, r * 0.44f, r * 0.44f);
        }
    }

    public static void DrawPaw(Graphics g, float cx, float cy, float s, ClassPalette pal)
    {
        using (var pad = Circle(cx, cy + s * 0.15f, s * 0.35f))
            FillAccent(g, pad, pal);
        for (int i = -2; i <= 2; i++)
        {
            float ox = i * s * 0.22f;
            float oy = -s * 0.15f - Math.Abs(i) * s * 0.05f;
            using (var toe = Circle(cx + ox, cy + oy, s * 0.14f))
                FillAccent(g, toe, pal);
        }
    }

    public static void DrawMoon(Graphics g, float cx, float cy, float r, ClassPalette pal)
    {
        using (var moon = Circle(cx, cy, r))
            FillAccent(g, moon, pal);
        using (var cut = Circle(cx + r * 0.35f, cy - r * 0.05f, r * 0.85f))
        using (var br = new SolidBrush(Color.Black))
            g.FillEllipse(br, cx + r * 0.05f, cy - r * 0.85f, r * 1.7f, r * 1.7f);
    }

    public static void DrawDragonEye(Graphics g, float cx, float cy, float r, ClassPalette pal)
    {
        var eye = new GraphicsPath();
        eye.AddBezier(new PointF(cx - r, cy), new PointF(cx - r * 0.2f, cy - r * 0.7f), new PointF(cx + r * 0.2f, cy - r * 0.7f), new PointF(cx + r, cy));
        eye.AddBezier(new PointF(cx + r, cy), new PointF(cx + r * 0.2f, cy + r * 0.7f), new PointF(cx - r * 0.2f, cy + r * 0.7f), new PointF(cx - r, cy));
        FillMetal(g, eye, pal, 0.35f, 0.25f);
        using (var iris = Circle(cx, cy, r * 0.45f))
            FillAccent(g, iris, pal);
        using (var pupil = Circle(cx, cy, r * 0.18f))
        using (var br = new SolidBrush(Color.FromArgb(220, 10, 10, 20)))
            g.FillEllipse(br, cx - r * 0.18f, cy - r * 0.18f, r * 0.36f, r * 0.36f);
        eye.Dispose();
    }

    public static void DrawFist(Graphics g, float cx, float cy, float s, ClassPalette pal)
    {
        using (var fist = RoundRect(cx - s * 0.35f, cy - s * 0.25f, s * 0.7f, s * 0.55f, s * 0.12f))
            FillMetal(g, fist, pal, 0.35f, 0.25f);
        for (int i = 0; i < 4; i++)
            using (var kn = RoundRect(cx - s * 0.32f + i * s * 0.18f, cy - s * 0.42f, s * 0.14f, s * 0.22f, s * 0.04f))
                FillAccent(g, kn, pal);
    }

    public static void DrawRuneBlade(Graphics g, float cx, float cy, float len, float angle, ClassPalette pal)
    {
        DrawSword(g, cx, cy, len, angle, pal);
        var state = g.Save();
        g.TranslateTransform(cx, cy);
        g.RotateTransform(angle);
        using (var rune = Circle(0, -len * 0.15f, len * 0.07f))
            FillAccent(g, rune, pal);
        g.Restore(state);
    }

    public static void DrawExclamation(Graphics g, float cx, float cy, float h, ClassPalette pal)
    {
        using (var body = RoundRect(cx - h * 0.12f, cy - h * 0.42f, h * 0.24f, h * 0.55f, h * 0.06f))
            FillAccent(g, body, pal);
        using (var dot = Circle(cx, cy + h * 0.32f, h * 0.12f))
            FillAccent(g, dot, pal);
    }

    public static void DrawGear(Graphics g, float cx, float cy, float r, ClassPalette pal, Action<Graphics, float, float, float> centerDraw)
    {
        var gear = new GraphicsPath();
        int teeth = 10;
        for (int i = 0; i < teeth; i++)
        {
            float a0 = (float)(i * 2 * Math.PI / teeth);
            float a1 = (float)((i + 0.45) * 2 * Math.PI / teeth);
            float a2 = (float)((i + 0.55) * 2 * Math.PI / teeth);
            float a3 = (float)((i + 1) * 2 * Math.PI / teeth);
            float rOuter = r * 1.05f, rInner = r * 0.72f;
            gear.AddLine(cx + (float)Math.Cos(a0) * rInner, cy + (float)Math.Sin(a0) * rInner, cx + (float)Math.Cos(a1) * rOuter, cy + (float)Math.Sin(a1) * rOuter);
            gear.AddLine(cx + (float)Math.Cos(a1) * rOuter, cy + (float)Math.Sin(a1) * rOuter, cx + (float)Math.Cos(a2) * rOuter, cy + (float)Math.Sin(a2) * rOuter);
            gear.AddLine(cx + (float)Math.Cos(a2) * rOuter, cy + (float)Math.Sin(a2) * rOuter, cx + (float)Math.Cos(a3) * rInner, cy + (float)Math.Sin(a3) * rInner);
        }
        gear.CloseFigure();
        FillMetal(g, gear, pal, 0.3f, 0.25f);
        using (var hole = Circle(cx, cy, r * 0.38f))
        using (var br = new SolidBrush(Color.Black))
            g.FillEllipse(br, cx - r * 0.38f, cy - r * 0.38f, r * 0.76f, r * 0.76f);
        centerDraw(g, cx, cy, r * 0.32f);
        gear.Dispose();
    }

    static Color Lerp(Color a, Color b, float t)
    {
        return Color.FromArgb(255,
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    public static void DrawClassEmblem(Graphics g, ClassPalette pal, float cx, float cy, float s)
    {
        switch (pal.Id)
        {
            case "TheWarrior": DrawShieldBack(g, cx, cy, s * 0.55f, s * 0.65f, pal); break;
            case "TheHunter": DrawBow(g, cx, cy, s * 0.7f, pal); break;
            case "TheRogue": DrawDagger(g, cx - s * 0.15f, cy, s * 0.9f, -25, pal); DrawDagger(g, cx + s * 0.15f, cy, s * 0.9f, 25, pal); break;
            case "ThePriest": DrawHolyCross(g, cx, cy, s * 0.9f, pal); break;
            case "TheShaman": DrawTotem(g, cx, cy, s * 0.85f, pal); break;
            case "TheMage": DrawStaff(g, cx, cy, s * 0.95f, pal, true); break;
            case "TheWarlock": DrawSkull(g, cx, cy, s * 0.42f, pal); break;
            case "TheMonk": DrawFist(g, cx, cy, s * 0.85f, pal); break;
            case "TheDruid": DrawPaw(g, cx, cy - s * 0.05f, s * 0.55f, pal); DrawMoon(g, cx + s * 0.35f, cy - s * 0.35f, s * 0.22f, pal); break;
            case "TheDeathKnight": DrawSkull(g, cx, cy, s * 0.4f, pal); break;
            case "TheEvoker": DrawDragonEye(g, cx, cy, s * 0.5f, pal); break;
        }
    }
}
