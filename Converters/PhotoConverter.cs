using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace OldGoodAvitoApplication.Converters
{
    public class PhotoConverter : IValueConverter
    {
        // Базовая директория для относительных путей
        private readonly string baseDirectory = @"C:\Images\Ads\"; // Измените на вашу директорию

        // Путь к изображению по умолчанию
        private readonly string defaultImagePath = @"C:\Images\default.png"; // Измените на ваш путь

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string photoPath = value as string;
            if (string.IsNullOrEmpty(photoPath))
            {
                // Возвращаем default.png, если путь не задан
                return LoadImage(defaultImagePath);
            }
            else
            {
                try
                {
                    string fullPath;

                    if (Path.IsPathRooted(photoPath))
                    {
                        // Если путь абсолютный, используем его напрямую
                        fullPath = photoPath;
                    }
                    else
                    {
                        // Если путь относительный, комбинируем с базовой директорией
                        fullPath = Path.Combine(baseDirectory, photoPath);
                    }

                    if (File.Exists(fullPath))
                    {
                        return LoadImage(fullPath);
                    }
                    else
                    {
                        // Если файл не найден, возвращаем default.png
                        return LoadImage(defaultImagePath);
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку и возвращаем default.png
                    System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
                    return LoadImage(defaultImagePath);
                }
            }
        }

        private BitmapImage LoadImage(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Чтобы файл мог быть закрыт после загрузки
                bitmap.EndInit();
                bitmap.Freeze(); // Делает изображение доступным для использования в разных потоках
                return bitmap;
            }
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем пустое изображение или можно повторно вернуть default.png
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке изображения из пути {path}: {ex.Message}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
