using System.Diagnostics;
using NAudio.Wave;

namespace Tts;

public class TtsEngine
{
    public static async Task Main()
    {
        
        
        Console.WriteLine("Demoing eSpeak-NG (adjustable voice/pitch/speed/range)...");
        var presets = new (string Label, string Voice, int Pitch, int Rate, int Range)[]
        {
            ("Male, normal", "en+m3", 50, 175, 50),
            ("Female, normal", "en+f3", 60, 175, 50),
            ("Male, deep & slow", "en+m6", 25, 120, 30),
            ("Female, high & fast (Mii-like)", "en+f2", 80, 210, 70),
            ("Child-like", "en+f5", 90, 200, 60),
            ("Robotic", "en+croak", 40, 160, 20),
        };

        var normalMale = new TtsVoice("Normal Male", "en+m3", 50, 175, 50);
        var normalFemale = new TtsVoice("Female, norma", "en+f3", 60, 175, 50);
        var maleSlow = new TtsVoice("Male, deep & slow", "en+m6", 25, 120, 30);
        var femaleMii = new TtsVoice("Female, high & fast (Mii-like)", "en+m2", 80, 210, 70);
        var child = new TtsVoice("Child-like", "en+f5", 90, 200, 60);
        var robot = new TtsVoice("Robotic", "en+croak", 40, 160, 20);

        var objectPresets = new TtsVoice[]
        {
            normalMale,
            normalFemale,
            maleSlow,
            femaleMii,
            child,
            robot
        };
        
        /*
        foreach (var (label, voice, pitch, rate, range) in presets)
        {
            Console.WriteLine($"  {label}: voice={voice} pitch={pitch} rate={rate} range={range}");
            var espeakAudio = await EspeakEngine.SynthesizeAsync(
                "Hello! This is a customizable text to speech voice.",
                voice: voice, pitch: pitch, rate: rate, range: range);
            await PlayAudioBytesAsync(espeakAudio);
        }
        */

        foreach (var ttsVoice in objectPresets)
        {
            Console.WriteLine($"  {ttsVoice.Name}: voice={ttsVoice.VoiceKey} pitch={ttsVoice.Pitch} rate={ttsVoice.Rate} range={ttsVoice.Range}");
            var espeakAudio =
                await EspeakEngine.SynthesizeAsync(
                    "Hello! This is a customizable text to speech voice using custom objects", ttsVoice);
            await PlayAudioBytesAsync(espeakAudio);
        }
        
    }

    private static async Task PlayAudioBytesAsync(byte[] wav)
    {
        var tmpWav = Path.Combine(Path.GetTempPath(), $"keytospeech_{Guid.NewGuid():N}.wav");
        await File.WriteAllBytesAsync(tmpWav, wav);
        await PlayWavAsync(tmpWav);
        File.Delete(tmpWav);
    }

    private static async Task PlayWavAsync(string wavPath)
    {
        if (OperatingSystem.IsWindows())
        {
            using var audioFile = new AudioFileReader(wavPath);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();

            while (outputDevice.PlaybackState == PlaybackState.Playing)
                await Task.Delay(100);

            return;
        }

        var player = OperatingSystem.IsMacOS() ? "afplay" : "pw-play";
        using var play = Process.Start(new ProcessStartInfo(player, wavPath) { UseShellExecute = false });
        await play!.WaitForExitAsync();
    }
}
