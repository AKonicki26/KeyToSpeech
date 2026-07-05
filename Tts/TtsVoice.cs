namespace Tts;

public class TtsVoice(string name, string voiceKey, int pitch, int rate, int range)
{
    private string _name = name;
    private string _voiceKey = voiceKey;
    private int _pitch = pitch;
    private int _rate = rate;
    private int _range = range;
    
    public string Name  { get => _name; set => _name = value; }
    public string VoiceKey { get => _voiceKey; set => _voiceKey = value; }
    public int Pitch { get => _pitch; set => _pitch = value; }
    public int Rate { get => _rate; set => _rate = value; }
    public int Range { get => _range; set => _range = value; }
}