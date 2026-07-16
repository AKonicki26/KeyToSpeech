using System.Media;
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
        
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return response.Content;
    }

    internal static async Task WriteSoundBytesToFile(HttpContent content, string fileLocation)
    {
        using var diskStream = new FileStream(fileLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        
        await content.CopyToAsync(diskStream);
    }

// disable warning since SoundPlayer is Windows only
#pragma warning disable CA1416 
    internal static async Task PlaySound(string fileLocation)
    {
        await PlaySound(fileLocation, _soundPlayer);
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