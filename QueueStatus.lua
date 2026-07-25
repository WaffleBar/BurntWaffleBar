local addonName, ns = ...

local queueAnchor
local queueScale = 1
local hooksInstalled = false
local ignoringPointHook = false
local managingQueueStatus = false
local savedUpdatePosition

-- Group Finder art: magnifying-glass lens center/size in normalized slot space.
local LENS_OFFSET_X = -0.10
local LENS_OFFSET_Y = 0.05
local LENS_DIAMETER = 0.80
local EYE_FILL = 0.96
local QUEUE_BUTTON_BASE_SIZE = 45

local function IsQueueStatusAvailable()
    return QueueStatusButton and not QueueStatusButton:IsForbidden()
end

local function ShouldManageQueueStatus()
    local db = ns.GetDB and ns.GetDB()
    if not db or not db.enabled or db.hideNativeMenu == false then
        return false
    end
    if db.showGroupFinder == false then
        return false
    end
    return true
end

local function GetGroupFinderAnchor()
    if not ShouldManageQueueStatus() then
        return nil
    end

    if ns.GetMenuButtonIconAnchor then
        return ns.GetMenuButtonIconAnchor("GroupFinder")
    end

    if ns.GetMenuButtonAnchor then
        return ns.GetMenuButtonAnchor("GroupFinder")
    end
end

local function GetMenuFrame(anchor)
    if not anchor then
        return nil
    end

    local frame = anchor
    while frame do
        if frame.GetName and frame:GetName() == "BurntWaffleBarMenuFrame" then
            return frame
        end
        frame = frame:GetParent()
    end
end

local function GetAnchorIconSize(anchor)
    local slot = anchor
    while slot do
        local width = slot.GetWidth and slot:GetWidth()
        if width and width > 0 then
            return width
        end
        slot = slot.GetParent and slot:GetParent()
    end
    return 36
end

local function GetGroupFinderSlotSize()
    if ns.GetMenuButtonAnchor then
        local slot = ns.GetMenuButtonAnchor("GroupFinder")
        local width = slot and slot.GetWidth and slot:GetWidth()
        if width and width > 0 then
            return width
        end
    end

    return GetAnchorIconSize(queueAnchor)
end

local function ComputeQueueLensPlacement(anchor)
    local iconSize = GetGroupFinderSlotSize()
    local offsetX = math.floor(iconSize * LENS_OFFSET_X + 0.5)
    local offsetY = math.floor(iconSize * LENS_OFFSET_Y + 0.5)
    local targetSize = iconSize * LENS_DIAMETER * EYE_FILL
    local scale = targetSize / QUEUE_BUTTON_BASE_SIZE
    scale = math.min(1.1, math.max(0.58, scale))
    return offsetX, offsetY, scale
end

local function RestoreUpdatePositionOverride()
    if not savedUpdatePosition or not IsQueueStatusAvailable() then
        return
    end

    QueueStatusButton.UpdatePosition = savedUpdatePosition
    savedUpdatePosition = nil
end

local function InstallUpdatePositionOverride()
    if savedUpdatePosition or not IsQueueStatusAvailable() or not QueueStatusButton.UpdatePosition then
        return
    end

    savedUpdatePosition = QueueStatusButton.UpdatePosition
    QueueStatusButton.UpdatePosition = function(self, microMenuPosition, isMenuHorizontal)
        if managingQueueStatus then
            if QueueStatusButton:IsShown() then
                ApplyQueueStatusLayout()
            end
            return
        end

        -- Blizzard calls this with no args when the micro menu is hidden/absent.
        if microMenuPosition == nil then
            return
        end

        return savedUpdatePosition(self, microMenuPosition, isMenuHorizontal)
    end
end

local function RestoreQueueStatusButton()
    if not IsQueueStatusAvailable() then
        return
    end

    RestoreUpdatePositionOverride()

    ignoringPointHook = true
    local parent = MicroMenuContainer or MainMenuBar or UIParent
    QueueStatusButton:SetParent(parent)
    QueueStatusButton:ClearAllPoints()
    QueueStatusButton:SetScale(1)
    ignoringPointHook = false
end

local function RepositionQueueStatusTimer(anchor)
    if not anchor then
        return
    end

    local iconSize = GetAnchorIconSize(anchor)
    local timerFrame = QueueStatusButton.Cooldown or QueueStatusButton.QueueStatusCooldown
    if not timerFrame or (type(timerFrame.IsForbidden) == "function" and timerFrame:IsForbidden()) then
        return
    end

    if timerFrame.SetParent then
        timerFrame:SetParent(anchor:GetParent() or anchor)
    end
    timerFrame:ClearAllPoints()
    timerFrame:SetPoint("BOTTOM", anchor, "TOP", 0, math.floor(iconSize * 0.08 + 0.5))
end

local function ShouldApplyQueueLayout()
    return managingQueueStatus
        and queueAnchor
        and queueAnchor:IsShown()
        and IsQueueStatusAvailable()
        and QueueStatusButton:IsShown()
end

local function ApplyQueueStatusLayout()
    if ignoringPointHook or not ShouldApplyQueueLayout() then
        return
    end

    ignoringPointHook = true
    local offsetX, offsetY, scale = ComputeQueueLensPlacement(queueAnchor)
    queueScale = scale
    QueueStatusButton:SetParent(queueAnchor)
    QueueStatusButton:ClearAllPoints()
    QueueStatusButton:SetScale(queueScale)
    QueueStatusButton:SetFrameLevel(queueAnchor:GetFrameLevel() + 15)
    QueueStatusButton:SetPoint("CENTER", queueAnchor, "CENTER", offsetX, offsetY)
    RepositionQueueStatusTimer(queueAnchor)
    ignoringPointHook = false
end

local function InstallQueueStatusHooks()
    if not IsQueueStatusAvailable() then
        return
    end

    InstallUpdatePositionOverride()

    if hooksInstalled then
        return
    end

    hooksInstalled = true

    hooksecurefunc(QueueStatusButton, "Show", function()
        if not managingQueueStatus then
            return
        end
        ApplyQueueStatusLayout()
    end)

    hooksecurefunc(QueueStatusButton, "SetPoint", function()
        if ignoringPointHook or not managingQueueStatus or not QueueStatusButton:IsShown() then
            return
        end
        ApplyQueueStatusLayout()
    end)

    hooksecurefunc(QueueStatusButton, "SetParent", function()
        if ignoringPointHook or not managingQueueStatus or not QueueStatusButton:IsShown() then
            return
        end
        ApplyQueueStatusLayout()
    end)

    hooksecurefunc(QueueStatusButton, "SetScale", function(self, scale)
        if ignoringPointHook or not managingQueueStatus or not QueueStatusButton:IsShown() then
            return
        end
        if scale ~= queueScale then
            ignoringPointHook = true
            self:SetScale(queueScale)
            ignoringPointHook = false
        end
    end)
end

function ns.UpdateQueueStatusAnchor()
    if not IsQueueStatusAvailable() then
        return
    end

    if not ShouldManageQueueStatus() then
        managingQueueStatus = false
        queueAnchor = nil
        RestoreQueueStatusButton()
        return
    end

    InstallQueueStatusHooks()

    local anchor = GetGroupFinderAnchor()
    local menuFrame = anchor and GetMenuFrame(anchor)
    if not anchor or not menuFrame or not menuFrame:IsShown() then
        managingQueueStatus = false
        queueAnchor = nil
        return
    end

    managingQueueStatus = true
    queueAnchor = anchor

    if QueueStatusButton:IsShown() then
        ApplyQueueStatusLayout()
    end
end

local initFrame = CreateFrame("Frame")
initFrame:RegisterEvent("PLAYER_ENTERING_WORLD")
initFrame:RegisterEvent("LFG_UPDATE")
initFrame:RegisterEvent("LFG_QUEUE_STATUS_UPDATE")
initFrame:RegisterEvent("UPDATE_BATTLEFIELD_STATUS")
initFrame:SetScript("OnEvent", function(_, event, isInitialLogin, isReloadingUI)
    if event == "PLAYER_ENTERING_WORLD" and (isInitialLogin or isReloadingUI) then
        InstallUpdatePositionOverride()
        C_Timer.After(1, ns.UpdateQueueStatusAnchor)
        return
    end

    C_Timer.After(0, ns.UpdateQueueStatusAnchor)
end)

if IsQueueStatusAvailable() then
    InstallUpdatePositionOverride()
end
