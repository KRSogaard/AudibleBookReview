using AudibleBookReview.Data;
using AudibleBookReview.Utils;
using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Parsing
{
    public class AudibleDownloader
    {
        public static AudioBook GetBook(string link)
        {
            AudioBook audioBook = new AudioBook()
            {
                Link = link,
                Id = link.Split('?')[0].Split('/').Last()
            };

            using (HttpClient httpClient = new HttpClient()) // WebClient class inherits IDisposable
            {
                using (HttpResponseMessage response = httpClient.GetAsync(link).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string pageContent = content.ReadAsStringAsync().Result;
                        CQ dom = pageContent;

                        audioBook.Title = dom.Select("h1").First().Text().Trim();
                        audioBook.Author = dom.Select(".authorLabel a").First().Text().Trim();
                        audioBook.AuthorId = dom.Select(".authorLabel a").First().Attr("href").Split('?')[0].Split('/').Last();
                        audioBook.Narrators = new List<string>();
                        foreach(var a in dom.Select(".narratorLabel a"))
                        {
                            audioBook.Narrators.Add(a.InnerText.Trim());
                        }
                        audioBook.Series = dom.Select(".seriesLabel a").First().Text().Trim();
                        if (dom.Select("li.seriesLabel").Length > 0)
                        {
                            audioBook.SeriesId = dom.Select(".seriesLabel a").First().Attr("href").Split('?')[0].Split('/').Last();
                            audioBook.BookNumber = dom.Select("li.seriesLabel").First().Text().Split(",").Last().Trim();
                        }
                        String runtime = dom.Select("li.runtimeLabel").Text();
                        string hours = RegexHelper.Match(@"(\d+)\s+hrs", runtime);
                        string mins = RegexHelper.Match(@"(\d+)\s+mins", runtime);
                        audioBook.Length = new TimeSpan(int.Parse(hours), int.Parse(mins), 0);
                    }
                }
            }
            return audioBook;
        }
    }
}
