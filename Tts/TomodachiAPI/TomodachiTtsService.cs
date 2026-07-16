using Tts.TomodachiAPI.Models;

namespace Tts.TomodachiAPI;

public class TomodachiTtsService
{
    private static MessageQueue _messageQueue = new();
    
    public static void StartMessageQueue()
    {
        _ = Task.Run(async () => _messageQueue.PlayMessagesInQueueLoop());
    } 
    
    public static async Task SpeakMessage(string message, TomodachiVoice voice)
    {
        /*
        var response = await TomodachiTtsEngine.GetVoiceResponse(message, voice);

        var soundFileLocation = _messageQueue.GetNextAvailableFileLocation();
        
        await TomodachiTtsEngine.WriteSoundBytesToFile(response, soundFileLocation);
        
        await TomodachiTtsEngine.PlaySound(soundFileLocation);
        */

        var tomodachiMessage = new TomodachiMessage(message, voice);
        _ = Task.Run(() => _messageQueue.InsertAndProcessMessage(tomodachiMessage));
        
    }
}