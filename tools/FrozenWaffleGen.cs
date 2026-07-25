using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class FrozenWaffleGen
{
    const int CanvasSize = 128;
    const int OutputSize = 256;
    const int RenderScale = 16;
    const int RenderSize = CanvasSize * RenderScale;
    const float PlateInset = 5f;
    const float PlateRadius = 26f;

    static readonly Color Clear = Color.FromArgb(0, 0, 0, 0);

    struct FrostPalette
    {
        public Color Top;
        public Color Mid;
        public Color Bottom;
        public Color Glow;
        public Color BackShape;
    }

    static readonly Dictionary<string, FrostPalette> Palettes = new Dictionary<string, FrostPalette>
    {
        { "Collections", new FrostPalette { Top = Color.FromArgb(255, 198, 232, 255), Mid = Color.FromArgb(255, 118, 188, 245), Bottom = Color.FromArgb(255, 62, 142, 220), Glow = Color.FromArgb(255, 230, 248, 255), BackShape = Color.FromArgb(210, 80, 170, 240) } },
        { "PVP", new FrostPalette { Top = Color.FromArgb(255, 215, 198, 255), Mid = Color.FromArgb(255, 168, 138, 235), Bottom = Color.FromArgb(255, 108, 78, 195), Glow = Color.FromArgb(255, 240, 225, 255), BackShape = Color.FromArgb(210, 140, 100, 220) } },
        { "AdventureGuide", new FrostPalette { Top = Color.FromArgb(255, 185, 245, 245), Mid = Color.FromArgb(255, 95, 210, 215), Bottom = Color.FromArgb(255, 42, 165, 175), Glow = Color.FromArgb(255, 225, 252, 252), BackShape = Color.FromArgb(210, 70, 190, 200) } },
        { "Housing", new FrostPalette { Top = Color.FromArgb(255, 195, 228, 255), Mid = Color.FromArgb(255, 108, 178, 242), Bottom = Color.FromArgb(255, 52, 128, 210), Glow = Color.FromArgb(255, 232, 246, 255), BackShape = Color.FromArgb(210, 88, 158, 230) } },
        { "GroupFinder", new FrostPalette { Top = Color.FromArgb(255, 190, 225, 255), Mid = Color.FromArgb(255, 102, 172, 238), Bottom = Color.FromArgb(255, 48, 122, 205), Glow = Color.FromArgb(255, 228, 244, 255), BackShape = Color.FromArgb(210, 82, 152, 225) } },
        { "QuestTracker", new FrostPalette { Top = Color.FromArgb(255, 200, 235, 255), Mid = Color.FromArgb(255, 112, 182, 240), Bottom = Color.FromArgb(255, 55, 135, 215), Glow = Color.FromArgb(255, 235, 248, 255), BackShape = Color.FromArgb(210, 90, 162, 232) } },
        { "AchievementTracker", new FrostPalette { Top = Color.FromArgb(255, 205, 238, 255), Mid = Color.FromArgb(255, 118, 188, 245), Bottom = Color.FromArgb(255, 58, 142, 218), Glow = Color.FromArgb(255, 238, 250, 255), BackShape = Color.FromArgb(210, 95, 168, 235) } },
        { "Professions", new FrostPalette { Top = Color.FromArgb(255, 198, 228, 255), Mid = Color.FromArgb(255, 108, 178, 242), Bottom = Color.FromArgb(255, 52, 128, 210), Glow = Color.FromArgb(255, 232, 246, 255), BackShape = Color.FromArgb(210, 88, 158, 230) } },
        { "Talents", new FrostPalette { Top = Color.FromArgb(255, 192, 230, 255), Mid = Color.FromArgb(255, 105, 175, 238), Bottom = Color.FromArgb(255, 50, 130, 212), Glow = Color.FromArgb(255, 230, 246, 255), BackShape = Color.FromArgb(210, 85, 155, 228) } },
        { "Character", new FrostPalette { Top = Color.FromArgb(255, 198, 232, 255), Mid = Color.FromArgb(255, 115, 185, 242), Bottom = Color.FromArgb(255, 58, 138, 215), Glow = Color.FromArgb(255, 232, 248, 255), BackShape = Color.FromArgb(210, 88, 160, 230) } },
        { "Guild", new FrostPalette { Top = Color.FromArgb(255, 202, 235, 255), Mid = Color.FromArgb(255, 110, 180, 240), Bottom = Color.FromArgb(255, 52, 132, 212), Glow = Color.FromArgb(255, 235, 248, 255), BackShape = Color.FromArgb(210, 92, 158, 232) } },
        { "Social", new FrostPalette { Top = Color.FromArgb(255, 208, 238, 255), Mid = Color.FromArgb(255, 122, 192, 245), Bottom = Color.FromArgb(255, 62, 148, 218), Glow = Color.FromArgb(255, 240, 250, 255), BackShape = Color.FromArgb(210, 98, 168, 238) } },
        { "GameMenu", new FrostPalette { Top = Color.FromArgb(255, 188, 225, 255), Mid = Color.FromArgb(255, 100, 168, 235), Bottom = Color.FromArgb(255, 46, 125, 208), Glow = Color.FromArgb(255, 226, 242, 255), BackShape = Color.FromArgb(210, 80, 148, 225) } },
    };

    class IconComposer
    {
        readonly List<GraphicsPath> _fills = new List<GraphicsPath>();
        readonly List<GraphicsPath> _cuts = new List<GraphicsPath>();

        public void Fill(GraphicsPath path) { _fills.Add(path); }

        public void Cut(float cx, float cy, float rx, float ry)
        {
            var path = new GraphicsPath();
            path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
            _cuts.Add(path);
        }

        public void CutPath(GraphicsPath path) { _cuts.Add(path); }

        public void ApplyCuts(Graphics g)
        {
            CompositingMode prev = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceCopy;
            using (var brush = new SolidBrush(Clear))
            {
                foreach (GraphicsPath cut in _cuts)
                {
                    g.FillPath(brush, cut);
                }
            }
            g.CompositingMode = prev;
        }

        public GraphicsPath BuildCombined()
        {
            var combined = new GraphicsPath();
            foreach (GraphicsPath fill in _fills)
            {
                combined.AddPath(fill, false);
            }
            return combined;
        }

        public void DisposePaths()
        {
            foreach (GraphicsPath p in _fills) p.Dispose();
            foreach (GraphicsPath p in _cuts) p.Dispose();
            _fills.Clear();
            _cuts.Clear();
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

    static RectangleF GetPlateBounds()
    {
        return new RectangleF(PlateInset, PlateInset, CanvasSize - PlateInset * 2, CanvasSize - PlateInset * 2);
    }

    static LinearGradientBrush MakeBackdropBrush(RectangleF bounds, FrostPalette palette)
    {
        var brush = new LinearGradientBrush(
            new PointF(bounds.Left, bounds.Top),
            new PointF(bounds.Left, bounds.Bottom),
            palette.Top,
            palette.Bottom);
        var blend = new ColorBlend(5);
        blend.Colors = new[]
        {
            palette.Top,
            Color.FromArgb(255,
                (palette.Top.R + palette.Mid.R) / 2,
                (palette.Top.G + palette.Mid.G) / 2,
                (palette.Top.B + palette.Mid.B) / 2),
            palette.Mid,
            palette.Bottom,
            Color.FromArgb(255, palette.Bottom.R - 6, palette.Bottom.G - 6, palette.Bottom.B - 4),
        };
        blend.Positions = new[] { 0f, 0.22f, 0.50f, 0.78f, 1f };
        brush.InterpolationColors = blend;
        return brush;
    }

    static void DrawPlateShadow(Graphics g, RectangleF plate)
    {
        using (var shadowPath = RoundRect(plate.X + 0.8f, plate.Y + 1.8f, plate.Width, plate.Height, PlateRadius))
        using (var shadowBrush = new SolidBrush(Color.FromArgb(42, 16, 32, 58)))
        {
            g.FillPath(shadowBrush, shadowPath);
        }
    }

    static void DrawBackdropGlow(Graphics g, RectangleF plate, FrostPalette palette)
    {
        float cx = plate.X + plate.Width * 0.68f;
        float cy = plate.Y + plate.Height * 0.30f;
        float r = plate.Width * 0.30f;
        using (var orb = Circle(cx, cy, r))
        using (var orbBrush = new PathGradientBrush(orb))
        {
            orbBrush.CenterColor = Color.FromArgb(100, palette.Glow);
            orbBrush.SurroundColors = new[] { Color.FromArgb(0, palette.Glow) };
            g.FillPath(orbBrush, orb);
        }
    }

    static void DrawFrostLayers(Graphics g, RectangleF plate)
    {
        using (var platePath = RoundRect(plate.X, plate.Y, plate.Width, plate.Height, PlateRadius))
        {
            GraphicsState clip = g.Save();
            g.SetClip(platePath);

            float bandY = plate.Y + plate.Height * 0.46f;
            using (var band = new LinearGradientBrush(
                new RectangleF(plate.X, bandY - 8f, plate.Width, 16f),
                Color.FromArgb(0, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.Vertical))
            {
                var blend = new ColorBlend(5);
                blend.Colors = new[]
                {
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb(72, 255, 255, 255),
                    Color.FromArgb(95, 255, 255, 255),
                    Color.FromArgb(72, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                };
                blend.Positions = new[] { 0f, 0.35f, 0.5f, 0.65f, 1f };
                band.InterpolationColors = blend;
                g.FillRectangle(band, plate.X, bandY - 8f, plate.Width, 16f);
            }

            using (var topFrost = new LinearGradientBrush(
                new RectangleF(plate.X, plate.Y, plate.Width, plate.Height * 0.50f),
                Color.FromArgb(78, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(topFrost, plate.X, plate.Y, plate.Width, plate.Height * 0.50f);
            }

            using (var bottomDepth = new LinearGradientBrush(
                new RectangleF(plate.X, plate.Y + plate.Height * 0.62f, plate.Width, plate.Height * 0.38f),
                Color.FromArgb(0, 255, 255, 255),
                Color.FromArgb(38, 255, 255, 255),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(bottomDepth, plate.X, plate.Y + plate.Height * 0.62f, plate.Width, plate.Height * 0.38f);
            }

            using (var sideSheen = new LinearGradientBrush(
                new PointF(plate.Left, plate.Top),
                new PointF(plate.Right, plate.Bottom),
                Color.FromArgb(42, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255)))
            {
                g.FillRectangle(sideSheen, plate);
            }

            g.Restore(clip);
        }
    }

    static void DrawGlassPolish(Graphics g, RectangleF plate)
    {
        using (var platePath = RoundRect(plate.X, plate.Y, plate.Width, plate.Height, PlateRadius))
        {
            using (var edge = new Pen(Color.FromArgb(225, 255, 255, 255), 1.45f))
            {
                edge.LineJoin = LineJoin.Round;
                g.DrawPath(edge, platePath);
            }

            using (var inner = new Pen(Color.FromArgb(55, 255, 255, 255), 0.75f))
            {
                inner.LineJoin = LineJoin.Round;
                using (var inset = RoundRect(plate.X + 1.1f, plate.Y + 1.1f, plate.Width - 2.2f, plate.Height - 2.2f, PlateRadius - 1f))
                {
                    g.DrawPath(inner, inset);
                }
            }
        }

        DrawCornerGlint(g, plate.X + 7f, plate.Y + 7f, 5f);
        DrawCornerGlint(g, plate.Right - 10f, plate.Y + 9f, 3.5f);
    }

    static void DrawCornerGlint(Graphics g, float cx, float cy, float size)
    {
        using (var h = new Pen(Color.FromArgb(180, 255, 255, 255), 1.1f))
        using (var v = new Pen(Color.FromArgb(180, 255, 255, 255), 1.1f))
        {
            g.DrawLine(h, cx - size, cy, cx + size * 0.4f, cy);
            g.DrawLine(v, cx, cy - size, cx, cy + size * 0.4f);
        }
    }

    static void DrawColoredBackShape(Graphics g, IconComposer icon, FrostPalette palette)
    {
        using (GraphicsPath combined = icon.BuildCombined())
        using (var matrix = new Matrix())
        {
            if (combined.PointCount == 0) return;

            matrix.Translate(1.2f, 2.0f);
            matrix.Scale(1.06f, 1.06f, MatrixOrder.Append);
            combined.Transform(matrix);

            using (var brush = new SolidBrush(palette.BackShape))
            {
                g.FillPath(brush, combined);
            }
        }
    }

    static void DrawCrispSymbol(Graphics g, IconComposer icon)
    {
        using (GraphicsPath combined = icon.BuildCombined())
        {
            if (combined.PointCount == 0) return;

            using (var shadow = (GraphicsPath)combined.Clone())
            using (var matrix = new Matrix())
            {
                matrix.Translate(0.45f, 0.75f);
                shadow.Transform(matrix);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(38, 20, 40, 70)))
                {
                    g.FillPath(shadowBrush, shadow);
                }
            }

            using (var fill = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
            {
                g.FillPath(fill, combined);
            }
        }

        icon.ApplyCuts(g);
    }

    static Bitmap RenderGlassIcon(string iconId, Action<IconComposer> buildSymbol)
    {
        FrostPalette palette = Palettes[iconId];
        RectangleF plate = GetPlateBounds();
        var bitmap = new Bitmap(RenderSize, RenderSize, PixelFormat.Format32bppArgb);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Clear);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.ScaleTransform(RenderScale, RenderScale);

            var icon = new IconComposer();
            buildSymbol(icon);

            DrawPlateShadow(g, plate);

            using (var platePath = RoundRect(plate.X, plate.Y, plate.Width, plate.Height, PlateRadius))
            using (var backdrop = MakeBackdropBrush(plate, palette))
            {
                g.FillPath(backdrop, platePath);
            }

            DrawBackdropGlow(g, plate, palette);
            DrawColoredBackShape(g, icon, palette);
            DrawFrostLayers(g, plate);
            DrawCrispSymbol(g, icon);
            DrawGlassPolish(g, plate);

            icon.DisposePaths();
        }

        return Downscale(bitmap, OutputSize);
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
        source.Dispose();
        return target;
    }

    // WoW retail micro menu silhouettes

    static void BuildCollections(IconComposer icon)
    {
        icon.Fill(Ellipse(64, 87, 20, 15));
        icon.Fill(Ellipse(40, 63, 9.5f, 9.5f));
        icon.Fill(Ellipse(54, 51, 9.5f, 9.5f));
        icon.Fill(Ellipse(74, 51, 9.5f, 9.5f));
        icon.Fill(Ellipse(88, 63, 9.5f, 9.5f));
    }

    static void BuildPVP(IconComposer icon)
    {
        var skull = new GraphicsPath();
        skull.AddBezier(64, 30, 88, 32, 92, 52, 92, 62);
        skull.AddBezier(92, 62, 92, 82, 78, 92, 64, 92);
        skull.AddBezier(64, 92, 50, 92, 36, 82, 36, 62);
        skull.AddBezier(36, 62, 36, 52, 40, 32, 64, 30);
        skull.CloseFigure();
        icon.Fill(skull);
        icon.Cut(52, 54, 5.5f, 7f);
        icon.Cut(76, 54, 5.5f, 7f);
        icon.Cut(64, 74, 9f, 5.5f);
    }

    static void BuildAdventureGuide(IconComposer icon)
    {
        icon.Fill(Circle(64, 64, 40));
        for (int i = 0; i < 12; i++)
        {
            double angle = i * Math.PI / 6 - Math.PI / 2;
            float radius = (i % 3 == 0) ? 35.5f : 36.5f;
            float dot = (i % 3 == 0) ? 2.8f : 1.8f;
            float x = 64 + (float)Math.Cos(angle) * radius;
            float y = 64 + (float)Math.Sin(angle) * radius;
            icon.Fill(Circle(x, y, dot));
        }
        var north = new GraphicsPath();
        north.AddPolygon(new[] { new PointF(64, 28), new PointF(72, 56), new PointF(64, 49), new PointF(56, 56) });
        icon.Fill(north);
        var south = new GraphicsPath();
        south.AddPolygon(new[] { new PointF(64, 100), new PointF(69, 78), new PointF(64, 82), new PointF(59, 78) });
        icon.Fill(south);
        icon.Fill(Circle(64, 64, 5f));
    }

    static void BuildHousing(IconComposer icon)
    {
        var house = new GraphicsPath();
        house.AddLine(64, 26, 98, 52);
        house.AddLine(98, 52, 98, 98);
        house.AddLine(98, 98, 30, 98);
        house.AddLine(30, 98, 30, 52);
        house.CloseFigure();
        icon.Fill(house);
        icon.Cut(64, 84, 9f, 12f);
        icon.Fill(Circle(88, 88, 13));
        for (int i = 0; i < 8; i++)
        {
            double angle = i * Math.PI / 4;
            float x = 88 + (float)Math.Cos(angle) * 9.5f;
            float y = 88 + (float)Math.Sin(angle) * 9.5f;
            icon.Fill(RoundRect(x - 2.2f, y - 2.2f, 4.4f, 4.4f, 1f));
        }
        icon.Cut(88, 88, 4.5f, 4.5f);
    }

    static void BuildGroupFinder(IconComposer icon)
    {
        var shield = new GraphicsPath();
        shield.AddBezier(64, 22, 94, 28, 96, 58, 96, 72);
        shield.AddBezier(96, 72, 96, 88, 80, 102, 64, 106);
        shield.AddBezier(64, 106, 48, 102, 32, 88, 32, 72);
        shield.AddBezier(32, 72, 32, 58, 34, 28, 64, 22);
        shield.CloseFigure();
        icon.Fill(shield);
    }

    static void BuildQuestTracker(IconComposer icon)
    {
        icon.Fill(Circle(64, 64, 34));
        icon.Fill(RoundRect(60.5f, 42, 7, 26, 2f));
        icon.Cut(64, 77, 5.5f, 5.5f);
    }

    static void BuildAchievementTracker(IconComposer icon)
    {
        icon.Fill(Circle(64, 64, 32));
        icon.Cut(64, 64, 19, 19);
        icon.Fill(Circle(64, 64, 7));
        var arrow = new GraphicsPath();
        arrow.AddLine(94, 34, 108, 48);
        arrow.AddLine(108, 48, 100, 48);
        arrow.AddLine(100, 48, 100, 66);
        arrow.AddLine(100, 66, 88, 66);
        arrow.AddLine(88, 66, 88, 48);
        arrow.AddLine(88, 48, 80, 48);
        arrow.CloseFigure();
        icon.Fill(arrow);
    }

    static void BuildProfessions(IconComposer icon)
    {
        icon.Fill(RoundRect(28, 72, 72, 16, 3f));
        icon.Fill(RoundRect(36, 82, 56, 20, 4f));
        icon.Fill(RoundRect(22, 80, 14, 10, 2f));
        icon.Fill(RoundRect(78, 32, 28, 14, 3f));
        icon.Fill(RoundRect(88, 44, 8, 36, 2f));
    }

    static void BuildTalents(IconComposer icon)
    {
        var left = new GraphicsPath();
        left.AddBezier(36, 38, 24, 64, 36, 98, 52, 98);
        left.AddLine(52, 98, 52, 38);
        left.CloseFigure();
        icon.Fill(left);
        var right = new GraphicsPath();
        right.AddBezier(92, 38, 104, 64, 92, 98, 76, 98);
        right.AddLine(76, 98, 76, 38);
        right.CloseFigure();
        icon.Fill(right);
        icon.Fill(RoundRect(50, 36, 28, 9, 3f));
    }

    static void BuildCharacter(IconComposer icon)
    {
        var helm = new GraphicsPath();
        helm.AddBezier(64, 22, 96, 24, 98, 48, 96, 62);
        helm.AddLine(96, 62, 96, 84);
        helm.AddLine(96, 84, 32, 84);
        helm.AddLine(32, 84, 32, 62);
        helm.AddBezier(32, 62, 30, 48, 32, 24, 64, 22);
        helm.CloseFigure();
        icon.Fill(helm);
        icon.Fill(RoundRect(32, 84, 64, 7, 2f));
        icon.Cut(50, 54, 6f, 9f);
        icon.Cut(78, 54, 6f, 9f);
        icon.Cut(64, 68, 10f, 4f);
    }

    static void BuildGuild(IconComposer icon)
    {
        icon.Fill(RoundRect(30, 54, 16, 44, 2f));
        icon.Fill(RoundRect(82, 54, 16, 44, 2f));
        icon.Fill(RoundRect(46, 66, 36, 32, 3f));
        icon.Fill(RoundRect(40, 46, 10, 12, 2f));
        icon.Fill(RoundRect(78, 46, 10, 12, 2f));
        for (int i = 0; i < 5; i++)
        {
            icon.Fill(RoundRect(48 + i * 8.5f, 38, 5.5f, 10, 1.5f));
        }
    }

    static void BuildSocial(IconComposer icon)
    {
        icon.Fill(Circle(46, 50, 14));
        icon.Fill(RoundRect(28, 68, 36, 26, 8f));
        icon.Fill(Circle(82, 50, 14));
        icon.Fill(RoundRect(64, 68, 36, 26, 8f));
    }

    static void BuildGameMenu(IconComposer icon)
    {
        icon.Fill(Capsule(28, 38, 72, 9));
        icon.Fill(Capsule(28, 59.5f, 72, 9));
        icon.Fill(Capsule(28, 81, 72, 9));
    }

    public static void GenerateProfessions(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        using (var bmp = RenderGlassIcon("Professions", BuildProfessions))
        {
            bmp.Save(Path.Combine(outputDir, "Professions.png"), ImageFormat.Png);
        }
    }

    public static void GenerateAll(string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var icons = new Dictionary<string, Action<IconComposer>>
        {
            { "Collections", BuildCollections },
            { "PVP", BuildPVP },
            { "AdventureGuide", BuildAdventureGuide },
            { "Housing", BuildHousing },
            { "GroupFinder", BuildGroupFinder },
            { "QuestTracker", BuildQuestTracker },
            { "AchievementTracker", BuildAchievementTracker },
            { "Professions", BuildProfessions },
            { "Talents", BuildTalents },
            { "Character", BuildCharacter },
            { "Guild", BuildGuild },
            { "Social", BuildSocial },
            { "GameMenu", BuildGameMenu },
        };

        foreach (var entry in icons)
        {
            using (var bmp = RenderGlassIcon(entry.Key, entry.Value))
            {
                bmp.Save(Path.Combine(outputDir, entry.Key + ".png"), ImageFormat.Png);
            }
        }
    }
}
