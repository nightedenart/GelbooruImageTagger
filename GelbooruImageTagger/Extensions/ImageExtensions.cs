using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GelbooruImageTagger.Extensions
{
    internal static class ImageExtensions
    {
        public static ImageSource? BitmapToImageSource(this Image? image)
        {
            if (image == null)
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
                return bitmap;
            }
            catch {
                return null;
            }
        }

    }
}
