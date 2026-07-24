# BurntWaffleBar

A compact custom micro menu bar for World of Warcraft retail, with multiple icon theme packs and built-in Edit Mode support.

## Features

- Custom 12-button micro menu (Collections, PvP, Adventure Guide, Housing, Group Finder, Quest Log, Achievements, Talents, Character, Guild, Social, Game Menu)
- Optional clock above the bar
- Hide Blizzard's default micro menu
- **Edit Mode:** drag the bar in WoW's native Edit Mode (`/editmode`) — no extra addons required
- Icon themes: Burnt Waffle, Pristine, Frozen Waffle, Spooky Waffle, The Paladin, The Illidari

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

BurntWaffleBar is distributed via **GitHub releases** for [WoWUp](https://wowup.io):

1. In WoWUp, click **Get Addons → Install from URL**
2. Paste: `https://github.com/WaffleBar/BurntWaffleBar`
3. WoWUp installs from the latest tagged release and keeps the addon updated

The repo must stay **public** with tagged releases that include a packaged zip (created automatically by GitHub Actions when you push a `v*` tag).

**GitHub topics** (for WoWUp Hub categories): add `wow-addon` and `action-bars` under your repo’s **About → Topics** on GitHub.

## Development

The `tools/` folder contains C# scripts used to process source icon PNGs into production assets. They are optional for running the addon in-game.

Regenerate Illidari icons (example):

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:library /r:System.Drawing.dll /out:"tools\ProcessIllidari.dll" "tools\ProcessTheIllidariIcons.cs"
Add-Type -Path "tools\ProcessIllidari.dll"
[ProcessTheIllidariIcons]::ProcessAll("Media\Themes\TheIllidari\source", "Media\Themes\TheIllidari")
```

## Third-party libraries

This addon embeds [LibEditMode](https://github.com/p3lim-wow/LibEditMode) (see `libs/LibEditMode/LICENSE.txt`) and LibStub for Edit Mode frame repositioning.

## Author

Burn and Waffle
