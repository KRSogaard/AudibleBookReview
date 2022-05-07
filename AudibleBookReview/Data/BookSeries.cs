using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Data
{
    public class BookSeries
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public List<BookSeriesItem> Items { get; set; }

    }
}
