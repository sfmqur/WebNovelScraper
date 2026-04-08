using System;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebNovelScraper.Services;

public class ChapterScraper(HttpClient httpClient)
{
  private const string UserAgent =
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:136.0) Gecko/20100101 Firefox/136.0";

  private const string Domain = "https://freewebnovel.com";

  /// <summary>Fired after each chapter is successfully scraped. Arg is the chapter title.</summary>
  public event Action<string>? ChapterScraped;
  
  /// <summary>
  /// Fetches the chapter at <paramref name="url"/> and returns its title and
  /// body paragraphs as a plain-text string.
  /// second return is the href for the next chapter. string.Empty if not found. 
  /// </summary>
  public async Task<(string,string)> ScrapeChapterAsync(string url)
  {
    if (!url.Contains("freewebnovel.com"))
      throw new ConstraintException($"Scraper only works for freewebnovel.com");
    
    using var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Add("User-Agent", UserAgent);

    using var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var html = await response.Content.ReadAsStringAsync();

    var doc = new HtmlDocument();
    doc.LoadHtml(html);
  
    
    var nextChapterNode = doc.DocumentNode.SelectSingleNode("//a[@title='Read Next chapter']");
    var nextChapter = nextChapterNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;

    var article = doc.DocumentNode.SelectSingleNode("//div[@id='article']");
    if (article is null)
    {
      //todo: better error handling if something borks.
      throw new ArgumentException($"div with id article not found in {url}");
    }

    var chapterText = new StringBuilder();

    var h4 = article.SelectSingleNode(".//h4");
    var spanChapter = doc.DocumentNode.SelectSingleNode("//span[@class='chapter']");

    var chapterTitle = spanChapter?.InnerText.Trim() ?? h4?.InnerText.Trim();

    if (chapterTitle is not null)
    {
      chapterText.AppendLine(chapterTitle);
      chapterText.AppendLine();
    }

    var pTags = article.SelectNodes(".//p")
                ?? throw new ArgumentException($"p Tags are null in {url}");
    foreach (var p in pTags)
    {
      var text = p.InnerText.Trim();
      if (text.Length > 0)
      {
        chapterText.AppendLine(text);
        chapterText.AppendLine();
      }
    }

    var eventTitle = chapterTitle ?? pTags[0].InnerText.Trim();
    ChapterScraped?.Invoke(eventTitle);
    return (chapterText.ToString(), nextChapter);
  }
}