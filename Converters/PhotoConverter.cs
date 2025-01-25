using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OldGoodAvitoApplication.Converters
{
    public class PhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string photoPath = value as string;
            if (string.IsNullOrEmpty(photoPath))
            {
                // Возвращаем default.png, если путь не задан
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default.png"));
            }
            else
            {
                try
                {
                    // Предполагается, что photoPath - относительный путь к изображению
                    return new BitmapImage(new Uri($"pack://application:,,,/{photoPath}"));
                }
                catch
                {
                    // В случае ошибки возвращаем default.png
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/default.png"));
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
