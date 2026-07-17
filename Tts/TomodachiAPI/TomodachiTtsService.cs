using Tts.TomodachiAPI.Models;

namespace Tts.TomodachiAPI;

public static class TomodachiTtsService
{
    private static readonly string MessageDirectory = "./voiceFiles";
    private static TomodachiMessageQueue _tomodachiMessageQueue = new(MessageDirectory);
    
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