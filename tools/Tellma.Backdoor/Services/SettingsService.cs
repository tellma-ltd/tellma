using System.IO;
using System.Text.Json;
using Tellma.Backdoor.Models;

namespace Tellma.Backdoor.Services;

public class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TellmaBackdoor");

    private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            // Corrupted file — start fresh
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            var tmpFile = SettingsFile + ".tmp";
            File.WriteAllText(tmpFile, json);
            File.Move(tmpFile, SettingsFile, overwrite: true);
        }
        catch
        {
            // Best effort — don't crash the app
        }
    }
}
