# Changelog

## v1.1 — 2026-11-14

**Fixed: export captured only the rows visible on screen.**

Long lists in FM27 are virtualised — the table container only holds the rows
that are currently rendered, so a squad or transfer list of 60-70 players
exported as the ~15 rows that happened to fit on the screen.

* Added scroll capture to `Fm27GridTableHandler`:
  * finds the table's `ScrollView` via `GetFirstAncestorOfType<ScrollView>()`
    and resets it to the top;
  * scrolls one third of the viewport per step (a full viewport jumps too far
    and can skip rows);
  * waits `ScrollStepDelayFrames` (default 30) between steps so the virtualised
    list can render;
  * accumulates rows with de-duplication by row signature, so overlapping
    rows are not written twice;
  * stops when `scrollOffset` no longer changes, with a 300-step safety cap;
  * writes the CSV/HTML from the accumulated rows.
* If the `ScrollView` cannot be found, the plugin logs a warning and exports
  whatever is visible instead of failing.
* Plugin version bumped to 5.2.0.

## v1.0 — 2026-11-12

First public FM27 build.

* Ported the FM26 Player Export plugin to Football Manager 27
  (Unity 6000.3.18f1, IL2CPP, universal binary).
* Replaced the removed `IL2CPPChainloader.MainThreadTick` with a targeted
  Harmony postfix on `FM.GamePlugin.GamePlugin.Tick` — this was the fix that
  stopped FM27 from hanging at 0% CPU on startup.
* Replaced the FM26 `UIDocument` root lookup with
  `SI.UI.DocumentManager.MainRoot`.
* Added a dedicated FM27 handler (`Fm27GridTableHandler`):
  * headers from `TIFO-TableHeaderTemplate` elements,
  * rows from the `View` container (largest child count),
  * cells from the `DragObject` child container,
  * checkbox/position column alignment when the first header is empty,
  * text collected from the whole cell subtree (FM27 nests labels several
    levels deep).
* CSV is written with cell order matching the rendered row, so columns line up
  with headers.
* HTML export uses the same cell container as the CSV.
* Output folder moved to `…/Sports Interactive/Football Manager 27/FM27PlayerExport by vinteset/`.
* Kept the FM26 handlers (player, staff, generic scrolled table) as fallbacks.

### Known limitations

* The HTML and CSV contain the same data; the HTML is a plain readable table,
  not a styled report.
* The plugin reads the visible table only — it does not page through screens
  that are not rendered.
