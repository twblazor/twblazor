using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using System.Text;
using System.Text.RegularExpressions;
using TwBlazor.A11yTests.Infrastructure;

namespace TwBlazor.A11yTests;

/// <summary>
/// Runs an axe-core scan (WCAG 2.1 A/AA rules) against every real, CSS-rendered page in
/// TwBlazor.Docs - one demo page per component, covering every color and state variant shown
/// there - in both light and dark mode. This is a "does the shipped component library have any
/// known accessibility violations" smoke test, not a page-content/copy review of the Docs site.
/// </summary>
[Collection(A11yCollection.Name)]
public partial class AccessibilityScanTests(A11yFixture fixture)
{
    private static readonly string[] _wcagTags = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"];

    // axe no longer reports duplicate ids, but one still breaks things quietly: a label's "for" resolves to
    // the first element with that id, so the control it was written for loses its name.
    private const string duplicateIdsScript =
        "(() => { const seen = new Map(); document.querySelectorAll('[id]').forEach(el => { if (el.id) seen.set(el.id, (seen.get(el.id) ?? 0) + 1); });"
        + " return [...seen].filter(([, count]) => count > 1).map(([id]) => id); })()";

    [Theory]
    [MemberData(nameof(AccessibilityRoutes.LightAndDark), MemberType = typeof(AccessibilityRoutes))]
    public async Task Page_HasNoAxeViolations(string route, bool dark)
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            if (dark)
            {
                // Set this the same way a real visitor would (via localStorage, read by
                // themeToggle.js's init() on load) rather than toggling the "dark" class after the
                // page has already rendered: Chromium doesn't reliably re-run style matching for
                // Tailwind's `:where(.dark, .dark *)` dark-mode selectors against elements that were
                // already computed under a different ancestor class state, which silently left this
                // scan checking light-mode colors and reporting false negatives for dark mode.
                await page.AddInitScriptAsync("localStorage.setItem('theme', 'dark')");
            }

            await page.GotoAsync(new Uri(fixture.BaseAddress, route).ToString());
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var results = await page.RunAxe(new AxeRunOptions
            {
                RunOnly = new RunOnlyOptions { Type = "tag", Values = [.. _wcagTags] }
            });

            if (results.Violations is { Length: > 0 })
            {
                Assert.Fail(FormatViolations(route, dark, results.Violations));
            }

            var duplicateIds = await page.EvaluateAsync<string[]>(duplicateIdsScript);
            Assert.True(duplicateIds.Length == 0, $"Duplicate id(s) on '{route}': {string.Join(", ", duplicateIds)}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static string FormatViolations(string route, bool dark, AxeResultItem[] violations)
    {
        var mode = dark ? "dark" : "light";
        var sb = new StringBuilder();
        sb.AppendLine($"{violations.Length} axe violation(s) on '{route}' ({mode} mode):");

        foreach (var violation in violations)
        {
            sb.AppendLine($"- [{violation.Impact}] {violation.Id}: {violation.Help} ({violation.HelpUrl})");
            foreach (var node in violation.Nodes)
            {
                sb.AppendLine($"    target: {string.Join(" ", node.Target)}");
                sb.AppendLine($"    html:   {node.Html}");

                // violation.Help/Id only say WHICH rule failed (e.g. "color-contrast"), not WHY - the
                // actual numbers (computed ratio, the foreground/background colors involved, what was
                // expected) live in each node's Any/All/None check messages, which axe's own CLI/browser
                // extension surface as "Fix any/all of the following". Without these, a contrast failure
                // gives no way to tell which color token in Theme.cs needs adjusting.
                AppendChecks(sb, "fix any of the following", node.Any);
                AppendChecks(sb, "fix all of the following", node.All);
                AppendChecks(sb, "fix none of the following (must not apply)", node.None);
            }
        }

        return sb.ToString();
    }

    private static void AppendChecks(StringBuilder sb, string label, AxeResultCheck[] checks)
    {
        if (checks.Length == 0) return;

        sb.AppendLine($"    {label}:");
        foreach (var check in checks)
        {
            sb.AppendLine($"      - {check.Message}");
            AppendContrastSuggestion(sb, check.Message);
        }
    }

    // axe-core's color-contrast check message is the only place the actual foreground/background
    // colors and the ratio this page needed to hit are ever surfaced - e.g. "Element has insufficient
    // color contrast of 2.85 (foreground color: #9810fa, background color: #1d232a, font size:
    // 10.5pt (14px), font weight: normal). Expected contrast ratio of 4.5:1". Reusing that instead of
    // recomputing it independently keeps this suggestion talking about the exact colors axe measured.
    [GeneratedRegex(
        @"foreground color:\s*(#[0-9a-fA-F]{3,8}).*?background color:\s*(#[0-9a-fA-F]{3,8}).*?[Ee]xpected contrast ratio of\s*([\d.]+):1",
        RegexOptions.Singleline)]
    private static partial Regex ContrastMessagePattern();

    private static void AppendContrastSuggestion(StringBuilder sb, string message)
    {
        var match = ContrastMessagePattern().Match(message);
        if (!match.Success) return;

        var foreground = match.Groups[1].Value;
        var background = match.Groups[2].Value;
        if (!double.TryParse(match.Groups[3].Value, out var requiredRatio)) return;

        var suggestion = ContrastCalculator.FindMinimumPassingColor(foreground, background, requiredRatio);
        sb.AppendLine(suggestion is { } s
            ? $"        -> {foreground} needs at least {s.LightnessPercent}% lightness (e.g. {s.Hex}) against {background} to reach {requiredRatio}:1"
            : $"        -> no lightness change to {foreground} reaches {requiredRatio}:1 against {background} - this hue/saturation needs to change, not just get lighter/darker");
    }
}
