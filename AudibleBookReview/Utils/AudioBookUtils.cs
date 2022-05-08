using AudibleBookReview.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Utils
{
    public class AudioBookUtils
    {
        public static List<AudioBook> GetNewAudioBooks(List<AudioBook> importedBooks, Dictionary<string, AudioBook> knownBooks)
        {

            List<AudioBook> unknownBooks = new List<AudioBook>();
            foreach (AudioBook book in importedBooks)
            {
                if (!knownBooks.ContainsKey(book.Id)) {
                    unknownBooks.Add(book);
                } 
            }

            return unknownBooks;
        }

        public static List<BookSeries> GetSeriesToDownload(List<AudioBook> audioBooks, Dictionary<string, BookSeries> knowSeries)
        {
            Dictionary<string, bool> addedMap = new Dictionary<string, bool>();
            List<BookSeries> series = new List<BookSeries>();
            foreach (AudioBook book in audioBooks)
            {
                if (book.SeriesId == null ||
                    addedMap.ContainsKey(book.SeriesId))
                {
                    continue;
                }
                //if (!knowSeries.ContainsKey(book.SeriesId))
                // if (!knowSeries.ContainsKey(book.SeriesId) ||
                //    knowSeries[book.SeriesId].LastUpdated < DateTime.Now.Subtract(TimeSpan.FromDays(30)))
                {
                    series.Add(new BookSeries()
                    {
                        Id = book.SeriesId,
                        Name = book.Series,
                        Link = book.SeriesLink
                    });
                    addedMap.Add(book.SeriesId, true);
                }
            }

            return series;
        }
    }
}
