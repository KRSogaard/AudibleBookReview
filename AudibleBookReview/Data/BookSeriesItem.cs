using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Data
{
    public class BookSeriesItem
    {
        public String Id { get; set; }
        public String Title { get; set; }
        public String Link { get; set; }
        public String Author { get; set; }
        public DateTime Released { get; set; }
    }
}
