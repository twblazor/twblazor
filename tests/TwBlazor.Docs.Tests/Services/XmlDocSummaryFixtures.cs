namespace TwBlazor.Docs.Tests.Services;

/// <summary>
/// Property bag whose XML doc comments exercise every branch of <c>XmlDocSummary.BuildSegments</c>:
/// plain text, resolvable and unresolvable &lt;see cref&gt;, &lt;seealso&gt;, &lt;see langword&gt;,
/// paramref/typeparamref, nested &lt;c&gt;/&lt;para&gt; wrappers, and whitespace collapsing/trimming.
/// Reflected over by <c>XmlDocSummaryTests</c> - never instantiated.
/// </summary>
public static class XmlDocSummaryFixtures
{
    /// <summary>Plain text only, no markup.</summary>
    public static string PlainText { get; set; } = string.Empty;

    // Deliberately has no XML doc comment - GetSummaryAsync should surface no segments at all.
    public static string NoSummaryProperty { get; set; } = string.Empty;

    /// <summary>
    /// Links to <see cref="TwBlazor.Enums.Color"/>, a type this docs site actually publishes.
    /// </summary>
    public static string LinkToTwBlazorType { get; set; } = string.Empty;

    /// <summary>
    /// See also <seealso cref="System.String"/>, a BCL type with no page on this site.
    /// </summary>
    public static string LinkToExternalType { get; set; } = string.Empty;

    /// <summary>
    /// Pass <see langword="null"/> to clear the value.
    /// </summary>
    public static string LangwordReference { get; set; } = string.Empty;

    /// <summary>
    /// The <paramref name="value"/> parameter is echoed via <typeparamref name="T"/>.
    /// </summary>
    public static string ParamAndTypeParamRefs { get; set; } = string.Empty;

    /// <summary>
    ///     Collapses    internal   whitespace
    ///     and trims the outer edges.
    /// </summary>
    public static string WhitespaceCollapsing { get; set; } = string.Empty;

    /// <summary>
    /// Leading text before <see cref="TwBlazor.Enums.Color"/> and trailing text after it.
    /// </summary>
    public static string LeadingAndTrailingText { get; set; } = string.Empty;

    /// <summary>
    /// Wrapped in <c>code</c> and <para>a paragraph</para> tags, whose inner text still counts.
    /// </summary>
    public static string NestedInlineElements { get; set; } = string.Empty;
}
