@echo off
setlocal
call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars32.bat" >nul
if errorlevel 1 exit /b 1
cd /d "%~dp0"
if not exist "Build" mkdir "Build"
cl /nologo /EHsc /O2 /MT /LD /W4 /Fo:"Build\gog_fulger_fov.obj" /Fe:"Build\gog_fulger_fov.dll" gog_fulger_fov.cpp /link /IMPLIB:"Build\gog_fulger_fov.lib"
if errorlevel 1 exit /b 1
copy /y "Build\gog_fulger_fov.dll" "..\Warrior Within\Payload\gog_fulger_fov.dll" >nul
copy /y "Build\gog_fulger_fov.dll" "..\The Two Thrones\Payload\gog_fulger_fov.dll" >nul
