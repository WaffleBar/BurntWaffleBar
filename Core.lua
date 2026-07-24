local addonName, ns = ...

ns.defaults = {
    enabled = true,
    hideNativeMenu = true,
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
    showTalents = true,
    showCharacter = true,
    showGuild = true,
    showSocial = true,
    showGameMenu = true,
    showClock = true,
    clockFormat = "12",
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

local function MigrateSavedVariables()
    if BurntWafflesDB and not BurntWaffleBarDB then
        BurntWaffleBarDB = BurntWafflesDB
    end

    BurntWaffleBarDB = BurntWaffleBarDB or {}
    MergeDefaults(BurntWaffleBarDB, ns.defaults)

    if BurntWaffleBarDB.iconTheme == "MinimalWhite" then
        BurntWaffleBarDB.iconTheme = "Pristine"
    end

    if BurntWaffleBarDB.iconTheme == "ScaryWaffle" then
        BurntWaffleBarDB.iconTheme = "SpookyWaffle"
    end

    if BurntWaffleBarDB.iconTheme == "Blizzard" or not ns.iconThemes[BurntWaffleBarDB.iconTheme] then
        BurntWaffleBarDB.iconTheme = ns.defaults.iconTheme
    end
end

local function OpenSettings()
    if ns.OpenSettings then
        ns.OpenSettings()
    end
end

EventUtil.ContinueOnAddOnLoaded(addonName, function()
    MigrateSavedVariables()

    SLASH_BURNTWAFFLEBAR1 = "/burntwafflebar"
    SLASH_BURNTWAFFLEBAR2 = "/bwb"
    SlashCmdList["BURNTWAFFLEBAR"] = OpenSettings

    -- Legacy slash commands from BurntWaffles
    SLASH_BURNTWAFFLES1 = "/burntwaffles"
    SLASH_BURNTWAFFLES2 = "/bw"
    SlashCmdList["BURNTWAFFLES"] = OpenSettings
end)
