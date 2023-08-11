namespace ContactWithBlazor.Client;

public static class LocalizerSettings
{
    public static CultureWithName NeutralCulture =
        new CultureWithName("English", "en-US");

    public static readonly List<CultureWithName> SupportedCulturesWithName =
        new List<CultureWithName>()
        {
            new CultureWithName("English", "en-US"),
            new CultureWithName("Arabic", "ar")
        };
}