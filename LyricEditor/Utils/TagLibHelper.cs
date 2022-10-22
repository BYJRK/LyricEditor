using System.IO;
using System.Windows.Media.Imaging;

namespace LyricEditor.Utils
{
    public static class TagLibHelper
    {
        /// <summary>
        /// 获取音乐文件的封面图
        /// </summary>
        public static BitmapImage GetAlbumArt(string filename)
        {
            var file = TagLib.File.Create(filename);
            var bin = file.Tag.Pictures[0].Data.Data;
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(bin);
            image.EndInit();

            return image;
        }

        /// <summary>
        /// 获取音乐文件的歌曲标题
        /// </summary>
        public static string GetTitle(string filename)
        {
            var file = TagLib.File.Create(filename);
            return file.Tag.Title;
        }
    }
}
