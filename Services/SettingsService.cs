using System;
using System.IO;
using System.Text.Json;
using LogoffUsersTool.Models;

namespace LogoffUsersTool.Services;

public class SettingsService
{
    private readonly string _settingsFilePath;

    // Конструктор для использования в приложении
    public SettingsService()
    {
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
    }

    // Конструктор для тестов
    public SettingsService(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
    }

    public FullAppSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new FullAppSettings();
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<FullAppSettings>(json) ?? new FullAppSettings();
        }
        catch (Exception)
        {
            // В случае ошибки чтения или десериализации, возвращаем настройки по умолчанию
            return new FullAppSettings();
        }
    }

    public void SaveSettings(FullAppSettings settings)
    {
        settings.Application.LastRun = DateTime.Now;
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(_settingsFilePath, json);
    }
}
