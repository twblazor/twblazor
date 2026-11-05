using System.Globalization;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Pages;

/// <summary>
/// The showcase's database form. A component of its own so dragging its storage slider re-renders this card only.
/// </summary>
public partial class ShowcaseDatabaseCard
{
    // The database form. The plans and regions are product names, so they are not translated.
    private static readonly IReadOnlyList<DatabasePlan> _databasePlans =
    [
        new("starter", "Starter", 9),
        new("pro", "Pro", 29),
        new("scale", "Scale", 99),
    ];

    private static readonly List<string> _regions = ["London", "Frankfurt", "Virginia", "Singapore"];

    private string databasePlan = "pro";
    private string databaseRegion = _regions[0];
    private int databaseStorage = 100;
    private bool databaseCreated;

    private string databaseCost =>
        (_databasePlans.First(plan => plan.Id == databasePlan).Price + databaseStorage * 0.1m).ToString("C0", CultureInfo.GetCultureInfo("en-GB"));

    private void CreateDatabase() => databaseCreated = true;

    private sealed record DatabasePlan(string Id, string Label, decimal Price);
}
