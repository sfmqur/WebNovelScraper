using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace WebNovelScraper.Services;

public class ChapterScraper : IDisposable
{
  private const string UnicodeWatermark = "𝕗𝕣𝐞𝐞𝘄𝐞𝚋𝚗𝗼𝘃𝗲𝗹.𝚌𝕠𝚖";

  private readonly IWebDriver _driver;

  /// <summary>Fired after each chapter is successfully scraped. Arg is the chapter title.</summary>
  public event Action<string>? ChapterScraped;

  /// <summary>
  /// Headless runs without a popup browser window. 
  /// </summary>
  /// <param name="headless"></param>
  public ChapterScraper(bool headless = true)
  {
    new DriverManager().SetUpDriver(new FirefoxConfig());

    var options = new FirefoxOptions();
    if (headless)
      options.AddArgument("-headless");

    _driver = new FirefoxDriver(options);
  }
  
  /// <summary>
  /// Fetches the chapter at <paramref name="url"/> and returns its title and
  /// body paragraphs as a plain-text string.
  /// second return is the href for the next chapter. string.Empty if not found.
  /// </summary>
  public Task<(string, string)> ScrapeChapterAsync(string url)
  {
    if (!url.Contains("freewebnovel.com"))
      throw new ConstraintException($"Scraper only works for freewebnovel.com");
    
    var scrapeTask =  Task.Factory.StartNew(() => ScrapeChapter(url));
    scrapeTask.ConfigureAwait(false);
    return scrapeTask;
  }

  public (string, string nextChapter) ScrapeChapter(string url)
  {
    _driver.Navigate().GoToUrl(url);

    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
    wait.Until(d => d.FindElements(By.Id("article")).Count > 0);

    var html = _driver.PageSource;

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
    var pTagsAdded = 0;
    foreach (var p in pTags)
    {
      var text = p.InnerText.Trim().Replace(UnicodeWatermark, string.Empty);
      if (text.Length > 0)
      {
        chapterText.AppendLine(text);
        chapterText.AppendLine();
        pTagsAdded++;
      }
    }

    var eventTitle = chapterTitle ?? pTags[0].InnerText.Trim();
    ChapterScraped?.Invoke(eventTitle);
    return (chapterText.ToString(), nextChapter);
  }


  public void Dispose()
  {
    _driver.Quit();
    _driver.Dispose();
  }
}