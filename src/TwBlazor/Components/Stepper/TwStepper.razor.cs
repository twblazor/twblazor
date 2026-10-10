// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a step-by-step progress indicator for multi-step flows (e.g. checkout, onboarding
/// wizards), showing which steps are completed, which is active, and which are still upcoming.
/// </summary>
/// <remarks>
/// Add <see cref="TwStep"/> children to declare each step. On <see cref="StepperOrientation.Horizontal"/>
/// (the default), the full step row is only shown from the "sm" breakpoint up; below it, the stepper
/// renders a compact progress bar with a "Step X of N" label instead, so the control stays usable on
/// narrow screens without steps wrapping or overflowing. <see cref="StepperOrientation.Vertical"/>
/// stacks steps top-to-bottom and needs no such collapse, since it never overflows horizontally.
/// </remarks>
public partial class TwStepper : TwBlazorComponentBase
{
    /// <summary>
    /// Distinguishes a step's visual/interactive state, derived purely from its position relative to
    /// <see cref="ActiveStepIndex"/> - see <see cref="GetStatus"/>. Navigating back to an earlier step
    /// re-evaluates every step's status, so a step only stays "completed" while it's still before the
    /// active one - moving back past it reverts it to "upcoming" again.
    /// </summary>
    private enum StepStatus { Completed, Active, Upcoming }

    private readonly List<TwStep> _steps = [];

    private TwStepperTheme theme => options.Theme.Components.Require<TwStepperTheme>();

    /// <summary>
    /// Gets or sets the index (zero-based) of the currently active step.
    /// </summary>
    [Parameter] public int ActiveStepIndex { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the active step changes, enabling two-way binding via
    /// <c>@bind-ActiveStepIndex</c>.
    /// </summary>
    [Parameter] public EventCallback<int> ActiveStepIndexChanged { get; set; }

    /// <summary>
    /// Gets or sets whether steps must be completed in order.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/> (the default), clicking a step's indicator is only allowed for the
    /// active step or one before it - i.e. a step the user can already see is completed - preventing
    /// the user from skipping ahead of steps whose prerequisites haven't been completed. Set to
    /// <see langword="false"/> to let the user freely jump to any non-disabled step by clicking it. This
    /// only restricts user-initiated clicks - <see cref="GoToStep"/> can always move programmatically to
    /// any non-disabled step regardless of this setting.
    /// </remarks>
    [Parameter] public bool Linear { get; set; } = true;

    /// <summary>
    /// Gets or sets the layout direction of the stepper.
    /// </summary>
    [Parameter] public StepperOrientation Orientation { get; set; } = StepperOrientation.Horizontal;

    /// <summary>
    /// Gets or sets the accent color used for the active and completed step indicators and connectors.
    /// </summary>
    /// <remarks>If not set, uses the theme's default primary color.</remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="TwStep"/> children declaring each step.
    /// </summary>
    [Parameter] public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Gets the currently active step, or <see langword="null"/> if <see cref="ActiveStepIndex"/> is out
    /// of range (e.g. before any steps have registered).
    /// </summary>
    public TwStep? ActiveStep => ActiveStepIndex >= 0 && ActiveStepIndex < _steps.Count ? _steps[ActiveStepIndex] : null;

    /// <summary>
    /// Gets whether the active step is the first step.
    /// </summary>
    public bool IsFirstStep => ActiveStepIndex <= 0;

    /// <summary>
    /// Gets whether the active step is the last step.
    /// </summary>
    public bool IsLastStep => ActiveStepIndex >= _steps.Count - 1;

    // Announced when the active step changes: the step's content is replaced without focus moving to it.
    private string statusMessage = string.Empty;

    private int mobileProgressValue => _steps.Count == 0 ? 0 : (int)Math.Round((ActiveStepIndex + 1) / (double)_steps.Count * 100);

    private string mobileAriaLabel => ActiveStep != null
        ? $"Step {ActiveStepIndex + 1} of {_steps.Count}: {ActiveStep.Label}"
        : string.Empty;

    internal void RegisterStep(TwStep step)
    {
        if (_steps.Contains(step))
        {
            return;
        }

        _steps.Add(step);
        StateHasChanged();
    }

    /// <summary>
    /// Advances to the next enabled step, if there is one.
    /// </summary>
    public Task NextStep()
    {
        for (var index = ActiveStepIndex + 1; index < _steps.Count; index++)
        {
            if (!_steps[index].Disabled)
            {
                return GoToStep(index);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Moves back to the previous enabled step, if there is one.
    /// </summary>
    public Task PreviousStep()
    {
        for (var index = ActiveStepIndex - 1; index >= 0; index--)
        {
            if (!_steps[index].Disabled)
            {
                return GoToStep(index);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Moves directly to the step at <paramref name="index"/>, if it exists, isn't disabled, and isn't
    /// already active.
    /// </summary>
    /// <remarks>
    /// Unlike clicking a step's indicator, this always succeeds for any in-range, non-disabled step,
    /// regardless of <see cref="Linear"/> - call this from your own "Next"/"Back" buttons, or after your
    /// own validation, rather than to bypass Linear on the user's behalf from a step click handler.
    /// </remarks>
    public async Task GoToStep(int index)
    {
        if (index < 0 || index >= _steps.Count || _steps[index].Disabled || index == ActiveStepIndex)
        {
            return;
        }

        ActiveStepIndex = index;
        statusMessage = $"Step {index + 1} of {_steps.Count}: {_steps[index].Label}";
        await ActiveStepIndexChanged.InvokeAsync(index);
    }

    /// <summary>
    /// Handles a user click on a step's indicator button, respecting <see cref="Linear"/>.
    /// </summary>
    private Task OnStepClicked(TwStep step) => CanActivate(step) ? GoToStep(_steps.IndexOf(step)) : Task.CompletedTask;

    /// <summary>
    /// Gets whether <paramref name="step"/> can currently be navigated to by clicking its indicator.
    /// </summary>
    private bool CanActivate(TwStep step) => !step.Disabled && (!Linear || _steps.IndexOf(step) <= ActiveStepIndex);

    private StepStatus GetStatus(TwStep step)
    {
        var index = _steps.IndexOf(step);
        if (index == ActiveStepIndex)
        {
            return StepStatus.Active;
        }

        return index < ActiveStepIndex ? StepStatus.Completed : StepStatus.Upcoming;
    }

    private string CircleClasses(TwStep step, StepStatus status) => new ClassBuilder(theme.Circle)
        .AddClass(colorBuilder.GetFilledVariantColor(Color), status is StepStatus.Active or StepStatus.Completed)
        .AddClass(theme.CircleUpcoming, status == StepStatus.Upcoming)
        .AddClass(theme.CircleDisabled, step.Disabled)
        .AddClass(theme.CircleClickable, CanActivate(step))
        .AddClass(colorBuilder.GetFocusRing(Color))
        .Build();

    private string LabelClasses(StepStatus status) => status == StepStatus.Upcoming ? theme.LabelUpcoming : theme.Label;

    private string ConnectorClasses(int beforeIndex) => new ClassBuilder(theme.Connector)
        .AddClass(colorBuilder.GetBorderColor(Color), beforeIndex < ActiveStepIndex)
        .AddClass(theme.ConnectorNeutral, beforeIndex >= ActiveStepIndex)
        .Build();

    private string VerticalConnectorClasses(int beforeIndex) => new ClassBuilder(theme.VerticalConnector)
        .AddClass(colorBuilder.GetBorderColor(Color), beforeIndex < ActiveStepIndex)
        .AddClass(theme.ConnectorNeutral, beforeIndex >= ActiveStepIndex)
        .Build();

    private static string StepAriaLabel(TwStep step, int index, StepStatus status)
    {
        var stateText = status switch
        {
            StepStatus.Completed => "completed",
            StepStatus.Active => "current",
            _ => "not started"
        };

        return $"Step {index + 1}: {step.Label}, {stateText}";
    }
}
