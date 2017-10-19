using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricEditor.Lyric
{
    public static class LrcHelper
    {
        public static TimeSpan ParseTimeSpan(string s)
        {
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
        public static string ToShortString(this TimeSpan ts)
        {
            return $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }
        public static string ToString(this TimeSpan ts)
        {
            return ToShortString(ts);
        }
    }
}
