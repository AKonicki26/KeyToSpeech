using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Tts.TomodachiAPI.Models;

public class MessageQueue
{
    private static readonly string MessageDirectory = "./voiceFiles/temps";
    private static int _maxTempFiles = 5;
    

    public MessageQueue()
    {
        // Ensure containing directory exists before storing any files i it
        Directory.CreateDirectory(MessageDirectory);
        Console.WriteLine("HI");

        for (int i = _maxTempFiles; i >= 1; i--)
            _fileLocationSet.Add($"tmp{i}.wav");
        
        foreach (var fileLocation in _fileLocationSet)
            Console.WriteLine(fileLocation);
    }
    
    

    public async Task InsertAndProcessMessage(TomodachiMessage message)
    {
        _messageQueue.Enqueue(message);
        
        var response = await TomodachiTtsEngine.GetVoiceResponse(message.Message, message.Voice);
        
        // loop trying to get a file location until we actually get one
        while ((message.FileLocation = GetNextAvailableFileLocation()) == null)
        {
            Thread.Sleep(100);
        }
        
        RemoveFileLocation(message.FileLocation);
        
        // TODO: Move this to the message so only it can assign IsReadyToPlay
        Console.WriteLine($"Assigning message {message.Message} to sound file {message.FileLocation}");
        
        await TomodachiTtsEngine.WriteSoundBytesToFile(response, message.FileLocation);

        message.IsReadyToPlay = true;
    }

    [DoesNotReturn]
    public async Task PlayMessagesInQueueLoop()
    {
        while (true)
        {
            if (!_messageQueue.TryDequeue(out var message)) continue;
            
            // wait for the message to be ready to play
            while (!message.IsReadyToPlay) { Thread.Sleep(100); }
                
            Console.WriteLine($"Playing message {message.Message} from file {message.FileLocation}");

            if (message.FileLocation == null)
            {
                throw new Exception("File Location of a message is null. It cannot be played");
            }
            
            await TomodachiTtsEngine.PlaySound(message.FileLocation);
                
            // This file is no longer being used for TTS messages, re-queue the location
            AddFileLocation(message.FileLocation);
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    
    
    // methods relating to the queue
    private ConcurrentQueue<TomodachiMessage> _messageQueue = new();
    private SortedSet<string> _fileLocationSet = new();


    private readonly ReaderWriterLockSlim _fileSetLock = new();

    private bool AddFileLocation(string location)
    {
        _fileSetLock.EnterWriteLock();
        try
        {
            return _fileLocationSet.Add(location);
        }
        finally
        {
            _fileSetLock.ExitWriteLock();
        }
    }

    private bool RemoveFileLocation(string location)
    {
        _fileSetLock.EnterWriteLock();
        try
        {
            return _fileLocationSet.Remove(location);
        }
        finally
        {
            _fileSetLock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Gets the lowest available temporary file for storing messages
    /// </summary>
    /// <returns>String of the next temporary file name, null if no files are available </returns>
    private string? GetNextAvailableFileLocation()
    {
        _fileSetLock.EnterReadLock();
        
        var nextAvail = _fileLocationSet.Min;
        
        _fileSetLock.ExitReadLock();
        
        return nextAvail;
    }

}