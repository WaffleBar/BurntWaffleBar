local addonName, ns = ...

local editModeRegistered = false

local function GetLibEditMode()
    if not LibStub then
        return nil
    end

    local ok, lib = pcall(LibStub, "LibEditMode")
    if ok then
        return lib
    end

    return nil
end

local function GetActiveLayoutName(lib)
    if lib and lib.GetActiveLayoutName then
        return lib:GetActiveLayoutName()
    end

    return nil
end

local function GetLegacyPosition(db)
    return {
        point = "CENTER",
        x = db.posX or ns.defaults.posX,
        y = db.posY or ns.defaults.posY,
    }
end

local function GetLayoutPosition(db, layoutName)
    if layoutName and db.editModeLayouts and db.editModeLayouts[layoutName] then
        return db.editModeLayouts[layoutName]
    end

    return GetLegacyPosition(db)
end

function ns.HasEditModeSupport()
    return GetLibEditMode() ~= nil
end

function ns.ApplyMenuPosition(frame)
    if not frame then
        return
    end

    local db = BurntWaffleBarDB or {}
    local lib = GetLibEditMode()
    local layoutName = GetActiveLayoutName(lib)
    local pos = GetLayoutPosition(db, layoutName)

    frame:ClearAllPoints()
    frame:SetPoint(pos.point, UIParent, pos.point, pos.x, pos.y)
end

function ns.SyncMenuPositionFromSettings()
    local lib = GetLibEditMode()
    if not lib then
        return
    end

    local db = BurntWaffleBarDB or {}
    local layoutName = GetActiveLayoutName(lib)
    if not layoutName then
        return
    end

    db.editModeLayouts = db.editModeLayouts or {}
    db.editModeLayouts[layoutName] = GetLegacyPosition(db)
end

function ns.SetupEditMode(frame)
    local lib = GetLibEditMode()
    if not lib or editModeRegistered or not frame then
        return false
    end

    local db = BurntWaffleBarDB or {}
    db.editModeLayouts = db.editModeLayouts or {}

    local default = GetLegacyPosition(db)

    lib:AddFrame(frame, function(_, layoutName, point, x, y)
        db.editModeLayouts[layoutName] = {
            point = point,
            x = math.floor(x + 0.5),
            y = math.floor(y + 0.5),
        }

        if point == "CENTER" then
            db.posX = db.editModeLayouts[layoutName].x
            db.posY = db.editModeLayouts[layoutName].y
        end
    end, default, "BurntWaffleBar")

    lib:RegisterCallback("layout", function(layoutName)
        if frame:IsShown() then
            ns.ApplyMenuPosition(frame)
        end
    end)

    editModeRegistered = true
    return true
end
