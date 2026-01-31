using KBShift.Core.Models;
using KBShift.Core.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace KBShift.UI;

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
    private bool _canClose = false;

    public MainWindow()
    {
        InitializeComponent();
        _hookService = new KeyboardHookService();
        _tray = new TrayIconController();
        InitializeLanguages();
        InitializeTriggers();
        
        // Initialize Tray immediately
        _tray.Initialize(
            onShow: () => { this.Show(); this.Activate(); }, 
            onExit: () => { _canClose = true; System.Windows.Application.Current.Shutdown(); }
        );
    }
    
    private void InitializeTriggers()
    {
        var triggers = Enum.GetValues<ShortcutType>();
        Group1TriggerCombo.ItemsSource = triggers;
        Group2TriggerCombo.ItemsSource = triggers;
        
        Group1TriggerCombo.SelectedItem = ShortcutType.LeftAltShift;
        Group2TriggerCombo.SelectedItem = ShortcutType.RightAltShift;
    }

    private void InitializeLanguages()
    {
        var languages = InputLanguage.InstalledInputLanguages.Cast<InputLanguage>()
                                     .Select(l => new LanguageViewModel(l))
                                     .ToList();
                                     
        LeftLangCombo.ItemsSource = languages;
        RightLangCombo.ItemsSource = languages;
        
        // Default Selections
        if (languages.Any())
        {
             // Try to select English for Left by default
             var english = languages.FirstOrDefault(l => l.Language.Culture.EnglishName.Contains("English"));
             if (english != null) LeftLangCombo.SelectedItems.Add(english);
             
             // Select last one for Right.
             if (languages.Count > 1) RightLangCombo.SelectedItems.Add(languages.Last());
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

    private void LeftLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _hookService.LeftLangs = LeftLangCombo.SelectedItems.Cast<LanguageViewModel>()
                                              .Select(vm => vm.Language)
                                              .ToList();
    }

    private void RightLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _hookService.RightLangs = RightLangCombo.SelectedItems.Cast<LanguageViewModel>()
                                               .Select(vm => vm.Language)
                                               .ToList();
    }

    private void Group1TriggerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Group1TriggerCombo.SelectedItem is ShortcutType type)
        {
            _hookService.Group1Trigger = type;
        }
    }

    private void Group2TriggerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Group2TriggerCombo.SelectedItem is ShortcutType type)
        {
            _hookService.Group2Trigger = type;
        }
    }
}