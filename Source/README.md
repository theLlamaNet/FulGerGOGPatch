# FulGer Patch launcher sources

The two launchers intentionally use the same source code. Game-specific
behavior is supplied by the `FulGerPatch.ini` file located next to each
compiled executable.

- `FulGerPatch_WarriorWithin.cs` builds the Warrior Within launcher.
- `FulGerPatch_TheTwoThrones.cs` builds the The Two Thrones launcher.
- `build.bat` compiles both Windows executables with the .NET Framework 4
  C# compiler included with Windows.

The generated executables are written to the `Build` subfolder.
