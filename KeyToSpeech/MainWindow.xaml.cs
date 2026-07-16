using Avalonia.Controls;
using KeyToSpeech.ViewModels;
using Tts.TomodachiAPI;

namespace KeyToSpeech;

public partial class MainWindow : Window
{
    public MainWindow(TtsEntryViewModel ttsEntryViewModel, VoiceSettingsViewModel voiceSettingsViewModel)
    {
        InitializeComponent();
        TtsEntryBox.DataContext = ttsEntryViewModel;
        VoiceSettingsPanel.DataContext = voiceSettingsViewModel;
        TomodachiTtsService.StartMessageQueue();
    }
}