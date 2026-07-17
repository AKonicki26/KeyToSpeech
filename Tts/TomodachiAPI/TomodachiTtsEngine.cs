using System.Diagnostics;
using System.Media;
using NAudio.Wave;
using Tts.TomodachiAPI.Models;

namespace Tts.TomodachiAPI;

internal static class TomodachiTtsEngine
{
    private static readonly string TomodachiApiUrl = "https://talkmodachi.dylanpdx.io/tts";
    private static HttpClient _client = new HttpClient();
    private static SoundPlayer? _soundPlayer = null;
    
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
        using (var outputDevice = new WaveOutEvent())
        using (var audioFile = new AudioFileReader(fileLocation))
        {
            outputDevice.Init(audioFile);

            outputDevice.Play();

            // while we are playing, sleep fo
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(10);
            }
        }
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
}