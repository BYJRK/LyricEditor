using System;

namespace LyricEditor.Lyric
{
    public static class LrcHelper
    {
        public static TimeSpan ParseTimeSpan(string s)
        {
            if (s.Split('.')[1].Length == 2)
                s += '0';
            return TimeSpan.Parse("00:" + s);
        }
        public static bool TryParseTimeSpan(string s, out TimeSpan t)
        {
            try
            {
                t = ParseTimeSpan(s);
                return true;
            }
            catch
            {
                t = TimeSpan.Zero;
                return false;
            }
        }
        public static string ToShortString(this TimeSpan ts, bool isApprox = false)
        {
            if (isApprox)
            {
                var mil = ts.Milliseconds;
                if (mil >= 100)
                    mil /= 10;
                return $"{ts.Minutes:00}:{ts.Seconds:00}.{mil:00}";
            }
            else
                return $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }
    }
}
