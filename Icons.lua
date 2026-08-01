local addonName, ns = ...

local ADDON_ROOT = "Interface\\AddOns\\BurntWaffleBar\\"
local ICON_EXTENSIONS = { ".png", ".tga" }
-- Switch to pre-sharpened small/ textures at compact bar sizes (Fire Mage freeform
-- silhouettes need this earlier than circular medallion themes).
local ICON_TEXTURE_SMALL_THRESHOLD = 80

local THEME_ICON_EXTENSIONS = {
    ThePaladin = { ".png" },
    TheIllidari = { ".png" },
    TheFireMage = { ".png" },
    TheRetPally = { ".png" },
}

local CLASS_THEME_IDS = {
    "TheWarrior",
    "TheHunter",
    "TheRogue",
    "ThePriest",
    "TheShaman",
    "TheMage",
    "TheFireMage",
    "TheRetPally",
    "TheWarlock",
    "TheMonk",
    "TheDruid",
    "TheDeathKnight",
    "TheEvoker",
}

for _, themeId in ipairs(CLASS_THEME_IDS) do
    THEME_ICON_EXTENSIONS[themeId] = { ".png" }
end

ns.classThemeByClassFile = {
    WARRIOR = "TheWarrior",
    HUNTER = "TheHunter",
    ROGUE = "TheRogue",
    PRIEST = "ThePriest",
    SHAMAN = "TheShaman",
    MAGE = "TheMage",
    WARLOCK = "TheWarlock",
    MONK = "TheMonk",
    DRUID = "TheDruid",
    DEATHKNIGHT = "TheDeathKnight",
    EVOKER = "TheEvoker",
    PALADIN = "ThePaladin",
    DEMONHUNTER = "TheIllidari",
}

local BUTTON_ICON_FILES = {
    Collections = "Collections",
    PVP = "PVP",
    AdventureGuide = "AdventureGuide",
    Housing = "Housing",
    GroupFinder = "GroupFinder",
    QuestTracker = "QuestTracker",
    AchievementTracker = "AchievementTracker",
    Professions = "Professions",
    Talents = "Talents",
    Character = "Character",
    Guild = "Guild",
    Social = "Social",
    GameMenu = "GameMenu",
}

local THE_PALADIN_CLOCK_OUTLINE = {
    { x = 1, y = 0, alpha = 0.82 },
    { x = -1, y = 0, alpha = 0.82 },
    { x = 0, y = 1, alpha = 0.82 },
    { x = 0, y = -1, alpha = 0.82 },
    { x = 1, y = 1, alpha = 0.68 },
    { x = -1, y = 1, alpha = 0.68 },
    { x = 1, y = -1, alpha = 0.68 },
    { x = -1, y = -1, alpha = 0.68 },
}

local THE_ILLIDARI_CLOCK_OUTLINE = {
    { x = 1, y = 0, alpha = 0.82 },
    { x = -1, y = 0, alpha = 0.82 },
    { x = 0, y = 1, alpha = 0.82 },
    { x = 0, y = -1, alpha = 0.82 },
    { x = 1, y = 1, alpha = 0.68 },
    { x = -1, y = 1, alpha = 0.68 },
    { x = 1, y = -1, alpha = 0.68 },
    { x = -1, y = -1, alpha = 0.68 },
}

local CLASS_CLOCK_OUTLINE = THE_ILLIDARI_CLOCK_OUTLINE

local function MakeClassTheme(themeId, label, clockColor, glowColor, outlineColor, shadowColor)
    return {
        label = label,
        root = ADDON_ROOT .. "Media\\Themes\\" .. themeId .. "\\",
        icons = BUTTON_ICON_FILES,
        iconOutline = {
            drawScale = 0.92,
            textureInset = 0.015,
            color = outlineColor,
            softScale = 1.012,
            softAlpha = 0.06,
            scaleOffsets = true,
            dropShadow = {
                offsetX = 0,
                offsetY = -1,
                scale = 1.02,
                alpha = 0.14,
                color = shadowColor,
            },
        },
        clockStyle = {
            color = clockColor,
            font = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "",
            outline = {
                color = outlineColor,
                offsets = CLASS_CLOCK_OUTLINE,
            },
        },
        hoverStyle = {
            scale = 1.10,
            brightness = 1.22,
            duration = 0.12,
            glowColor = glowColor,
            glowAlpha = 0.74,
            glowScale = 1.24,
            glowOuterScale = 1.42,
            glowOuterAlpha = 0.40,
        },
    }
end

local CLOCK_SIZE_RATIO = 0.8
local CLOCK_GAP_RATIO = 0.08

ns.iconThemes = {
    ThePaladin = {
        label = "The Paladin",
        root = ADDON_ROOT .. "Media\\Themes\\ThePaladin\\",
        icons = BUTTON_ICON_FILES,
        iconOutline = {
            drawScale = 0.92,
            textureInset = 0.015,
            color = { 0.08, 0.04, 0.06 },
            softScale = 1.015,
            softAlpha = 0.10,
            scaleOffsets = true,
            dropShadow = {
                offsetX = 0,
                offsetY = -1,
                scale = 1.02,
                alpha = 0.20,
                color = { 0.10, 0.05, 0.08 },
            },
        },
        clockStyle = {
            color = { 1.0, 0.92, 0.72 },
            font = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "",
            outline = {
                color = { 0.22, 0.10, 0.18 },
                offsets = THE_PALADIN_CLOCK_OUTLINE,
            },
        },
        hoverStyle = {
            scale = 1.10,
            brightness = 1.22,
            duration = 0.12,
            glowColor = { 0.96, 0.55, 0.73 },
            glowAlpha = 0.74,
            glowScale = 1.24,
            glowOuterScale = 1.42,
            glowOuterAlpha = 0.40,
        },
    },
    TheIllidari = {
        label = "The Illidari",
        root = ADDON_ROOT .. "Media\\Themes\\TheIllidari\\",
        icons = BUTTON_ICON_FILES,
        iconOutline = {
            drawScale = 0.92,
            textureInset = 0.015,
            color = { 0.06, 0.04, 0.10 },
            softScale = 1.012,
            softAlpha = 0.06,
            scaleOffsets = true,
            dropShadow = {
                offsetX = 0,
                offsetY = -1,
                scale = 1.02,
                alpha = 0.14,
                color = { 0.04, 0.06, 0.04 },
            },
        },
        clockStyle = {
            color = { 0.80, 0.58, 0.96 },
            font = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "",
            outline = {
                color = { 0.10, 0.04, 0.14 },
                offsets = THE_ILLIDARI_CLOCK_OUTLINE,
            },
        },
        hoverStyle = {
            scale = 1.10,
            brightness = 1.22,
            duration = 0.12,
            glowColor = { 0.35, 1.0, 0.55 },
            glowAlpha = 0.74,
            glowScale = 1.24,
            glowOuterScale = 1.42,
            glowOuterAlpha = 0.40,
        },
    },
    TheWarrior = MakeClassTheme(
        "TheWarrior", "The Warrior",
        { 0.78, 0.61, 0.43 },
        { 1, 0.55, 0.25 },
        { 0.072, 0.048, 0.03 },
        { 0.12, 0.08, 0.05 }
    ),
    TheHunter = MakeClassTheme(
        "TheHunter", "The Hunter",
        { 0.67, 0.83, 0.45 },
        { 0.55, 1, 0.35 },
        { 0.06, 0.084, 0.036 },
        { 0.1, 0.14, 0.06 }
    ),
    TheRogue = MakeClassTheme(
        "TheRogue", "The Rogue",
        { 1, 0.96, 0.41 },
        { 1, 0.92, 0.3 },
        { 0.072, 0.066, 0.036 },
        { 0.12, 0.11, 0.06 }
    ),
    ThePriest = MakeClassTheme(
        "ThePriest", "The Priest",
        { 1, 1, 1 },
        { 1, 0.95, 0.7 },
        { 0.084, 0.078, 0.108 },
        { 0.14, 0.13, 0.18 }
    ),
    TheShaman = MakeClassTheme(
        "TheShaman", "The Shaman",
        { 0, 0.44, 0.87 },
        { 0.35, 0.85, 1 },
        { 0.03, 0.06, 0.108 },
        { 0.05, 0.1, 0.18 }
    ),
    TheMage = MakeClassTheme(
        "TheMage", "The Mage",
        { 0.25, 0.78, 0.92 },
        { 0.45, 0.9, 1 },
        { 0.048, 0.084, 0.132 },
        { 0.08, 0.14, 0.22 }
    ),
    TheFireMage = (function()
        local theme = MakeClassTheme(
            "TheFireMage", "The Fire Mage",
            { 1.0, 0.55, 0.12 },
            { 1.0, 0.78, 0.28 },
            { 0.12, 0.035, 0.015 },
            { 0.2, 0.06, 0.025 }
        )
        -- Freeform silhouettes read a touch large after optical fill; keep slot rhythm tight.
        theme.iconOutline.drawScale = 0.90
        theme.iconOutline.textureInset = 0.02
        return theme
    end)(),
    TheRetPally = (function()
        local theme = MakeClassTheme(
            "TheRetPally", "The Ret Pally",
            { 1.0, 0.90, 0.55 },
            { 1.0, 0.82, 0.35 },
            { 0.10, 0.06, 0.08 },
            { 0.16, 0.08, 0.10 }
        )
        -- Freeform silhouettes (same optical-fill pipeline as The Fire Mage).
        theme.iconOutline.drawScale = 0.90
        theme.iconOutline.textureInset = 0.02
        return theme
    end)(),
    TheWarlock = MakeClassTheme(
        "TheWarlock", "The Warlock",
        { 0.53, 0.53, 0.93 },
        { 0.75, 0.35, 1 },
        { 0.084, 0.036, 0.108 },
        { 0.14, 0.06, 0.18 }
    ),
    TheMonk = MakeClassTheme(
        "TheMonk", "The Monk",
        { 0, 1, 0.59 },
        { 0.35, 1, 0.7 },
        { 0.036, 0.084, 0.06 },
        { 0.06, 0.14, 0.1 }
    ),
    TheDruid = MakeClassTheme(
        "TheDruid", "The Druid",
        { 1, 0.49, 0.04 },
        { 1, 0.72, 0.25 },
        { 0.096, 0.06, 0.03 },
        { 0.16, 0.1, 0.05 }
    ),
    TheDeathKnight = MakeClassTheme(
        "TheDeathKnight", "The Death Knight",
        { 0.77, 0.12, 0.23 },
        { 1, 0.35, 0.4 },
        { 0.072, 0.024, 0.036 },
        { 0.12, 0.04, 0.06 }
    ),
    TheEvoker = MakeClassTheme(
        "TheEvoker", "The Evoker",
        { 0.2, 0.58, 0.5 },
        { 0.4, 1, 0.85 },
        { 0.036, 0.072, 0.066 },
        { 0.06, 0.12, 0.11 }
    ),
}

ns.iconThemeOrder = {
    "ThePaladin",
    "TheRetPally",
    "TheIllidari",
    "TheWarrior",
    "TheHunter",
    "TheRogue",
    "ThePriest",
    "TheShaman",
    "TheMage",
    "TheFireMage",
    "TheWarlock",
    "TheMonk",
    "TheDruid",
    "TheDeathKnight",
    "TheEvoker",
}

function ns.GetActiveIconTheme()
    local db = ns.GetDB and ns.GetDB() or {}
    local themeId = db.iconTheme or ns.defaults.iconTheme

    if db.useClassTheme ~= false then
        local classTheme = ns.GetClassThemeForPlayer and ns.GetClassThemeForPlayer()
        if classTheme then
            themeId = classTheme
        end
    end

    local theme = ns.iconThemes[themeId]

    if not theme then
        themeId = ns.defaults.iconTheme
        theme = ns.iconThemes[themeId]
    end

    return theme, themeId
end

function ns.GetHoverStyle()
    local theme = select(1, ns.GetActiveIconTheme())
    if theme and theme.hoverStyle then
        return theme.hoverStyle
    end

    return {
        scale = 1.10,
        brightness = 1.22,
        duration = 0.12,
        glowColor = { 0.92, 0.96, 1.0 },
        glowAlpha = 0.72,
        glowScale = 1.22,
        glowOuterScale = 1.40,
        glowOuterAlpha = 0.38,
    }
end

function ns.GetIconOutlineStyle()
    local theme = select(1, ns.GetActiveIconTheme())
    return theme and theme.iconOutline or nil
end

function ns.GetClockStyle()
    local theme = select(1, ns.GetActiveIconTheme())
    if theme and theme.clockStyle then
        return theme.clockStyle
    end

    return {
        color = { 1, 1, 1 },
        font = "Fonts\\ARIALN.TTF",
        sizeRatio = CLOCK_SIZE_RATIO,
        gapRatio = CLOCK_GAP_RATIO,
        flags = "",
        shadow = true,
        shadowColor = { 0, 0, 0, 0.85 },
        shadowOffset = { 1, -1 },
    }
end

function ns.GetClockDigitLayout()
end

function ns.ClockUsesDigitGlass()
    return false
end

function ns.ClockUsesGlassTheme()
    return false
end

local function ResolveTintComponent(value, fallback)
    if type(value) ~= "number" then
        return fallback
    end

    if value > 1 then
        return value / 255
    end

    return value
end

function ns.GetClockTintColor(style)
    local db = ns.GetDB() or {}
    local base = (style and style.color) or { 1, 1, 1 }
    local strength = (db.clockTintStrength or 0) / 100
    if strength <= 0 then
        return base[1], base[2], base[3]
    end

    local tint = db.clockTintColor
    local tintR, tintG, tintB = 1, 1, 1
    if type(tint) == "table" then
        if tint.GetRGB then
            tintR, tintG, tintB = tint:GetRGB()
        else
            tintR = ResolveTintComponent(tint.r or tint[1], 1)
            tintG = ResolveTintComponent(tint.g or tint[2], 1)
            tintB = ResolveTintComponent(tint.b or tint[3], 1)
        end
    end

    local blend = strength * 0.65
    return base[1] + (tintR - base[1]) * blend,
        base[2] + (tintG - base[2]) * blend,
        base[3] + (tintB - base[3]) * blend
end

function ns.GetClockDigitTextureName(character)
    if character == ":" then
        return "ClockColon"
    end

    if character == " " then
        return "ClockSpace"
    end

    if character == "A" then
        return "ClockA"
    end

    if character == "M" then
        return "ClockM"
    end

    if character == "P" then
        return "ClockP"
    end

    if character:match("%d") then
        return "Clock" .. character
    end
end

function ns.GetIconTextureTier(iconSize)
    iconSize = iconSize or (ns.GetDB and ns.GetDB().iconSize) or 100
    if iconSize <= ICON_TEXTURE_SMALL_THRESHOLD then
        return "small"
    end
    return "full"
end

function ns.GetCustomIconPaths(buttonId, theme, iconSize)
    theme = theme or select(1, ns.GetActiveIconTheme())
    if not theme or not theme.root then
        return {}
    end

    local themeId = select(2, ns.GetActiveIconTheme())

    local fileName = theme.icons[buttonId] or buttonId
    local paths = {}
    local extensions = THEME_ICON_EXTENSIONS[themeId or ""] or ICON_EXTENSIONS
    local tier = ns.GetIconTextureTier(iconSize)

    for _, extension in ipairs(extensions) do
        if tier == "small" then
            paths[#paths + 1] = theme.root .. "small\\" .. fileName .. extension
        end
        paths[#paths + 1] = theme.root .. fileName .. extension
    end

    return paths
end

local function ProbeTexture(path)
    local probe = UIParent:CreateTexture(nil, "ARTWORK")
    probe:Hide()
    probe:SetTexture(path)

    local texture = probe:GetTexture()
    probe:SetTexture(nil)

    return texture
end

function ns.GetIconTextureInset()
    local style = ns.GetIconOutlineStyle()
    return style and style.textureInset or 0
end

function ns.ApplyIconTexCoords(icon)
    if not icon then
        return
    end

    local inset = ns.GetIconTextureInset()
    if inset and inset > 0 then
        icon:SetTexCoord(inset, 1 - inset, inset, 1 - inset)
    else
        icon:SetTexCoord(0, 1, 0, 1)
    end
end

function ns.ApplyCustomIcon(icon, buttonId, iconSize)
    local theme, themeId = ns.GetActiveIconTheme()
    if not theme or not theme.root then
        return false
    end

    for _, path in ipairs(ns.GetCustomIconPaths(buttonId, theme, iconSize)) do
        if ProbeTexture(path) then
            icon:SetTexture(nil)
            icon:SetTexture(path)
            ns.ApplyIconTexCoords(icon)
            if icon.SetBlendMode then
                icon:SetBlendMode("BLEND")
            end
            return true
        end
    end

    return false
end
