// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Localization;

/// <summary>
/// The resource keys for every string TwBlazor components render. Each constant matches an entry in
/// <c>Resources/TwBlazorStrings.resx</c>; use them with <see cref="ITwLocalizer"/> or as the names in your own
/// resource files.
/// </summary>
public static class TwStrings
{
    /// <summary>The key for "Close".</summary>
    public const string Common_Close = "Common_Close";

    /// <summary>The key for "Cancel".</summary>
    public const string Common_Cancel = "Common_Cancel";

    /// <summary>The key for "Confirm".</summary>
    public const string Common_Confirm = "Common_Confirm";

    /// <summary>The key for "Save".</summary>
    public const string Common_Save = "Common_Save";

    /// <summary>The key for "Delete".</summary>
    public const string Common_Delete = "Common_Delete";

    /// <summary>The key for "Loading".</summary>
    public const string Common_Loading = "Common_Loading";

    /// <summary>The key for "Loading...".</summary>
    public const string Common_LoadingEllipsis = "Common_LoadingEllipsis";

    /// <summary>The key for "Previous".</summary>
    public const string Common_Previous = "Common_Previous";

    /// <summary>The key for "Next".</summary>
    public const string Common_Next = "Common_Next";

    /// <summary>The key for "Start".</summary>
    public const string Common_Start = "Common_Start";

    /// <summary>The key for "End".</summary>
    public const string Common_End = "Common_End";

    /// <summary>The key for "No results".</summary>
    public const string Common_NoResults = "Common_NoResults";

    /// <summary>The key for "Yes".</summary>
    public const string Common_Yes = "Common_Yes";

    /// <summary>The key for "No".</summary>
    public const string Common_No = "Common_No";

    /// <summary>The key for "{0} - {1}".</summary>
    public const string Common_Range = "Common_Range";

    /// <summary>The key for "{0} result available".</summary>
    public const string Autocomplete_ResultsOne = "Autocomplete_ResultsOne";

    /// <summary>The key for "{0} results available".</summary>
    public const string Autocomplete_ResultsOther = "Autocomplete_ResultsOther";

    /// <summary>The key for "Breadcrumb".</summary>
    public const string Breadcrumb_Label = "Breadcrumb_Label";

    /// <summary>The key for "Today".</summary>
    public const string Calendar_Today = "Calendar_Today";

    /// <summary>The key for "Add event".</summary>
    public const string Calendar_AddEvent = "Calendar_AddEvent";

    /// <summary>The key for "Search events".</summary>
    public const string Calendar_SearchEvents = "Calendar_SearchEvents";

    /// <summary>The key for "Previous day".</summary>
    public const string Calendar_PreviousDay = "Calendar_PreviousDay";

    /// <summary>The key for "Previous week".</summary>
    public const string Calendar_PreviousWeek = "Calendar_PreviousWeek";

    /// <summary>The key for "Previous month".</summary>
    public const string Calendar_PreviousMonth = "Calendar_PreviousMonth";

    /// <summary>The key for "Next day".</summary>
    public const string Calendar_NextDay = "Calendar_NextDay";

    /// <summary>The key for "Next week".</summary>
    public const string Calendar_NextWeek = "Calendar_NextWeek";

    /// <summary>The key for "Next month".</summary>
    public const string Calendar_NextMonth = "Calendar_NextMonth";

    /// <summary>The key for "Day view".</summary>
    public const string Calendar_DayView = "Calendar_DayView";

    /// <summary>The key for "Week view".</summary>
    public const string Calendar_WeekView = "Calendar_WeekView";

    /// <summary>The key for "Month view".</summary>
    public const string Calendar_MonthView = "Calendar_MonthView";

    /// <summary>The key for "Select view".</summary>
    public const string Calendar_SelectView = "Calendar_SelectView";

    /// <summary>The key for "All day".</summary>
    public const string Calendar_AllDay = "Calendar_AllDay";

    /// <summary>The key for "All day events".</summary>
    public const string Calendar_AllDayEvents = "Calendar_AllDayEvents";

    /// <summary>The key for "{0} (all day)".</summary>
    public const string Calendar_AllDayRange = "Calendar_AllDayRange";

    /// <summary>The key for "Week {0}".</summary>
    public const string Calendar_WeekNumber = "Calendar_WeekNumber";

    /// <summary>The key for "QR code: {0}", the accessible name of a QR code.</summary>
    public const string QrCode_Label = "QrCode_Label";

    /// <summary>The key for "{0} out of {1}", the accessible name of one of a rating's values.</summary>
    public const string Rating_Value = "Rating_Value";

    /// <summary>The key for "No rating", the accessible name of the option that clears a rating.</summary>
    public const string Rating_None = "Rating_None";

    /// <summary>The key for "Rated {0} out of {1}", the accessible name of a read-only rating.</summary>
    public const string Rating_ReadOnly = "Rating_ReadOnly";

    /// <summary>The key for "Increase value", the accessible name of a numeric field's increase button.</summary>
    public const string Numeric_Increase = "Numeric_Increase";

    /// <summary>The key for "Decrease value", the accessible name of a numeric field's decrease button.</summary>
    public const string Numeric_Decrease = "Numeric_Decrease";

    /// <summary>The key for "Clear value", the accessible name of a text field's clear button.</summary>
    public const string Textfield_Clear = "Textfield_Clear";

    /// <summary>The key for "Enter a valid number.", shown when a numeric field holds text that is not a number.</summary>
    public const string Numeric_Invalid = "Numeric_Invalid";

    /// <summary>The key for "Minimum", the accessible name of a range slider's lower handle.</summary>
    public const string RangeSlider_Minimum = "RangeSlider_Minimum";

    /// <summary>The key for "Maximum", the accessible name of a range slider's upper handle.</summary>
    public const string RangeSlider_Maximum = "RangeSlider_Maximum";

    /// <summary>The key for "Barcode: {0}", the accessible name of a barcode.</summary>
    public const string Barcode_Label = "Barcode_Label";

    /// <summary>The key for "New event".</summary>
    public const string Calendar_NewEvent = "Calendar_NewEvent";

    /// <summary>The key for "Edit event".</summary>
    public const string Calendar_EditEvent = "Calendar_EditEvent";

    /// <summary>The key for "Event".</summary>
    public const string Calendar_Event = "Calendar_Event";

    /// <summary>The key for "{0}, {1}".</summary>
    public const string Calendar_EventLabel = "Calendar_EventLabel";

    /// <summary>The key for "{0}, {1} to {2}".</summary>
    public const string Calendar_EventTimeRange = "Calendar_EventTimeRange";

    /// <summary>The key for "Name".</summary>
    public const string Calendar_FieldName = "Calendar_FieldName";

    /// <summary>The key for "Color".</summary>
    public const string Calendar_FieldColor = "Calendar_FieldColor";

    /// <summary>The key for "Event color".</summary>
    public const string Calendar_EventColor = "Calendar_EventColor";

    /// <summary>The key for "Carousel".</summary>
    public const string Carousel_Label = "Carousel_Label";

    /// <summary>The key for "Previous slide".</summary>
    public const string Carousel_PreviousSlide = "Carousel_PreviousSlide";

    /// <summary>The key for "Next slide".</summary>
    public const string Carousel_NextSlide = "Carousel_NextSlide";

    /// <summary>The key for "Play automatic slideshow".</summary>
    public const string Carousel_Play = "Carousel_Play";

    /// <summary>The key for "Pause automatic slideshow".</summary>
    public const string Carousel_Pause = "Carousel_Pause";

    /// <summary>The key for "Choose slide to display".</summary>
    public const string Carousel_ChooseSlide = "Carousel_ChooseSlide";

    /// <summary>The key for "{0} of {1}".</summary>
    public const string Carousel_SlidePosition = "Carousel_SlidePosition";

    /// <summary>The key for "Slide {0} of {1}".</summary>
    public const string Carousel_SlideOf = "Carousel_SlideOf";

    /// <summary>The key for "carousel".</summary>
    public const string Carousel_RoleDescription = "Carousel_RoleDescription";

    /// <summary>The key for "slide".</summary>
    public const string Carousel_SlideRoleDescription = "Carousel_SlideRoleDescription";

    /// <summary>The key for "Chart".</summary>
    public const string Chart_Label = "Chart_Label";

    /// <summary>The key for "View as table".</summary>
    public const string Chart_ViewAsTable = "Chart_ViewAsTable";

    /// <summary>The key for "View as chart".</summary>
    public const string Chart_ViewAsChart = "Chart_ViewAsChart";

    /// <summary>The key for "Chart data".</summary>
    public const string Chart_TableLabel = "Chart_TableLabel";

    /// <summary>The key for "Legend".</summary>
    public const string Chart_Legend = "Chart_Legend";

    /// <summary>The key for "No data to display".</summary>
    public const string Chart_Empty = "Chart_Empty";

    /// <summary>The key for "Loading chart".</summary>
    public const string Chart_Loading = "Chart_Loading";

    /// <summary>The key for "Category".</summary>
    public const string Chart_Category = "Chart_Category";

    /// <summary>The key for "Value".</summary>
    public const string Chart_Value = "Chart_Value";

    /// <summary>The key for "Other".</summary>
    public const string Chart_Other = "Chart_Other";

    /// <summary>The key for "Group".</summary>
    public const string Chart_Group = "Chart_Group";

    /// <summary>The key for "Measure".</summary>
    public const string Chart_Measure = "Chart_Measure";

    /// <summary>The key for "Actual".</summary>
    public const string Chart_Actual = "Chart_Actual";

    /// <summary>The key for "Target".</summary>
    public const string Chart_Target = "Chart_Target";

    /// <summary>The key for "Rising".</summary>
    public const string Chart_Rising = "Chart_Rising";

    /// <summary>The key for "Falling".</summary>
    public const string Chart_Falling = "Chart_Falling";

    /// <summary>The key for "Period".</summary>
    public const string Chart_Period = "Chart_Period";

    /// <summary>The key for "Open".</summary>
    public const string Chart_Open = "Chart_Open";

    /// <summary>The key for "High".</summary>
    public const string Chart_High = "Chart_High";

    /// <summary>The key for "Low".</summary>
    public const string Chart_Low = "Chart_Low";

    /// <summary>The key for "Close".</summary>
    public const string Chart_Close = "Chart_Close";

    /// <summary>The key for "Of first stage".</summary>
    public const string Chart_OfFirstStage = "Chart_OfFirstStage";

    /// <summary>The key for "Task".</summary>
    public const string Chart_Task = "Chart_Task";

    /// <summary>The key for "Start".</summary>
    public const string Chart_Start = "Chart_Start";

    /// <summary>The key for "End".</summary>
    public const string Chart_End = "Chart_End";

    /// <summary>The key for "Range".</summary>
    public const string Chart_Range = "Chart_Range";

    /// <summary>The key for "{0} to {1}".</summary>
    public const string Chart_RangeValue = "Chart_RangeValue";

    /// <summary>The key for "Minimum".</summary>
    public const string Chart_Minimum = "Chart_Minimum";

    /// <summary>The key for "Maximum".</summary>
    public const string Chart_Maximum = "Chart_Maximum";

    /// <summary>The key for "Count".</summary>
    public const string Chart_Count = "Chart_Count";

    /// <summary>The key for "Lower quartile".</summary>
    public const string Chart_LowerQuartile = "Chart_LowerQuartile";

    /// <summary>The key for "Median".</summary>
    public const string Chart_Median = "Chart_Median";

    /// <summary>The key for "Upper quartile".</summary>
    public const string Chart_UpperQuartile = "Chart_UpperQuartile";

    /// <summary>The key for "1 icon = {0}".</summary>
    public const string Chart_IconLegend = "Chart_IconLegend";

    /// <summary>The key for "Share".</summary>
    public const string Chart_Share = "Chart_Share";

    /// <summary>The key for "Size".</summary>
    public const string Chart_Size = "Chart_Size";

    /// <summary>The key for "Series".</summary>
    public const string Chart_Series = "Chart_Series";

    /// <summary>The key for "Label".</summary>
    public const string Chart_PointLabel = "Chart_PointLabel";

    /// <summary>The key for "Rank".</summary>
    public const string Chart_Rank = "Chart_Rank";

    /// <summary>The key for "Total".</summary>
    public const string Chart_Total = "Chart_Total";

    /// <summary>The key for "Increase".</summary>
    public const string Chart_Increase = "Chart_Increase";

    /// <summary>The key for "Decrease".</summary>
    public const string Chart_Decrease = "Chart_Decrease";

    /// <summary>The key for "Source".</summary>
    public const string Chart_Source = "Chart_Source";

    /// <summary>The key for "Target", the node a link in a graph points to.</summary>
    public const string Chart_Destination = "Chart_Destination";

    /// <summary>The key for "Done".</summary>
    public const string Chart_Done = "Chart_Done";




    /// <summary>The key for "Date".</summary>
    public const string Chart_Date = "Chart_Date";

    /// <summary>The key for "Milestone".</summary>
    public const string Chart_Milestone = "Chart_Milestone";

    /// <summary>The key for "Summary".</summary>
    public const string Chart_Summary = "Chart_Summary";

    /// <summary>The key for "Today".</summary>
    public const string Chart_Today = "Chart_Today";

    /// <summary>The key for "Non-working day".</summary>
    public const string Chart_NonWorking = "Chart_NonWorking";

    /// <summary>The key for "Expand all".</summary>
    public const string Chart_ExpandAll = "Chart_ExpandAll";

    /// <summary>The key for "Collapse all".</summary>
    public const string Chart_CollapseAll = "Chart_CollapseAll";

    /// <summary>The key for "Show the tasks under {0}".</summary>
    public const string Chart_ExpandTask = "Chart_ExpandTask";

    /// <summary>The key for "Hide the tasks under {0}".</summary>
    public const string Chart_CollapseTask = "Chart_CollapseTask";

    /// <summary>The key for "Zoom".</summary>
    public const string Chart_Zoom = "Chart_Zoom";

    /// <summary>The key for "Fit".</summary>
    public const string Chart_ZoomAuto = "Chart_ZoomAuto";

    /// <summary>The key for "Hour".</summary>
    public const string Chart_ZoomHour = "Chart_ZoomHour";

    /// <summary>The key for "Day".</summary>
    public const string Chart_ZoomDay = "Chart_ZoomDay";

    /// <summary>The key for "Week".</summary>
    public const string Chart_ZoomWeek = "Chart_ZoomWeek";

    /// <summary>The key for "Month".</summary>
    public const string Chart_ZoomMonth = "Chart_ZoomMonth";

    /// <summary>The key for "Quarter".</summary>
    public const string Chart_ZoomQuarter = "Chart_ZoomQuarter";

    /// <summary>The key for "Year".</summary>
    public const string Chart_ZoomYear = "Chart_ZoomYear";

    /// <summary>The key for "W{0}".</summary>
    public const string Chart_WeekNumber = "Chart_WeekNumber";

    /// <summary>The key for "Q{0}".</summary>
    public const string Chart_QuarterNumber = "Chart_QuarterNumber";

    /// <summary>The key for "{1} starts after {0} finishes".</summary>
    public const string Chart_FinishToStart = "Chart_FinishToStart";

    /// <summary>The key for "{1} starts when {0} starts".</summary>
    public const string Chart_StartToStart = "Chart_StartToStart";

    /// <summary>The key for "{1} finishes when {0} finishes".</summary>
    public const string Chart_FinishToFinish = "Chart_FinishToFinish";

    /// <summary>The key for "{1} finishes after {0} starts".</summary>
    public const string Chart_StartToFinish = "Chart_StartToFinish";

    /// <summary>The key for "Remove {0}".</summary>
    public const string Chip_Remove = "Chip_Remove";

    /// <summary>The key for "Copy code".</summary>
    public const string CodeBlock_Copy = "CodeBlock_Copy";

    /// <summary>The key for "Copied".</summary>
    public const string CodeBlock_Copied = "CodeBlock_Copied";

    /// <summary>The key for "Copied to clipboard".</summary>
    public const string CodeBlock_CopiedToClipboard = "CodeBlock_CopiedToClipboard";

    /// <summary>The key for "Copy failed".</summary>
    public const string CodeBlock_CopyFailed = "CodeBlock_CopyFailed";

    /// <summary>The key for "Choose color".</summary>
    public const string ColorPicker_Dialog = "ColorPicker_Dialog";

    /// <summary>The key for "Color preview".</summary>
    public const string ColorPicker_Preview = "ColorPicker_Preview";

    /// <summary>The key for "Saturation and lightness".</summary>
    public const string ColorPicker_SaturationLightness = "ColorPicker_SaturationLightness";

    /// <summary>The key for "Saturation {0}%, Lightness {1}%".</summary>
    public const string ColorPicker_SaturationLightnessValue = "ColorPicker_SaturationLightnessValue";

    /// <summary>The key for "Hue".</summary>
    public const string ColorPicker_Hue = "ColorPicker_Hue";

    /// <summary>The key for "{0} degrees".</summary>
    public const string ColorPicker_HueValue = "ColorPicker_HueValue";

    /// <summary>The key for "Alpha".</summary>
    public const string ColorPicker_Alpha = "ColorPicker_Alpha";

    /// <summary>The key for "{0}%".</summary>
    public const string ColorPicker_PercentValue = "ColorPicker_PercentValue";

    /// <summary>The key for "Pick color from screen".</summary>
    public const string ColorPicker_PickFromScreen = "ColorPicker_PickFromScreen";

    /// <summary>The key for "Selected color: {0}".</summary>
    public const string ColorPicker_SelectedColor = "ColorPicker_SelectedColor";

    /// <summary>The key for "Search".</summary>
    public const string DataTable_SearchLabel = "DataTable_SearchLabel";

    /// <summary>The key for "Search…".</summary>
    public const string DataTable_SearchPlaceholder = "DataTable_SearchPlaceholder";

    /// <summary>The key for "Table pagination".</summary>
    public const string Table_Pagination = "Table_Pagination";

    /// <summary>The key for "Page {0} of {1}, showing {2} row".</summary>
    public const string DataTable_PageStatusOne = "DataTable_PageStatusOne";

    /// <summary>The key for "Page {0} of {1}, showing {2} rows".</summary>
    public const string DataTable_PageStatusOther = "DataTable_PageStatusOther";

    /// <summary>The key for "Search cleared".</summary>
    public const string DataTable_SearchCleared = "DataTable_SearchCleared";

    /// <summary>The key for "No results match your search".</summary>
    public const string DataTable_NoSearchResults = "DataTable_NoSearchResults";

    /// <summary>The key for "Showing {0} matching row".</summary>
    public const string DataTable_MatchingOne = "DataTable_MatchingOne";

    /// <summary>The key for "Showing {0} matching rows".</summary>
    public const string DataTable_MatchingOther = "DataTable_MatchingOther";

    /// <summary>The key for "Sorted by {0}, ascending".</summary>
    public const string DataTable_SortedAscending = "DataTable_SortedAscending";

    /// <summary>The key for "Sorted by {0}, descending".</summary>
    public const string DataTable_SortedDescending = "DataTable_SortedDescending";

    /// <summary>The key for "Sort by {0} removed".</summary>
    public const string DataTable_SortRemoved = "DataTable_SortRemoved";

    /// <summary>The key for "Sort by {0}".</summary>
    public const string DataTable_SortBy = "DataTable_SortBy";

    /// <summary>The key for "Sort by {0}, currently ascending".</summary>
    public const string DataTable_SortByAscending = "DataTable_SortByAscending";

    /// <summary>The key for "Sort by {0}, currently descending".</summary>
    public const string DataTable_SortByDescending = "DataTable_SortByDescending";

    /// <summary>The key for "No data available.".</summary>
    public const string DataTable_NoData = "DataTable_NoData";

    /// <summary>The key for "Columns".</summary>
    public const string DataTable_Columns = "DataTable_Columns";

    /// <summary>The key for "Reset columns".</summary>
    public const string DataTable_ResetColumns = "DataTable_ResetColumns";

    /// <summary>The key for "Columns reset".</summary>
    public const string DataTable_ColumnsReset = "DataTable_ColumnsReset";

    /// <summary>The key for "{0} column shown".</summary>
    public const string DataTable_ColumnShown = "DataTable_ColumnShown";

    /// <summary>The key for "{0} column hidden".</summary>
    public const string DataTable_ColumnHidden = "DataTable_ColumnHidden";



    /// <summary>The key for "{0} moved to position {1} of {2}".</summary>
    public const string DataTable_ColumnMoved = "DataTable_ColumnMoved";




    /// <summary>The key for "Export CSV".</summary>
    public const string DataTable_ExportCsv = "DataTable_ExportCsv";

    /// <summary>The key for "Export Excel".</summary>
    public const string DataTable_ExportExcel = "DataTable_ExportExcel";

    /// <summary>The key for "Exported {0} row".</summary>
    public const string DataTable_ExportedOne = "DataTable_ExportedOne";

    /// <summary>The key for "Exported {0} rows".</summary>
    public const string DataTable_ExportedOther = "DataTable_ExportedOther";

    /// <summary>The key for "Group by".</summary>
    public const string DataTable_GroupBy = "DataTable_GroupBy";

    /// <summary>The key for "No grouping".</summary>
    public const string DataTable_NoGrouping = "DataTable_NoGrouping";

    /// <summary>The key for "Grouped by {0}".</summary>
    public const string DataTable_GroupedBy = "DataTable_GroupedBy";

    /// <summary>The key for "Grouping removed".</summary>
    public const string DataTable_GroupingRemoved = "DataTable_GroupingRemoved";

    /// <summary>The key for "{0} row".</summary>
    public const string DataTable_GroupRowsOne = "DataTable_GroupRowsOne";

    /// <summary>The key for "{0} rows".</summary>
    public const string DataTable_GroupRowsOther = "DataTable_GroupRowsOther";

    /// <summary>The key for "{0} expanded".</summary>
    public const string DataTable_GroupExpanded = "DataTable_GroupExpanded";

    /// <summary>The key for "{0} collapsed".</summary>
    public const string DataTable_GroupCollapsed = "DataTable_GroupCollapsed";

    /// <summary>The key for "Expand all groups".</summary>
    public const string DataTable_ExpandAllGroups = "DataTable_ExpandAllGroups";

    /// <summary>The key for "Collapse all groups".</summary>
    public const string DataTable_CollapseAllGroups = "DataTable_CollapseAllGroups";

    /// <summary>The key for "All groups expanded".</summary>
    public const string DataTable_AllGroupsExpanded = "DataTable_AllGroupsExpanded";

    /// <summary>The key for "All groups collapsed".</summary>
    public const string DataTable_AllGroupsCollapsed = "DataTable_AllGroupsCollapsed";

    /// <summary>The key for "(No value)".</summary>
    public const string DataTable_GroupNoValue = "DataTable_GroupNoValue";

    /// <summary>The key for "Loading…".</summary>
    public const string DataTable_Loading = "DataTable_Loading";

    /// <summary>The key for "Actions".</summary>
    public const string DataTable_Actions = "DataTable_Actions";

    /// <summary>The key for "Edit {0}".</summary>
    public const string DataTable_EditRow = "DataTable_EditRow";

    /// <summary>The key for "Save {0}".</summary>
    public const string DataTable_SaveRow = "DataTable_SaveRow";

    /// <summary>The key for "Cancel editing {0}".</summary>
    public const string DataTable_CancelEdit = "DataTable_CancelEdit";

    /// <summary>The key for "Editing {0}".</summary>
    public const string DataTable_Editing = "DataTable_Editing";

    /// <summary>The key for "{0} saved".</summary>
    public const string DataTable_RowSaved = "DataTable_RowSaved";

    /// <summary>The key for "Editing {0} cancelled".</summary>
    public const string DataTable_EditCancelled = "DataTable_EditCancelled";

    /// <summary>The key for "{0} field needs fixing before saving".</summary>
    public const string DataTable_EditInvalidOne = "DataTable_EditInvalidOne";

    /// <summary>The key for "{0} fields need fixing before saving".</summary>
    public const string DataTable_EditInvalidOther = "DataTable_EditInvalidOther";

    /// <summary>The key for "{0} column options".</summary>
    public const string DataTable_ColumnOptions = "DataTable_ColumnOptions";

    /// <summary>The key for "Sort ascending".</summary>
    public const string DataTable_SortAscending = "DataTable_SortAscending";

    /// <summary>The key for "Sort descending".</summary>
    public const string DataTable_SortDescending = "DataTable_SortDescending";

    /// <summary>The key for "Remove sort".</summary>
    public const string DataTable_RemoveSort = "DataTable_RemoveSort";

    /// <summary>The key for "Group by this column".</summary>
    public const string DataTable_GroupByColumn = "DataTable_GroupByColumn";

    /// <summary>The key for "Stop grouping".</summary>
    public const string DataTable_StopGrouping = "DataTable_StopGrouping";

    /// <summary>The key for "Move left".</summary>
    public const string DataTable_MoveColumnLeft = "DataTable_MoveColumnLeft";

    /// <summary>The key for "Move right".</summary>
    public const string DataTable_MoveColumnRight = "DataTable_MoveColumnRight";

    /// <summary>The key for "Hide column".</summary>
    public const string DataTable_HideColumn = "DataTable_HideColumn";

    /// <summary>The key for "Filter {0}".</summary>
    public const string DataTable_FilterColumn = "DataTable_FilterColumn";

    /// <summary>The key for "Filter {0}, filtered".</summary>
    public const string DataTable_FilterColumnActive = "DataTable_FilterColumnActive";

    /// <summary>The key for "First condition".</summary>
    public const string DataTable_FirstCondition = "DataTable_FirstCondition";

    /// <summary>The key for "First value".</summary>
    public const string DataTable_FirstValue = "DataTable_FirstValue";

    /// <summary>The key for "Second condition".</summary>
    public const string DataTable_SecondCondition = "DataTable_SecondCondition";

    /// <summary>The key for "Second value".</summary>
    public const string DataTable_SecondValue = "DataTable_SecondValue";

    /// <summary>The key for "Combine conditions".</summary>
    public const string DataTable_FilterLogic = "DataTable_FilterLogic";

    /// <summary>The key for "And".</summary>
    public const string DataTable_FilterAnd = "DataTable_FilterAnd";

    /// <summary>The key for "Or".</summary>
    public const string DataTable_FilterOr = "DataTable_FilterOr";

    /// <summary>The key for "Filter".</summary>
    public const string DataTable_ApplyFilter = "DataTable_ApplyFilter";

    /// <summary>The key for "Clear".</summary>
    public const string DataTable_ClearFilter = "DataTable_ClearFilter";

    /// <summary>The key for "Filter applied to {0}, showing {1} row".</summary>
    public const string DataTable_FilterAppliedOne = "DataTable_FilterAppliedOne";

    /// <summary>The key for "Filter applied to {0}, showing {1} rows".</summary>
    public const string DataTable_FilterAppliedOther = "DataTable_FilterAppliedOther";

    /// <summary>The key for "Filter removed from {0}".</summary>
    public const string DataTable_FilterRemoved = "DataTable_FilterRemoved";

    /// <summary>The key for "Yes".</summary>
    public const string DataTable_Yes = "DataTable_Yes";

    /// <summary>The key for "No".</summary>
    public const string DataTable_No = "DataTable_No";

    /// <summary>The key for "Contains".</summary>
    public const string DataTable_OperatorContains = "DataTable_OperatorContains";

    /// <summary>The key for "Does not contain".</summary>
    public const string DataTable_OperatorDoesNotContain = "DataTable_OperatorDoesNotContain";

    /// <summary>The key for "Starts with".</summary>
    public const string DataTable_OperatorStartsWith = "DataTable_OperatorStartsWith";

    /// <summary>The key for "Ends with".</summary>
    public const string DataTable_OperatorEndsWith = "DataTable_OperatorEndsWith";

    /// <summary>The key for "Is equal to".</summary>
    public const string DataTable_OperatorIsEqualTo = "DataTable_OperatorIsEqualTo";

    /// <summary>The key for "Is not equal to".</summary>
    public const string DataTable_OperatorIsNotEqualTo = "DataTable_OperatorIsNotEqualTo";

    /// <summary>The key for "Is greater than".</summary>
    public const string DataTable_OperatorIsGreaterThan = "DataTable_OperatorIsGreaterThan";

    /// <summary>The key for "Is greater than or equal to".</summary>
    public const string DataTable_OperatorIsGreaterThanOrEqualTo = "DataTable_OperatorIsGreaterThanOrEqualTo";

    /// <summary>The key for "Is less than".</summary>
    public const string DataTable_OperatorIsLessThan = "DataTable_OperatorIsLessThan";

    /// <summary>The key for "Is less than or equal to".</summary>
    public const string DataTable_OperatorIsLessThanOrEqualTo = "DataTable_OperatorIsLessThanOrEqualTo";

    /// <summary>The key for "Is empty".</summary>
    public const string DataTable_OperatorIsEmpty = "DataTable_OperatorIsEmpty";

    /// <summary>The key for "Is not empty".</summary>
    public const string DataTable_OperatorIsNotEmpty = "DataTable_OperatorIsNotEmpty";

    /// <summary>The key for "Open date picker".</summary>
    public const string DatePicker_Open = "DatePicker_Open";

    /// <summary>The key for "Choose date".</summary>
    public const string DatePicker_Dialog = "DatePicker_Dialog";

    /// <summary>The key for "Previous month".</summary>
    public const string DatePicker_PreviousMonth = "DatePicker_PreviousMonth";

    /// <summary>The key for "Next month".</summary>
    public const string DatePicker_NextMonth = "DatePicker_NextMonth";

    /// <summary>The key for "Previous year".</summary>
    public const string DatePicker_PreviousYear = "DatePicker_PreviousYear";

    /// <summary>The key for "Next year".</summary>
    public const string DatePicker_NextYear = "DatePicker_NextYear";

    /// <summary>The key for "Previous decade".</summary>
    public const string DatePicker_PreviousDecade = "DatePicker_PreviousDecade";

    /// <summary>The key for "Next decade".</summary>
    public const string DatePicker_NextDecade = "DatePicker_NextDecade";

    /// <summary>The key for "Enter a valid date, for example {0}".</summary>
    public const string DatePicker_Invalid = "DatePicker_Invalid";

    /// <summary>The key for "Selected, {0}".</summary>
    public const string DatePicker_Selected = "DatePicker_Selected";

    /// <summary>The key for "Start of selected range, {0}".</summary>
    public const string DatePicker_RangeStart = "DatePicker_RangeStart";

    /// <summary>The key for "End of selected range, {0}".</summary>
    public const string DatePicker_RangeEnd = "DatePicker_RangeEnd";

    /// <summary>The key for "In selected range, {0}".</summary>
    public const string DatePicker_InRange = "DatePicker_InRange";

    /// <summary>The key for "Open date range picker".</summary>
    public const string DateRangePicker_Open = "DateRangePicker_Open";

    /// <summary>The key for "Choose date range".</summary>
    public const string DateRangePicker_Dialog = "DateRangePicker_Dialog";

    /// <summary>The key for "Enter a valid date range, for example {0}".</summary>
    public const string DateRangePicker_Invalid = "DateRangePicker_Invalid";

    /// <summary>The key for "Select a datetime".</summary>
    public const string DateTimePicker_Placeholder = "DateTimePicker_Placeholder";

    /// <summary>The key for "Open datetime range picker".</summary>
    public const string DateTimeRangePicker_Open = "DateTimeRangePicker_Open";

    /// <summary>The key for "Choose start date and time".</summary>
    public const string DateTimeRangePicker_DialogStart = "DateTimeRangePicker_DialogStart";

    /// <summary>The key for "Choose end date and time".</summary>
    public const string DateTimeRangePicker_DialogEnd = "DateTimeRangePicker_DialogEnd";

    /// <summary>The key for "Range step".</summary>
    public const string DateTimeRangePicker_RangeStep = "DateTimeRangePicker_RangeStep";

    /// <summary>The key for "Dialog".</summary>
    public const string Dialog_Label = "Dialog_Label";

    /// <summary>The key for "{0} selected".</summary>
    public const string FileUpload_FileSelected = "FileUpload_FileSelected";

    /// <summary>The key for "{0} files selected: {1}".</summary>
    public const string FileUpload_FilesSelected = "FileUpload_FilesSelected";

    /// <summary>The key for "Top navigation".</summary>
    public const string Navbar_Label = "Navbar_Label";

    /// <summary>The key for "Footer links", the name of the navigation landmark inside a footer.</summary>
    public const string Footer_Label = "Footer_Label";

    /// <summary>The key for "opens in a new tab", read after a link that does.</summary>
    public const string Footer_OpensInNewTab = "Footer_OpensInNewTab";

    /// <summary>The key for "Open navigation menu".</summary>
    public const string Navbar_OpenMenu = "Navbar_OpenMenu";

    /// <summary>The key for "Close navigation menu".</summary>
    public const string Navbar_CloseMenu = "Navbar_CloseMenu";

    /// <summary>The key for "On this page".</summary>
    public const string OnThisPage_Title = "OnThisPage_Title";

    /// <summary>The key for "pagination".</summary>
    public const string Pagination_Label = "Pagination_Label";

    /// <summary>The key for "Previous page".</summary>
    public const string Pagination_PreviousPage = "Pagination_PreviousPage";

    /// <summary>The key for "Next page".</summary>
    public const string Pagination_NextPage = "Pagination_NextPage";

    /// <summary>The key for "Items per page".</summary>
    public const string Pagination_ItemsPerPage = "Pagination_ItemsPerPage";

    /// <summary>The key for "Page {0} of {1}".</summary>
    public const string Pagination_PageStatus = "Pagination_PageStatus";

    /// <summary>The key for "page".</summary>
    public const string Pagination_PagePrefix = "Pagination_PagePrefix";

    /// <summary>The key for "Available".</summary>
    public const string PickList_Source = "PickList_Source";

    /// <summary>The key for "Selected".</summary>
    public const string PickList_Target = "PickList_Target";

    /// <summary>The key for "No items".</summary>
    public const string PickList_Empty = "PickList_Empty";

    /// <summary>The key for "Move selected {0} item up".</summary>
    public const string PickList_MoveItemUp = "PickList_MoveItemUp";

    /// <summary>The key for "Move selected {0} item down".</summary>
    public const string PickList_MoveItemDown = "PickList_MoveItemDown";

    /// <summary>The key for "Move selected items to {0}".</summary>
    public const string PickList_MoveSelectedTo = "PickList_MoveSelectedTo";

    /// <summary>The key for "Move all items to {0}".</summary>
    public const string PickList_MoveAllTo = "PickList_MoveAllTo";

    /// <summary>The key for "{0} item".</summary>
    public const string PickList_ItemsOne = "PickList_ItemsOne";

    /// <summary>The key for "{0} items".</summary>
    public const string PickList_ItemsOther = "PickList_ItemsOther";

    /// <summary>The key for "{0} moved to {1}".</summary>
    public const string PickList_MovedTo = "PickList_MovedTo";

    /// <summary>The key for "{0} moved up".</summary>
    public const string PickList_MovedUp = "PickList_MovedUp";

    /// <summary>The key for "{0} moved down".</summary>
    public const string PickList_MovedDown = "PickList_MovedDown";

    /// <summary>The key for "Select an option...".</summary>
    public const string Select_Placeholder = "Select_Placeholder";

    /// <summary>The key for "Options".</summary>
    public const string Select_Options = "Select_Options";

    /// <summary>The key for "Skip to main content".</summary>
    public const string Sidebar_SkipToMain = "Sidebar_SkipToMain";

    /// <summary>The key for "sidebar navigation".</summary>
    public const string Sidebar_NavigationLabel = "Sidebar_NavigationLabel";

    /// <summary>The key for "Search navigation".</summary>
    public const string Sidebar_SearchLabel = "Sidebar_SearchLabel";

    /// <summary>The key for "Search...".</summary>
    public const string Sidebar_SearchPlaceholder = "Sidebar_SearchPlaceholder";

    /// <summary>The key for "Open sidebar".</summary>
    public const string Sidebar_Open = "Sidebar_Open";

    /// <summary>The key for "Close sidebar".</summary>
    public const string Sidebar_Close = "Sidebar_Close";

    /// <summary>The key for "{0} result".</summary>
    public const string Sidebar_ResultsOne = "Sidebar_ResultsOne";

    /// <summary>The key for "{0} results".</summary>
    public const string Sidebar_ResultsOther = "Sidebar_ResultsOther";

    /// <summary>The key for "Progress".</summary>
    public const string Stepper_Label = "Stepper_Label";

    /// <summary>The key for "Step {0} of {1}: {2}".</summary>
    public const string Stepper_StepOf = "Stepper_StepOf";

    /// <summary>The key for "Step {0}: {1}, {2}".</summary>
    public const string Stepper_StepState = "Stepper_StepState";

    /// <summary>The key for "completed".</summary>
    public const string Stepper_Completed = "Stepper_Completed";

    /// <summary>The key for "current".</summary>
    public const string Stepper_Current = "Stepper_Current";

    /// <summary>The key for "not started".</summary>
    public const string Stepper_NotStarted = "Stepper_NotStarted";

    /// <summary>The key for "Scrollable table".</summary>
    public const string Table_ScrollRegion = "Table_ScrollRegion";

    /// <summary>The key for "Resize {0}".</summary>
    public const string Table_ResizeColumn = "Table_ResizeColumn";

    /// <summary>The key for "{0} pixels".</summary>
    public const string Table_ColumnWidthValue = "Table_ColumnWidthValue";

    /// <summary>The key for "Open time picker".</summary>
    public const string TimePicker_Open = "TimePicker_Open";

    /// <summary>The key for "Choose time".</summary>
    public const string TimePicker_Dialog = "TimePicker_Dialog";

    /// <summary>The key for "Select a time".</summary>
    public const string TimePicker_Placeholder = "TimePicker_Placeholder";

    /// <summary>The key for "Enter a valid time, for example {0}".</summary>
    public const string TimePicker_Invalid = "TimePicker_Invalid";

    /// <summary>The key for "Hours".</summary>
    public const string TimePicker_Hours = "TimePicker_Hours";

    /// <summary>The key for "Minutes".</summary>
    public const string TimePicker_Minutes = "TimePicker_Minutes";

    /// <summary>The key for "Increase hour".</summary>
    public const string TimePicker_IncreaseHour = "TimePicker_IncreaseHour";

    /// <summary>The key for "Decrease hour".</summary>
    public const string TimePicker_DecreaseHour = "TimePicker_DecreaseHour";

    /// <summary>The key for "Increase minute".</summary>
    public const string TimePicker_IncreaseMinute = "TimePicker_IncreaseMinute";

    /// <summary>The key for "Decrease minute".</summary>
    public const string TimePicker_DecreaseMinute = "TimePicker_DecreaseMinute";

    /// <summary>The key for "{0}, switch to {1}".</summary>
    public const string TimePicker_SwitchPeriod = "TimePicker_SwitchPeriod";

    /// <summary>The key for "Hour", the short label above the hours.</summary>
    public const string TimePicker_HourLabel = "TimePicker_HourLabel";

    /// <summary>The key for "Min", the short label above the minutes.</summary>
    public const string TimePicker_MinuteLabel = "TimePicker_MinuteLabel";

    /// <summary>The key for "AM or PM", the name of the pair of AM and PM buttons.</summary>
    public const string TimePicker_Period = "TimePicker_Period";

    /// <summary>The key for "Now", the button that sets the current time.</summary>
    public const string TimePicker_Now = "TimePicker_Now";

    /// <summary>The key for "Done", the button that closes the time picker.</summary>
    public const string TimePicker_Done = "TimePicker_Done";

    /// <summary>The key for "Open time range picker".</summary>
    public const string TimeRangePicker_Open = "TimeRangePicker_Open";

    /// <summary>The key for "Choose start time".</summary>
    public const string TimeRangePicker_DialogStart = "TimeRangePicker_DialogStart";

    /// <summary>The key for "Choose end time".</summary>
    public const string TimeRangePicker_DialogEnd = "TimeRangePicker_DialogEnd";

    /// <summary>The key for "Enter a valid time range, for example {0}".</summary>
    public const string TimeRangePicker_Invalid = "TimeRangePicker_Invalid";

    /// <summary>The key for "Notifications".</summary>
    public const string Toast_RegionLabel = "Toast_RegionLabel";

    /// <summary>The key for "just now".</summary>
    public const string Toast_JustNow = "Toast_JustNow";

    /// <summary>The key for "{0}m ago".</summary>
    public const string Toast_MinutesAgo = "Toast_MinutesAgo";

    /// <summary>The key for "{0}h ago".</summary>
    public const string Toast_HoursAgo = "Toast_HoursAgo";

    /// <summary>The key for "{0}d ago".</summary>
    public const string Toast_DaysAgo = "Toast_DaysAgo";

    /// <summary>The key for "Select {0}".</summary>
    public const string TreeList_Select = "TreeList_Select";

    /// <summary>The key for the optional .NET format string for a time of day (for example HH:mm).</summary>
    public const string Format_ShortTime = "Format_ShortTime";

    /// <summary>The key for the optional .NET format string for an hour label in the calendar gutter (for example HH:mm or h tt).</summary>
    public const string Format_Hour = "Format_Hour";

    /// <summary>The key for the optional .NET format string for a full date read by screen readers (for example dddd, d MMMM yyyy).</summary>
    public const string Format_LongDate = "Format_LongDate";

    /// <summary>The key for the optional .NET format string for an abbreviated date (for example d MMM yyyy).</summary>
    public const string Format_MediumDate = "Format_MediumDate";

    /// <summary>The key for the optional .NET format string for a month and year (for example MMMM yyyy).</summary>
    public const string Format_MonthYear = "Format_MonthYear";

    /// <summary>The key for the optional .NET format string for a day and month without the year (for example d MMMM).</summary>
    public const string Format_MonthDay = "Format_MonthDay";

    /// <summary>The key for the optional .NET format string for a month on its own (for example MMMM).</summary>
    public const string Format_Month = "Format_Month";

    /// <summary>The key for the optional .NET format string for an abbreviated month (for example MMM).</summary>
    public const string Format_ShortMonth = "Format_ShortMonth";

    /// <summary>The key for the optional .NET format string for a year (for example yyyy).</summary>
    public const string Format_Year = "Format_Year";

    /// <summary>The key for the optional .NET format string for a week view column heading (for example ddd d).</summary>
    public const string Format_WeekdayDay = "Format_WeekdayDay";

    /// <summary>The key for the optional .NET format string the date pickers type and parse (for example dd.MM.yyyy).</summary>
    public const string Format_DateInput = "Format_DateInput";

    /// <summary>The key for the optional .NET format string the time pickers type and parse (for example HH:mm).</summary>
    public const string Format_TimeInput = "Format_TimeInput";

    /// <summary>The key for the optional .NET format string the date time pickers type and parse (for example dd.MM.yyyy HH:mm).</summary>
    public const string Format_DateTimeInput = "Format_DateTimeInput";

    /// <summary>The key for "Options".</summary>
    public const string Menu_Options = "Menu_Options";

    /// <summary>The key for "Context menu", the name of a context menu when the app gives it no name of its own.</summary>
    public const string ContextMenu_Label = "ContextMenu_Label";

    /// <summary>The key for the hint read out on a focusable context menu area, saying how to open its menu from the keyboard.</summary>
    public const string ContextMenu_Hint = "ContextMenu_Hint";

    /// <summary>The key for "Reactions", the name of the row of reactions under a chat bubble.</summary>
    public const string Bubble_Reactions = "Bubble_Reactions";

    /// <summary>The key for "{0}: {1}", where {0} is a reaction's name and {1} is how many people chose it.</summary>
    public const string Bubble_Reaction = "Bubble_Reaction";

    /// <summary>The key for "Uploading {0}", where {0} is the file name.</summary>
    public const string Attachment_Uploading = "Attachment_Uploading";

    /// <summary>The key for "{0} uploaded", where {0} is the file name.</summary>
    public const string Attachment_Uploaded = "Attachment_Uploaded";

    /// <summary>The key for "{0} failed to upload", where {0} is the file name.</summary>
    public const string Attachment_Failed = "Attachment_Failed";

    /// <summary>The key for "Retry uploading {0}", where {0} is the file name.</summary>
    public const string Attachment_Retry = "Attachment_Retry";

    /// <summary>The key for "Pending", a step that has not started yet.</summary>
    public const string Marker_Pending = "Marker_Pending";

    /// <summary>The key for "In progress", a step that is running.</summary>
    public const string Marker_Running = "Marker_Running";

    /// <summary>The key for "Done", a step that finished successfully.</summary>
    public const string Marker_Succeeded = "Marker_Succeeded";

    /// <summary>The key for "Failed", a step that failed.</summary>
    public const string Marker_Failed = "Marker_Failed";

    /// <summary>The key for "There is a problem", the heading of a form's error summary.</summary>
    public const string Form_SummaryTitle = "Form_SummaryTitle";

    /// <summary>The key for "No data", the default title of an empty state.</summary>
    public const string Empty_Title = "Empty_Title";

    /// <summary>The key for "Tags", the name of a tag input's list of tags when it has no label.</summary>
    public const string TagInput_Tags = "TagInput_Tags";

    /// <summary>The key for "{0} added", where {0} is the tag.</summary>
    public const string TagInput_Added = "TagInput_Added";

    /// <summary>The key for "{0} removed", where {0} is the tag.</summary>
    public const string TagInput_Removed = "TagInput_Removed";

    /// <summary>The key for "All tags removed".</summary>
    public const string TagInput_Cleared = "TagInput_Cleared";

    /// <summary>The key for "Remove all tags".</summary>
    public const string TagInput_ClearAll = "TagInput_ClearAll";

    /// <summary>The key for "{0} has already been added", where {0} is the tag.</summary>
    public const string TagInput_Duplicate = "TagInput_Duplicate";

    /// <summary>The key for "You can add only {0} tag".</summary>
    public const string TagInput_LimitOne = "TagInput_LimitOne";

    /// <summary>The key for "You can add up to {0} tags".</summary>
    public const string TagInput_LimitOther = "TagInput_LimitOther";

    /// <summary>The key for "A tag can be at most {0} character long".</summary>
    public const string TagInput_TooLongOne = "TagInput_TooLongOne";

    /// <summary>The key for "A tag can be at most {0} characters long".</summary>
    public const string TagInput_TooLongOther = "TagInput_TooLongOther";

    /// <summary>The key for "Text editor". Accessible name of a rich text editor that has no label.</summary>
    public const string TextEditor_Label = "TextEditor_Label";

    /// <summary>The key for "Formatting". Accessible name of a rich text editor's toolbar.</summary>
    public const string TextEditor_Toolbar = "TextEditor_Toolbar";

    /// <summary>The key for "Press Alt+F10 to move to the toolbar.". Read out with a rich text editor that has a toolbar.</summary>
    public const string TextEditor_ToolbarHint = "TextEditor_ToolbarHint";

    /// <summary>The key for "Text style". Accessible name of the list of paragraph and heading styles.</summary>
    public const string TextEditor_TextStyle = "TextEditor_TextStyle";

    /// <summary>The key for "Paragraph". A plain paragraph text style.</summary>
    public const string TextEditor_Paragraph = "TextEditor_Paragraph";

    /// <summary>The key for "Heading 1". A top level heading text style.</summary>
    public const string TextEditor_Heading1 = "TextEditor_Heading1";

    /// <summary>The key for "Heading 2". A second level heading text style.</summary>
    public const string TextEditor_Heading2 = "TextEditor_Heading2";

    /// <summary>The key for "Heading 3". A third level heading text style.</summary>
    public const string TextEditor_Heading3 = "TextEditor_Heading3";

    /// <summary>The key for "Text formatting". Accessible name of the group of bold, italic, underline and strikethrough buttons.</summary>
    public const string TextEditor_TextFormatting = "TextEditor_TextFormatting";

    /// <summary>The key for "Bold". Name of the bold button.</summary>
    public const string TextEditor_Bold = "TextEditor_Bold";

    /// <summary>The key for "Italic". Name of the italic button.</summary>
    public const string TextEditor_Italic = "TextEditor_Italic";

    /// <summary>The key for "Underline". Name of the underline button.</summary>
    public const string TextEditor_Underline = "TextEditor_Underline";

    /// <summary>The key for "Strikethrough". Name of the strikethrough button.</summary>
    public const string TextEditor_Strikethrough = "TextEditor_Strikethrough";

    /// <summary>The key for "Lists". Accessible name of the group of list buttons.</summary>
    public const string TextEditor_Lists = "TextEditor_Lists";

    /// <summary>The key for "Bulleted list". Name of the bulleted list button.</summary>
    public const string TextEditor_BulletList = "TextEditor_BulletList";

    /// <summary>The key for "Numbered list". Name of the numbered list button.</summary>
    public const string TextEditor_NumberedList = "TextEditor_NumberedList";

    /// <summary>The key for "Blocks". Accessible name of the group of quote and code block buttons.</summary>
    public const string TextEditor_Blocks = "TextEditor_Blocks";

    /// <summary>The key for "Quote". Name of the quote button.</summary>
    public const string TextEditor_Blockquote = "TextEditor_Blockquote";

    /// <summary>The key for "Code block". Name of the code block button.</summary>
    public const string TextEditor_CodeBlock = "TextEditor_CodeBlock";

    /// <summary>The key for "Link". Name of the link button and of the bar for adding a link.</summary>
    public const string TextEditor_Link = "TextEditor_Link";

    /// <summary>The key for "Link address". Accessible name of the field for a link's address.</summary>
    public const string TextEditor_LinkUrl = "TextEditor_LinkUrl";

    /// <summary>The key for "Apply". Button that adds or changes a link.</summary>
    public const string TextEditor_LinkApply = "TextEditor_LinkApply";

    /// <summary>The key for "Remove link". Button that removes a link.</summary>
    public const string TextEditor_LinkRemove = "TextEditor_LinkRemove";

    /// <summary>The key for "Enter a web or email address, such as https://example.com". Shown when a link address is not valid.</summary>
    public const string TextEditor_LinkInvalid = "TextEditor_LinkInvalid";

    /// <summary>The key for "History". Accessible name of the group of clear formatting, undo and redo buttons.</summary>
    public const string TextEditor_History = "TextEditor_History";

    /// <summary>The key for "Clear formatting". Name of the clear formatting button.</summary>
    public const string TextEditor_ClearFormatting = "TextEditor_ClearFormatting";

    /// <summary>The key for "Undo". Name of the undo button.</summary>
    public const string TextEditor_Undo = "TextEditor_Undo";

    /// <summary>The key for "Redo". Name of the redo button.</summary>
    public const string TextEditor_Redo = "TextEditor_Redo";

    /// <summary>The key for "HTML source". Name of the button that shows the HTML source, and of the source view.</summary>
    public const string TextEditor_Source = "TextEditor_Source";

    /// <summary>The key for "Select all". Context menu item that selects all of the text.</summary>
    public const string TextEditor_SelectAll = "TextEditor_SelectAll";

    /// <summary>The key for "Export". Accessible name of a rich text editor's export menu button.</summary>
    public const string TextEditor_Export = "TextEditor_Export";

    /// <summary>The key for "Markdown". Export menu item for a Markdown file.</summary>
    public const string TextEditor_ExportMarkdown = "TextEditor_ExportMarkdown";

    /// <summary>The key for "Word document". Export menu item for a Word (.docx) file.</summary>
    public const string TextEditor_ExportDocx = "TextEditor_ExportDocx";

    /// <summary>The key for "OpenDocument text". Export menu item for an OpenDocument (.odt) file.</summary>
    public const string TextEditor_ExportOdt = "TextEditor_ExportOdt";

    /// <summary>The key for "Web page". Export menu item for an HTML file.</summary>
    public const string TextEditor_ExportHtml = "TextEditor_ExportHtml";

    /// <summary>The key for "Plain text". Export menu item for a plain text file.</summary>
    public const string TextEditor_ExportPlainText = "TextEditor_ExportPlainText";

    /// <summary>The key for "Rich Text Format". Export menu item for an RTF file.</summary>
    public const string TextEditor_ExportRtf = "TextEditor_ExportRtf";

    /// <summary>The key for "Exported {0}". Announced after an export, where {0} is the file name.</summary>
    public const string TextEditor_Exported = "TextEditor_Exported";

    /// <summary>The key for "Spreadsheet". Accessible name of a spreadsheet that has no label.</summary>
    public const string Spreadsheet_Label = "Spreadsheet_Label";

    /// <summary>The key for "Spreadsheet tools". Accessible name of a spreadsheet's toolbar.</summary>
    public const string Spreadsheet_Toolbar = "Spreadsheet_Toolbar";

    /// <summary>The key for "Alignment". Accessible name of the group of alignment buttons.</summary>
    public const string Spreadsheet_Alignment = "Spreadsheet_Alignment";

    /// <summary>The key for "Align left". Name of the button that lines cell content up on the left.</summary>
    public const string Spreadsheet_AlignLeft = "Spreadsheet_AlignLeft";

    /// <summary>The key for "Center". Name of the button that lines cell content up in the middle.</summary>
    public const string Spreadsheet_AlignCenter = "Spreadsheet_AlignCenter";

    /// <summary>The key for "Align right". Name of the button that lines cell content up on the right.</summary>
    public const string Spreadsheet_AlignRight = "Spreadsheet_AlignRight";

    /// <summary>The key for "Number format". Accessible name of the list of number formats.</summary>
    public const string Spreadsheet_NumberFormat = "Spreadsheet_NumberFormat";

    /// <summary>The key for "General". A number format that shows numbers as they are.</summary>
    public const string Spreadsheet_FormatGeneral = "Spreadsheet_FormatGeneral";

    /// <summary>The key for "Number". A number format with two decimal places.</summary>
    public const string Spreadsheet_FormatNumber = "Spreadsheet_FormatNumber";

    /// <summary>The key for "Currency". A number format for amounts of money.</summary>
    public const string Spreadsheet_FormatCurrency = "Spreadsheet_FormatCurrency";

    /// <summary>The key for "Percent". A number format that shows numbers as percentages.</summary>
    public const string Spreadsheet_FormatPercent = "Spreadsheet_FormatPercent";

    /// <summary>The key for "Cell reference". Accessible name of the box showing the active cell's address, such as B3.</summary>
    public const string Spreadsheet_NameBox = "Spreadsheet_NameBox";

    /// <summary>The key for "Cell contents". Accessible name of the field that shows and edits the active cell's contents or formula.</summary>
    public const string Spreadsheet_FormulaBar = "Spreadsheet_FormulaBar";

    /// <summary>The key for "Sheets". Accessible name of the row of sheet tabs.</summary>
    public const string Spreadsheet_SheetTabs = "Spreadsheet_SheetTabs";

    /// <summary>The key for "Add sheet". Name of the button that adds a sheet.</summary>
    public const string Spreadsheet_AddSheet = "Spreadsheet_AddSheet";

    /// <summary>The key for "Sheet{0}". Name given to a new sheet, where {0} is its number, such as Sheet2.</summary>
    public const string Spreadsheet_DefaultSheetName = "Spreadsheet_DefaultSheetName";

    /// <summary>The key for "Sheet name". Accessible name of the field for renaming a sheet.</summary>
    public const string Spreadsheet_RenameSheet = "Spreadsheet_RenameSheet";

    /// <summary>The key for "Type to replace a cell, or press Enter or F2 to edit it. Formulas start with an equals sign.". Read out with a spreadsheet's grid.</summary>
    public const string Spreadsheet_GridHint = "Spreadsheet_GridHint";

    /// <summary>The key for "Edit {0}". Accessible name of the field that edits a cell, where {0} is the cell's address.</summary>
    public const string Spreadsheet_EditCell = "Spreadsheet_EditCell";

    /// <summary>The key for "Copied {0}". Announced after copying, where {0} is the copied range, such as A1:B3.</summary>
    public const string Spreadsheet_Copied = "Spreadsheet_Copied";

    /// <summary>The key for "Pasted into {0}". Announced after pasting, where {0} is the range pasted into.</summary>
    public const string Spreadsheet_Pasted = "Spreadsheet_Pasted";

    /// <summary>The key for "Cleared {0}". Announced after clearing cells, where {0} is the cleared range.</summary>
    public const string Spreadsheet_Cleared = "Spreadsheet_Cleared";

    /// <summary>The key for "Excel workbook". Export menu item for an Excel (.xlsx) file.</summary>
    public const string Spreadsheet_ExportExcel = "Spreadsheet_ExportExcel";

    /// <summary>The key for "CSV of this sheet". Export menu item for a CSV file of the sheet being shown.</summary>
    public const string Spreadsheet_ExportCsv = "Spreadsheet_ExportCsv";

    /// <summary>The key for "{0} selected". Announced when a range of cells is selected, where {0} is the range, such as A1:C4.</summary>
    public const string Spreadsheet_RangeSelected = "Spreadsheet_RangeSelected";

    /// <summary>The key for "Font". Accessible name of the list of fonts.</summary>
    public const string Spreadsheet_Font = "Spreadsheet_Font";

    /// <summary>The key for "Font size". Accessible name of the list of text sizes.</summary>
    public const string Spreadsheet_FontSize = "Spreadsheet_FontSize";

    /// <summary>The key for "Default". The font option that uses the theme's own font.</summary>
    public const string Spreadsheet_DefaultFont = "Spreadsheet_DefaultFont";

    /// <summary>The key for "Text color". Name of the menu button that colors the text of the selected cells.</summary>
    public const string Spreadsheet_TextColor = "Spreadsheet_TextColor";

    /// <summary>The key for "Fill color". Name of the menu button that shades the selected cells.</summary>
    public const string Spreadsheet_FillColor = "Spreadsheet_FillColor";

    /// <summary>The key for "Automatic". Text color option that uses the theme's own text color.</summary>
    public const string Spreadsheet_Automatic = "Spreadsheet_Automatic";

    /// <summary>The key for "No fill". Fill color option that removes the shading.</summary>
    public const string Spreadsheet_NoFill = "Spreadsheet_NoFill";

    /// <summary>The key for "Red". A color name.</summary>
    public const string Spreadsheet_ColorRed = "Spreadsheet_ColorRed";

    /// <summary>The key for "Orange". A color name.</summary>
    public const string Spreadsheet_ColorOrange = "Spreadsheet_ColorOrange";

    /// <summary>The key for "Yellow". A color name.</summary>
    public const string Spreadsheet_ColorYellow = "Spreadsheet_ColorYellow";

    /// <summary>The key for "Green". A color name.</summary>
    public const string Spreadsheet_ColorGreen = "Spreadsheet_ColorGreen";

    /// <summary>The key for "Teal". A color name.</summary>
    public const string Spreadsheet_ColorTeal = "Spreadsheet_ColorTeal";

    /// <summary>The key for "Blue". A color name.</summary>
    public const string Spreadsheet_ColorBlue = "Spreadsheet_ColorBlue";

    /// <summary>The key for "Purple". A color name.</summary>
    public const string Spreadsheet_ColorPurple = "Spreadsheet_ColorPurple";

    /// <summary>The key for "Pink". A color name.</summary>
    public const string Spreadsheet_ColorPink = "Spreadsheet_ColorPink";

    /// <summary>The key for "Gray". A color name.</summary>
    public const string Spreadsheet_ColorGray = "Spreadsheet_ColorGray";

    /// <summary>The key for "Black". A color name.</summary>
    public const string Spreadsheet_ColorBlack = "Spreadsheet_ColorBlack";

    /// <summary>The key for "Freeze". Name of the menu button with the freeze panes options.</summary>
    public const string Spreadsheet_Freeze = "Spreadsheet_Freeze";

    /// <summary>The key for "Freeze panes". Menu item that keeps the rows above and the columns before the active cell in view.</summary>
    public const string Spreadsheet_FreezePanes = "Spreadsheet_FreezePanes";

    /// <summary>The key for "Freeze top row". Menu item that keeps the first row in view.</summary>
    public const string Spreadsheet_FreezeTopRow = "Spreadsheet_FreezeTopRow";

    /// <summary>The key for "Freeze first column". Menu item that keeps the first column in view.</summary>
    public const string Spreadsheet_FreezeFirstColumn = "Spreadsheet_FreezeFirstColumn";

    /// <summary>The key for "Unfreeze panes". Menu item that lets every row and column scroll again.</summary>
    public const string Spreadsheet_Unfreeze = "Spreadsheet_Unfreeze";

    /// <summary>The key for "{0} rows and {1} columns frozen". Announced after freezing panes, where {0} and {1} are numbers.</summary>
    public const string Spreadsheet_FrozenStatus = "Spreadsheet_FrozenStatus";

    /// <summary>The key for "Zoom". Accessible name of the group of zoom controls.</summary>
    public const string Spreadsheet_Zoom = "Spreadsheet_Zoom";

    /// <summary>The key for "Zoom in". Name of the button that makes the grid larger.</summary>
    public const string Spreadsheet_ZoomIn = "Spreadsheet_ZoomIn";

    /// <summary>The key for "Zoom out". Name of the button that makes the grid smaller.</summary>
    public const string Spreadsheet_ZoomOut = "Spreadsheet_ZoomOut";

    /// <summary>The key for "Zoom {0}%. Reset to 100%". Name of the button showing the zoom level, where {0} is the percentage; pressing it goes back to 100%.</summary>
    public const string Spreadsheet_ZoomReset = "Spreadsheet_ZoomReset";

    /// <summary>The key for "Zoom {0}%". Announced after the zoom changes, where {0} is the percentage.</summary>
    public const string Spreadsheet_ZoomChanged = "Spreadsheet_ZoomChanged";

    /// <summary>The key for "Sum: {0}". Shown under the grid for a selection of numbers, where {0} is their total.</summary>
    public const string Spreadsheet_StatusSum = "Spreadsheet_StatusSum";

    /// <summary>The key for "Average: {0}". Shown under the grid for a selection of numbers, where {0} is their average.</summary>
    public const string Spreadsheet_StatusAverage = "Spreadsheet_StatusAverage";

    /// <summary>The key for "Count: {0}". Shown under the grid for a selection, where {0} is how many cells hold something.</summary>
    public const string Spreadsheet_StatusCount = "Spreadsheet_StatusCount";

    /// <summary>The key for "Cut". Context menu item that cuts the selected cells.</summary>
    public const string Spreadsheet_Cut = "Spreadsheet_Cut";

    /// <summary>The key for "Copy". Context menu item that copies the selected cells.</summary>
    public const string Spreadsheet_Copy = "Spreadsheet_Copy";

    /// <summary>The key for "Paste". Context menu item that pastes from the clipboard.</summary>
    public const string Spreadsheet_Paste = "Spreadsheet_Paste";

    /// <summary>The key for "Insert row above". Context menu item that inserts a row above the selection.</summary>
    public const string Spreadsheet_InsertRowAbove = "Spreadsheet_InsertRowAbove";

    /// <summary>The key for "Insert row below". Context menu item that inserts a row below the selection.</summary>
    public const string Spreadsheet_InsertRowBelow = "Spreadsheet_InsertRowBelow";

    /// <summary>The key for "Insert column before". Context menu item that inserts a column before the selection.</summary>
    public const string Spreadsheet_InsertColumnBefore = "Spreadsheet_InsertColumnBefore";

    /// <summary>The key for "Insert column after". Context menu item that inserts a column after the selection.</summary>
    public const string Spreadsheet_InsertColumnAfter = "Spreadsheet_InsertColumnAfter";

    /// <summary>The key for "Delete rows". Context menu item that deletes the selected rows.</summary>
    public const string Spreadsheet_DeleteRows = "Spreadsheet_DeleteRows";

    /// <summary>The key for "Delete columns". Context menu item that deletes the selected columns.</summary>
    public const string Spreadsheet_DeleteColumns = "Spreadsheet_DeleteColumns";

    /// <summary>The key for "Sort A to Z". Context menu item that sorts the selected rows by the active column, smallest first.</summary>
    public const string Spreadsheet_SortAscending = "Spreadsheet_SortAscending";

    /// <summary>The key for "Sort Z to A". Context menu item that sorts the selected rows by the active column, largest first.</summary>
    public const string Spreadsheet_SortDescending = "Spreadsheet_SortDescending";

    /// <summary>The key for "Clear contents". Context menu item that empties the selected cells.</summary>
    public const string Spreadsheet_ClearContents = "Spreadsheet_ClearContents";

    /// <summary>The key for "Widen column". Context menu item that makes the active column wider.</summary>
    public const string Spreadsheet_WidenColumn = "Spreadsheet_WidenColumn";

    /// <summary>The key for "Narrow column". Context menu item that makes the active column narrower.</summary>
    public const string Spreadsheet_NarrowColumn = "Spreadsheet_NarrowColumn";

    /// <summary>The key for "Full screen". Name of the toggle button that makes the spreadsheet cover the window.</summary>
    public const string Spreadsheet_FullScreen = "Spreadsheet_FullScreen";

    /// <summary>The key for "More tools". Name of the button at the end of the spreadsheet toolbar that shows every control on several lines, or puts them back on one line.</summary>
    public const string Spreadsheet_MoreTools = "Spreadsheet_MoreTools";

    /// <summary>The key for "Full screen. Press Escape to exit.". Announced when the spreadsheet goes full screen.</summary>
    public const string Spreadsheet_FullScreenOn = "Spreadsheet_FullScreenOn";

    /// <summary>The key for "Full screen closed". Announced when the spreadsheet leaves full screen.</summary>
    public const string Spreadsheet_FullScreenOff = "Spreadsheet_FullScreenOff";

    /// <summary>The key for "Sheet options". Name of the button that opens the menu of actions for the sheet being shown.</summary>
    public const string Spreadsheet_SheetMenu = "Spreadsheet_SheetMenu";

    /// <summary>The key for "Rename". Sheet menu item that renames the sheet.</summary>
    public const string Spreadsheet_RenameSheetAction = "Spreadsheet_RenameSheetAction";

    /// <summary>The key for "Move left". Sheet menu item that moves the sheet one place to the left.</summary>
    public const string Spreadsheet_MoveSheetLeft = "Spreadsheet_MoveSheetLeft";

    /// <summary>The key for "Move right". Sheet menu item that moves the sheet one place to the right.</summary>
    public const string Spreadsheet_MoveSheetRight = "Spreadsheet_MoveSheetRight";

    /// <summary>The key for "Delete sheet". Sheet menu item that deletes the sheet.</summary>
    public const string Spreadsheet_DeleteSheet = "Spreadsheet_DeleteSheet";

    /// <summary>The key for "Delete {0}?". Title of the dialog that confirms deleting a sheet, where {0} is its name.</summary>
    public const string Spreadsheet_DeleteSheetTitle = "Spreadsheet_DeleteSheetTitle";

    /// <summary>The key for "The sheet and everything on it will be removed. Formulas that use it will show #REF! until you undo.". Message of the dialog that confirms deleting a sheet.</summary>
    public const string Spreadsheet_DeleteSheetMessage = "Spreadsheet_DeleteSheetMessage";

    /// <summary>The key for "{0} deleted. Press Ctrl+Z to undo.". Announced after a sheet is deleted, where {0} is its name.</summary>
    public const string Spreadsheet_SheetDeleted = "Spreadsheet_SheetDeleted";

    /// <summary>The key for "Insert table". Name of the toolbar button and the dialog that turn a range into a table.</summary>
    public const string Spreadsheet_InsertTable = "Spreadsheet_InsertTable";

    /// <summary>The key for "Table range". Label of the field for the cells a new table covers.</summary>
    public const string Spreadsheet_TableRange = "Spreadsheet_TableRange";

    /// <summary>The key for "My table has headers". Checkbox in the insert table dialog: the first row holds the column headings.</summary>
    public const string Spreadsheet_TableHasHeaders = "Spreadsheet_TableHasHeaders";

    /// <summary>The key for "Enter a range such as A1:D10 that does not overlap another table.". Shown when the range for a new table is not valid.</summary>
    public const string Spreadsheet_TableRangeInvalid = "Spreadsheet_TableRangeInvalid";

    /// <summary>The key for "Create". Button that creates the table in the insert table dialog.</summary>
    public const string Spreadsheet_Create = "Spreadsheet_Create";

    /// <summary>The key for "{0} created". Announced after a table is created, where {0} is its name.</summary>
    public const string Spreadsheet_TableCreated = "Spreadsheet_TableCreated";

    /// <summary>The key for "Remove table". Context menu item that turns a table back into plain cells, keeping their contents.</summary>
    public const string Spreadsheet_RemoveTable = "Spreadsheet_RemoveTable";

    /// <summary>The key for "Column{0}". Heading given to a table column when the table has no headings, where {0} is its number.</summary>
    public const string Spreadsheet_DefaultColumnName = "Spreadsheet_DefaultColumnName";

    /// <summary>The key for "Table{0}". Name given to a new table, where {0} is its number.</summary>
    public const string Spreadsheet_DefaultTableName = "Spreadsheet_DefaultTableName";

    /// <summary>The key for "Filter {0}". Name of the filter button of a table column, and title of its dialog, where {0} is the heading.</summary>
    public const string Spreadsheet_FilterColumn = "Spreadsheet_FilterColumn";

    /// <summary>The key for "Filter {0}, filtered". Name of the filter button of a table column that is filtered, where {0} is the heading.</summary>
    public const string Spreadsheet_FilterColumnActive = "Spreadsheet_FilterColumnActive";

    /// <summary>The key for "Select all". Checkbox that shows or hides every value in a column filter.</summary>
    public const string Spreadsheet_SelectAllValues = "Spreadsheet_SelectAllValues";

    /// <summary>The key for "(Blanks)". Column filter entry for empty cells.</summary>
    public const string Spreadsheet_Blanks = "Spreadsheet_Blanks";

    /// <summary>The key for "Apply". Button that applies a column filter.</summary>
    public const string Spreadsheet_ApplyFilter = "Spreadsheet_ApplyFilter";

    /// <summary>The key for "Clear filter". Button that removes a column filter.</summary>
    public const string Spreadsheet_ClearFilter = "Spreadsheet_ClearFilter";

    /// <summary>The key for "{0} of {1} rows shown". Announced after a table is filtered, where {0} and {1} are numbers.</summary>
    public const string Spreadsheet_RowsShown = "Spreadsheet_RowsShown";

    /// <summary>The key for "Increase row height". Context menu item that makes the active row taller.</summary>
    public const string Spreadsheet_IncreaseRowHeight = "Spreadsheet_IncreaseRowHeight";

    /// <summary>The key for "Decrease row height". Context menu item that makes the active row shorter.</summary>
    public const string Spreadsheet_DecreaseRowHeight = "Spreadsheet_DecreaseRowHeight";

    /// <summary>The key for "Fit column to contents". Context menu item that makes the selected columns as wide as their widest content.</summary>
    public const string Spreadsheet_FitColumn = "Spreadsheet_FitColumn";

    /// <summary>The key for "Fit row to contents". Context menu item that makes the selected rows as tall as their tallest text.</summary>
    public const string Spreadsheet_FitRow = "Spreadsheet_FitRow";

    /// <summary>The key for "Sort". Heading of the sort buttons in a table column filter.</summary>
    public const string Spreadsheet_SortHeading = "Spreadsheet_SortHeading";

    /// <summary>The key for "Show rows where". Heading of the conditions in a table column filter, such as "is greater than 100".</summary>
    public const string Spreadsheet_FilterConditionHeading = "Spreadsheet_FilterConditionHeading";

    /// <summary>The key for "Values". Heading of the list of column values in a table column filter.</summary>
    public const string Spreadsheet_FilterValuesHeading = "Spreadsheet_FilterValuesHeading";

    /// <summary>The key for "Search values". Label of the field that narrows the list of values in a table column filter.</summary>
    public const string Spreadsheet_SearchValues = "Spreadsheet_SearchValues";
}
