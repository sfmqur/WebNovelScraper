# WebNovelScraper

A small  desktop app for scraping web novels from freewebnovel.com into plain text files.

## Usage

Paste a chapter URL, set how many chapters to scrape and how many to group per file, then hit Scrape. Output files are written to `Documents/WebNovelScraper/` and named after the book and starting chapter, e.g. `the-first-legendary-beast-master_chapter-0001.txt`.

## Stack

- .NET 8, Avalonia UI, CommunityToolkit.Mvvm
- HtmlAgilityPack for parsing
