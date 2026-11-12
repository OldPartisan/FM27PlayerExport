# fmscout.com listing draft

**Title:** FM27 Player Export — CSV/HTML table export for macOS & Windows

**Category:** Tools / Editors & Utilities

**Version:** 1.0 (12 Nov 2026)

**Short description (one line):**
Press F9 in Football Manager 27 to dump any visible table — squad, player
search, shortlists, league tables — to CSV and HTML, including CA/PA when the
in-game editor is active.

**Long description:**
FM27 Player Export is a small BepInEx plugin that reads the table currently
on screen and writes it to a semicolon-separated CSV plus a styled HTML file.
It works on macOS (Apple Silicon) and Windows, needs no database editor, and
never touches your saves or the game files.

**Why it exists:** it was built to take squad lists and transfer-market
shortlists out of the game and analyse them with AI — export a CSV, drop it
into a language model (or Excel, pandas, R), and ask real questions: who is
underperforming relative to attributes and minutes, where is the squad weak,
which of 400 scouted names fits the tactical profile and the wage budget.
One key press, one clean file, every column you can see.

* Any table with rows: squad, player search, shortlists, staff, league tables
* Exports exactly the columns you have visible — including hidden CA/PA
  columns when the in-game editor unlocks them
* UTF-8 CSV (Excel/Numbers/Sheets ready) and a browser-friendly HTML copy
* One hotkey: F9 (F10 dumps the UI tree for troubleshooting)
* macOS: pairs with the FM27-BepInEx Apple Silicon package
* Windows: pairs with BepInEx 6 IL2CPP

**Requirements:** Football Manager 27; BepInEx (FM27-BepInEx package on macOS,
BepInEx 6 IL2CPP on Windows).

**Credits:** original FM26 Player Export by vinteset; macOS arm64 fork by TFP
FM contributors / DadMych; FM27 BepInEx for macOS by codexx01; FM27 port and
FM27 grid-UI handler by the community. MIT licence.

**Tags:** FM27, Football Manager 27, export, CSV, HTML, data, statistics,
BepInEx, plugin, macOS, Apple Silicon, Windows, moneyball

**Install summary:** copy `FM26PlayerExport.dll` into the BepInEx `plugins`
folder (macOS: `FM27-BepInEx/BepInEx/plugins`, Windows: `BepInEx/plugins`),
start the game, open a table, press F9. Exports land in
`…/Sports Interactive/Football Manager 27/FM27PlayerExport by vinteset/`.
