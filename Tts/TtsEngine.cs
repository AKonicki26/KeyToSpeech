using System.Diagnostics;
using PiperSharp;
using PiperSharp.Models;

namespace Tts;

public class TtsEngine
{
    public static async Task Main()
    {
        var cwd = Directory.GetCurrentDirectory();

        if (!Directory.Exists(Path.Join(cwd, "piper")))
        {
            Console.WriteLine($"Downloading piper to {cwd}...");
            await PiperDownloader.DownloadPiper().ExtractPiper(cwd);
        }

        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(
                PiperDownloader.DefaultPiperExecutableLocation,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);

        Console.WriteLine("Loading voice model...");
        var model = await PiperDownloader.DownloadModelByKey("en_US-amy-low");

        var provider = new PiperProvider(new PiperConfiguration
        {
            ExecutableLocation = PiperDownloader.DefaultPiperExecutableLocation,
            WorkingDirectory = PiperDownloader.DefaultPiperLocation,
            Model = model,
        });

        Console.WriteLine("Generating speech...");
        var audio = await provider.InferAsync("Hello! KeyToSpeech text to speech is working.", AudioOutputType.Wav);

        var tmpWav = Path.Combine(Path.GetTempPath(), "keytospeech_test.wav");
        await File.WriteAllBytesAsync(tmpWav, audio);

        Console.WriteLine("Playing audio...");
        using var play = Process.Start(new ProcessStartInfo("pw-play", tmpWav) { UseShellExecute = false });
        await play!.WaitForExitAsync();

        File.Delete(tmpWav);
    }
}