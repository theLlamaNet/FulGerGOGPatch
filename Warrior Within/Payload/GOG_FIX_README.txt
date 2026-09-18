Prince of Persia: Warrior Within - FulGer compatibility fix

This adaptation uses the generic wrappers from the GOG Preservation update
for The Sands of Time. It intentionally does not load gog_pop1.dll, which is
specific to and compatible only with The Sands of Time.

Features: FPS limiting, cursor confinement, Direct3D 9 and DirectInput 8
wrappers, and an optional CPU core limit configured through gog.toml.

The external FulGer Patch.exe launcher replaces PrinceOfPersia.exe: it keeps
the POP5Launcher and POP_Watchdog mutexes alive, starts
pop2_gogfix.exe -007, and waits until the game closes.

The original pop2.exe is not modified.
