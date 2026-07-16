using System.Diagnostics;

namespace Tts.TomodachiAPI.Models;

public record TomodachiMessage(string Message, TomodachiVoice Voice)
{
    public string? FileLocation { get; set; }
    public string Message { get; set; } = Message;
    public TomodachiVoice Voice { get; set; } = Voice;

    public bool IsReadyToPlay { get; set; } = false;
    
    internal async Task Process(string fileLocation)
    {
        FileLocation = fileLocation;
        
        var response = await TomodachiTtsEngine.GetVoiceResponse(Message, Voice);
        
        // TODO: Move this to the message so only it can assign IsReadyToPlay
        Debug.WriteLine($"Assigning message {Message} to sound file {FileLocation}");
        
        await TomodachiTtsEngine.WriteSoundBytesToFile(response, FileLocation);
        
        IsReadyToPlay = true;

    }
}