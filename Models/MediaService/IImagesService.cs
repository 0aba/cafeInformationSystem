using System;
using System.Text;
using Avalonia.Media.Imaging;

namespace cafeInformationSystem.Models.MediaService
{
    public abstract class IImagesService
    {
        private const string _CHARS_NAME_IMAGE = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        private const int _LENGTH_NAME_IMAGE = 64;

        public abstract void SaveImage(string mediaPathToDir, string fullFileName, Bitmap image);

        public abstract Bitmap GetImage(string mediaPathToImage);

        public abstract void DeleteImage(string mediaPathToImage);

        public string GeneratorNameLen64Image()
        {
            var random = new Random();
            var stringBuilder = new StringBuilder(_LENGTH_NAME_IMAGE);
            
            for (int i = 0; i < _LENGTH_NAME_IMAGE; i++)
            {
                stringBuilder.Append(_CHARS_NAME_IMAGE[random.Next(_CHARS_NAME_IMAGE.Length)]);
            }
            
            return stringBuilder.ToString();
        }

        public string GetPathDirectoryBasedOnCurrentDate()
        {
            DateTime currentDate = DateTime.Now;
            return $"{currentDate.Year:0000}/{currentDate.Month:00}/{currentDate.Day:00}";
        }
    }
}
