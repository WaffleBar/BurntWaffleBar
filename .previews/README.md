# WowUp Hub preview images

WowUp Hub reads PNG/JPG files from this folder when you publish a **tagged release** from the same branch (see [WowUp Hub guide](https://wowup.io/guide/wowup/hub#image-previews)).

## What shows where

| Location | Source |
|----------|--------|
| **Addon card thumbnail** | GitHub repo **Social preview** (Settings → Social preview) |
| **Previews gallery** | Every image in this `.previews/` folder at release time |

This folder is **not** packaged into the WoW addon zip (see `.pkgmeta`).

## Files

- `01-burnt-waffle.png` … in-game or marketing shots (preferred)
- `bar-burnt-waffle.png` … auto-generated icon strip (fallback / supplement)

After adding or changing images, commit to `main` and push a new version tag (e.g. `v2.4.2`) so WowUp Hub re-indexes the gallery.

## Regenerate icon-bar fallbacks

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /out:"tools\MakeWowUpPreviews.exe" "tools\MakeWowUpPreviews.cs"
& "tools\MakeWowUpPreviews.exe" .
```
