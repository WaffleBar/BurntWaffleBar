# Changelog

All notable changes to BurntWaffleBar are documented here.

## [2.1.0] — 2026-07-24

### Changed
- **Class theme props** — all 11 class packs now use procedurally drawn icons with unique class weapons and emblems (shields, bows, daggers, totems, etc.) instead of recolored Illidari silhouettes

## [2.0.0] — 2026-07-24

### Added
- **Class icon theme packs** — 11 new themes (Warrior, Hunter, Rogue, Priest, Shaman, Mage, Warlock, Monk, Druid, Death Knight, Evoker) using Illidari-style silhouettes with official class color palettes
- **ClassThemeBuilder** — `tools/ClassThemeBuilder.cs` generates sources and production icons from The Illidari templates

## [1.8.5] — 2026-07-24

### Changed
- **WowUp Hub** — BurntWaffleBar is now listed for search and updates via WowUpHub

## [1.8.4] — 2026-07-24

### Changed
- **Addon icon** — dedicated `Media/AddonIcon.png` with solid black background for addon managers (WowUp, in-game addon list)

## [1.8.3] — 2026-07-24

### Removed
- **Spooky Waffle** theme — removed from the addon pending a future rework

## [1.8.2] — 2026-07-24

### Changed
- **Spooky Waffle v2 sources regenerated** — solid opaque stone/metal bodies (no hollow wire frames); processor fills remaining interior gaps with purple crust or green glow

## [1.8.1] — 2026-07-24

### Fixed
- **Icon tooltips** — buttons are now parented correctly for mouse hit-testing; tooltips anchor above each icon on hover
- **Spooky Waffle sizing** — icons normalized to The Paladin footprints with tighter source crop and 98% fill

## [1.8.0] — 2026-07-24

### Added
- **Icon tooltips** — hover any menu icon or the clock to see what it opens (toggle in `/bw` → Show Icon Tooltips)

### Changed
- **Spooky Waffle v2** — fully rebuilt in Paladin-quality style with spooky/warlock palette (obsidian, void purple, sickly green glow). New source art for all 12 icons; Paladin-style processing pipeline

## [1.7.4] — 2026-07-24

### Fixed
- **Spooky Waffle** — reverted broken SDF/cyan rebuild; icons now use the same minimal pipeline as Burnt Waffle (scale + background key only) from the original spooky source art

## [1.7.3] — 2026-07-24

### Fixed
- **Spooky Waffle** icon sizing normalized to Burnt Waffle footprints so all 12 buttons match in the bar

## [1.7.2] — 2026-07-24

### Fixed
- **Spooky Waffle** icons looked thinned out in the bar — reprocessed with tight-crop and proper fill scaling (~92% vs ~66%)

## [1.7.1] — 2026-07-24

### Added
- WoWUp release workflow and `.pkgmeta` packaging

## [1.7.0] — 2026-07-24

### Added
- **The Illidari** theme — 12 Demon Hunter / Illidari icon pack with fel-green and clock-purple palette
- **Edit Mode support** — drag the bar in WoW's native Edit Mode (`/editmode`); positions save per layout
- Embedded **LibEditMode** so users don't need a separate library addon
- All 12 menu buttons use reliable click handlers (works with native micro menu hidden)

### Fixed
- Group Finder button opening PvP instead of Dungeon Finder
- Non-functional menu buttons when Blizzard micro menu was hidden
- Illidari PVP icon style and warglaive silhouette

### Themes included
- Burnt Waffle, Pristine, Frozen Waffle, The Paladin, The Illidari

## Earlier development

Prior versions were developed locally before the initial GitHub release.
