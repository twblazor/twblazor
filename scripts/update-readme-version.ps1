# Rewrites every "dotnet add package twblazor --version" install command in the
# given file so it always names the newest published release exactly:
#
#   dotnet add package twblazor --version 1.3.2
#
# Every release moves it, patches included, so neither the README, the NuGet
# package readme nor the Get Started docs page ever names a version older than
# what is on NuGet. README.md and src/TwBlazor/PackageReadme.md carry exactly
# one such line; GetStarted.razor carries two (one per hosting-model tab), so
# the match count is only required to be at least one, not exactly one -
# either way, every match in the file is rewritten.
#
# Versions live only in Git tags, so the release line comes from the newest
# stable tag through the shared helper - the same lookup the publish workflows
# use, so the file cannot name a line that was never released.
#
# -Version names the line explicitly, for the release workflow: at that point
# the version being cut has been decided but not yet tagged, so the tag lookup
# would still answer with the *previous* release.
#
# Writes "version" and "changed" to $GITHUB_OUTPUT when running under Actions.
# Call it once per file when more than one needs updating (see
# publish-release.yml, which runs it for README.md, PackageReadme.md and
# GetStarted.razor).
#
# Usage:
#   pwsh ./scripts/update-readme-version.ps1 [-Path README.md] [-TagPrefix v]
#   pwsh ./scripts/update-readme-version.ps1 -Version 1.4.0
#   pwsh ./scripts/update-readme-version.ps1 -Path docs/TwBlazor.Docs/Pages/GetStarted.razor -Version 1.4.0

[CmdletBinding()]
param(
    [string]$Path = "$PSScriptRoot/../README.md",
    [string]$TagPrefix = 'v',
    [string]$Version
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/version-tags.ps1"

if ($Version) {
    if ($Version -notmatch '^(\d+)\.(\d+)\.\d+') {
        throw "-Version must look like <major>.<minor>.<patch>, got '$Version'."
    }

    $installVersion = $Version
    Write-Host "Release version $Version -> --version $installVersion"
} else {
    $release = Get-LatestReleaseTag -TagPrefix $TagPrefix

    if ($null -eq $release) {
        Write-Host "No stable '$TagPrefix<major>.<minor>.<patch>' tag found; leaving $Path unchanged."
        Set-GitHubOutput -Name 'changed' -Value 'false'
        return
    }

    $installVersion = "$($release.Major).$($release.Minor).$($release.Patch)"
    Write-Host "Latest release tag: $($release.Tag) -> --version $installVersion"
}

$content = Get-Content -Path $Path -Raw

# Deliberately matches whatever the line carries today - the x.x.x placeholder,
# a pinned version, or an older floating one - so re-running is a no-op.
$pattern = '(dotnet add package twblazor --version )\S+'
$found = [regex]::Matches($content, $pattern)

# A silent no-op would be worse than failing: if the install command is ever
# reworded (or removed entirely), this script must say so rather than quietly
# stop maintaining it. At least one match, not exactly one, since
# GetStarted.razor legitimately carries two (Server and WebAssembly tabs).
if ($found.Count -lt 1) {
    throw "Expected at least one 'dotnet add package twblazor --version <version>' line in $Path, found none. Update `$pattern in this script if the install command was reworded."
}

$updated = [regex]::Replace($content, $pattern, "`${1}$installVersion")

Set-GitHubOutput -Name 'version' -Value $installVersion

if ($updated -eq $content) {
    Write-Host "$Path already reads '--version $installVersion'; nothing to do."
    Set-GitHubOutput -Name 'changed' -Value 'false'
    return
}

# -NoNewline because $readme was read with -Raw and already ends in whatever
# trailing newline the file had; Set-Content would otherwise append another.
Set-Content -Path $Path -Value $updated -NoNewline -Encoding utf8

Write-Host "Updated $Path to '--version $installVersion'."
Set-GitHubOutput -Name 'changed' -Value 'true'
