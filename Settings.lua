local addonName, ns = ...

local function RefreshMenu()
    if ns.RefreshMenu then
        ns.RefreshMenu()
    end
end

local function CreateCheckbox(category, key, label, tooltip)
    local setting = Settings.RegisterProxySetting(
        category,
        key,
        Settings.VarType.Boolean,
        label,
        ns.defaults[key],
        function()
            return BurntWaffleBarDB[key]
        end,
        function(value)
            BurntWaffleBarDB[key] = value
        end
    )

    local init = Settings.CreateCheckbox(category, setting, tooltip)
    setting:SetValueChangedCallback(function()
        RefreshMenu()
    end)
    return init
end

local function CreateSlider(category, key, label, min, max, step, tooltip, formatValue)
    local setting = Settings.RegisterProxySetting(
        category,
        key,
        Settings.VarType.Number,
        label,
        ns.defaults[key],
        function()
            return BurntWaffleBarDB[key]
        end,
        function(value)
            BurntWaffleBarDB[key] = value
        end
    )

    local options = Settings.CreateSliderOptions(min, max, step)
    options:SetLabelFormatter(MinimalSliderWithSteppersMixin.Label.Right, formatValue or function(value)
        if step and step < 1 then
            return string.format("%.1f", value)
        end

        return string.format("%d", value)
    end)
    local init = Settings.CreateSlider(category, setting, options, tooltip)
    setting:SetValueChangedCallback(function()
        RefreshMenu()
    end)
    return init
end

local function CreateThemeDropdown(category)
    local function GetThemeOptions()
        local container = Settings.CreateControlTextContainer()

        for _, themeId in ipairs(ns.iconThemeOrder or {}) do
            local theme = ns.iconThemes and ns.iconThemes[themeId]
            if theme then
                container:Add(themeId, theme.label)
            end
        end

        return container:GetData()
    end

    local setting = Settings.RegisterProxySetting(
        category,
        "iconTheme",
        Settings.VarType.String,
        "Icon Theme",
        ns.defaults.iconTheme,
        function()
            return BurntWaffleBarDB.iconTheme
        end,
        function(value)
            BurntWaffleBarDB.iconTheme = value
        end
    )

    local init = Settings.CreateDropdown(
        category,
        setting,
        GetThemeOptions,
        "Choose which icon set to use. Missing theme files fall back to default game icons."
    )

    setting:SetValueChangedCallback(function()
        RefreshMenu()
    end)

    return init
end

local function CreateClockFormatDropdown(category)
    local function GetClockFormatOptions()
        local container = Settings.CreateControlTextContainer()
        container:Add("12", "12-hour")
        container:Add("24", "24-hour")
        return container:GetData()
    end

    local setting = Settings.RegisterProxySetting(
        category,
        "clockFormat",
        Settings.VarType.String,
        "Clock Format",
        ns.defaults.clockFormat,
        function()
            return BurntWaffleBarDB.clockFormat
        end,
        function(value)
            BurntWaffleBarDB.clockFormat = value
        end
    )

    local init = Settings.CreateDropdown(
        category,
        setting,
        GetClockFormatOptions,
        "Choose between 12-hour and 24-hour time display."
    )

    setting:SetValueChangedCallback(function()
        RefreshMenu()
    end)

    return init
end

local function GetClockTintColorComponents()
    local color = BurntWaffleBarDB.clockTintColor or ns.defaults.clockTintColor
    return color.r or 1, color.g or 1, color.b or 1
end

local function SetClockTintColor(r, g, b)
    BurntWaffleBarDB.clockTintColor = { r = r, g = g, b = b, a = 1 }
    if (BurntWaffleBarDB.clockTintStrength or 0) <= 0 then
        BurntWaffleBarDB.clockTintStrength = 30
    end
    RefreshMenu()
end

local function OpenClockTintColorPicker()
    local r, g, b = GetClockTintColorComponents()
    local previousR, previousG, previousB = r, g, b

    ColorPickerFrame:SetupColorPickerAndShow({
        r = r,
        g = g,
        b = b,
        hasOpacity = false,
        swatchFunc = function()
            SetClockTintColor(ColorPickerFrame:GetColorRGB())
        end,
        cancelFunc = function()
            local previousValues = ColorPickerFrame:GetPreviousValues()
            if previousValues and previousValues.r then
                SetClockTintColor(previousValues.r, previousValues.g, previousValues.b)
            else
                SetClockTintColor(previousR, previousG, previousB)
            end
        end,
    })
end

local function CreateClockTintColorPicker(layout)
    return layout:AddInitializer(CreateSettingsButtonInitializer(
        "Clock Tint Color",
        "Choose Color",
        OpenClockTintColorPicker,
        "Open the color picker for a glass tint on the clock. Transparency and highlights stay intact — use Tint Strength to control how much shows.",
        true
    ))
end

local function InitializeSettings()
    local category, layout = Settings.RegisterVerticalLayoutCategory("BurntWaffleBar")
    Settings.RegisterAddOnCategory(category)
    ns.categoryID = category:GetID()

    layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("General"))

    local enabledInit = CreateCheckbox(category, "enabled", "Enable BurntWaffleBar", "Show the custom micro menu bar.")
    local hideNativeInit = CreateCheckbox(category, "hideNativeMenu", "Hide Blizzard Micro Menu", "Hide the default bottom menu while this addon is enabled.")
    local themeInit = CreateThemeDropdown(category)

    layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("Buttons"))

    local buttonInitializers = {}
    for _, entry in ipairs(ns.buttonSettings or {}) do
        buttonInitializers[#buttonInitializers + 1] = CreateCheckbox(category, entry.setting, entry.label)
    end

    layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("Layout"))

    CreateSlider(category, "iconSize", "Icon Size", 20, 100, 1, "Size of menu icons.")
    CreateSlider(category, "spacing", "Button Spacing", -25, 20, 1, "Space between buttons. Use negative values to overlap icons closer together.")

    local function SetMenuPosition(key, value)
        BurntWaffleBarDB[key] = value
        if (key == "posX" or key == "posY") and ns.SyncMenuPositionFromSettings then
            ns.SyncMenuPositionFromSettings()
        end
    end

    local posXSetting = Settings.RegisterProxySetting(
        category,
        "posX",
        Settings.VarType.Number,
        "Horizontal Position",
        ns.defaults.posX,
        function()
            return BurntWaffleBarDB.posX
        end,
        function(value)
            SetMenuPosition("posX", value)
        end
    )
    local posXOptions = Settings.CreateSliderOptions(-800, 800, 1)
    local posXInit = Settings.CreateSlider(category, posXSetting, posXOptions, "Move the bar left or right.")
    posXSetting:SetValueChangedCallback(function()
        RefreshMenu()
    end)

    local posYSetting = Settings.RegisterProxySetting(
        category,
        "posY",
        Settings.VarType.Number,
        "Vertical Position",
        ns.defaults.posY,
        function()
            return BurntWaffleBarDB.posY
        end,
        function(value)
            SetMenuPosition("posY", value)
        end
    )
    local posYOptions = Settings.CreateSliderOptions(-500, 200, 1)
    local posYInit = Settings.CreateSlider(category, posYSetting, posYOptions, "Move the bar up or down.")
    posYSetting:SetValueChangedCallback(function()
        RefreshMenu()
    end)

    if ns.HasEditModeSupport and ns.HasEditModeSupport() then
        layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("Edit Mode"))
        layout:AddInitializer(CreateSettingsButtonInitializer(
            "Reposition in Edit Mode",
            "Open Edit Mode",
            function()
                if not InCombatLockdown() then
                    ShowUIPanel(EditModeManagerFrame)
                end
            end,
            "Drag BurntWaffleBar in WoW's Edit Mode. Positions are saved per Edit Mode layout.",
            true
        ))
    end

    layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("Clock"))

    local clockInit = CreateCheckbox(category, "showClock", "Show Clock", "Display your system time centered above the menu icons.")
    local clockSizeInit = CreateSlider(category, "clockSize", "Clock Size", 25, 200, 5, "Scale the clock text size. 100 is the theme default.", function(value)
        return string.format("%d%%", value)
    end)
    local clockSpacingInit = CreateSlider(category, "clockSpacing", "Clock Spacing", -25, 40, 1, "Space between the clock and the icon row.")
    local clockPosXInit = CreateSlider(category, "clockPosX", "Clock Horizontal Position", -800, 800, 1, "Move the clock left or right.")
    local clockPosYInit = CreateSlider(category, "clockPosY", "Clock Vertical Position", -500, 200, 1, "Move the clock up or down.")
    local clockOpacityInit = CreateSlider(category, "clockOpacity", "Clock Glass Opacity", 25, 100, 5, "Translucent glass body strength for themed clocks.", function(value)
        return string.format("%d%%", value)
    end)
    local clockRimInit = CreateSlider(category, "clockRimStrength", "Clock Rim Strength", 0, 100, 5, "White glass rim around the clock text.", function(value)
        return string.format("%d%%", value)
    end)
    local clockGleamInit = CreateSlider(category, "clockGleamStrength", "Clock Gleam Strength", 0, 100, 5, "Top-left highlight gleam on the clock.", function(value)
        return string.format("%d%%", value)
    end)
    local clockTintColorInit = CreateClockTintColorPicker(layout)
    local clockTintStrengthInit = CreateSlider(category, "clockTintStrength", "Clock Tint Strength", 0, 100, 5, "How much your chosen tint blends onto the clock. Keeps glass transparency and highlights visible.", function(value)
        return string.format("%d%%", value)
    end)
    local clockFormatInit = CreateClockFormatDropdown(category)

    hideNativeInit:SetParentInitializer(enabledInit, function()
        return BurntWaffleBarDB.enabled
    end)
    hideNativeInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled
    end)

    themeInit:SetParentInitializer(enabledInit, function()
        return BurntWaffleBarDB.enabled
    end)
    themeInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled
    end)

    for _, init in ipairs(buttonInitializers) do
        init:SetParentInitializer(enabledInit, function()
            return BurntWaffleBarDB.enabled
        end)
        init:AddShownPredicate(function()
            return BurntWaffleBarDB.enabled
        end)
    end

    clockInit:SetParentInitializer(enabledInit, function()
        return BurntWaffleBarDB.enabled
    end)
    clockInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled
    end)

    clockFormatInit:SetParentInitializer(clockInit, function()
        return BurntWaffleBarDB.showClock
    end)
    clockFormatInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled and BurntWaffleBarDB.showClock
    end)

    clockSizeInit:SetParentInitializer(clockInit, function()
        return BurntWaffleBarDB.showClock
    end)
    clockSizeInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled and BurntWaffleBarDB.showClock
    end)

    clockSpacingInit:SetParentInitializer(clockInit, function()
        return BurntWaffleBarDB.showClock
    end)
    clockSpacingInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled and BurntWaffleBarDB.showClock
    end)

    clockPosXInit:SetParentInitializer(clockInit, function()
        return BurntWaffleBarDB.showClock
    end)
    clockPosXInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled and BurntWaffleBarDB.showClock
    end)

    clockPosYInit:SetParentInitializer(clockInit, function()
        return BurntWaffleBarDB.showClock
    end)
    clockPosYInit:AddShownPredicate(function()
        return BurntWaffleBarDB.enabled and BurntWaffleBarDB.showClock
    end)

    for _, init in ipairs({ clockTintColorInit, clockTintStrengthInit }) do
        init:SetParentInitializer(clockInit, function()
            return BurntWaffleBarDB.showClock
        end)
        init:AddShownPredicate(function()
            return BurntWaffleBarDB.enabled and BurntWaffleBarDB.showClock
        end)
    end

    for _, init in ipairs({ clockOpacityInit, clockRimInit, clockGleamInit }) do
        init:SetParentInitializer(clockInit, function()
            return BurntWaffleBarDB.showClock and ns.ClockUsesGlassTheme and ns.ClockUsesGlassTheme()
        end)
        init:AddShownPredicate(function()
            return BurntWaffleBarDB.enabled
                and BurntWaffleBarDB.showClock
                and ns.ClockUsesGlassTheme
                and ns.ClockUsesGlassTheme()
        end)
    end
end

function ns.OpenSettings()
    if ns.categoryID then
        Settings.OpenToCategory(ns.categoryID)
    end
end

EventUtil.ContinueOnPlayerLogin(InitializeSettings)
