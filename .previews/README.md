# WowUp Hub preview images

WowUp Hub reads PNG/JPG files from this folder when you publish a **tagged release** from the same branch (see [WowUp Hub guide](https://wowup.io/guide/wowup/hub#image-previews)).

## What shows where

| Location | Source |
|----------|--------|
| **Addon card thumbnail** | GitHub repo **Social preview** (Settings → Social preview) |
| **Previews gallery** | Every image in this `.previews/` folder at release time |

This folder is **not** packaged into the WoW addon zip (see `.pkgmeta`).

## Files

- `01-addon-icon.png` — addon icon (BWB monogram)
- `02-the-paladin.png` … `14-the-evoker.png` — theme bar strips (auto-generated)

After adding or changing images, commit to `main` and push a **new version tag** so WowUp Hub re-indexes the gallery. Previews are snapshotted at tag time only.

## Regenerate gallery

```powershell
$csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /out:"tools\MakeWowUpPreviews.exe" "tools\MakeWowUpPreviews.cs"
& "tools\MakeWowUpPreviews.exe" .
```

Also writes `.github/social-preview.png` (1280×640) for GitHub / WowUp card art — upload it under repo **Settings → Social preview** if WowUp still shows your GitHub avatar.
