namespace Tts.TomodachiAPI;

public class TomodachiLanguage
{
    public string LanguageCode { get; }
    public string Name { get; }

    private TomodachiLanguage(string languageCode, string name)
    {
        LanguageCode = languageCode;
        Name = name;
    }

    public static TomodachiLanguage UnitedStatesEnglish { get; } = new("useng", "United States English");
    public static TomodachiLanguage EuropeEnglish { get; } = new("eueng", "European English");
    public static TomodachiLanguage Spanish { get; } = new("es", "Spanish");
    public static TomodachiLanguage German { get; } = new("de", "German");
    public static TomodachiLanguage French { get; } = new("fr", "French");
    public static TomodachiLanguage Italian { get; } = new("it", "Italian");
    public static TomodachiLanguage Japanese { get; } = new("jp", "Japanese");
    public static TomodachiLanguage Korean { get; } = new("kr", "Korean");

    public override string ToString() => LanguageCode;
}