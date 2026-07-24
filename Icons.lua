local addonName, ns = ...

local ADDON_ROOT = "Interface\\AddOns\\BurntWaffleBar\\"
local ICON_EXTENSIONS = { ".png", ".tga" }

local THEME_ICON_EXTENSIONS = {
    Pristine = { ".png" },
    FrozenWaffle = { ".png" },
    SpookyWaffle = { ".png" },
    ThePaladin = { ".png" },
    TheIllidari = { ".png" },
}

local BUTTON_ICON_FILES = {
    Collections = "Collections",
    PVP = "PVP",
    AdventureGuide = "AdventureGuide",
    Housing = "Housing",
    GroupFinder = "GroupFinder",
    QuestTracker = "QuestTracker",
    AchievementTracker = "AchievementTracker",
    Talents = "Talents",
    Character = "Character",
    Guild = "Guild",
    Social = "Social",
    GameMenu = "GameMenu",
}

local FROZEN_WAFFLE_CLOCK_OUTLINE = {
    { x = 1, y = 0, alpha = 0.85 },
    { x = -1, y = 0, alpha = 0.85 },
    { x = 0, y = 1, alpha = 0.85 },
    { x = 0, y = -1, alpha = 0.85 },
    { x = 1, y = 1, alpha = 0.72 },
    { x = -1, y = 1, alpha = 0.72 },
    { x = 1, y = -1, alpha = 0.72 },
    { x = -1, y = -1, alpha = 0.72 },
}

local SPOOKY_WAFFLE_CLOCK_OUTLINE = {
    { x = 1, y = 0, alpha = 0.88 },
    { x = -1, y = 0, alpha = 0.88 },
    { x = 0, y = 1, alpha = 0.88 },
    { x = 0, y = -1, alpha = 0.88 },
    { x = 1, y = 1, alpha = 0.74 },
    { x = -1, y = 1, alpha = 0.74 },
    { x = 1, y = -1, alpha = 0.74 },
    { x = -1, y = -1, alpha = 0.74 },
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

local CLOCK_SIZE_RATIO = 0.8
local CLOCK_GAP_RATIO = 0.08

ns.iconThemes = {
    BurntWaffle = {
        label = "Burnt Waffle",
        root = ADDON_ROOT .. "Media\\Themes\\BurntWaffle\\",
        icons = BUTTON_ICON_FILES,
        clockStyle = {
            color = { 1, 0.85, 0.45 },
            font = "Fonts\\FRIZQT__.TTF",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "OUTLINE",
            shadow = false,
        },
        hoverStyle = {
            scale = 1.12,
            brightness = 1.18,
            duration = 0.12,
            glowColor = { 1, 0.92, 0.28 },
            glowAlpha = 0.78,
            glowScale = 1.24,
            glowOuterScale = 1.42,
            glowOuterAlpha = 0.42,
        },
    },
    Pristine = {
        label = "Pristine",
        root = ADDON_ROOT .. "Media\\Themes\\Pristine\\",
        icons = BUTTON_ICON_FILES,
        clockStyle = {
            color = { 1, 1, 1 },
            font = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "",
            useClockDigits = true,
        },
        hoverStyle = {
            scale = 1.12,
            brightness = 1.18,
            duration = 0.12,
            glowColor = { 0.92, 0.96, 1.0 },
            glowAlpha = 0.62,
            glowScale = 1.22,
            glowOuterScale = 1.38,
            glowOuterAlpha = 0.34,
        },
    },
    FrozenWaffle = {
        label = "Frozen Waffle",
        root = ADDON_ROOT .. "Media\\Themes\\FrozenWaffle\\",
        icons = BUTTON_ICON_FILES,
        clockStyle = {
            color = { 0.82, 0.93, 1.0 },
            font = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "",
            outline = {
                color = { 0.05, 0.12, 0.22 },
                offsets = FROZEN_WAFFLE_CLOCK_OUTLINE,
            },
        },
        hoverStyle = {
            scale = 1.10,
            brightness = 1.22,
            duration = 0.12,
            glowColor = { 0.28, 0.88, 0.82 },
            glowAlpha = 0.76,
            glowScale = 1.24,
            glowOuterScale = 1.44,
            glowOuterAlpha = 0.40,
        },
    },
    SpookyWaffle = {
        label = "Spooky Waffle",
        root = ADDON_ROOT .. "Media\\Themes\\SpookyWaffle\\",
        icons = BUTTON_ICON_FILES,
        clockStyle = {
            color = { 0.72, 1.0, 0.45 },
            font = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
            sizeRatio = CLOCK_SIZE_RATIO,
            gapRatio = CLOCK_GAP_RATIO,
            flags = "",
            outline = {
                color = { 0.12, 0.02, 0.18 },
                offsets = SPOOKY_WAFFLE_CLOCK_OUTLINE,
            },
        },
        hoverStyle = {
            scale = 1.10,
            brightness = 1.20,
            duration = 0.12,
            glowColor = { 0.48, 1.0, 0.32 },
            glowAlpha = 0.80,
            glowScale = 1.24,
            glowOuterScale = 1.44,
            glowOuterAlpha = 0.44,
        },
    },
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
}

ns.iconThemeOrder = {
    "BurntWaffle",
    "Pristine",
    "FrozenWaffle",
    "SpookyWaffle",
    "ThePaladin",
    "TheIllidari",
}

function ns.GetActiveIconTheme()
    local themeId = BurntWaffleBarDB and BurntWaffleBarDB.iconTheme or ns.defaults.iconTheme
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
        scale = 1.12,
        brightness = 1.18,
        duration = 0.12,
        glowColor = { 1, 0.92, 0.28 },
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
    local _, themeId = ns.GetActiveIconTheme()
    if themeId == "Pristine" and ns.pristineClockDigitLayout then
        return ns.pristineClockDigitLayout
    end
end

function ns.ClockUsesDigitGlass()
    local style = ns.GetClockStyle()
    return style.useClockDigits and ns.GetClockDigitLayout() ~= nil
end

function ns.ClockUsesGlassTheme()
    return ns.ClockUsesDigitGlass()
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
    local db = BurntWaffleBarDB or {}
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

function ns.GetCustomIconPaths(buttonId, theme)
    theme = theme or select(1, ns.GetActiveIconTheme())
    if not theme or not theme.root then
        return {}
    end

    local themeId = select(2, ns.GetActiveIconTheme())

    local fileName = theme.icons[buttonId] or buttonId
    local paths = {}
    local extensions = THEME_ICON_EXTENSIONS[themeId or ""] or ICON_EXTENSIONS

    for _, extension in ipairs(extensions) do
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

function ns.ApplyCustomIcon(icon, buttonId)
    local theme, themeId = ns.GetActiveIconTheme()
    if not theme or not theme.root then
        return false
    end

    for _, path in ipairs(ns.GetCustomIconPaths(buttonId, theme)) do
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
