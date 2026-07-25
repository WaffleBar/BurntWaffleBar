using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class MinimalIconGen
{
    const int OutputSize = 128;
    const int RenderScale = 6;
    const int RenderSize = OutputSize * RenderScale;
    const float EdgeWidth = 2.4f;

    static readonly Color Clear = Color.FromArgb(0, 0, 0, 0);
    static readonly Color EdgeOuter = Color.FromArgb(255, 0, 0, 0);
    static readonly Color EdgeInner = Color.FromArgb(255, 18, 18, 22);

    class IconComposer
    {
        readonly List<GraphicsPath> _solids = new List<GraphicsPath>();
        readonly List<GraphicsPath> _cuts = new List<GraphicsPath>();

        public void Solid(GraphicsPath path) { _solids.Add(path); }

        public void Cut(float cx, float cy, float rx, float ry)
        {
            var path = new GraphicsPath();
            path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
            _cuts.Add(path);
        }

        public void CutPath(GraphicsPath path) { _cuts.Add(path); }

        public void Draw(Graphics g)
        {
            foreach (GraphicsPath solid in _solids)
            {
                DrawPremiumGlass(g, solid);
            }

            foreach (GraphicsPath cut in _cuts)
            {
                PunchPath(g, cut);
                cut.Dispose();
            }

            foreach (GraphicsPath solid in _solids)
            {
                solid.Dispose();
            }
        }
    }

    static GraphicsPath Circle(float cx, float cy, float r)
    {
        var path = new GraphicsPath();
        path.AddEllipse(cx - r, cy - r, r * 2, r * 2);
        return path;
    }

    static GraphicsPath Ellipse(float cx, float cy, float rx, float ry)
    {
        var path = new GraphicsPath();
        path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
        return path;
    }

    static GraphicsPath RoundRect(float x, float y, float w, float h, float r)
    {
        var path = new GraphicsPath();
        float d = Math.Min(r * 2, Math.Min(w, h));
        path.AddArc(x, y, d, d, 180, 90);
        path.AddArc(x + w - d, y, d, d, 270, 90);
        path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
        path.AddArc(x, y + h - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    static GraphicsPath Capsule(float x, float y, float w, float h)
    {
        return RoundRect(x, y, w, h, h / 2f);
    }

    static void PunchPath(Graphics g, GraphicsPath path)
    {
        CompositingMode prev = g.CompositingMode;
        g.CompositingMode = CompositingMode.SourceCopy;
        using (var brush = new SolidBrush(Clear))
        {
            g.FillPath(brush, path);
        }
        g.CompositingMode = prev;
    }

    static void DrawPremiumGlass(Graphics g, GraphicsPath path)
    {
        RectangleF bounds = path.GetBounds();
        if (bounds.Width < 0.5f || bounds.Height < 0.5f)
        {
            return;
        }

        RectangleF b = bounds;
        b.Inflate(8f, 8f);
        PointF light = new PointF(bounds.X + bounds.Width * 0.32f, bounds.Y + bounds.Height * 0.22f);

        using (var shadow = (GraphicsPath)path.Clone())
        using (var matrix = new Matrix())
        {
            matrix.Translate(0.9f, 1.4f);
            shadow.Transform(matrix);
            using (var shadowBrush = new SolidBrush(Color.FromArgb(52, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadow);
            }
        }

        using (var outer = new Pen(EdgeOuter, EdgeWidth + 0.8f))
        {
            outer.LineJoin = LineJoin.Round;
            outer.StartCap = LineCap.Round;
            outer.EndCap = LineCap.Round;
            g.DrawPath(outer, path);
        }

        using (var edge = new Pen(EdgeInner, EdgeWidth))
        {
            edge.LineJoin = LineJoin.Round;
            edge.StartCap = LineCap.Round;
            edge.EndCap = LineCap.Round;
            g.DrawPath(edge, path);
        }

        using (var body = new PathGradientBrush(path))
        {
            body.CenterPoint = light;
            body.CenterColor = Color.FromArgb(252, 255, 255, 255);
            body.SurroundColors = new[] { Color.FromArgb(200, 210, 220, 235) };
            body.FocusScales = new PointF(0.62f, 0.55f);
            g.FillPath(body, path);
        }

        using (var vertical = new LinearGradientBrush(b,
            Color.FromArgb(210, 248, 250, 255),
            Color.FromArgb(175, 178, 188, 205),
            LinearGradientMode.Vertical))
        {
            var blend = new ColorBlend(6);
            blend.Colors = new[]
            {
                Color.FromArgb(230, 255, 255, 255),
                Color.FromArgb(215, 240, 245, 252),
                Color.FromArgb(200, 220, 230, 242),
                Color.FromArgb(190, 205, 215, 230),
                Color.FromArgb(180, 188, 198, 215),
                Color.FromArgb(168, 170, 180, 198),
            };
            blend.Positions = new[] { 0f, 0.18f, 0.42f, 0.65f, 0.84f, 1f };
            vertical.InterpolationColors = blend;
            g.FillPath(vertical, path);
        }

        GraphicsState clip = g.Save();
        g.SetClip(path);

        using (var cool = new LinearGradientBrush(b,
            Color.FromArgb(28, 185, 205, 228),
            Color.FromArgb(8, 130, 145, 168),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(cool, b);
        }

        using (var bloom = new LinearGradientBrush(
            new RectangleF(b.X, b.Y, b.Width, b.Height * 0.55f),
            Color.FromArgb(95, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(bloom, b);
        }

        using (var sheen = new LinearGradientBrush(
            new PointF(b.Left - b.Width * 0.15f, b.Top),
            new PointF(b.Right, b.Bottom * 0.65f),
            Color.FromArgb(38, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255)))
        {
            g.FillRectangle(sheen, b);
        }

        float hx = bounds.X + bounds.Width * 0.30f;
        float hy = bounds.Y + bounds.Height * 0.16f;
        float hr = Math.Max(5f, Math.Min(bounds.Width, bounds.Height) * 0.18f);
        using (var hotspot = Ellipse(hx, hy, hr, hr * 0.72f))
        using (var hotspotBrush = new PathGradientBrush(hotspot))
        {
            hotspotBrush.CenterColor = Color.FromArgb(120, 255, 255, 255);
            hotspotBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
            g.FillPath(hotspotBrush, hotspot);
        }

        using (var ao = new LinearGradientBrush(
            new RectangleF(b.X, b.Y + b.Height * 0.68f, b.Width, b.Height * 0.32f),
            Color.FromArgb(0, 0, 0, 0),
            Color.FromArgb(48, 0, 0, 0),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(ao, b);
        }

        using (var rim = new Pen(Color.FromArgb(110, 255, 255, 255), 1.1f))
        {
            rim.LineJoin = LineJoin.Round;
            g.DrawPath(rim, path);
        }

        g.Restore(clip);
    }

    static void DrawCollections(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(Ellipse(64, 87, 20, 15));
        icon.Solid(Ellipse(40, 63, 9.5f, 9.5f));
        icon.Solid(Ellipse(54, 51, 9.5f, 9.5f));
        icon.Solid(Ellipse(74, 51, 9.5f, 9.5f));
        icon.Solid(Ellipse(88, 63, 9.5f, 9.5f));
        icon.Draw(g);
    }

    static void DrawPVP(Graphics g)
    {
        var icon = new IconComposer();
        var skull = new GraphicsPath();
        skull.AddBezier(64, 30, 88, 32, 92, 52, 92, 62);
        skull.AddBezier(92, 62, 92, 82, 78, 92, 64, 92);
        skull.AddBezier(64, 92, 50, 92, 36, 82, 36, 62);
        skull.AddBezier(36, 62, 36, 52, 40, 32, 64, 30);
        skull.CloseFigure();
        icon.Solid(skull);
        icon.Cut(52, 54, 5.5f, 7f);
        icon.Cut(76, 54, 5.5f, 7f);
        icon.Cut(64, 74, 9f, 5.5f);
        icon.Draw(g);
    }

    static void DrawAdventureGuide(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(Circle(64, 64, 40));

        for (int i = 0; i < 12; i++)
        {
            double angle = i * Math.PI / 6 - Math.PI / 2;
            float radius = (i % 3 == 0) ? 35.5f : 36.5f;
            float dot = (i % 3 == 0) ? 2.8f : 1.8f;
            float x = 64 + (float)Math.Cos(angle) * radius;
            float y = 64 + (float)Math.Sin(angle) * radius;
            icon.Solid(Circle(x, y, dot));
        }

        var north = new GraphicsPath();
        north.AddPolygon(new[]
        {
            new PointF(64, 28),
            new PointF(72, 56),
            new PointF(64, 49),
            new PointF(56, 56),
        });
        icon.Solid(north);

        var south = new GraphicsPath();
        south.AddPolygon(new[]
        {
            new PointF(64, 100),
            new PointF(69, 78),
            new PointF(64, 82),
            new PointF(59, 78),
        });
        icon.Solid(south);

        icon.Solid(Circle(64, 64, 5f));
        icon.Draw(g);
    }

    static void DrawHousing(Graphics g)
    {
        var icon = new IconComposer();
        var house = new GraphicsPath();
        house.AddLine(64, 26, 98, 52);
        house.AddLine(98, 52, 98, 98);
        house.AddLine(98, 98, 30, 98);
        house.AddLine(30, 98, 30, 52);
        house.CloseFigure();
        icon.Solid(house);
        icon.Cut(64, 84, 9f, 12f);

        icon.Solid(Circle(88, 88, 13));
        for (int i = 0; i < 8; i++)
        {
            double angle = i * Math.PI / 4;
            float x = 88 + (float)Math.Cos(angle) * 9.5f;
            float y = 88 + (float)Math.Sin(angle) * 9.5f;
            icon.Solid(RoundRect(x - 2.2f, y - 2.2f, 4.4f, 4.4f, 1f));
        }
        icon.Cut(88, 88, 4.5f, 4.5f);
        icon.Draw(g);
    }

    static void DrawGroupFinder(Graphics g)
    {
        var icon = new IconComposer();
        var shield = new GraphicsPath();
        shield.AddBezier(64, 22, 94, 28, 96, 58, 96, 72);
        shield.AddBezier(96, 72, 96, 88, 80, 102, 64, 106);
        shield.AddBezier(64, 106, 48, 102, 32, 88, 32, 72);
        shield.AddBezier(32, 72, 32, 58, 34, 28, 64, 22);
        shield.CloseFigure();
        icon.Solid(shield);
        icon.Draw(g);
    }

    static void DrawQuestTracker(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(Circle(64, 64, 34));
        icon.Solid(RoundRect(60.5f, 42, 7, 26, 2f));
        icon.Cut(64, 77, 5.5f, 5.5f);
        icon.Draw(g);
    }

    static void DrawAchievementTracker(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(Circle(64, 64, 32));
        icon.Cut(64, 64, 19, 19);
        icon.Solid(Circle(64, 64, 7));

        var arrow = new GraphicsPath();
        arrow.AddLine(94, 34, 108, 48);
        arrow.AddLine(108, 48, 100, 48);
        arrow.AddLine(100, 48, 100, 66);
        arrow.AddLine(100, 66, 88, 66);
        arrow.AddLine(88, 66, 88, 48);
        arrow.AddLine(88, 48, 80, 48);
        arrow.CloseFigure();
        icon.Solid(arrow);
        icon.Draw(g);
    }

    static void DrawProfessions(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(RoundRect(28, 72, 72, 16, 3f));
        icon.Solid(RoundRect(36, 82, 56, 20, 4f));
        icon.Solid(RoundRect(22, 80, 14, 10, 2f));
        icon.Solid(RoundRect(78, 32, 28, 14, 3f));
        icon.Solid(RoundRect(88, 44, 8, 36, 2f));
        icon.Draw(g);
    }

    static void DrawTalents(Graphics g)
    {
        var icon = new IconComposer();
        var left = new GraphicsPath();
        left.AddBezier(36, 38, 24, 64, 36, 98, 52, 98);
        left.AddLine(52, 98, 52, 38);
        left.CloseFigure();
        icon.Solid(left);

        var right = new GraphicsPath();
        right.AddBezier(92, 38, 104, 64, 92, 98, 76, 98);
        right.AddLine(76, 98, 76, 38);
        right.CloseFigure();
        icon.Solid(right);

        icon.Solid(RoundRect(50, 36, 28, 9, 3f));
        icon.Draw(g);
    }

    static void DrawCharacter(Graphics g)
    {
        var icon = new IconComposer();
        var helm = new GraphicsPath();
        helm.AddBezier(64, 22, 96, 24, 98, 48, 96, 62);
        helm.AddLine(96, 62, 96, 84);
        helm.AddLine(96, 84, 32, 84);
        helm.AddLine(32, 84, 32, 62);
        helm.AddBezier(32, 62, 30, 48, 32, 24, 64, 22);
        helm.CloseFigure();
        icon.Solid(helm);
        icon.Solid(RoundRect(32, 84, 64, 7, 2f));

        icon.Cut(50, 54, 6f, 9f);
        icon.Cut(78, 54, 6f, 9f);
        icon.Cut(64, 68, 10f, 4f);
        icon.Draw(g);
    }

    static void DrawGuild(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(RoundRect(30, 54, 16, 44, 2f));
        icon.Solid(RoundRect(82, 54, 16, 44, 2f));
        icon.Solid(RoundRect(46, 66, 36, 32, 3f));
        icon.Solid(RoundRect(40, 46, 10, 12, 2f));
        icon.Solid(RoundRect(78, 46, 10, 12, 2f));
        for (int i = 0; i < 5; i++)
        {
            icon.Solid(RoundRect(48 + i * 8.5f, 38, 5.5f, 10, 1.5f));
        }
        icon.Draw(g);
    }

    static void DrawSocial(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(Circle(46, 50, 14));
        icon.Solid(RoundRect(28, 68, 36, 26, 8f));
        icon.Solid(Circle(82, 50, 14));
        icon.Solid(RoundRect(64, 68, 36, 26, 8f));
        icon.Draw(g);
    }

    static void DrawGameMenu(Graphics g)
    {
        var icon = new IconComposer();
        icon.Solid(Capsule(28, 38, 72, 9));
        icon.Solid(Capsule(28, 59.5f, 72, 9));
        icon.Solid(Capsule(28, 81, 72, 9));
        icon.Draw(g);
    }

    static Bitmap Downscale(Bitmap source, int size)
    {
        var target = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(target))
        {
            g.Clear(Clear);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(source, 0, 0, size, size);
        }
        return target;
    }

    static Bitmap RenderHiRes(Action<Graphics> draw)
    {
        var bitmap = new Bitmap(RenderSize, RenderSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Clear);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.ScaleTransform(RenderScale, RenderScale);
            draw(g);
        }

        return Downscale(bitmap, OutputSize);
    }

    static void SavePng(Bitmap bitmap, string path)
    {
        bitmap.Save(path, ImageFormat.Png);
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

    public static void GenerateProfessionsAssets(string themeDir, string sourceDir, string sourcePrefix)
    {
        Directory.CreateDirectory(themeDir);
        if (!string.IsNullOrEmpty(sourceDir))
        {
            Directory.CreateDirectory(sourceDir);
        }

        using (var hiRes = RenderHiRes(DrawProfessions))
        {
            if (!string.IsNullOrEmpty(sourceDir) && !string.IsNullOrEmpty(sourcePrefix))
            {
                SavePng(hiRes, Path.Combine(sourceDir, sourcePrefix + "_Professions.png"));
            }

            SavePng(hiRes, Path.Combine(themeDir, "Professions.png"));
            SaveTga(hiRes, Path.Combine(themeDir, "Professions.tga"));
        }
    }

    public static void GenerateAll(string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        string[] names = {
            "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
            "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
        };

        Action<Graphics>[] drawers = {
            DrawCollections, DrawPVP, DrawAdventureGuide, DrawHousing, DrawGroupFinder, DrawQuestTracker,
            DrawAchievementTracker, DrawProfessions, DrawTalents, DrawCharacter, DrawGuild, DrawSocial, DrawGameMenu,
        };

        for (int i = 0; i < names.Length; i++)
        {
            using (var hiRes = RenderHiRes(drawers[i]))
            {
                string pngPath = Path.Combine(outputDir, names[i] + ".png");
                string tgaPath = Path.Combine(outputDir, names[i] + ".tga");
                SavePng(hiRes, pngPath);
                SaveTga(hiRes, tgaPath);
            }
        }
    }
}
