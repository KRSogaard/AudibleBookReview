using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudibleBookReview.Data;
using System.IO;
using Prism.Commands;
using System.Windows.Input;
using System.Windows;
using AudibleBookReview.Parsing;
using AudibleBookReview.Utils;
using Microsoft.Win32;

namespace AudibleBookReview.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        private DataStore dataStore;
        //public ObservableCollection<BookItemViewModel> Items { get; set; }
        

        public MainViewModel()
        {
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudibleTracker");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            dataStore = new DataStore(basePath);
        }

        private ICommand _import;
        public ICommand Import
        {
            get
            {
                if (_import == null)
                {
                    _import = new DelegateCommand(() =>
                    {

                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            string importFileName = openFileDialog.FileName;

                            if (!File.Exists(importFileName))
                            {
                                return;
                            }

                            List<AudioBook> books = AudibleImportParser.ParseBooks(importFileName);
                            List<AudioBook> unknownBooks = AudioBookUtils.GetNewAudioBooks(books, dataStore.Books);

                            foreach(AudioBook book in unknownBooks)
                            {
                                dataStore.Books.Add(AudibleDownloader.GetBook(book.Link));
                                dataStore.MyBooks.Add(book.Id.ToLower().Trim());

                            }
                        }

                    });
                }
                return _import;
            }
        }
    }
}
