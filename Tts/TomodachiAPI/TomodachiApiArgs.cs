using System.Text.Json;

namespace Tts.TomodachiAPI;

public record TomodachiApiArgs
{
    public string Text { get; set; } = string.Empty;
    public int Pitch { get; set; } = 50;
    public int Speed { get; set; } = 50;
    public int Quality { get; set; } = 50;
    public int Tone { get; set; } = 50;
    public int Accent { get; set; } = 50;
    public int Intonation { get; set; } = 1;
    public TomodachiLanguage Language { get; set; } = TomodachiLanguage.UnitedStatesEnglish;
    
    public string SerializeToJson() => JsonSerializer.Serialize(this);
    
    public string ApiParamString => $"text={Text.Replace(" ", "+")}&" +
                                    $"pitch={Pitch}&" +
                                    $"speed={Speed}&" +
                                    $"quality={Quality}&" +
                                    $"tone={Tone}&" +
                                    $"accent={Accent}&" +
                                    $"intonation={Intonation}&" +
                                    $"lang={Language}";
    
    // ?text=Fuck+you&pitch=50&speed=50&quality=50&tone=50&accent=50&intonation=1&lang=useng"
}
