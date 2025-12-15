using System;
using System.IO;
using System.Text.Json;
using LogoffUsersTool.Models;

namespace LogoffUsersTool.Services;

public class SettingsService
{
    private readonly string _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

    public FullAppSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new FullAppSettings();
        }

        var json = File.ReadAllText(_settingsFilePath);
        return JsonSerializer.Deserialize<FullAppSettings>(json) ?? new FullAppSettings();
    }

    public void SaveSettings(FullAppSettings settings)
    {
        settings.Application.LastRun = DateTime.Now;
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }
}
