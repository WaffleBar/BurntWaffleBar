local addonName, ns = ...

local function DB()
    return ns.GetDB()
end

local function RefreshMenu()
    if ns.RefreshMenu then
        ns.RefreshMenu()
    end
end

local function CreateCheckbox(category, key, label, tooltip, onChanged)
    local setting = Settings.RegisterProxySetting(
        category,
        key,
        Settings.VarType.Boolean,
        label,
        ns.defaults[key],
        function()
            return DB()[key]
        end,
        function(value)
            DB()[key] = value
        end
    )

    local init = Settings.CreateCheckbox(category, setting, tooltip)
    setting:SetValueChangedCallback(function()
        if onChanged then
            onChanged()
        end
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
            return DB()[key]
        end,
        function(value)
            DB()[key] = value
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
            return DB().iconTheme
        end,
        function(value)
            DB().iconTheme = value
            DB().useClassTheme = false
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
            return DB().clockFormat
        end,
        function(value)
            DB().clockFormat = value
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

local function CreateClockFontDropdown(category)
    local function GetClockFontOptions()
        local container = Settings.CreateControlTextContainer()

        for _, fontId in ipairs(ns.clockFontOrder or {}) do
            local font = ns.clockFonts and ns.clockFonts[fontId]
            if font then
                container:Add(fontId, font.label)
            end
        end

        return container:GetData()
    end

    local setting = Settings.RegisterProxySetting(
        category,
        "clockFont",
        Settings.VarType.String,
        "Clock Font",
        ns.defaults.clockFont,
        function()
            return DB().clockFont
        end,
        function(value)
            DB().clockFont = value
        end
    )

    local init = Settings.CreateDropdown(
        category,
        setting,
        GetClockFontOptions,
        "Choose the clock typeface. Theme Default uses the font from your active icon theme."
    )

    setting:SetValueChangedCallback(function()
        RefreshMenu()
    end)

    return init
end

local function GetClockTintColorComponents()
    local color = DB().clockTintColor or ns.defaults.clockTintColor
    return color.r or 1, color.g or 1, color.b or 1
end

local function SetClockTintColor(r, g, b)
    DB().clockTintColor = { r = r, g = g, b = b, a = 1 }
    if (DB().clockTintStrength or 0) <= 0 then
        DB().clockTintStrength = 30
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
    local tooltipsInit = CreateCheckbox(category, "showTooltips", "Show Icon Tooltips", "Show the icon name when you hover over menu icons and the clock.")
    local fadeInCombatInit = CreateCheckbox(category, "fadeInCombat", "Hide In Combat", "Hide the menu bar completely while you are in combat so it can't be clicked accidentally.")
    local useClassThemeInit = CreateCheckbox(category, "useClassTheme", "Use Class Theme", "Automatically use the icon theme that matches this character's class.", function()
        if DB().useClassTheme and ns.ApplyClassThemeIfEnabled then
            ns.ApplyClassThemeIfEnabled()
        end
    end)
    local themeInit = CreateThemeDropdown(category)

    layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("Buttons"))

    local buttonInitializers = {}
    for _, entry in ipairs(ns.buttonSettings or {}) do
        buttonInitializers[#buttonInitializers + 1] = CreateCheckbox(category, entry.setting, entry.label)
    end

    local buttonOrderInit = layout:AddInitializer(CreateSettingsButtonInitializer(
        "Button Order",
        "Customize Order",
        function()
            if ns.OpenButtonOrderPanel then
                ns.OpenButtonOrderPanel()
            end
        end,
        "Drag to reorder menu icons on this character. Hidden buttons stay in the list and reappear in place when turned back on.",
        true
    ))

    layout:AddInitializer(CreateSettingsListSectionHeaderInitializer("Layout"))

    CreateSlider(category, "iconSize", "Icon Size", 20, 100, 1, "Size of menu icons.")
    CreateSlider(category, "spacing", "Button Spacing", -25, 20, 1, "Space between buttons. Use negative values to overlap icons closer together.")

    if ns.HasEditModeSupport and ns.HasEditModeSupport() then
        layout:AddInitializer(CreateSettingsButtonInitializer(
            "Bar Position",
            "Reposition in Edit Mode",
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
    local clockFontInit = CreateClockFontDropdown(category)

    hideNativeInit:SetParentInitializer(enabledInit, function()
        return DB().enabled
    end)
    hideNativeInit:AddShownPredicate(function()
        return DB().enabled
    end)

    themeInit:SetParentInitializer(enabledInit, function()
        return DB().enabled
    end)
    themeInit:AddShownPredicate(function()
        return DB().enabled and DB().useClassTheme == false
    end)

    useClassThemeInit:SetParentInitializer(enabledInit, function()
        return DB().enabled
    end)
    useClassThemeInit:AddShownPredicate(function()
        return DB().enabled
    end)

    for _, init in ipairs(buttonInitializers) do
        init:SetParentInitializer(enabledInit, function()
            return DB().enabled
        end)
        init:AddShownPredicate(function()
            return DB().enabled
        end)
    end

    buttonOrderInit:SetParentInitializer(enabledInit, function()
        return DB().enabled
    end)
    buttonOrderInit:AddShownPredicate(function()
        return DB().enabled
    end)

    clockInit:SetParentInitializer(enabledInit, function()
        return DB().enabled
    end)
    clockInit:AddShownPredicate(function()
        return DB().enabled
    end)

    clockFormatInit:SetParentInitializer(clockInit, function()
        return DB().showClock
    end)
    clockFormatInit:AddShownPredicate(function()
        return DB().enabled and DB().showClock
    end)

    clockFontInit:SetParentInitializer(clockInit, function()
        return DB().showClock
    end)
    clockFontInit:AddShownPredicate(function()
        return DB().enabled
            and DB().showClock
            and not (ns.ClockUsesDigitGlass and ns.ClockUsesDigitGlass())
    end)

    clockSizeInit:SetParentInitializer(clockInit, function()
        return DB().showClock
    end)
    clockSizeInit:AddShownPredicate(function()
        return DB().enabled and DB().showClock
    end)

    clockSpacingInit:SetParentInitializer(clockInit, function()
        return DB().showClock
    end)
    clockSpacingInit:AddShownPredicate(function()
        return DB().enabled and DB().showClock
    end)

    clockPosXInit:SetParentInitializer(clockInit, function()
        return DB().showClock
    end)
    clockPosXInit:AddShownPredicate(function()
        return DB().enabled and DB().showClock
    end)

    clockPosYInit:SetParentInitializer(clockInit, function()
        return DB().showClock
    end)
    clockPosYInit:AddShownPredicate(function()
        return DB().enabled and DB().showClock
    end)

    for _, init in ipairs({ clockTintColorInit, clockTintStrengthInit }) do
        init:SetParentInitializer(clockInit, function()
            return DB().showClock
        end)
        init:AddShownPredicate(function()
            return DB().enabled and DB().showClock
        end)
    end

    for _, init in ipairs({ clockOpacityInit, clockRimInit, clockGleamInit }) do
        init:SetParentInitializer(clockInit, function()
            return DB().showClock and ns.ClockUsesGlassTheme and ns.ClockUsesGlassTheme()
        end)
        init:AddShownPredicate(function()
            return DB().enabled
                and DB().showClock
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
