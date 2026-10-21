// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Localization;

namespace TwBlazor.Components;

/// <summary>
/// The error summary of a <see cref="TwForm"/>: a box above the fields that lists every error once the form has
/// been submitted, each linking to its field.
/// </summary>
/// <remarks>
/// It renders on its own as the form's validation state changes, so the fields around it do not have to render
/// again each time.
/// </remarks>
internal sealed class TwFormSummary : ComponentBase, IDisposable
{
    private const string classAttribute = "class";

    private EditContext? subscribedContext;

    [Inject] private ITwLocalizer localizer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the form the summary belongs to.
    /// </summary>
    [Parameter, EditorRequired] public TwForm Form { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (ReferenceEquals(subscribedContext, Form.CurrentEditContext))
        {
            return;
        }

        Unsubscribe();
        subscribedContext = Form.CurrentEditContext;
        subscribedContext.OnValidationStateChanged += HandleValidationStateChanged;
        Form.Validated += HandleValidated;
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Form.HasValidated)
        {
            return;
        }

        var entries = Form.GetSummaryEntries();
        if (entries.Count == 0)
        {
            return;
        }

        var theme = Form.formTheme;
        var titleId = $"{Form.summaryId}-title";

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Form.summaryId);
        builder.AddAttribute(2, classAttribute, theme.Summary);
        builder.AddAttribute(3, "role", "alert");
        builder.AddAttribute(4, "tabindex", "-1");
        builder.AddAttribute(5, "aria-labelledby", titleId);

        builder.OpenElement(6, "h2");
        builder.AddAttribute(7, "id", titleId);
        builder.AddAttribute(8, classAttribute, theme.SummaryTitle);
        builder.AddContent(9, localizer[TwStrings.Form_SummaryTitle]);
        builder.CloseElement();

        builder.OpenElement(10, "ul");
        builder.AddAttribute(11, classAttribute, theme.SummaryList);
        foreach (var (message, fieldId) in entries)
        {
            builder.OpenElement(12, "li");
            if (fieldId is not null)
            {
                builder.OpenElement(13, "a");
                builder.AddAttribute(14, "href", $"#{fieldId}");
                builder.AddAttribute(15, classAttribute, theme.SummaryLink);
                builder.AddAttribute(16, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => Form.FocusFieldAsync(fieldId)));
                builder.AddEventPreventDefaultAttribute(17, "onclick", true);
                builder.AddContent(18, message);
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(19, message);
            }
            builder.CloseElement();
        }
        builder.CloseElement();

        builder.CloseElement();
    }

    /// <summary>
    /// Stops listening to the form.
    /// </summary>
    public void Dispose() => Unsubscribe();

    private void Unsubscribe()
    {
        if (subscribedContext is not null)
        {
            subscribedContext.OnValidationStateChanged -= HandleValidationStateChanged;
            Form.Validated -= HandleValidated;
            subscribedContext = null;
        }
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) => _ = InvokeAsync(StateHasChanged);

    private void HandleValidated() => _ = InvokeAsync(StateHasChanged);
}
