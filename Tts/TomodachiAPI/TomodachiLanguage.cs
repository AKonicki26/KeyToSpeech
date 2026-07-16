namespace Tts.TomodachiAPI;

public class TomodachiLanguage
{
    public string LanguageCode { get; }
    
    private TomodachiLanguage(string languageCode) {  LanguageCode = languageCode; }

    public static TomodachiLanguage UnitedStatesEnglish { get; } = new("useng");
    public static TomodachiLanguage EuropeEnglish { get; } = new("eueng");
    public static TomodachiLanguage Spanish { get; } = new("es");
    public static TomodachiLanguage German { get; } = new("de");
    public static TomodachiLanguage French { get; } = new("fr");
    public static TomodachiLanguage Italian { get; } = new("it");
    public static TomodachiLanguage Japanese { get; } = new("jp");
    public static TomodachiLanguage Korean { get; } = new("kr");

    public override string ToString() => LanguageCode;
}