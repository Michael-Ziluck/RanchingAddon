function Get-PackageManifest([string]$PackageFile) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($PackageFile)
    try {
        foreach ($entry in @('manifest.json', 'README.md', 'icon.png')) {
            if (!$zip.GetEntry($entry)) { throw "Missing ZIP entry: $entry" }
        }
        if (!($zip.Entries | Where-Object FullName -match '\.dll$')) { throw 'No plugin DLL in package.' }
        $reader = [IO.StreamReader]::new($zip.GetEntry('manifest.json').Open())
        try { $manifest = $reader.ReadToEnd() | ConvertFrom-Json }
        finally { $reader.Dispose() }
    } finally { $zip.Dispose() }
    if ($manifest.name -notmatch '^[A-Za-z0-9_]+$' -or $manifest.version_number -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw 'Invalid package name or version.'
    }
    return $manifest
}

function Resolve-Package([string]$PackageFile, [string]$Root) {
    if (!$PackageFile) {
        $current = Get-Content (Join-Path $Root 'manifest.json') -Raw | ConvertFrom-Json
        $project = @(Get-ChildItem -LiteralPath $Root -Filter '*.csproj' -File)
        if ($project.Count -ne 1) { throw 'Expected one mod project to determine the ZIP name.' }
        $PackageFile = Join-Path $Root "artifacts/$($project[0].BaseName)-$($current.version_number)-Thunderstore.zip"
    }
    if (!(Test-Path -LiteralPath $PackageFile -PathType Leaf)) { throw 'Build a ZIP first or pass -PackageFile.' }
    return (Resolve-Path -LiteralPath $PackageFile).Path
}
