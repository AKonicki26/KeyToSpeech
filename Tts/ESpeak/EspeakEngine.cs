using System.Runtime.InteropServices;
using System.Text;

namespace Tts;

/// <summary>
/// Thin P/Invoke wrapper around the espeak-ng shared library. Unlike neural TTS (Piper/Kokoro),
/// espeak-ng is a formant synthesizer, so pitch/speed/intonation are native, real-time-adjustable
/// parameters.
/// </summary>
internal static class EspeakEngine
{
    private const string LibraryName = "espeak-ng";

    private const int AudioOutputRetrieval = 1;
    private const int PositionCharacter = 1;
    private const int CharsUtf8 = 1;
    private const int InitializeDontExit = 0x8000;

    private const int ParamRate = 1;
    private const int ParamPitch = 3;
    private const int ParamRange = 4;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int SynthCallback(IntPtr wav, int numSamples, IntPtr events);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int espeak_Initialize(int output, int buflength, IntPtr path, int options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void espeak_SetSynthCallback(SynthCallback callback);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int espeak_SetVoiceByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int espeak_SetParameter(int parameter, int value, int relative);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int espeak_Synth(byte[] text, int size, uint position, int positionType, uint endPosition, uint flags, ref uint uniqueIdentifier, IntPtr userData);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int espeak_Synchronize();

    private static bool _initialized;
    private static int _sampleRate;
    private static SynthCallback? _callback;
    private static List<short>? _pendingSamples;

    static EspeakEngine()
    {
        NativeLibrary.SetDllImportResolver(typeof(EspeakEngine).Assembly, ResolveLibrary);
    }

    private static string GetBaseDir() => Path.Combine(
        AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "espeak-win" : "espeak");

    private static IntPtr ResolveLibrary(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
            return IntPtr.Zero;

        var fileName = OperatingSystem.IsWindows()
            ? "libespeak-ng.dll"
            : OperatingSystem.IsMacOS()
                ? (RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "espeak-ng-macos-arm64.dll" : "espeak-ng-macos-amd64.dll")
                : "espeak-ng-linux-amd64.dll";

        return NativeLibrary.Load(Path.Combine(GetBaseDir(), fileName));
    }

    private static async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        var baseDir = GetBaseDir();
        if (OperatingSystem.IsWindows())
            await EspeakDownloader.EnsureWindowsBuildAsync(baseDir);

        var dataDirPtr = Marshal.StringToCoTaskMemUTF8(baseDir);
        try
        {
            _sampleRate = espeak_Initialize(AudioOutputRetrieval, 0, dataDirPtr, InitializeDontExit);
        }
        finally
        {
            Marshal.FreeCoTaskMem(dataDirPtr);
        }

        if (_sampleRate <= 0)
            throw new InvalidOperationException("Failed to initialize espeak-ng.");

        _callback = OnSynthCallback;
        espeak_SetSynthCallback(_callback);
        _initialized = true;
    }

    private static int OnSynthCallback(IntPtr wav, int numSamples, IntPtr events)
    {
        if (numSamples > 0 && wav != IntPtr.Zero)
        {
            var buffer = new short[numSamples];
            Marshal.Copy(wav, buffer, 0, numSamples);
            _pendingSamples!.AddRange(buffer);
        }

        return 0;
    }

    /// <summary>Synthesizes speech to WAV bytes using espeak-ng's tunable voice parameters.</summary>
    /// <param name="text">The text to speak.</param>
    /// <param name="voice">espeak-ng voice identifier, e.g. "en-us".</param>
    /// <param name="pitch">Base pitch, 0-100 (50 = normal). Higher = squeakier "Mii" voice.</param>
    /// <param name="rate">Speaking rate in words per minute, 80-450 (175 = normal).</param>
    /// <param name="range">Pitch range/intonation variation, 0-100 (0 = monotone, 50 = normal).</param>
    public static async Task<byte[]> SynthesizeAsync(string text, string voice = "en", int pitch = 50, int rate = 175, int range = 50)
    {
        await EnsureInitializedAsync();

        espeak_SetVoiceByName(voice);
        espeak_SetParameter(ParamPitch, Math.Clamp(pitch, 0, 100), 0);
        espeak_SetParameter(ParamRate, Math.Clamp(rate, 80, 450), 0);
        espeak_SetParameter(ParamRange, Math.Clamp(range, 0, 100), 0);

        _pendingSamples = new List<short>();

        var textBytes = Encoding.UTF8.GetBytes(text + "\0");
        uint uniqueId = 0;
        espeak_Synth(textBytes, textBytes.Length, 0, PositionCharacter, 0, CharsUtf8, ref uniqueId, IntPtr.Zero);
        espeak_Synchronize();

        return BuildWavFile(_pendingSamples, _sampleRate);
    }

    public static async Task<byte[]> SynthesizeAsync(string text, TtsVoice ttsVoice)
    {
        return await SynthesizeAsync(text, ttsVoice.VoiceKey, ttsVoice.Pitch, ttsVoice.Rate, ttsVoice.Range);
    }

    private static byte[] BuildWavFile(List<short> samples, int sampleRate)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        var dataByteCount = samples.Count * sizeof(short);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataByteCount);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)1); // mono
        writer.Write(sampleRate);
        writer.Write(sampleRate * sizeof(short)); // byte rate
        writer.Write((short)sizeof(short)); // block align
        writer.Write((short)16); // bits per sample
        writer.Write("data"u8);
        writer.Write(dataByteCount);
        foreach (var sample in samples)
            writer.Write(sample);

        return stream.ToArray();
    }
}
