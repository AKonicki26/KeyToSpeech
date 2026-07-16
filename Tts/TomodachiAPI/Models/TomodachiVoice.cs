namespace Tts.TomodachiAPI.Models;

/// <summary>
/// Stores data about the voice to speak messages in
/// </summary>
public class TomodachiVoice
{
    // TODO: Clamp value's in setters
    
    public int Pitch { get; set; } = 50;
    public int Speed { get; set; } = 50;
    public int Quality { get; set; } = 50;
    public int Tone { get; set; } = 50;
    public int Accent { get; set; } = 50;
    public int Intonation { get; set; } = 1;
    public TomodachiLanguage Language { get; set; } = TomodachiLanguage.UnitedStatesEnglish;
}