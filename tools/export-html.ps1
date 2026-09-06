<#
.SYNOPSIS
    Regenerates every HTML note from its Markdown source and refreshes index.html.

.DESCRIPTION
    Installs the export toolchain on first run (tools/node_modules), then exports
    every .md file in the repository to a sibling .html file, overwriting any
    existing output. Finally, the note catalogue inside index.html is rebuilt
    from whatever is on disk.

.PARAMETER Filter
    Only export Markdown files whose path contains this text.

.PARAMETER SkipIndex
    Export the HTML files but leave index.html untouched.

.PARAMETER IndexOnly
    Skip the export and only rebuild index.html.

.EXAMPLE
    ./tools/export-html.ps1

.EXAMPLE
    ./tools/export-html.ps1 -Filter "05-HighLevelDesign"
#>
[CmdletBinding()]
param(
    [string]$Filter,
    [switch]$SkipIndex,
    [switch]$IndexOnly
)

$ErrorActionPreference = 'Stop'

$toolsDir = $PSScriptRoot

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    throw "Node.js is required but was not found on PATH. Install it from https://nodejs.org/."
}

Push-Location $toolsDir
try {
    if (-not (Test-Path (Join-Path $toolsDir 'node_modules'))) {
        Write-Host 'Installing export toolchain (first run only)...' -ForegroundColor Cyan
        npm install --no-audit --no-fund
        if ($LASTEXITCODE -ne 0) { throw "npm install failed with exit code $LASTEXITCODE." }
    }

    $arguments = @('export-html.cjs')
    if ($Filter) { $arguments += @('--filter', $Filter) }
    if ($SkipIndex) { $arguments += '--skip-index' }
    if ($IndexOnly) { $arguments += '--index-only' }

    & node @arguments
    if ($LASTEXITCODE -ne 0) { throw "HTML export failed with exit code $LASTEXITCODE." }
}
finally {
    Pop-Location
}

Write-Host 'Done.' -ForegroundColor Green
