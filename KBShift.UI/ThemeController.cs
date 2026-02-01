using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace KBShift.UI
{

public static class ThemeController
{
    public static void WatchTheme()
    {
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // Must apply on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(ApplyTheme);
            }
        };
    }

    public static bool IsDarkMode()
    {
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int i && i == 0;
            }
        }
        catch
        {
            return true; // Default to dark
        }
    }

    public static void ApplyTheme()
    {
        bool isDark = IsDarkMode();
        var resources = System.Windows.Application.Current.Resources;

        if (isDark)
        {
            SetResource(resources, "WindowBg", "#121212");
            SetResource(resources, "PanelBg", "#1E1E1E");
            SetResource(resources, "PrimaryText", "#E0E0E0");
            SetResource(resources, "SecondaryText", "#AAAAAA");
            SetResource(resources, "AccentColor", "#BB86FC");
            SetResource(resources, "BorderColor", "#333333");
            SetResource(resources, "ButtonBg", "#1E1E1E");
        }
        else
        {
            SetResource(resources, "WindowBg", "#F5F5F5");
            SetResource(resources, "PanelBg", "#FFFFFF");
            SetResource(resources, "PrimaryText", "#000000");
            SetResource(resources, "SecondaryText", "#444444");
            SetResource(resources, "AccentColor", "#6200EE");
            SetResource(resources, "BorderColor", "#BBBBBB");
            SetResource(resources, "ButtonBg", "#E0E0E0");
        }
    }

    private static void SetResource(ResourceDictionary resources, string key, string colorHex)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
        var brush = new SolidColorBrush(color);
        brush.Freeze(); // Optimize for performance
        resources[key] = brush;
    }
    }
}
