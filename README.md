# BurntWaffleBar

A compact custom micro menu bar for World of Warcraft retail, with multiple icon theme packs and built-in Edit Mode support.

## Features

- Custom 12-button micro menu (Collections, PvP, Adventure Guide, Housing, Group Finder, Quest Log, Achievements, Talents, Character, Guild, Social, Game Menu)
- Optional clock above the bar
- Hide Blizzard's default micro menu
- **Edit Mode:** drag the bar in WoW's native Edit Mode (`/editmode`) — no extra addons required
- Icon themes: The Paladin, The Illidari, and **all 13 retail classes** (Warrior, Hunter, Rogue, Priest, Shaman, Mage, Warlock, Monk, Druid, Death Knight, Evoker). Class themes are selected automatically by default.

## Installation

1. Download or clone this repository.
2. Copy the `BurntWaffleBar` folder into:
   ```
   World of Warcraft\_retail_\Interface\AddOns\
   ```
3. Restart WoW (or `/reload` for Lua-only updates; restart for new/changed textures).

## Usage

- **Settings:** Esc → Options → AddOns → **BurntWaffleBar**, or type `/bwb` or `/burntwafflebar`
- **Reposition:** Esc → **Edit Mode**, select **BurntWaffleBar**, and drag it. Positions save per Edit Mode layout.

## WoWUp

BurntWaffleBar is on **WowUpHub** — search for **BurntWaffleBar** in WowUp’s **Get Addons** tab. You can also install from GitHub:

1. In WoWUp, click **Get Addons → Install from URL**
2. Paste: `https://github.com/WaffleBar/BurntWaffleBar`
3. WowUp installs from the latest tagged release

The repo must stay **public** with tagged releases that include a packaged zip (created automatically by GitHub Actions when you push a `v*` tag).

### Preview images (WowUp gallery)

WowUp Hub shows a **Previews** gallery from a `.previews/` folder in your repo ([Hub docs](https://wowup.io/guide/wowup/hub#image-previews)). When you tag a release, Hub snapshots whatever PNG/JPG files are in `.previews/` on that branch.

1. Add or replace screenshots in **`.previews/`** (in-game shots work best; `01-…`, `02-…` naming keeps order sensible). The repo ships five generated mockups — swap in real `/screenshot` captures when you have them.
2. Regenerate mockups: run `tools\MakeWowUpPreviews.exe` (see `.previews/README.md`).
3. Commit, push to `main`, then tag a release (e.g. `v2.4.2`). WowUp picks up the new gallery on the next Hub sync after that release.

The **addon list thumbnail** is separate: set your GitHub repo **Social preview** under **Settings → Social preview**.

1. Upload **`.github/social-preview.jpg`** (molten-W, under 1MB) — or `.github/social-preview.png` if you prefer PNG.
2. Tag a new release so WowUp Hub re-indexes.
3. If WowUp still shows a broken image, remove and re-upload the social preview (GitHub serves signed image URLs; a fresh upload forces Hub to refresh).

A stable copy also lives at `Media/WowUpThumb.jpg` (`https://raw.githubusercontent.com/WaffleBar/BurntWaffleBar/main/Media/WowUpThumb.jpg`).

This folder is excluded from the addon zip via `.pkgmeta`, so previews never ship to players' `Interface\AddOns` folder.

**GitHub-only installs:** If you installed via URL before Hub listing, some WowUp CF builds fail on *Update* for GitHub addons ([known bug](https://github.com/WowUp/WowUp.CF/issues/107)). Hub installs should update normally. If updates still fail, remove the addon and reinstall from search or URL, or add a GitHub token under WowUp → **Options → Addons**.

If WoW is installed under `Program Files`, try running WowUp as administrator when updating.

**GitHub topics** (for WowUp Hub categories): under your repo’s **About → Topics**, add `action-bars`. Optional but common: `warcraft-addon` or `world-of-warcraft-addon`. Topics are free-form — type them even if autocomplete doesn’t suggest them (ignore `wow-addon-snippet`; that’s for code snippets).

## Development

The `tools/` folder contains C# scripts used to process source icon PNGs into production assets. They are optional for running the addon in-game.

Regenerate Illidari icons (example):

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:library /r:System.Drawing.dll /out:"tools\ProcessIllidari.dll" "tools\ProcessTheIllidariIcons.cs"
Add-Type -Path "tools\ProcessIllidari.dll"
[ProcessTheIllidariIcons]::ProcessAll("Media\Themes\TheIllidari\source", "Media\Themes\TheIllidari")
```

Regenerate class theme packs (draws sources + processes to 256px):

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /out:"tools\ClassThemeBuilder.exe" "tools\ClassIconCore.cs" "tools\ClassIconRenderer.cs" "tools\DrawClassThemeSources.cs" "tools\ClassThemeBuilder.cs"
& "tools\ClassThemeBuilder.exe" .
```

Regenerate the WowUp addon icon or Hub preview images:

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /out:"tools\MakeAddonIcon.exe" "tools\MakeAddonIcon.cs"
& "tools\MakeAddonIcon.exe" .

& $csc /nologo /out:"tools\MakeWowUpPreviews.exe" "tools\MakeWowUpPreviews.cs"
& "tools\MakeWowUpPreviews.exe" .
```

Drop in-game screenshots into `.previews/` before tagging a release; see `.previews/README.md`.

## Third-party libraries

This addon embeds [LibEditMode](https://github.com/p3lim-wow/LibEditMode) (see `libs/LibEditMode/LICENSE.txt`) and LibStub for Edit Mode frame repositioning.

## Author

Waffle
