@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo ERROR: .NET Framework C# compiler not found at:
    echo %CSC%
    exit /b 1
)

if not exist "Build" mkdir "Build"

"%CSC%" /nologo /target:winexe /reference:System.dll /reference:System.Windows.Forms.dll /out:"Build\FulGer Patch - Warrior Within.exe" "FulGerPatch_WarriorWithin.cs"
if errorlevel 1 exit /b 1

"%CSC%" /nologo /target:winexe /reference:System.dll /reference:System.Windows.Forms.dll /out:"Build\FulGer Patch - The Two Thrones.exe" "FulGerPatch_TheTwoThrones.cs"
if errorlevel 1 exit /b 1

echo Both launchers were built successfully.
exit /b 0
