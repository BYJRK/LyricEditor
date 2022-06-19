using System;

namespace LyricEditor.Lyric
{
    public static class LrcHelper
    {
        /// <summary>
        /// 将时间戳字符串解析为 TimeSpan
        /// </summary>
        public static TimeSpan ParseTimeSpan(string s)
        {
            // 如果毫秒是两位的，则在结尾额外补一个 0
            if (s.Split('.')[1].Length == 2)
                s += '0';
            return TimeSpan.Parse("00:" + s);
        }

        /// <summary>
        /// 尝试将时间戳字符串解析为 TimeSpan，详见 <seealso cref="ParseTimeSpan(string)"/>
        /// </summary>
        public static bool TryParseTimeSpan(string s, out TimeSpan ts)
        {
            try
            {
                ts = ParseTimeSpan(s);
                return true;
            }
            catch
            {
                ts = TimeSpan.Zero;
                return false;
            }
        }

        /// <summary>
        /// 将时间戳变为两位毫秒的格式
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="isShort"></param>
        /// <returns></returns>
        public static string ToShortString(this TimeSpan ts, bool isShort = false)
        {
            if (isShort)
            {
                return $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            }
            else
                return $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }
    }
}
