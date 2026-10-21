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
    private const string tab = "Tab";
    private const string shiftTab = "Shift + Tab";
    private const string enter = "Enter";
    private const string space = "Space";
    private const string escape = "Escape";
    private const string up = "Arrow Up";
    private const string down = "Arrow Down";
    private const string left = "Arrow Left";
    private const string right = "Arrow Right";
    private const string home = "Home";
    private const string end = "End";
    private const string pageUp = "Pg.Up";
    private const string pageDown = "Pg.Down";

    private static KeyboardShortcut Key(string action, params string[] keys) => new(keys, action);

    private static KeyboardGuide Guide(string summary, params KeyboardShortcut[] shortcuts) => new(summary, shortcuts);

    private static readonly KeyboardGuide _notInteractive = Guide(
        "This component is not interactive, so it has no keyboard controls of its own and is not a Tab stop. "
        + "Anything interactive you place inside it keeps its own keyboard behaviour.");

    // Shared by every picker that opens a panel from a text field.
    private static readonly KeyboardShortcut[] _popoverField =
    [
        Key("Moves focus to the field. The panel stays closed, so you can tab straight past it.", tab),
        Key("Opens the panel and moves focus into it.", down),
        Key("On the icon button beside the field: opens the panel and moves focus into it, or closes it.", enter, space),
        Key("Commits a value typed into the field.", enter),
        Key("Closes the panel and returns focus to the field. Works from the field and from inside the panel.", escape),
        Key("Moves between the controls in the open panel. Focus stays in the panel until it is closed.", tab, shiftTab),
    ];

    private static readonly KeyboardShortcut[] _dateGrid =
    [
        Key("Moves to the previous or next day. Moving past the start or end of the month turns the page. The two keys swap in a right to left layout.", left, right),
        Key("Moves to the same weekday in the previous or next week.", up, down),
        Key("Moves to the first or last day of the week.", home, end),
        Key("Moves to the previous or next month.", pageUp, pageDown),
        Key("Moves to the previous or next year.", "Shift + Page Up", "Shift + Page Down"),
        Key("Selects the focused day.", enter, space),
        Key("On the month and year heading: switches to the list of months, then of years. Focus lands on the current one, and Tab moves between them.", enter, space),
    ];

    private static readonly KeyboardShortcut[] _timeFields =
    [
        Key("In the hour or minute field: increases or decreases the value by one, or moves to the next or previous step when Step is set. When Step is a whole number of hours, the minutes are fixed text and are skipped by Tab.", up, down),
        Key("On an arrow button or the AM/PM button: presses it.", enter, space),
    ];

    private static readonly KeyboardGuide _chart = Guide(
        "The plot is a single Tab stop. Each data point is announced with its series and value as focus reaches it, "
        + "and the table view presents the same data without the plot.",
        Key("Moves focus into the plot, onto the data point last visited.", tab),
        Key("Moves to the next data point.", right, down),
        Key("Moves to the previous data point.", left, up),
        Key("Moves to the first or last data point.", home, end),
        Key("Hides the tooltip of the focused data point.", escape),
        Key("On a legend item: shows or hides that series. On the table button: switches between the chart and its data table.", enter, space));

    private static readonly Dictionary<string, KeyboardGuide> _guides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/alert"] = Guide(
            "An alert is read out by screen readers when it appears. Only a dismissible alert has a control to focus.",
            Key("Moves focus to the dismiss button.", tab),
            Key("Dismisses the alert. Focus moves to the next control on the page.", enter, space)),

        ["/attachment"] = Guide(
            "An attachment is a group named after its file. Its file name is a link once the file is ready, and its retry and remove buttons name the file they act on. "
            + "Starting, finishing and failing an upload are announced.",
            Key("Moves to the file name link, then to the retry and remove buttons.", tab, shiftTab),
            Key("Opens the file, retries the upload, or removes the attachment, depending on what has focus.", enter)),

        ["/avatar"] = _notInteractive,

        ["/barcode"] = Guide(
            "A barcode is not interactive and is not a Tab stop. Screen readers announce it as an image named after the text it holds, "
            + "unless it is given a label of its own."),

        ["/bubble"] = Guide(
            "A bubble is not a Tab stop. Its reactions are a group named \"Reactions\". Clickable reactions are toggle buttons that say whether you chose them.",
            Key("Moves between the clickable reactions.", tab, shiftTab),
            Key("Adds or removes your reaction.", enter, space)),

        ["/breadcrumb"] = Guide(
            "The breadcrumb is a navigation landmark holding ordinary links. The current page is text, not a link.",
            Key("Moves to the next link.", tab),
            Key("Follows the focused link.", enter)),

        ["/button"] = Guide(
            "A button is a native button, or a native link when it has an Href.",
            Key("Moves focus to the button. A read-only button stays focusable and is announced as unavailable.", tab),
            Key("Activates the button.", enter, space),
            Key("Hides the button's tooltip, if it has one.", escape)),

        ["/calendar"] = Guide(
            "Each view is a grid with a single Tab stop for its days or time slots. Events are buttons reached with Tab. A read-only calendar has no time slots to focus, only its events. "
            + "Dragging an event has a keyboard equivalent: open the event and change its start and end.",
            Key("Moves between the toolbar buttons, the grid, and the events.", tab, shiftTab),
            Key("Month view: moves to the previous or next day. Week view: moves to the same time on the previous or next day. The two keys swap in a right to left layout.", left, right),
            Key("Month view: moves to the same weekday in the previous or next week. Day and week view: moves to the previous or next time slot.", up, down),
            Key("Month view: moves to the first or last day of the week. Day and week view: moves to the first or last time slot.", home, end),
            Key("On a day: opens that day and moves focus to the calendar's title, which names it. On a time slot: starts a new event there. On an event: opens it.", enter, space),
            Key("Closes the event dialog.", escape)),

        ["/card"] = _notInteractive,

        ["/carousel"] = Guide(
            "The arrow keys change slide from anywhere in the carousel, except while you are typing in a field or using "
            + "a control inside a slide that needs them. An automatic slideshow pauses while the carousel has focus, "
            + "and starts paused for anyone who has asked their system for reduced motion.",
            Key("Moves through the pause button (when the slideshow plays automatically), the slide's own controls, the previous and next buttons, and the slide indicators.", tab, shiftTab),
            Key("Shows the previous or next slide. The two keys swap in a right to left layout.", left, right),
            Key("Presses the focused button: pause or play, previous, next, or a slide indicator.", enter, space)),

        ["/checkbox"] = Guide(
            "A checkbox is a native checkbox, so it behaves as one. A read-only checkbox can be focused but not changed.",
            Key("Moves focus to the checkbox.", tab),
            Key("Checks or unchecks it.", space)),

        ["/chip"] = Guide(
            "A plain chip is not a Tab stop. A clickable chip is a button or a link, and a closable chip adds a remove button after it.",
            Key("Moves to the chip, then to its remove button.", tab),
            Key("Activates a clickable chip, or removes the chip when its remove button has focus.", enter, space)),

        ["/collapse"] = Guide(
            "The header is a button that reports whether its content is expanded. Collapsed content is hidden from the Tab order.",
            Key("Moves focus to the header.", tab),
            Key("Expands or collapses the content.", enter, space)),

        ["/color-picker"] = Guide(
            "The swatch opens the picker panel and the text field accepts a typed color. Inside the panel each slider is its own Tab stop.",
            Key("On the swatch: opens the panel and moves focus into it.", enter, space),
            Key("Moves between the controls in the open panel. Focus stays in the panel until it is closed.", tab, shiftTab),
            Key("Saturation and lightness square: changes saturation.", left, right),
            Key("Saturation and lightness square: changes lightness.", up, down),
            Key("Hue and alpha sliders: decreases or increases the value.", left, right),
            Key("Sets the focused slider to its minimum or maximum.", home, end),
            Key("Presses the focused button: the eyedropper, the color format button, Cancel, or Confirm.", enter, space),
            Key("Closes the panel without applying the color and returns focus to the swatch.", escape)),

        ["/data-table"] = Guide(
            "The table is read as a native table. Its controls are ordinary buttons, menus and fields, and sort, search, filter, page, grouping and column changes are announced. "
            + "With ResizableColumns, each column's border is a separator that reports the column's width.",
            Key("Moves through the toolbar, each column's sort, filter and options buttons, the resize separators, the table's scroll area and the pagination.", tab, shiftTab),
            Key("On a sort button: sorts by that column, then reverses the order, then clears it.", enter, space),
            Key("In the table's scroll area: scrolls a table that is wider or taller than its container. A virtualized table renders the rows as they scroll into view.", left, right, up, down, pageUp, pageDown),
            Key("Commits the search text.", enter),
            Key("On a column's options button: opens its menu, on the first or last item.", enter, space, down, up),
            Key("In a column's menu: moves between its items, chooses one, or closes it. A column toggle flips and the menu stays open.", up, down, enter, space, escape),
            Key("On a column's filter button: opens its filter with focus on the first condition.", enter, space),
            Key("In a filter: moves between its fields and buttons, applies the filter, or closes it without changing it. Focus returns to the filter button.", tab, enter, escape),
            Key("On a resize separator: makes the column wider or narrower, in bigger steps with Shift. The key moves the border the way it points, in either text direction.", left, right),
            Key("On an export button: downloads the table, then announces how many rows it holds.", enter, space),
            Key("On a group header's button: expands or collapses that group. The button says whether the group is expanded.", enter, space),
            Key("On the Group by list: opens it to choose a column to group by, or No grouping.", enter, space, down),
            Key("On a row's Edit button: turns the row into editors and moves focus to the first one.", enter, space),
            Key("While a row is edited: moves between its editors and the Save and Cancel buttons.", tab, shiftTab),
            Key("While a row is edited: cancels the edit and returns focus to the row's Edit button. A picker or list open in the row closes first.", escape)),

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
            [.. _popoverField, Key("On the Start or End button: switches to that end of the range.", enter, space), .. _dateGrid, .. _timeFields]),

        ["/dialog"] = Guide(
            "Opening a dialog moves focus into it and makes the page behind it unreachable. Closing it returns focus to the control that opened it.",
            Key("Moves to the next control in the dialog. From the last control it wraps to the first.", tab),
            Key("Moves to the previous control in the dialog. From the first control it wraps to the last.", shiftTab),
            Key("Closes the dialog, wherever focus is. If a picker or a select list is open inside the dialog, the first press closes that and the next press closes the dialog. Has no effect when CloseOnEscapeKey is off.", escape)),

        ["/file-upload"] = Guide(
            "The upload control is a native file input. Each chosen file is listed with its own remove button, and choosing or removing a file is announced.",
            Key("Moves to the upload control, then to each file's remove button.", tab),
            Key("On the upload control: opens the system file dialog.", enter, space),
            Key("On a remove button: removes that file. Focus returns to the upload control.", enter, space)),

        ["/icon"] = Guide(
            "An icon is decorative and is not a Tab stop. An icon with OnClick is a button.",
            Key("Moves focus to a clickable icon.", tab),
            Key("Activates a clickable icon.", enter, space),
            Key("Hides the icon's tooltip, if it has one.", escape)),

        ["/footer"] = Guide(
            "The footer is the page's content information landmark. Its sections sit in a navigation landmark, each a heading and a list of ordinary links.",
            Key("Moves to the next or previous link.", tab, shiftTab),
            Key("Follows the focused link. A link that opens in a new tab says so before you follow it.", enter)),

        ["/marker"] = Guide(
            "A marker is not interactive and is not a Tab stop. A status marker writes its state out for screen readers after its label, and a live one announces changes. "
            + "A separator marker is read as a separator named by its label."),

        ["/menu"] = Guide(
            "The button tells screen readers that it opens a menu and whether the menu is open. The menu follows the ARIA menu pattern: one item has focus at a time, and the arrow keys move between them.",
            Key("Moves focus to the menu's button.", tab),
            Key("On the button: opens the menu with focus on its first item.", enter, space, down),
            Key("On the button: opens the menu with focus on its last item.", up),
            Key("In the menu: moves to the next or previous item, wrapping round at either end. Disabled items are skipped.", down, up),
            Key("In the menu: moves to the first or last item.", home, end),
            Key("In the menu: moves to the next item that starts with the typed letter.", "A to Z"),
            Key("In the menu: chooses the focused item. An action closes the menu and returns focus to the button; a tick option flips and the menu stays open.", enter, space),
            Key("Closes the menu and returns focus to the button.", escape),
            Key("Closes the menu and moves focus on.", tab)),

        ["/message"] = Guide(
            "A message is not a Tab stop. Each message is an article named by its author and time, so screen readers can move between messages with their article shortcut. "
            + "Anything interactive inside a message, such as reactions or actions in its footer, keeps its own keyboard behaviour."),

        ["/navbar"] = Guide(
            "The navbar is a navigation landmark holding ordinary links. On a narrow screen the links sit behind a menu button.",
            Key("Moves to the next link or button.", tab),
            Key("Follows the focused link.", enter),
            Key("On the menu button: opens or closes the menu.", enter, space)),

        ["/numeric"] = Guide(
            "A numeric field is a text input with the spinbutton role, so screen readers announce its value and limits. "
            + "Its clear, decrease and increase buttons are not Tab stops, because the keys below do the same job.",
            Key("Moves focus into or out of the field.", tab, shiftTab),
            Key("Increases or decreases the value by one step, stopping at Max or Min.", up, down),
            Key("Increases or decreases the value by ten steps.", pageUp, pageDown),
            Key("Commits the typed value, when the field binds on change.", enter),
            Key("Moves the caret through the text.", left, right, home, end)),

        ["/on-this-page"] = Guide(
            "The list is a navigation landmark holding ordinary links. The link to the section being read is announced as the current location.",
            Key("Moves to the next or previous link.", tab, shiftTab),
            Key("Jumps to the focused link's section.", enter)),

        ["/pagination"] = Guide(
            "The pagination is a navigation landmark. The previous and next buttons stay focusable on the first and last page, where they are announced as unavailable.",
            Key("Moves between the previous button, the page buttons, the next button and the page size list.", tab, shiftTab),
            Key("Goes to that page. The new page number is announced.", enter, space)),

        ["/pick-list"] = Guide(
            "Each list is a single Tab stop. Select items in a list, then use the buttons between the lists to move them. Every move is announced.",
            Key("Moves between the two lists and the buttons.", tab, shiftTab),
            Key("Moves to the previous or next item in the list.", up, down),
            Key("Moves to the first or last item in the list.", home, end),
            Key("Selects the focused item, or clears its selection.", space, enter),
            Key("On a button: moves the selected items across, moves every item across, or moves the selection up or down.", enter, space)),

        ["/popover"] = Guide(
            "The button tells screen readers that it opens a dialog and whether it is open. The panel is a non-modal dialog named by its title, or else by the button.",
            Key("Moves focus to the popover's button.", tab),
            Key("On the button: opens the panel and moves focus into it, or closes it.", enter, space),
            Key("Moves between the controls in the panel. Tabbing out of the panel closes it.", tab, shiftTab),
            Key("Closes the panel and returns focus to the button.", escape)),

        ["/progress"] = Guide(
            "A progress bar is not interactive and is not a Tab stop. Screen readers announce its label and value, or that it is busy when it is indeterminate."),

        ["/qr-code"] = Guide(
            "A QR code is not interactive and is not a Tab stop. Screen readers announce it as an image named after the text it holds, "
            + "unless it is given a label of its own."),

        ["/radio-button"] = Guide(
            "Radio buttons are native radio inputs. A group is a single Tab stop.",
            Key("Moves focus into the group, onto the selected option, or the first option when none is selected.", tab),
            Key("Moves to the next option and selects it.", down, right),
            Key("Moves to the previous option and selects it.", up, left),
            Key("Selects the focused option when none is selected yet.", space)),

        ["/rating"] = Guide(
            "A rating is a group of native radio inputs, one for each value, and is a single Tab stop. A read-only rating is not a Tab stop: "
            + "screen readers announce it as an image named after its value.",
            Key("Moves focus into the rating, onto the selected value.", tab),
            Key("Selects the next value, half an icon or a whole icon at a time.", right, up),
            Key("Selects the previous value. Going below the first value selects No rating, when the rating can be cleared.", left, down)),

        ["/autocomplete"] = Guide(
            "Focus stays in the text field while you move through the suggestions, so you can keep typing to narrow them down. Typing opens the list.",
            Key("Opens the list and highlights the first or last suggestion. With the list open, moves to the next or previous suggestion, wrapping at the ends.", down, up),
            Key("Opens the list without highlighting a suggestion.", $"Alt + {down}"),
            Key("Selects the highlighted suggestion, puts its text in the field and closes the list.", enter),
            Key("Closes the list. Focus stays in the field.", escape),
            Key("Moves the cursor to the start or end of the text.", home, end),
            Key("Leaves the field. Text that matches no suggestion is replaced by the selected value's text, and emptying the field clears the selection.", tab)),

        ["/select"] = Guide(
            "The list opens below the field and focus moves into it. On a phone or tablet the browser's own picker is used.",
            Key("Opens the list.", enter, space, down, up),
            Key("Moves to the previous or next option.", up, down),
            Key("Moves to the first or last option.", home, end),
            Key("Moves to the next option starting with the typed letters.", "A to Z"),
            Key("Selects the focused option and closes the list. In a multiple select, adds or removes the option and keeps the list open.", enter, space),
            Key("Closes the list without changing the value and returns focus to the field.", escape, tab),
            Key("On a clearable select with the list closed: clears the selection.", "Delete", "Backspace"),
            Key("On a selected item's remove button in a multiple select: removes that item and returns focus to the field.", enter, space)),

        ["/sidebar"] = Guide(
            "The first Tab stop on the page is a link that skips the navigation. A closed sidebar is taken out of the Tab order entirely. "
            + "On a narrow screen the sidebar opens over the page as a drawer: focus moves into it and the page behind it cannot be reached until it closes.",
            Key("From the top of the page: shows the \"Skip to main content\" link.", tab),
            Key("On the skip link: moves focus to the main content.", enter),
            Key("On the sidebar button: opens or closes the sidebar.", enter, space),
            Key("Moves through the search field and the navigation items.", tab, shiftTab),
            Key("On a group: expands or collapses it.", enter, space),
            Key("On a link: goes to that page.", enter),
            Key("Closes the sidebar while it is open as a drawer, from wherever focus is, and returns focus to the sidebar button. In the search field the first press clears the text.", escape),
            Key("On the close button inside the drawer: closes it.", enter, space)),

        ["/skeleton"] = Guide(
            "A skeleton is not interactive and is not a Tab stop. While it is showing, screen readers are told the content is loading."),

        ["/slider"] = Guide(
            "A slider is a native range input. A read-only slider can be focused but its value cannot be changed.",
            Key("Moves focus to the slider.", tab),
            Key("Increases the value by one step.", right, up),
            Key("Decreases the value by one step.", left, down),
            Key("Sets the minimum or maximum value.", home, end),
            Key("Changes the value by a larger amount.", pageUp, pageDown)),

        ["/spinner"] = Guide(
            "A spinner is not interactive and is not a Tab stop. Screen readers announce its label when it appears."),

        ["/stepper"] = Guide(
            "The steps are a navigation landmark. Each step you can go to is a button, and the step you land on is announced.",
            Key("Moves between the steps you can go to. In a linear stepper those are the current step and the ones before it.", tab, shiftTab),
            Key("Goes to the focused step.", enter, space)),

        ["/switch"] = Guide(
            "A switch is announced as a switch that is on or off. A read-only switch can be focused but not changed.",
            Key("Moves focus to the switch.", tab),
            Key("Turns it on or off.", space)),

        ["/table"] = Guide(
            "The table is read as a native table. Its scroll area is a Tab stop so a table wider or taller than its container can be scrolled from the keyboard. "
            + "With ResizableColumns, each resizable column has a separator on its header's border that reports the column's width.",
            Key("Moves focus to the table's scroll area, then to each column's resize separator and the pagination.", tab, shiftTab),
            Key("In the scroll area: scrolls the table. A virtualized table renders the rows as they scroll into view.", left, right, up, down, pageUp, pageDown),
            Key("On a resize separator: makes the column wider or narrower. The key moves the border the way it points, in either text direction.", left, right),
            Key("On a resize separator: resizes in bigger steps.", "Shift + Arrow Left", "Shift + Arrow Right"),
            Key("In the pagination: moves to a page, or opens the page size list.", enter, space)),

        ["/tabs"] = Guide(
            "The tab list is a single Tab stop, and moving to a tab shows its panel straight away. Disabled tabs are skipped. "
            + "In a right to left layout Arrow Left moves to the next tab and Arrow Right to the previous one.",
            Key("Moves focus to the selected tab, then to the tab panel, then on to the panel's content.", tab),
            Key("Moves to the next tab and shows it. From the last tab it wraps to the first.", right, down),
            Key("Moves to the previous tab and shows it. From the first tab it wraps to the last.", left, up),
            Key("Moves to the first or last tab.", home, end)),

        ["/textfield"] = Guide(
            "A text field is a native input, so every text editing key works as usual. An error message is announced when it appears. "
            + "The clear button of a clearable field is not a Tab stop, because the text can be cleared from the keyboard.",
            Key("Moves focus into or out of the field, including a multiline field.", tab, shiftTab),
            Key("Commits the value, when the field binds on change. In a multiline field, inserts a new line instead.", enter),
            Key("Moves the caret through the text, line by line in a multiline field.", up, down, left, right),
            Key("Moves the caret to the start or end of the line.", home, end),
            Key("In a multiline field: moves the caret up or down by a visible page of text.", pageUp, pageDown)),

        ["/time-picker"] = Guide(
            "Type a time straight into the field, or open the panel. The panel does not open just because the field has focus.",
            [.. _popoverField, .. _timeFields]),

        ["/time-range-picker"] = Guide(
            "The panel sets one end of the range at a time. The Start and End buttons at the top switch between them.",
            [.. _popoverField, Key("On the Start or End button: switches to that end of the range.", enter, space), .. _timeFields]),

        ["/toast"] = Guide(
            "A toast is announced when it appears: errors and warnings at once, everything else when the screen reader is idle. "
            + "Its timer pauses while it has focus or the pointer is over it, and toasts stay reachable while a dialog is open.",
            Key("Moves focus to a toast's close button.", tab),
            Key("Closes the toast.", enter, space)),

        ["/tooltip"] = Guide(
            "A tooltip appears when its content gets keyboard focus, as well as on hover. Wrapped around a button or a link it "
            + "describes that control and adds no Tab stop of its own; around plain content the wrapper is the Tab stop.",
            Key("Moves focus to the content and shows its tooltip.", tab),
            Key("Hides the tooltip without moving focus.", escape)),

        ["/tree-list"] = Guide(
            "The tree is a single Tab stop. Collapsed branches are skipped, and disabled nodes cannot be focused. "
            + "In a right to left layout Arrow Left and Arrow Right swap roles.",
            Key("Moves focus into the tree, onto the node last visited.", tab),
            Key("Moves to the previous or next visible node.", up, down),
            Key("On a collapsed node: expands it. On an expanded node: moves to its first child.", right),
            Key("On an expanded node: collapses it. On any other node: moves to its parent.", left),
            Key("Moves to the first or last visible node.", home, end),
            Key("Activates the node: expands or collapses it and raises OnClick.", enter),
            Key("With checkboxes: checks or unchecks the node and everything under it. Without: activates the node.", space)),
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
