namespace WebNovelScraper.Models;

public class ConfigDto
{
  public string ChapterUrl { get; set; } = string.Empty;
  public int ChapterCount { get; set; }
  public int ChaptersPerFile { get; set; }
  public string OutputDir { get; set; } = string.Empty;
  public string SavedLinks { get; set; } = string.Empty;
}
