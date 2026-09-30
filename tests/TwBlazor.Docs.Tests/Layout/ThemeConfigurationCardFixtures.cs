using TwBlazor.Configuration.Color;

namespace TwBlazor.Docs.Tests.Layout;

/// <summary>
/// Stand-in "theme" type covering every branch of <c>ThemeConfigurationCard.FriendlyTypeName</c>
/// and the own-vs-global property split. Never instantiated - only reflected over.
/// </summary>
public class ThemeConfigurationCardTestTheme
{
    public string StringValue { get; set; } = string.Empty;
    public bool BoolValue { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public int? NullableIntValue { get; set; }
    public ThemeConfigurationCardTestEnum EnumValue { get; set; }

    // TwBlazorPalette is one of ThemeConfigurationCard's hardcoded "global token" types, so this
    // property should be split into the Global Theme Configuration table instead of the own-property one.
    public TwBlazorPalette PaletteValue { get; set; } = null!;
}

public enum ThemeConfigurationCardTestEnum
{
    A,
    B
}
