local addonName, ns = ...

ns.defaults = {
    enabled = true,
    hideNativeMenu = true,
    useClassTheme = true,
    iconTheme = "BurntWaffle",
    iconSize = 100,
    spacing = 2,
    posX = -50,
    posY = -200,
    editModeLayouts = {},
    showCollections = true,
    showPVP = true,
    showAdventureGuide = true,
    showHousing = true,
    showGroupFinder = true,
    showQuestTracker = true,
    showAchievementTracker = true,
    showProfessions = true,
    showTalents = true,
    showCharacter = true,
    showGuild = true,
    showSocial = true,
    showGameMenu = true,
    showTooltips = true,
    fadeInCombat = false,
    showClock = true,
    clockFormat = "12",
    clockFont = "Theme",
    clockSize = 100,
    clockSpacing = 8,
    clockPosX = 0,
    clockPosY = 0,
    clockOpacity = 100,
    clockRimStrength = 100,
    clockGleamStrength = 100,
    clockTintStrength = 0,
    clockTintColor = { r = 1, g = 1, b = 1, a = 1 },
}

local SKIP_DB_COPY_KEYS = {
    __initialized = true,
}

local function CopyTable(source, dest, skipKeys)
    dest = dest or {}
    skipKeys = skipKeys or SKIP_DB_COPY_KEYS

    for key, value in pairs(source) do
        if not skipKeys[key] then
            if type(value) == "table" then
                dest[key] = CopyTable(value, dest[key] or {})
            else
                dest[key] = value
            end
        end
    end

    return dest
end

local function MergeDefaults(target, source)
    for key, value in pairs(source) do
        if type(value) == "table" then
            target[key] = target[key] or {}
            MergeDefaults(target[key], value)
        elseif target[key] == nil then
            target[key] = value
        end
    end
end

local function MigrateThemeId(themeId)
    if themeId == "MinimalWhite" then
        return "Pristine"
    end

    if themeId == "ScaryWaffle" or themeId == "SpookyWaffle" then
        return "BurntWaffle"
    end

    if themeId == "Blizzard" or not ns.iconThemes or not ns.iconThemes[themeId] then
        return ns.defaults.iconTheme
    end

    return themeId
end

local function MigrateThemeSettings(db)
    db.iconTheme = MigrateThemeId(db.iconTheme)

    if db.clockFont and (not ns.clockFonts or not ns.clockFonts[db.clockFont]) then
        db.clockFont = ns.defaults.clockFont
    end
end

function ns.GetDB()
    BurntWaffleBarCharDB = BurntWaffleBarCharDB or {}
    return BurntWaffleBarCharDB
end

function ns.GetClassThemeForPlayer()
    local _, classFile = UnitClass("player")
    return ns.classThemeByClassFile and ns.classThemeByClassFile[classFile]
end

function ns.ApplyClassThemeIfEnabled()
    local db = ns.GetDB()
    if not db or db.useClassTheme == false then
        return false
    end

    local classTheme = ns.GetClassThemeForPlayer()
    if not classTheme then
        return false
    end

    db.iconTheme = classTheme
    return true
end

local function InitializeCharacterDB()
    BurntWaffleBarCharDB = BurntWaffleBarCharDB or {}

    if not BurntWaffleBarCharDB.__initialized then
        if BurntWaffleBarDB and next(BurntWaffleBarDB) then
            CopyTable(BurntWaffleBarDB, BurntWaffleBarCharDB)
        end

        BurntWaffleBarCharDB.useClassTheme = true
        local classTheme = ns.GetClassThemeForPlayer()
        if classTheme then
            BurntWaffleBarCharDB.iconTheme = classTheme
        end

        BurntWaffleBarCharDB.__initialized = true
    end

    MergeDefaults(BurntWaffleBarCharDB, ns.defaults)
    MigrateThemeSettings(BurntWaffleBarCharDB)
    if ns.EnsureButtonOrder then
        ns.EnsureButtonOrder(BurntWaffleBarCharDB)
    end
    ns.ApplyClassThemeIfEnabled()
end

local function MigrateSavedVariables()
    if BurntWafflesDB and not BurntWaffleBarDB then
        BurntWaffleBarDB = BurntWafflesDB
    end

    BurntWaffleBarDB = BurntWaffleBarDB or {}
    MergeDefaults(BurntWaffleBarDB, ns.defaults)
    MigrateThemeSettings(BurntWaffleBarDB)
end

local function OpenSettings()
    if ns.OpenSettings then
        ns.OpenSettings()
    end
end

local function OnPlayerReady()
    InitializeCharacterDB()

    if ns.RefreshMenu then
        ns.RefreshMenu()
    end
end

EventUtil.ContinueOnAddOnLoaded(addonName, function()
    MigrateSavedVariables()

    SLASH_BURNTWAFFLEBAR1 = "/burntwafflebar"
    SLASH_BURNTWAFFLEBAR2 = "/bwb"
    SlashCmdList["BURNTWAFFLEBAR"] = OpenSettings

    SLASH_BURNTWAFFLES1 = "/burntwaffles"
    SLASH_BURNTWAFFLES2 = "/bw"
    SlashCmdList["BURNTWAFFLES"] = OpenSettings
end)

EventUtil.ContinueOnPlayerLogin(OnPlayerReady)
