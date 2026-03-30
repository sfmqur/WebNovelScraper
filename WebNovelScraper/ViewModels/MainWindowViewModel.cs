using System;
using System.IO;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebNovelScraper.Services;

namespace WebNovelScraper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
  private HttpClient _httpClient;
  private ChapterScraper _scraper;
  private string _outputDir;
  public MainWindowViewModel()
  {
    _httpClient = new HttpClient();
    _scraper = new ChapterScraper(_httpClient);
    
    _outputDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      "WebNovelScraper");
    Directory.CreateDirectory(_outputDir);
  }
  
  [ObservableProperty] private string _chapterUrl = string.Empty;

  [ObservableProperty] private int _chapterCount = 100;

  [ObservableProperty] private int _chaptersPerFile = 10;
  
  [RelayCommand]
  public async void Scrape()
  {
    var (chapter,nextLink) = await _scraper.ScrapeChapterAsync(ChapterUrl);
  }
}