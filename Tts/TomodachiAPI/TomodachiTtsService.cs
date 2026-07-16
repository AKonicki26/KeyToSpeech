using Tts.TomodachiAPI.Models;

namespace Tts.TomodachiAPI;

public class TomodachiTtsService
{
    private static TomodachiMessageQueue _tomodachiMessageQueue = new();
    
    public static void StartMessageQueue()
    {
        Console.WriteLine("Starting message queue");
        _ = Task.Run(async () => _tomodachiMessageQueue.PlayMessagesInQueueLoop());
    } 
    
    public static async Task SpeakMessage(string message, TomodachiVoice voice)
    {
        var tomodachiMessage = new TomodachiMessage(message, voice);
        _ = Task.Run(() => _tomodachiMessageQueue.InsertAndProcessMessage(tomodachiMessage));
        
    }
}