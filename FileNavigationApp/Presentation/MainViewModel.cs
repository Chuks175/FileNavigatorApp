using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileNavigationApp.Models;
using FileNavigationApp.Services.Cross_Platform_Service;

namespace FileNavigationApp.Presentation;

/*public partial class MainViewModel : ObservableObject
 * {
 *    private INavigator _navigator;
 * 
 *    [ObservableProperty] private string? name;
 * 
 *    public MainViewModel(
 *        IStringLocalizer localizer,
 *        IOptions<AppConfig> appInfo,
 *        INavigator navigator)
 *    {
 *        _navigator = navigator;
 *        Title = "Main";
 *        Title += $" - {localizer["ApplicationName"]}";
 *        Title += $" - {appInfo?.Value?.Environment}";
 *        GoToSecond = new AsyncRelayCommand(GoToSecondView);
 *    }
 * 
 *    public string? Title { get; }
 * 
 *    public ICommand GoToSecond { get; }
 * 
 *    private async Task GoToSecondView()
 *    {
 *        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
 *    }
 * }*/



public class MainViewModel : INotifyPropertyChanged
{
    //    public readonly FileSearchService _searchService = new();
    private readonly FileSearchService _searchService = new();
    private readonly IWritableOptions<AppSettings> _writableOptions;
    private int _currentPage = 1;
    //    private int InitialPage;
    private const int PageSize = 20;
    private string _searchPath = "/"; // Default Linux Root
    private string _searchQuery = "";
    private string _statusText = "Ready";
    private bool _isShowingSettings = false;
    
    public MainViewModel(IWritableOptions<AppSettings> writableOptions)
    {
        _writableOptions = writableOptions;
        Settings = _writableOptions.Value; // Restores last session state
        
        // Listen to settings changing to trigger automatic background saving
        Settings.PropertyChanged += OnSettingsPropertyChanged;
    }
    
    
    public AppSettings Settings { get; }  
    //= new AppSettings();
    public ObservableCollection<SearchResultItem> DisplayedResults { get; } = new();
    
    public string SearchPath { get => _searchPath; set { _searchPath = value; OnPropertyChanged(); } }
    public string SearchQuery { get => _searchQuery; set { _searchQuery = value; OnPropertyChanged(); } }
    public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }
    public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); } }
    
    // public int InitialPage1
    // {
    //     get => InitialPage;
    //     set => InitialPage = value;
    // }
    
    
    public bool IsShowingSettings
    {
        get => _isShowingSettings;
        set { _isShowingSettings = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsShowingSearchView));
        }
    }
    
    public bool IsShowingSearchView => !IsShowingSettings;
    public ICommand SearchCommand => new RelayCommand(ExecuteSearch);
    public ICommand NextPageCommand => new RelayCommand(() => ChangePage(1));
    public ICommand PrevPageCommand => new RelayCommand(() => ChangePage(-1));
    public ICommand ToggleSettingsCommand => new RelayCommand(() => IsShowingSettings = !IsShowingSettings);
    
    private async void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await _writableOptions.UpdateAsync(cachedValue =>
        {
            return Settings;
        });
    }
    
    private void ExecuteSearch()
    {
        StatusText = "Searching...";
        CurrentPage = 1;
        
        var session = _searchService.PerformSearch(SearchPath, SearchQuery, Settings);
        
        UpdatePage();
        StatusText = $"Found {session.AllItems.Count} items in {session.ElapsedMilliseconds} ms.";
    }
    
    private void ChangePage(int direction)
    {
        int totalPages = (int)Math.Ceiling((double)_searchService.GetTotalResultsCount() / PageSize);
        int newPage = CurrentPage + direction;
        
        if (newPage >= 1 && newPage <= totalPages)
        {
            CurrentPage = newPage;
            UpdatePage();
        }
    }
    
    // private int IntialPage()
    // {
    //     CurrentPage = 1;
    //     int initialPage = _currentPage - 1;
    //     if (initialPage <= 0)
    //     {
    //         initialPage = _currentPage;
    //     }
    //     return initialPage;
    // }
    
    private void UpdatePage()
    {
        DisplayedResults.Clear();
        var items = _searchService.GetPaginatedPage(CurrentPage, PageSize);
        foreach (var item in items) DisplayedResults.Add(item);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => 
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Simple Helper Command Class
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}
