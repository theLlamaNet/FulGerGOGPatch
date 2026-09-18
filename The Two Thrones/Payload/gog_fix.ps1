$ErrorActionPreference = 'Stop'
$game = 'Prince of Persia: The Two Thrones'
$sourceExe = Join-Path $PSScriptRoot 'pop3.exe'
$outputExe = Join-Path $PSScriptRoot 'pop3_gogfix.exe'
$expectedSha = '5F48434CC412AA776CF995ED6CC215C258C5D4704C44748E356408AA2AD3A4AA'

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
if (-not (Test-Path -LiteralPath $sourceExe)) { throw "pop3.exe non trovato in $PSScriptRoot" }
$actualSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourceExe).Hash
if ($actualSha -ne $expectedSha) {
    throw "Versione pop3.exe non supportata. SHA256: $actualSha (atteso: $expectedSha)"
}
$bytes = [IO.File]::ReadAllBytes($sourceExe)
Replace-Import $bytes 'd3d9.dll' 'dx.dll'
Replace-Import $bytes 'dinput8.dll' 'di.dll'
[IO.File]::WriteAllBytes($outputExe, $bytes)
Write-Host "[OK] Creato: $outputExe" -ForegroundColor Green
Write-Host "Avvia con: .\Avvia con GOG fix.ps1"
