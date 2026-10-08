# Validates a pull request's title and description.
#
# Title must be Conventional Commits, and the subject - the part after
# "<type>(<optional scope>)<optional !>: " - must be 10 to 100 characters and end in a full
# stop. The type prefix does not count toward the length, so the limits measure
# how descriptive the subject actually is rather than how wide it renders:
#
#   chore: fixed versioning.           subject "fixed versioning." (17)  OK
#   chore: typo.                       subject "typo." (5)              too short
#   feat(button): add loading state.   subject "add loading state." (19) OK
#   feat(api)!: remove legacy route.   "!" marks a breaking change      OK
#   fix: something is broken           no full stop                     rejected
#
# Description must follow .github/pull_request_template.md: every required
# heading present, and each section filled in. A section counts as empty when it
# holds nothing but the template's HTML comment and whitespace, so submitting the
# untouched template fails. The checklist must have its "contributing guidelines"
# item ticked - the other items are left to reviewer judgement.
#
# Collects every problem and reports them together, so a contributor fixes one
# round of errors rather than rediscovering them one at a time.
#
# Usage:
#   pwsh ./scripts/validate-pr.ps1 -Title "chore: fixed versioning." -Body "..."

[CmdletBinding()]
param(
    [Parameter(Mandatory)][AllowEmptyString()][string]$Title,
    [AllowEmptyString()][string]$Body = '',

    # Must stay in step with the headings in .github/pull_request_template.md.
    [string[]]$RequiredSections = @('Changes', 'Testing', 'Checklist'),
    [int]$MinSubjectLength = 10,
    [int]$MaxSubjectLength = 100,

    # Substring (case-insensitive) identifying the checklist line that must be
    # ticked. Matched by content rather than full text so rewording the link
    # text in the template doesn't silently break this check.
    [string]$RequiredChecklistItem = 'contributing guidelines'
)

$ErrorActionPreference = 'Stop'

$types = @(
    'feat', 'fix', 'bug', 'docs', 'style', 'refactor',
    'perf', 'test', 'build', 'ci', 'chore', 'revert'
)

$errors = [System.Collections.Generic.List[string]]::new()

# ---------------------------------------------------------------- title ----

$trimmedTitle = $Title.Trim()
Write-Host "Title: $trimmedTitle"

$titlePattern = "^($($types -join '|'))(\([^)]+\))?!?: (.+)$"

if ($trimmedTitle -cmatch $titlePattern) {
    $subject = $Matches[3].Trim()

    if ($subject.Length -lt $MinSubjectLength) {
        $errors.Add("Title subject '$subject' is $($subject.Length) characters; at least $MinSubjectLength are required. Describe what changed, not just that something did.")
    }
    if ($subject.Length -gt $MaxSubjectLength) {
        $errors.Add("Title subject is $($subject.Length) characters; the maximum is $MaxSubjectLength.")
    }
    if (-not $subject.EndsWith('.')) {
        $errors.Add("Title must end with a full stop. Got: '$trimmedTitle'")
    } elseif ($subject.EndsWith('..')) {
        $errors.Add("Title must end with a single full stop, not '..'. Got: '$trimmedTitle'")
    }
} else {
    $errors.Add("Title does not follow Conventional Commits. Expected '<type>(<optional scope>)<optional !>: <subject>.' where type is one of: $($types -join ', '). Got: '$trimmedTitle'")
}

# ----------------------------------------------------------- description ----

# Strip HTML comments before judging emptiness so the template's own prompts do
# not count as content. Done on a copy; headings are located in the raw body.
$normalised = $Body -replace "`r`n", "`n"

if ([string]::IsNullOrWhiteSpace($normalised)) {
    $errors.Add("Description is empty. Fill in the pull request template (Changes, Why, Testing, Checklist).")
} else {
    # Index every "## Heading" so each section can be sliced out by position.
    $headingMatches = [regex]::Matches($normalised, '(?m)^[ \t]*##[ \t]+(.+?)[ \t]*$')

    foreach ($section in $RequiredSections) {
        $match = $headingMatches | Where-Object { $_.Groups[1].Value.Trim() -ieq $section } | Select-Object -First 1

        if ($null -eq $match) {
            $errors.Add("Description is missing the '## $section' section.")
            continue
        }

        # Content runs from the end of this heading to the start of the next one.
        $start = $match.Index + $match.Length
        $next = $headingMatches | Where-Object { $_.Index -gt $match.Index } | Select-Object -First 1
        $end = if ($next) { $next.Index } else { $normalised.Length }
        $content = $normalised.Substring($start, $end - $start)

        $stripped = ($content -replace '(?s)<!--.*?-->', '').Trim()

        if ([string]::IsNullOrWhiteSpace($stripped)) {
            $errors.Add("Section '## $section' is empty. Replace the template comment with real content.")
            continue
        }

        if ($section -ieq 'Checklist') {
            $requiredLine = ($stripped -split "`n") | Where-Object { $_ -match [regex]::Escape($RequiredChecklistItem) } | Select-Object -First 1

            if ($null -eq $requiredLine) {
                $errors.Add("Section '## Checklist' is missing the required '$RequiredChecklistItem' item. Restore it from .github/pull_request_template.md.")
            } elseif ($requiredLine -notmatch '(?im)^\s*[-*]\s*\[x\]') {
                $errors.Add("You must tick the '$RequiredChecklistItem' checklist item before submitting.")
            }
        }
    }
}

# --------------------------------------------------------------- report ----

if ($errors.Count -gt 0) {
    Write-Host ''
    foreach ($e in $errors) {
        Write-Host "::error::$e"
    }
    Write-Host ''
    Write-Host "$($errors.Count) problem(s) found. See .github/pull_request_template.md for the expected description format."
    exit 1
}

Write-Host 'PASS - title and description are valid.'
exit 0
