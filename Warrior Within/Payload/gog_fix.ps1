$ErrorActionPreference = 'Stop'
$game = 'Prince of Persia: Warrior Within'
$sourceExe = Join-Path $PSScriptRoot 'pop2.exe'
$outputExe = Join-Path $PSScriptRoot 'pop2_gogfix.exe'
$expectedSha = '48D6D9F9ED3FE4E07C4B90B30527FB7724560DEB0743A38C852A8051CF964E3D'

function Find-Pattern([byte[]]$Data, [byte[]]$Pattern) {
    $hits = [System.Collections.Generic.List[int]]::new()
    for ($i = 0; $i -le $Data.Length - $Pattern.Length; $i++) {
        $match = $true
        for ($j = 0; $j -lt $Pattern.Length; $j++) {
            if ($Data[$i + $j] -ne $Pattern[$j]) { $match = $false; break }
        }
        if ($match) { $hits.Add($i) }
    }
    return $hits.ToArray()
}

function Replace-Import([byte[]]$Data, [string]$Old, [string]$New) {
    $oldBytes = [Text.Encoding]::ASCII.GetBytes($Old)
    $newBytes = [Text.Encoding]::ASCII.GetBytes($New)
    $hits = @(Find-Pattern $Data $oldBytes)
    if ($hits.Count -ne 1) { throw "Import '$Old' trovato $($hits.Count) volte; patch annullata." }
    [Array]::Clear($Data, $hits[0], $oldBytes.Length)
    [Array]::Copy($newBytes, 0, $Data, $hits[0], $newBytes.Length)
    Write-Host "[OK] $Old -> $New (offset 0x$($hits[0].ToString('X')))"
}

Write-Host "=== $game - GOG wrapper compatibility fix ===" -ForegroundColor Cyan
if (-not (Test-Path -LiteralPath $sourceExe)) { throw "pop2.exe non trovato in $PSScriptRoot" }
$actualSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourceExe).Hash
if ($actualSha -ne $expectedSha) {
    throw "Versione pop2.exe non supportata. SHA256: $actualSha (atteso: $expectedSha)"
}
$bytes = [IO.File]::ReadAllBytes($sourceExe)
Replace-Import $bytes 'd3d9.dll' 'dx.dll'
Replace-Import $bytes 'DINPUT8.dll' 'di.dll'
[IO.File]::WriteAllBytes($outputExe, $bytes)
Write-Host "[OK] Creato: $outputExe" -ForegroundColor Green
Write-Host "Avvia con: .\Avvia con GOG fix.ps1"
