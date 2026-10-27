# Computes the next stable release version from the highest existing release
# tag, given a bump kind. Versions live only in Git tags (MinVer derives the
# package version from them), so this is the single place that decides what a
# release is numbered.
#
#   -Bump Major           -> 1.2.3 becomes 2.0.0
#   -Bump Minor (default) -> 1.2.3 becomes 1.3.0
#   -Bump Patch           -> 1.2.3 becomes 1.2.4
#
# When the repository has no release tag at all the base is 0.0.0, so the first
# Minor release is 0.1.0.
#
# -AlsoAbove takes release versions of sibling packages that must stay in step
# with this one (twblazor.templates follows every twblazor release). The bump is
# applied to the highest of this repository's latest tag and those versions, so
# the new version is always above both and the sibling can take the same number.
#   twblazor 3.1.1, templates patched to 3.1.2, -Bump Patch -> 3.1.3
#
# Writes "version" and "tag" to $GITHUB_OUTPUT when running under Actions.
#
# Usage:
#   pwsh ./scripts/next-release-version.ps1 [-Bump Major|Minor|Patch] [-TagPrefix v] [-AlsoAbove v3.1.2]

[CmdletBinding()]
param(
    [ValidateSet('Major', 'Minor', 'Patch')]
    [string]$Bump = 'Minor',
    [string]$TagPrefix = 'v',
    [string[]]$AlsoAbove = @()
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/version-tags.ps1"

$base = Get-LatestReleaseTag -TagPrefix $TagPrefix

if ($null -eq $base) {
    Write-Host "No stable '$TagPrefix<major>.<minor>.<patch>' tags found; treating the base version as 0.0.0."
    $base = [PSCustomObject]@{ Major = 0; Minor = 0; Patch = 0; Tag = $null }
} else {
    Write-Host "Latest release tag: $($base.Tag)"
}

foreach ($other in $AlsoAbove) {
    if ([string]::IsNullOrWhiteSpace($other)) { continue }
    if ($other.Trim() -notmatch '^v?(\d+)\.(\d+)\.(\d+)$') {
        throw "-AlsoAbove value '$other' is not a stable version such as 3.1.2 or v3.1.2."
    }
    $candidate = [PSCustomObject]@{ Major = [int]$Matches[1]; Minor = [int]$Matches[2]; Patch = [int]$Matches[3]; Tag = $other.Trim() }
    $isHigher = ($candidate.Major -gt $base.Major) -or
        ($candidate.Major -eq $base.Major -and $candidate.Minor -gt $base.Minor) -or
        ($candidate.Major -eq $base.Major -and $candidate.Minor -eq $base.Minor -and $candidate.Patch -gt $base.Patch)
    if ($isHigher) {
        Write-Host "$($candidate.Tag) is ahead of $($base.Tag ?? '0.0.0'); bumping from it so both packages can share the new version."
        $base = $candidate
    }
}

switch ($Bump) {
    'Major' { $version = "$($base.Major + 1).0.0" }
    'Minor' { $version = "$($base.Major).$($base.Minor + 1).0" }
    'Patch' { $version = "$($base.Major).$($base.Minor).$($base.Patch + 1)" }
}

$tag = "$TagPrefix$version"
Write-Host "$Bump bump -> $version (tag $tag)"

Set-GitHubOutput -Name 'version' -Value $version
Set-GitHubOutput -Name 'tag'     -Value $tag

Write-Output $version
