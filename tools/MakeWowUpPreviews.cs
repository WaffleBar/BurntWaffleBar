using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class MakeWowUpPreviews
{
    const int PreviewWidth = 1536;
    const int PreviewHeight = 1024;
    const int PreviewIconSize = 58;
    const int PreviewSpacing = 4;

    static readonly string[] IconOrder =
    {
        "Collections", "PVP", "AdventureGuide", "Housing", "GroupFinder", "QuestTracker",
        "AchievementTracker", "Professions", "Talents", "Character", "Guild", "Social", "GameMenu",
    };

    static readonly Color BgTop = Color.FromArgb(255, 24, 20, 18);
    static readonly Color BgBottom = Color.FromArgb(255, 10, 8, 8);
    static readonly Color PanelFill = Color.FromArgb(235, 18, 16, 14);
    static readonly Color PanelBorder = Color.FromArgb(255, 72, 58, 36);
    static readonly Color PanelHighlight = Color.FromArgb(255, 120, 92, 48);
    static readonly Color TextPrimary = Color.FromArgb(255, 255, 240, 210);
    static readonly Color TextMuted = Color.FromArgb(255, 170, 150, 120);
    static readonly Color TextGold = Color.FromArgb(255, 255, 209, 0);
    static readonly Color EditModeBorder = Color.FromArgb(255, 255, 214, 0);

    public static void Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string themesRoot = Path.Combine(root, "Media", "Themes");
        string previewsDir = Path.Combine(root, ".previews");
        Directory.CreateDirectory(previewsDir);

        CleanGeneratedPreviews(previewsDir);

        string addonIcon = Path.Combine(root, "Media", "AddonIcon.png");
        if (File.Exists(addonIcon))
        {
            WriteSocialPreview(root, addonIcon);
        }

        RenderInGamePreviews(themesRoot, previewsDir);
        Console.WriteLine("Wrote previews to " + previewsDir);
    }

    static void CleanGeneratedPreviews(string previewsDir)
    {
        foreach (string path in Directory.GetFiles(previewsDir, "*.png"))
        {
            File.Delete(path);
        }
    }

    static void RenderInGamePreviews(string themesRoot, string previewsDir)
    {
        WritePreview(previewsDir, "01-bar-ingame.png", canvas =>
        {
            DrawGameScene(themesRoot, canvas, "ThePaladin", 760f, false, false, null);
        });

        WritePreview(previewsDir, "02-class-themes-ingame.png", canvas =>
        {
            using (var g = Graphics.FromImage(canvas))
            {
                DrawGameBackground(g, PreviewWidth, PreviewHeight);
                DrawPlayerFrame(g, 28f, 24f);
                DrawMinimap(g, PreviewWidth - 210f, 24f);
                DrawActionBarBackdrop(g, PreviewHeight - 250f);

                float startY = 300f;
                string[] themes = { "ThePaladin", "TheIllidari", "TheWarrior" };
                string[] labels = { "The Paladin", "The Illidari", "The Warrior" };

                for (int i = 0; i < themes.Length; i++)
                {
                    float y = startY + i * 150f;
                    DrawLabel(g, labels[i], 120f, y - 28f, 26f, TextGold, FontStyle.Bold);
                    DrawMenuBar(g, Path.Combine(themesRoot, themes[i]), PreviewWidth / 2f, y + 36f, PreviewIconSize, PreviewSpacing, false, false, null);
                }

                DrawCaption(g, "Class icon themes auto-match your character, or pick one manually.");
            }
        });

        WritePreview(previewsDir, "03-edit-mode-ingame.png", canvas =>
        {
            DrawGameScene(themesRoot, canvas, "ThePaladin", 760f, true, false, null);
        });

        WritePreview(previewsDir, "04-settings-ingame.png", canvas =>
        {
            using (var g = Graphics.FromImage(canvas))
            {
                DrawGameBackground(g, PreviewWidth, PreviewHeight);
                DrawPlayerFrame(g, 28f, 24f);
                DrawMinimap(g, PreviewWidth - 210f, 24f);
                DrawSettingsPanel(g);
            }
        });

        WritePreview(previewsDir, "05-clock-queue-ingame.png", canvas =>
        {
            DrawGameScene(themesRoot, canvas, "TheIllidari", 760f, false, true, "2:47 PM");
        });
    }

    static void WritePreview(string previewsDir, string fileName, Action<Bitmap> draw)
    {
        using (var canvas = new Bitmap(PreviewWidth, PreviewHeight, PixelFormat.Format32bppArgb))
        {
            draw(canvas);
            string outputPath = Path.Combine(previewsDir, fileName);
            canvas.Save(outputPath, ImageFormat.Png);
            Console.WriteLine("Wrote " + fileName);
        }
    }

    static void DrawGameScene(string themesRoot, Bitmap canvas, string themeId, float barY, bool editMode, bool showQueueEye, string clockText)
    {
        using (var g = Graphics.FromImage(canvas))
        {
            DrawGameBackground(g, PreviewWidth, PreviewHeight);
            DrawPlayerFrame(g, 28f, 24f);
            DrawMinimap(g, PreviewWidth - 210f, 24f);
            DrawActionBarBackdrop(g, PreviewHeight - 250f);
            DrawMenuBar(g, Path.Combine(themesRoot, themeId), PreviewWidth / 2f, barY, PreviewIconSize, PreviewSpacing, editMode, showQueueEye, clockText);

            if (editMode)
            {
                DrawCaption(g, "Drag the bar in WoW Edit Mode — positions save per layout.");
            }
            else if (showQueueEye)
            {
                DrawCaption(g, "Optional clock above the bar, plus queue-eye positioning support.");
            }
            else
            {
                DrawCaption(g, "Custom micro menu bar with themed icons replacing Blizzard's default menu.");
            }
        }
    }

    static void DrawGameBackground(Graphics g, int width, int height)
    {
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (var bg = new LinearGradientBrush(new Rectangle(0, 0, width, height), BgTop, BgBottom, 90f))
        {
            g.FillRectangle(bg, 0, 0, width, height);
        }

        using (var vignette = new GraphicsPath())
        {
            vignette.AddEllipse(-width * 0.15f, height * 0.15f, width * 1.3f, height * 0.95f);
            using (var brush = new PathGradientBrush(vignette))
            {
                brush.CenterColor = Color.FromArgb(0, 0, 0, 0);
                brush.SurroundColors = new[] { Color.FromArgb(180, 0, 0, 0) };
                g.FillRectangle(brush, 0, 0, width, height);
            }
        }

        using (var pen = new Pen(Color.FromArgb(28, 255, 255, 255), 1f))
        {
            for (int y = 120; y < height; y += 48)
            {
                g.DrawLine(pen, 0, y, width, y + 40);
            }
        }
    }

    static void DrawPlayerFrame(Graphics g, float x, float y)
    {
        var frame = new RectangleF(x, y, 220f, 56f);
        using (var path = RoundedRect(frame, 8f))
        using (var fill = new SolidBrush(Color.FromArgb(210, 12, 10, 10)))
        using (var border = new Pen(Color.FromArgb(255, 58, 48, 36), 2f))
        {
            g.FillPath(fill, path);
            g.DrawPath(border, path);
        }

        g.FillEllipse(new SolidBrush(Color.FromArgb(255, 72, 108, 168)), x + 8f, y + 8f, 40f, 40f);
        DrawLabel(g, "Waffle", x + 58f, y + 10f, 18f, TextPrimary, FontStyle.Bold);
        DrawLabel(g, "Level 80", x + 58f, y + 32f, 14f, TextMuted, FontStyle.Regular);

        var health = new RectangleF(x + 58f, y + 42f, 150f, 10f);
        using (var healthBrush = new LinearGradientBrush(health, Color.FromArgb(255, 44, 160, 52), Color.FromArgb(255, 20, 110, 28), 90f))
        {
            g.FillRectangle(healthBrush, health);
        }
    }

    static void DrawMinimap(Graphics g, float x, float y)
    {
        const float size = 170f;
        g.FillEllipse(new SolidBrush(Color.FromArgb(220, 8, 8, 8)), x, y, size, size);
        using (var ring = new Pen(Color.FromArgb(255, 78, 62, 42), 4f))
        {
            g.DrawEllipse(ring, x + 2f, y + 2f, size - 4f, size - 4f);
        }

        using (var land = new SolidBrush(Color.FromArgb(255, 48, 72, 44)))
        {
            g.FillEllipse(land, x + 28f, y + 36f, 88f, 72f);
        }

        DrawLabel(g, "Dornogal", x + 18f, y + size + 8f, 16f, TextGold, FontStyle.Bold);
    }

    static void DrawActionBarBackdrop(Graphics g, float y)
    {
        var bar = new RectangleF(120f, y, PreviewWidth - 240f, 190f);
        using (var path = RoundedRect(bar, 16f))
        using (var fill = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
        using (var border = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
        {
            g.FillPath(fill, path);
            g.DrawPath(border, path);
        }

        const int slots = 12;
        float slotSize = 46f;
        float gap = 6f;
        float totalWidth = slots * slotSize + (slots - 1) * gap;
        float startX = (PreviewWidth - totalWidth) / 2f;
        float slotY = y + 92f;

        for (int i = 0; i < slots; i++)
        {
            var slot = new RectangleF(startX + i * (slotSize + gap), slotY, slotSize, slotSize);
            using (var path = RoundedRect(slot, 6f))
            using (var fill = new SolidBrush(Color.FromArgb(180, 18, 16, 14)))
            using (var border = new Pen(Color.FromArgb(120, 90, 70, 50), 1f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            using (var accent = new SolidBrush(Color.FromArgb(90, 120 + i * 7, 80, 40)))
            {
                g.FillEllipse(accent, slot.X + 8f, slot.Y + 8f, slot.Width - 16f, slot.Height - 16f);
            }
        }
    }

    static RectangleF DrawMenuBar(Graphics g, string themeDir, float centerX, float centerY, int iconSize, int spacing, bool editMode, bool showQueueEye, string clockText)
    {
        int count = IconOrder.Length;
        float barWidth = count * iconSize + (count + 1) * spacing;
        float x = centerX - barWidth / 2f;
        float y = centerY - iconSize / 2f - spacing;
        var bounds = new RectangleF(x - 12f, y - 12f, barWidth + 24f, iconSize + spacing * 2 + 24f);

        if (!string.IsNullOrEmpty(clockText))
        {
            DrawClock(g, centerX, y - 34f, clockText);
        }

        if (editMode)
        {
            using (var borderPen = new Pen(EditModeBorder, 3f))
            {
                borderPen.DashPattern = new[] { 8f, 6f };
                g.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            DrawLabel(g, "BurntWaffleBar", bounds.X + 8f, bounds.Y - 28f, 16f, EditModeBorder, FontStyle.Bold);
        }

        for (int i = 0; i < count; i++)
        {
            string path = Path.Combine(themeDir, IconOrder[i] + ".png");
            if (!File.Exists(path))
                continue;

            using (var icon = new Bitmap(path))
            {
                float drawX = x + spacing + i * (iconSize + spacing);
                float drawY = y + spacing;
                g.DrawImage(icon, drawX, drawY, iconSize, iconSize);
            }
        }

        if (showQueueEye)
        {
            DrawQueueEye(g, x + barWidth + 28f, y + iconSize / 2f);
        }

        return bounds;
    }

    static void DrawClock(Graphics g, float centerX, float y, string text)
    {
        using (var font = new Font("Segoe UI Semibold", 22f, FontStyle.Bold, GraphicsUnit.Pixel))
        {
            SizeF size = g.MeasureString(text, font);
            var rect = new RectangleF(centerX - size.Width / 2f - 18f, y - 8f, size.Width + 36f, size.Height + 16f);
            using (var path = RoundedRect(rect, 10f))
            using (var fill = new SolidBrush(Color.FromArgb(170, 12, 10, 10)))
            using (var border = new Pen(Color.FromArgb(180, 120, 90, 60), 1f))
            using (var textBrush = new SolidBrush(TextPrimary))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
                g.DrawString(text, font, textBrush, rect.X + 18f, rect.Y + 6f);
            }
        }
    }

    static void DrawQueueEye(Graphics g, float x, float centerY)
    {
        const float size = 28f;
        var rect = new RectangleF(x, centerY - size / 2f, size, size);
        g.FillEllipse(new SolidBrush(Color.FromArgb(230, 16, 14, 12)), rect.X, rect.Y, rect.Width, rect.Height);
        g.FillEllipse(new SolidBrush(Color.FromArgb(255, 72, 210, 96)), rect.X + 8f, rect.Y + 8f, 12f, 12f);
    }

    static void DrawSettingsPanel(Graphics g)
    {
        var panel = new RectangleF(180f, 90f, 980f, 760f);
        using (var path = RoundedRect(panel, 12f))
        using (var fill = new SolidBrush(PanelFill))
        using (var border = new Pen(PanelBorder, 2f))
        {
            g.FillPath(fill, path);
            g.DrawPath(border, path);
        }

        DrawLabel(g, "Options", panel.X + 24f, panel.Y + 18f, 28f, TextGold, FontStyle.Bold);

        var sidebar = new RectangleF(panel.X + 16f, panel.Y + 70f, 220f, panel.Height - 100f);
        using (var sidebarFill = new SolidBrush(Color.FromArgb(255, 12, 10, 10)))
        using (var sidebarBorder = new Pen(Color.FromArgb(255, 48, 38, 28), 1f))
        {
            g.FillRectangle(sidebarFill, sidebar);
            g.DrawRectangle(sidebarBorder, sidebar.X, sidebar.Y, sidebar.Width, sidebar.Height);
        }

        DrawLabel(g, "AddOns", sidebar.X + 16f, sidebar.Y + 18f, 16f, TextMuted, FontStyle.Bold);
        var selected = new RectangleF(sidebar.X + 8f, sidebar.Y + 52f, sidebar.Width - 16f, 34f);
        using (var selectedFill = new SolidBrush(Color.FromArgb(255, 36, 28, 18)))
        using (var selectedBorder = new Pen(PanelHighlight, 1f))
        {
            g.FillRectangle(selectedFill, selected);
            g.DrawRectangle(selectedBorder, selected.X, selected.Y, selected.Width, selected.Height);
        }
        DrawLabel(g, "BurntWaffleBar", selected.X + 12f, selected.Y + 8f, 16f, TextGold, FontStyle.Bold);

        float contentX = sidebar.Right + 28f;
        float contentY = panel.Y + 84f;
        DrawLabel(g, "General", contentX, contentY, 22f, TextGold, FontStyle.Bold);
        contentY += 42f;

        DrawSettingRow(g, contentX, contentY, "Enable BurntWaffleBar", true);
        contentY += 38f;
        DrawSettingRow(g, contentX, contentY, "Hide Blizzard Micro Menu", true);
        contentY += 38f;
        DrawSettingRow(g, contentX, contentY, "Use Class Theme", true);
        contentY += 38f;
        DrawSettingRow(g, contentX, contentY, "Show Clock", true);
        contentY += 52f;

        DrawLabel(g, "Layout", contentX, contentY, 22f, TextGold, FontStyle.Bold);
        contentY += 42f;
        DrawSliderRow(g, contentX, contentY, "Icon Size", 0.72f, "100");
        contentY += 48f;
        DrawSliderRow(g, contentX, contentY, "Button Spacing", 0.35f, "2");
        contentY += 58f;

        DrawLabel(g, "Icon Theme", contentX, contentY, 18f, TextPrimary, FontStyle.Regular);
        var dropdown = new RectangleF(contentX + 140f, contentY - 4f, 260f, 30f);
        using (var fill = new SolidBrush(Color.FromArgb(255, 10, 8, 8)))
        using (var border = new Pen(Color.FromArgb(255, 72, 58, 36), 1f))
        using (var textBrush = new SolidBrush(TextPrimary))
        using (var font = new Font("Segoe UI", 16f, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            g.FillRectangle(fill, dropdown);
            g.DrawRectangle(border, dropdown.X, dropdown.Y, dropdown.Width, dropdown.Height);
            g.DrawString("The Paladin", font, textBrush, dropdown.X + 10f, dropdown.Y + 5f);
        }

        var closeButton = new RectangleF(panel.Right - 120f, panel.Bottom - 52f, 92f, 32f);
        using (var fill = new SolidBrush(Color.FromArgb(255, 120, 24, 24)))
        using (var border = new Pen(Color.FromArgb(255, 170, 64, 64), 1f))
        {
            g.FillRectangle(fill, closeButton);
            g.DrawRectangle(border, closeButton.X, closeButton.Y, closeButton.Width, closeButton.Height);
        }
        DrawLabel(g, "Close", closeButton.X + 24f, closeButton.Y + 6f, 16f, TextPrimary, FontStyle.Bold);
    }

    static void DrawSettingRow(Graphics g, float x, float y, string label, bool enabled)
    {
        var box = new RectangleF(x, y + 2f, 18f, 18f);
        using (var fill = new SolidBrush(Color.FromArgb(255, 10, 8, 8)))
        using (var border = new Pen(Color.FromArgb(255, 96, 76, 48), 1f))
        {
            g.FillRectangle(fill, box);
            g.DrawRectangle(border, box.X, box.Y, box.Width, box.Height);
        }

        if (enabled)
        {
            using (var check = new Pen(Color.FromArgb(255, 255, 209, 0), 2f))
            {
                g.DrawLine(check, box.X + 3f, box.Y + 9f, box.X + 7f, box.Y + 13f);
                g.DrawLine(check, box.X + 7f, box.Y + 13f, box.X + 15f, box.Y + 4f);
            }
        }

        DrawLabel(g, label, x + 30f, y, 18f, TextPrimary, FontStyle.Regular);
    }

    static void DrawSliderRow(Graphics g, float x, float y, string label, float ratio, string valueText)
    {
        DrawLabel(g, label, x, y, 18f, TextPrimary, FontStyle.Regular);
        var track = new RectangleF(x + 180f, y + 8f, 360f, 8f);
        using (var trackFill = new SolidBrush(Color.FromArgb(255, 10, 8, 8)))
        using (var trackBorder = new Pen(Color.FromArgb(255, 72, 58, 36), 1f))
        {
            g.FillRectangle(trackFill, track);
            g.DrawRectangle(trackBorder, track.X, track.Y, track.Width, track.Height);
        }

        float fillWidth = track.Width * ratio;
        using (var fill = new SolidBrush(Color.FromArgb(255, 168, 112, 48)))
        {
            g.FillRectangle(fill, track.X, track.Y, fillWidth, track.Height);
        }

        g.FillEllipse(new SolidBrush(TextGold), track.X + fillWidth - 8f, track.Y - 4f, 16f, 16f);
        DrawLabel(g, valueText, track.Right + 16f, y, 18f, TextMuted, FontStyle.Regular);
    }

    static void DrawCaption(Graphics g, string text)
    {
        DrawCentered(g, text, new Font("Segoe UI", 20f, FontStyle.Regular, GraphicsUnit.Pixel), new SolidBrush(TextMuted), PreviewWidth, PreviewHeight - 42f);
    }

    static void DrawLabel(Graphics g, string text, float x, float y, float size, Color color, FontStyle style)
    {
        using (var font = new Font("Segoe UI", size, style, GraphicsUnit.Pixel))
        using (var brush = new SolidBrush(color))
        {
            g.DrawString(text, font, brush, x, y);
        }
    }

    static void DrawCentered(Graphics g, string text, Font font, Brush brush, int width, float y)
    {
        SizeF size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, (width - size.Width) / 2f, y);
    }

    static void WriteSocialPreview(string root, string addonIconPath)
    {
        string githubDir = Path.Combine(root, ".github");
        Directory.CreateDirectory(githubDir);
        string outputPath = Path.Combine(githubDir, "social-preview.png");
        string themeBarPath = Path.Combine(root, "Media", "Themes", "ThePaladin");

        const int size = 1280;

        using (var banner = new Bitmap(size, size, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(banner))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var bg = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(255, 8, 10, 14),
                Color.FromArgb(255, 18, 14, 10),
                90f))
            {
                g.FillRectangle(bg, 0, 0, size, size);
            }

            using (var glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(size * 0.12f, size * 0.38f, size * 0.76f, size * 0.34f);
                using (var glowBrush = new PathGradientBrush(glowPath))
                {
                    glowBrush.CenterColor = Color.FromArgb(48, 255, 145, 64);
                    glowBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 145, 64) };
                    g.FillPath(glowBrush, glowPath);
                }
            }

            using (var icon = new Bitmap(addonIconPath))
            {
                float iconSize = 220f;
                float scale = Math.Min(iconSize / icon.Width, iconSize / icon.Height);
                float drawWidth = icon.Width * scale;
                float drawHeight = icon.Height * scale;
                float x = (size - drawWidth) / 2f;
                float y = 150f;
                g.DrawImage(icon, x, y, drawWidth, drawHeight);
            }

            DrawSocialThemeBar(g, themeBarPath, size, 430f);

            using (var titleFont = new Font("Segoe UI", 92f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var subtitleFont = new Font("Segoe UI", 34f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var detailFont = new Font("Segoe UI", 24f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var creditFont = new Font("Segoe UI", 20f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var titleBrush = new SolidBrush(Color.FromArgb(255, 248, 244, 238)))
            using (var subtitleBrush = new SolidBrush(Color.FromArgb(255, 255, 168, 96)))
            using (var detailBrush = new SolidBrush(Color.FromArgb(255, 170, 156, 140)))
            {
                DrawCentered(g, "BurntWaffleBar", titleFont, titleBrush, size, 700f);
                DrawCentered(g, "Custom micro menu bar", subtitleFont, subtitleBrush, size, 810f);
                DrawCentered(g, "Class themes  ·  Edit Mode  ·  Queue eye", detailFont, detailBrush, size, size - 72f);
                DrawCentered(g, "By Waffle", creditFont, detailBrush, size, size - 40f);
            }

            banner.Save(outputPath, ImageFormat.Png);
        }

        Console.WriteLine("Wrote " + outputPath);
    }

    static void DrawSocialThemeBar(Graphics g, string themeDir, int canvasWidth, float y)
    {
        const int iconSize = 72;
        const int padding = 10;
        int iconsToDraw = Math.Min(IconOrder.Length, 10);
        int barWidth = iconsToDraw * iconSize + (iconsToDraw + 1) * padding;
        float x = (canvasWidth - barWidth) / 2f;
        var barRect = new RectangleF(x - 16f, y - 16f, barWidth + 32f, iconSize + padding * 2 + 32f);

        using (var barPath = RoundedRect(barRect, 18f))
        using (var barBrush = new SolidBrush(Color.FromArgb(210, 24, 20, 18)))
        using (var barBorder = new Pen(Color.FromArgb(255, 88, 62, 42), 2f))
        {
            g.FillPath(barBrush, barPath);
            g.DrawPath(barBorder, barPath);
        }

        for (int i = 0; i < iconsToDraw; i++)
        {
            string path = Path.Combine(themeDir, IconOrder[i] + ".png");
            if (!File.Exists(path))
                continue;

            using (var icon = new Bitmap(path))
            {
                float drawX = x + padding + i * (iconSize + padding);
                g.DrawImage(icon, drawX, y + padding, iconSize, iconSize);
            }
        }
    }

    static GraphicsPath RoundedRect(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2f;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
