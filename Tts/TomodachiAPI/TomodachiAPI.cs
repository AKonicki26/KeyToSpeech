using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Media;

namespace Tts.TomodachiAPI;

public static class TomodachiAPI
{
    
    private static readonly string apiPath = "https://talkmodachi.dylanpdx.io/tts";
    private static HttpClient client = new();
    
    public static async Task Main()
    {

        while (true)
        {
            var message = Console.ReadLine();
            if (message == "exit") 
            {
                break;
            }
            
            GetSoundAndPlayIt(message);
        }

        Console.WriteLine("Shutting down...");
        
    }


    private static async Task GetSoundAndPlayIt(string message)
    {
        var requestPayload = new TomodachiApiArgs()
        {
            Text = message,
            Pitch = 75,
        };
        
        var jsonBody = requestPayload.SerializeToJson();

        using (var request = new HttpRequestMessage(HttpMethod.Get, $"{apiPath}?{requestPayload.ApiParamString}"))
        {
            try
            {
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    Debug.WriteLine("Successful response:");

                    using (FileStream diskStream = new FileStream("./temp.wav", FileMode.Create, FileAccess.ReadWrite,
                        FileShare.Read))
                    {
                        await response.Content.CopyToAsync(diskStream);
                        
                        Debug.WriteLine("File saved successfully");
                        Debug.WriteLine($"File location: {diskStream.Name}");
                        
// TODO: Move to cross platform API or just make people use Wine.
// Prolly dev on Windows and force people to suck it up on wine if they use linux
#pragma warning disable CA1416
                        
                        
                        
                        SoundPlayer player = new SoundPlayer(diskStream.Name);

                        try
                        {
                            Debug.WriteLine("Playing sound...");
                            player.LoadAsync();
                            
                            player.PlaySync();
                        }
                        catch (FileNotFoundException exception)
                        {
                            // The file specified by 'P:System.Media.SoundPlayer.SoundLocation' cannot be found.
                            Console.WriteLine(exception.Message);
                        }
#pragma warning restore CA1416
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
        }
    }
    
    
    
}