namespace FileNavigationApp.Presentation;

public sealed partial class MainPage : Page
{
    // public MainViewModel ViewModel { get; } = new MainViewModel();
    public MainViewModel ViewModel => (MainViewModel)this.DataContext;

//    public FileSearchService Searchservice { get; } = new FileSearchService(); 
    public MainPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Host.Services.GetRequiredService<MainViewModel>();
        // this.DataContext = new MainViewModel();
//        this.DataContext = new FileSearchService();
    }
}
