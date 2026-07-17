using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Media;

namespace Tts.TomodachiAPI.Models;

public class TomodachiMessageQueue
{
    private readonly string _messageDirectory;
    private readonly string _temporaryMessageDirectory;
    private static int _maxTempFiles = 5;
    private readonly ConcurrentQueue<TomodachiMessage> _messageQueue = new();
    private readonly SortedSet<string> _fileLocationSet = new();
    private readonly ReaderWriterLockSlim _fileSetLock = new();
    private readonly SoundPlayer? _player = null;

    public TomodachiMessageQueue(string messageDirectory)
    {
        _messageDirectory = messageDirectory;
        _temporaryMessageDirectory =  Path.Combine(_messageDirectory, "temps");
        // Ensure containing directory exists before storing any files i it
        Directory.CreateDirectory(_messageDirectory);

        for (int i = _maxTempFiles; i >= 1; i--)
            _fileLocationSet.Add($"{Path.Combine(_temporaryMessageDirectory, $"tmp{i}.wav")}");
    }

    public async Task InsertAndProcessMessage(TomodachiMessage message)
    {
        _messageQueue.Enqueue(message);

        string? fileLocation = null;
        // loop trying to get a file location until we actually get one
        while ((fileLocation = GetNextAvailableFileLocation()) == null)
        {
            Thread.Sleep(100);
        }

        RemoveFileLocation(fileLocation);

        await message.Process(fileLocation);
    }

    [DoesNotReturn]
    public async Task PlayMessagesInQueueLoop()
    {
        while (true)
        {
            if (!_messageQueue.TryDequeue(out var message)) continue;
            
            // wait for the message to be ready to play
            while (!message.IsReadyToPlay) { Thread.Sleep(100); }

            Debug.WriteLine($"Playing message {message.Message} from file {message.FileLocation}");

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


    private async Task PlayMessage(string fileLocation)
    {

    }
    
    // Sorted Set helper functions
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