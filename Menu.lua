local addonName, ns = ...

local menuFrame
local activeButtons = {}
local nativeMenuHidden = false
local clockTicker

local function IsValidFrame(frame, frameType)
    if not frame then
        return false
    end

    local ok, objectType = pcall(function()
        return frame:GetObjectType()
    end)

    return ok and (not frameType or objectType == frameType)
end

local function FormatClockTime(use24Hour)
    local timeTable = date("*t")
    local hour = timeTable.hour
    local minute = timeTable.min

    if use24Hour then
        return string.format("%02d:%02d", hour, minute)
    end

    local suffix = hour >= 12 and " PM" or " AM"
    local displayHour = hour % 12
    if displayHour == 0 then
        displayHour = 12
    end

    return string.format("%d:%02d%s", displayHour, minute, suffix)
end

local function StopClockTicker()
    if clockTicker then
        clockTicker:Cancel()
        clockTicker = nil
    end
end

local CLOCK_FONT_FALLBACK = "Fonts\\ARIALN.TTF"

local function SafeSetFont(fontString, fontPath, fontSize, fontFlags)
    if not fontString or not fontPath or not fontSize then
        return false
    end

    local ok = pcall(fontString.SetFont, fontString, fontPath, fontSize, fontFlags or "")
    if not ok then
        return false
    end

    local resolvedPath = fontString:GetFont()
    return resolvedPath ~= nil and resolvedPath ~= ""
end

local function ResolveClockFont(fontPath, fontSize, fontFlags)
    local candidates = {}

    if fontPath then
        candidates[#candidates + 1] = fontPath
    end

    candidates[#candidates + 1] = CLOCK_FONT_FALLBACK

    if NumberFont_Outline_Med then
        local fallbackPath = NumberFont_Outline_Med:GetFont()
        if fallbackPath then
            candidates[#candidates + 1] = fallbackPath
        end
    end

    local gameFontPath = GameFontNormal:GetFont()
    if gameFontPath then
        candidates[#candidates + 1] = gameFontPath
    end

    for _, candidate in ipairs(candidates) do
        if SafeSetFont(menuFrame.clockText, candidate, fontSize, fontFlags) then
            return menuFrame.clockText:GetFont(), fontSize, fontFlags or ""
        end
    end

    return CLOCK_FONT_FALLBACK, fontSize, fontFlags or ""
end

local function ConfigureCrispTexture(texture)
    if not texture then
        return
    end

    if texture.SetSnapToPixelGrid then
        pcall(texture.SetSnapToPixelGrid, texture, false)
    end

    if texture.SetTexelSnappingBias then
        texture:SetTexelSnappingBias(0)
    end
end

local function SetTextureDrawOrder(texture, layer, subLevel)
    if texture and texture.SetDrawLayer then
        texture:SetDrawLayer(layer, subLevel)
    end
end

local function BuildScaledOutlineOffsets(iconSize)
    local step = math.max(1, math.floor(iconSize / 70 + 0.5))
    local rings = {
        { radius = step, alpha = 1.0, diagonal = 0.88 },
        { radius = step + 1, alpha = 0.68, diagonal = 0.52 },
        { radius = step + 2, alpha = 0.38, diagonal = 0.28 },
    }
    local offsets = {}

    for _, ring in ipairs(rings) do
        local radius = ring.radius
        offsets[#offsets + 1] = { x = radius, y = 0, alpha = ring.alpha }
        offsets[#offsets + 1] = { x = -radius, y = 0, alpha = ring.alpha }
        offsets[#offsets + 1] = { x = 0, y = radius, alpha = ring.alpha }
        offsets[#offsets + 1] = { x = 0, y = -radius, alpha = ring.alpha }

        for _, dx in ipairs({ -1, 1 }) do
            for _, dy in ipairs({ -1, 1 }) do
                offsets[#offsets + 1] = {
                    x = dx * radius,
                    y = dy * radius,
                    alpha = ring.diagonal,
                }
            end
        end
    end

    return offsets
end

local function HideClockOutlineLayers()
    if not menuFrame or not menuFrame.clockOutlineLayers then
        return
    end

    for _, layer in ipairs(menuFrame.clockOutlineLayers) do
        layer:Hide()
    end
end

local function HideClockDigitTextures()
    if not menuFrame or not menuFrame.clockDigitTextures then
        return
    end

    for _, texture in ipairs(menuFrame.clockDigitTextures) do
        texture:Hide()
    end
end

local function HideClockGlassLayers()
    HideClockDigitTextures()
end

local function GetClockDigitHeight(db, style)
    local iconSize = db.iconSize or 28
    local sizeRatio = style.sizeRatio or 0.55
    local clockScale = (db.clockSize or 100) / 100
    return math.max(10, iconSize * sizeRatio * clockScale)
end

local function GetClockDigitAlpha(db)
    local opacityScale = (db.clockOpacity or 100) / 100
    local rimScale = (db.clockRimStrength or 100) / 100
    local gleamScale = (db.clockGleamStrength or 100) / 100
    local blend = opacityScale * (0.70 + (rimScale * 0.15) + (gleamScale * 0.15))
    return math.max(0.35, math.min(1, blend))
end

local function ApplyClockOutlineLayers(style, text)
    if not menuFrame or not menuFrame.clockText then
        return
    end

    local outline = style.outline
    if not outline or not outline.offsets then
        HideClockOutlineLayers()
        return
    end

    menuFrame.clockOutlineLayers = menuFrame.clockOutlineLayers or {}
    local outlineColor = outline.color or { 0, 0, 0 }
    local fontPath, fontSize, fontFlags = menuFrame.clockText:GetFont()

    if (not fontPath or not fontSize) and menuFrame.clockFont then
        fontPath = menuFrame.clockFont.path
        fontSize = menuFrame.clockFont.size
        fontFlags = menuFrame.clockFont.flags
    end

    if not fontPath or not fontSize then
        HideClockOutlineLayers()
        return
    end

    for index, offset in ipairs(outline.offsets) do
        local layer = menuFrame.clockOutlineLayers[index]
        if not layer then
            layer = menuFrame.clockHolder:CreateFontString(nil, "ARTWORK")
            menuFrame.clockOutlineLayers[index] = layer
        end

        layer:ClearAllPoints()
        layer:SetPoint("CENTER", menuFrame.clockText, "CENTER", offset.x or 0, offset.y or 0)
        layer:SetFont(fontPath, fontSize, fontFlags or "")
        layer:SetTextColor(outlineColor[1], outlineColor[2], outlineColor[3], offset.alpha or 0.5)
        layer:SetShadowOffset(0, 0)
        layer:SetText(text or menuFrame.clockText:GetText() or "")
        layer:Show()
    end

    for index = #outline.offsets + 1, #menuFrame.clockOutlineLayers do
        menuFrame.clockOutlineLayers[index]:Hide()
    end
end

local function ApplyClockDigitGlass(style, text)
    if not menuFrame or not menuFrame.clockHolder then
        return
    end

    local layout = ns.GetClockDigitLayout and ns.GetClockDigitLayout()
    if not layout then
        return
    end

    local db = BurntWaffleBarDB or {}
    local theme = select(1, ns.GetActiveIconTheme())
    local root = theme and theme.root
    if not root then
        return
    end

    if menuFrame.clockText then
        menuFrame.clockText:Hide()
    end

    local digitHeight = math.max(10, GetClockDigitHeight(db, style))
    local digitAlpha = GetClockDigitAlpha(db)
    local tintR, tintG, tintB = ns.GetClockTintColor(style)
    local kerning = digitHeight * -0.015
    local totalWidth = 0
    local entries = {}

    for i = 1, #text do
        local character = text:sub(i, i)
        local metrics = layout[character] or layout["0"]
        local advance = digitHeight * metrics.advance
        entries[i] = {
            character = character,
            advance = advance,
            metrics = metrics,
        }
        totalWidth = totalWidth + advance
        if i < #text then
            totalWidth = totalWidth + kerning
        end
    end

    menuFrame.clockDigitTextures = menuFrame.clockDigitTextures or {}
    local xOffset = -totalWidth / 2

    for index, entry in ipairs(entries) do
        local texture = menuFrame.clockDigitTextures[index]
        if not texture then
            texture = menuFrame.clockHolder:CreateTexture(nil, "ARTWORK")
            menuFrame.clockDigitTextures[index] = texture
        end

        local textureName = ns.GetClockDigitTextureName and ns.GetClockDigitTextureName(entry.character)
        if textureName then
            texture:SetTexture(root .. textureName .. ".png")
            texture:SetTexCoord(entry.metrics.u0, entry.metrics.u1, 0, 1)
            texture:SetSize(entry.advance, digitHeight)
            if texture.SetSnapToPixelGrid then
                texture:SetSnapToPixelGrid(false)
            end
            if texture.SetTexelSnappingBias then
                texture:SetTexelSnappingBias(0, 0)
            end
            texture:ClearAllPoints()
            texture:SetPoint("LEFT", menuFrame.clockHolder, "CENTER", xOffset, 0)
            texture:SetVertexColor(tintR, tintG, tintB, digitAlpha)
            texture:Show()
        else
            texture:Hide()
        end

        xOffset = xOffset + entry.advance + kerning
    end

    for index = #text + 1, #menuFrame.clockDigitTextures do
        menuFrame.clockDigitTextures[index]:Hide()
    end

    menuFrame.clockHolder:SetSize(totalWidth, digitHeight + 4)
end

local function ApplyClockVisualLayers(style, text)
    if ns.ClockUsesDigitGlass and ns.ClockUsesDigitGlass() then
        HideClockOutlineLayers()
        ApplyClockDigitGlass(style, text)
    else
        HideClockDigitTextures()
        ApplyClockOutlineLayers(style, text)
    end
end

local function SizeClockHolder()
    if not menuFrame or not menuFrame.clockHolder then
        return 0
    end

    if ns.ClockUsesDigitGlass and ns.ClockUsesDigitGlass() then
        return menuFrame.clockHolder:GetHeight() or 0
    end

    if not menuFrame.clockText then
        return 0
    end

    local padX = 4
    local padY = 3
    local width = (menuFrame.clockText:GetStringWidth() or 0) + padX
    local height = (menuFrame.clockText:GetStringHeight() or 0) + padY
    menuFrame.clockHolder:SetSize(width, height)
    return height
end

local function UpdateClockText()
    if not menuFrame or not menuFrame.clockHolder then
        return
    end

    local db = BurntWaffleBarDB
    if not db or not db.showClock then
        return
    end

    local text = FormatClockTime(db.clockFormat == "24")
    local style = ns.GetClockStyle and ns.GetClockStyle() or {}

    if ns.ClockUsesDigitGlass and ns.ClockUsesDigitGlass() then
        ApplyClockDigitGlass(style, text)
        return
    end

    if not menuFrame.clockText then
        return
    end

    menuFrame.clockText:SetText(text)
    ApplyClockVisualLayers(style, text)
    SizeClockHolder()
end

local function ResolveClockFontSize(fontPath, fontFlags, iconSize, sizeRatio, clockScale, sampleText)
    local targetHeight = math.max(10, iconSize * sizeRatio * clockScale)
    local fontSize = math.max(10, math.floor(targetHeight + 0.5))

    fontPath, fontSize, fontFlags = ResolveClockFont(fontPath, fontSize, fontFlags)
    menuFrame.clockText:SetText(sampleText)

    local measured = menuFrame.clockText:GetStringHeight()
    if measured and measured > 0 then
        fontSize = math.max(10, math.floor(fontSize * (targetHeight / measured) + 0.5))
        fontPath, fontSize, fontFlags = ResolveClockFont(fontPath, fontSize, fontFlags)
    end

    menuFrame.clockFont = {
        path = fontPath,
        size = fontSize,
        flags = fontFlags,
    }

    return fontSize
end

local function ApplyClockStyle()
    if not menuFrame or not menuFrame.clockHolder then
        return
    end

    local db = BurntWaffleBarDB
    local style = ns.GetClockStyle and ns.GetClockStyle() or {}
    local text = FormatClockTime(db.clockFormat == "24")

    if ns.ClockUsesDigitGlass and ns.ClockUsesDigitGlass() then
        ApplyClockDigitGlass(style, text)
        return
    end

    if not menuFrame.clockText then
        menuFrame.clockText = menuFrame.clockHolder:CreateFontString(nil, "OVERLAY")
        menuFrame.clockText:SetPoint("CENTER", menuFrame.clockHolder, "CENTER", 0, 0)
    end

    menuFrame.clockText:Show()
    HideClockDigitTextures()

    local iconSize = db.iconSize or 28
    local sizeRatio = style.sizeRatio or 0.55
    local clockScale = (db.clockSize or 100) / 100
    local fontPath = style.font
    local fontFlags = style.flags or ""

    if not fontPath then
        if NumberFont_Outline_Med then
            fontPath, _, fontFlags = NumberFont_Outline_Med:GetFont()
        end
        if not fontPath then
            fontPath, _, fontFlags = GameFontNormal:GetFont()
        end
        fontFlags = fontFlags or "OUTLINE"
    end

    ResolveClockFontSize(fontPath, fontFlags, iconSize, sizeRatio, clockScale, text)
    menuFrame.clockText:SetShadowOffset(0, 0)

    local color = style.color or { 1, 1, 1 }
    local tintR, tintG, tintB = ns.GetClockTintColor(style)
    menuFrame.clockText:SetTextColor(tintR, tintG, tintB)

    if style.shadow then
        local shadowColor = style.shadowColor or { 0, 0, 0, 0.85 }
        local shadowOffset = style.shadowOffset or { 1, -1 }
        menuFrame.clockText:SetShadowOffset(shadowOffset[1], shadowOffset[2])
        menuFrame.clockText:SetShadowColor(shadowColor[1], shadowColor[2], shadowColor[3], shadowColor[4] or 1)
    end

    ApplyClockVisualLayers(style, text)
    SizeClockHolder()
end

local function GetClockGap()
    local db = BurntWaffleBarDB or {}
    return math.max(0, db.clockSpacing or 8)
end

local function EnsureClockFrame()
    if not menuFrame then
        return
    end

    if not menuFrame.clockHolder then
        menuFrame.clockHolder = CreateFrame("Frame", nil, menuFrame)
        menuFrame.clockHolder:SetPoint("TOP", menuFrame, "TOP", 0, 0)
    end

    if not menuFrame.clockText then
        menuFrame.clockText = menuFrame.clockHolder:CreateFontString(nil, "OVERLAY")
        menuFrame.clockText:SetPoint("CENTER", menuFrame.clockHolder, "CENTER", 0, 0)
    end

    ApplyClockStyle()
end

local function StartClockTicker()
    StopClockTicker()

    local db = BurntWaffleBarDB
    if not db or not db.showClock then
        return
    end

    UpdateClockText()
    clockTicker = C_Timer.NewTicker(1, UpdateClockText)
end

local NATIVE_MICRO_BUTTONS = {
    "CharacterMicroButton",
    "ProfessionMicroButton",
    "PlayerSpellsMicroButton",
    "AchievementMicroButton",
    "QuestLogMicroButton",
    "HousingMicroButton",
    "GuildMicroButton",
    "LFDMicroButton",
    "CollectionsMicroButton",
    "EJMicroButton",
    "PVPMicroButton",
    "StoreMicroButton",
    "MainMenuMicroButton",
    "HelpMicroButton",
    "QuickJoinToastButton",
    "SocialsMicroButton",
}

local FALLBACK_ATLASES = {
    Collections = "UI-HUD-MicroMenu-Collections-Up",
    AdventureGuide = "UI-HUD-MicroMenu-AdventureGuide-Up",
    Housing = "UI-HUD-MicroMenu-Housing-Up",
    GroupFinder = "UI-HUD-MicroMenu-Groupfinder-Up",
    QuestTracker = "UI-HUD-MicroMenu-Questlog-Up",
    AchievementTracker = "UI-HUD-MicroMenu-Achievements-Up",
    Talents = "UI-HUD-MicroMenu-SpecTalents-Up",
    Character = "UI-HUD-MicroMenu-Character-Up",
    Guild = "UI-HUD-MicroMenu-GuildCommunities-Up",
    Social = "UI-HUD-MicroMenu-Socials-Up",
    GameMenu = "UI-HUD-MicroMenu-GameMenu-Up",
}

local PVP_ATLAS_CANDIDATES = {
    "UI-HUD-MicroMenu-PVP-Up",
    "hud-microbutton-PVP-Up",
    "UI-HUD-MicroMenu-Honor-Up",
    "hud-microbutton-Honor-Up",
}

local PVP_TEXTURES = {
    Alliance = "Interface\\Icons\\PVPCurrency-Honor-Alliance",
    Horde = "Interface\\Icons\\PVPCurrency-Honor-Horde",
    Neutral = "Interface\\Icons\\Achievement_PVP_A_01",
}

local function StripButtonChrome(btn, isSecure)
    btn:SetNormalTexture("")
    btn:SetPushedTexture("")
    btn:SetHighlightTexture("")
    btn:SetDisabledTexture("")
end

local function AtlasExists(atlas)
    if not atlas or atlas == "" then
        return false
    end

    if C_Texture and C_Texture.GetAtlasInfo then
        return C_Texture.GetAtlasInfo(atlas) ~= nil
    end

    return pcall(function()
        local probe = UIParent:CreateTexture(nil, "ARTWORK")
        probe:SetAtlas(atlas)
        probe:Hide()
        probe:SetParent(nil)
    end)
end

local function TrySetAtlas(icon, atlas)
    if not AtlasExists(atlas) then
        return false
    end

    local ok = pcall(function()
        icon:SetAtlas(atlas)
    end)

    return ok
end

local function GetPVPAtlas()
    local faction = UnitFactionGroup("player")
    local factionCandidates = {}

    if faction then
        factionCandidates[#factionCandidates + 1] = "UI-HUD-MicroMenu-PVP-" .. faction .. "-Up"
        factionCandidates[#factionCandidates + 1] = "hud-microbutton-PVP-" .. faction .. "-Up"
    end

    for _, atlas in ipairs(factionCandidates) do
        if AtlasExists(atlas) then
            return atlas
        end
    end

    for _, atlas in ipairs(PVP_ATLAS_CANDIDATES) do
        if AtlasExists(atlas) then
            return atlas
        end
    end

    return nil
end

local function GetPVPTexture()
    local faction = UnitFactionGroup("player")
    return PVP_TEXTURES[faction] or PVP_TEXTURES.Neutral
end

local function CopyAtlasFromNative(icon, native)
    if not native then
        return false
    end

    local regions = {
        native.GetNormalTexture and native:GetNormalTexture(),
        native.Icon,
        native.icon,
    }

    for _, region in ipairs(regions) do
        if region and region.GetAtlas then
            local atlas = region:GetAtlas()
            if TrySetAtlas(icon, atlas) then
                return true
            end
        end
    end

    return false
end

local function SyncTextureFromIcon(target, source)
    local atlas = source.GetAtlas and source:GetAtlas()
    if atlas and atlas ~= "" then
        target:SetAtlas(atlas)
    else
        target:SetTexture(source:GetTexture())
    end

    target:SetTexCoord(source:GetTexCoord())
end

local function GetHoverGlowStyle()
    local style = ns.GetHoverStyle and ns.GetHoverStyle() or {}
    local color = style.glowColor or { 1, 0.85, 0.45 }
    return color[1], color[2], color[3],
        style.glowAlpha or 0.65,
        style.glowScale or 1.22,
        style.glowOuterScale or 1.40,
        style.glowOuterAlpha or 0.38
end

local function ApplyHoverGlowVisual(slot, glowAlpha, brightness)
    if not slot or not slot.hoverGlow then
        return
    end

    if not glowAlpha or glowAlpha <= 0.01 then
        slot.hoverGlow:Hide()
        if slot.hoverGlowOuter then
            slot.hoverGlowOuter:Hide()
        end
        slot.hoverGlowAlpha = 0
        return
    end

    local r, g, b, maxAlpha, _, outerScale, outerAlphaRatio = GetHoverGlowStyle()
    local innerAlpha = glowAlpha * maxAlpha
    local outerAlpha = glowAlpha * outerAlphaRatio
    slot.hoverGlow:SetVertexColor(r, g, b, innerAlpha)
    slot.hoverGlow:Show()

    if slot.hoverGlowOuter then
        slot.hoverGlowOuter:SetVertexColor(r, g, b, outerAlpha)
        slot.hoverGlowOuter:Show()
    end

    slot.hoverGlowAlpha = glowAlpha
end

local function EnsureHoverGlow(slot, icon, drawSize)
    if not slot or not slot.content or not icon then
        return
    end

    local _, _, _, _, glowScale, outerScale = GetHoverGlowStyle()
    local glowSize = math.max(8, math.floor(drawSize * glowScale + 0.5))
    local outerSize = math.max(8, math.floor(drawSize * outerScale + 0.5))

    if not slot.hoverGlowOuter then
        slot.hoverGlowOuter = slot.content:CreateTexture(nil, "BACKGROUND")
        slot.hoverGlowOuter:SetPoint("CENTER")
        slot.hoverGlowOuter:SetBlendMode("ADD")
    end

    if not slot.hoverGlow then
        slot.hoverGlow = slot.content:CreateTexture(nil, "BACKGROUND")
        slot.hoverGlow:SetPoint("CENTER")
        slot.hoverGlow:SetBlendMode("ADD")
    end

    slot.hoverGlowOuter:SetSize(outerSize, outerSize)
    SyncTextureFromIcon(slot.hoverGlowOuter, icon)
    ConfigureCrispTexture(slot.hoverGlowOuter)
    SetTextureDrawOrder(slot.hoverGlowOuter, "BACKGROUND", 0)

    slot.hoverGlow:SetSize(glowSize, glowSize)
    SyncTextureFromIcon(slot.hoverGlow, icon)
    ConfigureCrispTexture(slot.hoverGlow)
    SetTextureDrawOrder(slot.hoverGlow, "BACKGROUND", 1)

    ApplyHoverGlowVisual(slot, slot.hoverGlowAlpha or 0, slot.hoverBrightness or 1)
end

local hoverAnimator

local function EnsureHoverAnimator()
    if hoverAnimator then
        return hoverAnimator
    end

    hoverAnimator = CreateFrame("Frame")
    hoverAnimator.anims = {}
    hoverAnimator:SetScript("OnUpdate", function(_, elapsed)
        for slot, anim in pairs(hoverAnimator.anims) do
            if not slot.content or not slot.mmIcon then
                hoverAnimator.anims[slot] = nil
            else
                anim.elapsed = anim.elapsed + elapsed
                local t = math.min(1, anim.elapsed / anim.duration)
                local eased = t * (2 - t)
                local scale = anim.fromScale + (anim.toScale - anim.fromScale) * eased
                local bright = anim.fromBright + (anim.toBright - anim.fromBright) * eased
                local glow = anim.fromGlow + (anim.toGlow - anim.fromGlow) * eased

                slot.content:SetScale(scale)
                slot.mmIcon:SetVertexColor(bright, bright, bright, 1)
                ApplyHoverGlowVisual(slot, glow, bright)

                if t >= 1 then
                    slot.hoverBrightness = anim.toBright
                    slot.hoverGlowAlpha = anim.toGlow
                    hoverAnimator.anims[slot] = nil
                end
            end
        end
    end)

    return hoverAnimator
end

local function ResetSlotHoverVisual(slot)
    if not slot then
        return
    end

    if hoverAnimator and hoverAnimator.anims then
        hoverAnimator.anims[slot] = nil
    end

    slot.hoverBrightness = 1
    slot.hoverGlowAlpha = 0

    if slot.content then
        slot.content:SetScale(1)
    end

    if slot.mmIcon then
        slot.mmIcon:SetVertexColor(1, 1, 1, 1)
    end

    ApplyHoverGlowVisual(slot, 0, 1)
end

local function AnimateSlotHover(slot, toScale, toBright)
    if not slot or not slot.content or not slot.mmIcon then
        return
    end

    local style = ns.GetHoverStyle and ns.GetHoverStyle() or {}
    local animator = EnsureHoverAnimator()
    local fromScale = slot.content:GetScale() or 1
    local fromBright = slot.hoverBrightness or 1
    local fromGlow = slot.hoverGlowAlpha or 0
    local _, _, _, targetGlowAlpha = GetHoverGlowStyle()
    local toGlow = (toScale > 1) and targetGlowAlpha or 0

    animator.anims[slot] = {
        fromScale = fromScale,
        toScale = toScale,
        fromBright = fromBright,
        toBright = toBright,
        fromGlow = fromGlow,
        toGlow = toGlow,
        duration = style.duration or 0.12,
        elapsed = 0,
    }
end

local function SetSlotHovered(slot, hovered)
    local style = ns.GetHoverStyle and ns.GetHoverStyle() or {}
    if hovered then
        AnimateSlotHover(slot, style.scale or 1.12, style.brightness or 1.18)
    else
        AnimateSlotHover(slot, 1, 1)
    end
end

local function GetSlotContent(slot, iconSize)
    if not slot.content then
        slot.content = CreateFrame("Frame", nil, slot)
        slot.content:SetPoint("CENTER")
    end

    slot.content:SetSize(iconSize, iconSize)
    return slot.content
end

local function DisableButtonHighlight(slot, btn)
    if slot and slot.highlight then
        slot.highlight:Hide()
    end

    if btn then
        btn:SetHighlightTexture("")
        if btn.highlight then
            btn.highlight:Hide()
        end
    end
end

local function GetOrCreateButtonEntry(def)
    local wantsSecure = def.isSecure and true or false
    local entry = activeButtons[def.id]
    if entry and IsValidFrame(entry.slot, "Frame") and IsValidFrame(entry.btn, "Button") then
        if entry.isSecure == wantsSecure then
            return entry
        end

        entry.btn:Hide()
        entry.btn:SetParent(nil)
        entry.slot:Hide()
        entry.slot:SetParent(nil)
        activeButtons[def.id] = nil
    end

    local btnName = "BurntWaffleBarButton_" .. def.id
    local slot = CreateFrame("Frame", nil, menuFrame)
    slot:EnableMouse(false)

    local btn
    if wantsSecure then
        btn = CreateFrame("Button", btnName, menuFrame, "SecureActionButtonTemplate")
    else
        btn = CreateFrame("Button", btnName, menuFrame)
    end

    StripButtonChrome(btn, wantsSecure)
    btn:RegisterForClicks("AnyUp")
    btn:ClearAllPoints()
    btn:SetAllPoints(slot)
    btn:SetFrameLevel(slot:GetFrameLevel() + 2)

    btn:SetScript("OnEnter", function()
        SetSlotHovered(slot, true)
        GameTooltip:SetOwner(btn, "ANCHOR_TOP")
        GameTooltip:SetText(def.tooltip, 1, 1, 1, true)
        GameTooltip:Show()
    end)

    btn:SetScript("OnLeave", function()
        SetSlotHovered(slot, false)
        GameTooltip:Hide()
    end)

    entry = {
        slot = slot,
        btn = btn,
        isSecure = wantsSecure,
    }
    activeButtons[def.id] = entry
    return entry
end

local function HideIconOutline(slot)
    if not slot then
        return
    end

    if slot.outlines then
        for _, outline in ipairs(slot.outlines) do
            outline:Hide()
        end
    end

    if slot.outlineUnderlay then
        slot.outlineUnderlay:Hide()
    end

    if slot.softOutline then
        slot.softOutline:Hide()
    end

    if slot.dropShadow then
        slot.dropShadow:Hide()
    end
end

local function GetIconDrawSize(iconSize, style)
    style = style or (ns.GetIconOutlineStyle and ns.GetIconOutlineStyle())
    local drawScale = style and style.drawScale or 1
    return math.max(8, math.floor(iconSize * drawScale + 0.5))
end

local function ApplyIconOutline(slot, icon, iconSize)
    local style = ns.GetIconOutlineStyle and ns.GetIconOutlineStyle()
    if not slot or not icon or not style then
        HideIconOutline(slot)
        return
    end

    local content = slot.content or slot
    local drawSize = GetIconDrawSize(iconSize, style)

    if slot.outlines then
        for _, outline in ipairs(slot.outlines) do
            outline:Hide()
        end
    end

    if slot.softOutline then
        slot.softOutline:Hide()
    end

    if slot.dropShadow then
        slot.dropShadow:Hide()
    end

    if slot.outlineUnderlay then
        slot.outlineUnderlay:Hide()
    end

    if style.dropShadow then
        if not slot.dropShadow then
            slot.dropShadow = content:CreateTexture(nil, "BACKGROUND")
        end

        local shadow = style.dropShadow
        local shadowOffsetX = shadow.offsetX or 0
        local shadowOffsetY = shadow.offsetY or -1
        local shadowScale = shadow.scale or 1.03
        local shadowColor = shadow.color or { 0.02, 0.03, 0.06 }
        local shadowSize = drawSize * shadowScale

        slot.dropShadow:ClearAllPoints()
        slot.dropShadow:SetPoint("CENTER", content, "CENTER", shadowOffsetX, shadowOffsetY)
        slot.dropShadow:SetSize(shadowSize, shadowSize)
        SyncTextureFromIcon(slot.dropShadow, icon)
        ConfigureCrispTexture(slot.dropShadow)
        slot.dropShadow:SetVertexColor(shadowColor[1], shadowColor[2], shadowColor[3], shadow.alpha or 0.40)
        SetTextureDrawOrder(slot.dropShadow, "BACKGROUND", -1)
        slot.dropShadow:Show()
    end

    if style.softScale then
        if not slot.softOutline then
            slot.softOutline = content:CreateTexture(nil, "BACKGROUND")
            slot.softOutline:SetPoint("CENTER")
        end

        local softSize = drawSize * style.softScale
        slot.softOutline:SetSize(softSize, softSize)
        SyncTextureFromIcon(slot.softOutline, icon)
        ConfigureCrispTexture(slot.softOutline)
        local softColor = style.color or { 0, 0, 0 }
        slot.softOutline:SetVertexColor(softColor[1], softColor[2], softColor[3], style.softAlpha or 0.3)
        SetTextureDrawOrder(slot.softOutline, "BACKGROUND", 0)
        slot.softOutline:Show()
    elseif slot.softOutline then
        slot.softOutline:Hide()
    end

    local offsets = style.offsets
    if style.scaleOffsets then
        offsets = BuildScaledOutlineOffsets(iconSize)
    end

    if offsets and #offsets > 0 then
        slot.outlines = slot.outlines or {}
        local outlineLayer = "ARTWORK"

        for index, offset in ipairs(offsets) do
            local outline = slot.outlines[index]
            if not outline then
                outline = content:CreateTexture(nil, outlineLayer)
                slot.outlines[index] = outline
            end

            outline:ClearAllPoints()
            outline:SetPoint("CENTER", content, "CENTER", offset.x or 0, offset.y or 0)
            outline:SetSize(drawSize, drawSize)
            SyncTextureFromIcon(outline, icon)
            ConfigureCrispTexture(outline)
            local outlineColor = style.color or { 0, 0, 0 }
            outline:SetVertexColor(outlineColor[1], outlineColor[2], outlineColor[3], offset.alpha or 0.5)
            SetTextureDrawOrder(outline, outlineLayer, 1)
            outline:Show()
        end

        for index = #offsets + 1, #(slot.outlines or {}) do
            slot.outlines[index]:Hide()
        end
    end

    SetTextureDrawOrder(icon, "ARTWORK", 2)
end

local function ApplyButtonIcon(icon, def)
    if ns.ApplyCustomIcon and ns.ApplyCustomIcon(icon, def.id) then
        return true
    end

    icon:SetTexCoord(0, 1, 0, 1)

    if def.id == "Character" then
        SetPortraitTexture(icon, "player")
        return false
    end

    if def.id == "PVP" then
        if _G.PVPMicroButton and CopyAtlasFromNative(icon, PVPMicroButton) then
            return false
        end

        local pvpAtlas = GetPVPAtlas()
        if pvpAtlas and TrySetAtlas(icon, pvpAtlas) then
            return false
        end

        icon:SetTexture(GetPVPTexture())
        return false
    end

    if def.nativeBtn and CopyAtlasFromNative(icon, _G[def.nativeBtn]) then
        return false
    end

    if TrySetAtlas(icon, def.fallbackAtlas or FALLBACK_ATLASES[def.id]) then
        return false
    end

    if def.texture then
        icon:SetTexture(def.texture)
        return false
    end

    icon:SetTexture("Interface\\Icons\\INV_Misc_QuestionMark")
    return false
end

local function SetupButton(entry, def, iconSize)
    local slot = entry.slot
    local btn = entry.btn
    local content = GetSlotContent(slot, iconSize)

    slot:SetSize(iconSize, iconSize)
    btn:SetSize(iconSize, iconSize)

    if not slot.mmIcon then
        slot.mmIcon = content:CreateTexture(nil, "ARTWORK")
        slot.mmIcon:SetPoint("CENTER")
    elseif slot.mmIcon:GetParent() ~= content then
        slot.mmIcon:SetParent(content)
        slot.mmIcon:SetPoint("CENTER")
    end

    ResetSlotHoverVisual(slot)
    HideIconOutline(slot)

    local outlineStyle = ns.GetIconOutlineStyle and ns.GetIconOutlineStyle()
    local drawSize = GetIconDrawSize(iconSize, outlineStyle)

    local usedCustomIcon = ApplyButtonIcon(slot.mmIcon, def)
    slot.mmIcon:SetSize(drawSize, drawSize)
    slot.mmIcon:SetVertexColor(1, 1, 1, 1)
    ConfigureCrispTexture(slot.mmIcon)
    if slot.mmIcon.SetBlendMode then
        slot.mmIcon:SetBlendMode("BLEND")
    end
    SetTextureDrawOrder(slot.mmIcon, "ARTWORK", 2)
    slot.mmIcon:Show()

    if usedCustomIcon and ns.GetIconOutlineStyle() then
        ApplyIconOutline(slot, slot.mmIcon, iconSize)
    else
        HideIconOutline(slot)
    end

    EnsureHoverGlow(slot, slot.mmIcon, drawSize)

    DisableButtonHighlight(slot, btn)
end

local function ClickNativeMicroButton(label, ...)
    if InCombatLockdown() then
        print("|cffff8800BurntWaffleBar:|r Can't open " .. label .. " in combat.")
        return false
    end

    for i = 1, select("#", ...) do
        local name = select(i, ...)
        local native = _G[name]
        if native and native.Click then
            native:Click()
            return true
        end
    end

    return false
end

local function OpenCollections()
    ClickNativeMicroButton("Warband Collections", "CollectionsMicroButton")
end

local function OpenGroupFinder()
    if ClickNativeMicroButton("Group Finder", "LFDMicroButton", "GroupFinderMicroButton") then
        return
    end

    if ToggleLFDParentFrame then
        ToggleLFDParentFrame()
        return
    end

    if PVEFrame_ToggleFrame then
        PVEFrame_ToggleFrame()
    end
end

local function OpenAdventureGuide()
    ClickNativeMicroButton("Adventure Guide", "EJMicroButton")
end

local function OpenHousing()
    ClickNativeMicroButton("Housing", "HousingMicroButton")
end

local function OpenQuestTracker()
    ClickNativeMicroButton("Quest Log", "QuestLogMicroButton")
end

local function OpenAchievementTracker()
    ClickNativeMicroButton("Achievements", "AchievementMicroButton")
end

local function OpenTalents()
    ClickNativeMicroButton("Talents & Spellbook", "PlayerSpellsMicroButton", "ProfessionMicroButton")
end

local function OpenCharacter()
    ClickNativeMicroButton("Character Info", "CharacterMicroButton")
end

local function OpenGuild()
    ClickNativeMicroButton("Guild & Communities", "GuildMicroButton")
end

local function OpenSocial()
    if ClickNativeMicroButton("Social", "SocialsMicroButton", "QuickJoinToastButton") then
        return
    end

    if FriendsFrame_Show then
        FriendsFrame_Show()
    end
end

local function ConfigureButtonClick(btn, def)
    btn:SetAttribute("useOnKeyDown", nil)
    btn:SetAttribute("*type1", nil)
    btn:SetAttribute("*clickbutton1", nil)
    btn:SetAttribute("type", nil)
    btn:SetAttribute("clickbutton", nil)

    if def.onClick then
        btn:SetScript("OnClick", def.onClick)
    else
        btn:SetScript("OnClick", nil)
    end
end

local function OpenPVP()
    if InCombatLockdown() then
        print("|cffff8800BurntWaffleBar:|r Can't open PvP in combat.")
        return
    end

    if _G.PVPMicroButton and _G.PVPMicroButton.Click then
        PVPMicroButton:Click()
        return
    end

    C_AddOns.LoadAddOn("Blizzard_PVPUI")

    if TogglePVPUI then
        TogglePVPUI()
        return
    end

    if PVPUIFrame then
        ToggleFrame(PVPUIFrame)
    end
end

local function OpenGameMenu()
    if InCombatLockdown() then
        print("|cffff8800BurntWaffleBar:|r Can't open the game menu in combat.")
        return
    end

    ToggleFrame(GameMenuFrame)
end

-- Left-to-right order
local buttonDefs = {
    {
        id = "Collections",
        setting = "showCollections",
        label = "Warband Collections",
        nativeBtn = "CollectionsMicroButton",
        tooltip = "Warband Collections",
        isSecure = false,
        onClick = OpenCollections,
    },
    {
        id = "PVP",
        setting = "showPVP",
        label = "PvP",
        tooltip = "Player vs. Player",
        isSecure = false,
        onClick = OpenPVP,
    },
    {
        id = "AdventureGuide",
        setting = "showAdventureGuide",
        label = "Adventure Guide",
        nativeBtn = "EJMicroButton",
        tooltip = "Adventure Guide",
        isSecure = false,
        onClick = OpenAdventureGuide,
    },
    {
        id = "Housing",
        setting = "showHousing",
        label = "Housing",
        nativeBtn = "HousingMicroButton",
        tooltip = "Housing",
        isSecure = false,
        onClick = OpenHousing,
    },
    {
        id = "GroupFinder",
        setting = "showGroupFinder",
        label = "Group Finder",
        nativeBtn = "LFDMicroButton",
        tooltip = "Group Finder",
        isSecure = false,
        onClick = OpenGroupFinder,
    },
    {
        id = "QuestTracker",
        setting = "showQuestTracker",
        label = "Quest Tracker",
        nativeBtn = "QuestLogMicroButton",
        tooltip = "Quest Log",
        isSecure = false,
        onClick = OpenQuestTracker,
    },
    {
        id = "AchievementTracker",
        setting = "showAchievementTracker",
        label = "Achievement Tracker",
        nativeBtn = "AchievementMicroButton",
        tooltip = "Achievements",
        isSecure = false,
        onClick = OpenAchievementTracker,
    },
    {
        id = "Talents",
        setting = "showTalents",
        label = "Talents & Spellbook",
        nativeBtn = "PlayerSpellsMicroButton",
        tooltip = "Talents & Spellbook",
        isSecure = false,
        onClick = OpenTalents,
    },
    {
        id = "Character",
        setting = "showCharacter",
        label = "Character",
        nativeBtn = "CharacterMicroButton",
        tooltip = "Character Info",
        isSecure = false,
        onClick = OpenCharacter,
    },
    {
        id = "Guild",
        setting = "showGuild",
        label = "Guild",
        nativeBtn = "GuildMicroButton",
        tooltip = "Guild & Communities",
        isSecure = false,
        onClick = OpenGuild,
    },
    {
        id = "Social",
        setting = "showSocial",
        label = "Social",
        nativeBtn = "QuickJoinToastButton",
        tooltip = "Social",
        isSecure = false,
        onClick = OpenSocial,
    },
    {
        id = "GameMenu",
        setting = "showGameMenu",
        label = "Game Menu",
        nativeBtn = "MainMenuMicroButton",
        tooltip = "Game Menu",
        isSecure = false,
        onClick = function(_, button)
            if button == "LeftButton" then
                OpenGameMenu()
            end
        end,
    },
}

ns.buttonSettings = {}
for _, def in ipairs(buttonDefs) do
    ns.buttonSettings[#ns.buttonSettings + 1] = {
        setting = def.setting,
        label = def.label,
    }
end

local function HideFrame(frame)
    if not frame then
        return
    end

    frame:Hide()
    frame:SetScript("OnShow", frame.Hide)
end

local function ShowFrame(frame)
    if not frame then
        return
    end

    frame:SetScript("OnShow", nil)
    frame:Show()
end

local function HideNativeMicroMenu()
    if nativeMenuHidden then
        return
    end

    nativeMenuHidden = true

    HideFrame(MicroMenu)
    HideFrame(MicroMenuContainer)

    for _, name in ipairs(NATIVE_MICRO_BUTTONS) do
        HideFrame(_G[name])
    end
end

local function ShowNativeMicroMenu()
    if not nativeMenuHidden then
        return
    end

    nativeMenuHidden = false

    ShowFrame(MicroMenu)
    ShowFrame(MicroMenuContainer)

    for _, name in ipairs(NATIVE_MICRO_BUTTONS) do
        ShowFrame(_G[name])
    end
end

local function UpdateNativeMenuVisibility()
    local db = BurntWaffleBarDB
    if db and db.enabled and db.hideNativeMenu then
        HideNativeMicroMenu()
    else
        ShowNativeMicroMenu()
    end
end

local function UpdatePosition()
    if not menuFrame then
        return
    end

    if ns.ApplyMenuPosition then
        ns.ApplyMenuPosition(menuFrame)
        return
    end

    local db = BurntWaffleBarDB
    menuFrame:ClearAllPoints()
    menuFrame:SetPoint("CENTER", UIParent, "CENTER", math.floor((db.posX or 0) + 0.5), math.floor((db.posY or -200) + 0.5))
end

function ns.RefreshMenu()
    local db = BurntWaffleBarDB
    if not db or not db.enabled then
        if menuFrame then
            menuFrame:Hide()
        end
        StopClockTicker()
        ShowNativeMicroMenu()
        return
    end

    local ok, err = pcall(function()
        if not menuFrame then
            menuFrame = CreateFrame("Frame", "BurntWaffleBarMenuFrame", UIParent)
            menuFrame:SetFrameStrata("MEDIUM")
            if ns.SetupEditMode then
                ns.SetupEditMode(menuFrame)
            end
        end

        UpdatePosition()

        local enabled = {}
        for _, def in ipairs(buttonDefs) do
            if db[def.setting] ~= false then
                enabled[#enabled + 1] = def
            end
        end

        if #enabled == 0 then
            menuFrame:Hide()
            return
        end

        local iconSize = db.iconSize or 28
        local spacing = db.spacing or 2
        local showClock = db.showClock ~= false
        local placed = {}

        for _, def in ipairs(enabled) do
            local entry = GetOrCreateButtonEntry(def)
            local btn = entry.btn

            ConfigureButtonClick(btn, def)

            SetupButton(entry, def, iconSize)
            placed[#placed + 1] = entry
        end

        local totalWidth = 0
        local maxHeight = 0

        for index, entry in ipairs(placed) do
            totalWidth = totalWidth + entry.slot:GetWidth()
            if index < #placed then
                totalWidth = totalWidth + spacing
            end
            maxHeight = math.max(maxHeight, entry.slot:GetHeight())
        end

        local clockGap = showClock and GetClockGap() or 0
        local clockHeight = 0

        if showClock then
            EnsureClockFrame()
            ApplyClockStyle()
            clockHeight = SizeClockHolder() + clockGap
            menuFrame.clockHolder:Show()
            if menuFrame.clockText and ns.ClockUsesDigitGlass and ns.ClockUsesDigitGlass() then
                menuFrame.clockText:Hide()
            elseif menuFrame.clockText then
                menuFrame.clockText:Show()
            end
            StartClockTicker()
        else
            StopClockTicker()
            if menuFrame.clockHolder then
                menuFrame.clockHolder:Hide()
            end
            if menuFrame.clockText then
                menuFrame.clockText:Hide()
            end
            HideClockOutlineLayers()
            HideClockGlassLayers()
        end

        local xOffset = 0
        local snappedClockHeight = math.floor(clockHeight + 0.5)
        for _, entry in ipairs(placed) do
            local slot = entry.slot
            local btn = entry.btn
            slot:ClearAllPoints()
            if showClock then
                slot:SetPoint("TOPLEFT", menuFrame, "TOPLEFT", math.floor(xOffset + 0.5), -snappedClockHeight)
            else
                slot:SetPoint("LEFT", menuFrame, "LEFT", math.floor(xOffset + 0.5), 0)
            end
            slot:Show()
            btn:Show()
            xOffset = xOffset + slot:GetWidth() + spacing
        end

        if showClock then
            menuFrame.clockHolder:ClearAllPoints()
            menuFrame.clockHolder:SetPoint(
                "TOP",
                menuFrame,
                "TOP",
                math.floor((db.clockPosX or 0) + 0.5),
                math.floor((db.clockPosY or 0) + 0.5)
            )
        end

        menuFrame:SetSize(totalWidth, maxHeight + clockHeight)
        menuFrame:Show()
        UpdateNativeMenuVisibility()
    end)

    if ok then
        return
    end

    print("|cffff8800BurntWaffleBar:|r Menu failed to load: " .. tostring(err))
    ShowNativeMicroMenu()
end

local initFrame = CreateFrame("Frame")
initFrame:RegisterEvent("PLAYER_ENTERING_WORLD")
initFrame:SetScript("OnEvent", function(_, _, isInitialLogin, isReloadingUI)
    if isInitialLogin or isReloadingUI then
        C_Timer.After(1, ns.RefreshMenu)
    end
end)
