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
        private static string personalFileName = "data-personal.json";

        public Dictionary<string, AudioBook> AllBooks { get; set; } 
        public Dictionary<string, BookSeries> Series { get; set; }
        public Dictionary<string, AudioBook> MyBooks { get; set; }
        public Dictionary<string, bool> AbandonedSeries { get; set; }


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
            SaveBookData();
            SavePersonalData();
        }
        private void SaveBookData()
        {
            DataStoreObject obj = new DataStoreObject();
            obj.Books = AllBooks.Values.ToList();
            obj.Series = Series.Values.ToList();
            obj.MyBooks = MyBooks.Keys.ToList();

            string jsonString = JsonSerializer.Serialize(obj);
            File.WriteAllText(Path.Combine(basePath, dataFileName), jsonString);
        }
        public void SavePersonalData()
        {
            DataStorePersoncalObject obj = new DataStorePersoncalObject();
            obj.abandonedSeries = AbandonedSeries.Keys.ToList();

            string jsonString = JsonSerializer.Serialize(obj);
            File.WriteAllText(Path.Combine(basePath, personalFileName), jsonString);
        }

        public void Load()
        {
            AllBooks = new Dictionary<string, AudioBook>();
            Series = new Dictionary<string, BookSeries>();
            MyBooks = new Dictionary<string, AudioBook>();
            AbandonedSeries = new Dictionary<string, bool>();

            string file = Path.Combine(basePath, dataFileName);
            if (!File.Exists(file)) {
                SaveBookData();
            } else {

                string jsonString = File.ReadAllText(Path.Combine(basePath, dataFileName));
                DataStoreObject dataStore;
                try {
                    dataStore = JsonSerializer.Deserialize<DataStoreObject>(jsonString)!;
                } catch (Exception ex) {
                    dataStore = new DataStoreObject();
                }

                if (dataStore.Books != null)
                {
                    AllBooks = dataStore.Books.ToDictionary(x => x.Id, x => x);
                }
                if (dataStore.Series != null)
                {
                    Series = dataStore.Series.ToDictionary(x => x.Id, x => x);
                }
                if (dataStore.MyBooks != null)
                {
                    MyBooks = dataStore.MyBooks.ToDictionary(x => x, x => AllBooks[x]);
                }
            }  

            file = Path.Combine(basePath, personalFileName);
            if (!File.Exists(file))
            {
                SavePersonalData();
            } else {
                string jsonString = File.ReadAllText(Path.Combine(basePath, personalFileName));
                DataStorePersoncalObject personalDataStore;
                try
                {
                    personalDataStore = JsonSerializer.Deserialize<DataStorePersoncalObject>(jsonString)!;
                }
                catch (Exception ex)
                {
                    personalDataStore = new DataStorePersoncalObject();
                }

                if (personalDataStore.abandonedSeries != null)
                {
                    AbandonedSeries = personalDataStore.abandonedSeries.ToDictionary(x => x, x => true);
                }
            }
        }

        internal void ToogleAbandond(string id)
        {
            if (AbandonedSeries.ContainsKey(id))
            {
                AbandonedSeries.Remove(id);
            } else
            {
                AbandonedSeries.Add(id, true);
            }
            SavePersonalData();
        }

        public void Add(AudioBook audioBook)
        {
            if (AllBooks.ContainsKey(audioBook.Id))
            {
                AllBooks[audioBook.Id] = audioBook;
            }
            else
            {
                AllBooks.Add(audioBook.Id, audioBook);
            }
        }
        public void Add(BookSeries series)
        {
            if (Series.ContainsKey(series.Id)) {
                Series[series.Id] = series;
            } else {
                Series.Add(series.Id, series);
            }
        }
        public void AddMyBook(AudioBook audioBook)
        {
            if (!AllBooks.ContainsKey(audioBook.Id))
            {
                Add(audioBook);
            }
            if (MyBooks.ContainsKey(audioBook.Id))
            {
                MyBooks[audioBook.Id] = audioBook;
            }
            else
            {
                MyBooks.Add(audioBook.Id, audioBook);
            }
        }

        public class DataStoreObject
        {
            public List<AudioBook> Books { get; set; }
            public List<BookSeries> Series { get; set; }
            public List<string> MyBooks { get; set; }
        }

        public class DataStorePersoncalObject
        {
            public List<string> abandonedSeries { get; set; }
        }
    }
}
