namespace Tts;

public class TtsVoice(string name, string voiceKey, int pitch, int rate, int range)
{
    public string Name { get; set; } = name;
    public string VoiceKey { get; set; } = voiceKey;
    public int Pitch { get; set; } = pitch;
    public int Rate { get; set; } = rate;
    public int Range { get; set; } = range;
}