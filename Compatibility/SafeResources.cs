using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows.Media.Imaging;

namespace EPCommon;

public static class SafeResources
{
    public static object GetObject(ResourceManager manager, string name, CultureInfo culture = null)
    {
        object value = manager.GetObject(name, culture);
        if (!(value is byte[] bytes)) return value;
        using (var stream = new MemoryStream(bytes))
        {
            if (bytes.Length > 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 1 && bytes[3] == 0)
                using (var icon = new Icon(stream)) return icon.Clone();
            using (var bitmap = new Bitmap(stream)) return new Bitmap(bitmap);
        }
    }
    public static BitmapSource ToImageSource(Image image)
    {
        if (image == null) return null;
        using (var stream = new MemoryStream())
        {
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            var result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }
    }
}
