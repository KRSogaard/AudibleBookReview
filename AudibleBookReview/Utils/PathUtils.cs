using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleBookReview.Utils
{
    public class PathUtils
    {
        private static string _basePath;
        public static string GetBasePath()
        {
            if (_basePath == null)
            {
                _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudibleTracker");
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                }
            }
            return _basePath;
        }

        private static string _imagePath;
        public static string GetImagePath()
        {
            if (_imagePath == null)
            {
                _imagePath = Path.Combine(GetBasePath(), "images");
                if (!Directory.Exists(_imagePath))
                {
                    Directory.CreateDirectory(_imagePath);
                }
            }
            return _imagePath;
        }
    }
}
