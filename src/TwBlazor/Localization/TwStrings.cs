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
    public const string DataTable_Pagination = "DataTable_Pagination";

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
}
