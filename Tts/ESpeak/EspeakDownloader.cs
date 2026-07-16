using System.Diagnostics;

namespace Tts;

/// <summary>
/// The espeak-ng native binary bundled in the KokoroSharp NuGet package is broken on Windows
/// (espeak_Initialize access-violates). espeak-ng's GitHub releases only ship an MSI for Windows
/// (no portable zip), so this downloads it once and extracts the DLL + data via an administrative
/// MSI extraction (msiexec /a) - this only unpacks files to a folder, it does not install anything
/// system-wide (no Program Files entry, no registry, no PATH change).
/// </summary>
internal static class EspeakDownloader
{
    private const string MsiUrl = "https://github.com/espeak-ng/espeak-ng/releases/download/1.52.0/espeak-ng.msi";

    public static async Task EnsureWindowsBuildAsync(string targetDir)
    {
        var dllPath = Path.Combine(targetDir, "libespeak-ng.dll");
        if (File.Exists(dllPath))
            return;

        Console.WriteLine("Downloading espeak-ng...");
        var tempDir = Path.Combine(Path.GetTempPath(), $"espeak-ng-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var msiPath = Path.Combine(tempDir, "espeak-ng.msi");
            using (var http = new HttpClient())
            using (var response = await http.GetAsync(MsiUrl))
            {
                response.EnsureSuccessStatusCode();
                await using var fileStream = File.Create(msiPath);
                await response.Content.CopyToAsync(fileStream);
            }

            var extractDir = Path.Combine(tempDir, "extracted");
            var psi = new ProcessStartInfo("msiexec", $"/a \"{msiPath}\" /qn TARGETDIR=\"{extractDir}\"")
            {
                UseShellExecute = false,
            };
            using (var proc = Process.Start(psi)!)
                await proc.WaitForExitAsync();

            var sourceDir = Path.Combine(extractDir, "eSpeak NG");
            Directory.CreateDirectory(targetDir);
            File.Copy(Path.Combine(sourceDir, "libespeak-ng.dll"), dllPath, overwrite: true);
            CopyDirectory(Path.Combine(sourceDir, "espeak-ng-data"), Path.Combine(targetDir, "espeak-ng-data"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.GetDirectories(sourceDir))
            CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
    }
}
