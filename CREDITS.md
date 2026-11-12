# Credits

This package is a community effort. Please keep these credits if you
redistribute or fork it.

## Original plugin

**FM26 Player Export v5.1** by **vinteset** — the original BepInEx plugin that
reads Football Manager UI tables and writes CSV/HTML. Distributed through
[FM Scout](https://www.fmscout.com/) community channels.

## macOS compatibility work (FM26)

**TFP FM contributors / DadMych** — the macOS arm64 fork this port is based on:
`https://github.com/DadMych/fm26-player-export-macos`

That fork added Apple Silicon native execution, scroll stability and retry
logic. Licensed MIT.

## FM27 BepInEx for macOS (not bundled here)

**codexx01** — the community package that makes BepInEx work on FM27's
Unity 6.3:
`https://sortitoutsi.net/content/78080/`

It is **not** included in this download; install it separately. It replaced
the FM26 doorstop with a Unity 6.3-compatible loader, bundled Capstone, and
added the `FM27HookFix` preloader patcher.

## FM27 port (this package)

The FM27 port of the plugin:

* replaced the removed `IL2CPPChainloader.MainThreadTick` with a targeted
  Harmony patch on `FM.GamePlugin.GamePlugin.Tick`;
* replaced the FM26 `UIDocument` root lookup with `SI.UI.DocumentManager.MainRoot`;
* added a dedicated FM27 table handler that reads FM27's new grid UI
  (`columns-root` headers via `TIFO-TableHeaderTemplate`, rows via the
  `View` container and `DragObject` cell container);
* redirected the output folder to the FM27 user directory.

## Frameworks

| Component | Author | Licence |
|---|---|---|
| [BepInEx 6 IL2CPP](https://github.com/BepInEx/BepInEx) | BepInEx team | LGPL-2.1 |
| [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop) | BepInEx team | LGPL-2.1 |
| [UnityDoorstop](https://github.com/NeighTools/UnityDoorstop) | NeighTools | LGPL-3.0 |
| [HarmonyX](https://github.com/BepInEx/HarmonyX) | BepInEx team | MIT |
| [Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) | SamboyCoding | MIT |
| Capstone | Capstone project | BSD-3 |

## Disclaimer

Football Manager is a trademark of Sports Interactive / SEGA. This project is
unofficial and is not affiliated with, endorsed by, or supported by Sports
Interactive or SEGA. Use at your own risk; always keep a backup of your saves.
