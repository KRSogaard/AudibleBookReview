using AudibleBookReview.Data;
using AudibleBookReview.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AudibleBookReview.ViewModels
{
    public class SeriesBookViewModel : BindableBase
    {
        public bool Owned { get; set; }
        public Visibility ShowOwnedIcon
        {
            get { return Owned ? Visibility.Visible : Visibility.Collapsed; }
        }
        public Visibility ShowMissingIcon
        {
            get { return Owned ? Visibility.Collapsed : Visibility.Visible; }
        }
        public AudioBook AudioBook { get; set; }
        public string Title { get; set; }
        public BitmapImage BookCoverImage { get; set; }

        public string ReleasedText { 
            get {
                return AudioBook.Released.ToShortDateString();
            }
        }

        public string BookNumber
        {
            get
            {
                return AudioBook.BookNumber;
            }
        }

        public string Author
        {
            get
            {
                return AudioBook.Author;
            }
        }

        public string LengthText
        {
            get
            {
                string length = "";
                int hours = AudioBook.Length.Hours;
                if (AudioBook.Length.Days > 0)
                {
                    hours += AudioBook.Length.Days * 24;
                }
                if (hours > 0)
                {
                    length += hours + " hours, ";
                }
                length += AudioBook.Length.Minutes + " minutes";
                return length;
            }
        }
 
        private ICommand _navigateToBook;
        public ICommand NavigateToBook
        {
            get
            {
                if (_navigateToBook == null)
                {
                    _navigateToBook = new DelegateCommand(() =>
                    {
                        var sInfo = new System.Diagnostics.ProcessStartInfo(AudioBook.Link)
                        {
                            UseShellExecute = true,
                        };
                        System.Diagnostics.Process.Start(sInfo);
                    });
                }
                return _navigateToBook;
            }
        }

        public static SeriesBookViewModel Create(AudioBook book, DataStore dataStore, MainViewModel mainViewModel)
        {
            SeriesBookViewModel viewModel = new SeriesBookViewModel();
            viewModel.AudioBook = book;
            viewModel.Title = book.Title; 
            
            string imagePath = Path.Combine(PathUtils.GetImagePath(), book.Id + ".jpg");
            viewModel.BookCoverImage = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            viewModel.Owned = dataStore.MyBooks.ContainsKey(book.Id);

            return viewModel;
        }
    }
}
