using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebNovelScraper.Models;
using WebNovelScraper.Services;

namespace WebNovelScraper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
  [ObservableProperty] private string _outputDir = string.Empty;

  public MainWindowViewModel()
  {
    var httpClient = new HttpClient();
    

    OutputDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      "WebNovelScraper");
    Directory.CreateDirectory(OutputDir);
    loadConfig();
  }
  
  public string DefaultOutputPath { get;  } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "WebNovelScraper");
  private const string Domain = "https://freewebnovel.com";

  [ObservableProperty] private string _chapterUrl = string.Empty;

  [ObservableProperty] private int _chapterCount = 100;

  [ObservableProperty] private int _chaptersPerFile = 10;

  [ObservableProperty] private string _lastScrapedChapter = string.Empty;
  [ObservableProperty] private string _savedLinks = string.Empty;
  
  [RelayCommand]
  public async Task Scrape()
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
    
    using var scraper = new ChapterScraper(false);
    scraper.ChapterScraped += title => LastScrapedChapter = title;
    
    while (chaptersScraped < totalChapters && !string.IsNullOrEmpty(currentUrl))
    {
      var fileStartUrl = currentUrl;
      var sb = new StringBuilder();
      var chaptersInFile = 0;

      while (chaptersInFile < chaptersPerFile &&
             chaptersScraped < totalChapters &&
             !string.IsNullOrEmpty(currentUrl))
      {
        var (chapterText, nextUrl) = await scraper.ScrapeChapterAsync(currentUrl);
        Thread.Sleep(1000);
        sb.AppendLine(chapterText);
        chaptersInFile++;
        chaptersScraped++;
        currentUrl = string.IsNullOrEmpty(nextUrl) ? string.Empty : Domain + nextUrl;
      }

      var fileName = buildFileName(fileStartUrl);
      await File.WriteAllTextAsync(Path.Combine(OutputDir, fileName), sb.ToString());
    }
  }

  public void SaveConfig()
  {
    var config = new ConfigDto
    {
      ChapterUrl = ChapterUrl,
      ChapterCount = ChapterCount,
      ChaptersPerFile = ChaptersPerFile,
      OutputDir = OutputDir,
      SavedLinks = SavedLinks,
    };
    var json = JsonSerializer.Serialize(config);
    File.WriteAllText(Path.Combine(DefaultOutputPath, "config.json"), json);
  }

  private void loadConfig()
  {
    var path = Path.Combine(DefaultOutputPath, "config.json");
    if (!File.Exists(path)) return;
    try
    {
      var config = JsonSerializer.Deserialize<ConfigDto>(File.ReadAllText(path));
      if (config is null) return;
      ChapterUrl = config.ChapterUrl;
      ChapterCount = config.ChapterCount;
      ChaptersPerFile = config.ChaptersPerFile;
      OutputDir = config.OutputDir;
      SavedLinks = config.SavedLinks;
    }
    catch { }
  }

  private static string buildFileName(string url)
  {
    // e.g. https://freewebnovel.com/novel/book-name/chapter-1
    var afterNovel = url.Split("/novel/", StringSplitOptions.None);
    var parts = afterNovel[1].TrimEnd('/').Split('/');
    var bookName = parts[0];
    var rawSlug = parts.Length > 1 ? parts[1] : "chapter-1";
    var chapterSlug = "chapter-" + rawSlug.Split('-')[1].PadLeft(4, '0');
    return $"{bookName}_{chapterSlug}.txt";
  }
}