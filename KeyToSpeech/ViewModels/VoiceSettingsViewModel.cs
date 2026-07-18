
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyToSpeech.Services;
using Tts.TomodachiAPI;
using Tts.TomodachiAPI.Models;

namespace KeyToSpeech.ViewModels;

public partial class VoiceSettingsViewModel(VoiceService voiceService) : ObservableObject
{
    [ObservableProperty] public partial int VoicePitch { get; set; } = voiceService.ActiveVoice.Pitch;
    partial void OnVoicePitchChanged(int value)
    {
        voiceService.ActiveVoice.Pitch = value;
    }

    [ObservableProperty] public partial int VoiceSpeed { get; set; } = voiceService.ActiveVoice.Speed;
    partial void OnVoiceSpeedChanged(int value)
    {
        voiceService.ActiveVoice.Speed = value;
    }

    [ObservableProperty] public partial int VoiceQuality { get; set; } = voiceService.ActiveVoice.Quality;
    partial void OnVoiceQualityChanged(int value)
    {
        voiceService.ActiveVoice.Quality = value;
    }

    [ObservableProperty] public partial int VoiceTone { get; set; } = voiceService.ActiveVoice.Tone;
    partial void OnVoiceToneChanged(int value)
    {
        voiceService.ActiveVoice.Tone = value;
    }

    [ObservableProperty] public partial int VoiceAccent { get; set; } = voiceService.ActiveVoice.Accent;
    partial void OnVoiceAccentChanged(int value)
    {
        voiceService.ActiveVoice.Accent = value;
    }

    [RelayCommand]
    public void IntonationButtonClicked(int number)
    {
        // Button values are hardcoded. This cannot fail
        // var _ = numberEntry.TryParseInt(out var number);


        Console.WriteLine($"Intonation button clicked: {number}");
        voiceService.ActiveVoice.Intonation = number;
    }

    public List<TomodachiLanguage> TomodachiLanguages { get; } =
    [
        TomodachiLanguage.UnitedStatesEnglish,
        TomodachiLanguage.EuropeEnglish,
        TomodachiLanguage.Spanish,
        TomodachiLanguage.German,
        TomodachiLanguage.French,
        TomodachiLanguage.Italian,
        TomodachiLanguage.Japanese,
        TomodachiLanguage.Korean,
    ];

    [ObservableProperty]
    public partial TomodachiLanguage ActiveLanguage { get; set;  } = TomodachiLanguage.UnitedStatesEnglish;

    partial void OnActiveLanguageChanged(TomodachiLanguage value)
    {
        voiceService.ActiveVoice.Language = value;
    }
}
