# FulGer Patch

Two editions are included: `Warrior Within` and `The Two Thrones`. Run the
`FulGer Patch.exe` in the matching folder. On the first launch, select the
game installation folder if prompted. The launcher installs or updates its
payload and starts the patched game executable while holding the required
launcher mutexes. The original `pop2.exe` or `pop3.exe` is not changed.

The payload includes the GOG wrapper, the matching widescreen fix, and the
F11 FOV plugin. Press **F11** in game to open **FulGer Patch Options**.
The **FOV multiplier %** row has a slider from 50% to 200% and displays its
current value. Changes apply while playing. Select **Save Options** to store
the setting in `pop2.ini` or `pop3.ini`. The launcher preserves that file on
subsequent launches.

The 100% default is saved internally as `1.001` to keep the widescreen fix's
live update path active. `Xbox_fov` remains a separate game INI setting.

Technical details, a semi-decompiled F11 menu reconstruction, the DLL loading
path, build instructions, and binary-version requirements are in
[Source/README.md](Source/README.md).
