using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace KeyToSpeech.Views;

public partial class TtsEntryBox : UserControl
{
    public TtsEntryBox()
    {
        InitializeComponent();
    }

    public void OnSendButtonClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Button clicked");
    }
}