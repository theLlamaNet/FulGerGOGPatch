Prince of Persia: The Two Thrones - FulGer compatibility fix

This adaptation uses the generic wrappers from the GOG Preservation update
for The Sands of Time. It intentionally does not load gog_pop1.dll, which is
specific to and compatible only with The Sands of Time.

Features: FPS limiting, cursor confinement, Direct3D 9 and DirectInput 8
wrappers, and an optional CPU core limit configured through gog.toml.

F11 opens "FulGer Patch Options". The FOV plugin loads the matching pop3w.dll
widescreen fix, provides a 50-200% slider, and saves the setting to pop3.ini
when "Save Options" is selected.

The external FulGer Patch.exe launcher replaces PrinceOfPersia.exe: it keeps
the POP3Launcher mutex alive, starts pop3_gogfix.exe -007, and waits until the
game closes.

The original pop3.exe is not modified.
