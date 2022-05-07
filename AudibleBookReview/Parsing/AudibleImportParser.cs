using AudibleBookReview.Data;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudibleBookReview.Parsing
{
    public class AudibleImportParser
    {
        public static List<AudioBook> ParseBooks(string path)
        {
            List<AudioBook> books = new List<AudioBook>();
            using (var reader = new StreamReader(path))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    foreach (CSVClass record in csv.GetRecords<CSVClass>())
                    {
                        try
                        {
                            var id = record.Link.Split('?')[0].Split('/').Last();
                            books.Add(new AudioBook() { Author = record.Author, Title = record.Title, Id = id, Link = record.Link });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }
                }
            }
            return books;
        }

        public class CSVClass
        {
            [Name("title")]
            public string Title { get; set; }
            [Name("author")]
            public string Author { get; set; }
            [Name("book-link-href")]
            public string Link { get; set; }
        }
    }
}
