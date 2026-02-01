using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using KBShift.Core.Models;

namespace KBShift.Core.Services
{

public class AppSettings
{
    public ShortcutType Group1Trigger { get; set; } = ShortcutType.LeftAltShift;
    public ShortcutType Group2Trigger { get; set; } = ShortcutType.RightAltShift;
    public List<string> Group1Langs { get; set; } = new List<string>();
    public List<string> Group2Langs { get; set; } = new List<string>();
}

public class SettingsService
{
    private readonly string _filePath;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "KBShift");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            return CreateDefaultSettings();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<AppSettings>(json) ?? CreateDefaultSettings();
        }
        catch
        {
            return CreateDefaultSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
        catch { }
    }

    private AppSettings CreateDefaultSettings()
    {
        // On first run, we'll signal to use "All/None" logic via empty lists 
        // which will be handled during initialization in the UI.
        return new AppSettings
        {
            Group1Langs = new List<string> { "__ALL__" }, // Special marker for first run
            Group2Langs = new List<string>() // Explicitly none
        };
    }
    }
}
