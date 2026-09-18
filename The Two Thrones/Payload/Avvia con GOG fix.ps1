$ErrorActionPreference = 'Stop'
$exe = Join-Path $PSScriptRoot 'pop3_gogfix.exe'
if (-not (Test-Path -LiteralPath $exe)) { & (Join-Path $PSScriptRoot 'gog_fix.ps1') }

# PrinceOfPersia.exe normally owns this mutex while POP3.EXE is running.
$launcherMutex = [System.Threading.Mutex]::new($false, 'POP3Launcher')
try {
    $game = Start-Process -FilePath $exe -ArgumentList '-007' `
        -WorkingDirectory $PSScriptRoot -PassThru
    $game.WaitForExit()
    exit $game.ExitCode
}
finally {
    $launcherMutex.Dispose()
}
