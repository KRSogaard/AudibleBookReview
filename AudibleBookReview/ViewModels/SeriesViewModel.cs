using AudibleBookReview.Data;
using AudibleBookReview.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AudibleBookReview.ViewModels
{
    public class SeriesViewModel : BindableBase
    {
        private DataStore dataStore;
        private BookSeries series;
        private MainViewModel mainViewModel;

        public string Title { get; set; }
        public DateTime LastRelease {get; set;}
        public string LastReleaseText { get; set; }
        public int BookCount { get; set; }
        public int OwnedBooksCount { get; set; }
        public BitmapImage BookCoverImage { get; set; }

        public ObservableCollection<SeriesBookViewModel> Books { get; set; }

        public bool _isAbandond;
        public bool IsAbandond
        {
            get { return _isAbandond; }
            set { SetProperty(ref _isAbandond, value);
                RaisePropertyChanged(nameof(ShowAbandondButton));
                RaisePropertyChanged(nameof(ShowUnAbandondButton));
            }
        }
        public Visibility ShowAbandondButton
        {
            get
            {
                return IsAbandond ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public Visibility ShowUnAbandondButton
        {
            get
            {
                return IsAbandond ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private ICommand _toogleAbandond;
        public ICommand ToogleAbandond
        {
            get
            {
                if (_toogleAbandond == null)
                {
                    _toogleAbandond = new DelegateCommand(() => {
                        dataStore.ToogleAbandond(series.Id);
                        mainViewModel.ToogleAbandond(this);
                    });
                }
                return _toogleAbandond;
            }
        }

        public string GetId()
        {
            return series.Id;
        }
        public bool IsComplete()
        {
            return BookCount == OwnedBooksCount;
        }

        public SeriesViewModel(BookSeries series, DataStore dataStore, MainViewModel mainViewModel)
        {
            this.dataStore = dataStore;
            this.series = series;
            this.mainViewModel = mainViewModel;
        }

        public static SeriesViewModel Create(BookSeries series, DataStore dataStore, MainViewModel mainViewModel)
        {
            SeriesViewModel viewModel = new SeriesViewModel(series, dataStore, mainViewModel);
            viewModel.Title = series.Name;
            viewModel.BookCount = series.Items.Count;
            viewModel.OwnedBooksCount = series.Items.Where(x => dataStore.MyBooks.ContainsKey(x.Id)).Count(); 
            viewModel.Books = new ObservableCollection<SeriesBookViewModel>(series.Items
                .Where(x => dataStore.AllBooks.ContainsKey(x.Id))
                .Select(x => dataStore.AllBooks[x.Id])
                .OrderByDescending(x => x.Released)
                .Select(x => SeriesBookViewModel.Create(x, dataStore, mainViewModel))
                .ToList());

            AudioBook latest = viewModel.Books[0].AudioBook;

            viewModel.LastRelease = latest.Released;
            viewModel.LastReleaseText = latest.Released.ToShortDateString();
            viewModel.IsAbandond = dataStore.AbandonedSeries.ContainsKey(series.Id);

            string imagePath = Path.Combine(PathUtils.GetImagePath(), latest.Id + ".jpg");
            viewModel.BookCoverImage = new BitmapImage(new Uri(imagePath, UriKind.Absolute));

            return viewModel;
        }
    }
}
