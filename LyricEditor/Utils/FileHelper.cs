using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ude;

namespace LyricEditor.Utils;

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
        ".flac",
        ".m4a",
    };

    /// <summary>
    /// 支持的歌词文件后缀
    /// </summary>
    public static HashSet<string> LyricExtensions { get; } = new HashSet<string> { ".lrc", ".txt" };

    /// <summary>
    /// 自动保存缓存文件的完整路径（位于用户本地数据目录下的 LyricEditor 文件夹中，而非程序运行目录）
    /// </summary>
    public static string TempFileName
    {
        get
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LyricEditor"
            );
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "temp.txt");
        }
    }

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
