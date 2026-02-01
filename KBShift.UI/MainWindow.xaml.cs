using KBShift.Core.Models;
using KBShift.Core.Services;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace KBShift.UI
{

public class LanguageViewModel
{
    public InputLanguage Language { get; set; }
    public string DisplayName { get; set; }

    public LanguageViewModel(InputLanguage lang)
    {
        Language = lang;
        DisplayName = $"{lang.Culture.EnglishName} - {lang.LayoutName}";
    }
}

public partial class MainWindow : Window
{
    private readonly KeyboardHookService _hookService;
    private readonly TrayIconController _tray;
    private readonly SettingsService _settingsService;
    private AppSettings _settings;
    private bool _canClose = false;
    private bool _isInitializing = true;
    private const string AppName = "KBShift";

    public MainWindow()
    {
        InitializeComponent();
        
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();
        _hookService = new KeyboardHookService();
        _tray = new TrayIconController();

        // Apply dynamic theme
        ThemeController.ApplyTheme();
        ThemeController.WatchTheme();
        
        InitializeTriggers();
        InitializeLanguages();
        
        _isInitializing = false;

        // Initialize Tray immediately
        _tray.Initialize(
            onShow: () => { this.Show(); this.Activate(); }, 
            onExit: () => { _canClose = true; System.Windows.Application.Current.Shutdown(); }
        );
    }
    
    private void InitializeTriggers()
    {
        var triggers = Enum.GetValues(typeof(ShortcutType))
                           .Cast<ShortcutType>()
                           .Where(t => t != ShortcutType.None)
                           .ToList();
        Group1TriggerCombo.ItemsSource = triggers;
        Group2TriggerCombo.ItemsSource = triggers;
        
        Group1TriggerCombo.SelectedItem = _settings.Group1Trigger;
        Group2TriggerCombo.SelectedItem = _settings.Group2Trigger;
        
        _hookService.Group1Trigger = _settings.Group1Trigger;
        _hookService.Group2Trigger = _settings.Group2Trigger;
    }

    private void InitializeLanguages()
    {
        var languages = InputLanguage.InstalledInputLanguages.Cast<InputLanguage>()
                                     .Select(l => new LanguageViewModel(l))
                                     .ToList();
                                     
        LeftLangCombo.ItemsSource = languages;
        RightLangCombo.ItemsSource = languages;
        
        // Handle Defaults or Loaded Settings
        if (_settings.Group1Langs.Contains("__ALL__"))
        {
            // First run: Group 1 gets ALL, Group 2 gets NONE
            foreach (var lang in languages)
            {
                LeftLangCombo.SelectedItems.Add(lang);
            }
            // Right remains empty
        }
        else
        {
            // Load saved selections
            foreach (var lang in languages)
            {
                if (_settings.Group1Langs.Contains(lang.Language.Culture.Name))
                    LeftLangCombo.SelectedItems.Add(lang);
                    
                if (_settings.Group2Langs.Contains(lang.Language.Culture.Name))
                    RightLangCombo.SelectedItems.Add(lang);
            }
        }
        
        UpdateHookServiceLangs();
    }

    private void SaveSettings()
    {
        if (_isInitializing) return;

        _settings.Group1Trigger = (ShortcutType)Group1TriggerCombo.SelectedItem;
        _settings.Group2Trigger = (ShortcutType)Group2TriggerCombo.SelectedItem;
        
        _settings.Group1Langs = LeftLangCombo.SelectedItems.Cast<LanguageViewModel>()
                                             .Select(vm => vm.Language.Culture.Name)
                                             .ToList();
                                             
        _settings.Group2Langs = RightLangCombo.SelectedItems.Cast<LanguageViewModel>()
                                             .Select(vm => vm.Language.Culture.Name)
                                             .ToList();
                                             
        _settingsService.Save(_settings);
    }

    private void Group1TriggerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        _hookService.Group1Trigger = (ShortcutType)Group1TriggerCombo.SelectedItem;
        SaveSettings();
    }

    private void Group2TriggerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        _hookService.Group2Trigger = (ShortcutType)Group2TriggerCombo.SelectedItem;
        SaveSettings();
    }

    private void LeftLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        UpdateHookServiceLangs();
        SaveSettings();
    }

    private void RightLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        UpdateHookServiceLangs();
        SaveSettings();
    }

    private void UpdateHookServiceLangs()
    {
        _hookService.LeftLangs = LeftLangCombo.SelectedItems.Cast<LanguageViewModel>()
                                              .Select(vm => vm.Language)
                                              .ToList();
                                              
        _hookService.RightLangs = RightLangCombo.SelectedItems.Cast<LanguageViewModel>()
                                               .Select(vm => vm.Language)
                                               .ToList();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

    private void OskButton_Click(object sender, RoutedEventArgs e)
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            if (Environment.Is64BitOperatingSystem)
            {
                // Bypass redirection to ensure we find the real 64-bit osk.exe in System32
                Wow64DisableWow64FsRedirection(ref ptr);
            }
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "osk.exe",
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Could not start OSK: {ex.Message}", "KBShift", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Wow64RevertWow64FsRedirection(ptr);
            }
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_canClose)
        {
            e.Cancel = true;
            this.Hide();
            return;
        }

        _hookService.Dispose();
        base.OnClosing(e);
    }

    }
}