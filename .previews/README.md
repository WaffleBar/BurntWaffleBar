# WowUp Hub preview images

WowUp Hub reads PNG/JPG files from this folder when you publish a **tagged release** from the same branch (see [WowUp Hub guide](https://wowup.io/guide/wowup/hub#image-previews)).

## What shows where

| Location | Source |
|----------|--------|
| **Addon card thumbnail** | GitHub repo **Social preview** (Settings → Social preview) |
| **Previews gallery** | Every image in this `.previews/` folder at release time |

This folder is **not** packaged into the WoW addon zip (see `.pkgmeta`).

## Files

- `01-bar-ingame.png` — micro menu bar in a WoW-style UI scene (The Paladin theme)
- `02-class-themes-ingame.png` — Paladin, Illidari, and Warrior theme comparison
- `03-edit-mode-ingame.png` — Edit Mode selection outline on the bar
- `04-settings-ingame.png` — Options panel mockup for BurntWaffleBar settings
- `05-clock-queue-ingame.png` — clock above the bar plus queue-eye placement

Replace any shot with your own in-game screenshots anytime. Commit and tag a new release so WowUp Hub re-indexes the gallery.

## Regenerate mockups

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /out:"tools\MakeWowUpPreviews.exe" "tools\MakeWowUpPreviews.cs"
& "tools\MakeWowUpPreviews.exe" .
```

Also writes `.github/social-preview.png` (1280×1280 square) for GitHub / WowUp card art — upload it under repo **Settings → Social preview** if WowUp still shows a broken image or your GitHub avatar.
