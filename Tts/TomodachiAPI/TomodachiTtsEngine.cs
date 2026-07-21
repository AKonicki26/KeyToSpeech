using System.Diagnostics;
using System.Media;
using NAudio.Wave;
using Tts.TomodachiAPI.Models;

namespace Tts.TomodachiAPI;

internal static class TomodachiTtsEngine
{
    private static readonly string TomodachiApiUrl = "https://talkmodachi.dylanpdx.io/tts";
    private static readonly HttpClient _client = new HttpClient();
    private static SoundPlayer? _soundPlayer = null;

    /// <summary>
    /// Holds the operating system specific function to play the sound.
    /// </summary>
    private static readonly Func<string, Task> PlaySoundDelegate;

    static TomodachiTtsEngine()
    {
        // Assign the delegate for playing the wav audio file based on operating system
        PlaySoundDelegate = true switch
        {
            _ when OperatingSystem.IsWindows() => PlaySoundWindows,
            _ when OperatingSystem.IsLinux() => PlaySoundLinux,
            _ when OperatingSystem.IsMacOS() => PlaySoundMacOS,
            _ => throw new Exception(
                "Unsupported OS. KeyToSpeech only supports Windows, Linux, and MacOS at this time.")
        };
    }
    
    private static string GetApiParams(string message, TomodachiVoice voice) =>
        $"text={message.Replace(" ", "+")}&" +
        $"pitch={voice.Pitch}&" +
        $"speed={voice.Speed}&" +
        $"quality={voice.Quality}&" +
        $"tone={voice.Tone}&" +
        $"accent={voice.Accent}&" +
        $"intonation={voice.Intonation}&" +
        $"lang={voice.Language}";

    internal static async Task<HttpContent> GetVoiceResponse(string message, TomodachiVoice voice)
    {
        var requestUrl = $"{TomodachiApiUrl}?{GetApiParams(message, voice)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        Debug.WriteLine($"Sending API request for {message}");
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"Got file back for {message}");
        
        return response.Content;
    }

    internal static async Task WriteSoundBytesToFile(HttpContent content, string fileLocation)
    {
        using var diskStream = new FileStream(fileLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        Debug.WriteLine($"Writing to {fileLocation}");
        await content.CopyToAsync(diskStream);
        Debug.WriteLine($"Finished writing to {fileLocation}");
    }

// disable warning since SoundPlayer is Windows only
#pragma warning disable CA1416
    internal static async Task PlaySound(string fileLocation)
    {
        Debug.WriteLine($"Playing {fileLocation}");

        // Call the operating system specific function to play the sound
        await PlaySoundDelegate(fileLocation);
    }
    
    internal static async Task PlaySound(string fileLocation, SoundPlayer? player)
    {
        // if the sound player has not yet been initialized, do so
        if (player == null)
        {
            player = new SoundPlayer(fileLocation);
        }
        // otherwise we can avoid reallocation and just change the location its reading from
        else
        {
            player.SoundLocation = fileLocation;
        }
        
        player.LoadAsync();
        player.PlaySync();
    }
#pragma warning restore CA1416


    private static async Task PlaySoundWindows(string fileLocation)
    {
        using var outputDevice = new WaveOutEvent();
        using var audioFile = new AudioFileReader(fileLocation);

        outputDevice.Init(audioFile);
        outputDevice.Play();

        // while we are playing, sleep fo
        while (outputDevice.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(10);
        }
    }

    private static async Task PlaySoundLinux(string fileLocation)
    {
        const string playerProcessName = "pw-play";
        using var process = Process.Start(new ProcessStartInfo(playerProcessName, fileLocation) { UseShellExecute = false });
        await process!.WaitForExitAsync();
    }

    private static async Task PlaySoundMacOS(string fileLocation)
    {
        const string playerProcessName = "afplay";
        using var process = Process.Start(new ProcessStartInfo(playerProcessName, fileLocation) { UseShellExecute = false });
        await process!.WaitForExitAsync();
    }
}