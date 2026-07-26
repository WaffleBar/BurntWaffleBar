# Changelog

All notable changes to BurntWaffleBar are documented here.

## [2.6.12] — 2026-07-26

### Changed
- **The Fire Mage** — rebuilt circular medallion icons as freeform function silhouettes (hood, blades, book, house, flames, scroll, trophy, anvil, constellation, phoenix, twin spirits); kept paw crest + gear

## [2.6.11] — 2026-07-26

### Added
- **The Fire Mage** icon theme — Midnight Fire Mage pack (ember orange, molten bronze, combustion glow); selectable beside The Mage

## [2.6.10] — 2026-07-26

### Changed
- Release bump so WowUp Hub re-indexes GitHub social preview thumbnail art

## [2.6.9] — 2026-07-25

### Changed
- Replaced theme icon-strip WowUp previews with five in-game style gallery mockups (bar, class themes, Edit Mode, settings, clock/queue)

## [2.6.8] — 2026-07-25

### Changed
- Square 1280×1280 social preview for GitHub / WowUp Hub card art (upload under repo **Settings → Social preview**)

## [2.6.7] — 2026-07-25

### Changed
- Author metadata updated to **Waffle**

## [2.6.6] — 2026-07-25

### Changed
- Regenerated WowUp Hub preview gallery (BWB icon + numbered theme bar strips); removed legacy waffle theme preview images

## [2.6.5] — 2026-07-25

### Changed
- New addon icon: BWB monogram on black (replaces legacy waffle artwork)

## [2.6.4] — 2026-07-25

### Fixed
- Queue eye `UpdatePosition` override no longer calls a nil layout helper when the override installs before `ApplyQueueStatusLayout` is defined

## [2.6.3] — 2026-07-25

### Fixed
- Install the queue-eye `UpdatePosition` override when `Blizzard_QueueStatusFrame` loads (covers late frame init)

## [2.6.2] — 2026-07-25

### Fixed
- Queue eye Lua error could still fire when Blizzard called `UpdatePosition` without micro-menu args (nil `offsetX`); override now no-ops safely and installs earlier

## [2.6.1] — 2026-07-25

### Fixed
- Lua error when repositioning the queue eye with the Blizzard micro menu hidden (`UpdatePosition` no longer runs without micro-menu context)

## [2.6.0] — 2026-07-25

### Removed
- Burnt Waffle, Frozen Waffle, and Pristine icon theme packs (class and specialty themes remain)
- Pristine glass clock digit rendering and related tooling/assets

### Changed
- Default manual theme fallback is now The Paladin; removed legacy theme IDs migrate to the player's class theme when auto class themes are enabled

## [2.5.13] — 2026-07-25

### Fixed
- `Fonts.xml` defines all required FontFamily alphabets for Midnight (fixes `missing alphabet 1` on non-English clients)

## [2.5.12] — 2026-07-25

### Fixed
- Queue eye only appears when Blizzard shows it (queued, in instance, etc.); the addon no longer forces it visible at all times

## [2.5.11] — 2026-07-25

### Fixed
- Queue eye lens centering nudged slightly down and left

## [2.5.10] — 2026-07-25

### Fixed
- Queue eye lens centering corrected left after the previous nudge overshot down/right

## [2.5.9] — 2026-07-25

### Fixed
- Queue eye centering inside the Group Finder lens nudged down and right

## [2.5.8] — 2026-07-25

### Fixed
- Queue eye scale inside the Group Finder lens is much larger so it fills the magnifying glass instead of appearing as a tiny dot

## [2.5.7] — 2026-07-25

### Changed
- Queue eye now sits inside the Group Finder magnifying-glass lens, scaled to the icon size instead of perching on the outer edge

## [2.5.6] — 2026-07-25

### Fixed
- Queue eye inset scales with icon size so it sits further inward on the Group Finder icon instead of perching on the outer edge

## [2.5.5] — 2026-07-25

### Fixed
- Queue eye now badges the top-right corner of the Group Finder icon instead of floating above it in the clock row

## [2.5.4] — 2026-07-25

### Fixed
- Queue eye no longer floats into the clock row; it parents to the custom menu bar, sizes to the real button, and sits slightly overlapping the Group Finder icon
- Queue eye management only runs while the Blizzard micro menu is hidden; otherwise Blizzard keeps control

## [2.5.3] — 2026-07-25

### Added
- Re-anchor Blizzard's dungeon finder queue eye (`QueueStatusButton`) above the Group Finder icon when the default micro menu is hidden

## [2.5.2] — 2026-07-25

### Fixed
- `Fonts.xml` wraps custom fonts in `<Member alphabet="roman">` for Midnight (12.0) XML schema

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
