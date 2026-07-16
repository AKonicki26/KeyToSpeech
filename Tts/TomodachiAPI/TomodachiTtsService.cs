using Tts.TomodachiAPI.Models;

namespace Tts.TomodachiAPI;

public class TomodachiTtsService
{
    private static TomodachiMessageQueue _tomodachiMessageQueue = new();
    
    public static void StartMessageQueue()
    {
        _ = Task.Run(async () => _tomodachiMessageQueue.PlayMessagesInQueueLoop());
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
        _ = Task.Run(() => _tomodachiMessageQueue.InsertAndProcessMessage(tomodachiMessage));
        
    }
}