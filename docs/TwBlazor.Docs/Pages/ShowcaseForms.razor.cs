using System.Globalization;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Pages;

/// <summary>
/// The Forms tab of the landing page's showcase: a column of example forms with their own state.
/// </summary>
/// <remarks>
/// A slider and the colour picker report every step of a drag. Kept in the landing page, each step re-rendered the
/// whole page, so this tab is a component of its own, and the two cards with those controls are components of
/// their own inside it (<see cref="ShowcaseDatabaseCard"/>, <see cref="ShowcaseWorkspaceCard"/>).
/// </remarks>
public partial class ShowcaseForms
{
    /// <summary>
    /// Gets or sets the showcase's example colour, passed on to the workspace form's colour picker.
    /// </summary>
    [Parameter] public string BrandColour { get; set; } = "#9333ea";

    private readonly List<string> _environments =
    [
        Loc.Text("Home_Env_Production", "Production"),
        Loc.Text("Home_Env_Staging", "Staging"),
        Loc.Text("Home_Env_Development", "Development")
    ];

    private string email = string.Empty;
    private bool notificationsEnabled = true;

    private TwStepper? checkoutStepper;
    private int checkoutStep;

    private string signInEmail = string.Empty;
    private string signInPassword = string.Empty;
    private bool rememberMe = true;

    private string projectName = "Atlas";
    private string visibility = "team";
    private string environment = Loc.Text("Home_Env_Production", "Production");
    private bool emailAlerts = true;
    private bool autoDeploy;
    private bool settingsSaved;

    /// <summary>
    /// Handles the sample settings form. It only demonstrates the controls, so it just confirms the save.
    /// </summary>
    private void SaveSettings() => settingsSaved = true;

    private void ResetSettings()
    {
        projectName = "Atlas";
        visibility = "team";
        environment = Loc.Text("Home_Env_Production", "Production");
        emailAlerts = true;
        autoDeploy = false;
        settingsSaved = false;
    }

    /// <summary>
    /// Handles the sample sign-in form. It only demonstrates the controls, so it just clears the password.
    /// </summary>
    private void SignIn() => signInPassword = string.Empty;

    // The call booking form.
    private readonly List<string> _callDurations =
        [.. new[] { 15, 30, 45, 60 }.Select(minutes => Loc.Format("Home_Meet_Minutes", "{0} minutes", minutes))];

    private DateTime callDate = DateTime.Today.AddDays(1);
    private TimeOnly callTime = new(10, 30);
    private string callDuration = string.Empty;
    private bool callReminder = true;
    private bool callBooked;

    private void BookCall() => callBooked = true;

    // The upgrade form. Prices are shown in pounds whatever the language, like the database form's.
    private int paySeats = 3;
    private string payCard = string.Empty;
    private string payExpiry = string.Empty;
    private string payCvc = string.Empty;
    private bool payDone;

    private string payTotal => (paySeats * 12m).ToString("C0", CultureInfo.GetCultureInfo("en-GB"));

    private void Pay() => payDone = true;

    // The review form.
    private readonly List<string> _reviewTags =
    [
        Loc.Text("Home_Review_Tag_Speed", "Speed"),
        Loc.Text("Home_Review_Tag_Docs", "Docs"),
        Loc.Text("Home_Review_Tag_Support", "Support"),
        Loc.Text("Home_Review_Tag_Design", "Design"),
        Loc.Text("Home_Review_Tag_Price", "Price"),
    ];

    private double reviewRating = 4;
    private IEnumerable<string> reviewSelectedTags = [];
    private bool reviewSent;

    private void SendReview() => reviewSent = true;

    public ShowcaseForms() => callDuration = _callDurations[1];

}
