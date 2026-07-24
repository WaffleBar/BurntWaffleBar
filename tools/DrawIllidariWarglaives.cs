using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

public static class DrawIllidariWarglaives
{
    const int Size = 1024;

    // Illidari palette
    static readonly Color FelBright = Color.FromArgb(255, 195, 255, 205);
    static readonly Color FelMid = Color.FromArgb(255, 85, 255, 136);
    static readonly Color FelDeep = Color.FromArgb(255, 20, 170, 75);
    static readonly Color FelGlow = Color.FromArgb(120, 100, 255, 150);
    static readonly Color ClockPurple = Color.FromArgb(255, 204, 148, 245);
    static readonly Color ShieldLight = Color.FromArgb(255, 128, 88, 172);
    static readonly Color ShieldDark = Color.FromArgb(255, 44, 26, 70);
    static readonly Color RimSilver = Color.FromArgb(255, 220, 230, 235);

    /// <summary>
    /// Front-view double-ended warglaive: four diagonal crescent tips around a central grip.
    /// This is the Illidan/Azzinoth silhouette — not a sword blade.
    /// </summary>
    static GraphicsPath WarglaiveFront(float cx, float cy, float armLen, float armWidth)
    {
        float pinch = armLen * 0.14f;
        float bulge = armWidth;

        PointF nw = new PointF(cx - armLen, cy - pinch);
        PointF ne = new PointF(cx + pinch, cy - armLen);
        PointF se = new PointF(cx + armLen, cy + pinch);
        PointF sw = new PointF(cx - pinch, cy + armLen);

        var path = new GraphicsPath();
        path.StartFigure();
        path.AddBezier(
            nw,
            new PointF(cx - armLen * 0.55f, cy - armLen * 0.55f - bulge),
            new PointF(cx - armLen * 0.10f - bulge, cy - armLen * 0.55f),
            ne);
        path.AddBezier(
            ne,
            new PointF(cx + armLen * 0.55f + bulge, cy - armLen * 0.55f),
            new PointF(cx + armLen * 0.55f, cy - armLen * 0.10f - bulge),
            se);
        path.AddBezier(
            se,
            new PointF(cx + armLen * 0.55f, cy + armLen * 0.55f + bulge),
            new PointF(cx + armLen * 0.10f + bulge, cy + armLen * 0.55f),
            sw);
        path.AddBezier(
            sw,
            new PointF(cx - armLen * 0.55f - bulge, cy + armLen * 0.55f),
            new PointF(cx - armLen * 0.55f, cy + armLen * 0.10f + bulge),
            nw);
        path.CloseFigure();
        return path;
    }

    static void FillPathWithFelMetal(Graphics g, GraphicsPath path, float lightX, float lightY)
    {
        RectangleF bounds = path.GetBounds();
        using (var shadow = (GraphicsPath)path.Clone())
        using (var matrix = new Matrix())
        {
            matrix.Translate(6f, 8f);
            shadow.Transform(matrix);
            using (var shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                g.FillPath(shadowBrush, shadow);
        }

        using (var glowPen = new Pen(FelGlow, 18f) { LineJoin = LineJoin.Round })
            g.DrawPath(glowPen, path);

        using (var body = new PathGradientBrush(path))
        {
            body.CenterPoint = new PointF(bounds.Left + bounds.Width * lightX, bounds.Top + bounds.Height * lightY);
            body.CenterColor = FelBright;
            body.SurroundColors = new[] { FelDeep };
            body.FocusScales = new PointF(0.35f, 0.35f);
            g.FillPath(body, path);
        }

        using (var sheen = new LinearGradientBrush(
            bounds,
            Color.FromArgb(160, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.ForwardDiagonal))
        {
            sheen.SetBlendTriangularShape(0.18f, 1f);
            g.FillPath(sheen, path);
        }

        using (var rim = new Pen(RimSilver, 2.2f) { LineJoin = LineJoin.Round })
            g.DrawPath(rim, path);

        using (var edge = new Pen(Color.FromArgb(180, 40, 120, 60), 1.2f) { LineJoin = LineJoin.Round })
            g.DrawPath(edge, path);
    }

    static void DrawGripHub(Graphics g, float cx, float cy, float radius)
    {
        var rect = new RectangleF(cx - radius, cy - radius, radius * 2f, radius * 2f);
        using (var brush = new LinearGradientBrush(rect, FelBright, FelDeep, LinearGradientMode.ForwardDiagonal))
            g.FillEllipse(brush, rect);
        using (var rim = new Pen(RimSilver, 1.8f))
            g.DrawEllipse(rim, rect);
    }

    static void DrawWarglaive(Graphics g, float cx, float cy, float scale, float rotDeg)
    {
        var state = g.Save();
        g.TranslateTransform(cx, cy);
        g.RotateTransform(rotDeg);
        g.ScaleTransform(scale, scale);

        using (var path = WarglaiveFront(0f, 0f, 118f, 28f))
        {
            FillPathWithFelMetal(g, path, 0.28f, 0.22f);
        }

        DrawGripHub(g, 0f, 0f, 14f);
        g.Restore(state);
    }

    static void DrawShield(Graphics g, float cx, float cy, float radius)
    {
        var outer = new RectangleF(cx - radius, cy - radius, radius * 2f, radius * 2f);
        var inner = new RectangleF(cx - radius * 0.82f, cy - radius * 0.82f, radius * 1.64f, radius * 1.64f);

        using (var drop = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            g.FillEllipse(drop, outer.X + 8, outer.Y + 12, outer.Width, outer.Height);

        using (var brush = new LinearGradientBrush(
            outer,
            ShieldLight,
            ShieldDark,
            LinearGradientMode.ForwardDiagonal))
            g.FillEllipse(brush, outer);

        using (var innerBrush = new LinearGradientBrush(
            inner,
            Color.FromArgb(255, 96, 62, 140),
            Color.FromArgb(255, 32, 18, 58),
            LinearGradientMode.Vertical))
            g.FillEllipse(innerBrush, inner);

        using (var rim = new Pen(ClockPurple, 8f))
            g.DrawEllipse(rim, outer.X + 10, outer.Y + 10, outer.Width - 20, outer.Height - 20);

        using (var gleam = new LinearGradientBrush(
            outer,
            Color.FromArgb(90, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.ForwardDiagonal))
        {
            gleam.SetBlendTriangularShape(0.12f, 1f);
            g.FillEllipse(gleam, outer);
        }

        // Subtle crack lines
        using (var crack = new Pen(Color.FromArgb(55, 20, 10, 35), 2f))
        {
            g.DrawLine(crack, cx - radius * 0.35f, cy - radius * 0.15f, cx + radius * 0.1f, cy + radius * 0.25f);
            g.DrawLine(crack, cx + radius * 0.2f, cy - radius * 0.35f, cx - radius * 0.05f, cy + radius * 0.05f);
        }
    }

    public static Bitmap Render()
    {
        var bmp = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Black);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float cx = Size * 0.5f;
            float cy = Size * 0.52f;

            DrawShield(g, cx, cy, Size * 0.36f);
            DrawWarglaive(g, cx, cy, 1.55f, -45f);
            DrawWarglaive(g, cx, cy, 1.55f, 45f);
        }

        return bmp;
    }

    public static void Save(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!String.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        using (var bmp = Render())
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }

    public static void Main(string[] args)
    {
        string outPath = args.Length > 0
            ? args[0]
            : @"c:\Users\arkti\.cursor\projects\BurntWaffleBar\tools\warglaive_test.png";
        Save(outPath);
        Console.WriteLine("Saved " + outPath);
    }
}
