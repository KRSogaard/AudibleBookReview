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
        public static List<AudioBook> GetNewAudioBooks(List<AudioBook> importedBooks, List<AudioBook> knownBooks)
        {
            Dictionary<string, AudioBook> knownMap = new Dictionary<string, AudioBook>();
            foreach (AudioBook book in knownBooks)
            {
                knownMap.Add(book.Id.ToLower().Trim(), book);
            }

            List<AudioBook> unknownBooks = new List<AudioBook>();
            foreach (AudioBook book in importedBooks)
            {
                if (!knownMap.ContainsKey(book.Id.ToLower().Trim())) {
                    unknownBooks.Add(book);
                } 
            }

            return unknownBooks;
        }
    }
}
