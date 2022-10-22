using System.Collections.Generic;
using System.IO;
using System.Text;
using Ude;

namespace LyricEditor.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// 支持的音乐文件格式
        /// </summary>
        public static HashSet<string> MediaExtensions { get; } = new HashSet<string>
        {
            ".mp3",
            ".wav",
            ".3gp",
            ".mp4",
            ".avi",
            ".wmv",
            ".wma",
            ".aac",
            ".flac"
        };

        /// <summary>
        /// 支持的歌词文件后缀
        /// </summary>
        public static HashSet<string> LyricExtensions { get; } = new HashSet<string> { ".lrc", ".txt" };

        public const string TempFileName = "temp.txt";

        /// <summary>
        /// 判断读入文本的编码格式
        /// </summary>
        public static Encoding GetEncoding(string filename)
        {
            var bytes = File.ReadAllBytes(filename);
            var cdet = new CharsetDetector();
            cdet.Feed(bytes, 0, bytes.Length);
            cdet.DataEnd();
            var encoding = cdet.Charset;
            return Encoding.GetEncoding(encoding);
        }
    }
}
