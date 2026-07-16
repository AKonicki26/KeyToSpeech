namespace Tts.TomodachiAPI.Models;

public record TomodachiMessage(string Message, TomodachiVoice Voice)
{
    public string? FileLocation { get; set; }
    public string Message { get; set; } = Message;
    public TomodachiVoice Voice { get; set; } = Voice;

    public bool IsReadyToPlay { get; set; } = false;
}