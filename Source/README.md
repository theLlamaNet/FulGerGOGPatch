# FulGer Patch source and binary map

## Launcher

`FulGerPatch_WarriorWithin.cs` and `FulGerPatch_TheTwoThrones.cs` are identical
sources compiled separately by `build.bat` with the Windows .NET Framework 4
C# compiler. Each launcher reads the adjacent `FulGerPatch.ini`, locates the
game by `OriginalExecutable`, copies every `Payload` file into the game folder,
holds the configured launcher mutexes, and starts `GameExecutable` with
`Arguments`. The launcher waits until the game exits.

`FovIni` names `pop2.ini` or `pop3.ini`. On first installation the launcher
copies that file and sets `fov_multiplier` from `FovMultiplier` (0.5-2.0).
Later launches leave an existing game INI untouched, preserving F11 changes.
The special value 1.001 represents the displayed 100%: the widescreen fix
uses a separate static path at exactly 1.0, which would bypass live updates.

## DLL loading and F11 menu

The patched POP2/POP3 game executables import `dx.dll` and `di.dll`, the GOG
Direct3D 9 and DirectInput 8 wrappers. Those wrappers initialize
`gog_core.dll`; the core uses SDL3 and tomlplusplus, reads `gog.toml`, and
draws the F11 options window through its exported Nuklear functions. It scans
`gog_*.dll` plugins in the game folder. The separate `gog_pop1.dll` from
Sands of Time is game-specific and is not shipped here.

The plugin ABI was reconstructed from the bundled `gog_pop1.dll`. The four
exports are `pluginName`, `pluginCoreVersionCompatible`,
`pluginGameVersionCompatible`, and `pluginGet`. `pluginGet` returns an object
with virtual callbacks for options loading, core initialization, resolution,
game frames, options drawing, options saving, and shutdown. The core v1 plugin
interface and game CRC32 checks are implemented in `gog_fulger_fov.cpp`.

The title is owned by `gog_core.dll`, not by the FOV plugin. The shipped
32-bit core stores it in a 24-byte slot at file offset 273572 (RVA 0x43AA4).
`patch-gog-core-title.py` verifies the original SHA-256 and changes only that
slot from `FulGer GOG Options` to `FulGer Patch Options`; the next string,
`Limit FPS`, remains at RVA 0x43ABC. The script is idempotent.
`gog_core_f11_reconstruction.cpp` is a semi-decompiled description of this
menu path with checked addresses and explicitly marked pseudocode. It does
not claim to recreate the entire proprietary core.

## FOV plugin and widescreen fix

`gog_fulger_fov.cpp` builds the 32-bit `gog_fulger_fov.dll`. The GOG Direct3D
wrapper loads system Direct3D itself, so the widescreen fix's `d3d9.dll`
proxy is not reached in this launch path. At `pluginGet`, the plugin loads
the matching `pop2w.dll` or `pop3w.dll` directly. The plugin accepts only the
bundled POP2/POP3 executable CRC32 values `77DA8770` and `A2538C99`.

The two widescreen fix DLLs have the same data offsets but different game
hook locations. The plugin reads the fix's multiplier at RVA 0x191E8 and its
aspect factor at RVA 0x1A18C. On slider changes it updates the projection
factors at RVAs 0x1A178, 0x1A1A4, 0x1A1A0, 0x1A190, 0x1A170, and 0x1A19C.
These offsets apply only to the supplied `pop2w.dll` and `pop3w.dll` copies.
`Xbox_fov` stays independently configurable in the game INI.

The F11 UI uses `nk_layout_row_dynamic(ctx, 0, 3)`, `nk_label`,
`nk_slider_int`, and `nk_labelf` to match the Sands of Time menu's
label/slider/percentage row. The range is 50-200% in 1% steps. Changing the
slider updates the fix's in-memory factors. `Save Options` invokes the plugin
save callback, which writes `fov_multiplier` in `pop2.ini` or `pop3.ini`.

## Rebuild

1. Run `build.bat` and copy the two resulting launchers from `Build` into
   their matching game folders.
2. Run `build-fov-plugin.bat` from an installed Visual Studio C++ toolchain.
   It builds an x86 DLL in `Build` and copies it into both payloads.
3. Run `py patch-gog-core-title.py` once on unmodified bundled core DLLs.

The executable and widescreen-fix binaries are distributed payloads, not
built by these scripts. Keep the supplied versions together; the FOV plugin
depends on their exact ABI and data layout.
