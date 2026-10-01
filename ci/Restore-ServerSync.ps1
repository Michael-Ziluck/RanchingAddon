[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$spec = (Get-Content (Join-Path $PSScriptRoot 'dependencies.json') -Raw | ConvertFrom-Json).serverSync
$directory = Join-Path $root ".local/dependencies/ServerSync/$($spec.version)"
$file = Join-Path $directory 'ServerSync.dll'
if ((Test-Path -LiteralPath $file) -and (Get-FileHash -LiteralPath $file).Hash -eq $spec.sha256) { return }
New-Item -ItemType Directory -Path $directory -Force | Out-Null
$temporary = Join-Path $directory ('download-' + [guid]::NewGuid().ToString('N') + '.tmp')
try {
    Invoke-WebRequest -Uri $spec.url -OutFile $temporary
    if ((Get-FileHash -LiteralPath $temporary).Hash -ne $spec.sha256) { throw 'ServerSync download checksum mismatch.' }
    Move-Item -LiteralPath $temporary -Destination $file -Force
    Write-Host "Restored ServerSync $($spec.version)."
} finally {
    if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary }
}
