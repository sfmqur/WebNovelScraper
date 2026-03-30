using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebNovelScraper.Services;

namespace WebNovelScraper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
  private ChapterScraper _scraper;
  private string _outputDir;
  public MainWindowViewModel()
  {
    var httpClient = new HttpClient();
    _scraper = new ChapterScraper(httpClient);
    _scraper.ChapterScraped += title => LastScrapedChapter = title;
    
    _outputDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      "WebNovelScraper");
    Directory.CreateDirectory(_outputDir);
  }
  
  private const string Domain = "https://freewebnovel.com";

  [ObservableProperty] private string _chapterUrl = string.Empty;

  [ObservableProperty] private int _chapterCount = 100;

  [ObservableProperty] private int _chaptersPerFile = 10;

  [ObservableProperty] private string _lastScrapedChapter = string.Empty;
  
  [RelayCommand]
  public async void Scrape()
  {
    await scrapeChaptersAsync(ChapterUrl, ChapterCount, ChaptersPerFile);
  }

  
  /// <summary>
  /// Scrapes a sequence of chapters starting from <paramref name="startUrl"/> and writes
  /// them to text files in the output directory.
  /// </summary>
  /// <param name="startUrl">The URL of the first chapter to scrape.</param>
  /// <param name="totalChapters">The total number of chapters to scrape.</param>
  /// <param name="chaptersPerFile">The maximum number of chapters to write per output file.</param>
  private async Task scrapeChaptersAsync(string startUrl, int totalChapters, int chaptersPerFile)
  {
    var currentUrl = startUrl;
    var chaptersScraped = 0;

    while (chaptersScraped < totalChapters && !string.IsNullOrEmpty(currentUrl))
    {
      var fileStartUrl = currentUrl;
      var sb = new StringBuilder();
      var chaptersInFile = 0;

      while (chaptersInFile < chaptersPerFile &&
             chaptersScraped < totalChapters &&
             !string.IsNullOrEmpty(currentUrl))
      {
        var (chapterText, nextUrl) = await _scraper.ScrapeChapterAsync(currentUrl);
        Thread.Sleep(1000);
        sb.AppendLine(chapterText);
        chaptersInFile++;
        chaptersScraped++;
        currentUrl = string.IsNullOrEmpty(nextUrl) ? string.Empty : Domain + nextUrl;
      }

      var fileName = buildFileName(fileStartUrl);
      await File.WriteAllTextAsync(Path.Combine(_outputDir, fileName), sb.ToString());
    }
  }

  private static string buildFileName(string url)
  {
    // e.g. https://freewebnovel.com/novel/book-name/chapter-1
    var afterNovel = url.Split("/novel/", StringSplitOptions.None);
    var parts = afterNovel[1].TrimEnd('/').Split('/');
    var bookName = parts[0];
    var chapterSlug = parts.Length > 1 ? parts[1] : "chapter-1";
    return $"{bookName}_{chapterSlug}.txt";
  }
}