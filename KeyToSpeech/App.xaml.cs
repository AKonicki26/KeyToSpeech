using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using KeyToSpeech.Services;
using KeyToSpeech.ViewModels;
using KeyToSpeech.Views;


namespace KeyToSpeech;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<VoiceService>();
        services.AddTransient<TtsEntryViewModel>();
        services.AddTransient<VoiceSettingsViewModel>();

        var provider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(
                provider.GetRequiredService<TtsEntryViewModel>(),
                provider.GetRequiredService<VoiceSettingsViewModel>());
        }

        base.OnFrameworkInitializationCompleted();
    }
}