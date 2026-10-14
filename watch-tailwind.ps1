# TwBlazor.Docs Tailwind CSS Build Script
# Usage:
#   .\watch-tailwind.ps1           - Dev mode: watches for changes (unminified)
#   .\watch-tailwind.ps1 -Release  - Release mode: one-time minified build, then exits
#
# TwBlazor itself ships no pre-built stylesheet - Tailwind can't compile classes from a .DLL, so
# every class its components use is expressed as a literal in src/TwBlazor.Theme/Theme.cs instead,
# and a consuming app builds its own Tailwind output from its own copy of that file (see the
# "Get Started" docs). This script only builds the docs site's own CSS, which scans TwBlazor.Theme
# directly (see docs/TwBlazor.Docs/wwwroot/css/input.css) and so stays fully styled either way.

param(
    [switch]$Release
)

if ($Release) {
    Write-Host "Starting TwBlazor.Docs Tailwind Release Build (minified)..." -ForegroundColor Green
} else {
    Write-Host "Starting TwBlazor.Docs Tailwind Development Watcher..." -ForegroundColor Green
}
Write-Host ""

# Define paths
$twDocsInput = ".\docs\TwBlazor.Docs\wwwroot\css\input.css"
$twDocsOutput = ".\docs\TwBlazor.Docs\wwwroot\css\output.css"

# Verify paths exist
if (-not (Test-Path $twDocsInput)) {
    Write-Host "ERROR: TwBlazor.Docs input.css not found at $twDocsInput" -ForegroundColor Red
    exit 1
}

# Initial build
Write-Host "Running initial Tailwind build..." -ForegroundColor Yellow
$minifyArgs = if ($Release) { @("--minify") } else { @() }
Write-Host "Building TwBlazor.Docs..." -ForegroundColor Cyan
npx @tailwindcss/cli -i $twDocsInput -o $twDocsOutput @minifyArgs

# Each brand-inspired example theme (docs/TwBlazor.Docs/Pages/Showcase) has its own stylesheet, built from its
# own Theme.cs the way a consuming app would build one. They change rarely, so they are built once here and
# not watched: run this script again after editing one of those themes.
$showcaseCss = ".\docs\TwBlazor.Docs\wwwroot\css\showcase"
foreach ($showcaseInput in Get-ChildItem -Path $showcaseCss -Filter "*.input.css") {
    $showcaseOutput = Join-Path $showcaseCss ($showcaseInput.Name -replace "\.input\.css$", ".css")
    Write-Host "Building example theme $($showcaseInput.Name)..." -ForegroundColor Cyan
    npx @tailwindcss/cli -i $showcaseInput.FullName -o $showcaseOutput @minifyArgs
}

Write-Host "Initial build complete!" -ForegroundColor Green
Write-Host ""

if ($Release) {
    Write-Host "Release build complete. Minified output written to:" -ForegroundColor Green
    Write-Host "  $twDocsOutput" -ForegroundColor Cyan
    exit 0
}

# Start the Tailwind watcher as a background process
Write-Host "Starting Tailwind CLI watcher..." -ForegroundColor Yellow

$env:FORCE_COLOR = "0"  # Disable color codes for tailwind output

$twDocsProcess = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npx @tailwindcss/cli -i $twDocsInput -o $twDocsOutput --watch" -NoNewWindow -PassThru

Start-Sleep -Seconds 2  # Give the process time to start

if (-not $twDocsProcess.HasExited) {
    Write-Host "[OK] TwBlazor.Docs watcher started (PID: $($twDocsProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "[WARN] TwBlazor.Docs watcher exited with code $($twDocsProcess.ExitCode)" -ForegroundColor Red
}
Write-Host ""
Write-Host "================================" -ForegroundColor Magenta
Write-Host "  Watching for changes..." -ForegroundColor Magenta
Write-Host "  Refresh browser (Ctrl+F5) to see updates" -ForegroundColor Magenta
Write-Host "  Press Ctrl+C to stop" -ForegroundColor Magenta
Write-Host "================================" -ForegroundColor Magenta
Write-Host ""

# Keep script running and monitor the process
try {
    while ($true) {
        if ($twDocsProcess.HasExited) {
            Write-Host "TwBlazor.Docs watcher exited (code: $($twDocsProcess.ExitCode))! Restarting..." -ForegroundColor Red
            $twDocsProcess = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npx @tailwindcss/cli -i $twDocsInput -o $twDocsOutput --watch" -NoNewWindow -PassThru
        }

        Start-Sleep -Seconds 2
    }
}
finally {
    # Cleanup
    Write-Host ""
    Write-Host "Stopping watcher..." -ForegroundColor Yellow

    # Kill the process tree (cmd.exe -> node) to avoid orphaned processes
    if ($twDocsProcess -and -not $twDocsProcess.HasExited) {
        taskkill /PID $twDocsProcess.Id /T /F 2>$null | Out-Null
    }

    Write-Host "[OK] Cleanup complete" -ForegroundColor Green
}
