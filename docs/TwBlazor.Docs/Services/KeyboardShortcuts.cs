namespace TwBlazor.Docs.Services;

/// <summary>
/// One row of a component's keyboard table: the key (or keys) and what pressing it does.
/// </summary>
/// <param name="Keys">The keys, each shown as its own key cap. Alternatives are separate entries.</param>
/// <param name="Action">What the key does, as a sentence.</param>
internal sealed record KeyboardShortcut(IReadOnlyList<string> Keys, string Action);

/// <summary>
/// The keyboard controls of one docs page.
/// </summary>
/// <param name="Summary">A sentence or two of context shown above the table.</param>
/// <param name="Shortcuts">The keys the component responds to. Empty for a component with none of its own.</param>
internal sealed record KeyboardGuide(string Summary, IReadOnlyList<KeyboardShortcut> Shortcuts);

/// <summary>
/// The keyboard controls of every component, keyed by the route of its docs page. It is the single place
/// they are written down, so the "Keyboard navigation" section on each page stays in step with the others
/// and with the conventions every component shares.
/// </summary>
/// <remarks>
/// The same keys mean the same thing everywhere: <c>Tab</c> moves between controls and never changes a
/// value, the arrow keys move within a composite control (a tab list, a tree, a grid, a list of options),
/// <c>Home</c> and <c>End</c> jump to its first and last item, <c>Enter</c> and <c>Space</c> activate, and
/// <c>Escape</c> closes whatever is open and returns focus to the control that opened it.
/// </remarks>
internal static class KeyboardShortcuts
{
    private const string Tab = "Tab";
    private const string ShiftTab = "Shift + Tab";
    private const string Enter = "Enter";
    private const string Space = "Space";
    private const string Escape = "Escape";
    private const string Up = "Arrow Up";
    private const string Down = "Arrow Down";
    private const string Left = "Arrow Left";
    private const string Right = "Arrow Right";
    private const string Home = "Home";
    private const string End = "End";
    private const string PageUp = "Page Up";
    private const string PageDown = "Page Down";

    private static KeyboardShortcut Key(string action, params string[] keys) => new(keys, action);

    private static KeyboardGuide Guide(string summary, params KeyboardShortcut[] shortcuts) => new(summary, shortcuts);

    private static readonly KeyboardGuide _notInteractive = Guide(
        "This component is not interactive, so it has no keyboard controls of its own and is not a Tab stop. "
        + "Anything interactive you place inside it keeps its own keyboard behaviour.");

    // Shared by every picker that opens a panel from a text field.
    private static readonly KeyboardShortcut[] _popoverField =
    [
        Key("Moves focus to the field. The panel stays closed, so you can tab straight past it.", Tab),
        Key("Opens the panel and moves focus into it.", Down),
        Key("On the icon button beside the field: opens the panel and moves focus into it, or closes it.", Enter, Space),
        Key("Commits a value typed into the field.", Enter),
        Key("Closes the panel and returns focus to the field. Works from the field and from inside the panel.", Escape),
        Key("Moves between the controls in the open panel. Focus stays in the panel until it is closed.", Tab, ShiftTab),
    ];

    private static readonly KeyboardShortcut[] _dateGrid =
    [
        Key("Moves to the previous or next day. Moving past the start or end of the month turns the page.", Left, Right),
        Key("Moves to the same weekday in the previous or next week.", Up, Down),
        Key("Moves to the first or last day of the week.", Home, End),
        Key("Moves to the previous or next month.", PageUp, PageDown),
        Key("Moves to the previous or next year.", "Shift + Page Up", "Shift + Page Down"),
        Key("Selects the focused day.", Enter, Space),
    ];

    private static readonly KeyboardShortcut[] _timeFields =
    [
        Key("In the hour or minute field: increases or decreases the value by one.", Up, Down),
        Key("On an arrow button or the AM/PM button: presses it.", Enter, Space),
    ];

    private static readonly KeyboardGuide _chart = Guide(
        "The plot is a single Tab stop. Each data point is announced with its series and value as focus reaches it, "
        + "and the table view presents the same data without the plot.",
        Key("Moves focus into the plot, onto the data point last visited.", Tab),
        Key("Moves to the next data point.", Right, Down),
        Key("Moves to the previous data point.", Left, Up),
        Key("Moves to the first or last data point.", Home, End),
        Key("Hides the tooltip of the focused data point.", Escape),
        Key("On a legend item: shows or hides that series. On the table button: switches between the chart and its data table.", Enter, Space));

    private static readonly Dictionary<string, KeyboardGuide> _guides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/alert"] = Guide(
            "An alert is read out by screen readers when it appears. Only a dismissible alert has a control to focus.",
            Key("Moves focus to the dismiss button.", Tab),
            Key("Dismisses the alert. Focus moves to the next control on the page.", Enter, Space)),

        ["/avatar"] = _notInteractive,

        ["/breadcrumb"] = Guide(
            "The breadcrumb is a navigation landmark holding ordinary links. The current page is text, not a link.",
            Key("Moves to the next link.", Tab),
            Key("Follows the focused link.", Enter)),

        ["/button"] = Guide(
            "A button is a native button, or a native link when it has an Href.",
            Key("Moves focus to the button. A read-only button stays focusable and is announced as unavailable.", Tab),
            Key("Activates the button.", Enter, Space),
            Key("Hides the button's tooltip, if it has one.", Escape)),

        ["/calendar"] = Guide(
            "Each view is a grid with a single Tab stop for its days or time slots. Events are buttons reached with Tab. "
            + "Dragging an event has a keyboard equivalent: open the event and change its start and end.",
            Key("Moves between the toolbar buttons, the grid, and the events.", Tab, ShiftTab),
            Key("Month view: moves to the previous or next day. Week view: moves to the same time on the previous or next day.", Left, Right),
            Key("Month view: moves to the same weekday in the previous or next week. Day and week view: moves to the previous or next time slot.", Up, Down),
            Key("Month view: moves to the first or last day of the week. Day and week view: moves to the first or last time slot.", Home, End),
            Key("On a day: opens that day. On a time slot: starts a new event there. On an event: opens it.", Enter, Space),
            Key("Closes the event dialog.", Escape)),

        ["/card"] = _notInteractive,

        ["/carousel"] = Guide(
            "The arrow keys change slide from anywhere in the carousel, except while you are typing in a field or using "
            + "a control inside a slide that needs them. An automatic slideshow pauses while the carousel has focus, "
            + "and starts paused for anyone who has asked their system for reduced motion.",
            Key("Moves through the pause button (when the slideshow plays automatically), the slide's own controls, the previous and next buttons, and the slide indicators.", Tab, ShiftTab),
            Key("Shows the previous or next slide.", Left, Right),
            Key("Presses the focused button: pause or play, previous, next, or a slide indicator.", Enter, Space)),

        ["/checkbox"] = Guide(
            "A checkbox is a native checkbox, so it behaves as one. A read-only checkbox can be focused but not changed.",
            Key("Moves focus to the checkbox.", Tab),
            Key("Checks or unchecks it.", Space)),

        ["/chip"] = Guide(
            "A plain chip is not a Tab stop. A clickable chip is a button or a link, and a closable chip adds a remove button after it.",
            Key("Moves to the chip, then to its remove button.", Tab),
            Key("Activates a clickable chip, or removes the chip when its remove button has focus.", Enter, Space)),

        ["/collapse"] = Guide(
            "The header is a button that reports whether its content is expanded. Collapsed content is hidden from the Tab order.",
            Key("Moves focus to the header.", Tab),
            Key("Expands or collapses the content.", Enter, Space)),

        ["/color-picker"] = Guide(
            "The swatch opens the picker panel and the text field accepts a typed color. Inside the panel each slider is its own Tab stop.",
            Key("On the swatch: opens the panel and moves focus into it.", Enter, Space),
            Key("Moves between the controls in the open panel. Focus stays in the panel until it is closed.", Tab, ShiftTab),
            Key("Saturation and lightness square: changes saturation.", Left, Right),
            Key("Saturation and lightness square: changes lightness.", Up, Down),
            Key("Hue and alpha sliders: decreases or increases the value.", Left, Right),
            Key("Sets the focused slider to its minimum or maximum.", Home, End),
            Key("Presses the focused button: the eyedropper, the color format button, Cancel, or Confirm.", Enter, Space),
            Key("Closes the panel without applying the color and returns focus to the swatch.", Escape)),

        ["/data-table"] = Guide(
            "The table is read as a native table. Its controls are ordinary buttons and fields, and sort, search and page changes are announced.",
            Key("Moves through the search field, the sort buttons in the header, the table's scroll area, and the pagination.", Tab, ShiftTab),
            Key("On a sort button: sorts by that column, then reverses the order, then clears it.", Enter, Space),
            Key("In the table's scroll area: scrolls a table that is wider or taller than its container.", Left, Right, Up, Down),
            Key("Commits the search text.", Enter)),

        ["/date-picker"] = Guide(
            "Type a date straight into the field, or open the calendar. The calendar does not open just because the field has focus.",
            [.. _popoverField, .. _dateGrid]),

        ["/date-range-picker"] = Guide(
            "Type a range into the field, or open the calendar, which shows two months side by side. Pick the start day, then the end day.",
            [.. _popoverField, .. _dateGrid]),

        ["/datetime-picker"] = Guide(
            "Type a date and time into the field, or open the panel, which holds a calendar above an hour and minute picker.",
            [.. _popoverField, .. _dateGrid, .. _timeFields]),

        ["/datetime-range-picker"] = Guide(
            "The panel sets one end of the range at a time. The Start and End buttons at the top switch between them.",
            [.. _popoverField, Key("On the Start or End button: switches to that end of the range.", Enter, Space), .. _dateGrid, .. _timeFields]),

        ["/dialog"] = Guide(
            "Opening a dialog moves focus into it and makes the page behind it unreachable. Closing it returns focus to the control that opened it.",
            Key("Moves to the next control in the dialog. From the last control it wraps to the first.", Tab),
            Key("Moves to the previous control in the dialog. From the first control it wraps to the last.", ShiftTab),
            Key("Closes the dialog, wherever focus is. If a picker or a select list is open inside the dialog, the first press closes that and the next press closes the dialog. Has no effect when CloseOnEscapeKey is off.", Escape)),

        ["/file-upload"] = Guide(
            "The upload control is a native file input. Each chosen file is listed with its own remove button, and choosing or removing a file is announced.",
            Key("Moves to the upload control, then to each file's remove button.", Tab),
            Key("On the upload control: opens the system file dialog.", Enter, Space),
            Key("On a remove button: removes that file. Focus returns to the upload control.", Enter, Space)),

        ["/icon"] = Guide(
            "An icon is decorative and is not a Tab stop. An icon with OnClick is a button.",
            Key("Moves focus to a clickable icon.", Tab),
            Key("Activates a clickable icon.", Enter, Space),
            Key("Hides the icon's tooltip, if it has one.", Escape)),

        ["/navbar"] = Guide(
            "The navbar is a navigation landmark holding ordinary links. On a narrow screen the links sit behind a menu button.",
            Key("Moves to the next link or button.", Tab),
            Key("Follows the focused link.", Enter),
            Key("On the menu button: opens or closes the menu.", Enter, Space)),

        ["/pagination"] = Guide(
            "The pagination is a navigation landmark. The previous and next buttons stay focusable on the first and last page, where they are announced as unavailable.",
            Key("Moves between the previous button, the page buttons, the next button and the page size list.", Tab, ShiftTab),
            Key("Goes to that page. The new page number is announced.", Enter, Space)),

        ["/pick-list"] = Guide(
            "Each list is a single Tab stop. Select items in a list, then use the buttons between the lists to move them. Every move is announced.",
            Key("Moves between the two lists and the buttons.", Tab, ShiftTab),
            Key("Moves to the previous or next item in the list.", Up, Down),
            Key("Moves to the first or last item in the list.", Home, End),
            Key("Selects the focused item, or clears its selection.", Space, Enter),
            Key("On a button: moves the selected items across, moves every item across, or moves the selection up or down.", Enter, Space)),

        ["/progress"] = Guide(
            "A progress bar is not interactive and is not a Tab stop. Screen readers announce its label and value, or that it is busy when it is indeterminate."),

        ["/radio-button"] = Guide(
            "Radio buttons are native radio inputs. A group is a single Tab stop.",
            Key("Moves focus into the group, onto the selected option, or the first option when none is selected.", Tab),
            Key("Moves to the next option and selects it.", Down, Right),
            Key("Moves to the previous option and selects it.", Up, Left),
            Key("Selects the focused option when none is selected yet.", Space)),

        ["/select"] = Guide(
            "The list opens below the field and focus moves into it. On a phone or tablet the browser's own picker is used.",
            Key("Opens the list.", Enter, Space, Down, Up),
            Key("Moves to the previous or next option.", Up, Down),
            Key("Moves to the first or last option.", Home, End),
            Key("Moves to the next option starting with the typed letters.", "A to Z"),
            Key("Selects the focused option and closes the list. In a multiple select, adds or removes the option and keeps the list open.", Enter, Space),
            Key("Closes the list without changing the value and returns focus to the field.", Escape, Tab),
            Key("On a selected item's remove button in a multiple select: removes that item.", Enter, Space)),

        ["/sidebar"] = Guide(
            "The first Tab stop on the page is a link that skips the navigation. A closed sidebar is taken out of the Tab order entirely. "
            + "On a narrow screen the sidebar opens over the page as a drawer: focus moves into it and the page behind it cannot be reached until it closes.",
            Key("From the top of the page: shows the \"Skip to main content\" link.", Tab),
            Key("On the skip link: moves focus to the main content.", Enter),
            Key("On the sidebar button: opens or closes the sidebar.", Enter, Space),
            Key("Moves through the search field and the navigation items.", Tab, ShiftTab),
            Key("On a group: expands or collapses it.", Enter, Space),
            Key("On a link: goes to that page.", Enter),
            Key("Closes the sidebar while it is open as a drawer, and returns focus to the sidebar button.", Escape)),

        ["/skeleton"] = Guide(
            "A skeleton is not interactive and is not a Tab stop. While it is showing, screen readers are told the content is loading."),

        ["/slider"] = Guide(
            "A slider is a native range input. A read-only slider can be focused but its value cannot be changed.",
            Key("Moves focus to the slider.", Tab),
            Key("Increases the value by one step.", Right, Up),
            Key("Decreases the value by one step.", Left, Down),
            Key("Sets the minimum or maximum value.", Home, End),
            Key("Changes the value by a larger amount.", PageUp, PageDown)),

        ["/spinner"] = Guide(
            "A spinner is not interactive and is not a Tab stop. Screen readers announce its label when it appears."),

        ["/stepper"] = Guide(
            "The steps are a navigation landmark. Each step you can go to is a button, and the step you land on is announced.",
            Key("Moves between the steps you can go to. In a linear stepper those are the current step and the ones before it.", Tab, ShiftTab),
            Key("Goes to the focused step.", Enter, Space)),

        ["/switch"] = Guide(
            "A switch is announced as a switch that is on or off. A read-only switch can be focused but not changed.",
            Key("Moves focus to the switch.", Tab),
            Key("Turns it on or off.", Space)),

        ["/table"] = Guide(
            "The table is read as a native table. Its scroll area is a Tab stop so a table wider or taller than its container can be scrolled from the keyboard.",
            Key("Moves focus to the table's scroll area.", Tab),
            Key("Scrolls the table.", Left, Right, Up, Down)),

        ["/tabs"] = Guide(
            "The tab list is a single Tab stop, and moving to a tab shows its panel straight away. Disabled tabs are skipped.",
            Key("Moves focus to the selected tab, then to the tab panel, then on to the panel's content.", Tab),
            Key("Moves to the next tab and shows it. From the last tab it wraps to the first.", Right, Down),
            Key("Moves to the previous tab and shows it. From the first tab it wraps to the last.", Left, Up),
            Key("Moves to the first or last tab.", Home, End)),

        ["/textfield"] = Guide(
            "A text field is a native input, so every text editing key works as usual. An error message is announced when it appears.",
            Key("Moves focus to the field.", Tab),
            Key("Commits the value, when the field binds on change.", Enter)),

        ["/time-picker"] = Guide(
            "Type a time straight into the field, or open the panel. The panel does not open just because the field has focus.",
            [.. _popoverField, .. _timeFields]),

        ["/time-range-picker"] = Guide(
            "The panel sets one end of the range at a time. The Start and End buttons at the top switch between them.",
            [.. _popoverField, Key("On the Start or End button: switches to that end of the range.", Enter, Space), .. _timeFields]),

        ["/toast"] = Guide(
            "A toast is announced when it appears: errors and warnings at once, everything else when the screen reader is idle. "
            + "Its timer pauses while it has focus or the pointer is over it, and toasts stay reachable while a dialog is open.",
            Key("Moves focus to a toast's close button.", Tab),
            Key("Closes the toast.", Enter, Space)),

        ["/tooltip"] = Guide(
            "A tooltip appears when its content gets keyboard focus, as well as on hover. Wrapped around a button or a link it "
            + "describes that control and adds no Tab stop of its own; around plain content the wrapper is the Tab stop.",
            Key("Moves focus to the content and shows its tooltip.", Tab),
            Key("Hides the tooltip without moving focus.", Escape)),

        ["/tree-list"] = Guide(
            "The tree is a single Tab stop. Collapsed branches are skipped, and disabled nodes cannot be focused.",
            Key("Moves focus into the tree, onto the node last visited.", Tab),
            Key("Moves to the previous or next visible node.", Up, Down),
            Key("On a collapsed node: expands it. On an expanded node: moves to its first child.", Right),
            Key("On an expanded node: collapses it. On any other node: moves to its parent.", Left),
            Key("Moves to the first or last visible node.", Home, End),
            Key("Activates the node: expands or collapses it and raises OnClick.", Enter),
            Key("With checkboxes: checks or unchecks the node and everything under it. Without: activates the node.", Space)),
    };

    /// <summary>
    /// Returns the keyboard controls for the docs page at <paramref name="path"/>, or
    /// <see langword="null"/> when that page does not document a component.
    /// </summary>
    public static KeyboardGuide? ForPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (_guides.TryGetValue(path, out var guide))
        {
            return guide;
        }

        // Every chart shares one plot, so every chart page shares one set of keys.
        if (path.Equals("/chart", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/chart/", StringComparison.OrdinalIgnoreCase))
        {
            return _chart;
        }

        return null;
    }

    /// <summary>
    /// The routes that have an explicit entry, for tests that check every component page is covered.
    /// </summary>
    public static IReadOnlyCollection<string> DocumentedPaths => _guides.Keys;
}
