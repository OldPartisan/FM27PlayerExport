# Why this exists

Football Manager gives you an ocean of numbers and almost no way to take them
with you. The game shows you a table, you study it, and then it is gone —
filtering, sorting, comparing across screens is all locked inside the game.
Screenshots do not scale, and retyping 25 columns of 30 players by hand is not
a workflow.

**FM27 Player Export turns the table on your screen into a file you can think
with.**

The project was created for one specific, very practical use case: **take the
squad list or the transfer-market list out of the game and analyse it with
AI.** Export a CSV, drop it into a language model (or a spreadsheet, or a
notebook), and ask real questions:

* Which players in my squad are underperforming relative to their attributes
  and minutes played?
* Where is my squad structurally weak — by position, by age, by wage share?
* From this scouting list of 400 names, who fits my tactical profile and my
  wage budget?
* What does my transfer shortlist look like if I rank it by output per euro of
  salary?
* How does the league's top scorer list compare with my own forwards, column by
  column?

That is the whole point: **the game produces the data, the model does the
thinking.** Everything in this plugin exists to make that hand-off as short as
possible — one key press, one clean file, every column you can see.

## Who it is for

* Players who like to analyse their save instead of eyeballing it.
* People building moneyball-style shortlists and scouting workflows.
* Anyone who wants to work with FM data in Excel, Numbers, Google Sheets,
  Python/pandas, R, or an AI assistant.
* Modders who need a cheap way to inspect what the game's UI is actually
  showing.

## How it works (short version)

1. You open a table in the game — squad, player search results, shortlist,
   staff list, league table.
2. You press **F9**.
3. The plugin reads the table that is on screen, row by row and column by
   column, exactly as the game renders it, and writes:
   * a semicolon-separated **CSV** (UTF-8, opens in any spreadsheet),
   * a styled **HTML** copy (opens in any browser, easy to read or print).

No database editing, no external tools, no uploading anything anywhere — it
is a local read of the game's own interface.

## Why "whatever is on screen"

The export deliberately mirrors the visible columns instead of hard-coding a
schema. FM27 lets you build your own views, and the community already shares
view presets. That means:

* want CA/PA? unlock the in-game editor and add those columns — they will be
  exported;
* want 40 attributes for a moneyball model? build that view once and export it
  every time;
* want a lean list for a quick AI prompt? keep three columns and export that.

The plugin never assumes which columns matter — you decide by building the
view, and the file follows.

## What it is not

* It is not a database editor and it does not modify your save, your database
  or any game file. It only reads the UI.
* It is not a cheat tool: it exports the same numbers you can already see.
* It is not a live tracker or a data feed — it takes a snapshot of one table
  when you press the key.

## A note on AI workflows

A typical loop looks like this:

1. Export your squad with attributes, minutes, goals, assists, ratings and
   wages.
2. Export the scouting shortlist with the same columns.
3. Ask the model to compare the two, flag stylistic mismatches and rank
   candidates by output per wage euro.
4. Go back into the game, adjust your shortlist, export again — the file is
   always one keystroke away.

The CSV is plain, flat and boring on purpose: that is exactly what makes it
easy to feed into a model or a script without cleaning it first.
