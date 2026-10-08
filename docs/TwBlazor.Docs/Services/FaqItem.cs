namespace TwBlazor.Docs.Services;

/// <summary>
/// One question and its plain-text answer, shown on a docs page and mirrored into its FAQPage JSON-LD.
/// </summary>
/// <param name="Question">The question, phrased the way people search for it.</param>
/// <param name="Answer">The answer as plain text, so the page and the structured data say the same thing.</param>
public sealed record FaqItem(string Question, string Answer);
