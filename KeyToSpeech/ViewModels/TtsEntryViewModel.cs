using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyToSpeech.Services;
using Tts.TomodachiAPI;

namespace KeyToSpeech.ViewModels;

public partial class TtsEntryViewModel(VoiceService voiceService) : ObservableObject
{
    [ObservableProperty]
    public partial string TtsMessageText { get; set; }

    [RelayCommand]
    public void SendTtsMessage()
    {
        Console.WriteLine($"Sending TTS message: {TtsMessageText}");
        string messageCopy  = TtsMessageText;
        _ = Task.Run(async () => await TomodachiTtsService.SpeakMessage(messageCopy, voiceService.ActiveVoice));
        TtsMessageText = string.Empty;
    }
}