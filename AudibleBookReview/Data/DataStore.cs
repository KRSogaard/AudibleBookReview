using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace AudibleBookReview.Data
{
    public class DataStore
    {
        [JsonIgnore]
        private String basePath;
        [JsonIgnore]
        private static string dataFileName = "data.json";

        public List<AudioBook> Books { get; set; } 
        public List<BookSeries> Series { get; set; }
        public List<string> MyBooks { get; set; }


        public DataStore(String basePath)
        {
            this.basePath = basePath;
            Load();
        }
        public DataStore()
        {

        }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(Path.Combine(basePath, dataFileName), jsonString);
        }

        public void Load()
        {
            string file = Path.Combine(basePath, dataFileName);
            if (!File.Exists(file))
            {
                Books = new List<AudioBook>();
                Series = new List<BookSeries>();
                MyBooks = new List<string>();
                Save();
                return;
            }

            string jsonString = File.ReadAllText(Path.Combine(basePath, dataFileName));
            DataStore dataStore = JsonSerializer.Deserialize<DataStore>(jsonString)!;
            this.Books = dataStore.Books ?? new List<AudioBook>();
            this.Series = dataStore.Series ?? new List<BookSeries>();
            this.MyBooks = dataStore.MyBooks ?? new List<string>();
        }
    }
}
