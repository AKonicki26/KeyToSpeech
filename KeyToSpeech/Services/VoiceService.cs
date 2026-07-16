using Tts.TomodachiAPI.Models;

namespace KeyToSpeech.Services;

public class VoiceService
{
    public TomodachiVoice ActiveVoice { get; private set; } = new();
    public IReadOnlyDictionary<string, TomodachiVoice> SavedVoices => _savedVoices;

    private readonly Dictionary<string, TomodachiVoice> _savedVoices = [];

    public void SetActiveVoice(TomodachiVoice voice) => ActiveVoice = voice;
    public void SaveVoice(string name, TomodachiVoice voice) => _savedVoices.Add(name, voice);
}