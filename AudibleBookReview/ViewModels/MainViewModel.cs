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
using System.Threading;

namespace AudibleBookReview.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private string basePath;
        private DataStore dataStore;
        public ObservableCollection<SeriesViewModel> Series { get; set; }
        public ObservableCollection<SeriesViewModel> AbandondSeries { get; set; }
        public ObservableCollection<SeriesViewModel> CompleteSeries { get; set; }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                SetProperty(ref _statusMessage, value);
            }
        }

        private string _currentProcess;
        public string CurrentProcess
        {
            get { return _currentProcess; }
            set
            {
                SetProperty(ref _currentProcess, value);
            }
        }

        private Visibility _isProcessing;
        public Visibility IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                SetProperty(ref _isProcessing, value);
            }
        }

        private int _mainProcessCurrent;
        public int MainProcessCurrent
        {
            get { return _mainProcessCurrent; }
            set
            {
                SetProperty(ref _mainProcessCurrent, value);
            }
        }

        private int _mainProcessMax;
        public int MainProcessMax
        {
            get { return _mainProcessMax; }
            set
            {
                SetProperty(ref _mainProcessMax, value);
            }
        }

        private int _subProcessCurrent;
        public int SubProcessCurrent
        {
            get { return _subProcessCurrent; }
            set
            {
                SetProperty(ref _subProcessCurrent, value);
            }
        }

        private int _subProcessMax;
        public int SubProcessMax
        {
            get { return _subProcessMax; }
            set
            {
                SetProperty(ref _subProcessMax, value);
            }
        }

        private string _orderBy;
        public string OrderBy
        {
            get { return _orderBy; }
            set
            {
                SetProperty(ref _orderBy, value);
            }
        }


        public MainViewModel()
        {
            Series = new ObservableCollection<SeriesViewModel>();
            AbandondSeries = new ObservableCollection<SeriesViewModel>();
            CompleteSeries = new ObservableCollection<SeriesViewModel>();
            OrderBy = "latest-release";
            dataStore = new DataStore(PathUtils.GetBasePath());
            IsProcessing = Visibility.Collapsed;
            RebuildData();

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
                        IsProcessing = Visibility.Visible;
                        CurrentProcess = "Importing Audible Books";
                        StatusMessage = "Starting Audible book import";

                        MainProcessMax = 5;
                        MainProcessCurrent = 0;

                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            string importFileName = openFileDialog.FileName;

                            if (!File.Exists(importFileName))
                            {
                                return;
                            }

                            Task.Factory.StartNew(() =>
                            {
                                List<AudioBook> books = AudibleImportParser.ParseBooks(importFileName);
                                List<AudioBook> unknownBooks = AudioBookUtils.GetNewAudioBooks(books, dataStore.AllBooks);

                                int count = 0;
                                MainProcessCurrent++;
                                SubProcessMax = unknownBooks.Count;
                                SubProcessCurrent = 0;
                                foreach (AudioBook book in unknownBooks)
                                {
                                    count++;
                                    StatusMessage = "Downloading Book (" + count + "/" + unknownBooks.Count + ") " + book.Title + " by " + book.Author;
                                    dataStore.AddMyBook(AudibleDownloader.GetBook(book));
                                    if (count % 20 == 0)
                                    {
                                        dataStore.Save();
                                    }
                                    Thread.Sleep(1000);
                                }
                                dataStore.Save();

                                
                                RefreshSeries();
                                DownloadMissingImages();

                                StatusMessage = "Import Done";
                                IsProcessing = Visibility.Collapsed;
                            });
                        }
                    });
                }
                return _import;
            }
        }

        internal void ToogleAbandond(SeriesViewModel seriesViewModel)
        {
            if (dataStore.AbandonedSeries.ContainsKey(seriesViewModel.GetId()))
            {
                if (seriesViewModel.IsComplete()) {
                    CompleteSeries.Remove(seriesViewModel);
                } else {
                    Series.Remove(seriesViewModel);
                }
                AbandondSeries.Add(seriesViewModel);
                OrderList(AbandondSeries);
            }
            else
            {
                AbandondSeries.Remove(seriesViewModel);
                if (seriesViewModel.IsComplete())
                {
                    CompleteSeries.Add(seriesViewModel);
                    OrderList(Series);
                }
                else
                {
                    Series.Add(seriesViewModel);
                    OrderList(Series);
                }
            }
        }

        private void OrderList(ObservableCollection<SeriesViewModel> current)
        {
            var list = current.ToList().OrderByDescending(x => x.LastRelease);
            current.Clear();
            current.AddRange(list);
        }

        private ICommand _refresh;
        public ICommand Refresh
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new DelegateCommand(() =>
                    {
                        IsProcessing = Visibility.Visible;
                        CurrentProcess = "Refreshing Series";
                        StatusMessage = "Refreshing Series";

                        MainProcessMax = 4;
                        MainProcessCurrent = 0;

                        Task.Factory.StartNew(() =>
                        {
                            RefreshSeries();
                            DownloadMissingImages();
                            IsProcessing = Visibility.Collapsed;
                        });
                    });
                }
                return _refresh;
            }
        }

        private void RefreshSeries()
        {
            List<BookSeries> bookSeriesToDownload = AudioBookUtils.GetSeriesToDownload(dataStore.MyBooks.Values.ToList(), dataStore.Series);
            int count = 0;
            MainProcessCurrent++;
            SubProcessMax = bookSeriesToDownload.Count;
            SubProcessCurrent = 0;
            foreach (BookSeries bookSeries in bookSeriesToDownload)
            {
                count++;
                SubProcessCurrent = count;
                StatusMessage = "Downloading Series (" + count + "/" + bookSeriesToDownload.Count + ") " + bookSeries.Name;
                dataStore.Add(AudibleDownloader.GetSeries(bookSeries));
                if (count % 20 == 0)
                {
                    dataStore.Save();
                }
                Thread.Sleep(1000);
            }
            dataStore.Save();

            StatusMessage = "Finding unknown books in Series";
            List<AudioBook> unknownBooks = new List<AudioBook>();
            foreach (BookSeries bookSeries in dataStore.Series.Values)
            {
                foreach (BookSeriesItem item in bookSeries.Items)
                {
                    if (!dataStore.AllBooks.ContainsKey(item.Id))
                    {
                        unknownBooks.Add(new AudioBook()
                        {
                            Id = item.Id,
                            Link = item.Link,
                            Author = item.Author,
                            Title = item.Title
                        });
                    }
                }
            }
            count = 0;
            MainProcessCurrent++;
            SubProcessMax = unknownBooks.Count;
            SubProcessCurrent = 0;
            foreach (AudioBook book in unknownBooks)
            {
                count++;
                SubProcessCurrent = count;
                StatusMessage = "Downloading Book (" + count + "/" + unknownBooks.Count + ") " + book.Title + " by " + book.Author;
                dataStore.Add(AudibleDownloader.GetBook(book));
                if (count % 20 == 0)
                {
                    dataStore.Save();
                }
                Thread.Sleep(1000);
            }
            dataStore.Save();
        }

        private void DownloadMissingImages()
        {
            StatusMessage = "Finding missing book images";
            List<Tuple<AudioBook, string>> missingImages = new List<Tuple<AudioBook, string>>();
            foreach (AudioBook book in dataStore.AllBooks.Values)
            {
                string bookImagePath = Path.Combine(PathUtils.GetImagePath(), book.Id + ".jpg");
                if (!File.Exists(bookImagePath))
                {
                    missingImages.Add(new Tuple<AudioBook, string>(book, bookImagePath));
                }
            }
            int count = 0;
            MainProcessCurrent++;
            SubProcessMax = missingImages.Count;
            SubProcessCurrent = 0;
            foreach (Tuple<AudioBook, string> book in missingImages)
            {
                count++;
                StatusMessage = "Downloading Image (" + count + " of " + missingImages.Count + ") for " + book.Item1.Title + " by " + book.Item1.Author;
                SubProcessCurrent++;

                AudibleDownloader.DownloadBookImage(book.Item1, book.Item2);
                Thread.Sleep(1000);
            }
            StatusMessage = "Done downloading images";
        }

        private void RebuildData()
        {
            Series.Clear();
            List<SeriesViewModel> activeSeries = new List<SeriesViewModel>();
            List<SeriesViewModel> abandondSeries = new List<SeriesViewModel>();
            List<SeriesViewModel> completeSeries = new List<SeriesViewModel>();
            foreach (BookSeries series in dataStore.Series.Values)
            {
                SeriesViewModel seriesViewModel = SeriesViewModel.Create(series, dataStore, this);
                if (dataStore.AbandonedSeries.ContainsKey(series.Id))
                {
                    abandondSeries.Add(seriesViewModel);
                } else {
                    if (seriesViewModel.OwnedBooksCount == seriesViewModel.BookCount)
                    {
                        completeSeries.Add(seriesViewModel);
                    }
                    else
                    {
                        activeSeries.Add(seriesViewModel);
                    }
                }
            }
            
            Series.AddRange(activeSeries.OrderByDescending(x => x.LastRelease));
            AbandondSeries.AddRange(abandondSeries.OrderByDescending(x => x.LastRelease));
            CompleteSeries.AddRange(completeSeries.OrderByDescending(x => x.LastRelease));
        }
    }
}
