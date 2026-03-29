using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WebNovelScraper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
  [ObservableProperty] private string _chapterUrl = string.Empty;

  [ObservableProperty] private int _chapterCount = 100;

  [ObservableProperty] private int _chaptersPerFile = 10;

  [RelayCommand]
  public void Scrape()
  {
    // TODO: implement scraping logic
  }
}