using AudibleBookReview.Data;
using AudibleBookReview.Utils;
using CsQuery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Parsing
{
    public class AudibleDownloader
    {
        public static AudioBook GetBook(AudioBook fetch)
        {
            AudioBook audioBook = new AudioBook()
            {
                Link = fetch.Link,
                Id = fetch.Link.Split('?')[0].Split('/').Last().ToUpper().Trim()
            };

            using (HttpClient httpClient = new HttpClient()) // WebClient class inherits IDisposable
            {
                using (HttpResponseMessage response = httpClient.GetAsync(fetch.Link).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string pageContent = content.ReadAsStringAsync().Result;
                        CQ dom = pageContent;

                        if (dom.Select("h1").First().Text().Trim().Contains("Sorry"))
                        {
                            // Book is unavalible;
                            fetch.unavailable = true;
                            return audioBook;
                        }

                        audioBook.Title = WebUtility.HtmlDecode(dom.Select("h1").First().Text().Trim());
                        audioBook.Author = WebUtility.HtmlDecode(dom.Select(".authorLabel a").First().Text().Trim());
                        audioBook.AuthorId = dom.Select(".authorLabel a").First().Attr("href").Split('?')[0].Split('/').Last();
                        audioBook.Narrators = new List<string>();
                        foreach(var a in dom.Select(".narratorLabel a"))
                        {
                            audioBook.Narrators.Add(WebUtility.HtmlDecode(a.InnerText.Trim()));
                        }
                        audioBook.Series = dom.Select(".seriesLabel a").First().Text().Trim();
                        if (dom.Select("li.seriesLabel").Length > 0)
                        {
                            audioBook.SeriesLink = dom.Select(".seriesLabel a").First().Attr("href");
                            if (audioBook.SeriesLink.StartsWith('/'))
                            {
                                audioBook.SeriesLink = "https://www.audible.com" + audioBook.SeriesLink;
                            }
                            audioBook.SeriesId = dom.Select(".seriesLabel a").First().Attr("href").Split('?')[0].Split('/').Last();
                            if (dom.Select("li.seriesLabel").First().Text().Contains(',')) {
                                audioBook.BookNumber = dom.Select("li.seriesLabel").First().Text().Split(",").Last().Trim();
                            }
                        }
                        String runtime = dom.Select("li.runtimeLabel").Text();
                        string hours = RegexHelper.Match(@"(\d+)\s+hr[s]?", runtime);
                        if (String.IsNullOrWhiteSpace(hours))
                        {
                            hours = "0";
                        }
                        string mins = RegexHelper.Match(@"(\d+)\s+min[s]?", runtime);
                        if (String.IsNullOrWhiteSpace(mins))
                        {
                            mins = "0";
                        }
                        audioBook.Length = new TimeSpan(int.Parse(hours), int.Parse(mins), 0);

                        string released = RegexHelper.Match("\"datePublished\":\\s+\"([^\"]+)", pageContent);
                        audioBook.Released = DateTime.Parse(released);

                        audioBook.Image = dom.Select(".bc-col-responsive img.bc-image-inset-border").First().Attr("src");

                        string summary = dom.Select("div.bc-spacing-s2:nth-of-type(1)").Html();
                        summary = summary.Replace("<p>", "\n");
                        summary = RegexHelper.Replace(@"\<[^>]+\>", "", summary);
                        summary = summary.Replace("&quot;", "\"");
                        audioBook.Summary = WebUtility.HtmlDecode(summary.Trim());

                        audioBook.Tags = new List<string>();
                        foreach (var t in dom.Select("div.product-topic-tags .bc-chip-text"))
                        {
                            audioBook.Tags.Add(WebUtility.HtmlDecode(t.InnerText.Trim()));
                        }
                    }
                }
            }
            audioBook.LastUpdated = DateTime.Now;
            return audioBook;
        }

        internal static BookSeries GetSeries(BookSeries bookSeries)
        {
            BookSeries series = new BookSeries()
            {
                Id = bookSeries.Id,
                Name = bookSeries.Name,
                Link = bookSeries.Link
            };

            using (HttpClient httpClient = new HttpClient()) // WebClient class inherits IDisposable
            {
                using (HttpResponseMessage response = httpClient.GetAsync(bookSeries.Link).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string pageContent = content.ReadAsStringAsync().Result;
                        CQ dom = pageContent;

                        series.Name = dom.Select("h1").Text().Trim();

                        string summary = dom.Select(".series-summary-content").Html();
                        summary = summary.Replace("<p>", "\n");
                        summary = RegexHelper.Replace(@"\<[^>]+\>", "", summary);
                        summary = summary.Replace("&quot;", "\"");
                        series.Summary = WebUtility.HtmlDecode(summary.Trim());

                        series.Items = new List<BookSeriesItem>();
                        foreach (var product in dom.Select("li.productListItem"))
                        {
                            CQ _dom = product.OuterHTML;
                            BookSeriesItem item = new BookSeriesItem();
                            var titleLink = _dom.Select("h3.bc-heading a.bc-link");
                            item.Title = titleLink.Text().Trim();
                            if (String.IsNullOrEmpty(item.Title))
                            {
                                // This is if the book is not avaliable
                                continue;
                            }
                            item.Link = titleLink.Attr("href");
                            if (item.Link.StartsWith('/'))
                            {
                                item.Link = "https://www.audible.com" + item.Link;
                            }

                            item.Id = titleLink.Attr("href").Split('?')[0].Split('/').Last().ToUpper().Trim();
                            item.Author = _dom.Select("li.authorLabel a").Text().Trim();
                            string releasedText = _dom.Select(".releaseDateLabel").Text().Split(":").Last().Trim();
                            item.Released = DateTime.ParseExact(releasedText, "mm-dd-yy", CultureInfo.InvariantCulture);
                            series.Items.Add(item);
                        }
                    }
                }
            }
            series.LastUpdated = DateTime.Now;
            return series;
        }

        public static void DownloadBookImage(AudioBook book, string path)
        {
            if (String.IsNullOrEmpty(book.Image))
            {
                return;
            }
            using (HttpClient httpClient = new HttpClient()) // WebClient class inherits IDisposable
            {
                var imageBytes = httpClient.GetByteArrayAsync(book.Image).Result;
                File.WriteAllBytesAsync(path, imageBytes).Await();
            }
        }
    }
}
