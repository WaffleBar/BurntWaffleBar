local addonName, ns = ...

local ADDON_ROOT = "Interface\\AddOns\\BurntWaffleBar\\"

ns.clockFontOrder = {
    "Theme",
    "VarelaRound",
    "FrizQuadrata",
    "ArialNarrow",
    "PTSansNarrow",
    "Morpheus",
    "Skurri",
    "TwoThousandTwo",
    "TwoThousandTwoBold",
    "ARKaiThin",
    "ARKaiCondensed",
    "BeiLei",
    "Damage",
    "PVPInfo",
}

ns.clockFonts = {
    Theme = {
        label = "Theme Default",
    },
    VarelaRound = {
        label = "Varela Round",
        path = ADDON_ROOT .. "Media\\Fonts\\VarelaRound-Regular.ttf",
        flags = "",
    },
    FrizQuadrata = {
        label = "Friz Quadrata",
        path = "Fonts\\FRIZQT__.TTF",
        flags = "",
    },
    ArialNarrow = {
        label = "Arial Narrow",
        path = "Fonts\\ARIALN.TTF",
        flags = "",
    },
    PTSansNarrow = {
        label = "PT Sans Narrow",
        path = "Fonts\\PTSansNarrow.ttf",
        flags = "",
    },
    Morpheus = {
        label = "Morpheus",
        path = "Fonts\\MORPHEUS.TTF",
        flags = "",
    },
    Skurri = {
        label = "Skurri",
        path = "Fonts\\SKURRI.TTF",
        flags = "",
    },
    TwoThousandTwo = {
        label = "2002",
        path = "Fonts\\2002.TTF",
        flags = "",
    },
    TwoThousandTwoBold = {
        label = "2002 Bold",
        path = "Fonts\\2002B.TTF",
        flags = "",
    },
    ARKaiThin = {
        label = "AR Kai Thin",
        path = "Fonts\\ARKai_T.ttf",
        flags = "",
    },
    ARKaiCondensed = {
        label = "AR Kai Condensed",
        path = "Fonts\\ARKai_C.ttf",
        flags = "",
    },
    BeiLei = {
        label = "Bei Lei",
        path = "Fonts\\bLEI00d.ttf",
        flags = "",
    },
    Damage = {
        label = "Damage",
        path = "Fonts\\K_Damage.TTF",
        flags = "",
    },
    PVPInfo = {
        label = "PVP Info",
        path = "Fonts\\PVPInfoTextFont.ttf",
        flags = "",
    },
}

function ns.GetClockFontChoice()
    local db = ns.GetDB and ns.GetDB() or {}
    local fontId = db.clockFont or "Theme"
    local choice = ns.clockFonts[fontId]

    if not choice or fontId == "Theme" then
        return nil
    end

    return choice
end

function ns.ResolveClockFontOverride(themeFont, themeFlags)
    local choice = ns.GetClockFontChoice()
    if not choice then
        return themeFont, themeFlags
    end

    local fontPath = choice.path or themeFont
    local fontFlags = choice.flags
    if fontFlags == nil then
        fontFlags = themeFlags
    end

    return fontPath, fontFlags
end
