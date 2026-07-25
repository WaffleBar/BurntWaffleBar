local addonName, ns = ...

local ROW_HEIGHT = 30
local ROW_WIDTH = 292
local PANEL_WIDTH = 340
local PANEL_HEIGHT = 460

local panel
local content
local rows = {}
local draggingIndex

local function DB()
    return ns.GetDB()
end

local function RefreshMenu()
    if ns.RefreshMenu then
        ns.RefreshMenu()
    end
end

local function GetOrderedButtonIds()
    ns.EnsureButtonOrder(DB())
    local order = {}
    for index, id in ipairs(DB().buttonOrder) do
        order[index] = id
    end
    return order
end

local function SwapOrderIndices(fromIndex, toIndex)
    if not fromIndex or not toIndex or fromIndex == toIndex then
        return fromIndex
    end

    local order = GetOrderedButtonIds()
    local movedId = order[fromIndex]
    table.remove(order, fromIndex)
    table.insert(order, toIndex, movedId)
    DB().buttonOrder = order
    return toIndex
end

local function UpdateRowVisual(row, buttonId, index)
    local def = ns.buttonDefsById and ns.buttonDefsById[buttonId]
    row.buttonId = buttonId
    row.index = index
    row:SetPoint("TOPLEFT", content, "TOPLEFT", 0, -((index - 1) * ROW_HEIGHT))
    row.label:SetText(def and def.label or buttonId)

    local hidden = def and DB()[def.setting] == false
    if hidden then
        row.label:SetTextColor(0.55, 0.55, 0.55)
    else
        row.label:SetTextColor(1, 0.82, 0)
    end

    if draggingIndex == index then
        row:SetAlpha(0.55)
    else
        row:SetAlpha(1)
    end
end

local function RefreshRows()
    local order = GetOrderedButtonIds()
    content:SetHeight(#order * ROW_HEIGHT)

    for index, buttonId in ipairs(order) do
        local row = rows[index]
        if not row then
            row = CreateFrame("Button", nil, content)
            row:SetSize(ROW_WIDTH, ROW_HEIGHT)
            row:RegisterForDrag("LeftButton")

            row.highlight = row:CreateTexture(nil, "BACKGROUND")
            row.highlight:SetAllPoints()
            row.highlight:SetColorTexture(1, 1, 1, 0.08)
            row.highlight:Hide()

            row.grip = row:CreateTexture(nil, "ARTWORK")
            row.grip:SetTexture("Interface\\Buttons\\UI-DragHandle")
            row.grip:SetSize(12, 18)
            row.grip:SetPoint("LEFT", 6, 0)

            row.label = row:CreateFontString(nil, "ARTWORK", "GameFontHighlight")
            row.label:SetPoint("LEFT", row.grip, "RIGHT", 10, 0)
            row.label:SetJustifyH("LEFT")
            row.label:SetWidth(ROW_WIDTH - 40)

            row:SetScript("OnEnter", function(self)
                self.highlight:Show()
                if draggingIndex and draggingIndex ~= self.index then
                    draggingIndex = SwapOrderIndices(draggingIndex, self.index)
                    RefreshRows()
                end
            end)

            row:SetScript("OnLeave", function(self)
                self.highlight:Hide()
            end)

            row:SetScript("OnDragStart", function(self)
                draggingIndex = self.index
                self:SetAlpha(0.55)
                self.highlight:Show()
            end)

            row:SetScript("OnDragStop", function(self)
                draggingIndex = nil
                self:SetAlpha(1)
                self.highlight:Hide()
                RefreshRows()
                RefreshMenu()
            end)

            rows[index] = row
        end

        row:Show()
        UpdateRowVisual(row, buttonId, index)
    end

    for index = #order + 1, #rows do
        rows[index]:Hide()
    end
end

local function ResetOrder()
    DB().buttonOrder = ns.GetDefaultButtonOrder()
    draggingIndex = nil
    RefreshRows()
    RefreshMenu()
end

local function CreatePanel()
    panel = CreateFrame("Frame", "BurntWaffleBarButtonOrderFrame", UIParent, "BackdropTemplate")
    panel:SetSize(PANEL_WIDTH, PANEL_HEIGHT)
    panel:SetPoint("CENTER")
    panel:SetFrameStrata("DIALOG")
    panel:SetBackdrop({
        bgFile = "Interface\\DialogFrame\\UI-DialogBox-Background",
        edgeFile = "Interface\\DialogFrame\\UI-DialogBox-Border",
        tile = true,
        tileSize = 32,
        edgeSize = 32,
        insets = { left = 11, right = 12, top = 12, bottom = 11 },
    })
    panel:SetBackdropColor(0, 0, 0, 0.92)
    panel:EnableMouse(true)
    panel:Hide()
    panel:SetMovable(true)
    panel:RegisterForDrag("LeftButton")
    panel:SetScript("OnDragStart", panel.StartMoving)
    panel:SetScript("OnDragStop", panel.StopMovingOrSizing)
    panel:SetScript("OnHide", function()
        draggingIndex = nil
    end)

    local title = panel:CreateFontString(nil, "ARTWORK", "GameFontNormalLarge")
    title:SetPoint("TOP", 0, -18)
    title:SetText("Button Order")

    local subtitle = panel:CreateFontString(nil, "ARTWORK", "GameFontHighlightSmall")
    subtitle:SetPoint("TOP", 0, -40)
    subtitle:SetText("Drag rows to reorder icons on your menu bar.")

    local close = CreateFrame("Button", nil, panel, "UIPanelCloseButton")
    close:SetPoint("TOPRIGHT", -2, -2)
    close:SetScript("OnClick", function()
        panel:Hide()
    end)

    content = CreateFrame("Frame", nil, panel)
    content:SetPoint("TOPLEFT", 20, -64)
    content:SetSize(ROW_WIDTH, ROW_HEIGHT * 12)

    local reset = CreateFrame("Button", nil, panel, "UIPanelButtonTemplate")
    reset:SetSize(140, 24)
    reset:SetPoint("BOTTOM", panel, "BOTTOM", 0, 18)
    reset:SetText("Reset to Default")
    reset:SetScript("OnClick", ResetOrder)

    panel.RefreshRows = RefreshRows
end

function ns.OpenButtonOrderPanel()
    if not panel then
        CreatePanel()
    end

    RefreshRows()
    panel:Show()
end
