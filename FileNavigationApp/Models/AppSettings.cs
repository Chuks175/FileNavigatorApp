using System.IO;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileSystem = System.IO.Directory;

namespace FileNavigationApp.Models;

public class AppSettings : INotifyPropertyChanged
{
    private bool _includeHiddenFiles = false;
    private bool _isCaseInsensitive = true;
    private bool _isDarkMode = false;
    
    
    public bool IncludeHiddenFiles 
    { 
        get => _includeHiddenFiles; 
        set { _includeHiddenFiles = value; OnPropertyChanged();
            //UserSettings();
        } 
    }
    
    public bool IsCaseInsensitive 
    { 
        get => _isCaseInsensitive; 
        set { _isCaseInsensitive = value; OnPropertyChanged();
            //UserSettings();
        } 
    }
    
    public bool IsDarkMode 
    { 
        get => _isDarkMode; 
        set { _isDarkMode = value; OnPropertyChanged(); ApplyTheme();
            //UserSettings();
        } 
    }
    
    private void ApplyTheme()
    {
        // Dynamically changes the Uno Platform window theme environment
        if (Microsoft.UI.Xaml.Window.Current?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = _isDarkMode 
            ? Microsoft.UI.Xaml.ElementTheme.Dark 
            : Microsoft.UI.Xaml.ElementTheme.Light;
        }
    }
    
    
    private void UserSettings()
    {
        string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appFolder = Path.Combine(baseFolder, "FileNavigatioApp");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        string NewFile = Path.Combine(appFolder, "settings.json");
        
        var settings = new AppSettings();
        string jsonString = JsonSerializer.Serialize(settings);
        File.WriteAllText(NewFile,  jsonString);
        
        if (File.Exists(NewFile))
        {
            string loadedJson = File.ReadAllText(NewFile);
            var loadedSettings = JsonSerializer.Deserialize<AppSettings>(loadedJson);
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
