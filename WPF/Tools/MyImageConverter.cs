using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF.Tools
{

    public class MyImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Bitmap)
            {
                using (var stream = new MemoryStream())
                {
                    ((Bitmap)value).Save(stream, ImageFormat.Png);

                    var bitmap = CreateBitMapImageFromStream(stream);

                    return bitmap;
                }
            }
            else if (value is Icon icon)
            {
                using (var stream = new MemoryStream())
                {
                    icon.Save(stream);

                    var bitmap = CreateBitMapImageFromStream(stream);

                    return bitmap;
                }
            }
            return value;
        }
        private BitmapImage CreateBitMapImageFromStream(Stream stream)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            return bitmap;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
