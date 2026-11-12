# Building from source

The plugin is a single .NET 6 class library. It references the game's generated
IL2CPP interop assemblies and the BepInEx core, which are **not** in this
repository (they belong to the game and to BepInEx).

## Prerequisites

* .NET SDK 8 (builds the net6.0 target)
* Football Manager 27 installed (provides the interop assemblies)
* BepInEx 6 IL2CPP installed in the game folder (provides the BepInEx core)

## Build

```sh
export DOTNET_ROOT=$HOME/.dotnet        # or wherever your SDK lives
GAME="$HOME/Library/Application Support/Steam/steamapps/common/Football Manager 27"
BEP="$GAME/FM27-BepInEx/BepInEx"        # macOS; Windows: "<Steam>\...\Football Manager 27\BepInEx"

dotnet build FM26PlayerExport.csproj -c Release \
  -p:CoreDir="$BEP/core" \
  -p:InteropDir="$BEP/interop"
```

The result is `bin/Release/net6.0/FM26PlayerExport.dll`.

On Windows the interop folder is `BepInEx/interop` and the core folder is
`BepInEx/core`, so the same command works with the Windows paths.

## Install the build

Copy `FM26PlayerExport.dll` into `BepInEx/plugins/` (macOS:
`FM27-BepInEx/BepInEx/plugins/`) and start the game through the launcher.

## Notes

* `AssemblyName` is intentionally `FM26PlayerExport` — it keeps the plugin's
  GUID stable, so existing config files keep working.
* `Private=false` on every reference: the game and BepInEx provide those
  assemblies at runtime; do not copy them next to the plugin.
* The plugin targets `net6.0` because that is the runtime the BepInEx 6 IL2CPP
  loader for this game build uses.
