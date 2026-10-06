// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a file upload input component that allows users to select and upload files. This component provides properties for handling file selection events and configuring multiple file selection. It inherits from <see cref="TwBlazorInputComponentBase"/>, which provides common input component functionality such as labeling and disabled state management.
/// </summary>
public partial class TwFileUpload : TwBlazorInputComponentBase
{
    [Inject] private ButtonBuilder buttonBuilder { get; set; } = null!;

    private TwButtonTheme theme => options.Theme.Components.Require<TwButtonTheme>();

    private TwFileUploadTheme fileUploadTheme => options.Theme.Components.Require<TwFileUploadTheme>();

    /// <summary>
    /// The icon displayed at the start of the input, the default is <see cref="Icon.Cloud_Upload"/>, if you would like this blank explicity set this to null.
    /// </summary>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// The button variant style applied to the upload input.
    /// </summary>
    [Parameter] public ButtonVariant? Variant { get; set; }

    // The color applied to the upload input.
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file input should allow multiple file selection. When true, the user can select more than one file; when false, only a single file can be selected.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Optional list of allowed file types (MIME types or extensions) to restrict selectable files in the UI.
    /// Example values: new[]{".png",".jpg","image/pdf","application/pdf"}
    /// These values will be joined and applied to the input's "accept" attribute.
    /// </summary>
    [Parameter] public IEnumerable<string>? AllowedFileTypes { get; set; }

    private string? acceptAttr => AllowedFileTypes != null && AllowedFileTypes.Any()
        ? string.Join(",", AllowedFileTypes)
        : null;

    private List<IBrowserFile> selectedFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets the event callback that is invoked when the user selects one or more files using the file input.
    /// </summary>
    [Parameter] public EventCallback<InputFileChangeEventArgs> OnChange { get; set; }

    /// <summary>
    /// Two-way bindable file list. Parent can bind this with @bind-Files to keep synchronized.
    /// </summary>
    [Parameter] public List<IBrowserFile>? Files { get; set; }
    [Parameter] public EventCallback<List<IBrowserFile>?> FilesChanged { get; set; }

    private string rootClasses =>
        new ClassBuilder(RootClass)
        .AddClass(roundedBuilder.GetRounded())
        .AddClass(buttonBuilder.GetTypographyClasses(theme.ButtonUppercase))
        .Build();

    // The real <InputFile> is visually hidden (sr-only, not display:none) so it stays keyboard-focusable
    // and in the accessibility tree; this visible label doubles as its focus indicator via peer-focus-visible,
    // since the native input's own focus ring would otherwise land somewhere invisible.
    private string classes =>
        new ClassBuilder(options.Theme.Display.Block)
        .AddClass(Class)
        .AddClass(LabelClasses)
        .AddClass(roundedBuilder.GetRounded())
        .AddClass(buttonBuilder.GetVariantClasses(Variant, Color, Disabled))
        .AddClass(shadowBuilder.GetButtonShadow(theme))
        .AddClass(colorBuilder.GetPeerFocusRing(Color))
        .AddClass(options.Theme.Spacing.Padding.Lg).Build();

    private string inputClasses =>
        new ClassBuilder(options.Theme.Display.ScreenReaderOnly)
        .AddClass("peer").Build();

    private string fileListClasses =>
        new ClassBuilder(fileUploadTheme.FileList)
        .AddClass(options.Theme.Spacing.MarginTop.Lg)
        .AddClass(options.Theme.Spacing.Gap.Sm)
        .AddClass(options.Theme.Display.Block).Build();

    private string chipClasses =>
        new ClassBuilder(fileUploadTheme.IconSpacing)
        .AddClass(options.Theme.Spacing.MarginBottom.Md)
        .AddClass(fileUploadTheme.ChipTextColor).Build();


    /// <summary>
    /// When files are selected, store both the display names and the actual IBrowserFile objects
    /// so they can be removed later. Also invoke FilesChanged with the current file list.
    /// </summary>
    private async Task OnUpload(InputFileChangeEventArgs filesChangeEvent)
    {
        if (ReadOnly || Disabled)
            return;

        selectedFiles.Clear();

        if (!Multiple || (filesChangeEvent.FileCount > 0 && filesChangeEvent.FileCount <= 1))
        {
            selectedFiles.Add(filesChangeEvent.File);
        }
        else
        {
            foreach (var f in filesChangeEvent.GetMultipleFiles())
            {
                selectedFiles.Add(f);
            }
        }

        // Notify existing OnChange if provided (keeps backward compatibility)
        if (OnChange.HasDelegate)
        {
            await OnChange.InvokeAsync(filesChangeEvent);
        }

        // Support two-way binding via Files / FilesChanged
        if (FilesChanged.HasDelegate)
        {
            await FilesChanged.InvokeAsync(selectedFiles);
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // A file input with no "name" is omitted from form submissions; default to Id (already
        // unique per instance) unless the consumer supplied their own.
        Attributes ??= [];
        if (!Attributes.ContainsKey("name") && !string.IsNullOrEmpty(Id))
        {
            Attributes["name"] = Id;
        }

        // If parent provided a Files list via binding, synchronize internal state
        if (Files != null)
        {
            selectedFiles = Files;
        }

        if (AllowedFileTypes != null && AllowedFileTypes.Any())
        {
            var acceptVal = acceptAttr;
            if (!string.IsNullOrEmpty(acceptVal))
            {
                // If the consumer already supplied an "accept" attribute, append the AllowedFileTypes
                // value to it (comma-separated) while avoiding duplicate entries. Otherwise set it.
                if (Attributes.TryGetValue("accept", out var existingObj) && existingObj != null)
                {
                    var existing = existingObj.ToString() ?? string.Empty;
                    var combined = existing.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Concat(acceptVal.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct();

                    Attributes["accept"] = string.Join(",", combined);
                }
                else
                {
                    Attributes["accept"] = acceptVal;
                }
            }
        }
    }

    /// <summary>
    /// Remove a file from the component and update the two-way bound Files list.
    /// Call this from parent via @ref or from inside the component UI.
    /// </summary>
    public async Task RemoveFile(IBrowserFile file)
    {
        var existing = selectedFiles.FirstOrDefault(f => f.Name == file.Name);
        if (existing == null) return;

        selectedFiles.Remove(existing);

        // Update bound Files and notify parent
        Files = selectedFiles;
        if (FilesChanged.HasDelegate)
        {
            await FilesChanged.InvokeAsync(Files);
        }

        StateHasChanged();
    }
}
