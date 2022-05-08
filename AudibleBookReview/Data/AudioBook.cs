using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Data
{
    public class AudioBook
    {
        public bool unavailable { get; set; }
        public DateTime LastUpdated { get; set; }

        public String Id { get; set; }
        public String Link { get; set; }
        public String Title { get; set; }
        public String Author { get; set; }
        public String AuthorId { get; set; }
        public List<String> Narrators { get; set; }
        public String Series { get; set; }
        public String SeriesId { get; set; }
        public String SeriesLink { get; set; }
        public String BookNumber { get; set; }
        public TimeSpan Length { get; set; }
        public DateTime Released { get; set; }
        public String Image { get; set; }
        public String Summary { get; set; }
        public List<String> Tags { get; set; }

        public override string ToString()
        {
            return Title + " by " + Author;

        }
    }
}
