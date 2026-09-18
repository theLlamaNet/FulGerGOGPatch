$ErrorActionPreference = 'Stop'
$exe = Join-Path $PSScriptRoot 'pop2_gogfix.exe'
if (-not (Test-Path -LiteralPath $exe)) { & (Join-Path $PSScriptRoot 'gog_fix.ps1') }

# PrinceOfPersia.exe normally owns both mutexes while POP2.EXE is running.
# Keep strong references to them until the game exits: merely constructing
# them without waiting would let PowerShell close them too early.
$launcherMutex = [System.Threading.Mutex]::new($false, 'POP5Launcher')
$watchdogMutex = [System.Threading.Mutex]::new($false, 'POP_Watchdog')
try {
    $game = Start-Process -FilePath $exe -ArgumentList '-007' `
        -WorkingDirectory $PSScriptRoot -PassThru
    $game.WaitForExit()
    exit $game.ExitCode
}
finally {
    $watchdogMutex.Dispose()
    $launcherMutex.Dispose()
}
