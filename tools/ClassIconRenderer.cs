using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public static class ClassIconRenderer
{
    const float C = ClassIconCore.Canvas * 0.5f;

    public static Bitmap Render(ClassPalette pal, string iconName)
    {
        var bmp = new Bitmap(ClassIconCore.Canvas, ClassIconCore.Canvas);
        using (var g = Graphics.FromImage(bmp))
        {
            ClassIconCore.SetupGraphics(g);
            switch (iconName)
            {
                case "Collections": DrawCollections(g, pal); break;
                case "PVP": DrawPVP(g, pal); break;
                case "AdventureGuide": DrawAdventureGuide(g, pal); break;
                case "Housing": DrawHousing(g, pal); break;
                case "GroupFinder": DrawGroupFinder(g, pal); break;
                case "QuestTracker": DrawQuestTracker(g, pal); break;
                case "AchievementTracker": DrawAchievementTracker(g, pal); break;
                case "Professions": DrawProfessions(g, pal); break;
                case "Talents": DrawTalents(g, pal); break;
                case "Character": DrawCharacter(g, pal); break;
                case "Guild": DrawGuild(g, pal); break;
                case "Social": DrawSocial(g, pal); break;
                case "GameMenu": DrawGameMenu(g, pal); break;
                default: ClassIconCore.DrawClassEmblem(g, pal, C, C, 280); break;
            }
        }
        return bmp;
    }

    static void DrawCollections(Graphics g, ClassPalette pal)
    {
        using (var frame = ClassIconCore.Horseshoe(C, C + 40, 340, 220))
            ClassIconCore.FillMetal(g, frame, pal, 0.28f, 0.22f);
        ClassIconCore.DrawClassEmblem(g, pal, C, C - 30, 200);
    }

    static void DrawPVP(Graphics g, ClassPalette pal)
    {
        ClassIconCore.DrawShieldBack(g, C, C + 20, 260, 300, pal);
        switch (pal.Id)
        {
            case "TheWarrior":
                ClassIconCore.DrawSword(g, C - 40, C, 320, -35, pal);
                ClassIconCore.DrawSword(g, C + 40, C, 320, 35, pal);
                break;
            case "TheHunter":
                ClassIconCore.DrawBow(g, C, C - 20, 300, pal);
                break;
            case "TheRogue":
                ClassIconCore.DrawDagger(g, C - 30, C, 300, -40, pal);
                ClassIconCore.DrawDagger(g, C + 30, C, 300, 40, pal);
                break;
            case "ThePriest":
                ClassIconCore.DrawStaff(g, C, C, 340, pal, true);
                ClassIconCore.DrawHolyCross(g, C, C - 80, 120, pal);
                break;
            case "TheShaman":
                ClassIconCore.DrawTotem(g, C, C + 20, 280, pal);
                break;
            case "TheMage":
                ClassIconCore.DrawStaff(g, C, C, 360, pal, true);
                break;
            case "TheWarlock":
                ClassIconCore.DrawSkull(g, C, C - 20, 100, pal);
                using (var flame = ClassIconCore.Circle(C, C + 80, 70))
                    ClassIconCore.FillAccent(g, flame, pal);
                break;
            case "TheMonk":
                ClassIconCore.DrawFist(g, C, C, 260, pal);
                break;
            case "TheDruid":
                ClassIconCore.DrawPaw(g, C, C + 10, 200, pal);
                ClassIconCore.DrawMoon(g, C + 120, C - 100, 80, pal);
                break;
            case "TheDeathKnight":
                ClassIconCore.DrawRuneBlade(g, C - 35, C, 330, -30, pal);
                ClassIconCore.DrawRuneBlade(g, C + 35, C, 330, 30, pal);
                break;
            case "TheEvoker":
                ClassIconCore.DrawDragonEye(g, C, C - 30, 130, pal);
                var wing = new GraphicsPath();
                wing.AddBezier(new PointF(C - 180, C + 40), new PointF(C - 80, C - 120), new PointF(C + 20, C - 60), new PointF(C + 160, C + 20));
                wing.AddBezier(new PointF(C + 160, C + 20), new PointF(C + 40, C + 80), new PointF(C - 60, C + 100), new PointF(C - 180, C + 40));
                ClassIconCore.FillAccent(g, wing, pal);
                wing.Dispose();
                break;
        }
    }

    static void DrawAdventureGuide(Graphics g, ClassPalette pal)
    {
        using (var book = ClassIconCore.RoundRect(C - 200, C - 240, 400, 480, 30))
            ClassIconCore.FillMetal(g, book, pal, 0.32f, 0.22f);
        using (var page = ClassIconCore.RoundRect(C - 160, C - 200, 320, 400, 16))
            ClassIconCore.FillAccent(g, page, pal);
        ClassIconCore.DrawClassEmblem(g, pal, C, C - 20, 160);
    }

    static void DrawHousing(Graphics g, ClassPalette pal)
    {
        var arch = new GraphicsPath();
        arch.AddArc(C - 260, C - 120, 520, 420, 180, 180);
        arch.AddLine(C - 260, C + 90, C + 260, C + 90);
        arch.CloseFigure();
        ClassIconCore.FillMetal(g, arch, pal, 0.3f, 0.25f);
        using (var door = ClassIconCore.RoundRect(C - 90, C - 20, 180, 220, 20))
            ClassIconCore.FillAccent(g, door, pal);
        ClassIconCore.DrawClassEmblem(g, pal, C, C - 80, 100);
        arch.Dispose();
    }

    static void DrawGroupFinder(Graphics g, ClassPalette pal)
    {
        using (var lens = ClassIconCore.Circle(C + 40, C + 20, 200))
            ClassIconCore.FillMetal(g, lens, pal, 0.35f, 0.25f);
        using (var glass = ClassIconCore.Circle(C + 40, C + 20, 150))
        using (var br = new SolidBrush(Color.FromArgb(140, 20, 30, 45)))
            g.FillEllipse(br, C + 40 - 150, C + 20 - 150, 300, 300);
        ClassIconCore.DrawClassEmblem(g, pal, C + 40, C + 20, 110);
        using (var handle = ClassIconCore.RoundRect(C + 180, C + 150, 180, 36, 18))
            ClassIconCore.FillMetal(g, handle, pal, 0.4f, 0.3f);
    }

    static void DrawQuestTracker(Graphics g, ClassPalette pal)
    {
        using (var scroll = ClassIconCore.RoundRect(C - 220, C - 260, 440, 520, 40))
            ClassIconCore.FillMetal(g, scroll, pal, 0.32f, 0.22f);
        ClassIconCore.DrawExclamation(g, C, C - 20, 220, pal);
    }

    static void DrawAchievementTracker(Graphics g, ClassPalette pal)
    {
        using (var cup = ClassIconCore.Circle(C, C + 60, 180))
            ClassIconCore.FillMetal(g, cup, pal, 0.35f, 0.25f);
        var bowl = new GraphicsPath();
        bowl.AddBezier(new PointF(C - 200, C + 40), new PointF(C - 120, C - 180), new PointF(C + 120, C - 180), new PointF(C + 200, C + 40));
        bowl.AddLine(C + 200, C + 40, C - 200, C + 40);
        ClassIconCore.FillMetal(g, bowl, pal, 0.35f, 0.22f);
        ClassIconCore.DrawClassEmblem(g, pal, C, C - 40, 90);
        bowl.Dispose();
    }

    static void DrawProfessions(Graphics g, ClassPalette pal)
    {
        using (var anvilTop = ClassIconCore.RoundRect(C - 220, C + 20, 440, 80, 16))
            ClassIconCore.FillMetal(g, anvilTop, pal, 0.35f, 0.25f);
        using (var anvilBase = ClassIconCore.RoundRect(C - 160, C + 90, 320, 100, 20))
            ClassIconCore.FillMetal(g, anvilBase, pal, 0.32f, 0.22f);
        using (var horn = ClassIconCore.RoundRect(C - 280, C + 40, 80, 50, 12))
            ClassIconCore.FillMetal(g, horn, pal, 0.35f, 0.25f);
        using (var hammerHead = ClassIconCore.RoundRect(C + 40, C - 220, 160, 70, 14))
            ClassIconCore.FillMetal(g, hammerHead, pal, 0.4f, 0.28f);
        using (var hammerHandle = ClassIconCore.RoundRect(C + 100, C - 150, 40, 280, 10))
            ClassIconCore.FillMetal(g, hammerHandle, pal, 0.32f, 0.22f);
        ClassIconCore.DrawClassEmblem(g, pal, C - 40, C + 10, 100);
    }

    static void DrawTalents(Graphics g, ClassPalette pal)
    {
        using (var book = ClassIconCore.RoundRect(C - 190, C - 220, 380, 440, 28))
            ClassIconCore.FillMetal(g, book, pal, 0.32f, 0.22f);
        var star = new GraphicsPath();
        PointF[] pts = new PointF[8];
        for (int i = 0; i < 8; i++)
        {
            float a = (float)(i * Math.PI / 4 - Math.PI / 2);
            float r = (i % 2 == 0) ? 140f : 60f;
            pts[i] = new PointF(C + (float)Math.Cos(a) * r, C + (float)Math.Sin(a) * r);
        }
        star.AddPolygon(pts);
        ClassIconCore.FillAccent(g, star, pal);
        star.Dispose();
    }

    static void DrawCharacter(Graphics g, ClassPalette pal)
    {
        using (var disc = ClassIconCore.Circle(C, C, 280))
            ClassIconCore.FillMetal(g, disc, pal, 0.32f, 0.22f);
        switch (pal.Id)
        {
            case "TheWarrior":
                using (var helm = ClassIconCore.RoundRect(C - 150, C - 160, 300, 220, 40))
                    ClassIconCore.FillMetal(g, helm, pal, 0.35f, 0.25f);
                using (var visor = ClassIconCore.RoundRect(C - 120, C - 40, 240, 50, 12))
                    ClassIconCore.FillAccent(g, visor, pal);
                break;
            case "TheHunter":
                using (var hood = ClassIconCore.Circle(C, C - 30, 150))
                    ClassIconCore.FillMetal(g, hood, pal, 0.35f, 0.25f);
                ClassIconCore.DrawBow(g, C, C + 80, 140, pal);
                break;
            case "TheRogue":
                using (var mask = ClassIconCore.RoundRect(C - 130, C - 80, 260, 120, 30))
                    ClassIconCore.FillMetal(g, mask, pal, 0.35f, 0.25f);
                using (var eyeL = ClassIconCore.Circle(C - 55, C - 30, 22))
                using (var eyeR = ClassIconCore.Circle(C + 55, C - 30, 22))
                using (var br = new SolidBrush(pal.Glow))
                { g.FillEllipse(br, C - 77, C - 52, 44, 44); g.FillEllipse(br, C + 33, C - 52, 44, 44); }
                break;
            case "ThePriest":
                using (var hood = ClassIconCore.Circle(C, C - 20, 160))
                    ClassIconCore.FillMetal(g, hood, pal, 0.35f, 0.25f);
                ClassIconCore.DrawHolyCross(g, C, C + 40, 100, pal);
                break;
            case "TheShaman":
                using (var helm = ClassIconCore.Circle(C, C - 10, 150))
                    ClassIconCore.FillMetal(g, helm, pal, 0.35f, 0.25f);
                ClassIconCore.DrawTotem(g, C, C + 100, 120, pal);
                break;
            case "TheMage":
                using (var hood = ClassIconCore.Circle(C, C - 20, 155))
                    ClassIconCore.FillMetal(g, hood, pal, 0.35f, 0.25f);
                using (var orb = ClassIconCore.Circle(C, C + 60, 55))
                    ClassIconCore.FillAccent(g, orb, pal);
                break;
            case "TheWarlock":
                ClassIconCore.DrawSkull(g, C, C, 120, pal);
                break;
            case "TheMonk":
                ClassIconCore.DrawFist(g, C, C, 220, pal);
                break;
            case "TheDruid":
                using (var antler = ClassIconCore.Circle(C, C - 10, 140))
                    ClassIconCore.FillMetal(g, antler, pal, 0.35f, 0.25f);
                ClassIconCore.DrawPaw(g, C, C + 60, 120, pal);
                break;
            case "TheDeathKnight":
                using (var helm = ClassIconCore.RoundRect(C - 140, C - 150, 280, 210, 35))
                    ClassIconCore.FillMetal(g, helm, pal, 0.35f, 0.25f);
                ClassIconCore.DrawSkull(g, C, C - 20, 70, pal);
                break;
            case "TheEvoker":
                ClassIconCore.DrawDragonEye(g, C, C, 140, pal);
                break;
        }
    }

    static void DrawGuild(Graphics g, ClassPalette pal)
    {
        using (var bar = ClassIconCore.RoundRect(C - 220, C - 260, 440, 40, 12))
            ClassIconCore.FillMetal(g, bar, pal, 0.4f, 0.3f);
        var banner = new GraphicsPath();
        banner.AddPolygon(new[]
        {
            new PointF(C - 140, C - 220), new PointF(C + 140, C - 220),
            new PointF(C + 120, C + 220), new PointF(C, C + 160), new PointF(C - 120, C + 220),
        });
        ClassIconCore.FillMetal(g, banner, pal, 0.32f, 0.22f);
        ClassIconCore.DrawClassEmblem(g, pal, C, C + 10, 130);
    }

    static void DrawSocial(Graphics g, ClassPalette pal)
    {
        using (var b1 = ClassIconCore.RoundRect(C - 260, C - 120, 280, 200, 40))
            ClassIconCore.FillMetal(g, b1, pal, 0.35f, 0.25f);
        using (var b2 = ClassIconCore.RoundRect(C - 20, C - 40, 280, 200, 40))
            ClassIconCore.FillMetal(g, b2, pal, 0.35f, 0.25f);
        DrawSilhouette(g, pal, C - 120, C - 30, 90);
        DrawSilhouette(g, pal, C + 120, C + 50, 90);
    }

    static void DrawSilhouette(Graphics g, ClassPalette pal, float cx, float cy, float s)
    {
        using (var head = ClassIconCore.Circle(cx, cy - s * 0.35f, s * 0.28f))
        using (var br = new SolidBrush(Color.FromArgb(220, pal.Shadow)))
            g.FillEllipse(br, cx - s * 0.28f, cy - s * 0.63f, s * 0.56f, s * 0.56f);
        using (var body = ClassIconCore.RoundRect(cx - s * 0.35f, cy - s * 0.05f, s * 0.7f, s * 0.75f, s * 0.15f))
        using (var br = new SolidBrush(Color.FromArgb(220, pal.Shadow)))
            g.FillPath(br, body);
        // class hint
        ClassIconCore.DrawClassEmblem(g, pal, cx, cy + s * 0.05f, s * 0.35f);
    }

    static void DrawGameMenu(Graphics g, ClassPalette pal)
    {
        ClassIconCore.DrawGear(g, C, C, 280, pal, (gr, cx, cy, r) =>
        {
            ClassIconCore.DrawClassEmblem(gr, pal, cx, cy, r * 1.4f);
        });
    }
}
