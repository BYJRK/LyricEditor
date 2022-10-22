using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace LyricEditor.Utils
{
    public static class ResourceHelper
    {
        public const string IconFolder = "Icons";

        public static BitmapImage GetIcon(string filename)
        {
            return new BitmapImage(new Uri(Path.Combine(IconFolder, filename), UriKind.RelativeOrAbsolute));
        }
    }
}
